using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class InputStageEjectorConfig : BaseConfig
    {
        public enum TeachingPositionName
        {
            EjectBlockUp,
            EjectBlokReady,
            EjectPinOffset,
            EjectPinReady,
            EjectPinChange
            // 필요시 추가
        }

        public List<QMC.LCP_280.Process.Component.TeachingPosition> TeachingPositions { get; set; } = new List<QMC.LCP_280.Process.Component.TeachingPosition>();

        // Offsets (dx, dz1, dz2) -> we map per axis name; store dt for rotational placeholder (not used)
        public Dictionary<string, (double dzEjector, double dzPin)> Offsets { get; set; } = new Dictionary<string, (double dzEjector, double dzPin)>();

        public bool EnablePredictiveControl { get; set; } = false;
        public double MoveDoneRemainDistance { get; set; } = 0.005;

        public InputStageEjectorConfig() : base("InputStageEjectorConfig") { }

        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<QMC.LCP_280.Process.Component.TeachingPosition>();
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (TeachingPositions.FirstOrDefault(p => p.Name == posName) == null)
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "EJECTOR_Z", 0.0 },
                        { "EJECT_PIN_Z", 0.0 }
                    };
                    TeachingPositions.Add(new QMC.LCP_280.Process.Component.TeachingPosition(posName, axisPositions, $"Default {posName} Position"));
                }
                if (!Offsets.ContainsKey(posName)) Offsets[posName] = (0, 0);
            }
            Saveconfig();
        }

        public void SetTeachingPosition(QMC.LCP_280.Process.Component.TeachingPosition tp)
        {
            var exist = TeachingPositions.FirstOrDefault(p => p.Name == tp.Name);
            if (exist != null)
            {
                exist.AxisPositions = tp.AxisPositions;
                exist.Description = tp.Description;
                exist.ExtraInfo = tp.ExtraInfo;
            }
            else TeachingPositions.Add(tp);
            if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = (0, 0);
            Saveconfig();
        }

        public QMC.LCP_280.Process.Component.TeachingPosition GetTeachingPosition(string name)
            => TeachingPositions.FirstOrDefault(p => p.Name == name);

        public (double z, double pinZ) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0);
            double z = tp.AxisPositions.TryGetValue("EJECTOR_Z", out var vz) ? vz : 0;
            double pz = tp.AxisPositions.TryGetValue("EJECT_PIN_Z", out var vpz) ? vpz : 0;
            if (Offsets.TryGetValue(name, out var off))
            {
                z += off.dzEjector; pz += off.dzPin;
            }
            return (z, pz);
        }

        public void SetOffset(string name, double dzEjector, double dzPin)
        {
            Offsets[name] = (dzEjector, dzPin);
            Saveconfig();
        }

        public int Saveconfig()
        {
            var pure = TeachingPositions
                .Select(tp => new QMC.LCP_280.Process.Component.TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions;
            TeachingPositions = pure;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load();
            if (rc != 0) return rc;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            foreach (var tp in TeachingPositions)
                if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = (0, 0);
            return 0;
        }
    }
}