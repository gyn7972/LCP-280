using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class InputDieTransferConfig : BaseConfig
    {
        public enum TeachingPositionName
        {
            Loading,
            Unloading,
            Ready,
            Home,
            Pick,
            Place
        }

        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        // Offset (X,Y,T or Z axes depending) : positionName -> (t, pickZ, placeZ) but we keep generic dictionary with axis offsets
        // Use same pattern as InputStage: store (dx, dy, dt) although here axes are T, PickZ, PlaceZ; we map generically by axis name.
        public Dictionary<string, Dictionary<string, double>> Offsets { get; set; } = new Dictionary<string, Dictionary<string, double>>();

        // Predictive control flags (mirroring InputStage)
        public bool EnablePredictiveControl { get; set; } = false;
        public double MoveDoneRemainDistance { get; set; } = 0.005;

        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hard_inputs_backup = null; // reserved
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = "LEFT TOOL AIR TANK PRESSURE CHECK",    Disp = "X032" },
            new HardInputDef { No = 2, Name = "LEFT TOOL VACUUM TANK PRESSURE CHECK", Disp = "X033" },
            new HardInputDef { No = 3, Name = "LEFT TOOL ARM 1 FLOW CHECK",           Disp = "X034" },
            new HardInputDef { No = 4, Name = "LEFT TOOL ARM 2 FLOW CHECK",           Disp = "X035" },
            new HardInputDef { No = 5, Name = "LEFT TOOL ARM 3 FLOW CHECK",           Disp = "X036" },
            new HardInputDef { No = 6, Name = "LEFT TOOL ARM 4 FLOW CHECK",           Disp = "X037" }
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1,  Name = "LEFT ARM 1 VACUUM", Disp = "Y039" },
            new HardOutputDef { No = 2,  Name = "LEFT ARM 2 VACUUM", Disp = "Y040" },
            new HardOutputDef { No = 3,  Name = "LEFT ARM 3 VACUUM", Disp = "Y041" },
            new HardOutputDef { No = 4,  Name = "LEFT ARM 4 VACUUM", Disp = "Y042" },
            new HardOutputDef { No = 5,  Name = "LEFT ARM 1 BLOW",   Disp = "Y043" },
            new HardOutputDef { No = 6,  Name = "LEFT ARM 2 BLOW",   Disp = "Y044" },
            new HardOutputDef { No = 7,  Name = "LEFT ARM 3 BLOW",   Disp = "Y045" },
            new HardOutputDef { No = 8,  Name = "LEFT ARM 4 BLOW",   Disp = "Y046" },
            new HardOutputDef { No = 9,  Name = "LEFT ARM 1 VENT",   Disp = "Y047" },
            new HardOutputDef { No = 10, Name = "LEFT ARM 2 VENT",   Disp = "Y048" },
            new HardOutputDef { No = 11, Name = "LEFT ARM 3 VENT",   Disp = "Y049" },
            new HardOutputDef { No = 12, Name = "LEFT ARM 4 VENT",   Disp = "Y050" }
        };

        public InputDieTransferConfig() : base("InputDieTransferConfig") { }

        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (TeachingPositions.Find(p => p.Name == posName) == null)
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Left Tool T Axis", 0.0 },
                        { "Left Pick Z Axis", 50.0 },
                        { "Left Place Z Axis", 50.0 },
                    };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"Default {posName} Position"));
                }
                if (!Offsets.ContainsKey(posName))
                {
                    Offsets[posName] = new Dictionary<string, double>();
                }
            }
            Saveconfig();
        }

        public void SetTeachingPosition(TeachingPosition tp)
        {
            var exist = TeachingPositions.FirstOrDefault(p => p.Name == tp.Name);
            if (exist != null)
            {
                exist.AxisPositions = tp.AxisPositions;
                exist.Description = tp.Description;
                exist.ExtraInfo = tp.ExtraInfo;
            }
            else TeachingPositions.Add(tp);
            if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = new Dictionary<string, double>();
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

        public (double t, double pickZ, double placeZ) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0, 0);
            double t = tp.AxisPositions.TryGetValue("Left Tool T Axis", out var vt) ? vt : 0;
            double pz = tp.AxisPositions.TryGetValue("Left Pick Z Axis", out var vpz) ? vpz : 0;
            double plz = tp.AxisPositions.TryGetValue("Left Place Z Axis", out var vplz) ? vplz : 0;
            if (Offsets.TryGetValue(name, out var offDict))
            {
                if (offDict.TryGetValue("Left Tool T Axis", out var ot)) t += ot;
                if (offDict.TryGetValue("Left Pick Z Axis", out var opz)) pz += opz;
                if (offDict.TryGetValue("Left Place Z Axis", out var oplz)) plz += oplz;
            }
            return (t, pz, plz);
        }

        public void SetOffset(string name, string axisName, double delta)
        {
            if (!Offsets.ContainsKey(name)) Offsets[name] = new Dictionary<string, double>();
            Offsets[name][axisName] = delta;
            Saveconfig();
        }

        public int Saveconfig()
        {
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions;
            TeachingPositions = purePositions;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = Load();
            if (result != 0) return result;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            foreach (var tp in TeachingPositions)
                if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = new Dictionary<string, double>();
            return 0;
        }
    }
}