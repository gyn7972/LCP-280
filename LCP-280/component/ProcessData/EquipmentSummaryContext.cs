using QMC.Common;
using System;
using System.Collections.Generic;

namespace QMC.LCP_280.Process.Component.ProcessData
{
    /// <summary>
    /// 장비 단위로 "현재 처리 중인 공정(웨이퍼/빈)"의 Summary 컨텍스트를 단일화.
    /// Input/Output Stage 간 혼돈 방지 목적.
    /// </summary>
    public sealed class EquipmentSummaryContext
    {
        private readonly object _gate = new object();

        // 현재 진행중 Summary (기존 멤버/프로퍼티가 있다면 유지)
        public bool IsActive { get; private set; }
        public WaferSummary Current { get; private set; } = new WaferSummary();

        // [ADD] 완료된 Summary 히스토리
        private readonly List<WaferSummary.WaferTotalSummaryRow> _history = new List<WaferSummary.WaferTotalSummaryRow>();
        private int _historyCapacity = 500; // 필요 시 설정값으로 빼도 됨

        // [ADD] 외부 조회용(스냅샷 복사본)
        public WaferSummary.WaferTotalSummaryRow[] GetHistorySnapshots()
        {
            lock (_gate)
            {
                return _history.ToArray();
            }
        }

        // [ADD] UI가 "현재 진행중"까지 포함해서 보고 싶을 때
        public WaferSummary.WaferTotalSummaryRow[] GetHistoryPlusCurrentSnapshots()
        {
            lock (_gate)
            {
                var list = new List<WaferSummary.WaferTotalSummaryRow>(_history.Count + 1);
                list.AddRange(_history);

                if (IsActive && Current != null)
                {
                    var cur = Current.GetRowSnapshot();
                    if (cur != null) list.Add(cur);
                }

                return list.ToArray();
            }
        }

        // [ADD] wafer 종료 확정 시 호출(Equipment쪽에서 호출하는 위치에 연결 필요)
        public void CommitCurrentToHistoryAndDeactivate()
        {
            lock (_gate)
            {
                if (Current != null)
                {
                    var snap = Current.GetRowSnapshot();
                    if (snap != null)
                    {
                        _history.Add(snap);

                        // capacity 초과 시 앞에서 제거
                        if (_history.Count > _historyCapacity)
                            _history.RemoveRange(0, _history.Count - _historyCapacity);
                    }
                }

                IsActive = false;
                //Current = null; //Reset하고 다시 사용하면 됨.
            }
        }

        public string ActiveWaferId { get; private set; }
        public string ActiveBinId { get; private set; }
        public string ActiveMachineName { get; private set; }

        public void Begin(string waferId, string binId, string machineName)
        {
            lock (_gate)
            {
                if (string.IsNullOrWhiteSpace(waferId))
                    waferId = "UNKNOWN";
                if (string.IsNullOrWhiteSpace(binId))
                    binId = waferId;

                ActiveWaferId = waferId;
                ActiveBinId = binId;
                ActiveMachineName = machineName;

                if (Current == null)
                {
                    Current = new WaferSummary();
                }

                Current.BeginTotalSummary(waferId: waferId, binId: binId, machineName: machineName);
                IsActive = true;
            }
        }

        public void End()
        {
            lock (_gate)
            {
                if (!IsActive)
                    return;

                try
                {
                    Current.EndTotalSummary();
                }
                finally
                {
                    IsActive = false;
                }
            }
        }

        public WaferSummary GetCurrentSummaryOrNull()
        {
            lock (_gate)
            {
                return Current;
            }
        }

        public WaferSummary.WaferTotalSummaryRow GetSnapshotOrNull()
        {
            lock (_gate)
            {
                return Current?.GetRowSnapshot();
            }
        }

        public void AddAlarm(int delta = 1)
        {
            lock (_gate)
            {
                Current?.AddAlarmCount(delta);
            }
        }
    }
}