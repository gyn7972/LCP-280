using QMC.Common;
using QMC.Common.GEMSecs;
using System;
using System.Globalization;

namespace QMC.LCP_280.Process
{
    /*
     * 앞으로는 Equipment.Instance에서 GemService.SetVariableValue(...)를 직접 부르지 말고, 
     * 브리지 메서드를 호출하는 방식으로 정리됩니다.
    예:
    •	바코드 읽힘: _gemBridge.ReportBcrReadComplete(lotId, portId)
    •	레시피 선택: _gemBridge.UpdatePpid(ppid)
    •	LOT 결정: _gemBridge.UpdateLot(lotId)
    •	공정 시작/끝: _gemBridge.ReportProcessStart() / ReportProcessEnd()
     */
    public sealed class EquipmentGemBridge : IDisposable
    {
        private readonly Equipment _eq;
        private bool _disposed;

        private short _lastControlState = (short)XLinkGemService.Define.CONTROL_STATE.CONTROL_UNKOWN;

        public EquipmentGemBridge(Equipment equipment)
        {
            _eq = equipment ?? throw new ArgumentNullException(nameof(equipment));
        }

        public void Attach()
        {
            _eq.StateChanged += OnEquipmentStateChanged;

            if (_eq.GemService != null)
            {
                _eq.GemService.ControlStateChanged += OnGemControlStateChanged;
                _eq.GemService.AlarmEventReceived += OnGemAlarmEventReceived;
                _eq.GemService.ErrorEventReceived += OnGemErrorEventReceived;
            }

            // 초기 스냅샷 1회
            PublishEquipmentState("Attach");
        }

        public void Detach()
        {
            _eq.StateChanged -= OnEquipmentStateChanged;

            if (_eq.GemService != null)
            {
                _eq.GemService.ControlStateChanged -= OnGemControlStateChanged;
                _eq.GemService.AlarmEventReceived -= OnGemAlarmEventReceived;
                _eq.GemService.ErrorEventReceived -= OnGemErrorEventReceived;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Detach();
        }

        // =========================
        // 외부(시퀀스/유닛)에서 호출할 API
        // =========================
        public void UpdateLot(string lotId)
        {
            if (!IsGemReady()) return;

            Safe(() =>
            {
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_LOT_ID, lotId ?? "");
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CURRENT_TIME, _eq.GemService.GetCurrentTimeString());
            });
        }

        public void UpdatePpid(string ppid, bool reportSelected = true)
        {
            if (!IsGemReady()) return;

            Safe(() =>
            {
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_PPID, ppid ?? "");
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CURRENT_TIME, _eq.GemService.GetCurrentTimeString());

                if (reportSelected)
                    _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_PPSELECED);
            });
        }

        public void ReportBcrReadComplete(string lotId = null, string portId = null)
        {
            if (!IsGemReady()) return;

            Safe(() =>
            {
                if (!string.IsNullOrWhiteSpace(lotId))
                    _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_LOT_ID, lotId);
                if (!string.IsNullOrWhiteSpace(portId))
                    _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_PORT_ID, portId);

                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CURRENT_TIME, _eq.GemService.GetCurrentTimeString());
                _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_BCR_READ_COMPLETE);
            });
        }

        public void ReportProcessStart()
        {
            if (!IsGemReady()) return;

            Safe(() =>
            {
                var now = _eq.GemService.GetCurrentTimeString();
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_START_TIME, now);
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CURRENT_TIME, now);
                _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_PROCESS_START);
            });
        }

        public void ReportProcessEnd()
        {
            if (!IsGemReady()) return;

            Safe(() =>
            {
                var now = _eq.GemService.GetCurrentTimeString();
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_END_TIME, now);
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CURRENT_TIME, now);
                _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_PROCESS_END);
            });
        }

        // =========================
        // Equipment/GEM 이벤트 핸들러
        // =========================
        private void OnEquipmentStateChanged(object sender, EquipmentStateChangedEventArgs e)
            => PublishEquipmentState($"EqState:{e.OldState}->{e.NewState}");

        private void OnGemControlStateChanged(object sender, GemControlStateChangedEventArgs e)
        {
            _lastControlState = e.ControlState;

            if (!IsGemReady()) return;

            Safe(() =>
            {
                _eq.GemService.SetVariableValue(
                    XLinkGemService.Define.SVID.SVID_CONTROL_STATE,
                    e.ControlState.ToString(CultureInfo.InvariantCulture));

                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CURRENT_TIME, _eq.GemService.GetCurrentTimeString());
                _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_CONTROL_STATUS);
            });

            // 상세 ControlState CEID
            ReportControlStateDetail(e.ControlState);
        }

        private void OnGemAlarmEventReceived(object sender, GemAlarmEventArgs e)
        {
            if (!IsGemReady()) return;

            Safe(() =>
            {
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_ALID, e.ALID.ToString(CultureInfo.InvariantCulture));
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_ALCD, e.ALCD.ToString(CultureInfo.InvariantCulture));
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_ALTX, "");
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CURRENT_TIME, _eq.GemService.GetCurrentTimeString());
            });
        }

        private void OnGemErrorEventReceived(object sender, GemErrorEventArgs e)
        {
            Log.Write("GEM", $"Error {e.ErrorCode}: {e.ErrorText}");
        }

        // =========================
        // 내부 유틸
        // =========================
        private void PublishEquipmentState(string reason)
        {
            if (!IsGemReady()) return;

            Safe(() =>
            {
                var cs = _lastControlState;
                if (cs <= 0) cs = (short)MapEquipmentStateToControlState(_eq.EqState);

                var ms = MapEquipmentStateToMachineState(_eq.EqState);

                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CONTROL_STATE, cs.ToString(CultureInfo.InvariantCulture));
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_EQ_STATE, ms.ToString(CultureInfo.InvariantCulture));
                _eq.GemService.SetVariableValue(XLinkGemService.Define.SVID.SVID_CURRENT_TIME, _eq.GemService.GetCurrentTimeString());

                _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_CONTROL_STATUS);
                _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_MACHINE_STATUS);
            });

            ReportControlStateDetail(_lastControlState);
        }

        private void ReportControlStateDetail(short controlState)
        {
            if (!IsGemReady()) return;

            Safe(() =>
            {
                if (controlState == (short)XLinkGemService.Define.CONTROL_STATE.CONTROL_EQUIPMENT_OFFLINE ||
                    controlState == (short)XLinkGemService.Define.CONTROL_STATE.CONTROL_HOST_OFFLINE)
                    _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_CONTROLSTATE_OFFLINE);
                else if (controlState == (short)XLinkGemService.Define.CONTROL_STATE.CONTROL_ONLINE_LOCAL)
                    _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_CONTROLSTATE_LOCAL);
                else if (controlState == (short)XLinkGemService.Define.CONTROL_STATE.CONTROL_ONLINE_REMOTE)
                    _eq.GemService.EventReport(XLinkGemService.Define.CEID.CEID_CONTROLSTATE_REMOTE);
            });
        }

        private bool IsGemReady()
            => _eq.GemConfig != null && _eq.GemConfig.Enable && _eq.GemService != null;

        private static void Safe(Action action)
        {
            try { action(); } catch { }
        }

        private static XLinkGemService.Define.CONTROL_STATE MapEquipmentStateToControlState(EquipmentState st)
        {
            switch (st)
            {
                case EquipmentState.AutoRunning:
                    return XLinkGemService.Define.CONTROL_STATE.CONTROL_ONLINE_REMOTE;
                case EquipmentState.Ready:
                case EquipmentState.ManualRunning:
                case EquipmentState.CycleStop:
                case EquipmentState.Starting:
                case EquipmentState.Stopping:
                    return XLinkGemService.Define.CONTROL_STATE.CONTROL_ONLINE_LOCAL;
                default:
                    return XLinkGemService.Define.CONTROL_STATE.CONTROL_EQUIPMENT_OFFLINE;
            }
        }

        private static int MapEquipmentStateToMachineState(EquipmentState st)
        {
            switch (st)
            {
                case EquipmentState.AutoRunning:
                case EquipmentState.ManualRunning:
                    return 1; // Run
                case EquipmentState.Ready:
                case EquipmentState.CycleStop:
                    return 2; // Idle
                default:
                    return 0; // Down
            }
        }
    }
}