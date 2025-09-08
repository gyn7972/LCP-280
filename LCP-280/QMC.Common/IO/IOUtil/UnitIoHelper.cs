using System;
using System.Linq;
using QMC.Common.IO;
using QMC.Common.DIO;

namespace QMC.Common.IOUtil
{
    /// <summary>
    /// 공통 I/O 접근 Helper.
    /// 채널 '이름'(설정 JSON상의 Name)으로 입력/출력을 읽거나 기록한다.
    /// 여러 Unit에서 동일한 루틴(모듈/디스플레이번호 탐색 → DioScan 호출)을 반복하지 않도록 통합.
    /// </summary>
    public static class UnitIoHelper
    {
        /// <summary>
        /// 입력 채널 논리 이름으로 현재 값을 읽는다.
        /// </summary>
        /// <param name="channelName">DIO 설정상의 채널 Name (대소문자 무시)</param>
        /// <param name="value">읽은 논리 값</param>
        /// <returns>성공 여부</returns>
        public static bool TryReadInput(string channelName, out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(channelName)) return false;
            var eq = EquipmentLocator.Instance as IEquipment;
            var unit = eq?.UnitIO; var scan = eq?.DioScan;
            if (unit == null || scan == null) return false;

            var ci = FindChannel(unit, channelName, isOutput:false);
            if (ci == null) return false;
            return scan.TryGetInput(ci.ModuleName, ci.DisplayNo, out value);
        }

        /// <summary>
        /// 출력 채널 논리 이름으로 On/Off 설정.
        /// </summary>
        public static bool TryWriteOutput(string channelName, bool on)
        {
            if (string.IsNullOrWhiteSpace(channelName)) return false;
            var eq = EquipmentLocator.Instance as IEquipment;
            var unit = eq?.UnitIO; var scan = eq?.DioScan;
            if (unit == null || scan == null) return false;
            var co = FindChannel(unit, channelName, isOutput: true);
            if (co == null) return false;
            return scan.WriteOutput(co.ModuleName, co.DisplayNo, on) == 0;
        }

        private sealed class ChannelInfo
        {
            public string ModuleName; public string DisplayNo;
        }

        private static ChannelInfo FindChannel(DIOUnit unit, string channelName, bool isOutput)
        {
            foreach (var m in unit.Modules ?? Enumerable.Empty<DIOModuleSetup>())
            {
                var list = isOutput ? m.Outputs : m.Inputs;
                if (list == null) continue;
                var found = list.FirstOrDefault(c => c.Name != null && c.Name.Equals(channelName, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                    return new ChannelInfo { ModuleName = m.ModuleName, DisplayNo = found.DisplayNo };
            }
            return null;
        }
    }
}
