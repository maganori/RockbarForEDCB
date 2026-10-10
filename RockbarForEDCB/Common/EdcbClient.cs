using EpgTimer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RockbarForEDCB
{
    public class EdcbClient
    {
        // 通信1回あたりのタイムアウト時間（ミリ秒）
        private const int CmdTimeoutMs = 10000;

        // 普段の通信用のCtrlCmdUtilインスタンス
        private readonly CtrlCmdUtil _ctrlCmdUtil = new CtrlCmdUtil();

        // EpgTimerSrvと通信可能な状態か
        private volatile bool _isAvailable;

        /// <summary>
        /// EpgTimerSrv と通信可能な状態かを取得
        /// </summary>
        public bool IsAvailable => _isAvailable;

        // 設定が更新された回数（世代番号）
        // 非同期処理中に新しい設定が読み込まれた際、古い結果で上書きされるのを防ぐために使用
        private volatile int _settingsVersion;

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

            // コール時点の設定世代を記録
            int currentVersion = _settingsVersion;

            // 通信処理をバックグラウンドスレッドで実行（CtrlCmdUtilには通信を途中でやめる仕組みがないため）
            Task<ErrCode> task = Task.Run(() =>
            {
                try
                {
                    return command(_ctrlCmdUtil);
                }
                catch (Exception ex)
                {
                    // パイプ通信の失敗や、受信データの不正で例外が出たとき用
                    System.Diagnostics.Trace.WriteLine(ex);
                    return ErrCode.CMD_ERR_CONNECT;
                }
            });

            // 応答があれば結果を、終わらなければタイムアウトとする
            ErrCode errCode = task.Wait(CmdTimeoutMs) ? task.Result : ErrCode.CMD_ERR_TIMEOUT;

            // 通信障害が発生し、かつ世代が変わっていなければ通信不能状態に更新
            if (IsConnectionError(errCode) && currentVersion == _settingsVersion)
            {
                _isAvailable = false;
            }

            return errCode;
        }

        /// <summary>
        /// 「サーバーと繋がらない」ことを表すエラーかどうかを判定する
        /// ビジー（CMD_ERR_BUSY）や、引数の間違い（CMD_ERR_INVALID_ARG）などは含めない
        /// </summary>
        private static bool IsConnectionError(ErrCode errCode)
        {
            return errCode == ErrCode.CMD_ERR_CONNECT      // サーバーにコネクトできなかった
                || errCode == ErrCode.CMD_ERR_DISCONNECT   // サーバーから切断された
                || errCode == ErrCode.CMD_ERR_TIMEOUT;     // タイムアウト発生
        }

        /// <summary>
        /// 接続設定を反映し、EpgTimerSrvと通信できるかを確認
        /// </summary>
        /// <returns>
        /// 確認の結果（CMD_SUCCESSなら成功）
        /// 待機中に新しい設定が適用され、本結果が古くなった場合は null を返す
        /// </returns>
        public async Task<ErrCode?> ApplySettingsAndCheckAsync(bool useTcpIp, string ipAddress, uint portNumber)
        {
            // 設定世代をインクリメントし、自分の世代番号を保持
            int myVersion = ++_settingsVersion;

            // 接続確認を開始するため、いったん通信可能フラグを下げる
            _isAvailable = false;

            // 普段用のCtrlCmdUtilに接続設定を反映
            ApplySettings(_ctrlCmdUtil, useTcpIp, ipAddress, portNumber);

            // 接続確認（非同期）
            ErrCode errCode = await CheckConnectionAsync(useTcpIp, ipAddress, portNumber);

            // 待機中に新しい設定が適用されていた場合、この古い結果は破棄
            if (myVersion != _settingsVersion) return null;

            // 確認結果を反映
            _isAvailable = (errCode == ErrCode.CMD_SUCCESS);
            return errCode;
        }

        /// <summary>
        /// CtrlCmdUtilインスタンスに接続設定を適用
        /// </summary>
        private static void ApplySettings(CtrlCmdUtil ctrlCmdUtil, bool useTcpIp, string ipAddress, uint portNumber)
        {
            // trueならTCP/IP通信、falseならパイプ通信にする
            ctrlCmdUtil.SetSendMode(useTcpIp);

            // TCP/IP通信のときだけ、接続先のIPアドレスとポート番号を設定する
            if (useTcpIp)
            {
                ctrlCmdUtil.SetNWSetting(ipAddress, portNumber);
            }
        }

        /// <summary>
        /// EpgTimerSrvと通信可能かテストを行う
        /// </summary>
        private static async Task<ErrCode> CheckConnectionAsync(bool useTcpIp, string ipAddress, uint portNumber)
        {
            // 接続確認用に、新しいCtrlCmdUtilを作る
            var testCtrlCmdUtil = new CtrlCmdUtil();
            ApplySettings(testCtrlCmdUtil, useTcpIp, ipAddress, portNumber);

            // 通信は別スレッド（Task.Run）で開始する
            Task<ErrCode> checkTask = Task.Run(() =>
            {
                try
                {
                    // チューナーごとの予約一覧を取得するだけの軽い通信
                    var tuners = new List<TunerReserveInfo>();
                    return testCtrlCmdUtil.SendEnumTunerReserve(ref tuners);
                }
                catch (Exception ex)
                {
                    // パイプ通信は、接続できないときに例外が出ることがある
                    // アプリが落ちないように、ここで受け止めて「接続失敗」として返す
                    System.Diagnostics.Trace.WriteLine(ex);
                    return ErrCode.CMD_ERR_CONNECT;
                }
            });

            // 「通信が終わる」か「タイムアウト」かまで待つ
            Task done = await Task.WhenAny(checkTask, Task.Delay(CmdTimeoutMs));

            // 通信の方が先に終わっていればその結果を、そうでなければタイムアウトを返す
            return done == checkTask ? await checkTask : ErrCode.CMD_ERR_TIMEOUT;
        }
    }
}
