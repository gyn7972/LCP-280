using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FormOrderAttribute : Attribute
    {
        public int Order { get; }

        public FormOrderAttribute(int order)
        {
            Order = order;
        }
    }

    /// <summary>
    /// 메뉴 타입별로 폼을 등록하고 관리하는 클래스
    /// </summary>
    public class FormManager
    {
        private Dictionary<MenuButtonType, List<FormInfo>> _registeredForms;
        private static FormManager _instance;
        
        public static FormManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FormManager();
                return _instance;
            }
        }

        private FormManager()
        {
            _registeredForms = new Dictionary<MenuButtonType, List<FormInfo>>();
            // 각 메뉴 타입에 대한 빈 리스트 초기화
            foreach (MenuButtonType menuType in Enum.GetValues(typeof(MenuButtonType)))
            {
                _registeredForms[menuType] = new List<FormInfo>();
            }
        }

        /// <summary>
        /// 폼을 특정 메뉴 타입에 등록
        /// </summary>
        /// <param name="menuType">메뉴 타입</param>
        /// <param name="formType">폼의 Type</param>
        /// <param name="displayName">UI에 표시될 이름</param>
        /// <param name="description">폼 설명 (옵션사항)</param>
        public void RegisterForm(MenuButtonType menuType, Type formType, string displayName, string description = null)
        {
            if (!typeof(Form).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{formType.Name}은 Form을 상속받아야 합니다.");
            }

            var formInfo = new FormInfo
            {
                FormType = formType,
                DisplayName = displayName,
                Description = description ?? displayName
            };

            _registeredForms[menuType].Add(formInfo);
        }

        // FormManager.cs에 오버로드 메서드 추가
        /// <summary>
        /// 폼을 특정 메뉴 타입에 순서와 함께 등록
        /// </summary>
        public void RegisterForm(MenuButtonType menuType, Type formType, string displayName, string description = null, int order = int.MaxValue)
        {
            if (!typeof(Form).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{formType.Name}은 Form을 상속받아야 합니다.");
            }

            var formInfo = new FormInfo
            {
                FormType = formType,
                DisplayName = displayName,
                Description = description ?? displayName,
                Order = order
            };

            _registeredForms[menuType].Add(formInfo);
        }

        /// <summary>
        /// 특정 메뉴 타입에 등록된 폼 정보들을 가져옴
        /// </summary>
        /// <param name="menuType">메뉴 타입</param>
        /// <returns>등록된 폼 정보 리스트</returns>
        public List<FormInfo> GetRegisteredForms(MenuButtonType menuType)
        {
            return new List<FormInfo>(_registeredForms[menuType]);
        }

        /// <summary>
        /// 등록된 폼의 인스턴스를 생성
        /// </summary>
        /// <param name="formInfo">폼 정보</param>
        /// <returns>생성된 폼 인스턴스</returns>
        public Form CreateFormInstance(FormInfo formInfo)
        {
            try
            {
                Form formInstance = (Form)Activator.CreateInstance(formInfo.FormType);
                
                // 자동으로 배경색을 흰색으로 설정
                formInstance.BackColor = Color.White;
                
                return formInstance;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"폼 {formInfo.FormType.Name} 생성 중 오류가 발생했습니다: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 모든 등록된 폼을 초기화
        /// </summary>
        public void ClearAllRegistrations()
        {
            foreach (var formList in _registeredForms.Values)
            {
                formList.Clear();
            }
        }

        /// <summary>
        /// 특정 메뉴 타입의 등록된 폼들을 초기화
        /// </summary>
        /// <param name="menuType">메뉴 타입</param>
        public void ClearRegistrations(MenuButtonType menuType)
        {
            _registeredForms[menuType].Clear();
        }
    }

    /// <summary>
    /// 등록된 폼의 정보를 담는 클래스
    /// </summary>
    public class FormInfo
    {
        public Type FormType { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int Order { get; set; } = int.MaxValue; // 기본값은 가장 뒤로, 순서 때문에 추가

    }
}