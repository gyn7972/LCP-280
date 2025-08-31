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
    /// CassetteLoadingElevator Unit의 Config 폼
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class InputCassetteLifterUnit_Working : Form // : BaseContentForm : 
    {
        private const string UNIT_NAME = "InputCassetteLifterUnit";
        private Equipment Equipment => Equipment.Instance;
        private InputCassetteLifter InputCassetteLifterUnit { get; set; }

        // 디자이너에 설정되어 있던 기본 크기 스냅샷 + 중복 알림 방지
        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        // 추가: 속성 뷰와 데이터 보관

        public InputCassetteLifterUnit_Working()
        {
            InitializeComponent();

            // 디자이너(ClientSize) 기준 크기 저장
            _designerSize = this.Size;

            this.SuspendLayout();

            InitializeUI();
            this.ResumeLayout(true);

            Console.WriteLine("✅ InputCassetteLifterUnit_Working 생성자 완료");
        }

        private void InitializeUnit()
        {
            try
            {
                if (Equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                    InputCassetteLifterUnit = unit as InputCassetteLifter;

                if (InputCassetteLifterUnit == null)
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

        private void button_Test_Click(object sender, EventArgs e)
        {
            TestGyn testGyn = new TestGyn();
            testGyn.ShowDialog();
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

            Console.WriteLine($"📐 {nameof(InputCassetteLifterUnit_Working)}.SetPanelSize → {width}x{height}");
        }

        private void AxispositonListBoxItemsView_Load(object sender, EventArgs e)
        {
        }

        // (옵션) 저장 시 텍스트박스 → PropertyBase에 커밋하고 사용
        private void ApplyProperties()
        {
            // 텍스트 변경을 즉시 반영하도록 바인딩되어 있지만,
            // 명시 커밋이 필요할 때 호출

            // _lifterPropView.GetCurrentProperties()로 현재 컬렉션을 얻어
            // 모델/설비에 반영하는 로직을 추가 가능
        }
    }
}