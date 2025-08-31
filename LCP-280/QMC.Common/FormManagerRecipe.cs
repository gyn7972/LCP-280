using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Recipe 메뉴용 전용 FormManager
    /// </summary>
    public class FormManagerRecipe
    {
        private static FormManagerRecipe _instance;

        public static FormManagerRecipe Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FormManagerRecipe();
                return _instance;
            }
        }

        private FormManagerRecipe() { }

        /// <summary>
        /// Recipe용 폼을 등록
        /// </summary>
        /// <param name="formType">Recipe 폼 타입</param>
        /// <param name="displayName">표시명</param>
        /// <param name="description">설명</param>
        public void RegisterRecipeForm(Type formType, string displayName, string description = null)
        {
            FormManager.Instance.RegisterForm(MenuButtonType.Recipe, formType, displayName, description ?? displayName);
        }

        /// <summary>
        /// Recipe 폼을 자동으로 검색하여 등록
        /// (XXUnit_Recipe 패턴의 폼들을 자동으로 찾아서 등록)
        /// </summary>
        /// <param name="assemblyToSearch">검색할 어셈블리 (null이면 현재 어셈블리)</param>
        public void AutoRegisterUnitRecipeForms(System.Reflection.Assembly assemblyToSearch = null)
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
                    // Form을 상속받고 이름이 "Recipe"로 끝나는 클래스 찾기
                    if (typeof(Form).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        (type.Name.Contains("Unit_Recipe") || type.Name.Contains("UnitRecipe") || type.Name.EndsWith("Recipe")))
                    {
                        // Unit 이름 추출
                        string unitName = ExtractUnitNameFromType(type);
                        RegisterRecipeForm(type, unitName, $"{unitName} Recipe Management");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit Recipe 폼 자동 등록 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            // XXUnit_Recipe, XXUnitRecipe 패턴에서 XX 부분 추출
            if (typeName.Contains("Unit_Recipe"))
            {
                result = typeName.Replace("Unit_Recipe", "");
            }
            else if (typeName.Contains("UnitRecipe"))
            {
                result = typeName.Replace("UnitRecipe", "");
            }
            else if (typeName.EndsWith("Recipe"))
            {
                result = typeName.Replace("Recipe", "");
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
        /// Recipe용으로 등록된 폼들을 가져옴
        /// </summary>
        /// <returns>Recipe 폼 정보 리스트</returns>
        public List<FormInfo> GetRecipeForms()
        {
            return FormManager.Instance.GetRegisteredForms(MenuButtonType.Recipe);
        }

        /// <summary>
        /// 특정 Unit의 Recipe 폼을 생성
        /// </summary>
        /// <param name="unitName">Unit 이름</param>
        /// <returns>Recipe 폼 인스턴스</returns>
        public Form CreateRecipeForm(string unitName = null)
        {
            var recipeForms = GetRecipeForms();

            if (!string.IsNullOrEmpty(unitName))
            {
                var targetForm = recipeForms.Find(f => f.DisplayName.Contains(unitName));
                if (targetForm != null)
                {
                    return FormManager.Instance.CreateFormInstance(targetForm);
                }
                throw new ArgumentException($"{unitName}에 대한 Recipe 폼을 찾을 수 없습니다.");
            }

            if (recipeForms.Count >= 2)
            {
                return new FormRecipe();
            }

            // 첫 번째 등록된 폼 반환
            if (recipeForms.Count == 1)
            {
                return FormManager.Instance.CreateFormInstance(recipeForms[0]);
            }

            // 등록된 폼이 없으면 기본 폼 반환
            return CreateDefaultRecipeForm();
        }

        /// <summary>
        /// 기본 Recipe 폼 생성
        /// </summary>
        /// <returns>기본 Recipe 폼</returns>
        private Form CreateDefaultRecipeForm()
        {
            Form defaultForm = new Form
            {
                Text = "Recipe Management",
                BackColor = System.Drawing.Color.White
            };

            System.Windows.Forms.Label label = new System.Windows.Forms.Label
            {
                Text = "Recipe Management Area\n\nRegister your recipe forms using FormManagerRecipe.Instance.RegisterRecipeForm()\n\nOr use AutoRegisterUnitRecipeForms() to auto-discover XXUnit_Recipe forms",
                Font = new System.Drawing.Font("맑은 고딕", 12, System.Drawing.FontStyle.Regular),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            defaultForm.Controls.Add(label);
            return defaultForm;
        }

        /// <summary>
        /// 등록된 Recipe 폼들을 초기화하고 다시 로드
        /// </summary>
        public void RefreshRecipeForms()
        {
            FormManager.Instance.ClearRegistrations(MenuButtonType.Recipe);
            AutoRegisterUnitRecipeForms();
        }
    }
}