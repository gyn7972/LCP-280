using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QMC.Common.Controls;
using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieInputControl : UserControl
    {
        public event EventHandler<DisplayView.DisplayItemEventArgs> MotorMoveRequested;

        private List<MaterialDie> _chips = new List<MaterialDie>();

        // 픽업된 좌표를 누적 보관 (UI 강제 유지용)
        private readonly HashSet<Point> _pickedCoords = new HashSet<Point>();
        // 마지막 실제 Pick 좌표
        private Point? _lastPickedCoord;
        // 기존 Pick 완료 좌표
        private Point? _AfterPickCoord;

        public DieInputControl()
        {
            InitializeComponent();
            displayView1.MotorMoveRequested += OnDisplayView_MotorMoveRequested;
        }

        private void OnDisplayView_MotorMoveRequested(object sender, DisplayView.DisplayItemEventArgs e)
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
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateDieCount()));
                return;
            }
            int count = _chips.Count(c => c.State == DieProcessState.Mapped || c.State == DieProcessState.Picked);
            lblDieCountValue.Text = count.ToString();
        }

        // 모델에서 새 리스트가 들어와도, 이미 픽업된 좌표는 강제로 Picked 유지
        public void SetDieList(List<MaterialDie> chips)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetDieList(chips)));
                return;
            }

            var incoming = chips ?? new List<MaterialDie>();

            // 좌표 기준으로 Picked 강제 재적용
            foreach (var c in incoming)
            {
                var p = new Point(c.MapX, c.MapY);
                if (_pickedCoords.Contains(p))
                {
                    c.State = DieProcessState.Picked;
                }
            }

            _chips = incoming;
            UpdateDieCount();

            var items = _chips.Select(c =>
            {
                var p = new Point(c.MapX, c.MapY);

                // 기본 상태
                var state = ConvertState(c.State);

                // 1) 현재 픽업 좌표는 최우선으로 PickedUp
                if (_lastPickedCoord.HasValue && _lastPickedCoord.Value == p)
                {
                    state = DisplayView.ItemState.PickedUp;
                }
                // 2) 누적된(과거) 픽업 좌표는 AfterPickUp
                else if (_pickedCoords.Contains(p))
                {
                    state = DisplayView.ItemState.AfterPickUp;
                }
                //// 3) 아직 픽업 전이면서 다음 예정 좌표는 BeforePickUp
                //else if (_beforePickCoord.HasValue && _beforePickCoord.Value == p)
                //{
                //    state = DisplayView.ItemState.BeforePickUp;
                //}

                return new DisplayView.DisplayItem
                {
                    Position = p,
                    State = state
                };
            }).ToList();

            displayView1.SetItems(items);
            displayView1.Refresh();
        }

        private DisplayView.ItemState ConvertState(DieProcessState state)
        {
            switch (state)
            {
                case DieProcessState.Picked: return DisplayView.ItemState.AfterPickUp;
                case DieProcessState.Mapped:
                case DieProcessState.Inspecting:
                case DieProcessState.Inspected:
                case DieProcessState.Placed:
                case DieProcessState.Rejected:
                    return DisplayView.ItemState.Present;
                default:
                    return DisplayView.ItemState.Empty;
            }
        }

        public void UpdateChip(Point mapCoord, DieProcessState state)
        {
            var chip = _chips.FirstOrDefault(c => c.MapX == mapCoord.X && c.MapY == mapCoord.Y);
            if (chip != null)
            {
                chip.State = state;

                // Picked 라면 누적 집합에도 반영
                if (state == DieProcessState.Picked)
                    _pickedCoords.Add(mapCoord);

                UpdateDieCount();
                SetDieList(_chips);
            }
        }

        // 다음 픽업 예정 좌표 표시
        public void MarkAfterPickUp(Point coord)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkAfterPickUp(coord)));
                return;
            }
            _AfterPickCoord = coord;
            SetDieList(_chips);
        }

        // 예정 좌표 해제
        public void ClearAfterPickUp()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ClearAfterPickUp));
                return;
            }
            _AfterPickCoord = null;
            SetDieList(_chips);
        }

        // 실제 픽업 완료 처리: Picked 누적 + 예정 좌표 해제
        public void MarkCurrentPicked(Point current)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkCurrentPicked(current)));
                return;
            }

            _pickedCoords.Add(current);
            _lastPickedCoord = current;

            // 방금 픽했으므로 BeforePickUp 해제
            if (_AfterPickCoord.HasValue && _AfterPickCoord.Value == current)
                _AfterPickCoord = null;

            SetDieList(_chips);
        }

        // 필요 시 API 유지(현재는 이전 픽업을 지우지 않음)
        public void MarkCurrentPicked(Point current, bool clearPrevious)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkCurrentPicked(current, clearPrevious)));
                return;
            }

            MarkDieRemoved(current, showAsPicked: true);
            _lastPickedCoord = current;
        }

        public void MarkDieRemoved(Point mapCoord, bool showAsPicked = true)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkDieRemoved(mapCoord, showAsPicked)));
                return;
            }

            var chip = _chips.FirstOrDefault(c => c.MapX == mapCoord.X && c.MapY == mapCoord.Y);
            if (chip == null) return;

            // UI 누적 집합 업데이트
            if (showAsPicked)
                _pickedCoords.Add(mapCoord);
            else
                _pickedCoords.Remove(mapCoord);

            chip.State = showAsPicked ? DieProcessState.Picked : DieProcessState.None;

            UpdateDieCount();
            SetDieList(_chips);
        }

        public void MarkDiesRemoved(IEnumerable<Point> removedCoords, bool showAsPicked = true)
        {
            if (removedCoords == null) return;

            var removed = removedCoords as IList<Point> ?? removedCoords.ToList();

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkDiesRemoved(removed, showAsPicked)));
                return;
            }

            foreach (var pt in removed)
            {
                if (showAsPicked) _pickedCoords.Add(pt);
                else _pickedCoords.Remove(pt);
            }

            foreach (var chip in _chips)
            {
                var p = new Point(chip.MapX, chip.MapY);
                if (removed.Contains(p))
                {
                    chip.State = showAsPicked ? DieProcessState.Picked : DieProcessState.None;
                }
            }

            UpdateDieCount();
            SetDieList(_chips);
        }

        // 스냅샷 동기화 시에도, 누적 Picked 좌표는 유지
        public void ApplyPresenceSnapshot(IEnumerable<Point> presentCoords)
        {
            var present = presentCoords ?? Enumerable.Empty<Point>();
            var presentList = present as IList<Point> ?? present.ToList();

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ApplyPresenceSnapshot(presentList)));
                return;
            }

            var presentSet = new HashSet<Point>(presentList);

            foreach (var chip in _chips)
            {
                var p = new Point(chip.MapX, chip.MapY);

                if (_pickedCoords.Contains(p))
                {
                    chip.State = DieProcessState.Picked; // 강제 유지
                    continue;
                }

                if (presentSet.Contains(p))
                {
                    if (chip.State != DieProcessState.Picked)
                        chip.State = DieProcessState.Mapped;
                }
                else
                {
                    chip.State = DieProcessState.None;
                }
            }

            UpdateDieCount();
            SetDieList(_chips);
        }

        // 외부에서 명시적으로 초기화할 수 있는 API
        public void ResetPickedMarks()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ResetPickedMarks));
                return;
            }

            _pickedCoords.Clear();
            _lastPickedCoord = null;

            // 현재 리스트 기준으로 다시 렌더(강제 Picked 적용 제거됨)
            SetDieList(_chips);
        }
    }
}
