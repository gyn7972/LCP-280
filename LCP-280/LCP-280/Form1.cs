//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
// 🚀 Position 관련 using은 주석 처리 (프로젝트 참조 문제)
//using QMC.Common;
//using QMC.Common.Component;
//using QMC.Common.UI;

namespace LCP_280
{
    enum ServoPosition
    {
        Home,
        Position1,
        Position2,
        Position3,
        Position4
    }
    enum MoveMode
    {
        Absolute,
        Relative
    }
    public partial class Form1// : Form
    {
        //private WaferMapView waferMapView;

        // 🚀 Position 관련 변수들 주석 처리 (프로젝트 참조 문제)
        //private PositionDataCollection _positionCollection;
        //private PositionProperty _selectedPositionProperty;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 🚀 사용자 이미지와 같은 Position Item 및 Editor 구현 예제 (주석 처리)
        /// 프로젝트 참조 설정 후 사용 가능
        /// </summary>
        /*
        private void InitializePositionItemEditor()
        {
            try
            {
                Console.WriteLine("=== Position Item & Editor 초기화 시작 ===");

                // 🚀 1. PositionDataCollection 생성 (사용자 이미지와 동일)
                _positionCollection = new PositionDataCollection("Lifter & Feeder Unit");
                
                // 사용자 이미지의 Position Item들 추가
                _positionCollection.Add(new BasePositionData("Lifter Loading Position", 0.0, "mm", "리프터 로딩 위치"));
                _positionCollection.Add(new BasePositionData("Lifter Unloading Position", 50.0, "mm", "리프터 언로딩 위치"));
                _positionCollection.Add(new BasePositionData("Cassette Slot Pitch", 10.0, "mm", "카세트 슬롯 간격"));
                _positionCollection.Add(new BasePositionData("Feeder Ready Position", 100.0, "mm", "피더 준비 위치"));
                _positionCollection.Add(new BasePositionData("Feeder Avoid Position", 150.0, "mm", "피더 회피 위치"));
                _positionCollection.Add(new BasePositionData("Feeder Stage Position", 200.0, "mm", "피더 스테이지 위치"));
                _positionCollection.Add(new BasePositionData("Feeder Cassette Position", 250.0, "mm", "피더 카세트 위치"));

                // 🚀 2. ListBoxItemsView에 Position Item들 표시 (사용자 이미지 왼쪽)
                // this.listBoxItemsView.GroupName = "Position Item";
                // this.listBoxItemsView.SetPositionDataCollection(_positionCollection);

                // 🚀 3. Position Item 선택 이벤트 연결
                // this.listBoxItemsView.PropertySelected += OnPositionItemSelected;

                // 🚀 4. 기본 PropertyCollectionView를 Editor로 설정 (사용자 이미지 오른쪽)
                // this.propertyCollectionView.GroupName = "Editor";

                Console.WriteLine($"✅ Position Item & Editor 초기화 완료: {_positionCollection.Count}개 Position");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Position Item & Editor 초기화 오류: {ex.Message}");
            }
        }
        */

        /// <summary>
        /// 🚀 Position Item 선택 이벤트 처리 (주석 처리)
        /// </summary>
        /*
        private void OnPositionItemSelected(object sender, string selectedTitle)
        {
            try
            {
                Console.WriteLine($"📍 Position Item 선택: {selectedTitle}");

                // 🚀 선택된 Position 찾기
                var selectedPositionData = _positionCollection.FindByTitle(selectedTitle);
                if (selectedPositionData == null)
                {
                    Console.WriteLine($"⚠️ Position을 찾을 수 없습니다: {selectedTitle}");
                    return;
                }

                // 🚀 PositionProperty 생성
                _selectedPositionProperty = new PositionProperty(selectedPositionData);

                // 🚀 Editor용 PropertyCollection 생성
                var editorProperties = PositionEditorUIHelper.CreatePositionEditorProperties(_selectedPositionProperty);

                // 🚀 PropertyCollectionView에 Editor 내용 표시 (사용자 이미지 오른쪽)
                // this.propertyCollectionView.SetProperties(editorProperties);

                Console.WriteLine($"✅ Editor에 Position 표시: {selectedTitle}");
                Console.WriteLine($"   값: {_selectedPositionProperty.Value:F3} {_selectedPositionProperty.Unit}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Position Item 선택 처리 오류: {ex.Message}");
            }
        }
        */

        /// <summary>
        /// 🚀 Position Item & Editor 기능들 (주석 처리)
        /// 프로젝트 참조 설정 후 사용 가능
        /// </summary>
        /*
        private void OnSaveButtonClick() { ... }
        private void OnCancelButtonClick() { ... }
        private void OnMovePositionButtonClick() { ... }
        private void OnTeachingMoveButtonClick() { ... }
        private void ShowPositionEditorDialog() { ... }
        public static void TestPositionItemEditor() { ... }
        */

//        private void Form1_Load(object sender, EventArgs e)
//        {
//            // 🚀 Position Item & Editor 초기화 추가
//            InitializePositionItemEditor();

//            // WaferMapView 초기화
//            waferMapView = new WaferMapView
//            {
//                Dock = DockStyle.Fill
//            };
//            //Controls.Add(waferMapView);

//            // WaferMap 데이터 생성
//            var items = GenerateWaferMapDataForImage();

//            // WaferMapView에 데이터 설정
//            waferMapView.SetItems(items);

//            // Set GroupBox names and ensure white background for all property controls
//            this.propertyCollectionView.GroupName = "Vision Settings";
//            this.ioPropertyCollectionView.GroupName = "Digital I/O Status";
//            this.listBoxItemsView.GroupName = "Position Item";

//            // 🚀 Position Item Editor 예제 추가
//            var ioProperties = new PropertyCollection();
//            ioProperties.ShowNoColumn = false; // 0열 표시 옵션
//            ioProperties.Add(new TitleOnlyProperty("No", "Name", "State")); // title 행 표시
//            ioProperties.Add(new PropertyState("X00", "X00 Item Name", true));
//            ioProperties.Add(new PropertyState("Y01", "Y01 Item Name", true));
//            ioProperties.Add(new PropertyState("X02", "X02 Item Name", false));
//            ioProperties.Add(new PropertyState("Y02", "Y02 Item Name", false));
//            ioProperties.Add(new PropertyState("X02", "X02 Item Name", false));
//            ioProperties.Add(new PropertyState("Y02", "Y02 Item Name", false));
//            this.ioPropertyCollectionView.SetProperties(ioProperties);
//            this.ioPropertyCollectionView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
//| System.Windows.Forms.AnchorStyles.Left)
//| System.Windows.Forms.AnchorStyles.Right)));

//            // ListBoxItemsView에 Position Item들 표시 (사용자 이미지와 동일)
//            this.listBoxItemsView.SetPositionDataCollection(_positionCollection);
//            this.listBoxItemsView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
//| System.Windows.Forms.AnchorStyles.Left)
//| System.Windows.Forms.AnchorStyles.Right)));
//        }

//        private List<WaferMapItem> GenerateWaferMapDataForImage()
//        {
//            var items = new List<WaferMapItem>();
//            int radius = 5; // WaferMap의 반지름
//            int center = radius; // 중심 좌표

//            for (int y = 0; y <= 2 * radius; y++)
//            {
//                for (int x = 0; x <= 2 * radius; x++)
//                {
//                    // 원형 영역 계산
//                    int dx = x - center;
//                    int dy = y - center;
//                    if (dx * dx + dy * dy <= radius * radius)
//                    {
//                        // 작업 유무 설정 (녹색 영역)
//                        bool isProcessed = (x >= center + 2 && x <= center + 3) && (y >= center - 1 && y <= center + 1);

//                        // BinRank는 예시로 모두 1로 설정
//                        items.Add(new WaferMapItem(x, y, 1, isProcessed));
//                    }
//                }
//            }

//            return items;
//        }
    }
}
