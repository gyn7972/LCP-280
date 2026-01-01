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
    public sealed class IndexLoadAlignerRecipe : QMC.Common.BaseRecipe
    {
        public enum TeachingPositionName
        {
            AlignZ_Index1_Up,
            AlignZ_Index1_Ready,
            AlignZ_Index2_Up,
            AlignZ_Index2_Ready,
            AlignZ_Index3_Up,
            AlignZ_Index3_Ready,
            AlignZ_Index4_Up,
            AlignZ_Index4_Ready,
            AlignZ_Index5_Up,
            AlignZ_Index5_Ready,
            AlignZ_Index6_Up,
            AlignZ_Index6_Ready,
            AlignZ_Index7_Up,
            AlignZ_Index7_Ready,
            AlignZ_Index8_Up,
            AlignZ_Index8_Ready,
            AlignT_Foward,
            AlignT_Backward,
            AlignT_Ready,
            SafetyZone
        }

        /// <summary>
        /// Position 별 허용 축 매핑 (필요 시 일부 Position에서 특정 축만 사용하도록 조정)
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.AlignZ_Index1_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index1_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index2_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index2_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index3_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index3_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index4_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index4_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index5_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index5_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index6_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index6_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index7_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index7_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index8_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index8_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignT_Foward,       new [] { AxisNames.AlignT } },
            { TeachingPositionName.AlignT_Backward,     new [] { AxisNames.AlignT } },
            { TeachingPositionName.AlignT_Ready,        new [] { AxisNames.AlignT } },
            { TeachingPositionName.SafetyZone,          new [] { AxisNames.IndexZ } },
        };

        [Category("Teaching"), DisplayName("Positions")]
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        public IndexLoadAlignerRecipe() : this(null)
        {

        }
        public IndexLoadAlignerRecipe(string name = null) : base(name)
        {

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
            return new[] { AxisNames.IndexZ, AxisNames.AlignT };
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
            Log.Write(nameof(IndexLoadAlignerRecipe), nameof(LoadAndBindAxes),
                    $"Name='{Name}', file='{filePath}', existedBefore={existed}, LoadRc={rc}");

            if (rc != 0)
            {
                // 파일이 없으면 현재 Name으로 신규 생성
                TeachingPositions = TeachingPositions ?? new List<TeachingPosition>();
                InitializeDefaultTeachingPositions(save: true);

                Log.Write(nameof(IndexLoadAlignerRecipe), nameof(LoadAndBindAxes),
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
