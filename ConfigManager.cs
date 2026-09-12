using Nett;
using RockbarForEDCB.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using EpgTimer;

namespace RockbarForEDCB
{
    /// <summary>
    /// 設定およびサービス一覧ファイルの読み込み・保持・管理を行うクラス
    /// </summary>
    public class ConfigManager
    {
        // 設定ファイル名はハードコーディングとする
        public static string TOML_CONFIG_FILENAME = "RockbarForEDCB.toml";
        public static string TSV_SELECTED_SERVICE_FILENAME = "SelectedServices.tsv";
        public static string TSV_FAVORITE_SERVICE_FILENAME = "FavoriteServices.tsv";

        /// <summary>
        /// TOML設定
        /// </summary>
        public RockBarSetting RockbarSetting { get; private set; }

        /// <summary>
        /// 選択サービスリスト
        /// </summary>
        public List<Service> SelectedServiceList { get; private set; }

        /// <summary>
        /// お気に入りサービスリスト
        /// </summary>
        public List<Service> FavoriteServiceList { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConfigManager()
        {
            RockbarSetting = new RockBarSetting();
            SelectedServiceList = new List<Service>();
            FavoriteServiceList = new List<Service>();
            LoadFromFile();
        }

        /// <summary>
        /// TOML設定ファイルおよびTSVサービスリストを読み込み。
        /// </summary>
        public void LoadFromFile()
        {
            // TOML設定ファイルの読み込み
            try
            {
                RockbarSetting = Toml.ReadFile<RockBarSetting>(GetTomlSettingFilePath());
            }
            catch (FileNotFoundException)
            {
                // TOML設定ファイルが存在しない場合はデフォルト設定を使用
                RockbarSetting = new RockBarSetting();
            }

            // 選択サービス一覧（SelectedServices.tsv）の読み込み
            SelectedServiceList = LoadSelectedServicesFromFile() ?? new List<Service>();

            // お気に入りサービス一覧（FavoriteServices.tsv）の読み込み
            FavoriteServiceList = LoadFavoriteServicesFromFile() ?? new List<Service>();
        }

        /// <summary>
        /// 現在保持している設定およびサービス一覧をそれぞれのファイルに保存します。
        /// </summary>
        public void SaveFromFile()
        {
            // TOML設定ファイルの保存
            if (RockbarSetting != null)
            {
                Toml.WriteFile(RockbarSetting, GetTomlSettingFilePath());
            }

            // 選択サービス一覧の保存
            if (SelectedServiceList != null)
            {
                SaveSelectedServicesToFile(SelectedServiceList);
            }

            // お気に入りサービス一覧の保存
            if (FavoriteServiceList != null)
            {
                SaveFavoriteServicesToFile(FavoriteServiceList);
            }
        }

        /// <summary>
        /// TSVファイルからサービスリストを読み込んで返す
        /// </summary>
        /// <param name="filename">TSVファイル名</param>
        /// <returns>サービスリスト</returns>
        private List<Service> LoadServicesFromFile(string filename)
        {
            List<Service> result = new List<Service>();

            try
            {
                foreach (string line in File.ReadAllLines(filename))
                {
                    string[] fields = line.Split('\t');
                    if (fields.Length == 0)
                    {
                        continue;
                    }

                    // 前後の空白除去 + BOM 除去
                    string first = fields[0]?.Trim().TrimStart('\uFEFF');

                    // 形式: Tsid, Sid, Name, Type?, TvtestOption?
                    if (fields.Length >= 2 &&
                        ushort.TryParse(fields[0], out _) &&
                        ushort.TryParse(fields[1], out _))
                    {
                        Service service = new Service
                        {
                            Tsid = fields[0].Trim(),
                            Sid = fields[1].Trim(),
                            Name = fields.Length > 2 && !string.IsNullOrWhiteSpace(fields[2]) ? fields[2].Trim() : null,
                            TypeName = fields.Length > 3 && !string.IsNullOrWhiteSpace(fields[3]) ? fields[3].Trim() : null,
                            TvtestOption = fields.Length > 4 && !string.IsNullOrWhiteSpace(fields[4]) ? fields[4].Trim() : null
                        };

                        result.Add(service);
                    }
                }
            }
            catch
            {
                // TSVファイルがない場合フォーマットエラーの場合、空リストを返す
            }

            return result;
        }

        /// <summary>
        /// TSVファイルから選択サービスリストを読み込んで返す
        /// </summary>
        /// <returns>選択サービスリスト</returns>
        private List<Service> LoadSelectedServicesFromFile()
        {
            return LoadServicesFromFile(GetTsvSelectedServicesFilePath());
        }

        /// <summary>
        /// TSVファイルからお気に入りサービスリストを読み込んで返す
        /// </summary>
        /// <returns>お気に入りサービスリスト</returns>
        private List<Service> LoadFavoriteServicesFromFile()
        {
            return LoadServicesFromFile(GetTsvFavoriteServicesFilePath());
        }

        /// <summary>
        /// TSVファイルにサービスリストを書き込む
        /// </summary>
        /// <param name="services">サービスリスト</param>
        /// <param name="filename">ファイル名</param>
        private void SaveServicesToFile(List<Service> services, string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                writer.WriteLine("Tsid\tSid\tName\tType\tTVTestOption");

                foreach (Service service in services)
                {
                    List<string> fields = new List<string>
                    {
                        service.Tsid ?? "",
                        service.Sid ?? "",
                        service.Name ?? "",
                        service.TypeName ?? "",
                        service.TvtestOption ?? ""
                    };

                    int lastIndex = fields.Count - 1;
                    while (lastIndex >= 2 && string.IsNullOrEmpty(fields[lastIndex]))
                    {
                        lastIndex--;
                    }

                    writer.WriteLine(string.Join("\t", fields.Take(lastIndex + 1)));
                }
            }
        }

        /// <summary>
        /// TSVファイルに選択サービスリストを書き込む
        /// </summary>
        /// <param name="services">選択サービスリスト</param>
        private void SaveSelectedServicesToFile(List<Service> services)
        {
            SaveServicesToFile(services, GetTsvSelectedServicesFilePath());
        }

        /// <summary>
        /// TSVファイルにお気に入りサービスリストを書き込む
        /// </summary>
        /// <param name="services">お気に入りサービスリスト</param>
        private void SaveFavoriteServicesToFile(List<Service> services)
        {
            SaveServicesToFile(services, GetTsvFavoriteServicesFilePath());
        }

        /// <summary>
        /// TOML設定ファイルのフルパスを返す
        /// </summary>
        /// <returns>TOML設定ファイルのフルパス</returns>
        private string GetTomlSettingFilePath()
        {
            // フルパスを返す
            return GetFullPath(TOML_CONFIG_FILENAME);
        }

        /// <summary>
        /// TSV選択サービス設定ファイルのフルパスを返す
        /// </summary>
        /// <returns>TSV選択サービス設定ファイルのフルパス</returns>
        private string GetTsvSelectedServicesFilePath()
        {
            // フルパスを返す
            return GetFullPath(TSV_SELECTED_SERVICE_FILENAME);
        }

        /// <summary>
        /// TSVお気に入りサービス設定ファイルのフルパスを返す
        /// </summary>
        /// <returns>TSVお気に入りサービス設定ファイルのフルパス</returns>
        private string GetTsvFavoriteServicesFilePath()
        {
            // フルパスを返す
            return GetFullPath(TSV_FAVORITE_SERVICE_FILENAME);
        }

        /// <summary>
        /// アプリケーションディレクトリとファイル名を連結して返却する
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>フルパス</returns>
        private string GetFullPath(string filename)
        {
            // 実行ファイルのフルパスを取得
            string appFilePath = System.Reflection.Assembly.GetEntryAssembly().Location;

            // ディレクトリと連結して、設定ファイルのフルパスを返す
            return Path.Combine(Path.GetDirectoryName(appFilePath), filename);
        }
    }

    /// <summary>
    /// 設定ファイルクラス
    /// </summary>
    public class RockBarSetting
    {
        public RockBarSetting()
        {
            // デフォルト設定
            // 数値系は0初期化・bool系はfalse初期化・stringはnull初期化。設定が必要な箇所だけ設定する

            this.PortNumber = 4510;
            this.UseWebLink = true;
            this.WebEpgUrl = "http://localhost:5510/EMWUI/epg.html";
            this.WebLinkUrl = "http://localhost:5510/EMWUI/epginfo.html?onid={ONID}&tsid={TSID}&sid={SID}&eid={EID}";
            this.RecInfoWebLinkUrl = "http://localhost:5510/EMWUI/recinfodesc.html?id={RecID}";
            this.AutoOpenMargin = 15;
            this.AutoCloseMargin = 5;
            this.ShowTaskTrayIcon = true;
            this.RecListMaxCount = DEFAULT_REC_LIST_MAX_COUNT;
            this.TaskTrayIconLeftClick = "Rockバー表示";
            this.TaskTrayIconLeftDoubleClick = "";
            this.TaskTrayIconRightClick = "メニュー";
            this.TaskTrayIconRightDoubleClick = "";

            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
            this.Font = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.MenuFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.TabFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.ButtonFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.LabelFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.TextBoxFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);

            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            this.ForeColor = colorConverter.ConvertToString(Color.FromArgb(25, 250, 140));
            this.FormBackColor = colorConverter.ConvertToString(Color.FromArgb(163, 216, 232));
            this.ListBackColor = colorConverter.ConvertToString(Color.FromArgb(40, 40, 40));
            this.OkReserveListBackColor = colorConverter.ConvertToString(Color.DarkSlateGray);
            this.PartialReserveListBackColor = colorConverter.ConvertToString(Color.FromArgb(160, 160, 0));
            this.NgReserveListBackColor = colorConverter.ConvertToString(Color.Red);
            this.DisabledReserveListBackColor = colorConverter.ConvertToString(Color.FromArgb(96, 96, 96));
            this.ListHeaderForeColor = this.ListBackColor;
            this.ListHeaderBackColor = colorConverter.ConvertToString(Color.FromArgb(20, 163, 90));
            this.MenuBackColor = colorConverter.ConvertToString(SystemColors.ControlLight);
            this.OkReserveMenuBackColor = colorConverter.ConvertToString(Color.FromArgb(192, 192, 225));
            this.PartialReserveMenuBackColor = colorConverter.ConvertToString(Color.Yellow);
            this.NgReserveMenuBackColor = colorConverter.ConvertToString(Color.Red);
            this.DisabledReserveMenuBackColor = colorConverter.ConvertToString(Color.DarkGray);

            BonDriverNameToTunerName = new Dictionary<string, string>();
        }

        // 録画一覧の最大表示数のデフォルト値
        public const int DEFAULT_REC_LIST_MAX_COUNT = 1500;

        // 起動時x座標
        public int X { get; set; }
        // 起動時y座標
        public int Y { get; set; }
        // 起動時画面幅
        public int Width { get; set; }
        // 起動時画面高
        public int Height { get; set; }
        // 起動時スプリッター位置
        public int SplitterDistance { get; set; }
        // TCP/IP使用
        public bool UseTcpIp { get; set; }
        // IPアドレス
        public string IpAddress { get; set; }
        // ポート番号
        public uint PortNumber { get; set; }
        // Web Link使用
        public bool UseWebLink { get; set; }
        // WebEPG URL
        public string WebEpgUrl { get; set; }
        // Web Link URL
        public string WebLinkUrl { get; set; }
        // Web Link URL(録画結果)
        public string RecInfoWebLinkUrl { get; set; }

        // Rockbarによる予約追加機能
        public bool UseRockbarReserveAdd { get; set; }
        // Rockbarによる予約変更機能
        public bool UseRockbarReserveMod { get; set; }
        // Rockbarによる予約・録画情報削除機能
        public bool UseRockbarReserveDel { get; set; }
        // Rockbarによる予約・録画情報削除前確認表示
        public bool UseRockbarReserveDelConfirm { get; set; } = true;
        // 予約の有効フラグ
        public bool EnableReserve { get; set; } = true;
        // 優先モード(視聴登録優先)
        public bool PrioritizeView { get; set; } = false;
        // 録画モード
        public byte RecMode { get; set; } = 1;
        // 録画優先度
        public byte RecPriority { get; set; } = 2;
        // 録画追従の要否
        public bool RecTuijyuu { get; set; } = true;
        // 録画ServiceMode
        public uint RecServiceMode { get; set; } = 48;
        // ぴったり録画の要否
        public bool RecPittari { get; set; }
        // 録画後実行bat
        public string RecBatFilePath { get; set; }
        // 録画タグ
        public string RecTag { get; set; }
        // SuspendMode
        public byte SuspendModeAfterRec { get; set; }
        // 復帰後再起動する
        public bool RebootAfterReturn { get; set; }
        // カスタマイズの録画マージンを使用
        public bool UseCustomRecMargin { get; set; }
        // 録画開始マージン
        public int StartRecMargin { get; set; } = 5;
        // 録画終了マージン
        public int EndRecMargin { get; set; } = 5;
        // 後ろの予約を同一ファイルで出力する
        public bool ContinueRecSameFile { get; set; }
        // 部分受信(ワンセグ)を別ファイルに同時出力する
        public bool PartialRecSeparateFile { get; set; }
        // 使用チューナー強制指定
        public uint RecTunerID { get; set; }
        // 録画コメント
        public string RecComment { get; set; } = "RockbarForEDCB予約";

        // 録画保存先フォルダのパス一覧
        // 部分録画保存先フォルダのパス一覧
        // --- アプリ内部用([TomlIgnore] を付与して TOML 出力から除外) ---
        [TomlIgnore]
        public List<RecFileSetInfo> RecFolderList
        {
            get => RecFolderListDto?.Select(x => x.ToEntity()).ToList() ?? new List<RecFileSetInfo>();
            set => RecFolderListDto = value?.Select(x => new RecFileSetInfoDto(x)).ToList() ?? new List<RecFileSetInfoDto>();
        }

        [TomlIgnore]
        public List<RecFileSetInfo> PartialRecFolderList
        {
            get => PartialRecFolderListDto?.Select(x => x.ToEntity()).ToList() ?? new List<RecFileSetInfo>();
            set => PartialRecFolderListDto = value?.Select(x => new RecFileSetInfoDto(x)).ToList() ?? new List<RecFileSetInfoDto>();
        }

        // --- TOML保存用 (DTO型でTOMLに出力・読み込みする) ---
        [TomlMember(Key = "RecFolderList")]
        public List<RecFileSetInfoDto> RecFolderListDto { get; set; } = new List<RecFileSetInfoDto>();

        [TomlMember(Key = "PartialRecFolderList")]
        public List<RecFileSetInfoDto> PartialRecFolderListDto { get; set; } = new List<RecFileSetInfoDto>();


        // TVTest.exeパス
        public string TvtestPath { get; set; }
        // TVTest 地デジオプション
        public string TvtestDttvOption { get; set; }
        // TVTest BS/CSオプション
        public string TvtestBscsOption { get; set; }
        // TVTest CATVオプション
        public string TvtestCatvOption { get; set; }
        // TVTest SPHDオプション
        public string TvtestSphdOption { get; set; }
        // TVTest BS4Kオプション
        public string TvtestBs4kOption { get; set; }

        // TVTest TS再生オプション
        public string TvtestTsFileOption { get; set; }
        // TVTestダブルクリック起動使用
        public bool UseDoubleClickTvtest { get; set; }
        // TVTest自動起動使用
        public bool IsAutoOpenTvtest { get; set; }
        // TVTest自動起動(地デジ)
        public bool IsAutoOpenTvtestDttv { get; set; }
        // TVTest自動起動(BS)
        public bool IsAutoOpenTvtestBs { get; set; }
        // TVTest自動起動(CS)
        public bool IsAutoOpenTvtestCs { get; set; }
        // TVTest自動起動(CATV)
        public bool IsAutoOpenTvtestCatv { get; set; }
        // TVTest自動起動(BS4K)
        public bool IsAutoOpenTvtestBs4k { get; set; }
        // TVTest自動起動(SPHD)
        public bool IsAutoOpenTvtestSphd { get; set; }
        // TVTest自動起動(お気に入りサービス)
        public bool IsAutoOpenTvtestFavoriteService { get; set; }
        // TVTest自動起動開始マージン
        public uint AutoOpenMargin { get; set; }
        // TVTest自動起動終了マージン
        public uint AutoCloseMargin { get; set; }
        // タスクトレイアイコン常時表示
        public bool ShowTaskTrayIcon { get; set; }
        // ×ボタンでタスクトレイに格納
        public bool StoreTaskTrayByClosing { get; set; }
        // タスクトレイアイコンクリック時表示・非表示切り替え
        public bool ToggleVisibleTaskTrayIconClick { get; set; }
        // タスクトレイアイコン左クリック時動作
        public string TaskTrayIconLeftClick { get; set; }
        // タスクトレイアイコン左ダブルクリック時動作
        public string TaskTrayIconLeftDoubleClick { get; set; }
        // タスクトレイアイコン右クリック時動作
        public string TaskTrayIconRightClick { get; set; }
        // タスクトレイアイコン右ダブルクリック時動作
        public string TaskTrayIconRightDoubleClick { get; set; }
        // 水平分割
        public bool IsHorizontalSplit { get; set; }
        // 予約を無効にするとき、録画モードを「指定サービス」にする(EpgTimerSrvと同じ)
        public bool FixNoRecToServiceOnly { get; set; }
        // 録画一覧の最大表示数
        public int RecListMaxCount { get; set; }
        // フォント(シリアライズしたもの)
        public string Font { get; set; }
        // フォーム背景色(シリアライズしたもの)
        public string FormBackColor { get; set; }
        // 文字色(シリアライズしたもの)
        public string ForeColor { get; set; }
        // リスト背景色(シリアライズしたもの)
        public string ListBackColor { get; set; }
        // 予約リスト背景色(シリアライズしたもの)
        public string OkReserveListBackColor { get; set; }
        // 部分予約リスト背景色(シリアライズしたもの)
        public string PartialReserveListBackColor { get; set; }
        // 予約不可リスト背景色(シリアライズしたもの)
        public string NgReserveListBackColor { get; set; }
        // 無効予約リスト背景色(シリアライズしたもの)
        public string DisabledReserveListBackColor { get; set; }
        // リストヘッダ文字色(シリアライズしたもの)
        public string ListHeaderForeColor { get; set; }
        // リストヘッダ背景色(シリアライズしたもの)
        public string ListHeaderBackColor { get; set; }
        // メニューフォント(シリアライズしたもの)
        public string MenuFont { get; set; }
        // メニュー背景色(シリアライズしたもの)
        public string MenuBackColor { get; set; }
        // 予約メニュー背景色(シリアライズしたもの)
        public string OkReserveMenuBackColor { get; set; }
        // 部分予約メニュー背景色(シリアライズしたもの)
        public string PartialReserveMenuBackColor { get; set; }
        // 予約不可メニュー背景色(シリアライズしたもの)
        public string NgReserveMenuBackColor { get; set; }
        // 無効予約メニュー背景色(シリアライズしたもの)
        public string DisabledReserveMenuBackColor { get; set; }
        // タブのフォント(シリアライズしたもの)
        public string TabFont { get; set; }
        // ボタンのフォント(シリアライズしたもの)
        public string ButtonFont { get; set; }
        // ラベルのフォント(シリアライズしたもの)
        public string LabelFont { get; set; }
        // テキストボックスのフォント(シリアライズしたもの)
        public string TextBoxFont { get; set; }
        // BonDriver名→チューナー名マッピング
        public Dictionary<string, string> BonDriverNameToTunerName { get; set; }

        /// <summary>
        /// RecFileSetInfo の TOML シリアライズ用 DTO クラス
        /// </summary>
        public class RecFileSetInfoDto
        {
            public string RecFolder { get; set; } = "";
            public string WritePlugIn { get; set; } = "";
            public string RecNamePlugIn { get; set; } = "";

            public RecFileSetInfoDto() { }

            public RecFileSetInfoDto(RecFileSetInfo src)
            {
                if (src == null) return;
                this.RecFolder = src.RecFolder;
                this.WritePlugIn = src.WritePlugIn;
                this.RecNamePlugIn = src.RecNamePlugIn;
            }

            public RecFileSetInfo ToEntity()
            {
                return new RecFileSetInfo
                {
                    RecFolder = this.RecFolder,
                    WritePlugIn = this.WritePlugIn,
                    RecNamePlugIn = this.RecNamePlugIn,
                    RecFileName = ""
                };
            }
        }
    }
}