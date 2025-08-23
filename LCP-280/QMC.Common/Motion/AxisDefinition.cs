using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common
{
    /// <summary>
    /// 하나의 HW 축과 그 축의 PositionItem(PropertyPosition) 목록 관리
    /// </summary>
    public class AxisDefinition
    {
        public IMotionAxis MotionAxis { get; }
        public string DisplayName { get; }
        public string AxisKey { get; }   // 내부 식별자 (예: "X","Y","Z","T" 또는 "Z1")
        public List<PropertyPosition> PositionItems { get; } = new List<PropertyPosition>();

        public AxisDefinition(string axisKey, string displayName, IMotionAxis motionAxis)
        {
            AxisKey = axisKey;
            DisplayName = displayName;
            MotionAxis = motionAxis;
        }

        /// <summary>
        /// PositionItem 생성 (UI 표시용 논리 이름 기반)
        /// </summary>
        public PropertyPosition CreatePositionItem(string logicalName,
                                                   double positionValue = 0,
                                                   double velocity = 50,
                                                   double acc = 500,
                                                   double dec = 500,
                                                   int timeoutMs = 3000)
        {
            // Title = "Lifter Loading Position" 형태
            var itemTitle = logicalName;
            var pp = new PropertyPosition(itemTitle, logicalName + " position", AxisKey, MotionAxis.Unit, true);

            // Property 이름 규칙: 축표시 / 속성
            // 첫 번째 DoubleProperty 는 UI Editor 에서 기본 Position 으로 사용
            pp.AddDoubleProperty(MotionAxis.Name, positionValue);
            pp.AddDoubleProperty("Velocity", velocity);
            pp.AddDoubleProperty("Acceleration", acc);
            pp.AddDoubleProperty("Deceleration", dec);
            pp.AddDoubleProperty("TimeoutMs", timeoutMs);

            PositionItems.Add(pp);
            return pp;
        }

        /// <summary>
        /// PositionItem 생성 (강한 키 + UI 표시용 논리 이름)
        /// </summary>
        public PropertyPosition CreatePositionItem(string key,
                                                   string logicalName,
                                                   double positionValue,
                                                   double velocity,
                                                   double acc,
                                                   double dec,
                                                   int timeoutMs)
        {
            var pp = CreatePositionItem(logicalName, positionValue, velocity, acc, dec, timeoutMs);
            pp.Key = key;
            return pp;
        }

        public PropertyPosition GetPositionItem(string title)
        {
            return PositionItems.FirstOrDefault(p => p.Title == title);
        }

        public PropertyPosition GetPositionItemByKey(string key)
        {
            return PositionItems.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
        }
    }
}