using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Log 메뉴용 전용 FormManager
    /// </summary>
    public class FormManagerLog
    {
        private static FormManagerLog _instance;
        
        public static FormManagerLog Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FormManagerLog();
                return _instance;
            }
        }

        private FormManagerLog() { }

        /// <summary>
        /// Log 화면용 폼을 등록
        /// </summary>
        /// <param name="formType">Log 폼 타입</param>
        /// <param name="displayName">표시명</param>
        /// <param name="description">설명</param>
        public void RegisterLogForm(Type formType, string displayName, string description = null)
        {
            FormManager.Instance.RegisterForm(MenuButtonType.Log, formType, displayName, description ?? displayName);
        }

        /// <summary>
        /// Log 폼을 자동으로 검색하여 등록
        /// (XXUnit_Log 패턴의 폼들을 자동으로 찾아서 등록)
        /// </summary>
        /// <param name="assemblyToSearch">검색할 어셈블리 (null이면 현재 어셈블리)</param>
        public void AutoRegisterUnitLogForms(System.Reflection.Assembly assemblyToSearch = null)
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
                    // Form을 상속받고 이름이 "Log"로 끝나는 클래스 찾기
                    if (typeof(Form).IsAssignableFrom(type) && 
                        !type.IsAbstract && 
                        (type.Name.Contains("Unit_Log") || type.Name.Contains("UnitLog")))
                    {
                        // Unit 이름 추출
                        string unitName = ExtractUnitNameFromType(type);
                        RegisterLogForm(type, unitName, $"{unitName} Log Viewer");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit Log 폼 자동 등록 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            
            // XXUnit_Log, XXUnitLog 패턴에서 XX 부분 추출
            if (typeName.Contains("Unit_Log"))
            {
                return typeName.Replace("Unit_Log", "").Replace("_", " ");
            }
            else if (typeName.Contains("UnitLog"))
            {
                return typeName.Replace("UnitLog", "");
            }
            else if (typeName.EndsWith("Log"))
            {
                return typeName.Replace("Log", "");
            }
            else if (typeName.Contains("Logger"))
            {
                return typeName.Replace("Logger", "");
            }
            else if (typeName.Contains("History"))
            {
                return typeName.Replace("History", "");
            }
            else if (typeName.EndsWith("Viewer"))
            {
                return typeName.Replace("Viewer", "");
            }
            
            return typeName;
        }

        /// <summary>
        /// Log용으로 등록된 폼들을 가져옴
        /// </summary>
        /// <returns>Log 폼 정보 리스트</returns>
        public List<FormInfo> GetLogForms()
        {
            return FormManager.Instance.GetRegisteredForms(MenuButtonType.Log);
        }

        /// <summary>
        /// 특정 Unit의 Log 폼을 생성
        /// </summary>
        /// <param name="unitName">Unit 이름</param>
        /// <returns>Log 폼 인스턴스</returns>
        public Form CreateLogForm(string unitName = null)
        {
            var logForms = GetLogForms();
            
            if (!string.IsNullOrEmpty(unitName))
            {
                var targetForm = logForms.Find(f => f.DisplayName.Contains(unitName));
                if (targetForm != null)
                {
                    return FormManager.Instance.CreateFormInstance(targetForm);
                }
                throw new ArgumentException($"{unitName}에 대한 Log 폼을 찾을 수 없습니다.");
            }
            
            // 첫 번째 등록된 폼 반환
            if (logForms.Count > 0)
            {
                return FormManager.Instance.CreateFormInstance(logForms[0]);
            }
            
            // 등록된 폼이 없으면 기본 폼 반환
            return CreateDefaultLogForm();
        }

        /// <summary>
        /// 기본 Log 폼 생성
        /// </summary>
        /// <returns>기본 Log 폼</returns>
        private Form CreateDefaultLogForm()
        {
            Form defaultForm = new Form
            {
                Text = "Log Viewer",
                BackColor = System.Drawing.Color.White
            };

            System.Windows.Forms.Label label = new System.Windows.Forms.Label
            {
                Text = "Log & History Viewer\n\nRegister your log forms using FormManagerLog.Instance.RegisterLogForm()\n\nOr use AutoRegisterUnitLogForms() to auto-discover XXUnit_Log forms",
                Font = new System.Drawing.Font("맑은 고딕", 12, System.Drawing.FontStyle.Regular),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            defaultForm.Controls.Add(label);
            return defaultForm;
        }

        /// <summary>
        /// 등록된 Log 폼들을 초기화하고 다시 로드
        /// </summary>
        public void RefreshLogForms()
        {
            FormManager.Instance.ClearRegistrations(MenuButtonType.Log);
            AutoRegisterUnitLogForms();
        }
    }
}