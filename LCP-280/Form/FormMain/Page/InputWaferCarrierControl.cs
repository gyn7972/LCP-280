using QMC.Common;
using QMC.LCP_280.Process.Work;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class InputWaferCarrierControl : UserControl
    {
        public InputWaferCarrierControl()
        {
            InitializeComponent();

            this.Load -= InputWaferCarrierControl_Load;
            this.Load += InputWaferCarrierControl_Load;
        }

        public Component.WaferSelectMapView GetWaferSelectMapView()
        {
            return waferSelectMapView;
        }

        // UI 스레드 실행 헬퍼
        private void RunOnUI(Action action)
        {
            if (action == null) return;
            if (IsDisposed) return;
            if (!IsHandleCreated)
            {
                // 아직 Handle 미생성일 경우 Load 이후로 미루고 싶으면 필요 시 큐에 넣는 로직 추가 가능
                return;
            }

            if (InvokeRequired)
            {
                try 
                { 
                    BeginInvoke(action); 
                } 
                catch (Exception ex) { Log.Write(ex); }
            }
            else
            {
                action();
            }
        }

        public void SetWaferCarrierId(string id)
        {
            RunOnUI(() =>
            {
                if (lblWaferIdValue != null && !lblWaferIdValue.IsDisposed)
                    lblWaferIdValue.Text = id ?? string.Empty;
            });
        }

        public void UpdateWaferCount(int count)
        {
            RunOnUI(() =>
            {
                if (lblWaferCountValue != null && !lblWaferCountValue.IsDisposed)
                    lblWaferCountValue.Text = count.ToString();
            });
        }

        private void InputWaferCarrierControl_Load(object sender, EventArgs e)
        {
            var view = GetWaferSelectMapView();
            var inputUnit = Equipment.Instance.GetUnit("InputCassetteLifter") as InputCassetteLifter;
            if (inputUnit == null) return;

            // 1) 최초 바인딩
            view.SetMaterialCassette(inputUnit.GetMaterialCassette());

            // 2) 장비에서 UI 업데이트 이벤트 오면 갱신
            inputUnit.EventUpdateUICassette += (cassette) =>
            {
                try
                {
                    // 뷰 바인딩/갱신
                    view.SetMaterialCassette(cassette);

                    // 상단 ID/Count 갱신도 같이
                    if (cassette != null)
                    {
                        if (cassette.CarrierId == string.Empty)
                            cassette.CarrierId = $"QMC_OUT_CASSETTE_{cassette.SlotCount}";

                        SetWaferCarrierId(cassette.CarrierId);
                        UpdateWaferCount(GetPresentWaferCount(cassette));
                    }
                }
                catch (Exception ex) { Log.Write(ex); }
            };

            // 3) 클릭 시 수정
            view.SlotClicked += (s, slotEventArgs) =>
            {
                if (!inputUnit.CanEditCassetteFromUI())
                    return;

                int slot0 = slotEventArgs.SlotNumber - 1;

                // 현재 슬롯 wafer 정보 조회(현재 view가 들고 있는 cassette 기준)
                var cassette = inputUnit.GetMaterialCassette();
                var wafer = cassette?.GetWafer(slot0);

                FormWaferSlotEdit.SlotEditAction action;
                string waferId;
                QMC.Common.Material.MaterialProcessSatate? state;

                IWin32Window owner = this.FindForm() as IWin32Window ?? this;
                if (!FormWaferSlotEdit.TryShow(owner, slotEventArgs.SlotNumber, wafer, out action, out waferId, out state))
                    return;

                if (action == FormWaferSlotEdit.SlotEditAction.AddOrUpdate)
                {
                    inputUnit.UiApplySlotEdit(slot0, present: true, waferId: waferId);

                    // 상태 변경까지 진짜로 반영하려면 (아래 4) 참고)
                    inputUnit.UiApplySlotStateEdit(slot0, state.Value);
                }
                else if (action == FormWaferSlotEdit.SlotEditAction.Delete)
                {
                    inputUnit.UiApplySlotEdit(slot0, present: false, waferId: string.Empty);
                    inputUnit.UiApplySlotStateEdit(slot0, QMC.Common.Material.MaterialProcessSatate.Unknown);
                }

                view.NotifyCassetteChanged();
            };

            view.AllApplyRequested += (s, e2) =>
            {
                if (!inputUnit.CanEditCassetteFromUI()) return;

                var cassette = inputUnit.GetMaterialCassette();
                if (cassette == null) return;

                for (int slot0 = 0; slot0 < cassette.SlotCount; slot0++)
                {
                    var w = cassette.GetWafer(slot0);

                    // "없는 상태" 판단: wafer == null 또는 Presence != Exist
                    bool isEmpty = (w == null) || (w.Presence != QMC.Common.Material.MaterialPresence.Exist);

                    if (!isEmpty)
                        continue;

                    // 1) 존재하도록 만들기
                    inputUnit.UiApplySlotEdit(slot0, present: true, waferId: w?.WaferId ?? string.Empty);

                    // 2) 상태를 Ready로 (Presence=Exist 강제 포함)
                    inputUnit.UiApplySlotStateEdit(slot0, QMC.Common.Material.MaterialProcessSatate.Ready);
                }

                view.NotifyCassetteChanged();
            };

            view.ResetCassetteRequested += (s, e2) =>
            {
                if (!inputUnit.CanEditCassetteFromUI()) return;

                var cassette = inputUnit.GetMaterialCassette();
                if (cassette == null) return;

                cassette.CarrierId = string.Empty; // 또는 "N/A"

                for (int slot0 = 0; slot0 < cassette.SlotCount; slot0++)
                    inputUnit.UiApplySlotEdit(slot0, present: false, waferId: string.Empty);

                // 캐리어 라벨도 갱신
                SetWaferCarrierId("N/A");
                UpdateWaferCount(0);

                view.NotifyCassetteChanged();
            };
        }

        private static int GetPresentWaferCount(MaterialCassette cassette)
        {
            if (cassette == null || cassette.SlotCount <= 0) return 0;

            int count = 0;
            for (int i = 0; i < cassette.SlotCount; i++)
            {
                var w = cassette.GetWafer(i);
                if (w != null && w.Presence == QMC.Common.Material.MaterialPresence.Exist)
                    count++;
            }
            return count;
        }
    }
}
