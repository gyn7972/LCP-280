using QMC.Common;
using QMC.LCP_280.Process.Component;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(7)]
    public partial class Equipment_Setup : Form
    {
        private PropertyCollection _equipPc;
        private Equipment _equipment;
        private EquipmentConfig _config;

        // 저장/불러올 대상 전체 (BaseConfig의 IsDryRun/IsSimulation 포함)
        private readonly string[] _configPropertyNames = new[]
        {
            "EquipmentName",
            "EquipmentId",
            "IsDryRun",
            "IsSimulation",
            "LogPath",
            "ResultPath",
            "NetworkMode",
            "InspectionMapPath",
            "TXTResultPath",
            "PRDResultPath",
            "SUMResultPath",
            "BinResultPath",
            "WAFResultPath",
            "DBDataServerPath",
            "ProductionInfoPath",
            "MapMatchMode",
        };

        public Equipment_Setup()
        {
            InitializeComponent();
            this.Load += Equipment_Setup_Load;
        }

        private void Equipment_Setup_Load(object sender, EventArgs e)
        {
            _equipment = Equipment.Instance;
            _config = _equipment != null ? _equipment.EquipmentConfig : null;
            BuildConfigPropertyCollection();
        }

        private void BuildConfigPropertyCollection()
        {
            if (_config == null)
            {
                MessageBox.Show("EquipmentConfig가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _equipPc = new PropertyCollection { IsInputParameter = true };
            _equipPc.Add(new TitleOnlyProperty("EquipmentConfig"));

            var type = _config.GetType();
            foreach (var name in _configPropertyNames)
            {
                var pi = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                if (pi == null || !pi.CanRead) continue;

                var title = GetDisplayName(pi) ?? name; // 표시용 타이틀
                var key = name;                         // 내부 키는 CLR 이름 유지

                object value = pi.GetValue(_config, null);
                PropertyBase prop = null;

                if (pi.PropertyType == typeof(string))
                {
                    prop = CreateStringProperty(title, key, value as string ?? string.Empty);
                }
                else if (pi.PropertyType == typeof(bool))
                {
                    prop = CreateBoolProperty(title, key, value is bool b && b);
                }
                else if (pi.PropertyType == typeof(int))
                {
                    prop = CreateIntProperty(title, key, value is int iv ? iv : 0);
                }

                if (prop != null) _equipPc.Add(prop);
            }

            EquipmentPropertyCollectionView.GroupName = "EquipmentConfig";
            EquipmentPropertyCollectionView.SetProperties(_equipPc);
        }

        private static string GetDisplayName(PropertyInfo pi)
        {
            var dn = pi.GetCustomAttributes(typeof(DisplayNameAttribute), inherit: true)
                       .OfType<DisplayNameAttribute>()
                       .FirstOrDefault();
            return dn?.DisplayName;
        }

        private PropertyBase CreateStringProperty(string title, string key, string value)
        {
            try
            {
                var t = typeof(StringProperty);
                var ctor = t.GetConstructors()
                    .FirstOrDefault(c =>
                    {
                        var pars = c.GetParameters();
                        return pars.Length == 3 &&
                               pars[0].ParameterType == typeof(string) && // title
                               pars[1].ParameterType == typeof(string) && // key
                               pars[2].ParameterType == typeof(string);   // value
                    });
                if (ctor != null) return (PropertyBase)ctor.Invoke(new object[] { title, key, value });
                // 폴백: 기존 2인자 생성자(title, value)만 있는 경우 title을 표시로 쓰고 키는 내부에서 title로 매핑될 수 있음
                return new StringProperty(title, value);
            }
            catch { return new StringProperty(title, value); }
        }

        private PropertyBase CreateBoolProperty(string title, string key, bool value)
        {
            try
            {
                var t = typeof(BoolProperty);
                var ctor = t.GetConstructors()
                    .FirstOrDefault(c =>
                    {
                        var pars = c.GetParameters();
                        return pars.Length == 3 &&
                               pars[0].ParameterType == typeof(string) && // title
                               pars[1].ParameterType == typeof(string) && // key
                               pars[2].ParameterType == typeof(bool);     // value
                    });
                if (ctor != null) return (PropertyBase)ctor.Invoke(new object[] { title, key, value });
                return new BoolProperty(title, value);
            }
            catch { return new BoolProperty(title, value); }
        }

        private PropertyBase CreateIntProperty(string title, string key, int value)
        {
            try
            {
                // IntProperty 타입 탐색
                var intType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a => {
                        try { return a.GetTypes(); } catch { return Type.EmptyTypes; }
                    })
                    .FirstOrDefault(t => t.Name == "IntProperty");

                if (intType != null)
                {
                    var ctor = intType.GetConstructors()
                        .FirstOrDefault(c =>
                        {
                            var pars = c.GetParameters();
                            return pars.Length == 3 &&
                                   pars[0].ParameterType == typeof(string) && // title
                                   pars[1].ParameterType == typeof(string) && // key
                                   pars[2].ParameterType == typeof(int);      // value
                        });
                    if (ctor != null)
                        return (PropertyBase)ctor.Invoke(new object[] { title, key, value });
                }

                // 폴백: 문자열 프로퍼티로 표시
                return CreateStringProperty(title, key, value.ToString());
            }
            catch
            {
                return CreateStringProperty(title, key, value.ToString());
            }
        }

        private void btn_Save_Setup_Equipment_Click(object sender, EventArgs e)
        {
            ApplyAndSaveConfig();
        }

        private void ApplyAndSaveConfig()
        {
            if (_config == null || _equipPc == null)
            {
                MessageBox.Show("Config 또는 PropertyCollection 없음", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                EquipmentPropertyCollectionView.Apply();
                var current = EquipmentPropertyCollectionView.GetCurrentProperties();
                if (current == null)
                {
                    MessageBox.Show("PropertyCollection 조회 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var type = _config.GetType();

                foreach (var name in _configPropertyNames)
                {
                    var pi = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                    if (pi == null || !pi.CanWrite) continue;

                    try
                    {
                        if (pi.PropertyType == typeof(string))
                        {
                            if (!TryGetPcValue<string>(current, pi, name, out var v))
                                continue;

                            pi.SetValue(_config, v ?? string.Empty, null);
                        }
                        else if (pi.PropertyType == typeof(bool))
                        {
                            if (!TryGetPcValue<bool>(current, pi, name, out var v))
                                continue;

                            pi.SetValue(_config, v, null);
                        }
                        else if (pi.PropertyType == typeof(int))
                        {
                            // int로 시도
                            if (TryGetPcValue<int>(current, pi, name, out var iv))
                            {
                                pi.SetValue(_config, iv, null);
                                continue;
                            }

                            // string으로 받아서 파싱
                            if (TryGetPcValue<string>(current, pi, name, out var sv) && int.TryParse(sv, out iv))
                            {
                                pi.SetValue(_config, iv, null);
                                continue;
                            }
                        }
                    }
                    catch { }
                }

                // 경로 디렉터리 보장
                EnsureDirIfPath(_config.LogPath);
                EnsureDirIfPath(_config.ResultPath);
                EnsureDirIfPath(_config.InspectionMapPath);
                EnsureDirIfPath(_config.TXTResultPath);
                EnsureDirIfPath(_config.PRDResultPath);
                EnsureDirIfPath(_config.SUMResultPath);
                EnsureDirIfPath(_config.BinResultPath);
                EnsureDirIfPath(_config.WAFResultPath);
                EnsureDirIfPath(_config.DBDataServerPath);
                EnsureDirOfFile(_config.ProductionInfoPath);

                // 반드시 파일로 저장!
                var rc = _config.Save(); // BaseConfig.Save() 호출
                if (rc != 0)
                {
                    MessageBox.Show("저장 실패 (Save 반환값: " + rc + ")", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show("저장 완료", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // [ADD] PropertyCollection에서 CLR 속성명(name) 우선, 실패하면 DisplayName(title)로 재시도
        private bool TryGetPcValue<T>(PropertyCollection pc, PropertyInfo pi, string clrName, out T value)
        {
            value = default(T);
            if (pc == null || pi == null) return false;

            // 1) CLR name으로 시도
            try
            {
                value = pc.GetValue<T>(clrName);
                return true;
            }
            catch { }

            // 2) DisplayName(title)로 시도
            try
            {
                var title = GetDisplayName(pi);
                if (!string.IsNullOrWhiteSpace(title))
                {
                    value = pc.GetValue<T>(title);
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static void EnsureDirIfPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try { Directory.CreateDirectory(path); } catch { }
        }
        private static void EnsureDirOfFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);
            }
            catch { }
        }

        private void btnLanguage_Click(object sender, EventArgs e)
        {
            Form main = GetRootForm(this);
            LanguageSetupForm.ShowModeless(main);
        }

        private Form GetRootForm(Control c)
        {
            while (c != null && c.Parent != null) c = c.Parent;
            return c as Form;
        }

        private void brnMapMatch_Click(object sender, EventArgs e)
        {
            //Test
            double bestScore = 99.9;
            double scoreThreshold = 99.0;
            string strMapFile = @"D:\\111.wdf";

            using (var dlg = new MapMatchDecisionDialog(bestScore, scoreThreshold, strMapFile))
            {
                var dr = dlg.ShowDialog();
                if (dr != DialogResult.Yes)
                {
                    // 사용자 '중단' 선택 → 시퀀스 중단
                    Log.Write("LCP-280", "MapMatch", $"User chose STOP. Score={bestScore:F2} < Threshold={scoreThreshold:F2}. Sequence aborted.");
                }
                Log.Write("LCP-280", "MapMatch", $"User chose CONTINUE. Score={bestScore:F2}, Threshold={scoreThreshold:F2}.");
            }
        }
    }
}