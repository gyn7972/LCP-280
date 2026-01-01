using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motions;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Unit
{
    [Serializable]
    public sealed class IndexChipProbeControllerRecipe : QMC.Common.BaseRecipe
    {
        public enum TeachingPositionName
        {
            Top_Index1_Contact,
            Top_Index1_Ready,
            Top_Index2_Contact,
            Top_Index2_Ready,
            Top_Index3_Contact,
            Top_Index3_Ready,
            Top_Index4_Contact,
            Top_Index4_Ready,
            Top_Index5_Contact,
            Top_Index5_Ready,
            Top_Index6_Contact,
            Top_Index6_Ready,
            Top_Index7_Contact,
            Top_Index7_Ready,
            Top_Index8_Contact,
            Top_Index8_Ready,
            Bottom_Index1_Contact,
            Bottom_Index1_Ready,
            Bottom_Index2_Contact,
            Bottom_Index2_Ready,
            Bottom_Index3_Contact,
            Bottom_Index3_Ready,
            Bottom_Index4_Contact,
            Bottom_Index4_Ready,
            Bottom_Index5_Contact,
            Bottom_Index5_Ready,
            Bottom_Index6_Contact,
            Bottom_Index6_Ready,
            Bottom_Index7_Contact,
            Bottom_Index7_Ready,
            Bottom_Index8_Contact,
            Bottom_Index8_Ready,
            GripperX_Ready,
            GripperX_Clamp,
            GripperX_Index_Contact,
            SphereZ_Ready,
            SphereZ_Measure,
            SafetyZone
        }

        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap =
            new Dictionary<TeachingPositionName, string[]>
            {
                { TeachingPositionName.Top_Index1_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index1_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index2_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index2_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index3_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index3_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index4_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index4_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index5_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index5_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index6_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index6_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index7_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index7_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index8_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index8_Ready,     new [] { AxisNames.ProbeZ } },

                { TeachingPositionName.Bottom_Index1_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index1_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index2_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index2_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index3_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index3_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index4_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index4_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index5_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index5_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index6_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index6_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index7_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index7_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index8_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index8_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },

                { TeachingPositionName.GripperX_Ready,       new [] { AxisNames.GripperX } },
                { TeachingPositionName.GripperX_Clamp,       new [] { AxisNames.GripperX } },
                { TeachingPositionName.GripperX_Index_Contact,    new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.SphereZ_Ready,        new [] { AxisNames.SphereZ } },
                { TeachingPositionName.SphereZ_Measure,         new [] { AxisNames.SphereZ } },
                { TeachingPositionName.SafetyZone,           new [] { AxisNames.ProbeCardZ, AxisNames.ProbeZ } },
            };

        [Category("Teaching"), DisplayName("Positions")]
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        public IndexChipProbeControllerRecipe() : this(null) 
        { 

        }
        public IndexChipProbeControllerRecipe(string name = null) : base(name) 
        { 

        }

        public string GetBottomContactName(int index0Based)
        {
            return GetIndexedName("Bottom", index0Based, "Contact");
        }

        public string GetBottomReadyName(int index0Based)
        {
            return GetIndexedName("Bottom", index0Based, "Ready");
        }

        public string GetTopContactName(int index0Based)
        {
            return GetIndexedName("Top", index0Based, "Contact");
        }

        public string GetTopReadyName(int index0Based)
        {
            return GetIndexedName("Top", index0Based, "Ready");
        }

        private static string GetIndexedName(string prefix, int index0Based, string suffix)
        {
            if (index0Based < 0 || index0Based >= 8)
                throw new ArgumentOutOfRangeException(nameof(index0Based), "index0Based must be 0..7");

            // enum은 1-base 이름 규칙을 사용함: Bottom_Index1_Contact ...
            int idx1 = index0Based + 1;
            string enumName = $"{prefix}_Index{idx1}_{suffix}";

            TeachingPositionName en;
            if (!Enum.TryParse(enumName, out en) || !Enum.IsDefined(typeof(TeachingPositionName), en))
                throw new InvalidOperationException($"Invalid teaching enum name: '{enumName}'");

            return en.ToString();
        }

        public bool GetTeachingPositionName(int selIndex, out string name)
        {
            if (Enum.GetNames(typeof(TeachingPositionName)).Length <= selIndex)
            {
                name = "None";
                return false;
            }
            var tpn = (TeachingPositionName)selIndex;
            name = tpn.ToString();
            return true;
        }


        public TeachingPosition Get(string name)
        {
            return TeachingPositions?.FirstOrDefault
                (p => p != null && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) 
                return new List<string>();

            TeachingPositionName en;
            if (Enum.TryParse(positionName, out en))
            {
                string[] arr;
                if (_axisMap.TryGetValue(en, out arr)) return arr;
            }

            // 백워드 호환 기본값
            return new[] { AxisNames.ProbeZ, AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ, AxisNames.SphereZ };
        }

        public void ApplyAxisMapping()
        {
            if (TeachingPositions == null) 
                return;

            foreach (var tp in TeachingPositions)
            {
                if (tp == null) 
                    continue;

                var allowed = new HashSet<string>(GetAxisNamesForPosition(tp.Name), StringComparer.OrdinalIgnoreCase);
                var current = tp.AxisPositions ?? new Dictionary<string, double>();
                var next = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

                foreach (var axis in allowed)
                {
                    double v;
                    if (current.TryGetValue(axis, out v)) next[axis] = v;
                    else next[axis] = 0.0;
                }

                tp.AxisPositions = next;
            }
        }

        public void InitializeDefaultTeachingPositions(bool save = true)
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();

            var existing = new HashSet<string>(TeachingPositions.Where(t => t != null).Select(t => t.Name), StringComparer.OrdinalIgnoreCase);

            foreach (TeachingPositionName name in Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (!existing.Contains(posName))
                {
                    var axes = GetAxisNamesForPosition(posName);
                    var axisPositions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                    foreach (var a in axes) axisPositions[a] = 0.0;

                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치"));
                }
            }

            ApplyAxisMapping();

            if (save)
                RecipeManager.Save(this);
        }

        private static Dictionary<string, object> FilterExtraInfo(Dictionary<string, object> src)
        {
            var dst = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (src == null) return dst;

            foreach (var kv in src)
            {
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;
                var v = kv.Value;
                if (v == null) { dst[kv.Key] = null; continue; }

                var t = v.GetType();
                if (t.IsEnum || v is string || v is bool ||
                    v is int || v is long || v is float || v is double || v is decimal)
                {
                    dst[kv.Key] = v;
                }
                else
                {
                    // 복잡 객체는 문자열로만 남김(폭증 방지)
                    dst[kv.Key] = v.ToString();
                }
            }

            return dst;
        }

        public void UpsertFiltered(TeachingPosition tp, bool save = true)
        {
            if (tp == null || string.IsNullOrWhiteSpace(tp.Name))
                return;

            var allowed = new HashSet<string>(GetAxisNamesForPosition(tp.Name), StringComparer.OrdinalIgnoreCase);

            var filtered = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var axis in allowed)
            {
                double v = 0.0;
                if (tp.AxisPositions != null)
                    tp.AxisPositions.TryGetValue(axis, out v);
                filtered[axis] = v;
            }

            if (TeachingPositions == null)
                TeachingPositions = new List<TeachingPosition>();

            var exist = TeachingPositions.FirstOrDefault(p => p != null && string.Equals(p.Name, tp.Name, StringComparison.OrdinalIgnoreCase));
            if (exist == null)
            {
                tp.AxisPositions = filtered;
                tp.ExtraInfo = FilterExtraInfo(tp.ExtraInfo);
                TeachingPositions.Add(tp);
            }
            else
            {
                exist.AxisPositions = filtered;
                exist.Description = tp.Description;
                exist.ExtraInfo = FilterExtraInfo(tp.ExtraInfo);
            }

            ApplyAxisMapping();

            if (save)
                RecipeManager.Save(this);
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            // Recipe는 BaseRecipe.Load()가 파일 없으면 -1 반환할 수 있음.
            // 없으면 기본 생성 후 저장까지 해두는 쪽이 Teaching UX가 안정적임.
            var filePath = GetFilePath();
            var existed = System.IO.File.Exists(filePath);

            var rc = Load();
            Log.Write(nameof(IndexChipProbeControllerRecipe), nameof(LoadAndBindAxes),
                    $"Name='{Name}', file='{filePath}', existedBefore={existed}, LoadRc={rc}");

            if (rc != 0)
            {
                // 파일이 없으면 현재 Name으로 신규 생성
                TeachingPositions = TeachingPositions ?? new List<TeachingPosition>();
                InitializeDefaultTeachingPositions(save: true);
                
                Log.Write(nameof(IndexChipProbeControllerRecipe), nameof(LoadAndBindAxes),
                    $"Default teaching created & saved. Name='{Name}', file='{filePath}'");
            }

            // 누락 보강/정렬
            var byName = new Dictionary<string, TeachingPosition>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in TeachingPositions ?? new List<TeachingPosition>())
            {
                if (t == null || string.IsNullOrWhiteSpace(t.Name)) continue;
                if (!byName.ContainsKey(t.Name)) byName[t.Name] = t;
            }

            var rebuilt = new List<TeachingPosition>();
            foreach (TeachingPositionName en in Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = en.ToString();

                TeachingPosition tp;
                if (byName.TryGetValue(posName, out tp) && tp != null)
                {
                    rebuilt.Add(tp);
                }
                else
                {
                    var axes = GetAxisNamesForPosition(posName);
                    var axisPositions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                    foreach (var a in axes) axisPositions[a] = 0.0;
                    rebuilt.Add(new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치"));
                }
            }

            if (TeachingPositions == null)
                TeachingPositions = new List<TeachingPosition>();

            TeachingPositions.Clear();
            TeachingPositions.AddRange(rebuilt);

            ApplyAxisMapping();

            if (axisManager != null)
            {
                foreach (var tp in TeachingPositions)
                    tp.BindAxes(axisManager, "Unit");
            }

            return 0;
        }

        public override void Reset()
        {
            if (TeachingPositions == null)
                TeachingPositions = new List<TeachingPosition>();
        }

        public override bool Validate()
        {
            if (TeachingPositions == null) return true;
            return TeachingPositions.All(p => p != null && !string.IsNullOrWhiteSpace(p.Name));
        }

        protected override void OnSaving()
        {
            base.OnSaving();

            if (TeachingPositions == null) 
                return;

            foreach (var tp in TeachingPositions)
            {
                if (tp?.Axes != null)
                    tp.Axes.Clear(); // 런타임 캐시 제거(파일에 저장되면 안됨)
            }
        }

    }
}
