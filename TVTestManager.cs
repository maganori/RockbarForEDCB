using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using EpgTimer;

namespace RockbarForEDCB
{
    /// <summary>
    /// TVTestの起動・制御・自動プロセス管理を行うクラス
    /// </summary>
    public class TVTestManager
    {
        private readonly ConfigManager _configManager;
        private readonly CtrlCmdUtil _ctrlCmdUtil;

        // Rockbarから自動起動したTVTestのプロセス一覧
        private readonly Dictionary<string, Process> _tvtestProcesses = new Dictionary<string, Process>();

        public TVTestManager(ConfigManager configManager, CtrlCmdUtil ctrlCmdUtil)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _ctrlCmdUtil = ctrlCmdUtil ?? throw new ArgumentNullException(nameof(ctrlCmdUtil));
        }

        /// <summary>
        /// TVTest起動処理
        /// TSID, SIDを指定してTVTestを起動する。ネットワークタイプで異なるオプションを使用する。
        /// TVTest起動オプションがあれば最優先で適用される
        /// </summary>
        /// <param name="networkType">ネットワークタイプ</param>
        /// <param name="tsid">TSID</param>
        /// <param name="sid">SID</param>
        /// <param name="tvtestOption">TVTest起動オプション（オプション）</param>
        /// <returns>TVTestプロセス</returns>
        public Process StartTVTest(NetworkType networkType, uint tsid, uint sid, string tvtestOption = null)
        {
            Process result = null;

            try
            {
                if (tvtestOption != null)
                {
                    result = Process.Start(_configManager.RockbarSetting.TvtestPath, $"{tvtestOption} /tsid {tsid} /sid {sid}");
                }
                else if (networkType == NetworkType.DTTV)
                {
                    result = Process.Start(_configManager.RockbarSetting.TvtestPath, $"{_configManager.RockbarSetting.TvtestDttvOption} /tsid {tsid} /sid {sid}");
                }
                else if (networkType == NetworkType.BS || networkType == NetworkType.CS)
                {
                    result = Process.Start(_configManager.RockbarSetting.TvtestPath, $"{_configManager.RockbarSetting.TvtestBscsOption} /tsid {tsid} /sid {sid}");
                }
                else if (networkType == NetworkType.CATV)
                {
                    result = Process.Start(_configManager.RockbarSetting.TvtestPath, $"{_configManager.RockbarSetting.TvtestCatvOption} /tsid {tsid} /sid {sid}");
                }
                else if (networkType == NetworkType.SPHD)
                {
                    result = Process.Start(_configManager.RockbarSetting.TvtestPath, $"{_configManager.RockbarSetting.TvtestSphdOption} /tsid {tsid} /sid {sid}");
                }
                else if (networkType == NetworkType.BS4K)
                {
                    result = Process.Start(_configManager.RockbarSetting.TvtestPath, $"{_configManager.RockbarSetting.TvtestBs4kOption} /tsid {tsid} /sid {sid}");
                }
            }
            catch
            {
                MessageBox.Show("TVTestの起動に失敗しました。TVTestの設定を見直してください。", "TVTest起動エラー");
            }
            return result;
        }

        /// <summary>
        /// TvtPlayプラグインを有効化してTVTest起動処理
        /// ファイルパスとTvtPlayの起動オプションを指定してTVTestを起動する。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>TVTestプロセス</returns>
        public Process StartTvtPlay(string filePath)
        {
            Process result = null;

            try
            {
                result = Process.Start(_configManager.RockbarSetting.TvtestPath, $"{_configManager.RockbarSetting.TvtestTsFileOption} \"{filePath}\"");
            }
            catch
            {
                MessageBox.Show("TVTestの起動に失敗しました。TVTestの設定を見直してください。", "TVTest起動エラー");
            }

            return result;
        }

        /// <summary>
        /// 録画済みファイルをTVTest/TvtPlayで再生する
        /// </summary>
        /// <param name="recFile">録画ファイル情報</param>
        /// <returns>TVTestプロセス</returns>
        public Process PlayRecFile(RecFileInfo recFile)
        {
            if (_configManager.RockbarSetting.UseTcpIp && _configManager.RockbarSetting.IpAddress.IndexOf("127.0.0.1") < 0)
            {
                string networkPath = "";
                ErrCode errCode = _ctrlCmdUtil.SendGetRecFileNetworkPath(recFile.RecFilePath, ref networkPath);
                if (errCode != ErrCode.CMD_SUCCESS || string.IsNullOrEmpty(networkPath))
                {
                    MessageBox.Show("ネットワークパスの取得に失敗しました。EDCBの設定を見直してください。", "ネットワークパスエラー");
                    return null;
                }
                return StartTvtPlay(networkPath);
            }
            else
            {
                return StartTvtPlay(recFile.RecFilePath);
            }
        }

        /// <summary>
        /// TVTestの自動起動・自動終了の判定
        /// </summary>
        /// <param name="reserveDatas">予約情報リスト</param>
        /// <param name="favoriteServiceList">お気に入りサービスリスト</param>
        public void AutoStartAndCloseTVTest(List<ReserveData> reserveDatas, List<Service> favoriteServiceList)
        {
            if (!_configManager.RockbarSetting.IsAutoOpenTvtest)
            {
                return;
            }

            DateTime now = DateTime.Now;

            // TVTest自動起動
            // 実実装としては毎秒チェックするのではなく、毎分(59-マージン)秒タイミングで次の1分間に始まる番組をオープンする
            if (now.Second == (59 - _configManager.RockbarSetting.AutoOpenMargin))
            {
                // 0の場合はこの1分間なので59秒加算
                DateTime checkTime = now.AddSeconds(59);

                // お気に入りサービスのキー(大した件数ではない想定なので毎秒計算し直しで良いものとする)
                var favoriteServiceMap = favoriteServiceList?
                    .ToDictionary(x => RockbarUtility.GetKey(x.Tsid, x.Sid))
                    ?? new Dictionary<string, Service>();

                foreach (var reserve in reserveDatas)
                {
                    if (reserve.StartTime.Date == checkTime.Date &&
                        reserve.StartTime.Hour == checkTime.Hour &&
                        reserve.StartTime.Minute == checkTime.Minute)
                    {
                        string key = RockbarUtility.GetKey(reserve.TransportStreamID, reserve.ServiceID);

                        // すでに自動起動中のTVTestとTSID・SIDが同一の場合、(TVTest側でチャンネルが変わっていない前提で)起動スキップする
                        if (_tvtestProcesses.ContainsKey(key))
                        {
                            continue;
                        }

                        // お気に入りサービスに含まれているかチェックしつつ Service を取得
                        bool isFavorite = favoriteServiceMap.TryGetValue(key, out Service service);

                        // お気に入りサービスオプションが設定されている場合、お気に入りサービスに含まれていなかったらスキップ
                        if (_configManager.RockbarSetting.IsAutoOpenTvtestFavoriteService && !isFavorite)
                        {
                            continue;
                        }

                        NetworkType networkType = RockbarUtility.GetNetworkType(service?.TypeName, reserve.OriginalNetworkID);
                        if ((networkType == NetworkType.DTTV && _configManager.RockbarSetting.IsAutoOpenTvtestDttv) ||
                            ((networkType == NetworkType.BS) && _configManager.RockbarSetting.IsAutoOpenTvtestBs) ||
                            ((networkType == NetworkType.CS) && _configManager.RockbarSetting.IsAutoOpenTvtestCs) ||
                            ((networkType == NetworkType.CATV) && _configManager.RockbarSetting.IsAutoOpenTvtestCatv) ||
                            ((networkType == NetworkType.BS4K) && _configManager.RockbarSetting.IsAutoOpenTvtestBs4k) ||
                            ((networkType == NetworkType.SPHD) && _configManager.RockbarSetting.IsAutoOpenTvtestSphd) )
                        {
                            var p = StartTVTest(networkType, reserve.TransportStreamID, reserve.ServiceID, service?.TvtestOption);
                            if (p != null)
                            {
                                _tvtestProcesses.Add(key, p);
                            }
                        }
                    }
                }
            }

            // TVTest自動終了
            // 現時点のオプションにかかわらず、自身が開いたTVTestは予約終了時間でクローズ
            // 実実装としては毎秒チェックするのではなく、毎分マージン秒タイミングで現在放送してない番組をクローズ
            if (now.Second == _configManager.RockbarSetting.AutoCloseMargin)
            {
                // 閉じてるプロセスは取り除く
                var keys = _tvtestProcesses.Keys.ToList();

                foreach (var key in keys)
                {
                    try
                    {
                        if (_tvtestProcesses[key].HasExited)
                        {
                            _tvtestProcesses.Remove(key);
                        }
                    }
                    catch
                    {
                        // 何らかの理由でプロセスにアクセスできない場合もキーを削除
                        _tvtestProcesses.Remove(key);
                    }
                }

                // 把握してる生存プロセスの中で、録画放送に該当してるものがない場合はクローズ
                var currentReserves = new Dictionary<string, ReserveData>();

                // 現在放送中番組を抽出
                foreach (var data in reserveDatas)
                {
                    if (data.StartTime < now && data.StartTime.AddSeconds(data.DurationSecond) > now)
                    {
                        // 予約方法次第で同一番組が二重に登録されているケースあり
                        var key = RockbarUtility.GetKey(data.TransportStreamID, data.ServiceID);
                        if (!currentReserves.ContainsKey(key))
                        {
                            currentReserves.Add(key, data);
                        }
                    }
                }

                // 放送中番組でないTVTestを閉じる
                foreach (var p in _tvtestProcesses)
                {
                    if (!currentReserves.ContainsKey(p.Key))
                    {
                        try
                        {
                            p.Value.CloseMainWindow();
                        }
                        catch
                        {
                            // 何らかの理由でプロセスがクローズできない場合
                            // 想定外ケースなのでここに落ちる場合は原因究明要
                        }
                    }
                }
            }
        }
    }
}