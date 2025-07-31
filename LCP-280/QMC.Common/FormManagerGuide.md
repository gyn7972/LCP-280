# FormManager 시스템 사용법

## 개요
FormManager 시스템은 FormBottom의 메뉴 버튼(Main, Working, Recipe, Config, Setup, Log)에 대응하는 폼들을 효율적으로 관리하는 시스템입니다.

## 시스템 구조

### 핵심 클래스들
- **FormManager**: 모든 폼을 중앙 집중식으로 관리하는 메인 매니저
- **FormManagerMain**: Main 메뉴 전용 매니저 (XXUnit_Main 패턴 자동 검색)
- **FormManagerConfig**: Config 메뉴 전용 매니저 (XXUnit_Config 패턴 자동 검색)
- **FormManagerWorking**: Working 메뉴 전용 매니저 (XXUnit_Working 패턴 자동 검색)
- **FormManagerRecipe**: Recipe 메뉴 전용 매니저 (XXUnit_Recipe 패턴 자동 검색)
- **FormManagerSetup**: Setup 메뉴 전용 매니저 (XXUnit_Setup 패턴 자동 검색)
- **FormManagerLog**: Log 메뉴 전용 매니저 (XXUnit_Log 패턴 자동 검색)

### 주요 기능
1. **자동 등록**: 각 메뉴 타입별로 XXUnit_YYY 패턴의 폼들을 자동 검색 및 등록
2. **탭 기반 UI**: Config 메뉴에서 등록된 폼들을 탭으로 자동 생성
3. **Lazy Loading**: 탭 선택시 폼 인스턴스를 동적으로 생성
4. **리소스 관리**: 폼 종료시 자동 리소스 정리
5. **기본 폼 제공**: 등록된 폼이 없을 때 안내 메시지가 포함된 기본 폼 표시

## 자동 등록 패턴

각 FormManager는 다음 패턴의 폼들을 자동으로 검색합니다:

### FormManagerMain
- `XXUnit_Main` (예: DieLoaderUnit_Main)
- `XXUnitMain` (예: DieLoaderUnitMain)
- `XXMain` (예: DieLoaderMain)

### FormManagerConfig
- `XXUnit_Config` (예: DieLoaderUnit_Config)
- `XXUnitConfig` (예: DieLoaderUnitConfig)
- `XXConfig` (예: DieLoaderConfig)

### FormManagerWorking
- `XXUnit_Working` (예: DieLoaderUnit_Working)
- `XXUnitWorking` (예: DieLoaderUnitWorking)
- `XXWorking` (예: DieLoaderWorking)
- `XXMonitor` (예: ProcessMonitor)
- `XXProcess` (예: DieProcess)

### FormManagerRecipe
- `XXUnit_Recipe` (예: DieLoaderUnit_Recipe)
- `XXUnitRecipe` (예: DieLoaderUnitRecipe)
- `XXRecipe` (예: DieLoaderRecipe)
- `XXParameter` (예: ProcessParameter)

### FormManagerSetup
- `XXUnit_Setup` (예: DieLoaderUnit_Setup)
- `XXUnitSetup` (예: DieLoaderUnitSetup)
- `XXSetup` (예: DieLoaderSetup)
- `XXCalibration` (예: VisionCalibration)
- `XXInitialize` (예: SystemInitialize)
- `XXInit` (예: SystemInit)

### FormManagerLog
- `XXUnit_Log` (예: DieLoaderUnit_Log)
- `XXUnitLog` (예: DieLoaderUnitLog)
- `XXLog` (예: DieLoaderLog)
- `XXLogger` (예: SystemLogger)
- `XXHistory` (예: ProcessHistory)
- `XXViewer` (예: LogViewer)

## 사용 방법

### 1. 자동 등록 방식 (권장)

각 메뉴 타입별로 위의 패턴에 맞게 폼을 생성하면 자동으로 등록됩니다.

```csharp
// QMC.LCP_280.Process.Unit 네임스페이스에 폼 생성
public partial class DieLoaderUnit_Config : Form
{
    public DieLoaderUnit_Config()
    {
        InitializeComponent();
        // 초기화 코드
    }
}

public partial class DieLoaderUnit_Working : Form
{
    public DieLoaderUnit_Working()
    {
        InitializeComponent();
        // 초기화 코드
    }
}
```

### 2. 수동 등록 방식

```csharp
// Application 시작시 또는 적절한 위치에서
FormManagerConfig.Instance.RegisterConfigForm(
    typeof(CustomConfigForm), 
    "Custom Unit", 
    "사용자 정의 Unit 설정"
);

FormManagerMain.Instance.RegisterMainForm(
    typeof(CustomMainForm), 
    "Custom Main", 
    "사용자 정의 메인 화면"
);

FormManagerWorking.Instance.RegisterWorkingForm(
    typeof(ProcessMonitorForm), 
    "Process Monitor", 
    "공정 모니터링"
);

FormManagerRecipe.Instance.RegisterRecipeForm(
    typeof(RecipeEditorForm), 
    "Recipe Editor", 
    "레시피 편집기"
);

FormManagerSetup.Instance.RegisterSetupForm(
    typeof(SystemSetupForm), 
    "System Setup", 
    "시스템 설정"
);

FormManagerLog.Instance.RegisterLogForm(
    typeof(SystemLogForm), 
    "System Log", 
    "시스템 로그"
);
```

### 3. 등록 위치

폼 등록은 다음 위치들에서 할 수 있습니다:

#### Application 시작시 (권장)
```csharp
// Program.cs 또는 Application 시작 부분
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    
    // 폼 등록
    RegisterForms();
    
    Application.Run(new FormMain());
}

private static void RegisterForms()
{
    // 모든 타입의 폼들 자동 등록
    FormManagerConfig.Instance.AutoRegisterUnitConfigForms();
    FormManagerMain.Instance.AutoRegisterUnitMainForms();
    FormManagerWorking.Instance.AutoRegisterUnitWorkingForms();
    FormManagerRecipe.Instance.AutoRegisterUnitRecipeForms();
    FormManagerSetup.Instance.AutoRegisterUnitSetupForms();
    FormManagerLog.Instance.AutoRegisterUnitLogForms();
    
    // 추가 수동 등록 (필요시)
    // FormManagerMain.Instance.RegisterMainForm(typeof(CustomMainForm), "Custom Main");
}
```

#### FormMain 초기화시 (현재 구현됨)
```csharp
// FormMain.cs의 InitializeFormManagers() 메서드에서
private void InitializeFormManagers()
{
    // 모든 타입 자동 등록
    FormManagerConfig.Instance.AutoRegisterUnitConfigForms();
    FormManagerMain.Instance.AutoRegisterUnitMainForms();
    FormManagerWorking.Instance.AutoRegisterUnitWorkingForms();
    FormManagerRecipe.Instance.AutoRegisterUnitRecipeForms();
    FormManagerSetup.Instance.AutoRegisterUnitSetupForms();
    FormManagerLog.Instance.AutoRegisterUnitLogForms();
}
```

### 4. 실행 시 동작

1. **Config 버튼 클릭**: FormConfig가 표시되고, 등록된 Config 폼들이 탭으로 나타남
2. **탭 선택**: 해당 폼이 동적으로 생성되어 탭 내에 표시됨
3. **다른 메뉴 클릭**: 등록된 첫 번째 폼이 표시되거나, 없으면 안내 메시지와 함께 기본 폼 표시

## 고급 기능

### 특정 Unit의 폼 생성
```csharp
// 특정 Unit의 Config 폼 생성
Form configForm = FormManagerConfig.Instance.CreateConfigForm("DieLoader");

// 특정 Unit의 Working 폼 생성
Form workingForm = FormManagerWorking.Instance.CreateWorkingForm("ProcessMonitor");
```

### 폼 목록 조회
```csharp
// 등록된 Config 폼 목록 조회
List<FormInfo> configForms = FormManagerConfig.Instance.GetConfigForms();

// 등록된 Working 폼 목록 조회
List<FormInfo> workingForms = FormManagerWorking.Instance.GetWorkingForms();
```

### 폼 재등록
```csharp
// Config 폼들 재등록 (새로 추가된 폼들 반영)
FormManagerConfig.Instance.RefreshConfigForms();

// 특정 Config 탭들 새로고침
FormConfig formConfig = // FormConfig 인스턴스 참조
formConfig.RefreshConfigTabs();
```

## 예시 구현

### DieLoader Unit 전체 폼 세트
```csharp
// Config 폼
public partial class DieLoaderUnit_Config : Form { /* 설정 UI */ }

// Main 폼  
public partial class DieLoaderUnit_Main : Form { /* 메인 작업 UI */ }

// Working 폼
public partial class DieLoaderUnit_Working : Form { /* 모니터링 UI */ }

// Recipe 폼
public partial class DieLoaderUnit_Recipe : Form { /* 레시피 관리 UI */ }

// Setup 폼
public partial class DieLoaderUnit_Setup : Form { /* 셋업/캘리브레이션 UI */ }

// Log 폼
public partial class DieLoaderUnit_Log : Form { /* 로그 뷰어 UI */ }
```

이 모든 폼들은 자동으로 등록되어 해당 메뉴 버튼을 클릭할 때 사용됩니다.

## 주의사항

1. **폼 생성자**: 등록할 폼은 매개변수 없는 기본 생성자가 있어야 합니다.
2. **네임스페이스**: 자동 검색은 QMC.LCP_280.Process 어셈블리에서 수행됩니다.
3. **리소스 관리**: 폼 종료시 자동으로 리소스가 정리되므로 별도 처리가 필요 없습니다.
4. **스레드 안전성**: UI 스레드에서만 사용해야 합니다.
5. **패턴 매칭**: 폼 이름은 정확한 패턴을 따라야 자동 검색됩니다.

## 확장 방법

새로운 메뉴 타입이 필요한 경우:
1. MenuButtonType enum에 새 타입 추가
2. 해당 타입용 FormManagerXXX 클래스 생성 (기존 FormManager들을 참고)
3. FormMain의 SwitchCenterForm 메서드에 케이스 추가
4. InitializeFormManagers에 자동 등록 호출 추가

이 시스템을 통해 폼 관리가 체계적이고 확장 가능하게 구성되며, 각 Unit별로 모든 메뉴 타입의 폼을 일관성 있게 관리할 수 있습니다.