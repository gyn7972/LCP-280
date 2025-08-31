using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Spectrometer;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// Input Stage Ejector Unit의 Working 폼
    /// </summary>
    public partial class InputStageEjectorUnit_Working : Form
    {
        private const string UNIT_NAME = "Input Stage Ejector";
        private Equipment Equipment => Equipment.Instance;
        private InputStageEjector InputStageEjectorUnit { get; set; }
        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        public InputStageEjectorUnit_Working()
        {
            InitializeComponent();
            _designerSize = this.Size;
            this.SuspendLayout();
            InitializeUI();
            this.ResumeLayout(true);

            Console.WriteLine("✅ InputStageEjectorUnit_Working 생성자 완료");
        }

        private void InitializeUnit()
        {
            try
            {
                if (Equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                    InputStageEjectorUnit = unit as InputStageEjector;

                if (InputStageEjectorUnit == null)
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

        private void InitializeUI()
        {
            try
            {
                // 필요한 추가 UI 초기화 로직이 있으면 여기에 작성
            }
            catch (Exception)
            {
                // 무시 또는 로깅
            }
        }

        private void button_Test_Click(object sender, EventArgs e)
        {
            TestGyn testGyn = new TestGyn();
            testGyn.ShowDialog();
        }

        public void SetPanelSize(int width, int height)
        {
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
                try { MessageBox.Show(this, msg, "크기 불일치", MessageBoxButtons.OK, MessageBoxIcon.Warning); } catch { }
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

            Console.WriteLine($"📐 {nameof(InputStageEjectorUnit_Working)}.SetPanelSize → {width}x{height}");
        }
    }
}
