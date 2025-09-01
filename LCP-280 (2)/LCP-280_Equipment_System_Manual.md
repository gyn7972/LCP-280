# LCP-280 Equipment System 매뉴얼

## ?? 목차
1. [개요](#개요)
2. [시스템 구조](#시스템-구조)
3. [Equipment 클래스](#equipment-클래스)
4. [Unit 관리](#unit-관리)
5. [Config 및 Recipe 관리](#config-및-recipe-관리)
6. [GUI 시스템](#gui-시스템)
7. [개발 가이드](#개발-가이드)
8. [사용 예시](#사용-예시)
9. [문제 해결](#문제-해결)

---

## 개요

LCP-280 Equipment System은 설비 회사에 최적화된 통합 설비 관리 시스템입니다. 
이 시스템은 다음과 같은 핵심 기능을 제공합니다:

### ? 주요 특징
- **중앙 집중식 설비 관리**: 모든 Unit을 Equipment에서 통합 관리
- **Thread 기반 독립 실행**: 각 Unit이 독립적인 Thread에서 동작
- **실시간 모니터링**: Unit 상태, 실행 시간, 오류 상태 실시간 추적
- **Config/Recipe 관리**: XML 기반 설정 및 레시피 저장/로드
- **확장 가능한 구조**: 새로운 Unit 쉽게 추가 가능
- **GUI 통합**: FormManager를 통한 체계적인 화면 관리

### ?? 대상 독자
- 설비 개발자
- 시스템 통합자
- 설비 운영자
- QA/테스트 엔지니어

---

## 시스템 구조

### ??? 전체 아키텍처

```
LCP-280 Equipment System
├── Equipment (중앙 제어)
│   ├── Units Management
│   │   ├── CassetteLoadingElevator
│   │   ├── WaferAlignment
│   │   ├── DieLoader
│   │   └── ... (사용자 정의 Units)
│   ├── Config Manager (XML 저장/로드)
│   ├── Recipe Manager (XML 저장/로드)
│   └── Thread Management
├── FormManager System
│   ├── Main Forms
│   ├── Config Forms
│   ├── Working Forms
│   ├── Recipe Forms
│   ├── Setup Forms
│   └── Log Forms
└── Common Components
    ├── BaseUnit
    ├── BaseConfig
    ├── PropertyCollectionView
    └── IOPropertyCollectionView
```

### ?? 프로젝트 구조

```
QMC.LCP_280.Process/
├── Equipment.cs                    # 중앙 설비 관리 클래스
├── EquipmentManagers.cs            # Config/Recipe 관리자
├── EquipmentControlForm.cs         # 설비 제어 GUI
├── EquipmentExample.cs             # 사용 예시
├── Unit/                           # Unit 구현
│   ├── CassetteLoadingElevator.cs
│   ├── *Unit_Config.cs             # Unit별 Config 폼
│   ├── *Unit_Working.cs            # Unit별 작업 폼
│   └── *Unit_Recipe.cs             # Unit별 Recipe 폼
└── Component/                      # 하위 컴포넌트
    ├── CassetteElevator.cs
    ├── WaferSlotScanner.cs
    └── ...

QMC.Common/
├── FormManager*.cs                 # 폼 관리 시스템
├── PropertyCollectionView.cs      # 설정 편집 컨트롤
├── IOPropertyCollectionView.cs    # I/O 상태 컨트롤
├── Unit/BaseUnit.cs               # Unit 기본 클래스
└── BaseConfig.cs                  # Config 기본 클래스
```

---

## Equipment 클래스

### ?? 핵심 기능

Equipment 클래스는 **Singleton 패턴**으로 구현되어 애플리케이션 전체에서 하나의 인스턴스만 존재합니다.

#### 1. 인스턴스 가져오기
```csharp
var equipment = Equipment.Instance;
```

#### 2. 주요 속성
| 속성 | 타입 | 설명 |
|------|------|------|
| `Units` | `ConcurrentDictionary<string, BaseUnit>` | 등록된 모든 Unit |
| `State` | `EquipmentState` | 설비 전체 상태 |
| `ConfigManager` | `EquipmentConfigManager` | Config 관리자 |
| `RecipeManager` | `EquipmentRecipeManager` | Recipe 관리자 |

#### 3. 설비 상태
```csharp
public enum EquipmentState
{
    Stopped,        // 정지됨
    Initializing,   // 초기화 중
    Ready,          // 준비됨
    Starting,       // 시작 중
    Running,        // 실행 중
    Stopping,       // 정지 중
    Error           // 오류 발생
}
```

#### 4. 주요 이벤트
```csharp
equipment.StateChanged += (sender, e) => 
    Console.WriteLine($"설비 상태: {e.OldState} → {e.NewState}");

equipment.UnitStateChanged += (sender, e) => 
    Console.WriteLine($"Unit '{e.UnitName}': {e.State}");

equipment.ErrorOccurred += (sender, e) => 
    Console.WriteLine($"오류: {e.ErrorMessage}");
```

---

## Unit 관리

### ?? Unit 등록

#### 1. 자동 등록 (Equipment 초기화 시)
```csharp
// Equipment 생성자에서 자동 실행
private void AutoRegisterUnits()
{
    RegisterUnit(new CassetteLoadingElevator(), "CassetteLoadingElevator");
    RegisterUnit(new WaferAlignmentUnit(), "WaferAlignment");
    RegisterUnit(new DieLoaderUnit(), "DieLoader");
}
```

#### 2. 수동 등록
```csharp
var equipment = Equipment.Instance;
var myUnit = new MyCustomUnit();
equipment.RegisterUnit(myUnit, "MyUnit", "사용자 정의 Unit");
```

#### 3. 등록 해제
```csharp
equipment.UnregisterUnit("MyUnit");
```

### ?? Unit 실행 제어

#### 1. 전체 Unit 제어
```csharp
// 모든 Unit 시작
var startResult = await equipment.StartAllUnitsAsync();

// 모든 Unit 정지
var stopResult = await equipment.StopAllUnitsAsync();
```

#### 2. 개별 Unit 제어
```csharp
// 특정 Unit 시작
await equipment.StartUnitAsync("CassetteLoadingElevator");

// 특정 Unit 정지
await equipment.StopUnitAsync("CassetteLoadingElevator");
```

#### 3. Unit 상태 확인
```csharp
var unitStatuses = equipment.GetAllUnitStatus();
foreach (var status in unitStatuses.Values)
{
    Console.WriteLine($"{status.UnitName}: {status.State} (Runtime: {status.RunningTime})");
}
```

### ?? Thread 관리

각 Unit은 독립적인 Thread에서 실행됩니다:

- **CancellationToken**: 안전한 Thread 종료
- **Task 기반**: `async/await` 패턴 사용
- **독립적 실행**: Unit 간 상호 영향 최소화
- **오류 격리**: 한 Unit 오류가 다른 Unit에 영향 없음

---

## Config 및 Recipe 관리

### ?? Config 관리

#### 1. Config 구조
```csharp
public abstract class BaseConfig
{
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime LastModified { get; set; }
    public PropertyCollection propertyBases { get; set; }
    
    public virtual bool Validate();
    public virtual void Reset();
}
```

#### 2. Config 사용
```csharp
// Config 가져오기
var config = equipment.GetUnitConfig<CassetteElevatorConfig>("CassetteLoadingElevator");

// Config 설정
config.ReadyPosition = 10.0;
equipment.SetUnitConfig("CassetteLoadingElevator", config);

// 모든 Config 저장
equipment.SaveAllConfigs();

// 모든 Config 로드
equipment.LoadAllConfigs();
```

### ?? Recipe 관리

#### 1. Recipe 구조
```csharp
public abstract class BaseRecipe
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
    public string CreatedBy { get; set; }
    
    public virtual bool Validate();
    public virtual void Reset();
}
```

#### 2. CassetteElevator Recipe 예시
```csharp
public class CassetteElevatorRecipe : BaseRecipe
{
    public double ReadyPosition { get; set; } = 0.0;
    public double LoadingPosition { get; set; } = 10.0;
    public double UnloadingPosition { get; set; } = 20.0;
    public double ScanningPosition { get; set; } = 15.0;
    public double MoveSpeed { get; set; } = 100.0;
    public double Acceleration { get; set; } = 200.0;
    public int SettlingTime { get; set; } = 500;
}
```

#### 3. Recipe 사용
```csharp
// Recipe 가져오기
var recipe = equipment.GetUnitRecipe<CassetteElevatorRecipe>("CassetteLoadingElevator");

// Recipe 설정
recipe.MoveSpeed = 150.0;
equipment.SetUnitRecipe("CassetteLoadingElevator", recipe);

// 모든 Recipe 저장/로드
equipment.SaveAllRecipes();
equipment.LoadAllRecipes();
```

### ?? 파일 저장 구조

#### Config 파일
```
Config/
├── CassetteLoadingElevator_Config.xml
├── WaferAlignment_Config.xml
└── DieLoader_Config.xml
```

#### Recipe 파일
```
Recipe/
├── CassetteLoadingElevator_Recipe.xml
├── WaferAlignment_Recipe.xml
└── DieLoader_Recipe.xml
```

---

## GUI 시스템

### ??? EquipmentControlForm

종합적인 설비 제어 화면을 제공합니다.

#### 주요 기능
- **실시간 모니터링**: Unit 상태, 실행 시간 표시
- **전체/개별 제어**: 설비 전체 또는 개별 Unit 제어
- **Config/Recipe 관리**: 설정 저장/로드
- **실시간 로그**: Operation 로그 표시

#### 사용법
```csharp
var controlForm = new EquipmentControlForm();
controlForm.ShowDialog();
```

### ??? Unit Config 폼

각 Unit별 설정 화면을 제공합니다.

#### CassetteLoadingElevatorUnit_Config 예시
```csharp
public partial class CassetteLoadingElevatorUnit_Config : Form
{
    private Equipment Equipment => Equipment.Instance;
    private CassetteLoadingElevator CassetteLoadingElevator { get; set; }
    
    // PropertyCollectionView를 통한 설정 편집
    // Equipment와 실시간 연동
    // Unit 상태 모니터링
}
```

### ?? FormManager 시스템

체계적인 폼 관리를 제공합니다.

#### 자동 등록 패턴
| 폼 타입 | 네이밍 패턴 | 예시 |
|---------|-------------|------|
| Config | `*Unit_Config` | `CassetteLoadingElevatorUnit_Config` |
| Working | `*Unit_Working` | `CassetteLoadingElevatorUnit_Working` |
| Recipe | `*Unit_Recipe` | `CassetteLoadingElevatorUnit_Recipe` |
| Setup | `*Unit_Setup` | `CassetteLoadingElevatorUnit_Setup` |
| Log | `*Unit_Log` | `CassetteLoadingElevatorUnit_Log` |

#### 수동 등록
```csharp
FormManagerConfig.Instance.RegisterConfigForm(
    typeof(MyUnitConfig), 
    "My Unit", 
    "사용자 정의 Unit 설정"
);
```

---

## 개발 가이드

### ?? 새로운 Unit 개발

#### 1. BaseUnit 상속
```csharp
public class MyCustomUnit : BaseUnit
{
    public MyCustomUnit() : base("MyCustomUnit")
    {
        AddComponents();
    }
    
    public override void AddComponents()
    {
        // 컴포넌트 추가
        var myComponent = new MyComponent();
        Components.Add(myComponent);
    }
    
    public override void OnRun()
    {
        base.OnRun();
        // Unit 시작 시 실행할 코드
    }
    
    public override void OnStop()
    {
        base.OnStop();
        // Unit 정지 시 실행할 코드
    }
}
```

#### 2. Config 클래스 생성
```csharp
public class MyCustomUnitConfig : BaseConfig
{
    public double Parameter1 { get; set; } = 100.0;
    public int Parameter2 { get; set; } = 50;
    public string Parameter3 { get; set; } = "Default";
    
    public MyCustomUnitConfig() : base("MyCustomUnitConfig")
    {
    }
    
    public override bool Validate()
    {
        return Parameter1 > 0 && Parameter2 > 0 && !string.IsNullOrEmpty(Parameter3);
    }
}
```

#### 3. Recipe 클래스 생성
```csharp
public class MyCustomUnitRecipe : BaseRecipe
{
    public double ProcessSpeed { get; set; } = 100.0;
    public int ProcessCount { get; set; } = 10;
    
    public MyCustomUnitRecipe() : base("MyCustomUnitRecipe")
    {
    }
    
    public override bool Validate()
    {
        return ProcessSpeed > 0 && ProcessCount > 0 && base.Validate();
    }
}
```

#### 4. Config 폼 생성
```csharp
public partial class MyCustomUnit_Config : Form
{
    private const string UNIT_NAME = "MyCustomUnit";
    private Equipment Equipment => Equipment.Instance;
    private MyCustomUnitConfig UnitConfig { get; set; }
    
    public MyCustomUnit_Config()
    {
        InitializeComponent();
        LoadConfig();
    }
    
    private void LoadConfig()
    {
        UnitConfig = Equipment.GetUnitConfig<MyCustomUnitConfig>(UNIT_NAME);
        if (UnitConfig == null)
        {
            UnitConfig = new MyCustomUnitConfig();
            Equipment.SetUnitConfig(UNIT_NAME, UnitConfig);
        }
    }
}
```

#### 5. Equipment에 등록
```csharp
// Equipment 초기화 시 자동 등록
private void AutoRegisterUnits()
{
    RegisterUnit(new MyCustomUnit(), "MyCustomUnit", "사용자 정의 Unit");
}
```

### ?? GUI 개발 가이드

#### PropertyCollectionView 사용
```csharp
private PropertyCollection CreateConfigProperties()
{
    var properties = new PropertyCollection { IsInputParameter = true };
    
    properties.Add(new TitleOnlyProperty("Basic Settings"));
    properties.Add(new PropertyBase("Parameter 1", UnitConfig.Parameter1.ToString("F2")));
    properties.Add(new PropertyBase("Parameter 2", UnitConfig.Parameter2.ToString()));
    properties.Add(new ComboBoxProperty("Mode", "Auto", new List<string> { "Auto", "Manual" }));
    
    return properties;
}
```

#### IOPropertyCollectionView 사용
```csharp
private PropertyCollection CreateIOProperties()
{
    var ioProperties = new PropertyCollection { ShowNoColumn = true };
    
    ioProperties.Add(new TitleOnlyProperty("No", "Name", "State"));
    ioProperties.Add(new PropertyState("X00", "Input Signal 1", true));
    ioProperties.Add(new PropertyState("Y01", "Output Signal 1", false));
    
    return ioProperties;
}
```

---

## 사용 예시

### ?? 기본 사용법

#### 1. Equipment 초기화 및 시작
```csharp
static async Task Main(string[] args)
{
    try
    {
        // Equipment 인스턴스 가져오기
        var equipment = Equipment.Instance;
        
        // 이벤트 구독
        equipment.StateChanged += (s, e) => 
            Console.WriteLine($"Equipment: {e.OldState} → {e.NewState}");
            
        equipment.UnitStateChanged += (s, e) => 
            Console.WriteLine($"Unit '{e.UnitName}': {e.State}");
        
        // 모든 Unit 시작
        Console.WriteLine("설비 시작 중...");
        var result = await equipment.StartAllUnitsAsync();
        
        if (result)
        {
            Console.WriteLine("설비 시작 완료");
            
            // 5초 동작
            await Task.Delay(5000);
            
            // 설비 정지
            Console.WriteLine("설비 정지 중...");
            await equipment.StopAllUnitsAsync();
            Console.WriteLine("설비 정지 완료");
        }
        else
        {
            Console.WriteLine("설비 시작 실패");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"오류: {ex.Message}");
    }
}
```

#### 2. GUI 애플리케이션
```csharp
[STAThread]
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    
    try
    {
        // Equipment Control Form 실행
        var controlForm = new EquipmentControlForm();
        Application.Run(controlForm);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"애플리케이션 실행 중 오류: {ex.Message}", "오류");
    }
}
```

#### 3. Config/Recipe 관리
```csharp
static void ConfigManagementExample()
{
    var equipment = Equipment.Instance;
    
    // Config 수정
    var config = equipment.GetUnitConfig<CassetteElevatorConfig>("CassetteLoadingElevator");
    if (config != null)
    {
        config.ReadyPosition = 5.0;
        config.MoveSpeed = 120.0;
        equipment.SetUnitConfig("CassetteLoadingElevator", config);
    }
    
    // Recipe 수정
    var recipe = equipment.GetUnitRecipe<CassetteElevatorRecipe>("CassetteLoadingElevator");
    if (recipe != null)
    {
        recipe.MoveSpeed = 150.0;
        recipe.Acceleration = 250.0;
        equipment.SetUnitRecipe("CassetteLoadingElevator", recipe);
    }
    
    // 저장
    equipment.SaveAllConfigs();
    equipment.SaveAllRecipes();
    
    Console.WriteLine("Config/Recipe 저장 완료");
}
```

### ?? 모니터링 예시

#### 실시간 상태 모니터링
```csharp
static async Task MonitoringExample()
{
    var equipment = Equipment.Instance;
    
    // 1초마다 상태 출력
    var timer = new Timer(async _ =>
    {
        var summary = equipment.GetSummary();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {summary}");
        
        var unitStatuses = equipment.GetAllUnitStatus();
        foreach (var status in unitStatuses.Values)
        {
            Console.WriteLine($"  {status.UnitName}: {status.State} | " +
                            $"Components: {status.ComponentCount} | " +
                            $"Runtime: {status.RunningTime:hh\\:mm\\:ss}");
        }
        Console.WriteLine();
        
    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    
    // 10초 모니터링
    await Task.Delay(10000);
    timer.Dispose();
}
```

---

## 문제 해결

### ? 자주 묻는 질문

#### Q1: Unit이 시작되지 않습니다.
**A:** 다음을 확인하세요:
1. Unit이 Equipment에 올바르게 등록되었는지 확인
2. Unit의 `OnRun()` 메서드가 구현되었는지 확인
3. Equipment 상태가 Ready 또는 Running인지 확인
4. 오류 로그 확인: `equipment.ErrorOccurred` 이벤트 구독

#### Q2: Config가 저장되지 않습니다.
**A:** 다음을 확인하세요:
1. Config 클래스가 직렬화 가능한지 확인 (public 속성)
2. 파일 저장 권한 확인
3. `equipment.SaveAllConfigs()` 호출 확인
4. Config가 Equipment에 올바르게 등록되었는지 확인

#### Q3: GUI에서 Unit 상태가 업데이트되지 않습니다.
**A:** 다음을 확인하세요:
1. 이벤트 핸들러가 올바르게 구독되었는지 확인
2. UI Thread에서 업데이트하는지 확인 (`Invoke` 사용)
3. Timer 간격이 적절한지 확인
4. Unit이 실제로 실행 중인지 확인

### ?? 일반적인 오류

#### 1. Thread 관련 오류
```csharp
// 잘못된 예
private void UpdateUI()
{
    label.Text = "Updated"; // Cross-thread 오류
}

// 올바른 예
private void UpdateUI()
{
    if (InvokeRequired)
    {
        Invoke(new Action(UpdateUI));
        return;
    }
    label.Text = "Updated";
}
```

#### 2. 메모리 누수 방지
```csharp
// IDisposable 구현
public void Dispose()
{
    timer?.Stop();
    timer?.Dispose();
    
    // 이벤트 구독 해제
    equipment.StateChanged -= Equipment_StateChanged;
    equipment.UnitStateChanged -= Equipment_UnitStateChanged;
}
```

#### 3. XML 직렬화 오류
```csharp
// XML 직렬화가 가능한 클래스 설계
public class MyConfig : BaseConfig
{
    // public 속성만 직렬화됨
    public string Parameter { get; set; }
    
    // 기본 생성자 필요
    public MyConfig() : base() { }
    
    // private/internal 속성은 직렬화되지 않음
    private string _internalValue;
}
```

### ?? 지원 및 문의

- **개발팀**: QMC Development Team
- **버전**: 1.0.0.0
- **지원 프레임워크**: .NET Framework 4.8
- **문서 최종 업데이트**: 2025년 1월

---

## 부록

### ?? 참고 자료

1. **C# 비동기 프로그래밍 가이드**: https://docs.microsoft.com/ko-kr/dotnet/csharp/async
2. **Thread-Safe Collections**: https://docs.microsoft.com/ko-kr/dotnet/standard/collections/thread-safe/
3. **XML 직렬화**: https://docs.microsoft.com/ko-kr/dotnet/standard/serialization/xml-serialization
4. **Windows Forms 개발**: https://docs.microsoft.com/ko-kr/dotnet/desktop/winforms/

### ?? 개발 도구

- **IDE**: Visual Studio 2019/2022
- **프레임워크**: .NET Framework 4.8
- **언어**: C# 7.3
- **UI**: Windows Forms
- **버전 관리**: Git

### ?? 성능 최적화 팁

1. **ConcurrentDictionary 사용**: Thread-safe 컬렉션 활용
2. **ConfigureAwait(false)**: UI가 아닌 코드에서 성능 향상
3. **Task.Run 활용**: CPU 집약적 작업을 백그라운드에서 실행
4. **메모리 관리**: IDisposable 패턴 적극 활용
5. **이벤트 구독 해제**: 메모리 누수 방지

---

*본 매뉴얼은 LCP-280 Equipment System v1.0.0을 기준으로 작성되었습니다.*
*최신 정보는 개발팀에 문의하시기 바랍니다.*