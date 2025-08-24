using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QMC.Common
{
    /// <summary>
    /// Recipe ????? ???? FormManager
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

        public void RegisterRecipeForm(Type formType, string displayName, string description = null)
        {
            FormManager.Instance.RegisterForm(MenuButtonType.Recipe, formType, displayName, description ?? displayName);
        }

        public void AutoRegisterUnitRecipeForms(System.Reflection.Assembly assemblyToSearch = null)
        {
            try
            {
                if (assemblyToSearch == null)
                {
                    var processAssembly = System.Reflection.Assembly.LoadFrom("QMC.LCP_280.Process.exe");
                    if (processAssembly != null)
                        assemblyToSearch = processAssembly;
                    else
                        assemblyToSearch = System.Reflection.Assembly.GetExecutingAssembly();
                }

                var types = assemblyToSearch.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(Form).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        (type.Name.Contains("Unit_Recipe") || type.Name.Contains("UnitRecipe") || type.Name.EndsWith("Recipe")))
                    {
                        string unitName = ExtractUnitNameFromType(type);
                        RegisterRecipeForm(type, unitName, $"{unitName} Recipe Management");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit Recipe ?? ??? ??? ?? ????: {ex.Message}", "????", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string ExtractUnitNameFromType(Type type)
        {
            string typeName = type.Name;

            if (typeName.Contains("Unit_Recipe"))
            {
                return typeName.Replace("Unit_Recipe", "").Replace("_", " ");
            }
            else if (typeName.Contains("UnitRecipe"))
            {
                return typeName.Replace("UnitRecipe", "");
            }
            else if (typeName.EndsWith("Recipe"))
            {
                return typeName.Replace("Recipe", "");
            }
            else if (typeName.EndsWith("Parameter"))
            {
                return typeName.Replace("Parameter", "");
            }
            else if (typeName.Contains("Recipe"))
            {
                return typeName.Replace("Recipe", "");
            }

            return typeName;
        }

        public List<FormInfo> GetRecipeForms()
        {
            return FormManager.Instance.GetRegisteredForms(MenuButtonType.Recipe);
        }

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
                throw new ArgumentException($"{unitName}?? ???? Recipe ???? ??? ?? ???????.");
            }

            if (recipeForms.Count >= 2)
            {
                return new FormRecipe();
            }

            if (recipeForms.Count == 1)
            {
                return FormManager.Instance.CreateFormInstance(recipeForms[0]);
            }

            return CreateDefaultRecipeForm();
        }

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
                Font = new System.Drawing.Font("???? ???", 12, System.Drawing.FontStyle.Regular),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            defaultForm.Controls.Add(label);
            return defaultForm;
        }

        public void RefreshRecipeForms()
        {
            FormManager.Instance.ClearRegistrations(MenuButtonType.Recipe);
            AutoRegisterUnitRecipeForms();
        }
    }
}