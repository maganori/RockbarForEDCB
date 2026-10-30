using EpgTimer;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RockbarForEDCB
{
    /// <summary>
    /// EpgTimerSrvとの接続を一元管理するクラス
    /// ・接続設定（パイプ/TCP、IP、ポート、死活監視の間隔）を保持する
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
            public ConnectionSettings(int version, bool useTcpIp, string ipAddress, uint portNumber, int tcpWatchIntervalSec)
            {
                Version = version;
                UseTcpIp = useTcpIp;

                // 変換できなければ Loopback (127.0.0.1) を使う（nullも変換失敗扱い）
                IpAddress = (ipAddress != null && IPAddress.TryParse(ipAddress, out var parsed))
                    ? parsed
                    : IPAddress.Loopback;

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
            public IPAddress IpAddress { get; }
            public uint PortNumber { get; }
            /// <summary>実際に使われる死活監視の間隔（秒）。0なら死活監視なし</summary>
            public int WatchIntervalSec { get; }
        }

        // 現在の接続設定（UIスレッドで書き、他のスレッドから読む）
        private volatile ConnectionSettings _settings = new ConnectionSettings(0, false, null, 0, 0);

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
        /// 死活監視の結果を通知するイベント
        /// </summary>
        public event EventHandler<ServerStatusEventArgs> StatusChecked;

        /// <summary>EpgTimerSrv と通信可能な状態かを取得</summary>
        public bool IsAvailable => _isAvailable;

        /// <summary>TCP/IP接続かどうか（falseならパイプ接続）</summary>
        public bool UseTcpIp => _settings.UseTcpIp;

        /// <summary>接続先のIPアドレス（文字列）</summary>
        public string IpAddress => _settings.IpAddress.ToString();

        /// <summary>接続先のポート番号</summary>
        public uint PortNumber => _settings.PortNumber;

        /// <summary>実際に適用されている死活監視の間隔（秒）。0なら死活監視なし</summary>
        public int WatchIntervalSec => _settings.WatchIntervalSec;

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
                    CtrlCmdUtil ctrlCmdUtil = CreateCtrlCmdUtil(settings);

                    // 問い合わせの結果を受ける入れ物
                    // タイムアウトで諦めた通信が、あとから書き込む可能性があるため、
                    // 成功を確認できたときだけ status に渡す（status とは別の変数にしている）
                    var receivedInfo = new NotifySrvInfo();

                    // TCPは接続のタイムアウトが効かないことがあるため、RunWithTimeoutで時間を制限する
                    ErrCode err = RunWithTimeout(() => ctrlCmdUtil.SendGetNotifySrvStatus(ref receivedInfo), TcpWatchConnectTimeoutMs);
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
            bool useTcpIp, string ipAddress, uint portNumber, int watchIntervalSec)
        {
            // 世代番号の採番、接続設定の保存、通信可能フラグを下げる処理は、まとめてロックの中で行う
            // （採番と保存の間に別の呼び出しが割り込むと、古い設定が最新として残ってしまうため）
            ConnectionSettings settings;
            lock (_stateLock)
            {
                // 設定世代をインクリメントし、今回の世代番号として使う
                _settingsVersionCounter++;

                // 接続設定のスナップショットを作成し、最新の接続設定として保存
                settings = new ConnectionSettings(_settingsVersionCounter, useTcpIp, ipAddress, portNumber, watchIntervalSec);
                _settings = settings;

                // 接続確認を開始するため、いったん通信可能フラグを下げる
                _isAvailable = false;
            }

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
            lock (_stateLock)
            {
                // 設定が変わった後の古い結果では、状態を変えない
                if (version != _settings.Version) return false;

                _isAvailable = isAvailable;
                return true;
            }
        }

        /// <summary>
        /// EpgTimerSrvと通信可能かテストを行う
        /// </summary>
        private static async Task<ErrCode> CheckConnectionAsync(ConnectionSettings settings)
        {
            // 接続確認用に、新しいCtrlCmdUtilを作る
            CtrlCmdUtil ctrlCmdUtil = CreateCtrlCmdUtil(settings);

            // 通信は別スレッド（Task.Run）で行い、タイムアウトまで待つ
            Task<ErrCode> checkTask = Task.Run(() =>
            {
                try
                {
                    // 巡回カウンタ0で問い合わせると、サーバーは待たずに現在の状態を返す
                    var status = new NotifySrvInfo();
                    return ctrlCmdUtil.SendGetNotifySrvStatus(ref status);
                }
                catch (Exception ex)
                {
                    // パイプ通信は、接続できないときに例外が出ることがある
                    // アプリが落ちないように、ここで受け止めて「接続失敗」として返す
                    System.Diagnostics.Debug.WriteLine(ex);
                    return ErrCode.CMD_ERR_CONNECT;
                }
            });

            // 「通信が終わる」か「タイムアウト」かまで待つ
            Task done = await Task.WhenAny(checkTask, Task.Delay(CmdTimeoutMs));

            // 通信の方が先に終わっていればその結果を、そうでなければタイムアウトを返す
            return done == checkTask ? await checkTask : ErrCode.CMD_ERR_TIMEOUT;
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
            CtrlCmdUtil ctrlCmdUtil = CreateCtrlCmdUtil(settings);

            ErrCode errCode = RunWithTimeout(() => command(ctrlCmdUtil), CmdTimeoutMs);

            // 接続系のエラー（接続失敗・切断・タイムアウト）かどうか
            bool isConnectionError =
                errCode == ErrCode.CMD_ERR_CONNECT ||      // サーバーにコネクトできなかった
                errCode == ErrCode.CMD_ERR_DISCONNECT ||   // サーバーから切断された
                errCode == ErrCode.CMD_ERR_TIMEOUT;     // タイムアウト発生

            // 通信不能にすべきか
            // ・TCP：接続系のエラーなら通信不能
            // ・パイプ：接続待ちイベントが消えている（＝サーバーが停止した）ときだけ通信不能
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
        /// 呼び出し毎に作ることで、設定の変更と通信が同じインスタンスを共有しないようにする
        /// </summary>
        private static CtrlCmdUtil CreateCtrlCmdUtil(ConnectionSettings settings)
        {
            var ctrlCmdUtil = new CtrlCmdUtil();
            ctrlCmdUtil.SetSendMode(settings.UseTcpIp);

            // TCP/IP通信のときだけ、接続先を設定する
            if (settings.UseTcpIp)
            {
                ctrlCmdUtil.SetNWSetting(settings.IpAddress, settings.PortNumber);
            }
            return ctrlCmdUtil;
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