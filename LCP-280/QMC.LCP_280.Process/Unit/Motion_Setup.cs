using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// CassetteLoadingElevator Unit의 Config 폼
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class Motion_Setup : Form
    {
        private const string UNIT_NAME = "CassetteLoadingElevator";
        
        /// <summary>
        /// Equipment 인스턴스 참조
        /// </summary>
        private Equipment Equipment => Equipment.Instance;

        /// <summary>
        /// 해당 Unit 인스턴스
        public Motion_Setup()
        {
            InitializeComponent();
            // 폼 로딩 중에는 화면 업데이트 중단
            this.SuspendLayout();
            InitializeUI();
            // 모든 초기화가 완료된 후 화면 업데이트 재개
            this.ResumeLayout(true);
            
            Console.WriteLine($"✅ CassetteLoadingElevatorUnit_Config 생성자 완료");
        }

        ///// <summary>
        ///// Unit 초기화 및 Equipment에서 Unit 인스턴스 가져오기
        ///// </summary>
        private void InitializeUnit()
        {
        }
    }
}