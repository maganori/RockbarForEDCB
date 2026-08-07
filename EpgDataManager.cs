using System;
using System.Collections.Generic;
using System.Linq;
using EpgTimer;

namespace RockbarForEDCB
{
    /// <summary>
    /// EDCBからのデータ取得・キャッシュ保持・差分検知を担当するデータ層クラス
    /// </summary>
    public class EpgDataManager
    {
        private readonly CtrlCmdUtil _ctrlCmdUtil;

        // CtrlCmdの結果格納用
        private List<EpgServiceEventInfo> _serviceEvents = new List<EpgServiceEventInfo>();
        private List<TunerReserveInfo> _tunerReserveInfos = new List<TunerReserveInfo>();
        private List<ReserveData> _reserveDatas = new List<ReserveData>();
        private List<RecFileInfo> _recFileInfos = new List<RecFileInfo>();

        // CtrlCmdの結果のハッシュ格納用
        private string _prevServiceHash = string.Empty;
        private string _prevReserveHash = string.Empty;
        private string _prevRecHash = string.Empty;
        private string _prevTunerHash = string.Empty;

        // サービス一覧(+番組)の保持用(TSID + SID → サービス情報(+番組))
        public Dictionary<string, EpgServiceEventInfo> ServiceMap { get; private set; } = new Dictionary<string, EpgServiceEventInfo>();

        // 番組一覧の保持用(TSID + SID + EventID → 番組情報)
        public Dictionary<string, EpgEventInfo> AllEventMap { get; private set; } = new Dictionary<string, EpgEventInfo>();

        // 予約情報の保持用(TSID + SID + EventID → 予約情報)
        public Dictionary<string, ReserveData> ReserveMap { get; private set; } = new Dictionary<string, ReserveData>();

        // 録画済み情報の保持用(TSID + SID + EventID → 録画済み情報)
        public Dictionary<uint, RecFileInfo> RecMap { get; private set; } = new Dictionary<uint, RecFileInfo>();

        // 外部から参照するための Reader プロパティ（フィールドを返す）
        public List<EpgServiceEventInfo> ServiceEvents => _serviceEvents;
        public List<TunerReserveInfo> TunerReserveInfos => _tunerReserveInfos;
        public List<ReserveData> ReserveDatas => _reserveDatas;
        public List<RecFileInfo> RecFileInfos { get; private set; } = new List<RecFileInfo>(); // UpdateRecData内で再代入(GetRange)を行うためsetterを付ける
        public DateTime LastRecDataUpdateTime { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// 録画済み一覧の最大保持件数（0以下の場合は制限なし）
        /// </summary>
        public int RecListMaxCount { get; set; } = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ctrlCmdUtil">EDCB通信用ユーティリティ</param>
        public EpgDataManager(CtrlCmdUtil ctrlCmdUtil)
        {
            _ctrlCmdUtil = ctrlCmdUtil ?? throw new ArgumentNullException(nameof(ctrlCmdUtil));
        }

        /// <summary>
        /// EpgTimerSrv (EDCB) との通信接続チェックを行います。
        /// </summary>
        /// <param name="errCode">失敗時のエラーコード</param>
        /// <returns>接続成功の場合 true</returns>
        public bool CheckConnection(out ErrCode errCode)
        {
            var tunerReserveInfos = new List<TunerReserveInfo>();
            errCode = _ctrlCmdUtil.SendEnumTunerReserve(ref tunerReserveInfos);

            return errCode == ErrCode.CMD_SUCCESS;
        }

        /// <summary>
        /// EpgTimerSrvから番組一覧を取得し、serviceMap / allEventMapを構築します。
        /// </summary>
        /// <returns>番組情報が変更された場合はtrue、それ以外はfalse</returns>
        public bool UpdateServiceData()
        {
            // 番組一覧取得
            _serviceEvents.Clear();
            AllEventMap.Clear();
            _ctrlCmdUtil.SendEnumPgAll(ref _serviceEvents);

            // 番組一覧関連のハッシュを作成
            ServiceMap.Clear();
            foreach (EpgServiceEventInfo service in _serviceEvents)
            {
                // TSID + SID → サービス一覧(serviceが番組情報を保持している)
                string key = RockbarUtility.GetKey(service.serviceInfo.TSID, service.serviceInfo.SID);
                ServiceMap[key] = service;

                // TSID + SID + EventID → 番組情報
                foreach (EpgEventInfo ev in service.eventList)
                {
                    string evKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
                    AllEventMap[evKey] = ev;
                }
            }

            // 番組一覧の差分有無チェック
            // チェック対象：チャンネル数、前番組データ数
            string currentServiceHash = $"{ServiceEvents.Count}_{AllEventMap.Count}";
            bool isServiceChanged = (currentServiceHash != _prevServiceHash);
            _prevServiceHash = currentServiceHash;

            return isServiceChanged;
        }

        /// <summary>
        /// EpgTimerSrvから予約一覧を取得し、reserveMapを構築します。
        /// </summary>
        /// <returns>予約情報が変更された場合はtrue、それ以外はfalse</returns>
        public bool UpdateReserveData()
        {
            // 予約一覧取得
            _reserveDatas.Clear();
            _ctrlCmdUtil.SendEnumReserve(ref _reserveDatas);

            // 予約一覧関連のハッシュを作成
            ReserveMap.Clear();
            foreach (ReserveData reserveData in _reserveDatas)
            {
                // TSID + SID + EventID → 予約情報
                string evkey = RockbarUtility.GetKey(reserveData.TransportStreamID, reserveData.ServiceID, reserveData.EventID);
                ReserveMap[evkey] = reserveData;
            }

            // 予約一覧の差分有無チェック
            // チェック対象：予約数、予約ID、重複状態、録画モード(全サービス録画、無効など)
            string currentReserveHash = $"{_reserveDatas.Count}_" +
                string.Join(",", _reserveDatas.Select(r => $"{r.ReserveID}_{r.OverlapMode}_{r.RecSetting.RecMode}"));

            bool isReserveChanged = (currentReserveHash != _prevReserveHash);
            _prevReserveHash = currentReserveHash;

            return isReserveChanged;
        }

        /// <summary>
        /// EpgTimerSrvから録画済み情報を取得し、録画一覧を更新します。
        /// </summary>
        /// <returns>録画済み情報が変更された場合はtrue、それ以外はfalse</returns>
        public bool UpdateRecData()
        {
            _recFileInfos.Clear();
            RecMap.Clear();
            _ctrlCmdUtil.SendEnumRecInfoBasic(ref _recFileInfos);

            if (RecListMaxCount > 0 && RecFileInfos.Count > RecListMaxCount)
            {
                RecFileInfos = RecFileInfos.GetRange(RecFileInfos.Count - RecListMaxCount, RecListMaxCount);
            }
            RecFileInfos.Reverse();

            RecMap = RecFileInfos.ToDictionary(r => r.ID);

            // 更新時間として現在時刻を取得
            LastRecDataUpdateTime = DateTime.Now;

            // 録画済み情報の一覧の差分有無チェック
            // チェック対象：録画済みファイルの件数、各録画済みファイルの固有ID
            string currentRecHash = $"{RecFileInfos.Count}_" +
                string.Join(",", RecFileInfos.Select(r => r.ID));

            bool isRecChanged = (currentRecHash != _prevRecHash);
            _prevRecHash = currentRecHash;

            return isRecChanged;
        }

        /// <summary>
        /// EpgTimerSrvからチューナーごとの予約一覧を取得します。
        /// </summary>
        /// <returns>チューナー情報が変更された場合はtrue、それ以外はfalse</returns>
        public bool UpdateTunerData()
        {
            // チューナーごとの予約一覧取得
            _tunerReserveInfos.Clear();
            _ctrlCmdUtil.SendEnumTunerReserve(ref _tunerReserveInfos);

            // チューナーごとの予約一覧の差分有無チェック
            // チェック対象：チューナー台数、各チューナーの識別ID、各チューナーに割り当てられている予約の件数
            string currentTunerHash = $"{_tunerReserveInfos.Count}_" +
                string.Join(",", _tunerReserveInfos.Select(t => $"{t.tunerID}_{t.reserveList.Count}"));

            bool isTunerChanged = (currentTunerHash != _prevTunerHash);
            _prevTunerHash = currentTunerHash;

            return isTunerChanged;
        }
    }
}