using System;
using System.Collections.Generic;
using System.Drawing;

namespace SP_GridTypeView
{
    public enum MappingDirection
    {
        TopToBottom,
        BottomToTop
    }

    public class CassetteDataConfig
    {
        // 슬롯 개수
        public int SlotCount { get; set; }
        // 전체 매핑 시작점
        public Point MappingStartPoint { get; set; }
        // 전체 매핑 끝점
        public Point MappingEndPoint { get; set; }
        // 전체 오프셋
        public Point CommonOffset { get; set; }
        // 매핑 방향
        public MappingDirection MappingDirection { get; set; } = MappingDirection.TopToBottom;
        // 기타 확장 파라미터
        public Dictionary<string, object> ExtraParameters { get; set; } = new Dictionary<string, object>();
    }
}
