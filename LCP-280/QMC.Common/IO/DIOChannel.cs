using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.IO
{
    public enum DIOType { Input = 0, Output = 1 }
    public enum ActiveLevel { Low = 0, High = 1 }   // 논리 ON 기준

    /// <summary>DI/DO 채널 1개에 대한 Setup (UI 표 한 행 + 하단 Property)</summary>
    [Serializable]
    public sealed class DIOChannel
    {
        [Browsable(false)]
        public bool IsOutput { get; set; }   // false=DI, true=DO

        [Category("Mapping"), DisplayName("Index")]
        public int Index { get; set; }       // 0,1,2,...

        [Category("Mapping"), DisplayName("Display No")]
        public string DisplayNo { get; set; }  // e.g. X00, Y03 (UI 표기용)

        [Category("Property"), DisplayName("Name")]
        public string Name { get; set; } = "";

        [Category("Property"), DisplayName("Monitoring")]
        public bool Monitoring { get; set; } = true;

        [Category("Property"), DisplayName("Reverse")]
        public bool Reverse { get; set; } = false;

        // 필요 시 하드웨어 매핑
        [Browsable(false)] public int BoardNo { get; set; }
        [Browsable(false)] public int PortNo { get; set; }
        [Browsable(false)] public int ChannelNo { get; set; }

        public static DIOChannel Create(bool isOutput, int index, string displayNo, string name)
        {
            var it = new DIOChannel();
            it.IsOutput = isOutput;
            it.Index = index;
            it.DisplayNo = displayNo;
            it.Name = string.IsNullOrEmpty(name) ? (displayNo + " Item Name") : name;
            it.BoardNo = 0; it.PortNo = 0; it.ChannelNo = index; // 기본 채널 매핑
            return it;
        }
    }
}
