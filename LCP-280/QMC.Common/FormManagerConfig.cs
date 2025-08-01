using System;
using System.Collections.Generic;
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
        /// Config 폼을 등록 (Unit 이름 기반 자동 등록)
        /// </summary>
        /// <param name="formType">Config 폼 타입</param>
        /// <param name="unitName">Unit 이름 (예: "DieLoader", "WaferAlign" 등)</param>
        /// <param name="description">설명</param>
        public void RegisterConfigForm(Type formType, string unitName, string description = null)
        {
            string displayName = $"{unitName}";
            FormManager.Instance.RegisterForm(MenuButtonType.Config, formType, displayName, description ?? displayName);
        }

        /// <summary>
        /// Unit Config 폼을 자동으로 검색하여 등록
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
                    var processAssembly = System.Reflection.Assembly.LoadFrom("QMC.LCP_280.Process.exe");
                    if (processAssembly != null)
                        assemblyToSearch = processAssembly;
                    else
                        assemblyToSearch = System.Reflection.Assembly.GetExecutingAssembly();
                }

                var types = assemblyToSearch.GetTypes();
                foreach (var type in types)
                {
                    // Form을 상속받고 이름이 "Config"로 끝나는 클래스 찾기
                    if (typeof(Form).IsAssignableFrom(type) && 
                        !type.IsAbstract && 
                        (type.Name.Contains("Unit_Config") || type.Name.Contains("UnitConfig")))
                    {
                        // Unit 이름 추출
                        string unitName = ExtractUnitNameFromType(type);
                        RegisterConfigForm(type, unitName, $"{unitName} Unit Configuration");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit Config 폼 자동 등록 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            
            // XXUnit_Config, XXUnitConfig 패턴에서 XX 부분 추출
            if (typeName.Contains("Unit_Config"))
            {
                return typeName.Replace("Unit_Config", "").Replace("_", " ");
            }
            else if (typeName.Contains("UnitConfig"))
            {
                return typeName.Replace("UnitConfig", "");
            }
            else if (typeName.EndsWith("Config"))
            {
                return typeName.Replace("Config", "");
            }
            
            return typeName;
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
        public Form CreateConfigForm(string unitName)
        {
            var configForms = GetConfigForms();
            var targetForm = configForms.Find(f => f.DisplayName.Contains(unitName));
            
            if (targetForm != null)
            {
                return FormManager.Instance.CreateFormInstance(targetForm);
            }
            
            throw new ArgumentException($"{unitName}에 대한 Config 폼을 찾을 수 없습니다.");
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