using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public partial class Form1 : Form
    {
        private WaferMapView waferMapView;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // WaferMapView 초기화
            waferMapView = new WaferMapView
            {
                Dock = DockStyle.Fill
            };
            //Controls.Add(waferMapView);

            // WaferMap 데이터 생성
            var items = GenerateWaferMapDataForImage();

            // WaferMapView에 데이터 설정
            waferMapView.SetItems(items);

            // PropertyCollectionView 예시 데이터 생성
            var properties = new PropertyCollection();
            properties.Add(new TitleOnlyProperty("Common"));
            properties.Add(new ComboBoxProperty("ROI Visible", "Enable", new List<string> { "Enable", "Disable" }));
            properties.Add(new ComboBoxProperty("Cross Visible", "Enable", new List<string> { "Enable", "Disable" }));
            properties.Add(new TitleOnlyProperty("Lens Scale"));
            properties.Add(new PropertyBase("Lens Scale X", "1.000"));
            properties.Add(new PropertyBase("Lens Scale Y", "1.000"));
            properties.Add(new TitleOnlyProperty("Gain & Offset"));
            properties.Add(new PropertyBase("Gain", "1.000"));
            properties.Add(new PropertyBase("Offset", "0.000"));
            this.propertyCollectionView1.SetProperties(properties);
            // PropertyCollectionView에 데이터 바인딩
            this.propertyCollectionView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
    | System.Windows.Forms.AnchorStyles.Left)
    | System.Windows.Forms.AnchorStyles.Right)));
            
            var ioProperties = new PropertyCollection();
            ioProperties.Add(new TitleOnlyProperty("No", "Name", "State"));
            ioProperties.Add(new PropertyState("X00", "X00 Item Name", true));
            ioProperties.Add(new PropertyState("X01", "X01 Item Name", true));
            ioProperties.Add(new PropertyState("X02", "X02 Item Name", false));
            this.ioPropertyCollectionView1.SetProperties(ioProperties);
            this.ioPropertyCollectionView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
    | System.Windows.Forms.AnchorStyles.Left)
    | System.Windows.Forms.AnchorStyles.Right)));
        }

        private List<WaferMapItem> GenerateWaferMapDataForImage()
        {
            var items = new List<WaferMapItem>();
            int radius = 5; // WaferMap의 반지름
            int center = radius; // 중심 좌표

            for (int y = 0; y <= 2 * radius; y++)
            {
                for (int x = 0; x <= 2 * radius; x++)
                {
                    // 원형 영역 계산
                    int dx = x - center;
                    int dy = y - center;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        // 작업 유무 설정 (녹색 영역)
                        bool isProcessed = (x >= center + 2 && x <= center + 3) && (y >= center - 1 && y <= center + 1);

                        // BinRank는 예시로 모두 1로 설정
                        items.Add(new WaferMapItem(x, y, 1, isProcessed));
                    }
                }
            }

            return items;
        }
    }
}
