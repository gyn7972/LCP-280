using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// CassetteLoadingElevator Unit의 Config 폼 예시
    /// </summary>
    public partial class CassetteLoadingElevatorUnit_Config : Form
    {
        private CassetteLoadingElevator CassetteLoadingElevator { get; set; }


        public CassetteLoadingElevatorUnit_Config()
        {
            InitializeComponent();

            // 폼 로딩 중에는 화면 업데이트 중단
            this.SuspendLayout();

            InitializeUI();

            // 모든 초기화가 완료된 후 화면 업데이트 재개
            this.ResumeLayout(true);
        }

        /// <summary>
        /// 폼이 처음 표시될 때 호출되는 메서드 오버라이드
        /// </summary>
        protected override void SetVisibleCore(bool value)
        {
            // 폼이 완전히 초기화되기 전까지는 화면에 표시하지 않음
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        /// <summary>
        /// 폼 로드 완료 후 호출
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            // 모든 컨트롤이 로드된 후 한번에 화면 갱신
            this.SuspendLayout();
            base.OnLoad(e);
            this.ResumeLayout(true);
            this.Refresh();
        }
    }
}