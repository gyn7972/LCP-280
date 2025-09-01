using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class InputRingTransferConfig : BaseConfig
    {
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        public InputRingTransferConfig() : base("InputRingTransferConfig")
        {
        }

        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            var existingNames = new HashSet<string>(TeachingPositions.Select(tp => tp.Name));
            foreach (string posName in new[] { "Loading", "Unloading", "Ready", "Home" })
            {
                var tp = TeachingPositions.FirstOrDefault(p => p.Name == posName);
                if (tp == null)
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Axis X", 0.0 },
                        { "Axis Y", 0.0 },
                        { "Axis Z", 0.0 }
                    };
                    tp = new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치");
                    TeachingPositions.Add(tp);
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
            else
            {
                TeachingPositions.Add(tp);
            }
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name)
            => TeachingPositions.FirstOrDefault(p => p.Name == name);

        public int Saveconfig()
        {
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var backup = TeachingPositions;
            TeachingPositions = purePositions;
            int result = base.Save();
            TeachingPositions = backup;
            return result;
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = base.Load();
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager);
            return result;
        }
    }
}
