using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(3)]
    public partial class Measurement_Recipe : Form
    {
        private bool _initialized;
        private bool _reloadBusy;

        public Measurement_Recipe()
        {
            InitializeComponent();

            // 화면 전환(표시/활성) 시 현재 레시피/연관 파일 로드
            this.Shown += (_, __) => EnsureRecipeLoadedOnEnter();
            this.VisibleChanged += (_, __) =>
            {
                if (Visible)
                    EnsureRecipeLoadedOnEnter();
            };
            this.Activated += (_, __) => EnsureRecipeLoadedOnEnter();

        }

        private void EnsureRecipeLoadedOnEnter()
        {
            if (!Visible || IsDisposed || Disposing)
                return;

            if (_reloadBusy)
                return;

            _reloadBusy = true;
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                    return;

                var eq = Equipment.Instance;
                if (eq == null || eq.EquipmentRecipe == null)
                    return;

                // 1) 현재 레시피 동기화 (없으면 생성까지 보장)
                var recipe = eq.EquipmentRecipe.GetRecipe();
                if (recipe == null)
                    return;

                // 2) TestConditionSet / BinningSpec를 Tester에 반영(런타임 적용)
                //    - Main_Recipe 쪽은 ConditionSet.LoadFromFile을 직접 호출도 하지만,
                //      여기서는 RebuildTestMechanism까지 포함되는 LoadTestConditionSet을 사용
                try
                {
                    var tester = eq.Tester;
                    if (tester != null)
                    {
                        if (!string.IsNullOrWhiteSpace(recipe.TestConditionSetFile))
                            tester.LoadTestConditionSet(recipe.TestConditionSetFile);

                        if (!string.IsNullOrWhiteSpace(recipe.BinningSpecSheetFile))
                            tester.LoadBinningModel(recipe.BinningSpecSheetFile);

                        // 다른 모듈에서 참조하는 경우가 있어 동기화
                        try { eq.ResultWriterManager.CurrentTestConditionSet = tester.ConditionSet; } catch { }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    // 여기서 MessageBox를 띄우면 화면 전환 때마다 팝업이 반복될 수 있어 Log만 남김
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                _reloadBusy = false;
            }
        }

        private readonly int _tabBorderWidth = 2;
        private readonly Color _tabBorderColor = Color.Black;
        private readonly Font _tabFont = new Font("맑은 고딕", 10, FontStyle.Bold);

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tab = sender as TabControl;
            TabPage page = tab.TabPages[e.Index];
            Rectangle tabRect = tab.GetTabRect(e.Index);
            Color backColor = (e.Index == tab.SelectedIndex) ? Color.White : Color.Gainsboro;
            using (Brush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, tabRect);
            }
            using (Pen borderPen = new Pen(_tabBorderColor, _tabBorderWidth))
            {
                Rectangle borderRect = tabRect;
                if (_tabBorderWidth > 1)
                {
                    borderRect.Inflate(-_tabBorderWidth / 2, -_tabBorderWidth / 2);
                }
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }
            string text = page.Text;
            Size tabSize = tabRect.Size;
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            SizeF textSize = e.Graphics.MeasureString(text, _tabFont);

            if (textSize.Width > tabSize.Width - 8)
            {
                string[] words = text.Split(' ');
                string line1 = words[0];
                string line2 = string.Join(" ", words.Skip(1));
                if (words.Length > 1)
                {
                    for (int i = 1; i < words.Length; i++)
                    {
                        string testLine = line1 + " " + words[i];
                        if (e.Graphics.MeasureString(testLine, _tabFont).Width < tabSize.Width - 8)
                        {
                            line1 = testLine;
                            line2 = string.Join(" ", words.Skip(i + 1));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                RectangleF line1Rect = new RectangleF(tabRect.X, tabRect.Y + 2, tabRect.Width, tabRect.Height / 2 - 2);
                RectangleF line2Rect = new RectangleF(tabRect.X, tabRect.Y + tabRect.Height / 2, tabRect.Width, tabRect.Height / 2 - 2);
                e.Graphics.DrawString(line1, _tabFont, Brushes.Black, line1Rect, sf);
                e.Graphics.DrawString(line2, _tabFont, Brushes.Black, line2Rect, sf);
            }
            else
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    text,
                    _tabFont,
                    tabRect,
                    Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 탭이 바뀔 때마다 레시피 파일을 다시 읽어서 페이지에 반영
            try
            {
                // Test Condition Set 탭일 때만 강제 Reload (필요하면 다른 탭도 동일 패턴으로 확장)
                if (tabControl1.SelectedTab == tabPage1 && testConditionSetPage1 != null)
                {
                    testConditionSetPage1.ReloadFromRecipe(showErrorMessage: false);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}