using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Working 메뉴용 전용 FormManager
    /// </summary>
    public class FormManagerMenual
    {
        private static FormManagerMenual _instance;

        public static FormManagerMenual Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FormManagerMenual();
                return _instance;
            }
        }

        private FormManagerMenual() { }

        /// <summary>
        /// Main용 폼을 순서와 함께 등록
        /// </summary>
        /// <param name="formType">Main 폼 타입</param>
        /// <param name="displayName">표시명</param>
        /// <param name="description">설명</param>
        /// <param name="order">순서 (작을수록 앞에 표시)</param>
        public void RegisterManualForm(Type formType, string displayName, string description = null, int order = int.MaxValue)
        {
            FormManager.Instance.RegisterForm(MenuButtonType.Menual, formType, displayName, description ?? displayName, order);
        }

        /// <summary>
        /// Working 폼을 자동으로 검색하여 등록
        /// (XXUnit_Working 패턴의 폼들을 자동으로 찾아서 등록)
        /// </summary>
        /// <param name="assemblyToSearch">검색할 어셈블리 (null이면 현재 어셈블리)</param>
        public void AutoRegisterUnitManualForms(System.Reflection.Assembly assemblyToSearch = null)
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
                        (type.Name.Contains("Unit_Menual") || type.Name.Contains("UnitMenual") || type.Name.EndsWith("Menual")))
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
                    RegisterManualForm(type, unitName, $"{unitName} Working Screen", order);
                }
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Warning!", $"Unit Working 폼 자동 등록 중 오류: {ex.Message}");
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

            // XXUnit_Menual, XXUnitMenual 패턴에서 XX 부분 추출
            if (typeName.Contains("Unit_Menual"))
            {
                result = typeName.Replace("Unit_Menual", "");
            }
            else if (typeName.Contains("UnitMenual"))
            {
                result = typeName.Replace("UnitMenual", "");
            }
            else if (typeName.EndsWith("Menual"))
            {
                result = typeName.Replace("Menual", "");
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
        /// Manual용으로 등록된 폼들을 가져옴
        /// </summary>
        /// <returns>Manual 폼 정보 리스트</returns>
        public List<FormInfo> GetMenualForms()
        {
            return FormManager.Instance.GetRegisteredForms(MenuButtonType.Menual);
        }

        /// <summary>
        /// 특정 Unit의 Menaul 폼을 생성
        /// </summary>
        /// <param name="unitName">Unit 이름</param>
        /// <returns>Menaul 폼 인스턴스</returns>
        public Form CreateMenaulForm(string unitName = null)
        {
            var MenaulForms = GetMenualForms();

            if (!string.IsNullOrEmpty(unitName))
            {
                var targetForm = MenaulForms.Find(f => f.DisplayName.Contains(unitName));
                if (targetForm != null)
                {
                    return FormManager.Instance.CreateFormInstance(targetForm);
                }
                throw new ArgumentException($"{unitName}에 대한 Working 폼을 찾을 수 없습니다.");
            }

            if (MenaulForms.Count >= 2)
            {
                return new FormMenual();
            }

            // 첫 번째 등록된 폼 반환
            if (MenaulForms.Count == 1)
            {
                return FormManager.Instance.CreateFormInstance(MenaulForms[0]);
            }

            // 등록된 폼이 없으면 기본 폼 반환
            return CreateDefaultMenaulForm();
        }

        /// <summary>
        /// 기본 Menaul 폼 생성
        /// </summary>
        /// <returns>기본 Menaul 폼</returns>
        private Form CreateDefaultMenaulForm()
        {
            Form defaultForm = new Form
            {
                Text = "Working Screen",
                BackColor = System.Drawing.Color.White
            };

            System.Windows.Forms.Label label = new System.Windows.Forms.Label
            {
                Text = "Menaul & Monitoring Area\n\nRegister your Menaul forms using FormManagerMenaul.Instance.RegisterMenaulForm()\n\nOr use AutoRegisterUnitMenaulForms() to auto-discover XXUnit_Menaul forms",
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
        public void RefreshMenaulForms()
        {
            FormManager.Instance.ClearRegistrations(MenuButtonType.Menual);
            AutoRegisterUnitManualForms();
        }
    }
}