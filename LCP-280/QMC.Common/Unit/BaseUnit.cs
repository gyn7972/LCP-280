// QMC.Common\Unit\BaseUnit.cs
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using System.Collections.Generic;

namespace QMC.Common.Unit
{
    public abstract class BaseUnit
    {
        public string UnitName { get; set; }
        public List<BaseComponent> Components { get; } = new List<BaseComponent>();
        public BaseConfig Config { get; internal set; }

        // 공용 축 컴포넌트
        public Dictionary<string, MotionAxis> Axes { get; } = new Dictionary<string, MotionAxis>();
        
        // 공용 티칭 위치 관리
        public Dictionary<string, double> TeachingPositions { get; } = new Dictionary<string, double>();

        protected BaseUnit(string unitName = null)
        {
            UnitName = unitName;
        }

        public virtual void AddComponents() { }

        // 축 이동
        public virtual int MoveAxis(string axisKey, double pos, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            if (Axes.TryGetValue(axisKey, out var axis))
                return axis.MoveAbs(pos, vel, acc, dec, jerk);
            return -1;
        }

        // 티칭 위치 저장/로드
        public virtual void SetTeachingPosition(string key, double pos)
        {
            TeachingPositions[key] = pos;
        }
        public virtual double GetTeachingPosition(string key, double defaultValue = 0)
        {
            if (TeachingPositions.TryGetValue(key, out var pos))
                return pos;
            return defaultValue;
        }

        public void BindAxis(MotionAxisManager mgr, string unitName, string axisName, ref MotionAxis field)
        {
            if (mgr.TryGet(unitName, axisName, out var axis) && axis != null)
            {
                field = axis;
                Axes[axisName] = axis; // Axes 딕셔너리에 일관 등록
            }
            else
            {
                if (Axes.ContainsKey(axisName))
                    Axes.Remove(axisName);
                Log.Write("UnitAxis", $"[BindAxes] Axis '{unitName}||{axisName}' 미존재");
            }
        }

        // Unit 공통 동작 메서드
        public virtual void OnRun() { }
        public virtual void OnStop() { }
    }
}