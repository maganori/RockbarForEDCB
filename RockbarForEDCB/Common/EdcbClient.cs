using EpgTimer;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RockbarForEDCB
{
    /// <summary>
    /// EpgTimerSrvとの接続を一元管理するクラス
    /// ・接続設定（パイプ/TCP、ホスト名、ポート、死活監視の間隔）を保持する
    /// ・死活監視をして、結果を StatusChecked で知らせる
    /// ・コマンドの実行（ExecuteCmd）と、パイプ方式の通知登録・解除（ExecutePipeRegistCmd）を行う
    /// 　(TCP方式はロングポーリングのため、通知登録・解除はしない)
    /// </summary>
    public class EdcbClient : IDisposable
    {
        // 通信1回あたりのタイムアウト時間（ミリ秒）
        private const int CmdTimeoutMs = 5000;

        // パイプ登録・解除のタイムアウト時間（ミリ秒）
        private const int PipeRegistTimeoutMs = 3000; // EpgTimerと同じ3秒

        // 死活監視（TCP）の通信タイムアウト（ミリ秒）
        private const int TcpWatchConnectTimeoutMs = 5000;

        // 死活監視（パイプ）の間隔（秒）
        private const int PipeWatchIntervalSec = 1;

        /// <summary>
        /// 接続設定の1回分のスナップショット
        /// 設定が変わるたびに新しいインスタンスを作り、参照だけを差し替える
        /// </summary>
        private sealed class ConnectionSettings
        {
            public ConnectionSettings(int version, bool useTcpIp, string hostName, uint portNumber, int tcpWatchIntervalSec)
            {
                Version = version;
                UseTcpIp = useTcpIp;

                // 接続先のホスト名（またはIPアドレスの文字列）
                // 前後の空白は取り除いて保持するため
                HostName = hostName?.Trim() ?? string.Empty;

                PortNumber = portNumber;

                // 死活監視の間隔（秒）
                // ・パイプ：設定値によらず、必ず PipeWatchIntervalSec ごと
                // ・TCP：設定値をそのまま使う（0以下は「死活監視なし」）
                if (useTcpIp)
                {
                    WatchIntervalSec = Math.Max(tcpWatchIntervalSec, 0);
                }
                else
                {
                    WatchIntervalSec = PipeWatchIntervalSec;
                }
            }

            /// <summary>設定の世代番号</summary>
            public int Version { get; }
            public bool UseTcpIp { get; }

            /// <summary>接続先のホスト名、またはIPアドレスの文字列（空の場合もある）</summary>
            public string HostName { get; }

            public uint PortNumber { get; }

            /// <summary>実際に使われる死活監視の間隔（秒）。0なら死活監視なし</summary>
            public int WatchIntervalSec { get; }
        }

        // 現在の接続設定（UIスレッドで書き、他のスレッドから読む）
        private volatile ConnectionSettings _settings = new ConnectionSettings(0, false, string.Empty, 0, 0);

        // 設定の世代番号の採番用（_stateLock の中でだけ増やす）
        private int _settingsVersionCounter;

        // 接続設定（_settings）と通信可能フラグ（_isAvailable）の更新を守るロック
        // 世代の確認と更新の間に別のスレッドが割り込まないようにするため
        private readonly object _stateLock = new object();

        // EpgTimerSrvと通信可能な状態か
        private volatile bool _isAvailable;

        // 死活監視の二重実行を防ぐフラグ（0:待機中 1:確認中）
        private int _watchBusy;

        // 死活監視のタイマー（間隔は設定で変わるため、Changeで変更する）
        private readonly System.Threading.Timer _watchTimer;

        /// <summary>
        /// 通信可能状態（IsAvailable）が変わったときに発生する
        /// 発生元は別スレッドのことがあるため、受け取る側でUIスレッドに戻すこと
        /// </summary>
        public event EventHandler AvailabilityChanged;

        /// <summary>AvailabilityChanged を発生させる</summary>
        private void RaiseAvailabilityChanged()
        {
            AvailabilityChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 死活監視の結果を通知するイベント
        /// </summary>
        public event EventHandler<ServerStatusEventArgs> StatusChecked;

        /// <summary>EpgTimerSrv と通信可能な状態かを取得</summary>
        public bool IsAvailable => _isAvailable;

        /// <summary>TCP/IP接続かどうか（falseならパイプ接続）</summary>
        public bool UseTcpIp => _settings.UseTcpIp;

        /// <summary>接続先のポート番号</summary>
        public uint PortNumber => _settings.PortNumber;

        /// <summary>実際に適用されている死活監視の間隔（秒）。0なら死活監視なし</summary>
        public int WatchIntervalSec => _settings.WatchIntervalSec;

        /// <summary>
        /// 接続先のIPアドレスを、呼び出しのたびに解決する
        /// パイプ接続のとき、または変換できなかったときは null
        /// 解決は別スレッドで行い、タイムアウトを超えたら null を返す（UIスレッドを長く止めないため）
        /// </summary>
        public IPAddress ResolveServerAddress()
        {
            // パイプ接続では、接続先のIPアドレスは使わないため null
            ConnectionSettings settings = _settings;
            if (!settings.UseTcpIp) return null;

            Task<IPAddress> lookupTask = Task.Run(() => ResolveHostName(settings.HostName));

            // 時間内に終わらなければ変換失敗（null）とする
            return lookupTask.Wait(CmdTimeoutMs) ? lookupTask.Result : null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EdcbClient()
        {
            // 設定が反映されるまで死活監視は動かさない（ApplySettingsAndCheckAsyncで間隔をセットする）
            _watchTimer = new System.Threading.Timer(CheckServerAlive, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 死活監視（タイマーのスレッドから呼ばれる）
        /// TCPは巡回カウンタを含む状態を問い合わせ、パイプは接続待ちイベントの有無だけを見る
        /// </summary>
        private void CheckServerAlive(object state)
        {
            // 前回の確認がまだ終わっていなければ何もしない
            if (Interlocked.Exchange(ref _watchBusy, 1) == 1) return;

            try
            {
                ConnectionSettings settings = _settings;

                bool isAvailable;
                NotifySrvInfo status = null;

                if (settings.UseTcpIp)
                {
                    // 問い合わせの結果を受ける入れ物
                    // タイムアウトで諦めた通信が、あとから書き込む可能性があるため、
                    // 成功を確認できたときだけ status に渡す（status とは別の変数にしている）
                    var receivedInfo = new NotifySrvInfo();

                    // 名前解決と通信をまとめて、タイムアウト付きの別スレッドで行う
                    // （名前解決が遅くても、死活監視のスレッドを止めないため）
                    ErrCode err = RunOnServer(settings,
                        c => c.SendGetNotifySrvStatus(ref receivedInfo), TcpWatchConnectTimeoutMs);

                    isAvailable = (err == ErrCode.CMD_SUCCESS);
                    if (isAvailable) status = receivedInfo;
                }
                else
                {
                    // パイプはサーバーが生きていれば接続待ちイベントが存在する（通信はしない）
                    isAvailable = CreateCtrlCmdUtil(settings).PipeExists();
                }

                // 確認中に設定が変わっていた場合は、この結果は捨てる（世代の確認と反映は同時に行う）
                if (!TrySetAvailable(settings.Version, isAvailable)) return;

                // 死活監視の結果を購読者に通知する
                StatusChecked?.Invoke(this, new ServerStatusEventArgs(isAvailable, status));
            }
            catch (Exception ex)
            {
                // タイマーのスレッドで例外を出すとアプリが落ちるため、必ず受け止める
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                Interlocked.Exchange(ref _watchBusy, 0);
            }
        }

        /// <summary>
        /// 接続設定を反映し、EpgTimerSrvと通信できるかを確認
        /// </summary>
        /// <param name="watchIntervalSec">死活監視の間隔（秒）。0は死活監視なし。</param>
        /// <returns>
        /// 確認の結果（CMD_SUCCESSなら成功）
        /// 待機中に新しい設定が適用され、この結果が古くなった場合は null を返す
        /// </returns>
        public async Task<ErrCode?> ApplySettingsAndCheckAsync(
            bool useTcpIp, string hostName, uint portNumber, int watchIntervalSec)
        {
            // 世代番号の採番、接続設定の保存、通信可能フラグを下げる処理は、まとめてロックの中で行う
            // （採番と保存の間に別の呼び出しが割り込むと、古い設定が最新として残ってしまうため）
            ConnectionSettings settings;
            bool wasAvailable;
            lock (_stateLock)
            {
                // 設定世代をインクリメントし、今回の世代番号として使う
                _settingsVersionCounter++;

                // 接続設定のスナップショットを作成し、最新の接続設定として保存
                settings = new ConnectionSettings(_settingsVersionCounter, useTcpIp, hostName, portNumber, watchIntervalSec);
                _settings = settings;

                // 接続確認を開始するため、いったん通信可能フラグを下げる
                wasAvailable = _isAvailable;
                _isAvailable = false;
            }

            // 状態が変わったときだけ、ロックの外でイベントを発生させる
            if (wasAvailable) RaiseAvailabilityChanged();

            // 自分の世代番号を保持
            int myVersion = settings.Version;

            // 死活監視の間隔を反映（0なら止めたまま）
            if (settings.WatchIntervalSec > 0)
            {
                int intervalMs = settings.WatchIntervalSec * 1000;
                _watchTimer.Change(intervalMs, intervalMs);
            }
            else
            {
                _watchTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            // 接続確認（非同期）
            ErrCode errCode = await CheckConnectionAsync(settings);

            // 確認結果を反映する
            // 待機中に新しい設定が適用されていた場合は、この結果を破棄する
            if (!TrySetAvailable(myVersion, errCode == ErrCode.CMD_SUCCESS)) return null;

            return errCode;
        }

        /// <summary>
        /// 設定の世代が最新のときだけ、通信可能フラグを更新する
        /// 世代の確認と更新を、ロックの中でまとめて行う
        /// </summary>
        /// <param name="version">結果を得たときの設定の世代番号</param>
        /// <param name="isAvailable">新しい通信可能フラグ</param>
        /// <returns>更新できたらtrue（設定が変わっていて結果が古い場合は、何もせずfalse）</returns>
        private bool TrySetAvailable(int version, bool isAvailable)
        {
            bool changed;
            lock (_stateLock)
            {
                // 設定が変わった後の古い結果では、状態を変えない
                if (version != _settings.Version) return false;

                // 実際に値が変わったかを覚えておく
                changed = _isAvailable != isAvailable;
                _isAvailable = isAvailable;
            }

            // 変わったときだけ、ロックの外で通知する
            if (changed) RaiseAvailabilityChanged();
            return true;
        }

        /// <summary>
        /// EpgTimerSrvと通信可能かテストを行う
        /// 通信は別スレッドで行い、タイムアウトまで待つ（UIスレッドを止めないため）
        /// </summary>
        private static Task<ErrCode> CheckConnectionAsync(ConnectionSettings settings)
        {
            return Task.Run(() =>
            {
                // 巡回カウンタ0で問い合わせると、サーバーは待たずに現在の状態を返す
                return RunOnServer(settings, c =>
                {
                    var status = new NotifySrvInfo();
                    return c.SendGetNotifySrvStatus(ref status);
                }, CmdTimeoutMs);
            });
        }

        /// <summary>
        /// CtrlCmdUtilのラッパーメソッド
        /// 使い方の例： ErrCode err = _edcbClient.ExecuteCmd(c => c.SendEnumReserve(ref list));
        /// CtrlCmdUtil：ErrCode err = _ctrlCmdUtil.SendEnumReserve(ref list);
        /// </summary>
        /// <param name="command">CtrlCmdUtilを使った通信処理（ErrCodeを返す関数）</param>
        /// <returns>通信の結果。通信しなかった場合やタイムアウトの場合も、ErrCodeで返す</returns>
        public ErrCode ExecuteCmd(Func<CtrlCmdUtil, ErrCode> command)
        {
            // 通信できない状態なら、即座に接続エラーを返す
            if (!_isAvailable) return ErrCode.CMD_ERR_CONNECT;

            ConnectionSettings settings = _settings;
            ErrCode errCode = RunOnServer(settings, command, CmdTimeoutMs);

            // 接続系のエラー（接続失敗・切断・タイムアウト）かどうか
            bool isConnectionError =
                errCode == ErrCode.CMD_ERR_CONNECT ||      // サーバーにコネクトできなかった
                errCode == ErrCode.CMD_ERR_DISCONNECT ||   // サーバーから切断された
                errCode == ErrCode.CMD_ERR_TIMEOUT;        // タイムアウト発生

            // 通信不能にすべきか
            // ・TCP：接続系のエラーなら通信不能
            // ・パイプ：接続待ちイベントが消えている（＝サーバーが停止した）ときだけ通信不能
            //   （パイプのときは CreateCtrlCmdUtil が必ず非nullを返すため、ここで参照しても安全）
            bool serverDown = isConnectionError &&
                (settings.UseTcpIp || !CreateCtrlCmdUtil(settings).PipeExists());

            // 通信不能にする（設定が変わった後の古い結果の場合は、TrySetAvailableが何もしない）
            if (serverDown)
            {
                TrySetAvailable(settings.Version, false);
            }

            return errCode;
        }

        /// <summary>
        /// ホスト名またはIPアドレスの文字列を IPAddress に変換する（タイムアウトなし・同期）
        /// ・IPアドレスの文字列なら、DNSを使わずにそのまま変換する
        /// ・それ以外はホスト名として DNS で調べる（IPv4アドレスを優先する）
        /// ・変換できなければ null を返す
        /// 呼び出し側（別スレッド）でタイムアウトを設けること
        /// </summary>
        private static IPAddress ResolveHostName(string hostName)
        {
            string text = NormalizeHostName(hostName);
            if (text == null) return null;

            // IPアドレスとして読めるなら、DNSを使わずにそのまま返す（IPv4もIPv6もここで変換される）
            if (IPAddress.TryParse(text, out IPAddress ip)) return ip;

            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(text);

                // IPv4アドレスを優先し、無ければ最初のアドレスを使う
                return addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                    ?? addresses.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // 存在しないホスト名などは例外になるため、ここで受け止めて「変換失敗」とする
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// 入力された文字列を、名前解決に使える形に整える
        /// 前後の空白と、IPv6の角かっこを取り除く
        /// 空文字、空白だけ、途中に空白を含む場合は null（変換しない）
        /// </summary>
        private static string NormalizeHostName(string hostName)
        {
            if (string.IsNullOrWhiteSpace(hostName)) return null;

            // 前後の空白は取り除く
            string text = hostName.Trim();

            // IPv6は「[::1]」のように角かっこで囲まれることがあるため、角かっこを外す
            if (text.Length >= 2 && text.StartsWith("[") && text.EndsWith("]"))
            {
                text = text.Substring(1, text.Length - 2);
            }

            // 空白を含むものは、ホスト名としても IPアドレスとしても無効
            // （空文字の場合に Dns.GetHostAddresses("") が自分のPCのアドレスを返すのを防ぐため）
            if (text.Length == 0 || text.Any(char.IsWhiteSpace)) return null;

            return text;
        }

        /// <summary>
        /// ホスト名またはIPアドレスの文字列を IPAddress に変換する（WakeOnLan などの非同期の呼び出し元向け）
        /// 時間内に変換できなければ null を返す
        /// </summary>
        /// <param name="hostName">ホスト名、またはIPアドレスの文字列</param>
        public static async Task<IPAddress> ResolveHostAsync(string hostName)
        {
            Task<IPAddress> lookupTask = Task.Run(() => ResolveHostName(hostName));

            // DNSの問い合わせは応答が遅いことがあるため、タイムアウトを設けて待つ
            Task done = await Task.WhenAny(lookupTask, Task.Delay(CmdTimeoutMs));

            // 時間内に終わらなければ変換失敗とする
            return done == lookupTask ? await lookupTask : null;
        }

        /// <summary>
        /// パイプ方式の通知登録・解除を行う（EpgTimerSrvへの命令）
        /// 接続設定（TCP/パイプ）や通信可能状態に関係なく、必ずローカルのパイプへ送る
        /// 例： _edcbClient.ExecutePipeRegistCmd(c => c.SendRegistGUI(pid));
        /// </summary>
        public ErrCode ExecutePipeRegistCmd(Func<CtrlCmdUtil, ErrCode> command, int timeoutMs = PipeRegistTimeoutMs)
        {
            var ctrlCmdUtil = new CtrlCmdUtil();
            ctrlCmdUtil.SetSendMode(false);
            ctrlCmdUtil.SetConnectTimeOut(timeoutMs);

            return RunWithTimeout(() => command(ctrlCmdUtil), timeoutMs);
        }

        /// <summary>
        /// 接続設定に従って、新しいCtrlCmdUtilを作る
        /// TCPのときは、この時点でホスト名をIPアドレスに変換する（通信のたびに変換するため）
        /// 変換できなかったときは null を返す（呼び出し側で接続失敗として扱う）
        /// パイプのときは、必ず非nullを返す
        /// </summary>
        private static CtrlCmdUtil CreateCtrlCmdUtil(ConnectionSettings settings)
        {
            var ctrlCmdUtil = new CtrlCmdUtil();
            ctrlCmdUtil.SetSendMode(settings.UseTcpIp);

            // パイプ接続のときは、接続先の設定が不要なのでそのまま返す
            if (!settings.UseTcpIp) return ctrlCmdUtil;

            // TCP/IP接続のときは、ホスト名をIPアドレスに変換して接続先を設定する
            IPAddress ip = ResolveHostName(settings.HostName);
            if (ip == null) return null;

            ctrlCmdUtil.SetNWSetting(ip, settings.PortNumber);
            return ctrlCmdUtil;
        }

        /// <summary>
        /// 接続設定に従って通信を行う（名前解決と通信を、同じタイムアウトの中で行う）
        /// 名前解決に失敗したときは、通信せずに接続失敗（CMD_ERR_CONNECT）を返す
        /// </summary>
        private static ErrCode RunOnServer(ConnectionSettings settings, Func<CtrlCmdUtil, ErrCode> command, int timeoutMs)
        {
            return RunWithTimeout(() =>
            {
                // 通信用のCtrlCmdUtilを作る（TCPのときはここでホスト名を変換する）
                CtrlCmdUtil ctrlCmdUtil = CreateCtrlCmdUtil(settings);

                // 変換できなかった場合は、通信せずに接続失敗とする
                if (ctrlCmdUtil == null) return ErrCode.CMD_ERR_CONNECT;

                return command(ctrlCmdUtil);
            }, timeoutMs);
        }

        /// <summary>
        /// 通信処理を別スレッドで実行し、タイムアウトまで待つ
        /// （CtrlCmdUtilには通信を途中でやめる仕組みがないため、待つ側だけで時間を制限する）
        /// タイムアウトしても通信処理は終わるまで残るが、使うのは呼び出し専用のCtrlCmdUtilだけのため他に影響しない
        /// </summary>
        private static ErrCode RunWithTimeout(Func<ErrCode> func, int timeoutMs)
        {
            Task<ErrCode> task = Task.Run(() =>
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    // パイプ通信は、接続できないときに例外が出ることがある
                    // アプリが落ちないように、ここで受け止めて「接続失敗」として返す
                    System.Diagnostics.Debug.WriteLine(ex);
                    return ErrCode.CMD_ERR_CONNECT;
                }
            });

            return task.Wait(timeoutMs) ? task.Result : ErrCode.CMD_ERR_TIMEOUT;
        }

        /// <summary>
        /// 死活監視のタイマーを破棄する
        /// </summary>
        public void Dispose()
        {
            _watchTimer.Dispose();
        }
    }

    /// <summary>
    /// 死活監視の結果
    /// </summary>
    public class ServerStatusEventArgs : EventArgs
    {
        /// <summary>通信できたかどうか</summary>
        public bool IsAvailable { get; }

        /// <summary>
        /// TCPで通信できたときの状態（巡回カウンタを含む）。
        /// パイプの場合や通信できなかった場合は null
        /// </summary>
        public NotifySrvInfo Status { get; }

        public ServerStatusEventArgs(bool isAvailable, NotifySrvInfo status)
        {
            IsAvailable = isAvailable;
            Status = status;
        }
    }
}