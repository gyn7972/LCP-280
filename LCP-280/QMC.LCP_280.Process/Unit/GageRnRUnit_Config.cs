using QMC.Common;
using QMC.Common.Spectrometer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// CassetteLoadingElevator Unit의 Config 폼
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class GageRnRUnit_Config : Form
    {
        private const string UNIT_NAME = "GageRnRUnit";
        private Equipment Equipment => Equipment.Instance;
        private GageRnR GageRnRUnit { get; set; }
        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        public GageRnRUnit_Config()
        {
            InitializeComponent();
            this.SuspendLayout();
            _designerSize = this.Size;
            InitializeUI();
            this.ResumeLayout(true);

            Console.WriteLine($"✅ GageRnRUnit_Config 생성자 완료");
        }

        private void InitializeUnit()
        {
            try
            {
                if (Equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    GageRnRUnit = unit as GageRnR;
                }

                if (GageRnRUnit == null)
                {
                    MessageBox.Show($"{UNIT_NAME} Unit을 찾을 수 없습니다.\nEquipment에 Unit이 등록되어 있는지 확인하세요.",
                        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Console.WriteLine($"{UNIT_NAME} Unit 연결 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetPanelSize(int width, int height)
        {
            // 디자이너 값과 다른 경우 경고(1회)
            if (!_sizeMismatchWarned && (width != _designerSize.Width || height != _designerSize.Height))
            {
                string formName = this.GetType().Name;
                string msg =
                    $"폼: {formName}\n" +
                    $"디자이너 크기: {_designerSize.Width} x {_designerSize.Height}\n" +
                    $"전달 크기(SetPanelSize): {width} x {height}\n\n" +
                    "크기가 일치하지 않습니다.";
#if DEBUG
                Debug.WriteLine($"[SizeMismatch] {msg}");
#endif
                try { MessageBox.Show(this, msg, "크기 불일치", MessageBoxButtons.OK, MessageBoxIcon.Warning); } catch { /* ignore */ }
                _sizeMismatchWarned = true;
            }

            try
            {
                this.SuspendLayout();
                this.Size = new Size(width, height);
                this.Invalidate();
                this.Update();
            }
            finally
            {
                this.ResumeLayout(true);
            }

            Console.WriteLine($"📐 {nameof(InputCassetteLifterUnit_Config)}.SetPanelSize → {width}x{height}");
        }
    }
}