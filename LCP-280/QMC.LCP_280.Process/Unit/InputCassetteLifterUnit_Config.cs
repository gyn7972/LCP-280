using System;
using System.Collections.Generic;
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
    public partial class InputCassetteLifterUnit_Config : Form
    {
        private const string UNIT_NAME = "InputCassetteLifterUnit";
        
        /// <summary>
        /// Equipment 인스턴스 참조
        /// </summary>
        private Equipment Equipment => Equipment.Instance;

        /// <summary>
        /// 해당 Unit 인스턴스
        /// </summary>
        private InputCassetteLifter InputCassetteLifterUnit { get; set; }

        public InputCassetteLifterUnit_Config()
        {
            InitializeComponent();
            // 폼 로딩 중에는 화면 업데이트 중단
            this.SuspendLayout();
            InitializeUI();
            // 모든 초기화가 완료된 후 화면 업데이트 재개
            this.ResumeLayout(true);
            
            Console.WriteLine($"✅ InputCassetteLifterUnitUnit_Config 생성자 완료");
        }

        ///// <summary>
        ///// Unit 초기화 및 Equipment에서 Unit 인스턴스 가져오기
        ///// </summary>
        private void InitializeUnit()
        {
            try
            {
                // Equipment에서 CassetteLoadingElevator Unit 가져오기
                if (Equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    InputCassetteLifterUnit = unit as InputCassetteLifter;
                }

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

        /// <summary>
        /// FormConfig 탭 호스트가 전달하는 가용 크기에 맞춰 자동으로 폼 크기를 조정합니다.
        /// </summary>
        /// <param name="width">가용 너비</param>
        /// <param name="height">가용 높이 (탭 헤더 제외)</param>
        public void SetPanelSize(int width, int height)
        {
            try
            {
                this.SuspendLayout();

                // 호스트(TabPage)의 클라이언트 영역을 그대로 사용
                this.Size = new Size(width, height);
                this.ClientSize = new Size(width, height);

                // 포함된 컨트롤들이 Dock=Fill 등으로 배치되었다면 자동으로 맞춰짐
                // 필요 시 내부 루트 컨테이너가 있다면 여기에서 Size/Dock 조정 가능

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