using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Main 메뉴용 전용 FormManager
    /// </summary>
    public class FormManagerMain
    {
        private static FormManagerMain _instance;
        
        public static FormManagerMain Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FormManagerMain();
                return _instance;
            }
        }

        private FormManagerMain() { }

        /// <summary>
        /// Main 화면용 폼을 등록
        /// </summary>
        /// <param name="formType">Main 폼 타입</param>
        /// <param name="displayName">표시명</param>
        /// <param name="description">설명</param>
        public void RegisterMainForm(Type formType, string displayName, string description = null)
        {
            FormManager.Instance.RegisterForm(MenuButtonType.Main, formType, displayName, description ?? displayName);
        }

        /// <summary>
        /// Main 폼을 자동으로 검색하여 등록
        /// (XXUnit_Main 패턴의 폼들을 자동으로 찾아서 등록)
        /// </summary>
        /// <param name="assemblyToSearch">검색할 어셈블리 (null이면 현재 어셈블리)</param>
        public void AutoRegisterUnitMainForms(System.Reflection.Assembly assemblyToSearch = null)
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
                    // Form을 상속받고 이름이 "Main"로 끝나는 클래스 찾기
                    if (typeof(Form).IsAssignableFrom(type) && 
                        !type.IsAbstract && 
                        (type.Name.Contains("Unit_Main") || type.Name.Contains("UnitMain")))
                    {
                        // Unit 이름 추출
                        string unitName = ExtractUnitNameFromType(type);
                        RegisterMainForm(type, unitName, $"{unitName} Main Screen");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit Main 폼 자동 등록 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            
            // XXUnit_Main, XXUnitMain 패턴에서 XX 부분 추출
            if (typeName.Contains("Unit_Main"))
            {
                return typeName.Replace("Unit_Main", "").Replace("_", " ");
            }
            else if (typeName.Contains("UnitMain"))
            {
                return typeName.Replace("UnitMain", "");
            }
            else if (typeName.EndsWith("Main"))
            {
                return typeName.Replace("Main", "");
            }
            
            return typeName;
        }

        /// <summary>
        /// Main용으로 등록된 폼들을 가져옴
        /// </summary>
        /// <returns>Main 폼 정보 리스트</returns>
        public List<FormInfo> GetMainForms()
        {
            return FormManager.Instance.GetRegisteredForms(MenuButtonType.Main);
        }

        /// <summary>
        /// 특정 Unit의 Main 폼을 생성
        /// </summary>
        /// <param name="unitName">Unit 이름</param>
        /// <returns>Main 폼 인스턴스</returns>
        public Form CreateMainForm(string unitName = null)
        {
            var mainForms = GetMainForms();
            
            if (!string.IsNullOrEmpty(unitName))
            {
                var targetForm = mainForms.Find(f => f.DisplayName.Contains(unitName));
                if (targetForm != null)
                {
                    return FormManager.Instance.CreateFormInstance(targetForm);
                }
                throw new ArgumentException($"{unitName}에 대한 Main 폼을 찾을 수 없습니다.");
            }
            
            // 2개 이상의 메인 폼이 등록된 경우, 탭 형식의 폼(FormMain) 반환
            if (mainForms.Count >= 2)
            {
                return new FormMain();
            }
            
            // 1개인 경우 해당 폼 반환
            if (mainForms.Count == 1)
            {
                return FormManager.Instance.CreateFormInstance(mainForms[0]);
            }
            
            // 등록된 폼이 없으면 기본 폼 반환
            return CreateDefaultMainForm();
        }

        /// <summary>
        /// 기본 Main 폼 생성
        /// </summary>
        /// <returns>기본 Main 폼</returns>
        private Form CreateDefaultMainForm()
        {
            Form defaultForm = new Form
            {
                Text = "Main Screen",
                BackColor = System.Drawing.Color.White
            };

            System.Windows.Forms.Label label = new System.Windows.Forms.Label
            {
                Text = "Main Working Area\n\nRegister your main forms using FormManagerMain.Instance.RegisterMainForm()\n\nOr use AutoRegisterUnitMainForms() to auto-discover XXUnit_Main forms",
                Font = new System.Drawing.Font("맑은 고딕", 12, System.Drawing.FontStyle.Regular),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            defaultForm.Controls.Add(label);
            return defaultForm;
        }

        /// <summary>
        /// 등록된 Main 폼들을 초기화하고 다시 로드
        /// </summary>
        public void RefreshMainForms()
        {
            FormManager.Instance.ClearRegistrations(MenuButtonType.Main);
            AutoRegisterUnitMainForms();
        }
    }
}