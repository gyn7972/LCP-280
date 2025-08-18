using System;
using System.Linq;

namespace QMC.Common.Examples
{
    /// <summary>
    /// ?? PropertyPosition 사용 예제
    /// PropertyCollection을 가지고 있는 PropertyPosition 클래스 활용법
    /// PositionProperty 의존성 제거, PropertyBase와 DoubleProperty만 사용
    /// </summary>
    public static class PropertyPositionUsageExample
    {
        /// <summary>
        /// ?? 예시 1: 기본 PropertyPosition 생성 및 사용
        /// </summary>
        public static void Example1_BasicUsage()
        {
            try
            {
                Console.WriteLine("=== 예시 1: 기본 PropertyPosition 사용법 ===");

                // 1. PropertyPosition 생성
                var lifterPositions = new PropertyPosition("Lifter Control", "리프터 제어용 Position들", "Lifter");

                // 2. DoubleProperty들 추가
                lifterPositions.AddDoubleProperty("Loading Position", 0.0);
                lifterPositions.AddDoubleProperty("Unloading Position", 50.0);
                lifterPositions.AddDoubleProperty("Ready Position", 25.0);
                lifterPositions.AddDoubleProperty("Maintenance Position", 100.0);

                // 3. 정보 출력
                Console.WriteLine($"PropertyPosition: {lifterPositions}");
                Console.WriteLine($"Property 개수: {lifterPositions.PropertyCount}");
                Console.WriteLine($"Property 목록: {string.Join(", ", lifterPositions.GetPropertyTitles())}");

                // 4. 특정 Property 값 조회
                var loadingPos = lifterPositions.GetPropertyByTitle("Loading Position");
                if (loadingPos is DoubleProperty doubleProp)
                {
                    Console.WriteLine($"Loading Position 값: {doubleProp.Value:F2} {lifterPositions.Unit}");
                }

                Console.WriteLine("? 예시 1 완료\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? 예시 1 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// ?? 예시 2: 팩토리 메서드 사용
        /// </summary>
        public static void Example2_FactoryMethods()
        {
            try
            {
                Console.WriteLine("=== 예시 2: 팩토리 메서드 사용 ===");

                // 1. 미리 정의된 Lifter Position들 생성
                var lifterPositions = PropertyPosition.CreateLifterPositions();
                Console.WriteLine($"Lifter Positions: {lifterPositions.PropertyCount}개");

                // 2. 미리 정의된 Feeder Position들 생성
                var feederPositions = PropertyPosition.CreateFeederPositions();
                Console.WriteLine($"Feeder Positions: {feederPositions.PropertyCount}개");

                // 3. 사용자 정의 Position들 생성
                var scannerPositions = PropertyPosition.CreateCustomPositions("Scanner",
                    ("Scan Start Position", 10.0),
                    ("Scan End Position", 90.0),
                    ("Scan Home Position", 0.0)
                );
                Console.WriteLine($"Scanner Positions: {scannerPositions.PropertyCount}개");

                // 4. 모든 Position 정보 출력
                var allPositionGroups = new[] { lifterPositions, feederPositions, scannerPositions };
                foreach (var group in allPositionGroups)
                {
                    Console.WriteLine($"  {group.Title}: {string.Join(", ", group.GetPropertyTitles())}");
                }

                Console.WriteLine("? 예시 2 완료\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? 예시 2 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// ?? 예시 3: PropertyCollection과 연동
        /// </summary>
        public static void Example3_PropertyCollectionIntegration()
        {
            try
            {
                Console.WriteLine("=== 예시 3: PropertyCollection 연동 ===");

                // 1. PropertyPosition 생성
                var positions = PropertyPosition.CreateLifterPositions();

                // 2. PropertyCollection 가져오기
                var propertyCollection = positions.PositionCollection;
                Console.WriteLine($"PropertyCollection 항목 수: {propertyCollection.Count}");
                Console.WriteLine($"편집 가능 여부: {propertyCollection.IsInputParameter}");

                // 3. PropertyCollection을 PropertyCollectionView에서 사용
                /*
                // 실제 UI에서 사용하는 경우:
                propertyCollectionView.SetProperties(propertyCollection);
                */

                // 4. 특정 Property 값 변경
                var loadingPos = positions.GetPropertyByTitle("Loading Position");
                if (loadingPos is DoubleProperty doubleProp)
                {
                    Console.WriteLine($"변경 전: {doubleProp.Title} = {doubleProp.Value:F2}");
                    doubleProp.Value = 15.5;
                    Console.WriteLine($"변경 후: {doubleProp.Title} = {doubleProp.Value:F2}");
                }

                // 5. 문자열로 여러 Property 값 설정
                positions.SetValue("Loading Position:5.0,Ready Position:30.0");
                Console.WriteLine("문자열 설정 후 값들:");
                foreach (var title in positions.GetPropertyTitles())
                {
                    var prop = positions.GetPropertyByTitle(title);
                    if (prop is DoubleProperty dp)
                    {
                        Console.WriteLine($"  {title}: {dp.Value:F2}");
                    }
                }

                Console.WriteLine("? 예시 3 완료\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? 예시 3 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// ?? 예시 4: DoubleProperty 활용
        /// </summary>
        public static void Example4_DoublePropertyUsage()
        {
            try
            {
                Console.WriteLine("=== 예시 4: DoubleProperty 활용 ===");

                // 1. PropertyPosition 생성
                var positions = new PropertyPosition("Test Positions", "테스트용 Position들");

                // 2. 다양한 DoubleProperty 추가
                positions.AddDoubleProperty("Test Position 1", 150.0);
                positions.AddDoubleProperty("Test Position 2", 50.0);
                positions.AddDoubleProperty("Test Position 3", -10.0);

                // 3. DoubleProperty들만 가져오기
                var doubleProperties = positions.GetDoubleProperties();
                Console.WriteLine($"DoubleProperty 개수: {doubleProperties.Count}");

                // 4. 모든 DoubleProperty 값 출력
                foreach (var doubleProp in doubleProperties)
                {
                    Console.WriteLine($"  {doubleProp.Title}: {doubleProp.Value:F2}");
                }

                // 5. DoubleProperty 값들을 배열로 가져오기
                var values = positions.GetDoublePropertyValues();
                Console.WriteLine($"값 배열: [{string.Join(", ", values.Select(v => v.ToString("F2")))}]");

                Console.WriteLine("? 예시 4 완료\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? 예시 4 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// ?? 예시 5: PropertyCollection과 직접 연동
        /// </summary>
        public static void Example5_DirectPropertyCollectionUsage()
        {
            try
            {
                Console.WriteLine("=== 예시 5: PropertyCollection 직접 연동 ===");

                // 1. PropertyPosition 생성
                var equipmentPositions = new PropertyPosition("Equipment Positions", "장비 Position들");
                equipmentPositions.AddDoubleProperty("Home Position", 0.0);
                equipmentPositions.AddDoubleProperty("Work Position", 100.0);
                equipmentPositions.AddDoubleProperty("Safe Position", 200.0);

                Console.WriteLine($"원본 PropertyPosition: {equipmentPositions.PropertyCount}개 Property");

                // 2. PropertyCollection 직접 접근
                var propertyCollection = equipmentPositions.PositionCollection;
                Console.WriteLine($"PropertyCollection: {propertyCollection.Count}개 항목");

                // 3. Property 값 변경
                var homePos = equipmentPositions.GetPropertyByTitle("Home Position");
                if (homePos is DoubleProperty doubleProp)
                {
                    doubleProp.Value = 5.0; // 값 변경
                    Console.WriteLine($"Home Position 값 변경: {doubleProp.Value:F2}");
                }

                // 4. PropertyCollection을 통한 UI 연동 가능
                /*
                // 실제 UI에서 사용하는 경우:
                propertyCollectionView.SetProperties(propertyCollection);
                */

                // 5. 변경된 값들 확인
                Console.WriteLine("모든 Property 값들:");
                foreach (var title in equipmentPositions.GetPropertyTitles())
                {
                    var prop = equipmentPositions.GetPropertyByTitle(title);
                    if (prop is DoubleProperty dp)
                    {
                        Console.WriteLine($"  {title}: {dp.Value:F2} {equipmentPositions.Unit}");
                    }
                }

                Console.WriteLine("? 예시 5 완료\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? 예시 5 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// ?? 예시 6: 실제 UI에서 사용하는 방법
        /// </summary>
        public static void Example6_UIUsage()
        {
            try
            {
                Console.WriteLine("=== 예시 6: 실제 UI 사용법 ===");

                // 1. 장비별 PropertyPosition들 생성
                var lifterPositions = PropertyPosition.CreateLifterPositions();
                var feederPositions = PropertyPosition.CreateFeederPositions();

                Console.WriteLine("장비별 PropertyPosition 생성 완료");

                // 2. PropertyCollectionView에 표시할 수 있는 형태로 준비
                /*
                // 실제 UI 코드 예시:
                
                // Lifter Position 편집
                listBoxItemsView.SetItems(lifterPositions.GetPropertyTitles());
                listBoxItemsView.PropertySelected += (sender, title) =>
                {
                    var selectedProperty = lifterPositions.GetPropertyByTitle(title);
                    if (selectedProperty != null)
                    {
                        var editorProperties = new PropertyCollection();
                        editorProperties.Add(selectedProperty);
                        propertyCollectionView.SetProperties(editorProperties);
                    }
                };

                // Save 버튼 클릭 시
                btnSave.Click += (sender, e) =>
                {
                    propertyCollectionView.Apply(); // PropertyCollectionView의 변경사항 적용
                    // lifterPositions의 값들이 자동으로 업데이트됨
                };
                */

                // 3. Property 값들을 Configuration에 저장/로드
                Console.WriteLine("Property 값들:");
                foreach (var title in lifterPositions.GetPropertyTitles())
                {
                    var prop = lifterPositions.GetPropertyByTitle(title);
                    if (prop is DoubleProperty dp)
                    {
                        Console.WriteLine($"  {title}: {dp.Value:F2} {lifterPositions.Unit}");
                    }
                }

                // 4. Configuration 파일 형태로 변환 가능
                var configString = string.Join(",", 
                    lifterPositions.GetDoubleProperties().Select(p => $"{p.Title}:{p.Value:F3}"));
                Console.WriteLine($"Config 문자열: {configString}");

                Console.WriteLine("? 예시 6 완료\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? 예시 6 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// ?? 모든 예시 실행
        /// </summary>
        public static void RunAllExamples()
        {
            Console.WriteLine("?? PropertyPosition 사용 예시들\n");
            
            Example1_BasicUsage();
            Example2_FactoryMethods();
            Example3_PropertyCollectionIntegration();
            Example4_DoublePropertyUsage();
            Example5_DirectPropertyCollectionUsage();
            Example6_UIUsage();
            
            Console.WriteLine("?? 모든 예시 완료!");
        }
    }
}