//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

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
        public Form1()
        {
            InitializeComponent();
        }

//        private void Form1_Load(object sender, EventArgs e)
//        {
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

//            // PropertyCollectionView 예시 데이터 생성 - 동적 크기 조정 테스트를 위해 다양한 갯수로 설정
//            var properties = new PropertyCollection();
//            properties.IsInputParameter = false;
//            properties.Add(new TitleOnlyProperty("Common"));
//            properties.Add(new ComboBoxProperty("ROI Visible", "Enable", new List<string> { "Enable", "Disable" }));
//            properties.Add(new ComboBoxProperty("Cross Visible", "Enable", new List<string> { "Enable", "Disable" }));
//            properties.Add(new TitleOnlyProperty("Lens Scale"));
//            properties.Add(new PropertyBase("Lens Scale X", "1.000"));
//            properties.Add(new PropertyBase("Lens Scale Y", "1.000"));
//            properties.Add(new TitleOnlyProperty("Gain & Offset"));
//            properties.Add(new PropertyBase("Gain", "1.000"));
//            properties.Add(new PropertyBase("Position", "0.000"));
//            properties.Add(new PropertyBase("Offset", "0.000"));
//            this.propertyCollectionView.SetProperties(properties);
//            // PropertyCollectionView에 데이터 바인딩
//            this.propertyCollectionView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
//| System.Windows.Forms.AnchorStyles.Left)
//| System.Windows.Forms.AnchorStyles.Right)));

//            // IOPropertyCollectionView - 동적 크기 조정 테스트를 위해 다양한 갯수로 설정
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

//            visionImageview.ShowLiveGrabButtons = true;
//            visionImageview.SetImageViewName("Input Camera", "Output Camera");
//            this.visionImageview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
//| System.Windows.Forms.AnchorStyles.Left)
//| System.Windows.Forms.AnchorStyles.Right)));

//            string imagePath = System.IO.Path.Combine(Application.StartupPath, "AI_참고그림", "WaferMap.png");
//            if (System.IO.File.Exists(imagePath))
//            {
//                visionImageview.SetImage(0, new Bitmap(imagePath));
//            }
//            else
//            {
//                MessageBox.Show("이미지 파일을 찾을 수 없습니다: " + imagePath);
//            }
//            string imagePath2 = System.IO.Path.Combine(Application.StartupPath, "AI_참고그림", "Mapping.png");
//            if (System.IO.File.Exists(imagePath2))
//            {
//                visionImageview.SetImage(1, new Bitmap(imagePath2));
//            }
//            else
//            {
//                MessageBox.Show("이미지 파일을 찾을 수 없습니다: " + imagePath2);
//            }

//            // ListBoxItemsView 예시 데이터 설정 - 동적 크기 조정 테스트
//            // ServoPosition은 enum 타입이므로, enum의 값들을 배열로 전달해야 합니다.
//            this.listBoxItemsView.SetItems(typeof(ServoPosition));
//            this.listBoxItemsView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
//| System.Windows.Forms.AnchorStyles.Left)
//| System.Windows.Forms.AnchorStyles.Right)));

//            this.radioButtonView.SetOptions(false, typeof(MoveMode));

//            {
//                var cassetteList = new List<CassetteData>();

//                // 첫 번째 CassetteData 생성
//                var cassetteA = new CassetteData
//                {
//                    CassetteIndex = 0,
//                    CassetteId = "A001",
//                    State = CassetteState.Present
//                };
//                cassetteA.GenerateWaferData(20); // 슬롯 개수 예시: 25

//                // 두 번째 CassetteData 생성
//                var cassetteB = new CassetteData
//                {
//                    CassetteIndex = 1,
//                    CassetteId = "B001",
//                    State = CassetteState.Present
//                };
//                cassetteB.GenerateWaferData(20);

//                cassetteList.Add(cassetteA);
//                cassetteList.Add(cassetteB);

//                // CassetteMapView에 전달
//                cassetteMapView.SetCassettes(cassetteList);

//                // 예시: 모든 Wafer의 모든 Slot을 Loaded로 변경
//                foreach (var cassette in cassetteList)
//                {
//                    foreach (var wafer in cassette.WaferList)
//                    {
//                        for (int i = 0; i < wafer.SlotStates.Length; i++)
//                        {
//                            wafer.SlotStates[i] = WaferCassetteLoadState.Loaded;
//                        }
//                    }
//                }
//            }
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
