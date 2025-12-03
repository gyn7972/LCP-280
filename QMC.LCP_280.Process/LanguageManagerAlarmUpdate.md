# LanguageManager - Alarm 언어 관리 업데이트

## 업데이트 내용

### 새로운 기능

LanguageManager에 **Alarm 언어 관리** 기능이 추가되었습니다.

#### 1. Alarm 정보 스캔
- Equipment의 모든 Unit에서 `m_dicAlarms` (Dictionary<int, AlarmInfo>) 필드를 재귀적으로 스캔
- AlarmInfo의 **Title**, **Cause**, **Grade** 속성 자동 수집
- **UnitName.AlarmCode** 형식으로 키 생성

#### 2. Alarm 언어 파일
새로운 파일 형식:
```ini
[AlarmTitle]
InputStage.3001 = Die Transfer Z축이 안전 위치가 아닙니다
OutputStage.3001 = Die Transfer Z축이 안전 위치가 아닙니다

[AlarmCause]
InputStage.3001 = Die Transfer Z-Axis가 안전 위치가 아닙니다. 상태를 확인 후 다시 시도 하십시오.
OutputStage.3001 = Die Transfer Z-Axis가 안전 위치가 아닙니다. 상태를 확인 후 다시 시도 하십시오.

[AlarmGrade]
InputStage.3001 = Error
OutputStage.3001 = Error
```

#### 3. 새로운 메서드

**LanguageManager.cs**에 추가된 메서드:
- `ScanEquipmentAlarms(object equipment)` - Alarm 스캔
- `SaveAlarmLanguage(string language)` - Alarm 언어 파일 저장
- `LoadAlarmLanguage(string language)` - Alarm 언어 파일 로드
- `ApplyAlarmLanguage(object equipment)` - Alarm에 언어 적용
- `GetAlarmTitle(string alarmKey, string defaultValue)` - Alarm Title 가져오기
- `GetAlarmCause(string alarmKey, string defaultValue)` - Alarm Cause 가져오기
- `GetAlarmGrade(string alarmKey, string defaultValue)` - Alarm Grade 가져오기

#### 4. LanguageSetupForm 업데이트
- **"Scan Alarms"** 버튼 추가
- Equipment, Alarm, Form을 개별적으로 스캔 가능
- UI 레이아웃 개선 (그룹박스 3개 → 4개)

### 파일 구조

생성되는 언어 파일:
```
Languages/
├── Equipment_Korean.ini    (Equipment 속성)
├── Equipment_English.ini
├── Alarm_Korean.ini        (NEW! Alarm 정보)
├── Alarm_English.ini       (NEW! Alarm 정보)
├── Form_Korean.ini         (Form 컨트롤)
└── Form_English.ini
```

## 사용 방법

### 1. Alarm 언어 파일 생성

#### 방법 A: LanguageSetupForm 사용 (권장)
```csharp
var setupForm = new LanguageSetupForm();
setupForm.ShowDialog();

// 1. Scan Equipment 클릭
// 2. Scan Alarms 클릭  ← NEW!
// 3. Scan All Forms 클릭
// 4. Save Korean 클릭
// 5. Save English 클릭
```

#### 방법 B: 코드로 직접
```csharp
var langManager = LanguageManager.Instance;
var equipment = Equipment.Instance;

// Alarm 스캔 및 저장
langManager.ScanEquipmentAlarms(equipment);
langManager.SaveAlarmLanguage("Korean");
langManager.SaveAlarmLanguage("English");
```

### 2. Alarm 언어 파일 편집

`Languages/Alarm_English.ini` 파일을 열어서 번역:
```ini
[AlarmTitle]
InputStage.3001 = Die Transfer Z-Axis Not at Safety Position
OutputStage.3001 = Die Transfer Z-Axis Not at Safety Position

[AlarmCause]
InputStage.3001 = Die Transfer Z-Axis is not at safety position. Please check status and retry.
OutputStage.3001 = Die Transfer Z-Axis is not at safety position. Please check status and retry.

[AlarmGrade]
InputStage.3001 = Error
OutputStage.3001 = Error
```

### 3. Alarm 언어 적용

#### 프로그램 시작 시
```csharp
// Program.cs 또는 MainForm_Load
var langManager = LanguageManager.Instance;
langManager.CurrentLanguage = "Korean";
langManager.LoadLanguage("Korean");

var equipment = Equipment.Instance;
langManager.ApplyEquipmentLanguage(equipment);
langManager.ApplyAlarmLanguage(equipment);  // ← NEW!
```

#### 언어 변경 시
```csharp
private void CboLanguage_SelectedIndexChanged(object sender, EventArgs e)
{
    string selectedLang = cboLanguage.SelectedItem.ToString();
  
    _langManager.CurrentLanguage = selectedLang;
    
    var equipment = Equipment.Instance;
    _langManager.ApplyEquipmentLanguage(equipment);
    _langManager.ApplyAlarmLanguage(equipment);  // ← NEW!
    _langManager.ApplyFormLanguage(this);
}
```

### 4. AlarmManager와 연동 (선택사항)

Alarm 발생 시 자동으로 현재 언어 적용:

```csharp
public class AlarmManager
{
    public void PostAlarm(AlarmInfo alarm)
    {
        // 언어 자동 적용
        var langManager = LanguageManager.Instance;
        
     if (alarm != null && !string.IsNullOrWhiteSpace(alarm.Source))
        {
         string alarmKey = $"{alarm.Source}.{alarm.Code}";
            
     alarm.Title = langManager.GetAlarmTitle(alarmKey, alarm.Title);
       alarm.Cause = langManager.GetAlarmCause(alarmKey, alarm.Cause);
  alarm.Grade = langManager.GetAlarmGrade(alarmKey, alarm.Grade);
        }
        
        // 기존 로직...
    Alarms.Add(alarm);
   OnPostAlarm?.Invoke(alarm);
    }
}
```

## Alarm 정의 예제

### Unit에서 Alarm 정의하기

```csharp
public class InputStage : BaseUnit<InputStageConfig>
{
    public enum AlarmKeys
    {
     eDieTransferPlaceZNotSafety = 3001,
        eOutputFeederCylinderZNotSafety = 3002,
 eOutputFeederYNotSafe = 3003,
    }

    protected override void InitAlarm()
  {
        base.InitAlarm();
        
        AlarmInfo alarm = new AlarmInfo();
        alarm.Code = (int)AlarmKeys.eDieTransferPlaceZNotSafety;
        alarm.Title = "Die Transfer Z축이 안전 위치가 아닙니다";
   alarm.Cause = "Die Transfer Z-Axis가 안전 위치가 아닙니다. 상태를 확인 후 다시 시도 하십시오.";
        alarm.Source = this.UnitName;  // ← 중요: Source 설정
        alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
        m_dicAlarms.Add(alarm.Code, alarm);
    }
}
```

**주의사항:**
1. ?? **AlarmInfo의 Source 속성을 반드시 설정**하세요
2. ?? 각 Unit마다 **고유한 AlarmCode 범위**를 사용하세요
3. ?? `m_dicAlarms`는 **protected 이상**으로 선언되어야 합니다

## Alarm 키 규칙

### 키 형식
```
UnitName.AlarmCode
```

### 예시
```
InputStage.3001    → InputStage의 3001번 알람
OutputStage.3001      → OutputStage의 3001번 알람
InputCassetteLifter.2001 → InputCassetteLifter의 2001번 알람
DieTransfer.4001         → DieTransfer의 4001번 알람
```

### AlarmCode 범위 권장
각 Unit마다 고유한 범위 사용:
```csharp
// InputStage
public enum AlarmKeys
{
    eError1 = 1001,
    eError2 = 1002,
}

// OutputStage
public enum AlarmKeys
{
  eError1 = 2001,
    eError2 = 2002,
}

// DieTransfer
public enum AlarmKeys
{
    eError1 = 3001,
    eError2 = 3002,
}
```

## 업데이트된 메서드

### SaveLanguage / LoadLanguage
이제 Alarm도 자동으로 포함됩니다:

```csharp
// 전체 저장 (Equipment + Alarm + Form)
_langManager.SaveLanguage("Korean");

// 전체 로드 (Equipment + Alarm + Form)
_langManager.LoadLanguage("Korean");
```

### ApplyLanguage
Equipment와 Alarm을 함께 적용:

```csharp
var equipment = Equipment.Instance;
var form = this;

// Equipment, Alarm, Form 모두 적용
_langManager.ApplyLanguage(equipment, form);
```

## 문제 해결

### Q: Alarm이 스캔되지 않아요
**A**: 
1. `m_dicAlarms`가 protected 또는 public으로 선언되어 있는지 확인
2. `InitAlarm()` 메서드가 호출되어 m_dicAlarms에 항목이 있는지 확인
3. BaseComponent를 상속받았는지 확인

### Q: Alarm 키가 중복됩니다
**A**:
1. 각 Unit마다 고유한 AlarmCode 범위 사용
2. enum으로 AlarmKeys를 정의하여 시작 값 지정
3. 언어 파일에서 `UnitName.AlarmCode` 형식으로 구분됨

### Q: 일부 Alarm만 번역됩니다
**A**:
1. AlarmInfo의 **Source 속성**이 올바르게 설정되어 있는지 확인
2. 언어 파일의 키 형식이 `UnitName.AlarmCode`인지 확인
3. Log 파일에서 스캔 중 오류 확인

### Q: Alarm이 여러 Unit에서 같은 코드를 사용해요
**A**:
괜찮습니다! 키가 `UnitName.AlarmCode` 형식이므로 구분됩니다:
```
InputStage.3001  ≠ OutputStage.3001
```

## 참고 문서

- `QMC.Common\LanguageManagerQuickStart.md` - 빠른 시작 가이드
- `QMC.Common\LanguageManagerGuide.md` - 상세 사용 가이드
- `QMC.Common\LanguageManager.cs` - 소스 코드
- `QMC.Common\LanguageSetupForm.cs` - 유틸리티 폼

## 변경 이력

### v2.0 (현재)
- ? Alarm 언어 관리 기능 추가
- ? LanguageSetupForm에 "Scan Alarms" 버튼 추가
- ? Alarm_*.ini 파일 형식 추가
- ?? 문서 업데이트

### v1.0 (이전)
- Equipment 속성 언어 관리
- Form 컨트롤 언어 관리
- LanguageSetupForm 유틸리티
