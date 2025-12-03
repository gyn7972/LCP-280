using QMC.Common;
using QMC.LCP_280.Process.Component;
using System;
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
            "BinResultPath",
            "PRDResultPath",
            "SUMResultPath",
            "TXTResultPath",
            "WAFResultPath",
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

                object value = pi.GetValue(_config, null);
                PropertyBase prop = null;

                if (pi.PropertyType == typeof(string))
                    prop = CreateStringProperty(name, value as string ?? string.Empty);
                else if (pi.PropertyType == typeof(bool))
                    prop = CreateBoolProperty(name, value is bool b && b);

                if (prop != null) _equipPc.Add(prop);
            }

            EquipmentPropertyCollectionView.GroupName = "EquipmentConfig";
            EquipmentPropertyCollectionView.SetProperties(_equipPc);
        }

        private PropertyBase CreateStringProperty(string key, string value)
        {
            try
            {
                var t = typeof(StringProperty);
                var ctor = t.GetConstructors()
                    .FirstOrDefault(c =>
                    {
                        var pars = c.GetParameters();
                        return pars.Length == 3 &&
                               pars[0].ParameterType == typeof(string) &&
                               pars[1].ParameterType == typeof(string) &&
                               pars[2].ParameterType == typeof(string);
                    });
                if (ctor != null) return (PropertyBase)ctor.Invoke(new object[] { key, key, value });
                return new StringProperty(key, value);
            }
            catch { return new StringProperty(key, value); }
        }

        private PropertyBase CreateBoolProperty(string key, bool value)
        {
            try
            {
                var t = typeof(BoolProperty);
                var ctor = t.GetConstructors()
                    .FirstOrDefault(c =>
                    {
                        var pars = c.GetParameters();
                        return pars.Length == 3 &&
                               pars[0].ParameterType == typeof(string) &&
                               pars[1].ParameterType == typeof(string) &&
                               pars[2].ParameterType == typeof(bool);
                    });
                if (ctor != null) return (PropertyBase)ctor.Invoke(new object[] { key, key, value });
                return new BoolProperty(key, value);
            }
            catch { return new BoolProperty(key, value); }
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
                            string v;
                            try { v = current.GetValue<string>(name); } catch { continue; }
                            pi.SetValue(_config, v ?? string.Empty, null);
                        }
                        else if (pi.PropertyType == typeof(bool))
                        {
                            bool v;
                            try { v = current.GetValue<bool>(name); } catch { continue; }
                            pi.SetValue(_config, v, null);
                        }
                    }
                    catch { }
                }

                // 경로 디렉터리 보장
                EnsureDirIfPath(_config.LogPath);
                EnsureDirIfPath(_config.ResultPath);
                EnsureDirIfPath(_config.BinResultPath);
                EnsureDirIfPath(_config.PRDResultPath);
                EnsureDirIfPath(_config.SUMResultPath);
                EnsureDirIfPath(_config.TXTResultPath);
                EnsureDirIfPath(_config.WAFResultPath);
                EnsureDirOfFile(_config.ProductionInfoPath);
                //_config.MapMatchMode

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