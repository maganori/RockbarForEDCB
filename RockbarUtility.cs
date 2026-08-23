using CsvHelper.Configuration;
using EpgTimer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RockbarForEDCB
{
    /// <summary>
    /// ネットワーク種別列挙型
    /// </summary>
    public enum NetworkType
    {
        // 地デジ(Digital Terrestrial Television)
        DTTV,
        // BS (Broadcasting Satellite)
        BS,
        // CS (Communication Satellite)
        CS,
        // SKY Perfect(advanced narrow-band CS digital broadcasting)
        SPHD,
        // BS4K (Broadcasting Satellite 4K)
        BS4K,
    };

    /// <summary>
    /// 予約ステータス列挙子
    /// </summary>
    public enum ReserveStatus
    {
        // 予約なし
        NONE,
        // 正常予約
        OK,
        // 部分予約
        PARTIAL,
        // 予約不可(チューナー不足)
        NG,
        // 番組消失
        DISAPPEARED,
        // 予約無効
        DISABLED,
    };

    /// <summary>
    /// タブの種別を表すEnumを定義
    /// </summary>
    public enum MainFormTabType
    {
        All,
        DTTV,
        BS,
        CS,
        Favorite,
        NewProgram,
        Reserve,
        Rec,
    }

    /// <summary>
    /// サービスクラス
    /// </summary>
    public class Service
    {
        public string Tsid { get; set; }
        public string Sid { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string TvtestOption { get; set; }
    }

    /// <summary>
    /// 予約詳細テキスト構造体
    /// </summary>
    public struct RecInfoDetailText
    {
        public string Value { get; set; }
        public string CopyText { get; set; }
    }

    /// <summary>
    /// ユーティリティクラス
    /// </summary>
    class RockbarUtility
    {
        private const string DELIMITER = "-";

        /// <summary>
        /// 文字列分割処理
        /// もとからある改行は残し、lineCount文字以上ある行に改行を挿入する
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <param name="lineCount">1行の文字数</param>
        /// <returns>文字列リスト(行毎)</returns>
        public static List<string> BreakString(string input, int lineCount = 40)
        {
            if (input == null)
            {
                return null;
            }

            input = input.TrimEnd();

            List<string> results = new List<string>();

            string[] lines = input.Split('\n');
            foreach (string line in lines)
            {
                string temp = line.Trim();

                while (temp.Length > lineCount)
                {
                    results.Add(temp.Substring(0, lineCount));
                    temp = temp.Substring(lineCount);
                }

                results.Add(temp);
            }

            return results;
        }

        /// <summary>
        /// ONIDからネットワーク種別(BS/CS/地デジ)を返す
        /// </summary>
        /// <param name="originalNetworkId">ONID</param>
        /// <returns>ネットワーク種別</returns>
        public static NetworkType GetNetworkType(int originalNetworkId)
        {
            // ref: https://github.com/tsukumijima/KonomiTV/blob/master/server/app/utils/TSInformation.py
            // 以下は ARIB STD-B10 第2部 付録N より抜粋
            // ref: https://web.archive.org/web/20140427183421/http://www.arib.or.jp/english/html/overview/doc/2-STD-B10v5_3.pdf#page=256
            // ref: https://www.arib.or.jp/english/html/overview/doc/6-STD-B10v5_13-E1.pdf#page=273
            // ref: https://www.arib.or.jp/english/html/overview/doc/6-STD-B10v5_13-E1.pdf#page=274

            switch (originalNetworkId)
            {
                case 0x0004: // BSデジタル放送: 0x0004
                    return NetworkType.BS;
                case 0x0006: // CS1: 0x0006 (旧プラット・ワン系)
                    return NetworkType.CS;
                case 0x0007: // CS2: 0x0007 (旧スカイパーフェクTV!2系)
                    return NetworkType.CS;
                case 0x000A: // SPHD: 0x000A (スカパー！プレミアムサービス)
                    return NetworkType.SPHD;
                case 0x000B: // 高度BSデジタル放送: 0x000B (BS4K)
                    return NetworkType.BS4K;
                default:
                    // 地上デジタルテレビジョン放送: 0x7880 - 0x7FE8

                    // ケーブルテレビ (リマックス方式・トランスモジュレーション方式)
                    // ケーブルテレビ独自のチャンネルのみで、地上波・BS の再送信は含まない
                    // デジタル放送リマックス: 0xFFFE (HD・SD チャンネル (MPEG-2))
                    // デジタル放送高度リマックス: 0xFFFA (ケーブル4Kチャンネル (H.264, H.265))
                    // JC-HITSトランスモジュレーション: 0xFFFD (HD・SD チャンネル (MPEG-2))
                    // 高度JC-HITSトランスモジュレーション: 0xFFF9 (ケーブル4Kチャンネル (H.264, H.265))
                    // 高度ケーブル自主放送: 0xFFF7 (ケーブル4Kチャンネル (H.264, H.265))

                    // 高度110度CSデジタル放送: 0x000C (CS4K: 運用終了)

                    // 124/128度CSデジタル放送
                    // SPSD-PerfecTV: 0x0001 (スターデジオ: 運用終了)
                    // SPSD-SKY: 0x0003 (運用終了)

                    return NetworkType.DTTV;
            }
        }

        /// <summary>
        /// ネットワーク種別指定を優先し、未指定時のみONIDからネットワーク種別を返す
        /// </summary>
        /// <param name="networkTypeText">ネットワーク種別</param>
        /// <param name="originalNetworkId">ONID</param>
        /// <returns>放送種別</returns>
        public static NetworkType GetNetworkType(string networkTypeText, int? originalNetworkId)
        {
            if (!string.IsNullOrWhiteSpace(networkTypeText))
            {
                string normalizedType = networkTypeText.Trim().ToUpperInvariant();

                if (normalizedType == "BS")
                {
                    return NetworkType.BS;
                }

                else if (normalizedType == "CS")
                {
                    return NetworkType.CS;
                }

                else if (normalizedType == "SPHD")
                {
                    return NetworkType.SPHD;
                }

                else if (normalizedType == "BS4K")
                {
                    return NetworkType.BS4K;
                }

                else if (normalizedType == "地")
                {
                    return NetworkType.DTTV;
                }
            }

            if (originalNetworkId.HasValue)
            {
                return GetNetworkType(originalNetworkId.Value);
            }

            return NetworkType.DTTV;
        }

        /// <summary>
        /// NetworkTypeからNetworkType名文字列(短)を返す
        /// </summary>
        /// <param name="networkType">NetworkType</param>
        /// <returns>NetworkType名文字列(短)</returns>
        public static string GetShortNetworkTypeName(NetworkType networkType)
        {
            switch (networkType)
            {
                case NetworkType.DTTV:
                    return "地";
                case NetworkType.BS:
                    return "BS";
                case NetworkType.CS:
                    return "CS";
                case NetworkType.SPHD:
                    return "SPHD";
                case NetworkType.BS4K:
                    return "BS4K";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 予約ステータスから予約ステータス識別文字列を返す
        /// </summary>
        /// <param name="reserveStatus">予約ステータス</param>
        /// <returns>予約ステータス識別子</returns>
        public static string GetReserveStatusString(ReserveStatus reserveStatus)
        {
            switch (reserveStatus)
            {
                case ReserveStatus.NONE:
                    return "　";
                case ReserveStatus.OK:
                    return "◎";
                case ReserveStatus.PARTIAL:
                    return "欠";
                case ReserveStatus.NG:
                    return "×";
                case ReserveStatus.DISAPPEARED:
                    return "消";
                case ReserveStatus.DISABLED:
                    return "無";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 録画結果ステータスから録画結果ステータス識別文字列を返す
        /// </summary>
        /// <param name="recEndStatus">録画結果ステータス</param>
        /// <returns>録画結果ステータス識別子</returns>
        public static string GetRecEndStatusString(RecEndStatus recEndStatus)
        {
            switch (recEndStatus)
            {
                case RecEndStatus.NORMAL:
                    return "◎";
                case RecEndStatus.NEXT_START_END:
                case RecEndStatus.CHG_TIME:
                case RecEndStatus.END_SUBREC:
                    return "○";
                case RecEndStatus.ERR_END:
                case RecEndStatus.NOT_START_HEAD:
                    return "△";
                case RecEndStatus.NO_RECMODE:
                    return "無";
                default:
                    return "×";
            }
        }

        /// <summary>
        /// キー取得処理
        /// </summary>
        /// <param name="ids">ID配列</param>
        /// <returns>連結キー</returns>
        public static string GetKey(params ushort[] ids)
        {
            return String.Join(DELIMITER, ids);
        }

        /// <summary>
        /// キー取得処理
        /// </summary>
        /// <param name="ids">ID配列</param>
        /// <returns>連結キー</returns>
        public static string GetKey(params string[] ids)
        {
            return String.Join(DELIMITER, ids);
        }

        /// <summary>
        /// BonDriver名からデフォルトチューナー名を生成する処理
        /// </summary>
        /// <param name="bonDriverName">BonDriver名</param>
        /// <returns>デフォルトチューナー名</returns>
        public static string GetDefaultTunerName(string bonDriverName)
        {
            if (bonDriverName == "チューナー不足")
            {
                return "TU不足";
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(bonDriverName, @"_(MLT|S3U|uSUNpTV|Bulldog)[0-9]*\.dll", RegexOptions.IgnoreCase))
            {
                return "3波";
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(bonDriverName, @"_S[0-9]*\.dll", RegexOptions.IgnoreCase))
            {
                return "BS/CS";
            }
            else
            {
                return "地デジ";
            }
        }

        /// <summary>
        /// URLをブラウザで開く処理
        /// </summary>
        public static void OpenBrowser(string url)
        {
            try
            {
                new Uri(url);
                var startInfo = new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch
            {
                MessageBox.Show($"Web番組詳細URLが不正です。Web番組詳細URLの設定を見直してください。\nURL: {url}", "ブラウザ起動エラー");
            }
        }

    }
}
