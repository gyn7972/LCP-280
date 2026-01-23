using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Config 메뉴용 전용 FormManager
    /// </summary>
    public class FormManagerConfig
    {
        private static FormManagerConfig _instance;

        public static FormManagerConfig Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FormManagerConfig();
                return _instance;
            }
        }

        private FormManagerConfig() { }

        /// <summary>
        /// Config용 폼을 등록
        /// </summary>
        /// <param name="formType">Config 폼 타입</param>
        /// <param name="displayName">표시명</param>
        /// <param name="description">설명</param>
        /// <param name="order">순서 (작을수록 앞에 표시)</param>
        public void RegisterConfigForm(Type formType, string displayName, string description = null, int order = int.MaxValue)
        {
            FormManager.Instance.RegisterForm(MenuButtonType.Config, formType, displayName, description ?? displayName, order);
        }

        /// <summary>
        /// Config 폼을 자동으로 검색하여 등록
        /// (XXUnit_Config 패턴의 폼들을 자동으로 찾아서 등록)
        /// </summary>
        /// <param name="assemblyToSearch">검색할 어셈블리 (null이면 현재 어셈블리)</param>
        public void AutoRegisterUnitConfigForms(System.Reflection.Assembly assemblyToSearch = null)
        {
            try
            {
                if (assemblyToSearch == null)
                {
                    // QMC.LCP_280.Process 어셈블리에서 검색
                    //var processAssembly = System.Reflection.Assembly.LoadFrom("QMC.LCP_280.Process.exe");
                    var processAssembly = System.Reflection.Assembly.LoadFrom("LCP-280.exe");

                    if (processAssembly != null)
                        assemblyToSearch = processAssembly;
                    else
                        assemblyToSearch = System.Reflection.Assembly.GetExecutingAssembly();
                }

                var types = assemblyToSearch.GetTypes();
                var formTypes = new List<(Type type, int order)>();

                foreach (var type in types)
                {
                    // Form을 상속받고 이름이 "Config"로 끝나는 클래스 찾기
                    if (typeof(Form).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        (type.Name.Contains("Unit_Config") || type.Name.Contains("UnitConfig") || type.Name.EndsWith("Config")))
                    {
                        // FormOrder Attribute 확인
                        var orderAttr = type.GetCustomAttribute<FormOrderAttribute>();
                        int order = orderAttr?.Order ?? int.MaxValue;

                        formTypes.Add((type, order));
                    }
                }

                // Order 순으로 정렬한 후 등록
                foreach (var (type, order) in formTypes.OrderBy(x => x.order).ThenBy(x => x.type.Name))
                {
                    string unitName = ExtractUnitNameFromType(type);
                    RegisterConfigForm
                        (type, unitName, $"{unitName} Unit Configuration", order);
                }
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Warning!", $"Unit Config 폼 자동 등록 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 타입 이름에서 Unit 이름을 추출
        /// </summary>
        /// <param name="type">폼 타입</param>
        /// <returns>Unit 이름</returns>
        private string ExtractUnitNameFromType(Type type)
        {
            string typeName = type.Name;
            string result;

            // XXUnit_Config, XXUnitConfig 패턴에서 XX 부분 추출
            if (typeName.Contains("Unit_Config"))
            {
                result = typeName.Replace("Unit_Config", "");
            }
            else if (typeName.Contains("UnitConfig"))
            {
                result = typeName.Replace("UnitConfig", "");
            }
            else if (typeName.EndsWith("Config"))
            {
                result = typeName.Replace("Config", "");
            }
            else
            {
                result = typeName;
            }

            result = result.Replace('_', ' ');
            result = System.Text.RegularExpressions.Regex.Replace(result, "\\s+", " ").Trim();
            return result;
        }

        /// <summary>
        /// Config용으로 등록된 폼들을 가져옴
        /// </summary>
        /// <returns>Config 폼 정보 리스트</returns>
        public List<FormInfo> GetConfigForms()
        {
            return FormManager.Instance.GetRegisteredForms(MenuButtonType.Config);
        }

        /// <summary>
        /// 특정 Unit의 Config 폼을 생성
        /// </summary>
        /// <param name="unitName">Unit 이름</param>
        /// <returns>Config 폼 인스턴스</returns>
        public Form CreateConfigForm(string unitName = null)
        {
            var configForms = GetConfigForms();

            if (!string.IsNullOrEmpty(unitName))
            {
                var targetForm = configForms.Find(f => f.DisplayName.Contains(unitName));
                if (targetForm != null)
                {
                    return FormManager.Instance.CreateFormInstance(targetForm);
                }
                throw new ArgumentException($"{unitName}에 대한 Config 폼을 찾을 수 없습니다.");
            }

            if (configForms.Count >= 2)
            {
                return new FormConfig();
            }

            // 첫 번째 등록된 폼 반환
            if (configForms.Count == 1)
            {
                return FormManager.Instance.CreateFormInstance(configForms[0]);
            }

            // 등록된 폼이 없으면 기본 폼 반환
            return CreateDefaultConfigForm();
        }

        /// <summary>
        /// 기본 Config 폼 생성
        /// </summary>
        /// <returns>기본 Config 폼</returns>
        private Form CreateDefaultConfigForm()
        {
            Form defaultForm = new Form
            {
                Text = "Configuration Screen",
                BackColor = System.Drawing.Color.White
            };

            System.Windows.Forms.Label label = new System.Windows.Forms.Label
            {
                Text = "Configuration Management Area\n\nRegister your config forms using FormManagerConfig.Instance.RegisterConfigForm()\n\nOr use AutoRegisterUnitConfigForms() to auto-discover XXUnit_Config forms",
                Font = new System.Drawing.Font("맑은 고딕", 12, System.Drawing.FontStyle.Regular),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            defaultForm.Controls.Add(label);
            return defaultForm;
        }

        /// <summary>
        /// 등록된 Config 폼들을 초기화하고 다시 로드
        /// </summary>
        public void RefreshConfigForms()
        {
            FormManager.Instance.ClearRegistrations(MenuButtonType.Config);
            AutoRegisterUnitConfigForms();
        }
    }
}