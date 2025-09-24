using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Setup 메뉴용 전용 FormManager
    /// </summary>
    public class FormManagerSetup
    {
        private static FormManagerSetup _instance;

        public static FormManagerSetup Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FormManagerSetup();
                return _instance;
            }
        }

        private FormManagerSetup() { }

        /// <summary>
        /// Main용 폼을 순서와 함께 등록
        /// </summary>
        /// <param name="formType">Main 폼 타입</param>
        /// <param name="displayName">표시명</param>
        /// <param name="description">설명</param>
        /// <param name="order">순서 (작을수록 앞에 표시)</param>
        public void RegisterSetupForm(Type formType, string displayName, string description = null, int order = int.MaxValue)
        {
            FormManager.Instance.RegisterForm(MenuButtonType.Setup, formType, displayName, description ?? displayName, order);
        }


        /// <summary>
        /// Setup 폼을 자동으로 검색하여 등록
        /// (XXUnit_Setup 패턴의 폼들을 자동으로 찾아서 등록)
        /// </summary>
        /// <param name="assemblyToSearch">검색할 어셈블리 (null이면 현재 어셈블리)</param>
        public void AutoRegisterUnitSetupForms(System.Reflection.Assembly assemblyToSearch = null)
        {
            try
            {
                if (assemblyToSearch == null)
                {
                    // QMC.LCP_280.Process 어셈블리에서 검색
                    var processAssembly = System.Reflection.Assembly.LoadFrom("QMC.LCP_280.Process.exe");
                    if (processAssembly != null)
                        assemblyToSearch = processAssembly;
                    else
                        assemblyToSearch = System.Reflection.Assembly.GetExecutingAssembly();
                }

                var types = assemblyToSearch.GetTypes();
                var formTypes = new List<(Type type, int order)>();

                foreach (var type in types)
                {
                    if (typeof(Form).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        (type.Name.Contains("Unit_Setup") || type.Name.Contains("UnitSetup") || type.Name.EndsWith("Setup")))
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
                    RegisterSetupForm(type, unitName, $"{unitName} Setup & Calibration", order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit Setup 폼 자동 등록 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            // XXUnit_Setup, XXUnitSetup 패턴에서 XX 부분 추출
            if (typeName.Contains("Unit_Setup"))
            {
                result = typeName.Replace("Unit_Setup", "");
            }
            else if (typeName.Contains("UnitSetup"))
            {
                result = typeName.Replace("UnitSetup", "");
            }
            else if (typeName.EndsWith("Setup"))
            {
                result = typeName.Replace("Setup", "");
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
        /// Setup용으로 등록된 폼들을 가져옴
        /// </summary>
        /// <returns>Setup 폼 정보 리스트</returns>
        public List<FormInfo> GetSetupForms()
        {
            return FormManager.Instance.GetRegisteredForms(MenuButtonType.Setup);
        }

        /// <summary>
        /// 특정 Unit의 Setup 폼을 생성
        /// </summary>
        /// <param name="unitName">Unit 이름</param>
        /// <returns>Setup 폼 인스턴스</returns>
        public Form CreateSetupForm(string unitName = null)
        {
            var setupForms = GetSetupForms();

            if (!string.IsNullOrEmpty(unitName))
            {
                var targetForm = setupForms.Find(f => f.DisplayName.Contains(unitName));
                if (targetForm != null)
                {
                    return FormManager.Instance.CreateFormInstance(targetForm);
                }
                throw new ArgumentException($"{unitName}에 대한 Setup 폼을 찾을 수 없습니다.");
            }

            if (setupForms.Count >= 2)
            {
                return new FormSetup();
            }

            // 첫 번째 등록된 폼 반환
            if (setupForms.Count == 1)
            {
                return FormManager.Instance.CreateFormInstance(setupForms[0]);
            }

            // 등록된 폼이 없으면 기본 폼 반환
            return CreateDefaultSetupForm();
        }

        /// <summary>
        /// 기본 Setup 폼 생성
        /// </summary>
        /// <returns>기본 Setup 폼</returns>
        private Form CreateDefaultSetupForm()
        {
            Form defaultForm = new Form
            {
                Text = "Setup & Calibration",
                BackColor = System.Drawing.Color.White
            };

            System.Windows.Forms.Label label = new System.Windows.Forms.Label
            {
                Text = "Setup & Calibration Area\n\nRegister your setup forms using FormManagerSetup.Instance.RegisterSetupForm()\n\nOr use AutoRegisterUnitSetupForms() to auto-discover XXUnit_Setup forms",
                Font = new System.Drawing.Font("맑은 고딕", 12, System.Drawing.FontStyle.Regular),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            defaultForm.Controls.Add(label);
            return defaultForm;
        }

        /// <summary>
        /// 등록된 Setup 폼들을 초기화하고 다시 로드
        /// </summary>
        public void RefreshSetupForms()
        {
            FormManager.Instance.ClearRegistrations(MenuButtonType.Setup);
            AutoRegisterUnitSetupForms();
        }
    }
}