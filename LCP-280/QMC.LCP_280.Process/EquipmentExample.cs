using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// Equipment 사용 예시 및 테스트 클래스
    /// </summary>
    public static class EquipmentExample
    {
        /// <summary>
        /// Equipment 기본 사용 예시
        /// </summary>
        public static async Task BasicUsageExample()
        {
            try
            {
                Console.WriteLine("=== Equipment 기본 사용 예시 ===");
                
                // 1. Equipment 인스턴스 가져오기 (Singleton)
                var equipment = Equipment.Instance;
                
                // 2. 이벤트 구독
                equipment.StateChanged += (sender, e) => 
                    Console.WriteLine($"Equipment 상태 변경: {e.OldState} → {e.NewState}");
                    
                equipment.UnitStateChanged += (sender, e) => 
                    Console.WriteLine($"Unit '{e.UnitName}' 상태: {e.State}");
                    
                equipment.ErrorOccurred += (sender, e) => 
                    Console.WriteLine($"Equipment 오류: {e.ErrorMessage}");

                // 3. 등록된 Unit 목록 확인
                var unitNames = equipment.GetRegisteredUnitNames();
                Console.WriteLine($"등록된 Unit 수: {unitNames.Count}");
                foreach (var unitName in unitNames)
                {
                    Console.WriteLine($"  - {unitName}");
                }

                // 4. 모든 Unit 시작
                Console.WriteLine("\n모든 Unit 시작 중...");
                var startResult = await equipment.StartAllUnitsAsync();
                Console.WriteLine($"시작 결과: {(startResult ? "성공" : "실패")}");

                // 5. Unit 상태 확인
                await Task.Delay(2000); // 2초 대기
                var unitStatuses = equipment.GetAllUnitStatus();
                Console.WriteLine("\nUnit 상태:");
                foreach (var kvp in unitStatuses)
                {
                    var status = kvp.Value;
                    Console.WriteLine($"  {status.UnitName}: {status.State} | Components: {status.ComponentCount} | Runtime: {status.RunningTime:hh\\:mm\\:ss}");
                }

                // 6. 개별 Unit 제어 예시
                if (unitNames.Count > 0)
                {
                    var firstUnit = unitNames[0];
                    Console.WriteLine($"\n'{firstUnit}' Unit 개별 제어 테스트");
                    
                    // Unit 정지
                    await equipment.StopUnitAsync(firstUnit);
                    await Task.Delay(1000);
                    
                    // Unit 재시작
                    await equipment.StartUnitAsync(firstUnit);
                    await Task.Delay(1000);
                }

                // 7. Config 및 Recipe 관리 예시
                Console.WriteLine("\nConfig 및 Recipe 저장...");
                var configSaved = equipment.SaveAllConfigs();
                var recipeSaved = equipment.SaveAllRecipes();
                Console.WriteLine($"Config 저장: {(configSaved ? "성공" : "실패")}");
                Console.WriteLine($"Recipe 저장: {(recipeSaved ? "성공" : "실패")}");

                // 8. 모든 Unit 정지
                Console.WriteLine("\n모든 Unit 정지 중...");
                var stopResult = await equipment.StopAllUnitsAsync();
                Console.WriteLine($"정지 결과: {(stopResult ? "성공" : "실패")}");

                Console.WriteLine("\n=== Equipment 기본 사용 예시 완료 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Equipment 사용 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 새로운 Unit 등록 예시
        /// </summary>
        public static void RegisterNewUnitExample()
        {
            try
            {
                Console.WriteLine("=== 새로운 Unit 등록 예시 ===");
                
                var equipment = Equipment.Instance;
                
                // 새로운 Unit 등록 (예시로 기존 Unit을 다른 이름으로 등록)
                var newUnit = new Unit.CassetteLoadingElevator();
                equipment.RegisterUnit(newUnit, "TestCassetteElevator", "테스트용 카세트 엘리베이터");
                
                Console.WriteLine("새로운 Unit 등록 완료");
                
                // 등록된 Unit 목록 확인
                var unitNames = equipment.GetRegisteredUnitNames();
                Console.WriteLine($"총 등록된 Unit 수: {unitNames.Count}");
                
                // 등록 해제 예시
                equipment.UnregisterUnit("TestCassetteElevator");
                Console.WriteLine("테스트 Unit 등록 해제 완료");
                
                Console.WriteLine("=== 새로운 Unit 등록 예시 완료 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unit 등록 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// Config 및 Recipe 관리 예시
        /// </summary>
        public static void ConfigRecipeManagementExample()
        {
            try
            {
                Console.WriteLine("=== Config 및 Recipe 관리 예시 ===");
                
                var equipment = Equipment.Instance;
                var unitNames = equipment.GetRegisteredUnitNames();
                
                if (unitNames.Count > 0)
                {
                    var unitName = unitNames[0];
                    
                    // Config 가져오기 및 수정
                    var config = equipment.GetUnitConfig<Component.CassetteElevatorConfig>(unitName);
                    if (config != null)
                    {
                        //Console.WriteLine($"현재 {unitName} Config - Ready Position: {config.ReadyPosition}");
                        
                        // Config 수정
                        //config.ReadyPosition = 5.0;
                        //config.LoadingPosition = 15.0;
                        equipment.SetUnitConfig(unitName, config);
                        
                        //Console.WriteLine($"수정된 {unitName} Config - Ready Position: {config.ReadyPosition}");
                    }
                    
                    // Recipe 가져오기 및 수정
                    var recipe = equipment.GetUnitRecipe<CassetteElevatorRecipe>(unitName);
                    if (recipe != null)
                    {
                        Console.WriteLine($"현재 {unitName} Recipe - Move Speed: {recipe.MoveSpeed}");
                        
                        // Recipe 수정
                        recipe.MoveSpeed = 150.0;
                        recipe.Acceleration = 250.0;
                        equipment.SetUnitRecipe(unitName, recipe);
                        
                        Console.WriteLine($"수정된 {unitName} Recipe - Move Speed: {recipe.MoveSpeed}");
                    }
                    
                    // 저장
                    equipment.SaveAllConfigs();
                    equipment.SaveAllRecipes();
                    Console.WriteLine("Config 및 Recipe 저장 완료");
                }
                
                Console.WriteLine("=== Config 및 Recipe 관리 예시 완료 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Config/Recipe 관리 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// Equipment Control Form 실행
        /// </summary>
        public static void ShowEquipmentControlForm()
        {
            try
            {
                var controlForm = new EquipmentControlFormUnit_Main();
                controlForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Equipment Control Form 실행 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 모든 예시 실행 (CassetteLoadingElevator 포함)
        /// </summary>
        public static async Task RunAllExamples()
        {
            Console.WriteLine("Equipment 사용 예시 실행 시작\n");
            
            // CassetteLoadingElevator Unit 등록 확인
            RegisterCassetteLoadingElevatorUnit();
            await Task.Delay(1000);
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 기본 사용 예시
            await BasicUsageExample();
            await Task.Delay(1000);
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // CassetteLoadingElevator Unit 개별 테스트
            await TestCassetteLoadingElevatorUnit();
            await Task.Delay(1000);
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // Unit 등록 예시
            RegisterNewUnitExample();
            await Task.Delay(1000);
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // Config/Recipe 관리 예시
            ConfigRecipeManagementExample();
            
            Console.WriteLine("\nEquipment 사용 예시 실행 완료");
        }

        /// <summary>
        /// CassetteLoadingElevator 전용 예시 실행
        /// </summary>
        public static async Task RunCassetteElevatorExamples()
        {
            Console.WriteLine("CassetteLoadingElevator 전용 예시 실행 시작\n");
            
            // 1. Unit 등록 확인
            RegisterCassetteLoadingElevatorUnit();
            await Task.Delay(1000);
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            // 2. Unit 개별 테스트
            await TestCassetteLoadingElevatorUnit();
            
            Console.WriteLine("\nCassetteLoadingElevator 예시 실행 완료");
        }

        /// <summary>
        /// CassetteLoadingElevator Unit 등록 확인 및 추가 등록 예시
        /// </summary>
        public static void RegisterCassetteLoadingElevatorUnit()
        {
            try
            {
                Console.WriteLine("=== CassetteLoadingElevator Unit 등록 ===");
                
                var equipment = Equipment.Instance;
                
                // 현재 등록된 Unit 목록 확인
                var unitNames = equipment.GetRegisteredUnitNames();
                Console.WriteLine($"현재 등록된 Unit 수: {unitNames.Count}");
                
                foreach (var unitName in unitNames)
                {
                    Console.WriteLine($"  - {unitName}");
                }
                
                // CassetteLoadingElevator가 이미 등록되어 있는지 확인
                const string CASSETTE_UNIT_NAME = "CassetteLoadingElevator";
                
                if (equipment.Units.ContainsKey(CASSETTE_UNIT_NAME))
                {
                    Console.WriteLine($"\n? '{CASSETTE_UNIT_NAME}' Unit이 이미 등록되어 있습니다.");
                    
                    // Unit 상태 확인
                    var unitStatuses = equipment.GetAllUnitStatus();
                    if (unitStatuses.TryGetValue(CASSETTE_UNIT_NAME, out var status))
                    {
                        Console.WriteLine($"   상태: {status.State}");
                        Console.WriteLine($"   컴포넌트 수: {status.ComponentCount}");
                        Console.WriteLine($"   실행 중: {status.IsRunning}");
                        
                        if (status.IsRunning)
                        {
                            Console.WriteLine($"   실행 시간: {status.RunningTime:hh\\:mm\\:ss}");
                        }
                    }
                    
                    // Config 및 Recipe 확인
                    var config = equipment.GetUnitConfig<Component.CassetteElevatorConfig>(CASSETTE_UNIT_NAME);
                    if (config != null)
                    {
                        //Console.WriteLine($"   Config: Ready Position = {config.ReadyPosition}, Loading Position = {config.LoadingPosition}");
                    }
                    else
                    {
                        Console.WriteLine("   ?? Config가 없습니다. 기본 Config를 생성합니다.");
                        var defaultConfig = new Component.CassetteElevatorConfig();
                        equipment.SetUnitConfig(CASSETTE_UNIT_NAME, defaultConfig);
                    }
                    
                    var recipe = equipment.GetUnitRecipe<CassetteElevatorRecipe>(CASSETTE_UNIT_NAME);
                    if (recipe != null)
                    {
                        Console.WriteLine($"   Recipe: Move Speed = {recipe.MoveSpeed}, Acceleration = {recipe.Acceleration}");
                    }
                    else
                    {
                        Console.WriteLine("   ?? Recipe가 없습니다. 기본 Recipe를 생성합니다.");
                        var defaultRecipe = new CassetteElevatorRecipe();
                        equipment.SetUnitRecipe(CASSETTE_UNIT_NAME, defaultRecipe);
                    }
                }
                else
                {
                    Console.WriteLine($"\n? '{CASSETTE_UNIT_NAME}' Unit이 등록되어 있지 않습니다.");
                    Console.WriteLine("새로운 CassetteLoadingElevator Unit을 등록합니다...");
                    
                    // 새로운 CassetteLoadingElevator 인스턴스 생성 및 등록
                    var cassetteUnit = new Unit.CassetteLoadingElevator();
                    equipment.RegisterUnit(cassetteUnit, CASSETTE_UNIT_NAME, "카세트 로딩 엘리베이터 Unit");
                    
                    Console.WriteLine($"? '{CASSETTE_UNIT_NAME}' Unit 등록 완료");
                }
                
                // 최종 등록 상태 확인
                unitNames = equipment.GetRegisteredUnitNames();
                Console.WriteLine($"\n등록 완료 후 총 Unit 수: {unitNames.Count}");
                
                Console.WriteLine("\n=== CassetteLoadingElevator Unit 등록 완료 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CassetteLoadingElevator Unit 등록 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// CassetteLoadingElevator Unit 테스트 실행
        /// </summary>
        public static async Task TestCassetteLoadingElevatorUnit()
        {
            try
            {
                Console.WriteLine("=== CassetteLoadingElevator Unit 테스트 ===");
                
                var equipment = Equipment.Instance;
                const string CASSETTE_UNIT_NAME = "CassetteLoadingElevator";
                
                // Unit이 등록되어 있는지 확인
                if (!equipment.Units.ContainsKey(CASSETTE_UNIT_NAME))
                {
                    Console.WriteLine($"? '{CASSETTE_UNIT_NAME}' Unit이 등록되어 있지 않습니다.");
                    Console.WriteLine("먼저 RegisterCassetteLoadingElevatorUnit()을 실행하세요.");
                    return;
                }
                
                Console.WriteLine($"? '{CASSETTE_UNIT_NAME}' Unit 발견");
                
                // 이벤트 구독
                equipment.StateChanged += (sender, e) => 
                    Console.WriteLine($"   Equipment 상태: {e.OldState} → {e.NewState}");
                    
                equipment.UnitStateChanged += (sender, e) => 
                    Console.WriteLine($"   Unit '{e.UnitName}' 상태: {e.State}");
                
                // 1. Unit 개별 시작
                Console.WriteLine($"\n1. '{CASSETTE_UNIT_NAME}' Unit 시작 중...");
                var startResult = await equipment.StartUnitAsync(CASSETTE_UNIT_NAME);
                Console.WriteLine($"   시작 결과: {(startResult ? "성공" : "실패")}");
                
                if (startResult)
                {
                    // 2. 상태 확인 (3초 동안)
                    Console.WriteLine("\n2. Unit 상태 모니터링 (3초)...");
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Delay(1000);
                        var unitStatuses = equipment.GetAllUnitStatus();
                        if (unitStatuses.TryGetValue(CASSETTE_UNIT_NAME, out var status))
                        {
                            Console.WriteLine($"   [{i + 1}초] {status.UnitName}: {status.State} | Runtime: {status.RunningTime:mm\\:ss}");
                        }
                    }
                    
                    // 3. Config 테스트
                    Console.WriteLine("\n3. Config 테스트...");
                    var config = equipment.GetUnitConfig<Component.CassetteElevatorConfig>(CASSETTE_UNIT_NAME);
                    if (config != null)
                    {
                        //Console.WriteLine($"   현재 Ready Position: {config.ReadyPosition}");
                        //config.ReadyPosition = 5.0;
                        equipment.SetUnitConfig(CASSETTE_UNIT_NAME, config);
                        //Console.WriteLine($"   수정된 Ready Position: {config.ReadyPosition}");
                    }
                    
                    // 4. Recipe 테스트
                    Console.WriteLine("\n4. Recipe 테스트...");
                    var recipe = equipment.GetUnitRecipe<CassetteElevatorRecipe>(CASSETTE_UNIT_NAME);
                    if (recipe != null)
                    {
                        Console.WriteLine($"   현재 Move Speed: {recipe.MoveSpeed}");
                        recipe.MoveSpeed = 120.0;
                        equipment.SetUnitRecipe(CASSETTE_UNIT_NAME, recipe);
                        Console.WriteLine($"   수정된 Move Speed: {recipe.MoveSpeed}");
                    }
                    
                    // 5. Config/Recipe 저장
                    Console.WriteLine("\n5. Config/Recipe 저장...");
                    var configSaved = equipment.SaveAllConfigs();
                    var recipeSaved = equipment.SaveAllRecipes();
                    Console.WriteLine($"   Config 저장: {(configSaved ? "성공" : "실패")}");
                    Console.WriteLine($"   Recipe 저장: {(recipeSaved ? "성공" : "실패")}");
                    
                    // 6. Unit 정지
                    Console.WriteLine($"\n6. '{CASSETTE_UNIT_NAME}' Unit 정지 중...");
                    var stopResult = await equipment.StopUnitAsync(CASSETTE_UNIT_NAME);
                    Console.WriteLine($"   정지 결과: {(stopResult ? "성공" : "실패")}");
                }
                
                Console.WriteLine("\n=== CassetteLoadingElevator Unit 테스트 완료 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CassetteLoadingElevator Unit 테스트 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 간단한 실행 예시 - CassetteLoadingElevator 기본 테스트
        /// </summary>
        public static async Task QuickCassetteElevatorTest()
        {
            try
            {
                Console.WriteLine("=== CassetteLoadingElevator 간단 테스트 ===");
                
                var equipment = Equipment.Instance;
                
                // Unit 등록 확인
                RegisterCassetteLoadingElevatorUnit();
                
                // 전체 Equipment 시작
                Console.WriteLine("\n설비 전체 시작...");
                var startResult = await equipment.StartAllUnitsAsync();
                
                if (startResult)
                {
                    Console.WriteLine("? 설비 시작 성공");
                    
                    // 5초 동작
                    Console.WriteLine("5초 동작 중...");
                    for (int i = 1; i <= 5; i++)
                    {
                        await Task.Delay(1000);
                        var summary = equipment.GetSummary();
                        Console.WriteLine($"[{i}초] {summary}");
                    }
                    
                    // 설비 정지
                    Console.WriteLine("\n설비 전체 정지...");
                    var stopResult = await equipment.StopAllUnitsAsync();
                    Console.WriteLine(stopResult ? "? 설비 정지 성공" : "? 설비 정지 실패");
                }
                else
                {
                    Console.WriteLine("? 설비 시작 실패");
                }
                
                Console.WriteLine("\n=== 간단 테스트 완료 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"간단 테스트 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// Console Application Main 메서드 예시
        /// 이 메서드를 참고하여 실제 Main 메서드에서 사용하세요
        /// </summary>
        public static async Task ExampleMainMethod()
        {
            Console.WriteLine("LCP-280 Equipment System 시작");
            Console.WriteLine("============================");
            
            try
            {
                // 옵션 1: 간단한 테스트
                await QuickCassetteElevatorTest();
                
                Console.WriteLine("\n계속하려면 아무 키나 누르세요...");
                Console.ReadKey();
                
                // 옵션 2: 전체 예시 실행
                //await RunAllExamples();
                
                // 옵션 3: CassetteElevator 전용 예시
                //await RunCassetteElevatorExamples();
                
                // 옵션 4: GUI 실행
                //ShowEquipmentControlForm();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"프로그램 실행 중 오류: {ex.Message}");
                Console.WriteLine("계속하려면 아무 키나 누르세요...");
                Console.ReadKey();
            }
            
            Console.WriteLine("\n프로그램 종료");
        }
    }

    /// <summary>
    /// Equipment 관련 확장 메서드
    /// </summary>
    public static class EquipmentExtensions
    {
        /// <summary>
        /// Equipment의 현재 상태를 요약 정보로 가져오기
        /// </summary>
        public static EquipmentSummary GetSummary(this Equipment equipment)
        {
            var unitStatuses = equipment.GetAllUnitStatus();
            var runningUnits = 0;
            var stoppedUnits = 0;
            var errorUnits = 0;
            
            foreach (var status in unitStatuses.Values)
            {
                switch (status.State)
                {
                    case UnitState.Running:
                        runningUnits++;
                        break;
                    case UnitState.Stopped:
                        stoppedUnits++;
                        break;
                    case UnitState.Error:
                        errorUnits++;
                        break;
                }
            }
            
            return new EquipmentSummary
            {
                EquipmentState = equipment.State,
                TotalUnits = unitStatuses.Count,
                RunningUnits = runningUnits,
                StoppedUnits = stoppedUnits,
                ErrorUnits = errorUnits,
                LastUpdated = DateTime.Now
            };
        }
    }

    /// <summary>
    /// Equipment 요약 정보
    /// </summary>
    public class EquipmentSummary
    {
        public EquipmentState EquipmentState { get; set; }
        public int TotalUnits { get; set; }
        public int RunningUnits { get; set; }
        public int StoppedUnits { get; set; }
        public int ErrorUnits { get; set; }
        public DateTime LastUpdated { get; set; }

        public override string ToString()
        {
            return $"Equipment: {EquipmentState} | Units: {TotalUnits} (Running: {RunningUnits}, Stopped: {StoppedUnits}, Error: {ErrorUnits})";
        }
    }
}