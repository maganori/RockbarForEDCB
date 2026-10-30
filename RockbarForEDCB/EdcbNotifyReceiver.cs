using EpgTimer;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RockbarForEDCB
{
    /// <summary>
    /// EpgTimerSrvからの通知を受け取るクラス
    /// ・パイプ接続：名前付きパイプでプッシュ通知を受け取る（EpgTimerSrvへの登録が必要）
    /// ・TCP/IP接続：NWConnect（ロングポーリング）で通知を受け取る（登録は不要）
    /// 死活監視はEdcbClientが行い、その結果（StatusChecked）を受けて接続・再接続を行う
    /// </summary>
    public sealed class EdcbNotifyReceiver : IDisposable
    {
        // EDCB(CtrlCmdDef.h / SendCtrlCmd.cpp)の命名規則（EpgTimer本体と一致させる必要がある）
        // パイプ名   : "EpgTimerGUI_Ctrl_BonPipe_" + プロセスID(10進数)
        // イベント名 : パイプ名の"Pipe"を"Connect"に置き換え、先頭に"Global\"を付けたもの
        private const string GuiPipePrefix = "EpgTimerGUI_Ctrl_BonPipe_";
        private const string GuiEventPrefix = "Global\\EpgTimerGUI_Ctrl_BonConnect_";

        /// <summary>
        /// 受け取りの状態（EdcbNotifyReceiverの内部だけで使う）
        /// </summary>
        private enum ReceiveState
        {
            Stopped,        // 受け取りを要望していない（Start前、またはStop後）
            Disconnected,   // 受け取りを要望しているが、未接続（死活監視の結果で接続を試みる）
            Connecting,     // 接続処理の途中（二重に接続しないため）
            Connected,      // 登録・接続済み
        }

        private readonly EdcbClient _edcbClient;

        // このプロセスのID（登録と解除で同じ値を使うため）
        private readonly uint _pid = GetCurrentProcessId();

        // 以下の状態は _lock で保護する
        // 死活監視の結果や接続処理は別スレッドから届くため、読み書きは必ずロックの中で行うこと
        private readonly object _lock = new object();
        private ReceiveState _state = ReceiveState.Stopped;
        private bool _useTcpIp;          // Start時の接続方式
        private int _generation;         // Start・Stopのたびに増やす（古い接続処理の結果を捨てるため）
        private uint _lastTcpCount;      // TCP：最後に受け取った巡回カウンタ
        private PipeServer _pipeServer;  // パイプ：受け口（登録が失敗しても残す）
        private NWConnect _nwConnect;    // TCP：ロングポーリング

        /// <summary>
        /// 実際に接続できているかどうか
        /// 受け取りを要望していても、接続できていなければfalse
        /// </summary>
        public bool IsConnected
        {
            get
            {
                lock (_lock) { return _state == ReceiveState.Connected; }
            }
        }

        /// <summary>
        /// 通知を受け取ったときに発生する
        /// パイプ・TCPのどちらの受信スレッドからも呼ばれる（UIスレッドではない）
        /// </summary>
        public event EventHandler<UpdateNotifyItem> NotificationReceived;

        /// <summary>
        /// 通知の受け取りを接続したときに発生する
        /// 起動時の接続、停止からの復帰、再起動の検知による接続し直しのすべてで発生する
        /// 止まっている間の更新を取りこぼしている可能性があるため、受け取った側は一覧を取り直すこと
        /// 接続処理のスレッドから呼ばれる（UIスレッドではない）
        /// </summary>
        public event EventHandler Reconnected;

        /// <summary>
        /// コンストラクタ。死活監視の結果を購読する
        /// </summary>
        public EdcbNotifyReceiver(EdcbClient edcbClient)
        {
            _edcbClient = edcbClient;
            _edcbClient.StatusChecked += EdcbClient_StatusChecked;
        }

        /// <summary>
        /// 通知の受け取り登録を要望する（接続方式と接続先はEdcbClientの設定を使う）
        /// 接続処理は別スレッドで行うため、すぐに戻る。接続できたらReconnectedで知らせる
        /// 接続できなくても要望は残り、死活監視の結果に合わせて接続を試みる
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                // すでに要望している場合は何もしない
                if (_state != ReceiveState.Stopped) return;

                _state = ReceiveState.Disconnected;
                _useTcpIp = _edcbClient.UseTcpIp;
                _generation++;
            }

            // 接続の完了を待つと、サーバーが応答しないとき画面が止まるため、別スレッドで行う
            Task.Run(() => TryConnect());
        }

        /// <summary>
        /// 通知の受け取りを止める（パイプは登録を解除してから受け口を閉じる）
        /// 要望していなければ何もしない。何度呼んでもよい
        /// </summary>
        public void Stop()
        {
            PipeServer pipeServer;
            NWConnect nwConnect;
            bool needsUnregister;
            lock (_lock)
            {
                // 要望していなければ、後始末するものはない
                if (_state == ReceiveState.Stopped) return;

                // 接続処理の途中でStopされた場合は、TryConnectの中で世代の違いを見て取り消す
                needsUnregister = _state == ReceiveState.Connected && !_useTcpIp;
                _state = ReceiveState.Stopped;
                _generation++;

                pipeServer = _pipeServer;
                _pipeServer = null;
                nwConnect = _nwConnect;
                _nwConnect = null;
            }

            // 登録を解除してから受け口を閉じる（EpgTimer本体と同じ順番）
            if (needsUnregister)
            {
                _edcbClient.ExecutePipeRegistCmd(c => c.SendUnRegistGUI(_pid));
            }
            pipeServer?.Dispose();

            // ロングポーリングの終了を待つため、ロックの外で行う
            nwConnect?.Dispose();

            Debug.WriteLine("[EdcbNotify] 通知の受け取りを停止");
        }

        /// <summary>
        /// 購読を解除し、通知の受け取りを止める
        /// 終了処理で呼ぶ（先に通知の受け取りを止めてから、EdcbClientを破棄する）
        /// </summary>
        public void Dispose()
        {
            _edcbClient.StatusChecked -= EdcbClient_StatusChecked;
            Stop();
        }

        /// <summary>
        /// 接続を試みる（Startと、死活監視でサーバーが戻ったときに呼ばれる）
        /// 時間のかかる処理はロックの外で行うため、状態はConnectingにして二重に接続しないようにする
        /// </summary>
        /// <returns>接続できていればtrue</returns>
        private bool TryConnect()
        {
            bool useTcpIp;
            int generation;
            lock (_lock)
            {
                // 未接続（Disconnected）のときだけ接続を試みる
                // 要望していない、接続済み、接続中のいずれかなら、現在の状態で戻る
                if (_state != ReceiveState.Disconnected) return _state == ReceiveState.Connected;

                _state = ReceiveState.Connecting;
                useTcpIp = _useTcpIp;
                generation = _generation;

                // 古いNWConnectは解除済みなので、新しい接続の巡回カウンタを0から始める
                // 接続を始めてから0にすると、最初の応答で受け取った値を消してしまうため、先に0にする
                _lastTcpCount = 0;
            }

            NWConnect nwConnect = null;
            bool isConnectSucceeded = false;
            try
            {
                if (useTcpIp)
                {
                    nwConnect = ConnectTcp();
                    isConnectSucceeded = (nwConnect != null);
                }
                else
                {
                    isConnectSucceeded = ConnectPipe();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[EdcbNotify] 接続例外: " + ex.Message);
            }

            bool connected;
            lock (_lock)
            {
                // 接続中にStop・Startされていた場合は、世代が変わっているので接続済みにしない
                connected = isConnectSucceeded && _state == ReceiveState.Connecting && _generation == generation;

                if (connected)
                {
                    _state = ReceiveState.Connected;
                    if (useTcpIp) _nwConnect = nwConnect;
                }
                else if (_state == ReceiveState.Connecting && _generation == generation)
                {
                    // 接続に失敗した。要望は残っているので、次の死活監視の結果で再試行する
                    _state = ReceiveState.Disconnected;
                }
            }

            if (!connected)
            {
                // 作ったNWConnectは捨てる
                // パイプの登録は、Stopされていた場合は受け口が閉じられるため、サーバーが自然に削除する
                nwConnect?.Dispose();
                return false;
            }

            Debug.WriteLine("[EdcbNotify] 接続しました (" + (useTcpIp ? "TCP" : "パイプ") + ")");

            // 接続できたことを知らせる（一覧の取り直しは受け取った側で行う）
            Reconnected?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// 【パイプ】受け口を用意してから、EpgTimerSrvに登録する
        /// 受け口は登録が失敗しても残すため（再登録のときも接続待ちイベントが必要になる）
        /// </summary>
        private bool ConnectPipe()
        {
            lock (_lock)
            {
                // 停止された後に受け口が残らないよう、接続処理の途中であることを確かめてから作る
                if (_state != ReceiveState.Connecting) return false;

                // 接続待ちイベントもこの時点で作られる
                if (_pipeServer == null)
                {
                    _pipeServer = new PipeServer(GuiEventPrefix + _pid, GuiPipePrefix + _pid, HandlePipeCommand);
                }
            }

            // EpgTimerSrvに「このプロセスへ通知を送ってよい」と登録する
            // 受け口を先に作るのは、サーバーが登録後に接続待ちイベントを最大5秒待つため
            // 登録のタイムアウトは3秒なので、先に作っておかないと登録が失敗するため
            ErrCode err = _edcbClient.ExecutePipeRegistCmd(c => c.SendRegistGUI(_pid));
            if (err != ErrCode.CMD_SUCCESS)
            {
                Debug.WriteLine("[EdcbNotify] 登録失敗: " + err);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 【TCP】NWConnectのロングポーリングを開始する
        /// 接続できなければnullを返す
        /// </summary>
        private NWConnect ConnectTcp()
        {
            // 接続先はEdcbClientの設定と同じにするため、変換できなければLoopbackを使う
            IPAddress ip = IPAddress.TryParse(_edcbClient.IpAddress, out var parsed) ? parsed : IPAddress.Loopback;

            var nw = new NWConnect();
            if (!nw.ConnectServer(ip, _edcbClient.PortNumber, 0, HandleTcpCommand))
            {
                // サーバーに接続できない、またはロングポーリング未対応
                nw.Dispose();
                return null;
            }

            return nw;
        }

        /// <summary>
        /// 接続を解除する（死活監視でサーバーの停止を検知したときに呼ぶ）
        /// 要望は残すため、次の死活監視の結果で接続し直せる
        /// ロングポーリングの終了を待つため、時間がかかることがある（別スレッドから呼ぶこと）
        /// </summary>
        private void Disconnect()
        {
            NWConnect nwConnect;
            lock (_lock)
            {
                // 接続済みのときだけ解除する（停止中や未接続のときは何もしない）
                if (_state != ReceiveState.Connected) return;

                _state = ReceiveState.Disconnected;
                nwConnect = _nwConnect;
                _nwConnect = null;
            }

            // ロングポーリングの終了を待つため、ロックの外で行う
            nwConnect?.Dispose();
            Debug.WriteLine("[EdcbNotify] 接続を解除しました");
        }

        /// <summary>
        /// 接続を解除して、接続し直す（再起動の検知時に呼ぶ）
        /// 解除と接続の両方で時間がかかることがあるため、別スレッドから呼ぶこと
        /// </summary>
        private void Reconnect()
        {
            Disconnect();
            TryConnect();
        }

        /// <summary>
        /// 死活監視の結果を処理する（EdcbClientのタイマーのスレッドから呼ばれる）
        /// 停止を見たら接続を解除し、戻ってきたら接続し直す
        /// </summary>
        private void EdcbClient_StatusChecked(object sender, ServerStatusEventArgs e)
        {
            ReceiveState state;
            bool useTcpIp;
            uint lastCount;
            lock (_lock)
            {
                state = _state;
                useTcpIp = _useTcpIp;
                lastCount = _lastTcpCount;
            }

            // 要望していない、または接続処理の途中なら何もしない
            if (state == ReceiveState.Stopped || state == ReceiveState.Connecting) return;

            // このメソッドは死活監視のタイマーのスレッドから呼ばれる
            // 接続・解除は時間がかかることがあり、ここで待つと死活監視が止まるため、別スレッドで行う
            // 状態の変更はロックの中で行われるため、続けて呼ばれても二重に接続・解除はされない

            if (!e.IsAvailable)
            {
                // サーバーの停止を検知するため、接続を解除する（戻ってきたら接続し直す）
                // 接続済みのときだけ、別スレッドを使う
                if (state == ReceiveState.Connected)
                {
                    Task.Run(() => Disconnect());
                }
                return;
            }

            if (state == ReceiveState.Disconnected)
            {
                // 起動時に停止していた、または停止したため未接続になっているので接続を試みる
                // 接続できたかどうかは、TryConnectの中でReconnectedにより知らされる
                Task.Run(() => TryConnect());
                return;
            }

            // 接続済みの場合：TCPは巡回カウンタが戻ったら、サーバーが再起動したとみなして接続し直す
            // パイプは状態を問い合わせないため、ここでは再起動を検知しない（停止は上で検知する）
            if (useTcpIp && e.Status != null)
            {
                // 巡回カウンタは2^32で一周するため、差が2^31未満かどうかで前後を判定する
                uint count = e.Status.param3;
                bool counterWentBack = lastCount != 0
                    && count != lastCount
                    && unchecked(lastCount - count) < 0x80000000u;

                if (counterWentBack)
                {
                    Debug.WriteLine("[EdcbNotify] EpgTimerSrvの再起動を検知。接続し直します");
                    Task.Run(() => Reconnect());
                }
            }
        }

        /// <summary>
        /// 通知を購読側に知らせる（パイプ・TCP共通）
        /// 購読側の例外は、EpgTimerSrvへの応答に影響させないため、ここで受け止める
        /// </summary>
        private void RaiseNotification(NotifySrvInfo info)
        {
            Debug.WriteLine("[EdcbNotify] 受信 notifyID=" + info.notifyID +
                " param1=" + info.param1 + " param3=" + info.param3);

            try
            {
                NotificationReceived?.Invoke(this, (UpdateNotifyItem)info.notifyID);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[EdcbNotify] 通知の購読側で例外: " + ex.Message);
            }
        }

        /// <summary>
        /// 【パイプ】パイプ受信スレッドから呼ばれるコマンド処理
        /// 通知以外のコマンドはCMD_NON_SUPPORTを返す（EpgTimer本体と同じ）
        /// CMD_SUCCESSを返すと、EpgTimerSrvは「GUIが対応した」と判断し、スタンバイ問い合わせなどを横取りするため
        /// </summary>
        private Tuple<ErrCode, byte[], uint> HandlePipeCommand(uint cmdParam, byte[] cmdData)
        {
            if (cmdParam != (uint)CtrlCmd.CMD_TIMER_GUI_SRV_STATUS_NOTIFY2)
            {
                return Tuple.Create(ErrCode.CMD_NON_SUPPORT, (byte[])null, 0u);
            }

            // 通知データの解析（失敗したときだけ、読み取れなかったことをEpgTimerSrvに返す）
            NotifySrvInfo info;
            try
            {
                info = ParseNotifyInfo(cmdData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[EdcbNotify] パイプ受信処理の例外: " + ex.Message);
                return Tuple.Create(ErrCode.CMD_ERR, (byte[])null, 0u);
            }

            // 通知を発生させる
            RaiseNotification(info);

            // 巡回カウンタをItem3で返す（EpgTimer本体と同じ。パイプでは使われない）
            return Tuple.Create(ErrCode.CMD_SUCCESS, (byte[])null, info.param3);
        }

        /// <summary>
        /// 【TCP】ロングポーリングの受信スレッドから呼ばれるコマンド処理
        /// 戻り値のItem3は「次に待ち受ける巡回カウンタ」で、NWConnectが次の問い合わせに使う
        /// </summary>
        private Tuple<ErrCode, byte[], uint> HandleTcpCommand(uint cmdParam, byte[] cmdData)
        {
            uint lastCount;
            lock (_lock)
            {
                lastCount = _lastTcpCount;
            }

            if (cmdParam != (uint)CtrlCmd.CMD_TIMER_GUI_SRV_STATUS_NOTIFY2)
            {
                return Tuple.Create(ErrCode.CMD_NON_SUPPORT, (byte[])null, lastCount);
            }

            // 通知データの解析
            NotifySrvInfo info;
            try
            {
                info = ParseNotifyInfo(cmdData);
            }
            catch (Exception ex)
            {
                // 解析に失敗した場合も直前のカウンタを返す（0を返すと同じ通知が連続で届くため）
                Debug.WriteLine("[EdcbNotify] TCP受信処理の例外: " + ex.Message);
                return Tuple.Create(ErrCode.CMD_SUCCESS, (byte[])null, lastCount);
            }

            // 次回はこの巡回カウンタより新しい通知だけを待ち受けるため、保存する
            lock (_lock)
            {
                _lastTcpCount = info.param3;
            }

            // 通知を発生させる
            RaiseNotification(info);

            return Tuple.Create(ErrCode.CMD_SUCCESS, (byte[])null, info.param3);
        }

        /// <summary>
        /// このプロセスのIDを取得する
        /// Processオブジェクトは破棄が必要なため、usingで囲む
        /// </summary>
        private static uint GetCurrentProcessId()
        {
            using (Process process = Process.GetCurrentProcess())
            {
                return (uint)process.Id;
            }
        }

        /// <summary>
        /// 通知データ（先頭2バイトがバージョン、そのあとNotifySrvInfo）を解析する
        /// パイプとTCPで共通（EpgTimer本体のOutsideCmdCallbackと同じ読み方）
        /// </summary>
        private static NotifySrvInfo ParseNotifyInfo(byte[] cmdData)
        {
            using (var ms = new MemoryStream(cmdData, false))
            {
                var reader = new CtrlCmdReader(ms);

                // 先頭2バイトはバージョン
                ushort version = 0;
                reader.Read(ref version);
                reader.Version = version;

                // 通知データ本体を読む
                var info = new NotifySrvInfo();
                reader.Read(ref info);
                return info;
            }
        }
    }
}