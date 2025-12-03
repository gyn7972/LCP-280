# LanguageManager 빠른 시작 가이드

## 1. 언어 파일 생성하기

### 방법 1: LanguageSetupForm 사용 (권장)

```csharp
// LanguageSetupForm 열기
var setupForm = new QMC.Common.LanguageSetupForm();
setupForm.ShowDialog();
```

LanguageSetupForm에서:
1. "Scan Equipment" 버튼 클릭
2. "Scan Alarms" 버튼 클릭
3. "Scan All Forms" 버튼 클릭
4. "Save Korean" 버튼 클릭
5. "Save English" 버튼 클릭

생성된 파일:
- `Languages/Equipment_Korean.ini`
- `Languages/Equipment_English.ini`
- `Languages/Alarm_Korean.ini`
- `Languages/Alarm_English.ini`
- `Languages/Form_Korean.ini`
- `Languages/Form_English.ini`

### 방법 2: 코드로 직접 생성

```csharp
// Equipment 스캔 및 저장
var langManager = LanguageManager.Instance;
var equipment = Equipment.Instance;

langManager.ScanEquipmentProperties(equipment);
langManager.ScanEquipmentAlarms(equipment);
langManager.SaveEquipmentLanguage("Korean");
langManager.SaveAlarmLanguage("Korean");
langManager.SaveEquipmentLanguage("English");
langManager.SaveAlarmLanguage("English");

// Form 스캔 및 저장
var mainForm = new MainForm();
langManager.ScanFormControls(mainForm);
langManager.SaveFormLanguage("Korean");
langManager.SaveFormLanguage("English");
```

## 2. 언어 파일 편집하기

생성된 INI 파일을 텍스트 에디터로 열어서 번역합니다.

### Equipment_Korean.ini 예시
```ini
[DisplayName]
Equipment.DieLoader.Speed = 속도
Equipment.DieLoader.Position = 위치
Equipment.Tester.Voltage = 전압
Equipment.Tester.MotionAxis.MaxVelocity = 최대 속도

[Category]
Equipment.DieLoader.Speed = 모션
Equipment.Tester.Voltage = 테스트
```

### Alarm_Korean.ini 예시
```ini
[AlarmTitle]
InputStage.3001 = Die Transfer Z축이 안전 위치가 아닙니다
OutputStage.3001 = Die Transfer Z축이 안전 위치가 아닙니다
InputCassetteLifter.2001 = Feeder Y축 안전 위치 오류

[AlarmCause]
InputStage.3001 = Die Transfer Z-Axis가 안전 위치가 아닙니다. 상태를 확인 후 다시 시도 하십시오.
OutputStage.3001 = Die Transfer Z-Axis가 안전 위치가 아닙니다. 상태를 확인 후 다시 시도 하십시오.
InputCassetteLifter.2001 = Feeder Y축이 안전 위치가 아닙니다. Feeder Y축을 안전 위치로 이동 후 다시 시도 하십시오.

[AlarmGrade]
InputStage.3001 = Error
OutputStage.3001 = Error
InputCassetteLifter.2001 = Error
```

### Alarm_English.ini 예시
```ini
[AlarmTitle]
InputStage.3001 = Die Transfer Z-Axis Not at Safety Position
OutputStage.3001 = Die Transfer Z-Axis Not at Safety Position
InputCassetteLifter.2001 = Feeder Y-Axis Safety Position Error

[AlarmCause]
InputStage.3001 = Die Transfer Z-Axis is not at safety position. Please check status and retry.
OutputStage.3001 = Die Transfer Z-Axis is not at safety position. Please check status and retry.
InputCassetteLifter.2001 = Feeder Y-Axis is not at safety position. Move to safety position and retry.

[AlarmGrade]
InputStage.3001 = Error
OutputStage.3001 = Error
InputCassetteLifter.2001 = Error
```

### Form_Korean.ini 예시
```ini
[Form]
MainForm.btnStart = 시작
MainForm.btnStop = 정지
MainForm.grpSettings = 설정
MainForm.grpSettings.lblSpeed = 속도
```

### Form_English.ini 예시
```ini
[Form]
MainForm.btnStart = Start
MainForm.btnStop = Stop
MainForm.grpSettings = Settings
MainForm.grpSettings.lblSpeed = Speed
```

## 3. 언어 적용하기

### MainForm에 언어 변경 기능 추가

```csharp
public partial class MainForm : Form
{
    private LanguageManager _langManager;
    private ComboBox cboLanguage;

    public MainForm()
    {
        InitializeComponent();
        
        _langManager = LanguageManager.Instance;
        
        // 언어 콤보박스 초기화
        SetupLanguageComboBox();
    }

    private void SetupLanguageComboBox()
    {
        // 사용 가능한 언어 목록 가져오기
   var languages = _langManager.GetAvailableLanguages();
   cboLanguage.Items.Clear();
        
        foreach (var lang in languages)
     {
            cboLanguage.Items.Add(lang);
        }
 
// 현재 언어 선택
        cboLanguage.SelectedItem = _langManager.CurrentLanguage;
     
// 언어 변경 이벤트
        cboLanguage.SelectedIndexChanged += CboLanguage_SelectedIndexChanged;
    }

    private void CboLanguage_SelectedIndexChanged(object sender, EventArgs e)
    {
   if (cboLanguage.SelectedItem != null)
        {
       string selectedLang = cboLanguage.SelectedItem.ToString();
            
 // 언어 변경
 _langManager.CurrentLanguage = selectedLang;
            
        // Equipment에 적용
  var equipment = Equipment.Instance;
    _langManager.ApplyEquipmentLanguage(equipment);
         _langManager.ApplyAlarmLanguage(equipment);
   
            // 현재 Form에 적용
            _langManager.ApplyFormLanguage(this);
         
            // 다른 열린 Form들에도 적용
          foreach (Form form in Application.OpenForms)
        {
  if (form != this)
       {
             _langManager.ApplyFormLanguage(form);
         }
  }
  }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        // 초기 언어 로드 및 적용
    _langManager.LoadLanguage(_langManager.CurrentLanguage);
   _langManager.ApplyFormLanguage(this);
  
    var equipment = Equipment.Instance;
        _langManager.ApplyEquipmentLanguage(equipment);
        _langManager.ApplyAlarmLanguage(equipment);
    }
}
```

### 간편 버전 (FormLanguageHelper 사용)

```csharp
public partial class MainForm : Form
{
    private FormLanguageHelper _langHelper;

    public MainForm()
 {
        InitializeComponent();
        _langHelper = new FormLanguageHelper(this);
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
     // 현재 언어 적용
        _langHelper.OnFormLoad();
        
        // ComboBox 자동 설정
        _langHelper.SetupLanguageComboBox(cboLanguage);
    }
}
```

## 4. 언어 파일 구조

### Alarm 경로 규칙

Alarm의 키는 다음과 같이 구성됩니다:
```
UnitName.AlarmCode
```

예시:
```
InputStage.3001
OutputStage.3001
InputCassetteLifter.2001
OutputCassetteLifter.2001
DieTransfer.4001
```

AlarmInfo의 Source 속성이 있으면 Source를 사용하고, 없으면 객체 경로를 사용합니다.

### Equipment 속성 경로 규칙

Equipment 속성의 경로는 다음과 같이 구성됩니다:
```
Equipment.UnitName.PropertyName.SubPropertyName
```

예시:
```
Equipment.DieLoader.MotionAxis.MaxVelocity
Equipment.Tester.Config.Voltage
Equipment.VisionUnit.Camera.ExposureTime
```

### Form 컨트롤 경로 규칙

Form 컨트롤의 경로는 다음과 같이 구성됩니다:
```
FormName.ParentControl.ChildControl.ControlName
```

예시:
```
MainForm.btnStart
MainForm.grpSettings.lblSpeed
ConfigForm.tabControl.tabMotion.btnSave
```

## 5. Alarm 언어 변경 동작 원리

1. **스캔**: 모든 Unit의 `m_dicAlarms` (Dictionary<int, AlarmInfo>) 필드를 재귀적으로 스캔
2. **키 생성**: `UnitName.AlarmCode` 형식으로 키 생성
3. **속성 수집**: AlarmInfo의 Title, Cause, Grade 속성 수집
4. **저장**: INI 파일로 저장 (AlarmTitle, AlarmCause, AlarmGrade 섹션)
5. **로드**: INI 파일에서 딕셔너리로 로드
6. **적용**: Equipment의 모든 Unit을 재귀 탐색하여 AlarmInfo 속성에 번역된 값 설정

## 6. 프로그램 시작 시 자동 언어 적용

```csharp
// Program.cs
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
      // 언어 설정 (환경 변수나 설정 파일에서 읽기)
        var lang = Properties.Settings.Default.Language ?? "Korean";
        var langManager = LanguageManager.Instance;
        langManager.CurrentLanguage = lang;
        langManager.LoadLanguage(lang);
        
  // Equipment 초기화 후 언어 적용
        var equipment = Equipment.Instance;
        langManager.ApplyEquipmentLanguage(equipment);
        langManager.ApplyAlarmLanguage(equipment);
        
        Application.Run(new MainForm());
    }
}
```

## 7. 문제 해결

### Q: Alarm이 번역되지 않아요
**A**: 
1. Alarm 파일이 올바른 경로에 있는지 확인 (`실행파일경로/Languages/Alarm_Korean.ini`)
2. `ScanEquipmentAlarms()` 호출 후 생성된 파일에서 키 형식 확인 (`UnitName.AlarmCode`)
3. `ApplyAlarmLanguage()` 호출 확인
4. AlarmInfo의 Source 속성이 올바른지 확인

### Q: 일부 Alarm만 번역됩니다
**A**:
1. m_dicAlarms가 public 또는 protected로 선언되어 있는지 확인
2. InitAlarm() 메서드에서 AlarmInfo의 Source 속성이 설정되어 있는지 확인
3. Log 파일에서 스캔 중 오류 메시지 확인

### Q: Alarm 코드가 중복됩니다
**A**:
1. 각 Unit마다 고유한 AlarmCode를 사용하세요
2. enum으로 AlarmKeys를 관리하여 중복 방지
3. 언어 파일에서 `UnitName.AlarmCode` 형식으로 구분

## 8. 팁

1. **Alarm 코드 관리**: enum으로 AlarmKeys를 정의하여 타입 안정성 확보

```csharp
public enum AlarmKeys
{
    eDieTransferPlaceZNotSafety = 3001,
    eOutputFeederCylinderZNotSafety = 3002,
    eOutputFeederYNotSafe = 3003,
}
```

2. **언어 파일 버전 관리**: Alarm 언어 파일도 Git에 포함시켜 관리

3. **번역 누락 확인**: 정기적으로 Alarm_Korean.ini와 Alarm_English.ini를 비교하여 누락된 번역 확인

4. **실시간 Alarm 표시**: Alarm 발생 시 현재 언어로 표시되도록 AlarmManager 수정 가능

5. **설정 저장**: 사용자가 선택한 언어를 Settings에 저장하여 재시작 후에도 유지

```csharp
// 언어 변경 시 저장
Properties.Settings.Default.Language = selectedLang;
Properties.Settings.Default.Save();
```

## 9. 고급: AlarmManager와 연동

AlarmManager에서 Alarm을 표시할 때 자동으로 현재 언어 적용:

```csharp
public class AlarmManager
{
    public void PostAlarm(AlarmInfo alarm)
    {
        // 언어 적용
        var langManager = LanguageManager.Instance;
        var equipment = Equipment.Instance;

        // 현재 Alarm에 언어 적용
        if (alarm != null && !string.IsNullOrWhiteSpace(alarm.Source))
     {
   string alarmKey = $"{alarm.Source}.{alarm.Code}";
        
            var title = langManager.GetAlarmTitle(alarmKey, alarm.Title);
     var cause = langManager.GetAlarmCause(alarmKey, alarm.Cause);
            var grade = langManager.GetAlarmGrade(alarmKey, alarm.Grade);
            
    alarm.Title = title;
     alarm.Cause = cause;
     alarm.Grade = grade;
        }
        
        // 기존 로직...
    }
}
