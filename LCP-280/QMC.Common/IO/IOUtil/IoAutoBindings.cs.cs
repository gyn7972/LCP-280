// IoAutoBindings.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QMC.Common.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QMC.Common.IOUtil
{
    /// <summary>
    /// Unit.dio.setup.json(DIOUnit) 내용을 스캔해:
    /// - Cylinder: (UP/DOWN) 또는 (FWD/BWD) 짝이 DI/DO에 있는 경우 자동 바인딩
    /// - Vacuum : "VAC"/"VACUUM"/"SUCTION" 출력과 "CHECK/OK/SENSOR" 입력을 자동 매칭
    /// 만들어진 도메인 인스턴스는 Dictionaries로 노출합니다.
    /// </summary>
    public static class IoAutoBindings
    {
        public static readonly Dictionary<string, Cylinder> Cylinders = new Dictionary<string, Cylinder>(StringComparer.OrdinalIgnoreCase);
        public static readonly Dictionary<string, Vacuum> Vacuums = new Dictionary<string, Vacuum>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterAll()
        {
            var eq = EquipmentLocator.Instance
                ?? throw new InvalidOperationException("EquipmentLocator.Initialize(...) 전에 RegisterAll 호출됨.");
            var unit = eq.UnitIO
                ?? throw new InvalidOperationException("Equipment.UnitIO가 초기화되지 않았습니다.");

            // 1) 이름→채널 목록으로 수집
            var inputs = Enumerate(unit, isOutput: false).ToList();
            var outputs = Enumerate(unit, isOutput: true).ToList();

            // 2) Vacuum 바인딩
            BuildVacuums(unit, inputs, outputs);

            // 3) Cylinder 바인딩 (UP/DOWN & FWD/BWD)
            BuildCylinders(unit, inputs, outputs);
        }

        // ===== Helpers =====

        private sealed class ChRef
        {
            public string Module, Disp, NameNorm, NameRaw;
            public bool IsOutput;
        }

        private static IEnumerable<ChRef> Enumerate(DIOUnit unit, bool isOutput)
        {
            foreach (var m in unit.Modules ?? Enumerable.Empty<DIOModuleSetup>())
            {
                var list = isOutput ? m.Outputs : m.Inputs;
                if (list == null) continue;

                foreach (var c in list)
                {
                    yield return new ChRef
                    {
                        Module = m.ModuleName,
                        Disp = c.DisplayNo,
                        NameNorm = Normalize(c.Name),
                        NameRaw = c.Name,
                        IsOutput = isOutput
                    };
                }
            }
        }

        private static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim().ToUpperInvariant();
            s = Regex.Replace(s, @"[\s/_\-]+", " "); // 구분자 통일
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }

        private static bool Has(string norm, params string[] tokens)
        {
            foreach (var t in tokens)
                if (!norm.Contains(t)) return false;
            return true;
        }

        private static string RemoveTokens(string norm, params string[] tokens)
        {
            var r = norm;
            foreach (var t in tokens)
                r = Regex.Replace(r, @"\b" + Regex.Escape(t) + @"\b", "", RegexOptions.IgnoreCase).Trim();
            r = Regex.Replace(r, @"\s+", " ").Trim();
            return r;
        }

        // ---------- Vacuum ----------
        private static void BuildVacuums(DIOUnit unit, List<ChRef> inputs, List<ChRef> outputs)
        {
            var vacOuts = outputs.Where(o => Has(o.NameNorm, "VAC") || Has(o.NameNorm, "VACUUM") || Has(o.NameNorm, "SUCTION")).ToList();

            foreach (var o in vacOuts)
            {
                // base 이름 추출 (예: "EJECTOR VACUUM" -> "EJECTOR")
                var baseName = RemoveTokens(o.NameNorm, "VACUUM", "VAC", "SUCTION", "VALVE", "OUT");
                if (string.IsNullOrWhiteSpace(baseName)) baseName = o.NameNorm;

                // 센서 입력 매칭
                var inCand = inputs.FirstOrDefault(i =>
                    (i.NameNorm.Contains(baseName) || baseName.Contains(i.NameNorm)) &&
                    (Has(i.NameNorm, "CHECK") || Has(i.NameNorm, "OK") || Has(i.NameNorm, "SENSOR") || Has(i.NameNorm, "VAC")));

                // 키 생성
                var keyOut = baseName + ".VacOut";
                var keyOk = baseName + ".VacOk";

                DIO.Map(keyOut, o.Module, o.Disp, isOutput: true);

                if (inCand != null)
                {
                    DIO.Map(keyOk, inCand.Module, inCand.Disp, isOutput: false);
                    var vac = new Vacuum(baseName, keyOut, keyOk);
                    Vacuums[baseName] = vac;
                }
                else
                {
                    // 센서가 없으면 On/Off만 가능한 Vacuum으로 등록(OK 키 없이)
                    var vac = new Vacuum(baseName, keyOut, okInKey: keyOut + "/*NO_SENSOR*/");
                    Vacuums[baseName] = vac;
                }
            }
        }

        // ---------- Cylinders ----------
        private static void BuildCylinders(DIOUnit unit, List<ChRef> inputs, List<ChRef> outputs)
        {
            // 후보: OUT 중에서 UP/DOWN 또는 FWD/BWD가 존재하는 것
            var outsUp = outputs.Where(o => Has(o.NameNorm, "UP") && !Has(o.NameNorm, "UNCLAMP")).ToList();
            var outsDown = outputs.Where(o => Has(o.NameNorm, "DOWN")).ToList();
            var outsFwd = outputs.Where(o => Has(o.NameNorm, "FWD") || Has(o.NameNorm, "FORWARD")).ToList();
            var outsBwd = outputs.Where(o => Has(o.NameNorm, "BWD") || Has(o.NameNorm, "BACK") || Has(o.NameNorm, "BACKWARD")).ToList();

            // UP/DOWN 그룹
            GroupAndBuild("UP", "DOWN", outsUp, outsDown, inputs);

            // FWD/BWD 그룹
            GroupAndBuild("FWD", "BWD", outsFwd, outsBwd, inputs, altA: "FORWARD", altB: "BACKWARD", alsoTreatBack: true);
        }

        private static void GroupAndBuild(string a, string b,
                                          List<ChRef> outsA, List<ChRef> outsB,
                                          List<ChRef> inputs,
                                          string altA = null, string altB = null,
                                          bool alsoTreatBack = false)
        {
            // baseName 은 A/B 토큰 제거한 문자열
            string BaseOf(ChRef x) => RemoveTokens(x.NameNorm, a, b, altA ?? "", altB ?? "", "VALVE", "OUT");

            // A 기준으로 매칭
            foreach (var oa in outsA)
            {
                var baseName = BaseOf(oa);
                // 대응 B 출력 찾기
                var ob = outsB.FirstOrDefault(x => BaseOf(x).Equals(baseName, StringComparison.OrdinalIgnoreCase));
                if (ob == null) continue;

                // 입력 센서 찾기
                var inA = inputs.FirstOrDefault(i => i.NameNorm.Contains(baseName) && (Has(i.NameNorm, a) || (altA != null && Has(i.NameNorm, altA))));
                var inB = inputs.FirstOrDefault(i => i.NameNorm.Contains(baseName) && (Has(i.NameNorm, b) || (altB != null && Has(i.NameNorm, altB)) || (alsoTreatBack && Has(i.NameNorm, "BACK"))));

                // 키 등록
                var keyAOut = baseName + "." + a + "Out";
                var keyBOut = baseName + "." + b + "Out";
                DIO.Map(keyAOut, oa.Module, oa.Disp, isOutput: true);
                DIO.Map(keyBOut, ob.Module, ob.Disp, isOutput: true);

                string keyAIn = null, keyBIn = null;
                if (inA != null) { keyAIn = baseName + "." + a + "In"; DIO.Map(keyAIn, inA.Module, inA.Disp, isOutput: false); }
                if (inB != null) { keyBIn = baseName + "." + b + "In"; DIO.Map(keyBIn, inB.Module, inB.Disp, isOutput: false); }

                // 도메인 생성 (센서가 하나라도 있다면 정상, 없으면 출력만 동작)
                var fwdKey = keyAOut; var bwdKey = keyBOut;
                var fwdIn = keyAIn ?? (baseName + "." + a + "In/*NO_SENSOR*/");
                var bwdIn = keyBIn ?? (baseName + "." + b + "In/*NO_SENSOR*/");

                var cyl = new Cylinder(baseName, fwdKey, bwdKey, fwdIn, bwdIn);
                Cylinders[baseName] = cyl;
            }
        }
    }
}


//// Program.Main (또는 앱 시작 루틴)
//var eq = new QMC.LCP_280.Process.Equipment();
//eq.InitializeEquipment();                       // DioScan/UnitIO 준비 완료
//QMC.Common.EquipmentLocator.Initialize(eq);     // Locator에 주입

//QMC.Common.IOUtil.IoAutoBindings.RegisterAll(); // ← JSON 스캔해서 자동 바인딩



//// IO 함수로 직접
//QMC.Common.IOUtil.DIO.Out("WAFER FEEDER.UPOut", true); // 실제 키 이름은 자동 생성 규칙에 따라 생성됨

//// 도메인으로 간단히
//QMC.Common.IOUtil.IoAutoBindings.Cylinders["WAFER FEEDER"].Extend();
//QMC.Common.IOUtil.IoAutoBindings.Vacuums["EJECTOR"].OnWaitOk();

//자동 바인딩 규칙 요약

//Vacuum

//출력 이름에 VAC/VACUUM/SUCTION 포함 → Base.VacOut 키 생성

//같은 베이스 이름을 가진 입력 중 CHECK/OK/SENSOR/VAC 포함 → Base.VacOk 키 생성 → Vacuum(Base) 생성

//Cylinder

//출력 이름에 (UP, DOWN) 또는 (FWD/FORWARD, BWD/BACK/BACKWARD) 쌍이 존재 →

//키: Base.UpOut / DownOut / UpIn / DownIn 또는 Base.FWDOut / BWDOut / FWDIn / BWDIn

//센서가 없으면 /*NO_SENSOR*/ 가 붙은 키로 채움 (출력만 동작)

//Cylinder(Base) 생성