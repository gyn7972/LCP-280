# LCP-280 모션 아키텍처 개요

본 문서는 현재 워크스페이스의 모션 관련 구성 요소와 흐름을 Markdown으로 정리한 개요입니다.

## 주요 구성 요소
- IMotionAxis
  - 실제 모션 드라이버/컨트롤러를 추상화하는 인터페이스
  - MoveAbs, Jog, ServoOn/Off, GetActualPosition 등 명령 제공
- AxisDefinition
  - 하나의 IMotionAxis를 감싸고, 해당 축의 Position 목록(PropertyPosition)을 보관/생성
  - CreatePositionItem으로 위치/속도/가감속/타임아웃 등 DoubleProperty 자동 생성
- AxisManager
  - 여러 AxisDefinition을 등록/관리/검색
- AxisResolver
  - 이름 규칙에 따라 IMotionAxis를 해석/선택하는 헬퍼
- PropertyPosition / DoubleProperty
  - UI/설정 편집을 위한 속성 컨테이너. Position 값 편집, 직렬화, 검증 등에 사용
- BaseComponent
  - 컴포넌트 공통 기반. InitializeAxes/BuildPositionItemsFromConfig/SyncToConfig/ReloadFromConfig 제공
- BaseUnit
  - 여러 Component를 보유하는 상위 단위. 필요 시 Unit 차원에서 InitializeUnitAxes 수행
- CassetteElevator (+ CassetteElevatorConfig)
  - 단일 Z 축을 보유하는 컴포넌트
  - Config(LoadingZ/UnloadingZ/ReadyZ)를 기반으로 PositionItem 구성 및 동기화

## 구조 다이어그램
```
BaseUnit
└─ Components: BaseComponent[]
   └─ CassetteElevator (BaseComponent 상속)
      ├─ CassetteElevatorConfig
      │   ├─ LoadingZ
      │   ├─ UnloadingZ
      │   └─ ReadyZ
      ├─ AxisManager
      │   └─ AxisDefinition("Z", "CassetteElevator Z Axis", IMotionAxis Z)
      │       └─ PositionItems: PropertyPosition[]
      │           ├─ "CassetteElevator Loading Position"
      │           ├─ "CassetteElevator Unloading Position"
      │           └─ "CassetteElevator Ready Position"
      └─ AxisResolver: "Z" / "CassetteElevatorZ" / "CassetteZ" / "Z1" 등으로 축 검색
```

## 동작 흐름
1) Unit 초기화: BaseUnit.InitializeUnitAxes(provider)
   - provider가 제공하는 IMotionAxis[]를 각 Component.InitializeAxes(...)로 전달
2) CassetteElevator.InitializeAxes
   - AxisResolver로 Z 축 IMotionAxis 탐색 (이름 규칙: Z, CassetteElevatorZ, CassetteZ, Z1)
   - 찾으면 AxisManager.Register → AxisDefinition 등록
   - BuildPositionItemsFromConfig 호출
3) BuildPositionItemsFromConfig
   - AxisDefinition.CreatePositionItem으로 PositionItems 생성
   - 각 Position에 DoubleProperty 자동 추가
     - [축이름] = 위치값, Velocity, Acceleration, Deceleration, TimeoutMs
4) UI/Teaching 편집
   - PropertyPosition의 DoubleProperty를 통해 위치값/속도 등 편집
5) SyncToConfig
   - PositionItems에서 [축이름] DoubleProperty 값을 읽어 CassetteElevatorConfig에 저장
6) ReloadFromConfig
   - Config 값으로 PositionItems 재구성

## CassetteElevator의 Position 아이템 구성
- CassetteElevator Loading Position → Config.LoadingZ
- CassetteElevator Unloading Position → Config.UnloadingZ
- CassetteElevator Ready Position → Config.ReadyZ

## 명명 규칙(축 해석)
- 우선순위: "Z" → "CassetteElevatorZ" → "CassetteZ" → "Z1"

## 관련 코드 경로
- QMC.Common/IMotionAxis.cs
- QMC.Common/AxisDefinition.cs
- QMC.Common/AxisResolver.cs
- QMC.Common/Component/BaseComponent.cs
- QMC.Common/Unit/BaseUnit.cs