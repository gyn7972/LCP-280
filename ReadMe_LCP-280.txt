2025-10-12
1. 1차 Top기준 시컨스 진행.
 - 초기화 시 Tool, Index에 있는 제품 제거 시컨스 추가 할 것.
   -> 초기화 시에 추가 완료. 
 -:: 시컨스 진행 시 추가 필요.
 - Main Ready Button -> Ready 완료 될때까지 기다렸다가 진행되도록 수정 완료.
 -:: InputDieTransfer 정지 버튼 눌러도 하던 행위 완료 후 멈추게 수정 할 것.
 
 
 
* 개발 및 기능 구현 항목
1. 장비초기화 및 Ready 위치 이동 - 100%
   -> Safety 위치로 최적화 하도록 적용 할 것! 계속 확인 해야함
   
2. 시컨스테스트 스타트 스톱 기능 - 90%
   -> 인풋스테이지 맵핑전 인풋트렌스퍼 레디 위치 대기
   -> 언로더 다이 시에 언로더비젼얼라인 시 아웃풋트렌스퍼 레이 위치 대기
   -> Bottom 시컨스 검토 및 검증 해야함!!!
   
3. 택타임기능 - 50% 
   -> GUI 기능 필요!

4. 언어변경기능 - 0%
5. 언로더비젼얼라인기능 - 0%
6. 바코드기능 - 95%
   -> 시컨스에 등록 완료.
   -> Config에 사용 유/무 추가 및 적용 완료.!
   
7. 인터락 확인 - 90%
8. MES 기능 구현 - 0%
9. 조명컨트롤러교체 - 11/13 이 후 예정
10. BOTTOM 공정관련 축 삽입 2개 ( Z축, Gripper )
  - Z축 추가 - 90% ( 시컨스 및 기능 구현 )
  - Gripper - 11/13 이 후 예정
11. Bin Stage - T보상 기능 
12. 재현성 Test를 위한 시컨스 기능
13. 인덱스 Cal 기능구현   
  

 2025-11-03 월요일
 0. 설비 안착 및 전체 초기화 구동 확인 완료
 
 1. 초기화 홈 오프셋 적용할수있게 기능 수정
  -> ApplyHomeFromSetup 이 함수에 홈 메소드랑 속도 셋팅 후에 
  -> AjinDriver -> Home 함수에서 ApplyHomeFromSetup 셋팅하고
  -> HomeStart(축번호) 하면 되어야 한다.
  -> 초기화 후 Ready 위치로 보내는걸로 대체.
  -> Test 완료
  
 2. 프로그램 1차 업데이트
 -> 업데이트 완료.
 
 3. 25축 - 추가 축-> 홈 진행 시 1번 잡히고 2번째 알람발생.
                 -> 다시 또 잡으면 진행됨. -> 해결 완료. 리밋 조정 및 속도 조정
    -> 프로그램에서 상태값 받아오는 것 및 제어 안됨. 
	-> 축 번호 변경 필요해 보임. 
	-> index 축을 맨 마지막으로 보내 봐야함.
	-> Test 필요함.
	-> 장비 Test 완료
	
 4. Recipe -> Vision Dlg 처음 Open시 안열리는 문제 수정 필요.
 -> 예외처리로
 •	VS 2022: Debug > Windows > Exception Settings 
             > Managed Debugging Assistants > LoaderLock 체크 해제.
 -> 1차 완료
 
 5. 초기화 후 모터 축들 Ready 위치 이동 시컨스 추가 할 것.
 -> 코드 작업 완료 & 장비 Test 
 -> 완료
 
 6. Manual seq 기능 구현 필요.
	
	
 2025-11-04 화요일
 1. H/W 셋팅 확인 - 정차장
 
 2. EquipmentStatus class 작성
   -> 경광등 SW 상시 I/O 및 인터락 항목 적용
   -> 정리 필요 하자. 11/4(화) 퇴근 후.
   
 3. GUI -> 티칭 위치 이동 버튼 조건 및 인터락 기능 구현.
   -> Test 완료.
 
  2025-11-05 수요일
1. 정지 시 완전히 정지 되었다는 화면 표시 필요
2. 정지 시 속도 변화 발생.
 -> 런 속도에서 정지 되면서 오토->메뉴얼로 변경되면서
    조그 속도로 동작됨. 정지 버튼 눌렀을때 완전히 동작 
    끝나고 속도 적용 필요함
 => 위에꺼 같이 해결 될듯.
 
  -> var ok = await eq.StopAllUnitsAsync().ConfigureAwait(true);
  여기에서 task로 Progress바로 표시해서 전체 Unit 정지 되었으면
  정지로 표시하자. 
  
  

3. 정지 후 재 시작시 Ready 필요없이 구동되게 수정

4. 택타임 기능 구현 
 -> 파일로 저장되게 우선 남김.
 -> GUI 개발 필요. ( 주말에 하자 )

5. Die 로딩시 무조건 index에 칩 로딩하도록 모드 만들기.	
 -> 기능 적용 중.

6. Safety 위치 이동 시에... 인터락, 모션 완료 OK 신호 점검 필요. 
  -> Run 진행 시 문제되는 부분 while문으로 확인 후 진행하도록 코드 처리. 
  
7. 빈스테이지 T보정 기능 필요. 
  -> T보상 위한 작업.
  -> 준비 항목
   -> Theta 범위 내 확인. 
   -> 제품 안착 최 외곽에 Chip 한개씩 내려나야함.
   -> 링크구조이고 0.1mm 간격으로 최대로 움직여서 좌표를 전부 취득해야함. 
   -> 4쪽의 좌표를 따야함. 
   좌상단 / 우상단 칩 이용해서 0.1mm 간격으로 각도계산 해야함. +0.1도, -0.1에 좌표 4개
   이렇게 저장되어야함. data를 가지고 있어야함. 
   회전변환 알고리즘 개발 필요. 
   
   1) 장비 구동 시 4개 내려 놓는 위치를 최외각으로 설정해야함.
   
   0도 일떄 좌표 4개, 1도일떄 좌표 4개를 가지고 8개의 좌표를 가지고 
   회전변환 메트릭스 계산을 하고 
   
   0도 일떄 10,10에 놔야 할떄 메트릭스로 계산하면 됨. 11.1, 9.3 이렇게 위치 나옴
   
   기능구현 - 측정해서 Data를 얻을수있는 프로그램 필요함.
   -> 미리 준비 필요. 
   
   
2025-11-06(목)
1. 시컨스테스트 스타트 스톱 기능 - 90%
-> 인풋스테이지 맵핑전 인풋트렌스퍼 레디 위치 대기
-> 언로더 다이 시에 언로더비젼얼라인 시 아웃풋트렌스퍼 레이 위치 대기
-> Bottom 시컨스 검토 및 검증 해야함
   -> Test 필요.

-> 메카얼라인, 프로브 시컨스 변경 완료.
  1) safety -> index 회전 -> Z축 Ready -> z축 Up
  2) Up -> Safety
  
2. 택타임기능 - 50% 
   -> GUI 기능 필요!

3. 언로더비젼얼라인기능 - 90%

4. 바코드기능 - 95%
   -> 시컨스에 등록 완료.
   -> Config에 사용 유/무 추가 및 적용 완료.!

5. Manual 기능 수정.
   -> inputTr : pickUp/pickDown으로 구분?
   
   
etc. -> 오늘 꼭 하자.
1. 정지 시 완전히 정지 되었다는 화면 표시 필요.
---------
1. 정지 시 완전히 정지 되었다는 화면 표시 필요.
2. 정지 시 속도 변화 발생.
 -> 런 속도에서 정지 되면서 오토->메뉴얼로 변경되면서
    조그 속도로 동작됨. 정지 버튼 눌렀을때 완전히 동작 
    끝나고 속도 적용 필요함.
 => 위에꺼 같이 해결 될듯.
 
  -> var ok = await eq.StopAllUnitsAsync().ConfigureAwait(true);
  여기에서 task로 Progress바로 표시해서 전체 Unit 정지 되었으면
  정지로 표시하자. 
---------
  -> 시작시에도 표시 고민하자.
  

2. GUI에서 모션 구동 시 Progress바 보이도록 처리하자. 
  
3. EquipmentStatus class 작성.
   -> 경광등 SW 상시 I/O 및 인터락 항목 적용.
   -> 상시 알람 확인 후 알람 발생 기능 구현.
      -> 프로그램 처음 시작 시 무조건 1차 알람 발생. ( 수정필요 !!!)
	  
4. Recipe -> Measurment 탭 열릴때 프로그램 다운 현상 버그 수정 필요!!!!

5. Vision - 해상력 사이즈 불러오기 / 저장 되게 하고 
   카메라 해상력에 따라 이미지뷰어 변경 필요!!!
   
6. InputCassetteLifterConfig -> 변수 등록시 저장/불러오기 안됨.
 -> 완료

7. 시컨스에 알람 전부 추가 할것!!!
 -> 이거 해야함!!!
 
2025-11-07(금)

1. GUI Die화면 수정. 
 1) wafer, bin 180도 회전 해야함.!
 2) 놓친거 표시 해야함.!
 -> View 표시는 해결 완료. 
 -> Bin Stage에 내려 놓는거는 아직 위에서부터 내려 놓음.
 -> 재현성Test를 위해 좌하단부터 내려놓는걸로 수정필요.!!!!

2. Vision 화면에 매칭 표시 해야함. 
 -> Test 화면 말고 operater 화면에 찾은거 맵핑 표시.

2. 맴핑 전부 잘 찾는지 검증 필요. 
3. T 얼라인 제대로 되는지 검증 필요.
 -> 모니터링중 ( 현재까지는 맵핑 후 2차 얼라인 수행 후 진행 중 )
 
5. 언로더 비젼 -> 언로더 하는 스탭. 택타임 로스 발생. 
 -> 문제 해결 필요함...
 -> 11/8(토) 1차 수정 적용. ( 딜레이 걸리는 현상 X )

6. 제품 제거 기능 필요. 
 -> 인덱스는 클리어 수행. 
 -> 모든 유닛 ready 위치 수행. 
 -> 웨이퍼스테이지, 빈스테이지는 언로더 수행.
 
 -> 현재 리셋버튼에서 웨이퍼, 빈 언로더까지 수행하면 될꺼 같다. 
 
11. Bin Stage - T보상 기능 
-> Theta 범위 내 확인. 
   -> 제품 안착 최 외곽에 Chip 한개씩 내려나야함. (4면 꼭지점)
    -> 내려놓던가 있다고 가정?
    
	*T보상 모드시에 Bin - mapping시에 사이즈에서 가장 최외각 4점만
	내려놓게 설정.
   
   - 각 제품당 +-0.1mm간격으로 움직이면서 좌표를 취득.
    -> 매트릭스해야하기때문에 Data가 파일로 저장되어서 남아야함.
	
	*-- 시컨스 기능 구현
	 1) 우상단 좌표 센터로 이동
	 2) 이동 후 +-0.1mm씩 카메라 FOV안에 들어오는 범위에서 
	    마크 서치 후 좌표 취득.(이미지좌표, 모션좌표)
     3) Data 저장.
	 4) 나머지 3개의 좌표도 1~3번까지 시컨스 수행.
	 
	 *Data 파일명: BinThetaMet_시분초.
	 *정지했다가 다시 진행할때 시컨스 필요.
	 *리셋 후 처음부터 다시 시작 필요.
	-----------------------------------------------------
	
	*-- 시컨스 기능 구현
	1) 좌상단/우상단 최외각에 있는 칩을 가지고 0.1mm씩 Theta를 돌리면서
	   마크 서치.
	2) 두 개의 마크를 찾은 후 각도 계산.
	3) 각도를 계산한 후 상위에서 구한 파일의 4개의 제품옆에 +-0.1mm씩 구한
       각도값을 저장해야함. 
    ------------------------------------------------------

    - Data를 구한 후 회전변환 알고리즘 개발해야함.
	1) 0도 일때 좌표 4개, 1도일때 좌표 4개 총 8개의 좌표를 가지고
	   회전변환 메트릭스 계산
	2) 제품은 안착전에 10,10에 내려놔야 한다면 10.5, 9.9 이런식으로 제품을 안착하게 됨.
	
12. 인덱스 Cal를 위한 시컨스 기능. -> 기능 구현 및 테스트 완료.
 - 웨이퍼 로딩. (Data도 AutoRun과 동일하게 가지고 있어야함.)
 - 다이를 인덱스 1번에 제품 안착. 
 - 로딩얼라인
 - 프로브검사
 - 로딩위치
 - place로 다이 들기
 - 인덱스 2번 회전
 - 회전 후 2번에 제품 안착.
 - 로딩얼라인
 - 프로브검사
 - 로딩위치
 *소켓 8개 반복 진행.
 *Data 파일명 재현성Test_시분초.
 *정지했다가 다시 진행할때 시컨스 필요.
 *리셋 후 처음부터 다시 시작 필요.
 *다음꺼 다이로 진행 필요.

13. 재현성 Test 모드 적용 후 AutoRun 수행.
 * LCP-280 장비 특성상 180도 회전하여 제품을 언로딩함.
 - 우상단부터 로딩하면 -> 좌하단부터 언로딩해야함.
 - 만약에...제품을 로딩하지 못했다면 언로더시에도 언로딩 위치를 패스 한 후에 
   제품을 내려놓아야함.
   -> 우선 180도는 적용 완료.
   -> Test 필요함. 
   -> wafer 맵핑된 좌표대로 180도 회전해서 binStage에 내려나야돼.
 -> 맵핑할때 4면중에 어디서부터 시작할껀지 정해야함.
 -> 지그재그 모드도 필요함.
 -> 다이 좌표값도 남겨놔야함.
 
14. 공정Data에 따라 보상 Offset 개념
   -> 소켓별로... Recipe 기능 구현 완료. 
   
15. 맵핑시에 좌표 겹치는거 처리 필요함.!!!
  -> code 구현 완료. 장비Test 필. -> 11/8(토) RunTest 1차 완료
  -> 모니터링중.
  
  
  
11/8(토)

 
2. 맵핑시에 좌표 겹치는거 처리 필요함.!!!
  -> code 구현 완료. 장비Test 필. -> 11/8(토) RunTest 1차 완료
  -> 모니터링중.
3. 공정Data에 따라 보상 Offset 개념
   -> 소켓별로... Recipe 기능 구현 완료. 
4. 인덱스 Cal를 위한 시컨스 기능. -> 기능 구현 및 테스트 완료.


1. GUI Die화면 수정. 
 1) wafer, bin 180도 회전 해야함.!
 2) 놓친거 표시 해야함.!
 -> View 표시는 해결 완료. 
 -> Bin Stage에 내려 놓는거는 아직 위에서부터 내려 놓음.
 -> 재현성Test를 위해 좌하단부터 내려놓는걸로 수정필요.!!!!
 -> 수정함.!!!!!!!!!!!!!!!!!!!!!!!!!!!
 
5. 제품 제거 기능 필요. 
 -> 인덱스는 클리어 수행. 
 -> 모든 유닛 ready 위치 수행. 
 -> 웨이퍼스테이지, 빈스테이지는 언로더 수행.
 -> 현재 리셋버튼에서 웨이퍼, 빈 언로더까지 수행하면 될꺼 같다. !!!
 
6. GUI에서 waferID 입력 및 생성 및 제거 기능 필요. !!!
7. GUI에서 Index Die 생성 및 제거 기능 필요. !!!
8. GUI에서 Cassette ID 생성 및 제거, Cassette 내부 Wafer 생성/삭제 기능 구현 필요. !!!
-> wafer ID 입력도 필요. !!!

9. 재현성 Test 모드 적용 후 AutoRun 수행.        !!!!!!!!!!!!!!!!!!!!!!!!!!!
 * LCP-280 장비 특성상 180도 회전하여 제품을 언로딩함.
 - 우상단부터 로딩하면 -> 좌하단부터 언로딩해야함.
 - 만약에...제품을 로딩하지 못했다면 언로더시에도 언로딩 위치를 패스 한 후에 
   제품을 내려놓아야함.
   -> 우선 180도는 적용 완료.
   -> Test 필요함. 
   -> wafer 맵핑된 좌표대로 180도 회전해서 binStage에 내려나야돼.
 -> 맵핑할때 4면중에 어디서부터 시작할껀지 정해야함. -> 코드상으로 정할수있음.
 -> 지그재그 모드도 필요함. -> 지그재그 모드 적용 완료.
 -> 다이 좌표값도 남겨놔야함.
 
10. 시컨스에 알람 전부 추가 할것!!!
 -> 이거 해야함!!! --------------------> 했음.
 -> 앞으로 수정하면서 계속 정리 할것.!!!
 
11. EquipmentStatus class 작성.
   -> 경광등 SW 상시 I/O 및 인터락 항목 적용.
   -> 상시 알람 확인 후 알람 발생 기능 구현.
      -> 프로그램 처음 시작 시 무조건 1차 알람 발생. ( 수정필요 !!!)
	  
12. Recipe -> Measurment 탭 열릴때 프로그램 다운 현상 버그 수정 필요!!!!
  -> 1차 확인 :: 현재 장비에서 프로그램 다운 안됨. 
  -> 모니터링중

13. Vision - 해상력 사이즈 불러오기 / 저장 되게 하고 
   카메라 해상력에 따라 이미지뷰어 변경 필요!!!
   
14. Bin Stage - T보상 기능  !!!!!!!!!!!!!!!
-> Theta 범위 내 확인. 
   -> 제품 안착 최 외곽에 Chip 한개씩 내려나야함. (4면 꼭지점)
    -> 내려놓던가 있다고 가정?
    
	*T보상 모드시에 Bin - mapping시에 사이즈에서 가장 최외각 4점만
	내려놓게 설정.
   
   - 각 제품당 +-0.1mm간격으로 움직이면서 좌표를 취득.
    -> 매트릭스해야하기때문에 Data가 파일로 저장되어서 남아야함.
	
	*-- 시컨스 기능 구현
	 1) 우상단 좌표 센터로 이동
	 2) 이동 후 +-0.1mm씩 카메라 FOV안에 들어오는 범위에서 
	    마크 서치 후 좌표 취득.(이미지좌표, 모션좌표)
     3) Data 저장.
	 4) 나머지 3개의 좌표도 1~3번까지 시컨스 수행.
	 
	 *Data 파일명: BinThetaMet_시분초.
	 *정지했다가 다시 진행할때 시컨스 필요.
	 *리셋 후 처음부터 다시 시작 필요.
	-----------------------------------------------------
	
	*-- 시컨스 기능 구현
	1) 좌상단/우상단 최외각에 있는 칩을 가지고 0.1mm씩 Theta를 돌리면서
	   마크 서치.
	2) 두 개의 마크를 찾은 후 각도 계산.
	3) 각도를 계산한 후 상위에서 구한 파일의 4개의 제품옆에 +-0.1mm씩 구한
       각도값을 저장해야함. 
    ------------------------------------------------------

    - Data를 구한 후 회전변환 알고리즘 개발해야함.
	1) 0도 일때 좌표 4개, 1도일때 좌표 4개 총 8개의 좌표를 가지고
	   회전변환 메트릭스 계산
	2) 제품은 안착전에 10,10에 내려놔야 한다면 10.5, 9.9 이런식으로 제품을 안착하게 됨.
	\
	
15. DiePickUp / DiePlaceDown 기능 구현 완료.
  -> 맵핑된 Data 없을때 현재 가운데 있는 제품으로 픽업 진행.
  
  
  


11/25(화) 

1. Top 재현성 검증
 ***1) T 보정 기능 개선 ( 불안정 )
 -> Pitch 이동으로 X축 보상 적용 완료.
 -> 마크 서치 시에 가운데 기준 1순위로 대표 마크 찾도록 마크 서치 알고리즘 개선 필요.
 -> pitch 이동으로 Y축 보상도 적용할 수 있게 기능 개선 필요. g
 
 
 ***2) Die Scan(맵핑) 기능 개선 ( 불안정 )
 -> T 보정 안정화 후 1차 개선. 
 -> Index 순서 적용 시 기능 개선으로 2차 개선.
 -> Index Number 일정하게 적용되도록 기능 개선 필요. 
 -> 맵 매치. k
 
2. 시컨스 
 1) 인풋 웨이퍼 와 아웃풋 웨이퍼가 1:1 매칭으로 동작 되어야함. 
 2) 로딩 / 언로딩 ( 시작/정지 시 불안정 )
  -> 언로딩 시컨스 개선으로 1차 개선.
  -> 로딩/언로딩시에 알람 및 EMO를 제외한 모든 시컨스 완료 후에 
     멈추도록 시컨스 개선.
	 : 이렇게 했을때 문제 발생시에 대한 예외 로직 추가 필요.
 
3. Index Unloader - Vision Align 기능 구현
 1) Vision Align 기능 구현 및 적용 완료. 
  -> 

4. M-Align - Vision Align 기능 구현
 1) M-Align 시에 Image Search 기능 구현 필요. -> 이미지 Grab 및 마크 서치 기능 완료.
 2) Search 후 실패 시에 Probe 검사 및 Unloader Align, Unloader Pass -> 쓰레기통에서 버림. 
  -> 이와 관련된 Data 처리 필요. 
  -> OutStage에 넘어갔을떄 die 정보가 순서대로 없을때 없는 정보는 Error 처리해야함.! -?g 


5. 택업 개선 - 택타임 기능 구현
 1) 택타임 기능 구현 필요.
 2) 장비 구동하면서 개선 아이템 도출 필요. gg
  

6. OutputStage TCorrectionDialog 기능 구현 및 보상 Data 적용 완료.
  popupDlg
  1. T-보정 기능 구현 - 마크 4점 찾아서 T 회전변환 보정
   - 1. 마크 1점에 대해서 -4~ +5도를 0.1도 만큼씩 이동
   - 2. 0.1도씩 돌리면서 마크 서치 후 마크 서치한 Data를 기록. 
     -> index, 각도, 이미지 좌표, 스테이지 좌표
  2. outputStage 
   - 이미지 그랩 및 서치 기능 구현
   - 마크 찾고 스테이지 좌표로 변환 
   - 저장.
    -> indexUnloaderAlign 기능 그대로 가져와서 사용하면 될듯
   - 시컨스 만들자. 
     1. 인터락확인
	 2. 마크1번 위치로 이동
	 3. 각도 0.1도씩 Max ~ Min 이동하면서 마크 서치 후 Data 저장.
	 4. 완료 -> 파일로 저장
	 5. 마크 2번 동일 수행. -> 파일로 저장
	 6. 마크 3번 동일 수행. -> 파일로 저장
	 7. 마크 4번 동일 수행. -> 파일로 저장

7. 생산 결과를 네트워크 폴더에 저장해야함.
 1) 기존 파일 참고해서 그대로 만들어서 기능 구현 필요. g
	
************ 중요.	
7. 측정 Recipe -> 기존 설비와 동일하게 구현 요청. 동.일.하.게. XXXXXXXXXXXX

8. Barcode 기능 개선 (UseAutoTrigger)
 1) Seq 진행 시 Barcode 연동 필요. 
  -> Trigger Mode로 진행.
  -> 못찾으면 앞/뒤로 움직이면서 Scan 동작 필요. g


11/29(토)
1.
 ***2) Die Scan(맵핑) 기능 개선 ( 불안정 )
 -> T 보정 안정화 후 1차 개선. 
 -> Index 순서 적용 시 기능 개선으로 2차 개선.
 --> Map Number 일정하게 나오도록 기능 개선 필요. k

2. 컨택 아이템 기능 구현 필요. 
 -> KF`, KF``
 -> Data 1:1 단위 및 설정 등.
3. 바코드 기능 - 시컨스 적용 Test 
5. M-Align 튜닝. -> 프로브Z, 
6. Bin Data 확인 필요
7. Bottom 시컨스 검증. 
4. 샘플 Data 제공 ->
1. T축-보정 기능 구현 완료


11/28(금)
1. Index Unloader - Vision Align 기능 구현
 - Vision Align 기능 구현 및 적용 완료.  
 - Bin Stage : 배열 일정화 확보
 
2. KELFS, KELDG 측정 항목 Test 진행 중.

3. Tack-up 확보 중.
 - Tack-Up 진행 시 시컨스 수정.
 
11/29(토)
1. Index M-Align - Vision Search 기능 구현
 - Vision Search 기능 구현 및 적용 완료.  
 - M-Align 모니터링 후 Die 검사, Unloader 시컨스 구현 중.
 
2. KELFS, KELDG 측정 항목 확보.
 - 측정 Test 및 모니터링 중.

3. Tack-up 확보 중.
 - Tack-Up 진행 시 시컨스 수정.
 
4. Barcode 기능 개선
 1) Seq 진행 시 Barcode 못 찾으면 
    모션 전/후진 하면서 재측정하도록 시컨스 개선.
	

11/30(일)

1. 맵 매칭 기능 구현 및 검증.
 --> 맵파일 읽어오는 기능 구현 필요. k 
 --> 맵파일 읽어서 우리 장비 맵과 1:1 비교 k
 -> 다운로드한 Die Map과 측정한 Die Map 1:1 비교?
 --> Bin1번만 픽업해야함. ( Bin 구분해서 픽업하는 기능 필요 )
 ---> Bin 별로 색깔 구분. ( OK, NG, 없는 경우 등) 
 
2. 언어변경 기능구현



3. GUI - 모니터링 기능 개선
 -> 생산량 표기. (토탈, 오케이, 엔쥐, 백분율, UPH, Bin Number )
 -> 생산 진행 현황 
   - 현재꺼를 작게해서.. Probe 측정 Data를 모니터링? ( 따로 Dlg 처리? )
   - Die 맵정보, 생산량, 카세트, Vision, 측정Data, 
   
 
4. GUI - Manual 기능 개선
 -> Manual에서 시컨스 단위 동작 되도록 기능 구현. 
   (미비항목 전부 추가 필요)
   

5. 생산 결과를 네트워크 폴더에 저장해야함.
 1) 기존 파일 참고해서 그대로 만들어서 기능 구현 필요.
    기존파일이..

	
************ 중요.	
6. 측정 Recipe -> 기존 설비와 동일하게 구현 요청. 동.일.하.게.

1. EQP 화면
 1) 항목
 
 - QMC장비명 : LPH-280
 - 장비명 : 서울바이오에서 지정 장비명
 - 결과Log폴더경로: bin, prd, sum, txt, waf
 - 생산로그폴더경로: svn
   
   
   
11/30(일) 
1. 생산결과및로그 경로 설정 기능 구현 완료
2. 시컨스 - 버그 수정 ( 검사 정지시 재시작 기능 개선 )
3. Vision View 기능 개선 
4. 샘플 제작 ( 1 Index 검사 )
5. 언어변경 기능구현중 ( 50% )
6. Map Match 기능 구현 및 Test 중 (50%)

12/1(월)
1. Map Match 기능 구현 및 Test 중
 1) Map Match 사용 유/무 기능 구현 완료.
 2) Map Match 경로 설정 기능 구현 완료.
 3) Map Match 후 시컨스에서 진행 유/무 선택 기능 구현 완료.
 4) Map Match Test 자재 수령 완료.
  : 장비에서 실제 Test 진행 예정. (Waftt Test 후)
  
2. 카세트 1:1 매칭시에 데이터 동시에 읽는 경우 문제 발생 
 : 수정 완료.

3. Vf3, VF1, WD Spec In.
   watt Spec 3.0% -> 6.0%나와서 Fail.
  : Spectrometer  - Integration Time(ms) : 7 -> 50 변경 
    Test 진행 시 Watt Data는 괜찮지만 Intensity Data가 좋지않아
   -> 10 으로 변경 Test. 

4. 생산 시 사용하는 실 자재 바코드 Test.
  : Test했던 바코드와 다른 문제인지 바코드 인식 안됨.
  : 셋팅 및 테스트 예정. 
  
5. 초기화 유/무 확인 후 장비 구동하도록 기능 수정.
6. 결과Data - Recipe 단위를 타 장비와 통일화 작업 예정.
7. Recipe Item - min/max값 업로드 작업 완료.
8. 파일에서 Bin파일명과 waferID 위치 변경 작업 완료.

12/2(화)
1. Vf3, VF1, WD Spec In.
   watt Spec 3.0% -> 6.0%나와서 Fail.
   
  Test자재 350개 수량 Test - 1 Index ( Socket 1번 )
  : Spectrometer  - Integration Time(ms) : 7 -> 50 변경 
    Test 진행 시 Watt Data는 NG. 
    Spectrometer  - Integration Time(ms) : 10 변경 
    Test 진행 시 Watt Data는 NG.
   
    Spectrometer  - Filter: 1(100%) -> 2(10%) 변경.
	Integration Time(ms) : 40 변경 ( ADC 값: 25000) 
    Test 진행 시 Watt Data NG.
	
	Spectrometer  - Filter: 1(100%) -> 2(10%) 변경.
	Integration Time(ms) : 30 변경 ( ADC 값: 20000) 
    Test 진행 시 Watt Data NG.
	
	Spectrometer  - Filter: 1(100%) -> 2(10%) 변경.
	Integration Time(ms) : 50 변경 ( ADC 값: 30000) 
    Test 진행 시 Watt Data NG.
	
	Spectrometer  - Filter: 1(100%) 설정.
	Integration Time(ms) : 5 변경, 2번 측정 
    Test 진행 시 Watt Data NG.

	Spectrometer  - Filter: 1(100%) 설정.
	Integration Time(ms) : 7 변경, 2번 측정 
    Test 진행 시 ADC 값 50,000 이상으로 Test X.
	

2. 결과Data - Recipe 단위를 타 장비와 통일화 작업 예정.
  : Recipe 단위 - 기존 장비와 통일화 작업.
  : TEST 진행 중.
  
3. Map Match 기능 구현 및 Test 중
 1) Map Match 사용 유/무 기능 구현 완료.
 2) Map Match 경로 설정 기능 구현 완료.
 3) Map Match 후 시컨스에서 진행 유/무 선택 기능 구현 완료.
 4) Map Match Test 자재 수령 완료.
  : 설비에서 실제 매칭 Test 완료.

  
 12/03(수)
1. Vf3, VF1, WD Spec In.
   watt Spec 3.0% -> 6.0%나와서 Fail.
   
   : Test 진행 중.
   : 측정 파라미터 변경하면서 1Index 1Chip 반복성 Test 진행.
   : 현재 조건: IntegrationTime(ms): 5, 4번 반복 할 경우
     반복성 데이터 가장 좋음. 
   : watt Spec In. Cul 완료.
   
2. Recipe - 타 장비와 단위 통일화 작업 완료.   
3. 결과Data - Recipe 단위를 타 장비와 통일화 작업 완료.
4. Network 경로 지정 기능 작업 완료. 


5. 생산결과및로그 파일로 작성 후 네트워크 폴더에 업로드 기능 구현 작업 예정.
 1) 202510WaferTotalSummaryData.csv
 2) 202510WaferTotalOUT_DB.csv
 -> 두 개 파일 형석 맞춰서 Data 업로드 필요. 
 

6. 생산 시작 시 Seq. 
 1. AOI 프로빙을 통해 표면 NG 칩을 제거할 맵을 생성
  -> NG칩을 제거?? 
 2. 프로빙으로 특성 측정.
 3. OK 칩 선별 1과 2 단계 유지. 
 


12/04(목)
1. Master Sample 1:1 Calibration 작업 중. 
 1) 1차 Watt NG.
 2) 2차 VF1 NG. 
    -> VF1: 1mV -> 1uV로 수정.
 3) Data Spec In 완료.	
 4) 1차 Offset 적용 후 Run Test. ( 
 5) Final Offset 적용 후 Run Test.
 6) WATT NG.
 
 
2. 바코드 리더기 Input/Output Setting 완료 ( Code39 사용 )

3. Recipe 파일 -> *.ITF파일(기존장비 사용파일)과 1:1 작업은 장비에 맞는 파라미터 기준 선정 후 
   *.ITF 파일과 맞춰서 동작되도록 작업 하는 것으로 공정엔지니어와 얘기함. 
   현재 *.ITF 파일에 대하여 1:1 작업은 불가. 
   
4. 바코드 시컨스 진행 시 NG 발생 및 예외 처리 시컨스 수정 예정.

5. 측정 시 Min max 값을 보고 장비 NG Alarm 발생.
  * NG Alarm 발생 여부 선택 할 수 있도록 UI 및 기능 구현. (이미지 참고)
    (Contact - Miss Setting)
	
6. 기존 장비 DB 존재 ( DB 정보 파악 필요 )

7. 생산 및 양산 or Test 시 Unloader에서 180도 회전 안하도록 기능 수정 ( 옵션처리 필요 )
   1:1로 Loading -> Unloading... 


12/05(금)
1. Master Sample 1:1 Calibration
  - WATT NG 대응을 위한 Test 진행 중.
  
12/06(토)
1. Master Sample 1:1 Calibration
  : 2차 측정 -> WATT NG. 
   -> WATT NG 대응을 위한 Test 진행 중.
 
12/07(일)
1. Master Sample 1:1 Calibration
  : 2차 측정 -> WATT NG. 
   -> WATT NG 대응을 위한 Test 진행 중.  

12/08(월)
1. Master Sample 1:1 Calibration
  : 2차 측정 -> WATT NG. 
   -> WATT NG 대응을 위한 Test 진행 중.  
   
2. ADC (MAX Count) Display 기능 구현 완료. 
3. wafer, Bin - Index 순서에 따른 구동 방향 설정 기능 구현 완료.
4. 공정Test - Manual 컨택 기능 구현 완료.
5. Recipe - Offset 공통 등록 기능 구현 완료.

12/09(화)
1. Master Sample 1:1 Calibration
  : 2차 측정 -> WATT NG. 
   -> WATT NG 대응을 위한 Test 진행 중.
   -> Script 수정 후 대응. - 결과 X.
   -> Delay 적용 후 대응. -> 1, 2차 결과 OK 이지만... 공정엔지니어 Fail. 
   -> 기존에 했던 방식대로 해서 Data 나오지 않음.
   -> (7, 2, 1)
   
   -> MaxCount 15000 맞춰서 재 진행. ( 25, 2, 2 ) - X.
   
   -> WATT 반복성 0.2% 이하 조건으로 Test 진행 중. 
      [ 적분구 높이 0.7mm Up, MaxCount(ADC) 33,000 조건으로 
	    5, 5, 1 -> 조건으로 반복성 Test 시 0.15% ]
   -> 재현성 Test 진행.
   

2. Script 수정 완료. (측정 방식 변경 등 )

* Z축 서보 OFF 하고 측정. WATT 어떻게 되는지? 확인? 진동? 
* 컨택 하지말고 측정. WATT 어떻게 되는지? 확인? 잡광일 수도 있음. ---.

3. 1번 Data를 가지고 8번 Index에 Data 입력하는 기능 구현
 1) 칩 로딩 -> 1번 소켓 -> 2~8번 반복 이동 하면서 Data 측정.
 2) 8번 소켓에서 완료 후 칩 언로딩 후 Wafer에 다시 안착. 
 
 - 5번 반복 수행. 
 
 --> 다이얼로그 구현 필요. ( Offset 복사/붙여넣기 되도록 )
 -> Index Cal 기능 구현 완료. 
 
12/16(화)
 1. Top - Compare with Ref machine 진행 중. 
  -> WATT 기준. 타 장비 측정 값 대비 2~4mW 낮게 측정되는 문제 발생 중.
   : 현재 - 원인 불분명. 

  -> WATT Data 256~260mW 기준인 Master Sample Data와 Cal 작업을 통하여 
     Gain, Offset 적용 및 검증 완료. ( Cal Test OK )
  -> But, 타 설비 WATT 270mW Die 소팅한 wafer를 검사 시 현재 자사 장비는 
     WATT 268 ~ 269mW로 측정되고 있음. 
  -> Cal 작업을 통하여 Data 통일화(Gain, Offset값 적용)를 완료 하였는데, 타 설비에서 측정한 
     Wafer - Die의 WATT 값 차이점에 대하여 원인 불분명 중...
  
  -> ADC 값 30000 -> 15000 ( 타 설비 기준 ) 으로 설비 셋팅하여 Test 결과 : 변화 X.
    : 적분구 높이 ( 현재 타 설비가 우리보다 낮게 셋팅되어 있음. [ 자사 설비 Pin Type 측정 설비로 
	              타 설비와 동일한 높이 셋팅 불가 )
	: Integration Time(ms) : 5, Filter : 1 - 변화 X.  ( ADC 약 30,000 )
	: Integration Time(ms) : 50, Filter : 2 - 변화 X. ( ADC 약 30,000 )
	: Integration Time(ms) : 23, Filter : 2 - 변화 X.  ( ADC 약 15,000 )
	: Setting에 대한 부분 변경 Test 시 자사 설비 반복성, 재현성 OK.
	: Cal Test OK. 
	: 타 설비와의 WATT 차이 문제 발생. NG.
	
  -> Timming 증가 후 Ok.
  -> VR -> VF -> WATT 측정 시 OK. 
  -> Script 수정 후 Cal Test 진행. 
  -> 
	
	
	
	
	
  -> Script 변경 Test : 변화 X.
    : Scrpt 수정 후 VF3 Data 0.01~0.04 오차 발생으로 Script 원복 ( 12/16 )	
	
  -> WATT 측정 시 Timming 10 -> 50ms 증가 - OK.
  -> VF3 전에 VR 측정 적용 시 - OK.  
  -> CAS 측정 시에 Timming 문제 추정. 
  
  
  마스터 
  
  
12/22(월)
1. 맵매칭시에 맵매칭하고서 1:1 유/무 확인하고 
  맵매칭이 맞지 않으면 메뉴얼로 맞출수있는 기능 구현.
  -> 50% 기능 구현

*** 이거 -.  
2. 택타임 확인용 기능 구현.
  -> Bin Wafer에 die 내려놓고 다음 die 내려 놓는 시간까지 측정.
  -> 1차 UI까지 기능 구현 완료. 
  -> 타임리스트 기반으로 택타임 측정 GUI 기능 구현 필요.
  
3. 바코드 시컨스 
  -> inputFeeder 기능 구현 완료. - 2차 Test 필요.
  --> OutputFeeder 기능 구현 필요.


11/29, 11/30, 12/6, 12/7, 12/20 이렇게 출근.

12/24(수)
1. Bottom - Gripper 추가.

12/25(목)
1. 모터 알람 발생시에 알람 추가 할 것!
 1) 서보 Off, Limit 꼭 추가 할 것.!
 2) Probe Z축 - Safety Pos 이동 두개로 분할해서 구동하도록 수정 완료.
 
 3. Buttom - Gripper 추가 완료.
    -> 구동 확인 OK.
	
 Dino2.0 프로그램 다운로드 할것.
 Dino프로그램2.0 program.  -> 받았음
 
 
12/26(금) 
1. Bottum - 반복성 / 재현성 Test 
  1) Gripper 무 : 반복 측정 중 (진기적 틀성 검증)
  2) Gripper 유 : Gripper 기구 이슈로 X.
                 -> 적분구 높이 기구 수정 후 가능 여부 재 확인. 
				 -> 현재 사용 안함 모드로 구동 중.

2. Gripper 사용 유/무 기능 구현.
 -> 기능 구현 완료
 
12/27(토)
1. Config Unit Name 정리 완료
2. working -> manual 변경 및 Name 정리 완료
3. 축별 알람 기능 추가 완료( 서보 Off, 알람, 리밋 )

12/28(일)
1. 빈스테이지 die 내려놓는 위치 꼬임 버그 현상 - 수정 완료. 
2. 4848 Model -> 재현성 Test 진행 중. 
3. CAS 적분구 -> UV Cal 진행 필요.!!! 
  -> 서울반도체 대여 X, 킴스 대여 X.

4. LoadArm 진행 시 LoadArm에 Die 가지고 있는 경우. 
-> Index Load 되도록 시컨스 수정 필요.
-> 수정 완료함.

12/29(월)
1. 바코드 - 인풋스테이지기능 -> 아웃풋스테이지에도 똑같이 적용.
 -> 적용 완료. 장비 Test 필요.
 -> 장비 Test 완료.
2. D4848 Model(Bottom) 재현성 Test. 
 -> 재현성 VF - X
 -> Probe Up/Down 기능 구현. ( Up/Down 반복Test ) 
 
12/30(화)
1. Manual 티칭 이동 기능 수정.
 -> Config 티칭 이동 기능을 Dlg -> Unit으로 함수 구현 필요. 
 -> Manual 에서도 같은 함수 사용하여 이동 기능 구현 필요.
 
2. 맵매칭 - 메뉴얼 기능 수정 및 검증 필요.
 -> 1차 구현 완료. 
 -> 실제 맵매칭가능 제품 수령 후 Test 필요.
 
3. Config Teaching Pos Data를 Recipe로 관리 필요. 
 -> *Top / Bottom에 따라 분리 필요함. 
 -> Recipe 별로 가져가는게 가장 안전하고 확장성이 좋음.
 
 1/2(금)
1. Manual 티칭 이동 기능 수정.
 -> Config 티칭 이동 기능을 Dlg -> Unit으로 함수 구현 필요. 
 -> Manual 에서도 같은 함수 사용하여 이동 기능 구현 필요.
 -> 기능 구현 완료.
 
2. 맵매칭 - 메뉴얼 기능 수정 및 검증 필요.
 -> 1차 구현 완료. 
 -> 실제 맵매칭가능 제품 수령 후 Test 필요.
 
3. Config Teaching Pos Data를 Recipe로 관리 필요. 
 -> *Top / Bottom에 따라 분리 필요함. 
 -> Recipe 별로 가져가는게 가장 안전하고 확장성이 좋음.
 -> probe 구현 완료. 
 -> M-Align 구현 완료.
 -> Input/Output Arm 구현 중.
 -> 기능 구현 완료.
 
 1/5(월)
 1. MES 작업 진행 중 ( 10% )
 2. Unit별 Config Teaching Data를 Recipe로 분할.
   -> Recipe ( Probe, M-Align, InputTr, OutputTr )
 3.   
   
 
 1/6(화)
 1. Wafer, Bin Data - MapX, MapY 1:1 매칭 코드 수정.
 2. MES 작업 진행 중 
 
 3. M-Align : Stage 간섭관련 기구 교체 작업 완료
 4. Bin Stage: 제품 사이즈 대응관련 기구 교체 작업 완료는
 5. Niddle Block : 제품 사이즈 대응관련 기구 교체 작업 NG
   -> Blick 기구 수정 필요.
 
 * 1/7(수) : 확인 할 것.!!
 6. GEM - x64Bit 설치 파일 확인. ( 현재 x86 임 )
 7. 
 
 
 
*지출결의서 작성 및 제출 필요. 
 1. 법인 지출결의서 -> 호텔, 비행기
 2. 개인 지출결의서 -> 인경비, 출/입국 교통비, 로밍(통신비)
 
 
  1/6(화) LCP-280
  LCP-280 해결 과제 ( 남은 과제 )
  
  1. Bottom VF Data 재현성 확보 필요. 
   -> 케이블 교체 Test 진행 예정 ( 본사 -> 베트남 )
     - 기존 I/O Cable -> BNC Cable 변경 Test.

  2. Bottom 광학 관련 (WP/WD, WATT) Data 확보 필요.
   -> CAS Set 교체 예정.
     - 현재 CAS 구매 진행 중. ( 납기 확인 필요 )

  3. Bottom 고객사 컨펌 필요.
   1) 재현성 검증
   2) Master Sample Calibration Data 검증
   3) 시양산 검증
   4) 양산 검증

  4. Top 고객사 컨펌 필요. 
   1) Master Sample Calibration Data 검증 ( 재검증 )
   2) 시양산 검증
   3) 양산 검증

  - 내부 작업 
  1. [기구] Top 공정관련 -> Stage Area 확보 필요. ( 100mm -> 140mm 샘플 대응 ) 
   -> (Input, Output) 기구 변경 작업 예정 및 Test 필요. 
  
  2. [제어] 양산 시컨스관련 기능 및 검증
   1) MES 기능 구현 
   2) Map Maching 기능 Test ( Manual 기능 구현 )
   3) Result Data - Upload 기능 Test 
   4) 실제 양산 진행 시 필요한 로직 구현 Test
   5) (보완) 맵 서치 이 후 Die 정보 Index 및 위치 보완 진행 시 
            속도가 엄청 느린 문제 발생. ( 1000 개 이상되면 느려짐 현상 ) 
      -> 알고리즘 개선 필요.	  
   
   
   
   yngoo@iqmc.co.kr
   ekdus1002!
  
D:\\LCP-280\\GEM\\



1/12(월) LCP-280
  LCP-280 해결 과제 ( 남은 과제 )
  
  1. Bottom VF Data 재현성 확보 필요. 
   -> 케이블 교체 Test 진행 예정 ( 본사 -> 베트남 )
     - 기존 I/O Cable -> BNC Cable 변경 Test.
	 
   -> Gripper 사용 + BNC Cable 사용으로 재현성 확보 중.
   -> Gripper 사용 : OD 50이상 필요.

  2. Bottom 광학 관련 (WP/WD, WATT) Data 확보 필요.
   -> CAS Set 교체 예정.
     - 현재 CAS 구매 진행 중. ( 납기 확인 필요 )

  3. Bottom 고객사 컨펌 필요.
   1) 재현성 검증 -> 자체 Data 확보.
   2) Master Sample Calibration Data 검증 
   3) 시양산 검증
   4) 양산 검증


  4. Top 고객사 컨펌 필요. 
   1) Master Sample Calibration Data 검증 ( 재검증 )
   2) 시양산 검증.
   3) 양산 검증.
   

  - 내부 작업 
  1. [기구] Top 공정관련 -> Stage Area 확보 필요. ( 100mm -> 140mm 샘플 대응 ) 
   -> (Input, Output) 기구 변경 작업 예정 및 Test 필요. 
   -> 기구 작업 1차 완료. 
   -> Niddle 브라켓 -> 재 작업 필요. 
   -> Niddle 브라켓 납기 일정 확인 필요.
   
  2. [제어] 양산 시컨스관련 기능 및 검증
   1) MES 기능 구현. (50%) ->.||   
   2) Map Maching 기능 Test ( Manual 기능 구현 ).
   3) Result Data - Upload 기능 Test.
   4) 실제 양산 진행 시 필요한 로직 구현 Test.
      -> 공정 시 장비 상태 Data Update. (DATE	MachineName	WAFERID	BINID	START	END	Total Time	Run Time	Down Time	Scan Time	Ld Time	ULd Time	SortTime	AlarmCnt	Total Count	Scan Count	Out Count	Miss Count	Scan NG	OutSide	WaferVision	AlignVision	IndexVision	Contact	Ld Pick	Ld Place	ULd Pick	ULd Place	C/T	Total NG	Contact Retry	 Yield	 UPH	 UPD	Picker1	Picker2	Picker3	Picker4	Picker5	Picker6	Picker7	Picker8)
   
   5) (보완) 맵 서치 이 후 Die 정보 Index 및 위치 보완 진행 시 
            속도가 엄청 느린 문제 발생. ( 1000 개 이상되면 느려짐 현상 ) 
      -> 알고리즘 개선 필요.
	  



	  
	  


	  MovePositionSafetyProbeZ
	  

	  담당자 얘기 하고...
	  1/14(수) 한국 ~ 1/21(수) 베트남  ... 
 
 
 
var result = await equipment.SequenceStopAllAsync(CancellationToken.None);

bool ok = await eq.SequenceStopAsync(unitName, CancellationToken.None);
 
try
{
    var ctx = Equipment.Instance.SummaryContext;
    ctx.GetCurrentSummaryOrNull()?.StartUnload();
}
catch (Exception ex)
{ Log.Write(ex); }


 try
 {
     var ctx = Equipment.Instance.SummaryContext;
     ctx.GetCurrentSummaryOrNull()?.StopUnload();
 }
 catch (Exception ex)
 { Log.Write(ex); }
 
 
 Miss Count	Scan NG	OutSide	WaferVision	AlignVision	IndexVision	Contact	Ld Pick	Ld Place	ULd Pick	ULd Place


1/14(수) LCP-280
  LCP-280 해결 과제 ( 남은 과제 )
  
  1. Bottom 광학 관련 (WP/WD, WATT) Data 확보 필요.
   -> CAS Set 교체 예정.
     - 현재 CAS 구매 진행 중. ( 납기 확인 필요 )
	 
  2. Bottom 고객사 컨펌 및 공정 Data 확보.
   1) 재현성 검증 -> 자체 Data 확보. -> 고객사 컨펌 OK.
   2) Master Sample Calibration Data 검증 중. 
   3) 시양산 검증
   4) 양산 검증

  3. Top 고객사 컨펌 필요. 
   1) Master Sample Calibration Data 검증 ( 재검증 )
   2) 시양산 검증.
   3) 양산 검증.
   

  - 내부 작업 
  1. [기구] Top 공정관련 -> Stage Area 확보 필요. ( 100mm -> 140mm 샘플 대응 ) 
   -> (Input, Output) 기구 변경 작업 예정 및 Test 필요. 
   -> 기구 작업 1차 완료. 
   -> Niddle 브라켓 -> 재 작업 필요. 
   -> Niddle 브라켓 납기 일정 확인 필요.
   
  2. ProbeCardZ축 150mm/sec 이상 구동시 과부하 발생.
   
  3. [제어] 양산 시컨스관련 기능 및 검증
   1) MES 기능 구현. (50%) ->.||   
   2) Map Maching 기능 Test ( Manual 기능 구현 ).
   3) Result Data - Upload 기능 Test.
   4) 실제 양산 진행 시 필요한 로직 구현 Test.
     - S/W 작업 진행.
   5) (보완) 맵 서치 이 후 Die 정보 Index 및 위치 보완 진행 시 
            속도가 엄청 느린 문제 발생. ( 1000 개 이상되면 느려짐 현상 ) 
      -> 알고리즘 개선 필요. -> Test 진행 중.
  
 
 
 Log.Write(UnitName, "[RotateToolTForPlace_AsyncWait] AddWaferVisionAsMiss");
try
{
    var ctx = Equipment.Instance.SummaryContext;
    ctx.GetCurrentSummaryOrNull()?.AddWaferVisionAsMiss();
}
catch (Exception ex)
{ Log.Write(ex); }
  
 

 1/16(금) LCP-280
  LCP-280 해결 과제 ( 남은 과제 )
 
 1. Align -> 무조건 Center Mark 찾는 함수 만들어서 적용.!
 2. Align -> X,Y,T 찾은 마크 위치로 이동 함수.!
 3. Align -> X방향 / Y방향 선택 기능 넣을 것.!
 4. Recipe -> Vision Recipe 기능 재검토!
 
 1/21(수) LCP-280

  1. Bottom 광학 관련 (WP/WD, WATT) Data 확보 필요.
   -> CAS Set 교체 예정.
     - 현재 CAS 구매 진행 중. ( 납기 확인 필요 )
	 
  2. Bottom 고객사 컨펌 및 공정 Data 확보.
   1) 재현성 검증 -> 자체 Data 확보. -> 고객사 컨펌 OK.
   2) Master Sample Calibration Data 검증. -> 고객사 컨펌 OK.
   3) Bin level 정합성 -> Data 검증 중. -> 1/21(수) D4848 Model 1차 OK?
   3) 시양산 검증
   4) 양산 검증
   
   -> 6964개 Pitch X 제품 물류 생산 Test 진행. Bin Table Pitch 0.9mm 진행. 
   -> Test 결과 : 물류 이상 무. ( 컨택하여 Data 측정은 안함. )
   -> pickUp Error : 201개
   -> Unload PickUp Error : 4개
. 
  3. Top 고객사 컨펌 필요. 
   1) Master Sample Calibration Data 검증 ( 재검증 )
   2) 시양산 검증.
   3) 양산 검증.
   

  - 내부 작업 
  1. [기구] Top 공정관련 -> Stage Area 확보 필요. ( 100mm -> 140mm 샘플 대응 ) 
   -> (Input, Output) 기구 변경 작업 예정 및 Test 필요. 
   -> 기구 작업 1차 완료. 
   -> Niddle 브라켓 -> 재 작업 필요. 
   -> Niddle 브라켓 납기 일정 확인 필요.
   -> 변경 및 티칭 완료.
   
  2. ProbeCardZ축 150mm/sec 이상 구동시 과부하 발생.
  -> 변경 및 티칭 완료.
  -> 이동 거리에 따라 Max Speed가 다름.
  -> 5mm 이동 시 Max Speed 1000 mm/sec
  -> 10mm 이동 시 Max Speed 600 mm/sec
  -> 15mm 이동 시 Max Speed 300 mm/sec
   
  3. BinStage 교체 작업 완료.
   
  4. [제어] 양산 시컨스관련 기능 및 검증
   1) MES 기능 구현. (50%) ->.||   
   2) Map Maching 기능 Test ( Manual 기능 구현 ).
   3) Result Data - Upload 기능 Test.
     - 업데이트 완료. 
	 *신규 문제점.
	 - 네트워크 폴더에 바로 Data를 작성시 부하로 인하여 프로그램 느려짐 발생.
	 - 파일에 Data 입력 시 딜레이 및 시간 차에 따라 Error 발생. 
	 -> 개선 필요.!
	 -> 1차 개선 시 알람 발생 및 멈추는 건 X.
	 -> 파일 업로드 시 지연 발생. -> 2차 개선 필요.!!
	 
   4) 실제 양산 진행 시 필요한 로직 구현 Test.
     - S/W 작업 진행.
	  (1. Align -> 무조건 Center Mark 찾는 함수 만들어서 적용.! -> OK
	  (2. Align -> X,Y,T 찾은 마크 위치로 이동 함수.! -> OK
	  (3. Align -> X방향 / Y방향 선택 기능 넣을 것.! -> OK
	  (4. Recipe -> Vision Recipe 기능 재검토! -> OK
 
   5) (보완) 맵 서치 이 후 Die 정보 Index 및 위치 보완 진행 시 
            속도가 엄청 느린 문제 발생. ( 1000 개 이상되면 느려짐 현상 ) 
      -> 알고리즘 개선 필요. 
	  -> Test 진행 중. 95% OK. 


 1/22(목) LCP-280
 
 1. Bottom 공정 조건 확보 
  1) ProbeCardZ축 H/W 작업으로 재 검증.
  
 2. Map Maching 기능 Test ( Manual 기능 구현 ).



제품 측정 조건
1. 로딩 <-> 언로딩 피치 동일.
2. 측정 중 알람 발생. ( 알람 카운터 증가 )
3. 1인덱스만 측정. offset 0으로.
4. Data와 알람 파일 전달.


 1/23(금) LCP-280
 
 1. 맵서치 기능 재 검토 중.
  * pitch 0.55mm 인 경우 맵 서치시 die 중복 현상 발생.
  
 2. 추가 기능 요청. 
  -> 제품에서 외곽부를 설정하여 검사 안하는 기능 요청.
  -> wafer 내부 외곽의 die는 전부 NG 상태.
  
 1/24(토) LCP-280.
 
 1. 맵서치 기능 재 검토 중.
  -> 기능 업데이트 Test 완료. 
  
var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
if (recipe != null)
{
    DieSkipLine = recipe.DieSkipLine;
}
  
 
  1/25(일) LCP-280.
  1. 네트워크 폴더에 Data 업데이트 기능 구현.
   -> 너무느려서 메인프로그램에 영향 발생. 
   -> 백그라운드 파일 업데이트로 기능 수정. 
   
  2. 맵매치Test. 
   -> Menual에서도 맵매치 기능으로 Test 가능하도록 기능 수정 필요.
   

 1/27(화) LCP-280.
  1. 네트워크 폴더에 Data 업데이트 기능 구현.
   -> 너무느려서 메인프로그램에 영향 발생. 
   -> 백그라운드 파일 업데이트로 기능 수정.
   -> 2차 기능 업데이트 중.   
   
  2. 맵매치Test. 
   -> Menual에서도 맵매치 기능으로 Test 가능하도록 기능 수정 필요.
   -> 기능 구현 및 Test 완료.
   
  3. Recipe 기능 보완
   1) Recipe 복사 / 신규생성 시 Vision Data도 같이 되도록 기능 수정.
   2) Delete 기능 Recipe 폴더 삭제 안되는 버그 수정.
    -> 기능 구현 및 Test 완료.
	
  4. Index Cal 완료 후 공정Recipe Offset - 버튼으로 전체 적용 하도록 기능 수정필요 
    -> 기능 구현 및 Test 완료. 
	
 1/29(목) LCP-280

 1. 네트워크 폴더에 Data 업데이트 기능 구현.
   -> 너무느려서 메인프로그램에 영향 발생. 
   -> 백그라운드 파일 업데이트로 기능 수정.
   -> 2차 기능 업데이트 중.
   
 1/30(금) LCP-280

 1. 네트워크 폴더에 Data 업데이트 기능 구현.
   -> 너무느려서 메인프로그램에 영향 발생. 
   -> 백그라운드 파일 업데이트로 기능 수정.
   -> 2차 기능 업데이트 중.
   -> 백그라운드에서 파일 write하도록 전체 수정. 
   -> 백그라운드에서 큐에 데이터담아서 네트워크 폴더(공유폴더)에 전송 하도록 수정. 
   
 2. 맵매칭은 맵좌표까지 생성한 Data Load해서 자동 보정되는지 기능 검증 할 것.
 
 3. MES 기본 구조 잡아 놓을것.
 
 4. 메뉴얼 기능 업데이트 할 것. ( 모션 이동 및 Data 이동 기능 등 )
 
 
 2/3(화) LCP-280
 
 1. 맵매칭 맴좌표 생성한 Data Load 후 자동 보정 기능 검증.
 2. 마스터 샘플 - 캘리브레이션 진행.
 
 
  ============== 일정 협의 ==================
 1. 캘리브레이션 
 - 진행 중   - 2/3(화)
 - 캘리브레이션 검증 완료. 
  -> Index Cal 완료. 
  *** Index 8개 전부 사용하여 공정 진행 중 probe Card Pin 부러짐.
   -> 상황: 
       (1) Index Cal 진행시 8개 측정 확인까지 완료 후 Run 진행.
	   (2) 장비 구동 및 검증 확인 중 NG 다발하여 확인 결과 Pin 부러짐 확인.
   -> 원인 추정
       (1) 장비 구동 중 Pin 흔들림으로 인하여 부러지는 것으로 판단됨.
	   (2) pin의 흔들림이 Index8개에 존재하는 Hole center에 위치해야 하지만
          현재 Setting시 어려움 발생함.
           Hole 벽을 타고 Pin이 상승되도록 되어있지만 벽을 타게 되면서 부러지는 것으로 판단됨.
      		   
			   
2. data QMC vs data ADP  ★★★
 - Bin Rank 재현성 - 2/3(화) ~ 2/9(일)
 -> Bin Rank 재현성 조건은 현재 서울반도체장비와 1:1 매칭 조건.
 
3. 마스크 맵 - 차 주 제공 예정(2/10(월)~) 
 -> 차주 제품 제공 사유: 가장자리 양품 칩 색상 변경하여 샘플 제공
 - 가장자리 양품 칩 색상 변경 ( 마크 재등록 후 확인 필요 )
 - 설비에서의 스캔 정밀도 확인 ( 현재까지 이상 없음 )
 - 차주 제공 테스트 필요

4. 니들 데미지 검증
 - 셋팅 / 검증 완료 (모니터링 중)

5. 프로빙 3~5 웨이퍼 검증 - 차 주 제공 예정(2/10(월)~)
 -> 차주 제품 제공 사유: 가장자리 양품 칩 색상 변경하여 샘플 제공

6. 언로드 데이타 YSM(?) 체크 - 차 주 제공 예정(2/10(월)~)
 -> 차주 제품 제공 사유: 가장자리 양품 칩 색상 변경하여 샘플 제공
 - 확인 필요. 
 -> 자체적으로 기능 Test 진행 예정. -> 완료.

 
*프로그램 수정 필요 및 버그 	
1. 스캔 완료 후 첫번째 칩 로드 진행시 실패 발생 현상 
	 -> 상황: 맵스캔 완료 후 첫번째 칩 로딩 실패 현상 발생.
			 -> 맵스캔 완료 후 첫번째 칩 위치로 스테이지 이동 후 재 시작시 문제 없음 확인.
	 -> 조치예정: 맵스캔 완료 후 첫번째 위치로 1차 모션 이동 이 후 다시 한 번 이동 후 진행하도록
				시컨스 수정.
	 -> 2/3(화) 프로그램 적용 완료. 1차 Test OK. ( 모니터링 중 )		

2. [제어] 양산 시컨스관련 기능 및 검증 
	1) Map Maching 기능 Test. ( AutoRun/Manual 기능 구현 완료 )
	 -> OP ID 입력후 진행 기능 추가. 
	 -> Mask Map Maching 기능 수정.  
	 -> 2/4 Test 완료. 
	   : 수정: 맵파일 포맷 변경 대응, 좌표계 변경 대응, 외곽부 몇 라인 제거 후 맵매칭 기능 대응.
	 -> 2/5 고객사와 검증 예정.
	 
	2) 네트워크 폴더에 Data 업로드 기능 업데이트 필요.
	 -> 상황: 현재 네트워크 폴더에 Data를 바로 업로드하여 프로그램 지연 현상 발생. 
	 -> 조치예정: 네트워크 폴더에 업로드하는 Data를 백그라운드에서 상시 업데이트하는 방법으로 
				프로그램 수정 예정.
	 -> 2/3(화) 프로그램 적용 완료. 1차 Test OK. ( 모니터링 중 )
	 -> 2/4(수) 모니터링 중. ( Data 5000개 이상 파일 Test 진행 )
	 
	4) 실제 양산 진행 시 필요한 로직 구현 Test.
	  1. Align -> 무조건 Center Mark 찾는 함수 적용. -> 완료
	  2. Align -> X,Y,T 찾은 마크 위치로 이동 함수. -> 완료
	  3. Align -> X방향 / Y방향 선택 기능, Align Setting 기능 구현. -> 완료
	  4. Recipe -> Vision Recipe 기능 재검증 -> 완료
	  
	5) Recipe 파라미터 변경 시 콤보박스 마우스 스크롤로 변경되면서 원하지 않는 파라미터 설정됨.
	  -> 마우스 스크롤 기능 잠금 기능 추가 구현. -> 완료. (2/3)
	
	6) Result Data 폴더명 날짜 업데이트 안되는 현상 -> 수정 완료.
	
	7) MES 기능 구현. (50%): 상위 <-> 32bit MES 프로그램 <-> Main 프로그램 구현 완료.
	 -> 시나리오 제공 및 MES 방법 문의 시 뚜렷한 답변을 못받음.
	 -> 타장비 설비 세팅 및 로그 파일 분석 후 기능 구현 예정. 
	 
	8) 맵서치 이 후 외곽부 라인 제거 기능 관련하여 추가 필요 예상.
	 -> 원형, 반원, 사각형 등의 모형 대응 필요할 것으로 판단.
	 -> 현재: 최외각 몇라인 제거 후 진행. ( 원형 기준 )
	

*남은 과제 (필요 기능)
1. Recipe 공정 Data Offst 입력 기능 수정 요청.
 -> 엑셀파일에서 붙여넣기/복사 하면 적용되도록 기능 수정 요청.
2. Manual 동작 기능 추가 구현.
 -> wafer 로드/언로드 manual 기능 구현.
 
 
 2/5(목) LCP-280
 1. 맵매치 UserID 장비에 설정하여 사용.
  -> 따로 Log에 남겨놓도록 기능 구현 완료. ( 업체에서 미 지정. 문의 하였지만 모름 )
  -> UserId 입력란 기능 업데이트 완료.
  -> Map File Load 실패 시 Manual로 File 선택 기능 업데이트 완료.
  
  *Wafer 맵매치시 wafer Map은 공용으로 하나 있음. wafer Map과 1:1 비교 후 진행.
  : 기존 Top Model처럼 상위에서 1장 마다 맵 다운로드하여 매칭 비교 아님.
 
 2. 네트워크 폴더에 Data 업로드 기능 업데이트
  -> 5800개 Data 확인 중 마지막 5개 정도의 Data 업로드 안됨. 
    : 기능 수정 완료. Test 및 모니터링중.
 
 3. 로그 기간별 삭제 기능 구현 완료.
 
 4. MES 기능 구현.
  -> 기존 설비 Log 분석 중.
  -> MES 프로그램 개발 진행 중. 
  
 5. YMS 기능 수정.
  -> Result Data *.waf, *.prd 파일을 네트워크 폴더에 업로드 하는 것을 의미함.
  -> 라인 수 포맷 정확하게 맞춰서 작성되도록 코드 수정. 
  -> 기존: 측정 아이템 개수에 따라 변경되었지만 줄 수는 고정되도록 기능 수정.
  -> 기능 구현 완료.
  
 2/6(금) LCP-280
 1. 네트워크 폴더에 Data 업로드 시 무언정지 발생. 
  -> 원인:멀티쓰레드로 인한 프로그램 정지 발생.
  -> 기능 수정 후 조치 완료. ( YMS 완료. )
  
 2. 맵매치 재 검증 및 기능 수정 완료.
  -> 현상: wafer 3장 스캔 Test 결과 X축 반전으로 맵매칭 진행.
  -> 조치 완료 및 시뮬레이션 기능 구현 및 검증 완료.
 
 4. MES 기능 구현.
  -> 기존 설비 Log 분석 중.
  -> MES 프로그램 개발 진행 중. 
 
 //이거 하자. 
1. Recipe 공정 Data Offst 입력 기능 수정 요청.
 -> 엑셀파일에서 붙여넣기/복사 하면 적용되도록 기능 수정 요청.
2. Manual 동작 기능 추가 구현.
 -> wafer 로드/언로드 manual 기능 구현.
3. 알람 정리 
 -> ( 현재 구현되어있고 미비한 알람 내역 정리 )   


 2/7(토) LCP-280
 1. wafer Align 진행 시 Align 실패 알람 보완
  조치 : 설정 횟수만큼 얼라인 진행 후 맨 마지막에 
        설정 리밋값만큼 얼라인보정이 되지 않았을때 Max 7번까지 반복하여
		얼라인 수행하도록 기능 보완
		
 2. MapScan 기능 수정.
  현상: Test 자재 구동 시에 Die간의 거리가 멀면 Center기준에서 Die Mapping을 안하는 현상 발생.
  조치: Center기준에서 serch 영역 파라미터 변경 [1.5% -> 10%]
       -> Serch하는 die가 많을시에 처리 속도가 ↑. (모니터링)

 3. Index Load Align 에서 제품 못찾은 경우 Probe, Unloader 공정 Pass되도록 시컨스 수정.

 4. Wafer / Bin 제어 Manual 기능 업데이트.
  -> 기능 업데이트 중. 
  
 2/8(일) LPC-280
 1. Wafer / Bin 제어 Manual 기능 업데이트.
   1) wafer Load/unload manual 기능 업데이트.
   2) Bin Load/unload manual 기능 업데이트.
   3) wafer/bin 선택 후 공정 진행 기능 업데이트.
 
 2. Index Load Align 에서 제품 못찾은 경우 Probe, Unloader 공정 
	Pass되도록 시컨스 업데이트(버그 수정)
	
 3. 프로그램 GUI 업데이트.	
   

 2/9(월) LPC-280
 1. Recipe 공정 Data Offst 입력 기능 수정 요청.
 -> 엑셀파일에서 붙여넣기/복사 하면 적용되도록 기능 수정 요청사항 
 -> 기능 업데이트 완료. 
 
 2. Camera 기능 업데이트. 
 -> Camera 변경으로 인한 느려짐 현상 업데이트
 
 2/10(화) LPC-280
 1. 제품 자체 Run Test.
  -> 디버깅 및 버그 수정.
 2. LoadAlign 제품 Skip 시 Data 전달 로직 구현.
 3. Unload Arm 진행 시 Pick/Place WaitTime Param으로 적용.
 4. socket 미 사용관련 기능 업데이트.

 *남은 과제.
 1. MES 구현. 
  -> 현재 업체에서는 MES에 대한 중요도 X.
  -> 하지만 언제 해야한다고 할지 몰라서 개발 필요함. 
 2. 제어 메뉴얼 작성.
 3. 베트남 언어 번역. ( 기능 구현 완료. - 번역 필요 )
 4. 장비 Manual 동작 기능 보완.
 
 2/11(수) LPC-280
 1. 제품 자체 Run Test.
  -> 디버깅 및 버그 수정.
 2. Ready, Reset 기능 업데이트.
 
 2/12(목) LPC-280
 1. Index 예외처리 로직 수정.
  -> Index -> Unloader Seq 정리 및 기능 수정.
  
 
============== 진행 현황 ==================
1. 캘리브레이션 ~ 2/13(금)
 -> 신규 마스터 샘플 제공. 
 - 캘리브레이션 재 검증 (프로브 카드 및 소켓 셋팅)
 
2. data QMC vs data ADP  ★★★
 - Bin Rank 재현성 - 2/14(토) ~ 
  -> Bin Rank 재현성 조건은 현재 서울반도체장비와 1:1 매칭 조건.  
 - index socket 4번으로 Bin 재현성 Test 예정.

3. 프로빙 3~5 웨이퍼 검증
 -> 제품 제공 사유: 가장자리 양품 칩 색상 변경하여 샘플 제공
 -> Data 검증 후 진행 예정.
 
4. 마스크 맵 확인 완료. (모니터링 중)
 -> 설비에서의 스캔 정밀도 확인( 현재까지 이상 없음 )

5. 언로드 데이터 YSM 확인 완료. (모니터링 중)

*LCP-280 핵심 과제 
1. 데이터 검증 
 -> Bin Level 정합성 검증.
 -> 프로그카드 파손 문제 해결
2. 택타임 (목표: 0.5sec, 현재 1.2sec)
	 
*남은 과제 
1. 시양산 모니터링.(Test)
2. MES 기능 구현.
3. 언어 번역. (기능 구현 Test / 번역 필요)
4. 교육 및 메뉴얼 작성. 
 
2/13(금)
1. 마스터 샘플 캘리브레이션 작업
 -> 현재 1차 진행 시 WATT Data Fail.
  * 테스트 진행 시 조건 원복 후 변경하면서 Test 진행.
  ( 기존: integration 70ms, Averages 1회 )
  ( OD : 50mm )
  ( 적분구 높이: -46.2mm )
 -> 1차: Integration Time : 70 -> 100 Test : Fail.
 -> 2차: ProbeCard Z축 OD Data : 50mm -> 80mm Test : Fail.
 -> 3차: 적분구 높이 Z축 1mm Down ( 최대 근접 거리 측정 ) : Fail.
 -> 4차: 적분구 높이 Z축 1mm Down + 그리퍼 0.02mm Clamp. : 
 
  * 기존 대비 변경 항목.
  1. 마스터 웨이퍼 변경.
  2. Probe Card 변경.
  3. Probe Card 변경으로 인한 측정 셋팅 변경.
  
  
 
2. TeachingPos 추가.
 -> GripperX Init Pos.
 -> 

2/14(토)
1. DryRun Mode - 재 검증.
2. DryRun 구동으로 Socket Hole에 ProbeCard Pin 통과 확인.
  -> 4번 Index OK. ( Vision Mode시 Image 저장 )
3. 마스터 샘플 캘리브레이션
 -> 적분구 높이 Z축 3mm Up + 그리퍼 0.04mm Clamp : Fail.
 -> Setting 변경으로 WATT Data 확보가 안됨.

2/15(일)
1. 마스터 샘플 캘리브레이션 - WATT Fail.
 -> 적분구 높이 Z축 1.5mm Down + 그리퍼 0.03mm Clamp
 -> Setting 변경으로 WATT Data 확보가 안됨.
2. DryRun Mode 기능 보안 및 Test 완료.

2/16(월)
1. 마스터 샘플 캘리브레이션 - WATT Fail.
 -> Setting 변경으로 WATT Data 확보가 안됨.
 -> 측정관련된 항목 변경 Test 하였지만 현상 동일.

2/17(화)
1. 마스터 샘플 캘리브레이션 - WATT Fail.
 -> Setting 변경으로 WATT Data 확보가 안됨.
 -> 측정관련된 항목 변경 Test 하였지만 현상 동일.

2/18(수)
1. 마스터 샘플 캘리브레이션 - WATT Fail.
 -> Setting 변경으로 WATT Data 확보가 안됨.
2. wafer, bin - die Start 설정관련 기능 보안. 

2/19(목)
1. 마스터 샘플 캘리브레이션
 -> Setting 변경으로 WATT Data 확보가 안됨.
2. Recipe 설정관련 기능 보안.


2/20(금)
1. 마스터 샘플 캘리브레이션
 -> probecard 점프선 제거 후 측정. - 의미없음.
 
2. 기능 추가 및 보완
 1) Manual - Index
  - manual 구동 기능 보완
 2) Manual - UnloadArm
  - manual 구동 기능 보완

3. Wafer/Bin 예외처리 보완

2/23(월) LCP-280
1. Script 비교 분석 중.

2/24(화) LCP-280
1. Script 광측정 시컨스 변경 Test.

2/25(수) LCP-280
1. TackTime 분석 기능 구현 및 적용 완료.
2. 버튼별 인터락 기능 추가.
3. 

2/26(목) LCP-280
1. 스펙트럼, 소스메타를 통한 측정 시컨스 검토 중. 
 -> 기존: 전기특성 -> 광특성 검사
 -> 변경: 광특성 -> 전기특성 검사
 -> 변화 없음.
 
2/27(금) LCP-280
1. 서울반도체 MES 분석 및 MES 프로그램 개발 중.

2. 현재 상황 관련 메일 작성. 
 
 - 현재 마스터 캘리브레이션 공정 진행 중 WATT 항목에 대하여 통과를 못하고 있습니다. 
   장비 상태에 대하여 여러가지를 변경하고 바꿔서 10일 넘게 테스트를 하고 있지만 
   WATT 항목관련하여 통과가 안되고 있는 상황입니다. 
   
 - 자사에서 현재 준비하고 있는 부분은 CAS 제품을 변경하여 Test 진행할려고 준비 중에 있습니다.
   CAS 제품 납기 일정은 3/18 ~3/20 예정입니다. 최대한 당겨 보겠습니다.

 - 또한, 요청하신 플라스틱 니들도 준비하고 있으며 납기 예정일은 3/14 예정입니다.
 

 
* 메일 내용.

안녕하세요. 먼저 장비 셋업 및 캘리브레이션 일정이 지연되고 있는 점에 대해 진심으로 사과드립니다. 
현재 마스터 캘리브레이션 공정을 진행하는 과정에서 WATT 항목이 기준을 만족하지 못하여 통과되지 않고 있는 상황입니다. 
장비 상태 및 측정 조건 등 여러 항목을 변경하며 지속적으로 테스트를 진행하고 있으나, 
아직 WATT 항목 관련 문제가 해결되지 못하고 있습니다. 
해당 문제 해결을 위해 현재 CAS 장비를 변경하여 추가 테스트를 진행할 예정이며, 
신규 CAS 장비의 납기 일정은 3월 18일 ~ 3월 20일로 예상되고 있습니다. 
가능한 한 일정이 앞당겨질 수 있도록 지속적으로 확인하고 있습니다. 
또한 요청하신 플라스틱 니들 역시 준비 중이며, 납기 예정일은 3월 14일입니다. 
일정이 지연되고 있는 점 다시 한번 사과드리며, 
문제를 조속히 해결하여 장비 안정화 및 정상 운영이 가능하도록 최선을 다하겠습니다. 
감사합니다.


Dear Sir,

First of all, we sincerely apologize for the delay in the equipment setup and calibration schedule.

Currently, during the Master Calibration process, the WATT item has not met the required criteria and has therefore not passed.
We have been continuously conducting tests while adjusting various equipment 
conditions and measurement parameters; however, the issue related to the WATT item has not yet been resolved.

To address this matter, we plan to replace the current CAS equipment and proceed with additional testing.
The expected delivery schedule for the new CAS unit is between March 18 and March 20. 
We are closely monitoring the situation and will do our best to expedite the schedule if possible.

In addition, the requested plastic needles are currently being prepared, 
with an expected delivery date of March 14.

Once again, we sincerely apologize for the delay. We will make every effort to resolve the 
issue promptly and ensure the stabilization and normal operation of the equipment as soon as possible.

Thank you for your understanding.

Best regards.
 
2/28(토) LCP-280
1. 서울반도체 MES 분석 및 MES 프로그램 개발 중.
2. 진행현황 메일 송부.
 
3/2(월) LCP-280
1. 스크립트 수정 Test...
2. 알람 리스트 정리 및 기능 수정 

3/3(화) LCP-280
1. 알람 리스트 정리 및 기능 수정 완료. 
 - 알람 파일에서 불러와서 사용 
 - 알람 코드 정리하여 MES시에 송부 필요. ( 알람을 파일로 관리하도록 기능 수정 완료 )
 
3/4 (수) LCP-280
1. INO기준 셋팅값 및 SW 제어 방법론 분석하여 적용Test. 
 - WATT 결과 - 동일.


2. 해야할것.
 1) 레시피 구조 : 변경 요청의 건
 2) MES 개발의 건.
 3) 메뉴얼 기능 보완 (?) 
 4) 

3/8 (일) LCP-280
 1) 택타임 작업 중 현재: 850ms ( D4848 Model )
 
 3/12(수) LCP-280
 1) 택타임 작업 중 현재: 810ms (D4848 Model)
 
 한글로 출력되는 메시지 및 알람을 전부 영어로 번역해서 코드 구현해줘.
 주석은 안바꿔도돼. 메시지랑 알람만 한글로 되어 있는거 영어로 번역해줘. 패치형태로 제공해줘.


 var mb = new MessageBoxOk();
 mb.ShowDialog("Error", "MovePositionReady failed");
 
 
 
D4848 Model - 20번줄없어짐. 26번줄없어짐. -> 라인 +1씩 Y값 해야함....
-> 근데 이렇게 하면... 우선 이렇게 해야함.  
 우선 20번줄없어짐.!

 
 
 
 
 
CAS 교체 작업 이 후 
마스터 샘플과의 상관성은 OK 상황입니다. 

하지만
마스터 샘플과의 상관성을 확인 후 Gain, Offset 적용한 상태에서 
타 장비에서 측정한 제품과의 Data를 비교했을 경우(Bin Level 상관성)에 
Vf5 - OK
VF3 - NG
WP - NG
WATT - NG 
상황입니다. 

현재 진행 상황으로는 "마스터 샘플"을 다른 제품으로 다시 받아서 
재측정 후 Bin Level 상관성 Test를 다시 하고 있습니다. 

Data 기준으로 VF5, VF3, WP Data만 OK 되면 되는 것으로 얘기 되고 있습니다. 
WATT의 경우에는 장비의 상태가 달라서 나오는 것으로 판단하고 Offset 적용해서 
진행한다고 서울반도체 공정엔지니어가 얘기한 상황입니다. 
 
 
 
 
 
 
 
현장 공정 엔지니어에게 양산설비 <-> 표준설비 정합성 확인 요청하였지만
안된다는 답변을 받았습니다. 
사유로는 측정 시 제품 손상 및 칩 유실이 발생 할 수 있어서 할 수 없다는 입장입니다. 
( 측정을 다시 할 수 없다 및 현재 기준으로 우리 장비가 그냥 맞춰야 한다는 늬앙스 입니다. )

 표준 설비 문의 및 마스터 시료 재측정 요구 시에는 
R&D부서를 통해서 고객사로 제품 전달 후 전체 공정을 확인 후
분석실로 전달 받아서 검증하면 1~2주 소요 되어 어렵다고합니다.


 표준설비 <-> 자사설비 정합은 마스터 샘플로 맞춘다는 답변을 받았고 
마스터 샘플과 상관성을 맞추고 
타 장비에서 측정한 제품과의 상관성이 VF5, VF3, WP 항목이 맞으면 된다고 합니다.

현재 2-wire, 4-wire 컨택하여 마스터샘플 상관성 결과 Test 진행해 보았는데
큰 차이점이 없습니다. 

마스터 샘플 재측정하여 Data 확인하자고 요청하여 현재 마스터 샘플 재 측정 중입니다.

오후에 Bin level 다른 제품 준다고하여, 제품 받으면 측정하여 DATA 검증 예정입니다.   




타사 측정 제품의 Data를 가지고 
Gain/Offset을 비율 및 공통 Offset으로 넣을 수 있는지도 같이 확인 하고 있습니다.
(공정엔지니어와 같이 하고 있습니다.)
어제 1차 테스트 했을 경우 VF3 Data에 공통 offset을 적용해도 bin level이 변경되는 부분이 있어서
오차 범위를 더 줄여달라는 요청을 받았습니다. 


Bin31번 자재를 타사 장비에서 재 측정한 결과와 비교 결과
현재 자사 장비의 측정 Data와 유사한 것으로 검증되었습니다. 

명일 공정엔지니어가 다른 Bin Level의 자재로 똑같이 다시 측정하여
검증해보자고 하였으며, 내일 다시 한 번 검증해 보면 될 것 같습니다. 

공정엔지니어 말로는 이유를 모르겠다고 하지만.. 
Bin31 자재 or 서울반도체 장비 오차 같습니다. 

명일 재 측정 후 내용 공유 드리도록 하겠습니다. 

WATT의 경우에는 웨이퍼 설비와 인덱스 설비의 오차라 판단하고 
Data 검증에서 Skip 하도록 협의 되었습니다. 
(실제 재 측정결과 웨이퍼 설비에서는 WATT Data가 더 높게 측정 되었습니다.)


 3/26(목), 3/27(금) LCP-280
1. 인덱스 캘 진행시 횟수에따라 암이 계속 들고 진행하고. 
  마지막에만 칩을 들었던 위치에 다시 내려놓자. 
  -> 수정완료. TEST 필요.

2. 인덱스 캘 진행시 stop하고 start할때는 현재 칩기준 무조건 다음 칩으로 진행. 
  -> 수정완료. TEST 필요.

3. 인덱스 캘 진행시 중간에 로그가 아니라 알람으로 변경해서 진행. 
** 칩을 들때는 메뉴얼로 이동 시키고 시작하게?  FailAndStop("Index Cal Fail");
 -> 알람처리완료. Test 필요.
 
5. 장비 시작 시 측정기 / 카스 사용 상태 확인 후 진행 여부 판단.
  -> 아니면 그냥 알람. 돌리지 못하게.
  -> 수정완료. TEST 필요.
  
6. InputStage - 2차 얼라인 타이밍 확인 및 적용. 
  -> 수정완료. TEST 필요.  30 -> 50으로 완전히 벗어나고 찍도록 해놨는데...
  -> 의미없고 콜렛 변경 후 이미지 해상력 좋아짐.

7. preAling NG 발생 시 알람 울리도록 수정.  
  -> 수정완료. 
  
8. GripperX probeZ Up (Contect) Pos -> 1개에서 8개로 분할.
  -> 수정완료. TEST 필요.
  
9. 공정 진행 시 픽엔플랜, 메카얼라인 비젼 확인 시 
  -> 연속 알람 횟수 얼마에 따라 알람 발생 유/무 적용. 

10. 프로브 컨택 횟수 모니터링 기능 구현.

4. 프로그램 초기화 잡고 Ready잡을때 프로바카드 X,Y Ready 위치로 이동 시킬것.
  -> 수정완료. TEST 필요.
  -> 2차 수정. Test 필요.

 

 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 