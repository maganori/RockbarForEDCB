using Nett;
using RockbarForEDCB.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EpgTimer;

namespace RockbarForEDCB
{
    /// <summary>
    /// 設定およびサービス一覧ファイルの読み込み・保持・管理を行うクラス
    /// </summary>
    public class ConfigManager
    {
        // 設定ファイル名はハードコーディングとする
        private const string TomlConfigFileName = "RockbarForEDCB.toml";
        private const string TsvSelectedServiceFileName = "SelectedServices.tsv";
        private const string TsvFavoriteServiceFileName = "FavoriteServices.tsv";

        /// <summary>
        /// TOML設定
        /// </summary>
        public RockbarSetting RockbarSetting { get; private set; }

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
            RockbarSetting = new RockbarSetting();
            SelectedServiceList = new List<Service>();
            FavoriteServiceList = new List<Service>();
            LoadFromFile();
        }

        /// <summary>
        /// TOML設定ファイルおよびTSVサービスリストを読み込み。
        /// </summary>
        public void LoadFromFile()
        {
            // TOML設定ファイル（RockbarForEDCB.toml）の読み込み
            LoadRockbarSettingFromFile();

            // 選択サービス一覧（SelectedServices.tsv）の読み込み
            LoadSelectedServicesFromFile();

            // お気に入りサービス一覧（FavoriteServices.tsv）の読み込み
            LoadFavoriteServicesFromFile();
        }

        /// <summary>
        /// 現在保持している設定およびサービス一覧をそれぞれのファイルに保存します。
        /// </summary>
        public void SaveToFile()
        {
            // TOML設定ファイルの保存
            SaveRockbarSettingToFile();

            // 選択サービス一覧の保存
            SaveSelectedServicesToFile();

            // お気に入りサービス一覧の保存
            SaveFavoriteServicesToFile();
        }

        /// <summary>
        /// TOML設定ファイルから設定を読み込む。
        /// </summary>
        private void LoadRockbarSettingFromFile()
        {
            string tomlPath = GetFullPath(TomlConfigFileName);
            RockbarSetting = new RockbarSetting();

            // ファイルが存在しない場合はそのままデフォルト設定を使用
            if (File.Exists(tomlPath))
            {
                try
                {
                    // 全プロパティを型に直接一括マッピングして読み込む
                    RockbarSetting = Toml.ReadFile<RockbarSetting>(tomlPath);

                    // 名称が変更されたパラメータのバックワードコンパチ処理
                    TomlTable table = Toml.ReadFile(tomlPath);

                    // 20260823版:WebEpgUrl, 20260731版:WebEPGUrl
                    if (!table.ContainsKey("WebEpgUrl") && table.ContainsKey("WebEPGUrl"))
                    {
                        RockbarSetting.WebEpgUrl = table.Get<string>("WebEPGUrl");
                    }

                    // 20260912の次版:ListFont, 初版:Font
                    if (!table.ContainsKey("ListFont") && table.ContainsKey("Font"))
                    {
                        RockbarSetting.ListFont = table.Get<string>("Font");
                    }

                    // 20260912の次版:ListForeColor, 初版:ForeColor
                    if (!table.ContainsKey("ListForeColor") && table.ContainsKey("ForeColor"))
                    {
                        RockbarSetting.ListForeColor = table.Get<string>("ForeColor");
                    }
                }
                catch (Exception ex)
                {
                    // パースエラー（uintに負の値、型の不一致、構文エラー等）が発生した場合
                    string message = $"設定ファイル ({TomlConfigFileName}) の読み込み中にエラーが発生しました。\n\n" +
                                     $"【エラー詳細】\n{ex.Message}\n\n" +
                                     "デフォルトの設定で起動しますか？\n" +
                                     "(「いいえ」を選択するとアプリケーションを終了します)";

                    DialogResult result = MessageBox.Show(message, "設定ファイル読み込みエラー",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        // 起動中断（アプリケーションを終了）
                        Environment.Exit(1);
                        return;
                    }

                    // 「はい」が選ばれた場合はデフォルト設定を使用
                    RockbarSetting = new RockbarSetting();
                }
            }

            // 読み込み後の値の検証と補正（補正が行われた場合はダイアログで通知）
            if (RockbarSetting.ValidateAndFix())
            {
                string fixMessage = $"設定ファイル ({TomlConfigFileName}) 内に無効な数値（範囲外の値）が" +
                                    $"含まれていたため、安全なデフォルト値に補正しました。\n\n" +
                                   "このまま補正後の設定で起動しますか？\n" +
                                   "(「いいえ」を選択するとアプリケーションを終了します)";

                DialogResult result = MessageBox.Show(fixMessage, "設定値の自動補正警告",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    // 起動中断（アプリケーションを終了）
                    Environment.Exit(1);
                    return;
                }
            }
        }

        /// <summary>
        /// TSVファイルから選択サービスリストを読み込む
        /// </summary>
        /// <returns>選択サービスリスト</returns>
        private void LoadSelectedServicesFromFile()
        {
            SelectedServiceList = LoadServicesFromFile(GetFullPath(TsvSelectedServiceFileName)) ?? new List<Service>();
        }

        /// <summary>
        /// TSVファイルからお気に入りサービスリストを読み込む
        /// </summary>
        /// <returns>お気に入りサービスリスト</returns>
        private void LoadFavoriteServicesFromFile()
        {
            FavoriteServiceList = LoadServicesFromFile(GetFullPath(TsvFavoriteServiceFileName)) ?? new List<Service>();
        }

        /// <summary>
        /// TSVファイルからサービスリストを読み込んで返す
        /// </summary>
        /// <param name="fileName">TSVファイル名</param>
        /// <returns>サービスリスト</returns>
        private List<Service> LoadServicesFromFile(string fileName)
        {
            List<Service> result = new List<Service>();

            try
            {
                foreach (string line in File.ReadAllLines(fileName))
                {
                    string[] fields = line.Split('\t');
                    if (fields.Length < 2)
                    {
                        continue;
                    }

                    // 1列目（Tsid）の前後の空白除去 + 先頭のBOMを除去
                    string tsid = fields[0].Trim().TrimStart('\uFEFF');
                    string sid = fields[1].Trim();

                    // 形式: Tsid, Sid, Name, Type?, TvtestOption?
                    if (ushort.TryParse(tsid, out _) && ushort.TryParse(sid, out _))
                    {
                        Service service = new Service
                        {
                            Tsid = tsid,
                            Sid = sid,
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
        /// TOML設定ファイルに設定を書き込む
        /// </summary>
        private void SaveRockbarSettingToFile()
        {
            Toml.WriteFile(RockbarSetting, GetFullPath(TomlConfigFileName));
        }

        /// <summary>
        /// TSVファイルに選択サービスリストを書き込む
        /// </summary>
        private void SaveSelectedServicesToFile()
        {
            SaveServicesToFile(SelectedServiceList, GetFullPath(TsvSelectedServiceFileName));
        }

        /// <summary>
        /// TSVファイルにお気に入りサービスリストを書き込む
        /// </summary>
        private void SaveFavoriteServicesToFile()
        {
            SaveServicesToFile(FavoriteServiceList, GetFullPath(TsvFavoriteServiceFileName));
        }

        /// <summary>
        /// TSVファイルにサービスリストを書き込む
        /// </summary>
        /// <param name="services">サービスリスト</param>
        /// <param name="fileName">ファイル名</param>
        private void SaveServicesToFile(List<Service> services, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
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
        /// アプリケーションディレクトリとファイル名を連結して返却する
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>フルパス</returns>
        private string GetFullPath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }

    /// <summary>
    /// 設定ファイルクラス
    /// </summary>
    public class RockbarSetting
    {
        // 録画一覧の最大表示数のデフォルト値
        public const int DEFAULT_REC_LIST_MAX_COUNT = 1500;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RockbarSetting()
        {
            // デフォルト設定
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
            this.ListFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.MenuFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.MainFormFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.TabFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.TextBoxFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.ButtonFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);
            this.SettingFormFont = fontConverter.ConvertToString(SystemFonts.DefaultFont);

            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            this.FormBackColor = colorConverter.ConvertToString(Color.FromArgb(163, 216, 232));
            this.ListForeColor = colorConverter.ConvertToString(Color.FromArgb(25, 250, 140));
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
        }

        // デフォルト設定
        // stringは""、オブジェクトは空オブジェクトで初期化。

        // 起動時x座標
        public int X { get; set; } = 50;
        // 起動時y座標
        public int Y { get; set; } = 50;
        // 起動時画面幅
        public int Width { get; set; } = 650;
        // 起動時画面高
        public int Height { get; set; } = 300;
        // 起動時スプリッター位置
        public int SplitterDistance { get; set; } = 200;
        // TCP/IP使用
        public bool UseTcpIp { get; set; }
        // IPアドレス
        public string IpAddress { get; set; } = "127.0.0.1";
        // ポート番号
        public uint PortNumber { get; set; } = 4510;
        // Web Link使用
        public bool UseWebLink { get; set; } = true;
        // WebEPG URL
        public string WebEpgUrl { get; set; } = "http://localhost:5510/EMWUI/epg.html";
        // Web Link URL
        public string WebLinkUrl { get; set; } = "http://localhost:5510/EMWUI/epginfo.html?onid={ONID}&tsid={TSID}&sid={SID}&eid={EID}";
        // Web Link URL(録画結果)
        public string RecInfoWebLinkUrl { get; set; } = "http://localhost:5510/EMWUI/recinfodesc.html?id={RecID}";

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
        // 録画モード
        public byte RecMode { get; set; } = 1;
        // 優先モード(視聴登録優先)
        public bool PrioritizeView { get; set; }
        // 録画優先度
        public byte RecPriority { get; set; } = 2;
        // 録画追従の要否
        public bool RecTuijyuu { get; set; } = true;
        // 録画ServiceMode
        public uint RecServiceMode { get; set; } = 48;
        // ぴったり録画の要否
        public bool RecPittari { get; set; }
        // 録画後実行bat*録画タグ
        public string RecBatFilePath { get; set; } = "";
        // SuspendMode
        public byte SuspendModeAfterRec { get; set; } = 0;
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
        public uint RecTunerID { get; set; } = 0;
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
        public string TvtestPath { get; set; } = "";
        // TVTest 地デジオプション
        public string TvtestDttvOption { get; set; } = "";
        // TVTest BS/CSオプション
        public string TvtestBscsOption { get; set; } = "";
        // TVTest CATVオプション
        public string TvtestCatvOption { get; set; } = "";
        // TVTest SPHDオプション
        public string TvtestSphdOption { get; set; } = "";
        // TVTest BS4Kオプション
        public string TvtestBs4kOption { get; set; } = "";

        // TVTest TS再生オプション
        public string TvtestTsFileOption { get; set; } = "";
        // TVTestダブルクリック起動使用
        public bool UseDoubleClickTvtest { get; set; }
        // TVTest自動起動使用
        public bool IsAutoOpenTvtest { get; set; }
        // TVTest自動起動(地デジ)
        public bool IsAutoOpenTvtestDttv { get; set; } = true;
        // TVTest自動起動(BS)
        public bool IsAutoOpenTvtestBs { get; set; } = true;
        // TVTest自動起動(CS)
        public bool IsAutoOpenTvtestCs { get; set; } = true;
        // TVTest自動起動(CATV)
        public bool IsAutoOpenTvtestCatv { get; set; } = true;
        // TVTest自動起動(BS4K)
        public bool IsAutoOpenTvtestBs4k { get; set; } = true;
        // TVTest自動起動(SPHD)
        public bool IsAutoOpenTvtestSphd { get; set; } = true;
        // TVTest自動起動(お気に入りサービス)
        public bool IsAutoOpenTvtestFavoriteService { get; set; }
        // TVTest自動起動開始マージン
        public uint AutoOpenMargin { get; set; } = 15;
        // TVTest自動起動終了マージン
        public uint AutoCloseMargin { get; set; } = 5;

        // タスクトレイアイコン常時表示
        public bool ShowTaskTrayIcon { get; set; } = true;
        // ×ボタンでタスクトレイに格納
        public bool StoreTaskTrayByClosing { get; set; }
        // タスクトレイアイコンクリック時表示・非表示切り替え
        public bool ToggleVisibleTaskTrayIconClick { get; set; }
        // タスクトレイアイコン左クリック時動作
        public string TaskTrayIconLeftClick { get; set; } = "Rockバー表示";
        // タスクトレイアイコン左ダブルクリック時動作
        public string TaskTrayIconLeftDoubleClick { get; set; } = "";
        // タスクトレイアイコン右クリック時動作
        public string TaskTrayIconRightClick { get; set; } = "メニュー";
        // タスクトレイアイコン右ダブルクリック時動作
        public string TaskTrayIconRightDoubleClick { get; set; } = "";
        // 水平分割
        public bool IsHorizontalSplit { get; set; } = true;
        // 予約を無効にするとき、録画モードを「指定サービス」にする(EpgTimerSrvと同じ)
        public bool FixNoRecToServiceOnly { get; set; }
        // 録画一覧の最大表示数
        public int RecListMaxCount { get; set; } = DEFAULT_REC_LIST_MAX_COUNT;
        // リストフォント(シリアライズしたもの)
        public string ListFont { get; set; } = "";
        // フォーム背景色(シリアライズしたもの)
        public string FormBackColor { get; set; } = "";
        // リスト文字色(シリアライズしたもの)
        public string ListForeColor { get; set; } = "";
        // リスト背景色(シリアライズしたもの)
        public string ListBackColor { get; set; } = "";
        // 予約リスト背景色(シリアライズしたもの)
        public string OkReserveListBackColor { get; set; } = "";
        // 部分予約リスト背景色(シリアライズしたもの)
        public string PartialReserveListBackColor { get; set; } = "";
        // 予約不可リスト背景色(シリアライズしたもの)
        public string NgReserveListBackColor { get; set; } = "";
        // 無効予約リスト背景色(シリアライズしたもの)
        public string DisabledReserveListBackColor { get; set; } = "";
        // リストヘッダ文字色(シリアライズしたもの)
        public string ListHeaderForeColor { get; set; } = "";
        // リストヘッダ背景色(シリアライズしたもの)
        public string ListHeaderBackColor { get; set; } = "";
        // メニューフォント(シリアライズしたもの)
        public string MenuFont { get; set; } = "";
        // メニュー背景色(シリアライズしたもの)
        public string MenuBackColor { get; set; } = "";
        // 予約メニュー背景色(シリアライズしたもの)
        public string OkReserveMenuBackColor { get; set; } = "";
        // 部分予約メニュー背景色(シリアライズしたもの)
        public string PartialReserveMenuBackColor { get; set; } = "";
        // 予約不可メニュー背景色(シリアライズしたもの)
        public string NgReserveMenuBackColor { get; set; } = "";
        // 無効予約メニュー背景色(シリアライズしたもの)
        public string DisabledReserveMenuBackColor { get; set; } = "";
        // メイン画面フォント(シリアライズしたもの)
        public string MainFormFont { get; set; } = "";
        // メイン画面フォントをメイン画面スケーリングに使用する
        public bool UseMainFormFontForScaling { get; set; } = true;
        // メイン画面のフォントを個別に指定する
        public bool UseIndividualMainFormFonts { get; set; }
        // タブのフォント(シリアライズしたもの)
        public string TabFont { get; set; } = "";
        // テキストボックスのフォント(シリアライズしたもの)
        public string TextBoxFont { get; set; } = "";
        // ボタンのフォント(シリアライズしたもの)
        public string ButtonFont { get; set; } = "";
        // 設定画面フォント(シリアライズしたもの)
        public string SettingFormFont { get; set; } = "";
        // BonDriver名→チューナー名マッピング
        public Dictionary<string, string> BonDriverNameToTunerName { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 設定値の妥当性をチェックし、異常値があれば修復する。
        /// 修復が発生した場合は true を返す。
        /// </summary>
        public bool ValidateAndFix()
        {
            bool isFixed = false;

            // 起動時座標、幅、高さ、スプリッター位置
            if (this.X <= -this.Width + 30 || this.Y < 0 || this.Width <= 0 || this.Height <= 0 || this.SplitterDistance <= 0)
            {
                this.X = 50;
                this.Y = 50;
                this.Width = 650;
                this.Height = 300;
                this.SplitterDistance = 200;
                //isFixed = true;　//自然に発生する可能性があるためコメントアウト
            }

            // ポート番号
            if (this.PortNumber < 1 || this.PortNumber > 65535)
            {
                this.PortNumber = 4510;
                isFixed = true;
            }

            // 録画モード
            if (this.RecMode < 0 || this.RecMode > 3)
            {
                this.RecMode = 1;
                isFixed = true;
            }

            // 録画優先度
            if (this.RecPriority < 1 || this.RecPriority > 5)
            {
                this.RecPriority = 2;
                isFixed = true;
            }

            // uint RecServiceMode は現状問題なさそうなためチェック不要

            // SuspendMode
            if (this.SuspendModeAfterRec < 0 || this.SuspendModeAfterRec > 4)
            {
                this.SuspendModeAfterRec = 0;
                isFixed = true;
            }

            // int StartRecMargin は現状問題なさそうなためチェック不要
            // int EndRecMargin は現状問題なさそうなためチェック不要
            // uint RecTunerID は現状問題なさそうなためチェック不要


            // AutoOpenMargin
            if (this.AutoOpenMargin < 0 || this.AutoOpenMargin > 59)
            {
                this.AutoOpenMargin = 15;
                isFixed = true;
            }

            // AutoCloseMargin
            if (this.AutoCloseMargin < 0 || this.AutoCloseMargin > 59)
            {
                this.AutoCloseMargin = 5;
                isFixed = true;
            }

            // 録画一覧の最大表示数
            if (this.RecListMaxCount < 0)
            {
                this.RecListMaxCount = DEFAULT_REC_LIST_MAX_COUNT;
                isFixed = true;
            }

            return isFixed;
        }

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