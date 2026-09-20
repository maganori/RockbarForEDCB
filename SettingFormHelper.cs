using EpgTimer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RockbarForEDCB
{
    /// <summary>
    /// ListView用の比較クラス
    /// ListViewごとに作成・状態保持しておき、列ヘッダクリックごとにソートを行う
    /// </summary>
    public class ListViewItemComparer : IComparer
    {
        private int _columnIndex = -1;
        private SortOrder _sortOrder = SortOrder.Ascending;
        private bool _isNumber = false;
        private bool _isNetworkTypeColumn = false;

        /// <summary>
        /// 列index更新処理
        /// </summary>
        /// <param name="columnIndex">列index</param>
        /// <param name="isNumber">数値列の場合はtrueにする</param>
        /// <param name="isNetworkTypeColumn">ネットワーク種別列の場合はtrueにする</param>
        public void SetColumn(int columnIndex, bool isNumber = false, bool isNetworkTypeColumn = false)
        {
            _isNumber = isNumber;
            _isNetworkTypeColumn = isNetworkTypeColumn;

            // 同じカラムがクリックされた場合はソート方向を反転
            if (_columnIndex == columnIndex)
            {
                if (_sortOrder == SortOrder.Ascending)
                {
                    _sortOrder = SortOrder.Descending;
                }
                else
                {
                    _sortOrder = SortOrder.Ascending;
                }
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
            }

            _columnIndex = columnIndex;
        }

        /// <summary>
        /// 比較処理
        /// 要素1<要素2 → 負, 要素1>要素2 → 正, 要素1=要素2 → 0
        /// </summary>
        /// <param name="x1">要素1</param>
        /// <param name="x2">要素2</param>
        /// <returns>比較結果</returns>
        public int Compare(object x1, object x2)
        {
            if (_columnIndex < 0)
            {
                return 0;
            }

            ListViewItem item1 = (ListViewItem)x1;
            ListViewItem item2 = (ListViewItem)x2;

            string text1 = item1.SubItems[_columnIndex].Text;
            string text2 = item2.SubItems[_columnIndex].Text;

            int result = 0;

            // ネットワーク種別カラムの場合
            if (_isNetworkTypeColumn)
            {
                // ネットワーク種別文字列に応じた並び順インデックスを取得して並べ替え
                int order1 = GetNetworkTypeOrder(text1);
                int order2 = GetNetworkTypeOrder(text2);
                result = order1.CompareTo(order2);

                // 同じ種別（地デジ同士など）なら名前で比較
                if (result == 0)
                {
                    result = string.Compare(text1, text2);
                }
            }
            // 数値カラムの場合
            else if (_isNumber)
            {
                if (int.TryParse(text1, out int num1) && int.TryParse(text2, out int num2))
                {
                    result = num1.CompareTo(num2);
                }
                else
                {
                    result = string.Compare(text1, text2);
                }
            }
            // その他(文字列カラム)の場合
            else
            {
                result = string.Compare(text1, text2);
            }

            if (_sortOrder == SortOrder.Descending)
            {
                result *= -1;
            }

            return result;
        }

        /// <summary>
        /// ネットワーク種別文字列に応じた表示優先度（並び順インデックス）を取得する
        /// </summary>
        private int GetNetworkTypeOrder(string value)
        {
            if (value.StartsWith("地"))
                return 0;

            // BSで引っかかる前に設定
            if (value.StartsWith("BS4K"))
                return 5;

            if (value.StartsWith("BS"))
                return 1;

            if (value.StartsWith("CS"))
                return 2;

            if (value.StartsWith("CATV"))
                return 3;

            if (value.StartsWith("SPHD"))
                return 4;

            return 99;
        }
    }

    /// <summary>
    /// チューナー名設定のListViewを管理するクラス
    /// </summary>
    public class TunerNameListViewManager
    {
        private readonly ConfigManager _configManager;
        private readonly List<TunerReserveInfo> _tunerReserveInfos;
        private readonly ListView _tunerNameListView;

        private readonly ListViewItemComparer _listViewSorter;
        private const int _bonDriverNameColumnIndex = 2;
        private const int _tunerNameColumnIndex = 3;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TunerNameListViewManager(
            ConfigManager configManager,
            List<TunerReserveInfo> tunerReserveInfos,
            ListView tunerNameListView)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _tunerReserveInfos = tunerReserveInfos ?? throw new ArgumentNullException(nameof(tunerReserveInfos));
            _tunerNameListView = tunerNameListView ?? throw new ArgumentNullException(nameof(tunerNameListView));

            _listViewSorter = new ListViewItemComparer();
        }

        /// <summary>
        /// チューナー名設定の読み込み
        /// </summary>
        public void Load()
        {
            _tunerNameListView.Items.Clear();

            // チューナー名タブのListView作成
            foreach (TunerReserveInfo tunerReserveInfo in _tunerReserveInfos)
            {
                // ListView作成
                if (!_tunerNameListView.Items.ContainsKey(tunerReserveInfo.tunerName))
                {
                    string[] data;
                    if (_configManager.RockbarSetting.BonDriverNameToTunerName.ContainsKey(tunerReserveInfo.tunerName))
                    {
                        data = new[]
                        {
                            "",
                            (tunerReserveInfo.tunerID & 0xffff0000).ToString("x8").Substring(0, 4),
                            tunerReserveInfo.tunerName,
                            _configManager.RockbarSetting.BonDriverNameToTunerName[tunerReserveInfo.tunerName]
                        };
                    }
                    else
                    {
                        data = new[]
                        {
                            "",
                            (tunerReserveInfo.tunerID & 0xffff0000).ToString("x8").Substring(0, 4),
                            tunerReserveInfo.tunerName,
                            RockbarUtility.GetDefaultTunerName(tunerReserveInfo.tunerName)
                        };
                    }

                    ListViewItem item = new ListViewItem(data)
                    {
                        Name = tunerReserveInfo.tunerName
                    };
                    _tunerNameListView.Items.Add(item);
                }
            }

            // 取得したチューナに含まれず、設定にだけあるものを表示
            foreach (var kv in _configManager.RockbarSetting.BonDriverNameToTunerName)
            {
                // ListView上になければ追加しておく
                if (!_tunerNameListView.Items.ContainsKey(kv.Key))
                {
                    string[] data =
                    {
                        "！",
                        "",
                        kv.Key,
                        kv.Value
                    };

                    ListViewItem item = new ListViewItem(data)
                    {
                        Name = kv.Key
                    };
                    _tunerNameListView.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// チューナー名設定の保存
        /// </summary>
        public void Save()
        {
            _configManager.RockbarSetting.BonDriverNameToTunerName = new Dictionary<string, string>();

            foreach (ListViewItem item in _tunerNameListView.Items)
            {
                _configManager.RockbarSetting.BonDriverNameToTunerName.Add(
                    item.SubItems[_bonDriverNameColumnIndex].Text,
                    item.SubItems[_tunerNameColumnIndex].Text
                );
            }
        }

        /// <summary>
        /// 列ヘッダクリック時のソート処理
        /// </summary>
        public void Sort(int columnIndex)
        {
            _listViewSorter.SetColumn(columnIndex);
            _tunerNameListView.ListViewItemSorter = _listViewSorter;
            _tunerNameListView.Sort();
        }

        /// <summary>
        /// 選択されているチューナー名をテキストボックスに反映する
        /// </summary>
        public void DisplaySelectedNameTo(TextBox targetTextBox)
        {
            if (_tunerNameListView.SelectedItems.Count > 0)
            {
                targetTextBox.Text = _tunerNameListView.SelectedItems[0].SubItems[_tunerNameColumnIndex].Text;
            }
        }

        /// <summary>
        /// テキストボックスの値を選択中アイテムのチューナー名に反映する
        /// </summary>
        public void UpdateSelectedNameFrom(TextBox sourceTextBox)
        {
            if (_tunerNameListView.SelectedItems.Count > 0)
            {
                _tunerNameListView.SelectedItems[0].SubItems[_tunerNameColumnIndex].Text = sourceTextBox.Text;
            }
        }
    }

    public static class UiFontHelper
    {
        private static readonly TypeConverter _fontConverter = TypeDescriptor.GetConverter(typeof(Font));

        /// <summary>
        /// 設定値に基づいて、フォームおよび配下コントロールのフォントに一括適用
        /// </summary>
        public static void ApplyUiFontSettings(Control rootControl, RockBarSetting setting)
        {
            if (rootControl == null || setting == null) return;

            try
            {
                Font defaultFont = ParseFont(setting.Font);
                Font menuFont = ParseFont(setting.MenuFont) ?? defaultFont;
                Font tabFont = ParseFont(setting.TabFont) ?? defaultFont;
                Font buttonFont = ParseFont(setting.ButtonFont) ?? defaultFont;
                Font labelFont = ParseFont(setting.LabelFont) ?? defaultFont;
                Font textBoxFont = ParseFont(setting.TextBoxFont) ?? defaultFont;

                // フォーム全体のベースフォントを設定
                if (defaultFont != null)
                {
                    rootControl.Font = defaultFont;
                }

                // メニュー類のフォント設定
                if (rootControl is Form form && form.MainMenuStrip != null && menuFont != null)
                {
                    form.MainMenuStrip.Font = menuFont;
                }

                // コントロールツリーを走査してフォントを上書き適用
                ApplyFontRecursive(rootControl, menuFont, tabFont, buttonFont, labelFont, textBoxFont);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyUiFontSettings Error: {ex.Message}");
            }
        }

        /// <summary>
        /// コントロールとその子コントロールに対して再帰的にフォントを適用する
        /// </summary>
        private static void ApplyFontRecursive(
            Control control,
            Font menuFont,
            Font tabFont,
            Font buttonFont,
            Font labelFont,
            Font textBoxFont)
        {
            if (control == null) return;

            if (control is ToolStrip toolStrip && menuFont != null)
            {
                toolStrip.Font = menuFont;
            }
            else if (control is TabControl && tabFont != null)
            {
                control.Font = tabFont;
            }
            else if (control is Button && buttonFont != null)
            {
                control.Font = buttonFont;
            }
            else if ((control is Label || control is CheckBox || control is RadioButton ||
                      control is GroupBox || control is ListView) && labelFont != null)
            {
                control.Font = labelFont;
            }
            else if ((control is TextBox || control is ComboBox || control is NumericUpDown ||
                      control is ListBox) && textBoxFont != null)
            {
                control.Font = textBoxFont;
            }

            // 子コントロールを再帰的に走査
            foreach (Control child in control.Controls)
            {
                ApplyFontRecursive(child, menuFont, tabFont, buttonFont, labelFont, textBoxFont);
            }
        }

        /// <summary>
        /// フォント設定文字列を Font オブジェクトに変換
        /// 変換に失敗した場合や空文字列の場合は null を返す
        /// </summary>
        /// <param name="fontString">変換対象のフォント設定文字列</param>
        /// <returns>生成された Font オブジェクト（失敗時は null）</returns>
        private static Font ParseFont(string fontString)
        {
            if (string.IsNullOrWhiteSpace(fontString)) return null;
            try
            {
                return _fontConverter.ConvertFromString(fontString) as Font;
            }
            catch
            {
                return null;
            }
        }
    }
}
