using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common
{
    /// <summary>
    /// ?? PropertyCollection을 가지고 있는 PropertyPosition 클래스
    /// PropertyBase를 상속받아 PropertyCollection을 포함하는 컨테이너 클래스
    /// PositionProperty 의존성 제거, PropertyCollection과 PropertyBase만 사용
    /// </summary>
    public class PropertyPosition : PropertyBase
    {
        /// <summary>
        /// Property들을 담고 있는 PropertyCollection
        /// </summary>
        public PropertyCollection PositionCollection { get; private set; }

        /// <summary>
        /// Position 그룹의 설명
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Position들이 편집 가능한지 여부
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Position 단위 (기본값: "mm")
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Position 그룹의 카테고리 (예: "Lifter", "WaferTransferArm", "Scanner")
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public PropertyPosition() : base()
        {
            PositionCollection = new PropertyCollection();
            PositionCollection.IsInputParameter = true; // 편집 가능하게 설정
            Description = string.Empty;
            IsEditable = true;
            Unit = "mm";
            Category = string.Empty;
        }

        /// <summary>
        /// Title을 지정하는 생성자
        /// </summary>
        /// <param name="title">Position 그룹 이름</param>
        public PropertyPosition(string title) : base(title, null)
        {
            PositionCollection = new PropertyCollection();
            PositionCollection.IsInputParameter = true;
            Description = string.Empty;
            IsEditable = true;
            Unit = "mm";
            Category = title ?? string.Empty;
        }

        /// <summary>
        /// 전체 매개변수를 지정하는 생성자
        /// </summary>
        /// <param name="title">Position 그룹 이름</param>
        /// <param name="description">설명</param>
        /// <param name="category">카테고리</param>
        /// <param name="unit">단위</param>
        /// <param name="isEditable">편집 가능 여부</param>
        public PropertyPosition(string title, string description, string category = "", string unit = "mm", bool isEditable = true) 
            : base(title, null)
        {
            PositionCollection = new PropertyCollection();
            PositionCollection.IsInputParameter = isEditable;
            Description = description ?? string.Empty;
            IsEditable = isEditable;
            Unit = unit ?? "mm";
            Category = category ?? title ?? string.Empty;
        }

        /// <summary>
        /// 기존 PropertyCollection을 사용하는 생성자
        /// </summary>
        /// <param name="title">Position 그룹 이름</param>
        /// <param name="existingCollection">기존 PropertyCollection</param>
        public PropertyPosition(string title, PropertyCollection existingCollection) : base(title, null)
        {
            PositionCollection = existingCollection ?? new PropertyCollection();
            Description = string.Empty;
            IsEditable = PositionCollection.IsInputParameter;
            Unit = "mm";
            Category = title ?? string.Empty;
        }

        #region PropertyBase 관리 메서드들

        /// <summary>
        /// ?? PropertyBase 추가
        /// </summary>
        /// <param name="property">추가할 PropertyBase</param>
        public void AddProperty(PropertyBase property)
        {
            if (property != null)
            {
                PositionCollection.Add(property);
                Console.WriteLine($"? Property 추가: {property.Title}");
            }
        }

        /// <summary>
        /// ?? 여러 PropertyBase들을 한 번에 추가
        /// </summary>
        /// <param name="properties">추가할 PropertyBase 배열</param>
        public void AddProperties(params PropertyBase[] properties)
        {
            if (properties != null)
            {
                foreach (var property in properties.Where(p => p != null))
                {
                    PositionCollection.Add(property);
                }
                Console.WriteLine($"? {properties.Length}개 Property 추가 완료");
            }
        }

        /// <summary>
        /// ?? 간단한 DoubleProperty 추가 (Title, Value만 지정)
        /// </summary>
        /// <param name="title">Property 이름</param>
        /// <param name="value">Property 값</param>
        public void AddDoubleProperty(string title, double value)
        {
            var doubleProperty = new DoubleProperty(title, value);
            PositionCollection.Add(doubleProperty);
            Console.WriteLine($"? DoubleProperty 추가: {title} = {value:F3}");
        }

        /// <summary>
        /// ?? Property 제거
        /// </summary>
        /// <param name="title">제거할 Property의 Title</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveProperty(string title)
        {
            var propertyToRemove = GetPropertyByTitle(title);
            if (propertyToRemove != null)
            {
                bool removed = PositionCollection.Remove(propertyToRemove);
                if (removed)
                {
                    Console.WriteLine($"? Property 제거: {title}");
                }
                return removed;
            }
            return false;
        }

        /// <summary>
        /// ?? 모든 Property 제거
        /// </summary>
        public void ClearProperties()
        {
            int count = PositionCollection.Count;
            PositionCollection = new PropertyCollection();
            PositionCollection.IsInputParameter = IsEditable;
            Console.WriteLine($"? 모든 Property 제거: {count}개 항목");
        }

        #endregion

        #region Property 검색 및 접근 메서드들

        /// <summary>
        /// ?? Title로 PropertyBase 찾기
        /// </summary>
        /// <param name="title">찾을 Property의 Title</param>
        /// <returns>PropertyBase (없으면 null)</returns>
        public PropertyBase GetPropertyByTitle(string title)
        {
            foreach (var prop in PositionCollection)
            {
                if (prop.Title == title)
                {
                    return prop;
                }
            }
            return null;
        }

        /// <summary>
        /// ?? 모든 PropertyBase들 반환
        /// </summary>
        /// <returns>PropertyBase 목록</returns>
        public List<PropertyBase> GetAllProperties()
        {
            var properties = new List<PropertyBase>();
            foreach (var prop in PositionCollection)
            {
                properties.Add(prop);
            }
            return properties;
        }

        /// <summary>
        /// ?? DoubleProperty들만 반환
        /// </summary>
        /// <returns>DoubleProperty 목록</returns>
        public List<DoubleProperty> GetDoubleProperties()
        {
            var doubleProperties = new List<DoubleProperty>();
            foreach (var prop in PositionCollection)
            {
                if (prop is DoubleProperty doubleProp)
                {
                    doubleProperties.Add(doubleProp);
                }
            }
            return doubleProperties;
        }

        /// <summary>
        /// ?? 특정 조건에 맞는 Property들 찾기
        /// </summary>
        /// <param name="predicate">조건</param>
        /// <returns>조건에 맞는 PropertyBase 목록</returns>
        public List<PropertyBase> FindProperties(Func<PropertyBase, bool> predicate)
        {
            return GetAllProperties().Where(predicate).ToList();
        }

        /// <summary>
        /// ?? Property 개수
        /// </summary>
        public int PropertyCount => PositionCollection.Count;

        /// <summary>
        /// ?? Property Title들을 배열로 반환
        /// </summary>
        /// <returns>Property Title 배열</returns>
        public string[] GetPropertyTitles()
        {
            return GetAllProperties().Select(p => p.Title).ToArray();
        }

        /// <summary>
        /// ?? DoubleProperty 값들을 배열로 반환
        /// </summary>
        /// <returns>DoubleProperty 값 배열</returns>
        public double[] GetDoublePropertyValues()
        {
            return GetDoubleProperties().Select(p => p.Value).ToArray();
        }

        #endregion

        #region PropertyBase 오버라이드

        /// <summary>
        /// 문자열에서 값 설정 (PropertyBase 오버라이드)
        /// JSON 형태나 특정 형식의 문자열에서 Property들을 파싱할 수 있음
        /// </summary>
        /// <param name="text">입력 문자열</param>
        public override void SetValue(string text)
        {
            try
            {
                // 간단한 예: "Property1:10.5,Property2:20.3" 형태 파싱
                if (!string.IsNullOrEmpty(text))
                {
                    var pairs = text.Split(',');
                    foreach (var pair in pairs)
                    {
                        var parts = pair.Split(':');
                        if (parts.Length == 2)
                        {
                            string title = parts[0].Trim();
                            if (double.TryParse(parts[1].Trim(), out double value))
                            {
                                var existingProperty = GetPropertyByTitle(title);
                                if (existingProperty != null && existingProperty is DoubleProperty doubleProp)
                                {
                                    doubleProp.Value = value;
                                }
                                else
                                {
                                    AddDoubleProperty(title, value);
                                }
                            }
                        }
                    }
                }
                
                base.Value = text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? PropertyPosition SetValue 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// PropertyPosition 정보를 문자열로 반환
        /// </summary>
        /// <returns>PropertyPosition 정보 문자열</returns>
        public override string ToString()
        {
            return $"{Title} ({Category}): {PropertyCount}개 Property";
        }

        #endregion

        #region 변환 및 호환성 메서드들

        /// <summary>
        /// ?? PropertyPosition 복사본 생성
        /// </summary>
        /// <returns>복사된 PropertyPosition</returns>
        public PropertyPosition Clone()
        {
            var cloned = new PropertyPosition(Title, Description, Category, Unit, IsEditable);
            
            var properties = GetAllProperties();
            foreach (var property in properties)
            {
                // PropertyBase의 복사본 생성 (간단한 복사)
                if (property is DoubleProperty doubleProp)
                {
                    cloned.AddDoubleProperty(doubleProp.Title, doubleProp.Value);
                }
                else
                {
                    var newProp = new PropertyBase(property.Title, property.Value);
                    cloned.AddProperty(newProp);
                }
            }

            return cloned;
        }

        #endregion

        #region 사용 예시를 위한 팩토리 메서드들
        /// <summary>
        /// ?? 사용자 정의 PropertyPosition 생성
        /// </summary>
        /// <param name="category">카테고리</param>
        /// <param name="positionData">Position 데이터 (Title:Value 쌍들)</param>
        /// <returns>생성된 PropertyPosition</returns>
        public static PropertyPosition CreateCustomPositions(string category, params (string title, double value)[] positionData)
        {
            var customPositions = new PropertyPosition($"{category} Positions", $"{category} 관련 Position들", category);
            
            if (positionData != null)
            {
                foreach (var (title, value) in positionData)
                {
                    customPositions.AddDoubleProperty(title, value);
                }
            }

            return customPositions;
        }

        #endregion
    }
}