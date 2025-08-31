using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Working 메뉴용 전용 FormManager
    /// </summary>
    public class FormManagerWorking
    {
        private static FormManagerWorking _instance;

        public static FormManagerWorking Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FormManagerWorking();
                return _instance;
            }
        }

        private FormManagerWorking() { }

        /// <summary>
        /// Working용 폼을 등록
        /// </summary>
        /// <param name="formType">Working 폼 타입</param>
        /// <param name="displayName">표시명</param>
        /// <param name="description">설명</param>
        public void RegisterWorkingForm(Type formType, string displayName, string description = null)
        {
            FormManager.Instance.RegisterForm(MenuButtonType.Working, formType, displayName, description ?? displayName);
        }

        /// <summary>
        /// Working 폼을 자동으로 검색하여 등록
        /// (XXUnit_Working 패턴의 폼들을 자동으로 찾아서 등록)
        /// </summary>
        /// <param name="assemblyToSearch">검색할 어셈블리 (null이면 현재 어셈블리)</param>
        public void AutoRegisterUnitWorkingForms(System.Reflection.Assembly assemblyToSearch = null)
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
                foreach (var type in types)
                {
                    // Form을 상속받고 이름이 "Working"로 끝나는 클래스 찾기
                    if (typeof(Form).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        (type.Name.Contains("Unit_Working") || type.Name.Contains("UnitWorking") || type.Name.EndsWith("Working")))
                    {
                        // Unit 이름 추출
                        string unitName = ExtractUnitNameFromType(type);
                        RegisterWorkingForm(type, unitName, $"{unitName} Working Screen");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit Working 폼 자동 등록 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            // XXUnit_Working, XXUnitWorking 패턴에서 XX 부분 추출
            if (typeName.Contains("Unit_Working"))
            {
                result = typeName.Replace("Unit_Working", "");
            }
            else if (typeName.Contains("UnitWorking"))
            {
                result = typeName.Replace("UnitWorking", "");
            }
            else if (typeName.EndsWith("Working"))
            {
                result = typeName.Replace("Working", "");
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
        /// Working용으로 등록된 폼들을 가져옴
        /// </summary>
        /// <returns>Working 폼 정보 리스트</returns>
        public List<FormInfo> GetWorkingForms()
        {
            return FormManager.Instance.GetRegisteredForms(MenuButtonType.Working);
        }

        /// <summary>
        /// 특정 Unit의 Working 폼을 생성
        /// </summary>
        /// <param name="unitName">Unit 이름</param>
        /// <returns>Working 폼 인스턴스</returns>
        public Form CreateWorkingForm(string unitName = null)
        {
            var workingForms = GetWorkingForms();

            if (!string.IsNullOrEmpty(unitName))
            {
                var targetForm = workingForms.Find(f => f.DisplayName.Contains(unitName));
                if (targetForm != null)
                {
                    return FormManager.Instance.CreateFormInstance(targetForm);
                }
                throw new ArgumentException($"{unitName}에 대한 Working 폼을 찾을 수 없습니다.");
            }

            if (workingForms.Count >= 2)
            {
                return new FormWorking();
            }

            // 첫 번째 등록된 폼 반환
            if (workingForms.Count == 1)
            {
                return FormManager.Instance.CreateFormInstance(workingForms[0]);
            }

            // 등록된 폼이 없으면 기본 폼 반환
            return CreateDefaultWorkingForm();
        }

        /// <summary>
        /// 기본 Working 폼 생성
        /// </summary>
        /// <returns>기본 Working 폼</returns>
        private Form CreateDefaultWorkingForm()
        {
            Form defaultForm = new Form
            {
                Text = "Working Screen",
                BackColor = System.Drawing.Color.White
            };

            System.Windows.Forms.Label label = new System.Windows.Forms.Label
            {
                Text = "Working & Monitoring Area\n\nRegister your working forms using FormManagerWorking.Instance.RegisterWorkingForm()\n\nOr use AutoRegisterUnitWorkingForms() to auto-discover XXUnit_Working forms",
                Font = new System.Drawing.Font("맑은 고딕", 12, System.Drawing.FontStyle.Regular),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            defaultForm.Controls.Add(label);
            return defaultForm;
        }

        /// <summary>
        /// 등록된 Working 폼들을 초기화하고 다시 로드
        /// </summary>
        public void RefreshWorkingForms()
        {
            FormManager.Instance.ClearRegistrations(MenuButtonType.Working);
            AutoRegisterUnitWorkingForms();
        }
    }
}