using System;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// DieLoader Unit의 Config 폼 예시
    /// </summary>
    public partial class DieLoaderUnit_Config : Form
    {
        public DieLoaderUnit_Config()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form 기본 설정
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 600);
            this.Name = "DieLoaderUnit_Config";
            this.Text = "Die Loader Unit Configuration";
            
            this.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;

            // 메인 패널
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            this.Controls.Add(mainPanel);

            // 제목 라벨
            Label titleLabel = new Label
            {
                Text = "Die Loader Unit Configuration",
                Font = new Font("맑은 고딕", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(titleLabel);

            // 설정 그룹박스
            GroupBox configGroup = new GroupBox
            {
                Text = "설정 옵션",
                Font = new Font("맑은 고딕", 10, FontStyle.Regular),
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(configGroup);

            // 설정 컨트롤들
            TableLayoutPanel settingsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10)
            };
            
            settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            
            for (int i = 0; i < 5; i++)
            {
                settingsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            }

            configGroup.Controls.Add(settingsPanel);

            // 설정 항목들 추가
            AddSettingItem(settingsPanel, 0, "Pick Position X:", "0.0");
            AddSettingItem(settingsPanel, 1, "Pick Position Y:", "0.0");
            AddSettingItem(settingsPanel, 2, "Pick Speed:", "100");
            AddSettingItem(settingsPanel, 3, "Retry Count:", "3");
            AddSettingItem(settingsPanel, 4, "Timeout (ms):", "5000");
        }

        private void AddSettingItem(TableLayoutPanel parent, int row, string labelText, string defaultValue)
        {
            Label label = new Label
            {
                Text = labelText,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            parent.Controls.Add(label, 0, row);

            TextBox textBox = new TextBox
            {
                Text = defaultValue,
                Dock = DockStyle.Fill,
                Margin = new Padding(3)
            };
            parent.Controls.Add(textBox, 1, row);
        }
    }
}