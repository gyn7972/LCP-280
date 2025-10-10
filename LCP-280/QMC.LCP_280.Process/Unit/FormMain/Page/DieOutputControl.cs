using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QMC.Common.Controls;                 // DisplayView_DieOutputControl
using QMC.LCP_280.Process.Component;       // MaterialDie, DieProcessState

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieOutputControl : UserControl
    {
        // 기존 내부 Die / DieState 제거
        //  - DieInputControl 과 동일하게 공정 데이터 모델 MaterialDie 사용
        //  - Display 표시는 DisplayView_DieOutputControl 전용 ItemState 로 변환

        public event EventHandler<DisplayView_DieOutputControl.DisplayItemEventArgs> MotorMoveRequested;

        private List<MaterialDie> _dies = new List<MaterialDie>();

        public DieOutputControl()
        {
            InitializeComponent();
            displayView1.MotorMoveRequested += OnDisplayView_MotorMoveRequested;
        }

        private void OnDisplayView_MotorMoveRequested(object sender, DisplayView_DieOutputControl.DisplayItemEventArgs e)
        {
            MotorMoveRequested?.Invoke(this, e);
        }

        public void SetWaferId(string waferId)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetWaferId(waferId)));
                return;
            }
            lblWaferIdValue.Text = waferId;
        }

        private void UpdateDieCount()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateDieCount));
                return;
            }

            // 출력 쪽 카운트 정의:
            //  - Placed : 이미 출력 영역(또는 배출 완료 위치)에 놓인 상태
            //  - Picked : (필요 시) 픽 완료 후 상태 유지되는 경우 포함
            // 프로젝트 정책에 따라 조정 가능
            int total = _dies.Count;
            int present = _dies.Count(d =>
                   d.State == DieProcessState.Placed
                || d.State == DieProcessState.Picked
                || d.State == DieProcessState.Inspected    // 필요하면 표시
                || d.State == DieProcessState.Rejected);   // 필요 시 제외 가능

            lblDieCountValue.Text = present.ToString() + "/" + total.ToString();
        }

        /// <summary>
        /// 출력 다이 목록 설정 (MaterialDie 사용)
        /// </summary>
        public void SetDieList(List<MaterialDie> dies)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetDieList(dies)));
                return;
            }

            _dies = dies ?? new List<MaterialDie>();
            UpdateDieCount();

            var items = _dies.Select(d => new DisplayView_DieOutputControl.DisplayItem
            {
                Position = new Point(d.BinX, d.BinY),
                State = ConvertState(d.State),
                DieId = d.Index,
                Info = d.TesterResult.BinningResult.BinLabel    // MaterialDie 에 BinCode / 기타 출력용 필드가 있다고 가정
            }).ToList();

            displayView1.SetItems(items);
            displayView1.Refresh();
        }

        /// <summary>
        /// 개별 다이 상태 업데이트
        /// </summary>
        public void UpdateDie(Point mapCoord, DieProcessState newState)
        {
            var die = _dies.FirstOrDefault(d => d.BinX == mapCoord.X && d.BinY == mapCoord.Y);
            if (die == null) return;

            die.State = newState;
            UpdateDieCount();
            // 뷰 전체 갱신 (부분 갱신 인터페이스 없으면 재바인딩)
            SetDieList(_dies);
        }

        private DisplayView_DieOutputControl.ItemState ConvertState(DieProcessState state)
        {
            // DisplayView_DieOutputControl.ItemState : Empty / Present / Picked
            switch (state)
            {
                case DieProcessState.Picked:
                    return DisplayView_DieOutputControl.ItemState.Picked;

                case DieProcessState.Placed:
                case DieProcessState.Inspected:
                case DieProcessState.Inspecting:
                case DieProcessState.Mapped:
                case DieProcessState.Rejected:
                    return DisplayView_DieOutputControl.ItemState.Present;

                default:
                    return DisplayView_DieOutputControl.ItemState.Empty;
            }
        }

        #region (선택) 호환용 Legacy Wrapper
        // 과거 코드에서 DieOutputControl.Die 사용하던 부분이 있다면
        // 아래 Legacy 구조체/메서드로 일시적 호환 가능. 필요 없으면 제거.
        [Obsolete("MaterialDie 로 교체됨. SetDieList(List<MaterialDie>) 사용.")]
        public class LegacyDie
        {
            public int Index { get; set; }
            public Point Position { get; set; }
            public bool Present { get; set; }
        }

        
        #endregion
    }
}
