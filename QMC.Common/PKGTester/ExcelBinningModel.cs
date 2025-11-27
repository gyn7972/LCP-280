using System;
using System.Collections.Generic;

namespace QMC.Common.PKGTester
{
    /// <summary>
    /// 엑셀 한 줄(BIN 하나)에 해당하는 데이터
    /// </summary>
    public class ExcelBinItem
    {
        /// <summary>엑셀의 No</summary>
        public int No { get; set; }

        /// <summary>엑셀의 BIN 번호</summary>
        public int Bin { get; set; }

        /// <summary>Sub BIN (필요 시 사용)</summary>
        public int Sub { get; set; }

        /// <summary>Name (SV700-HA-A-01 등)</summary>
        public string Name { get; set; }

        /// <summary>OP 플래그 (예비용)</summary>
        public string Op { get; set; }

        /// <summary>NG 플래그 (예비용)</summary>
        public string Ng { get; set; }

        /// <summary>
        /// Key : Item 이름 (KELFS, KELDG, VR1, VF3, Watt, WD, ...)
        /// Value : 허용 범위
        /// </summary>
        public Dictionary<string, BinningRange> Items { get; } =
            new Dictionary<string, BinningRange>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 엑셀 전체를 메모리로 보관하는 모델
    /// </summary>
    public class ExcelBinningModel
    {
        /// <summary>아이템 키 목록 (KELFS, KELDG, VR1, ...)</summary>
        public List<string> ItemKeys { get; } = new List<string>();

        /// <summary>보여줄 이름 (보통 ItemKeys와 동일하게 사용)</summary>
        public List<string> ItemDisplayNames { get; } = new List<string>();

        /// <summary>단위 정보 (10uA, 150mA 등)</summary>
        public List<string> ItemUnits { get; } = new List<string>();

        /// <summary>BIN 행 목록</summary>
        public List<ExcelBinItem> Bins { get; } = new List<ExcelBinItem>();

        public void Clear()
        {
            ItemKeys.Clear();
            ItemDisplayNames.Clear();
            ItemUnits.Clear();
            Bins.Clear();
        }
    }
}
