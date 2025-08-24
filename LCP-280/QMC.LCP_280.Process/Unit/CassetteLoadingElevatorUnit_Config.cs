using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// CassetteLoadingElevator Unit의 Config 폼
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class CassetteLoadingElevatorUnit_Config : Form
    {
        private const string UNIT_NAME = "CassetteLoadingElevator";

        /// <summary>
        /// Equipment 인스턴스 참조
        /// </summary>
        private Equipment Equipment => Equipment.Instance;

        /// <summary>
        /// 해당 Unit 인스턴스
        /// </summary>
        private CassetteLoadingElevator CassetteLoadingElevator { get; set; }

        public CassetteLoadingElevatorUnit_Config()
        {
            //InitializeComponent();
            //// 폼 로딩 중에는 화면 업데이트 중단
            //this.SuspendLayout();
            //InitializeUI();
            // 모든 초기화가 완료된 후 화면 업데이트 재개
            this.ResumeLayout(true);

            Console.WriteLine($"✅ CassetteLoadingElevatorUnit_Config 생성자 완료");
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
                    CassetteLoadingElevator = unit as CassetteLoadingElevator;
                }

                if (CassetteLoadingElevator == null)
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
            MotionAxisManager _mgr = new MotionAxisManager();
            string _axisFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Axes");
        }
    }
}