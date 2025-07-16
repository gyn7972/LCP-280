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
        private PropertyCollectionView propertyCollectionView;

        PropertyBase MYName = new PropertyBase("이름", "홍길동");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 예시 데이터 생성
            var properties = new PropertyCollection();
            properties.Add(MYName);
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new PropertyBase("나이", "30세"));
            properties.Add(new PropertyBase("성별", "남성"));
            properties.Add(new PropertyBase("주소", "서울시 강남구 역삼동"));
            properties.Add(new PropertyBase("전화번호", "010-1234-5678"));
            properties.Add(new PropertyBase("상태", "활성"));
            properties.Add(new PropertyBase("설명", "이것은 예시 데이터입니다."));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new PropertyBase("나이", "30세"));
            properties.Add(new PropertyBase("성별", "남성"));
            properties.Add(new PropertyBase("주소", "서울시 강남구 역삼동"));
            properties.Add(new PropertyBase("전화번호", "010-1234-5678"));
            properties.Add(new PropertyBase("상태", "활성"));
            properties.Add(new PropertyBase("설명", "이것은 예시 데이터입니다."));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new DoubleProperty("점수", 95.5));
            properties.Add(new PropertyBase("나이", "30세"));
            properties.Add(new PropertyBase("성별", "남성"));
            properties.Add(new PropertyBase("주소", "서울시 강남구 역삼동"));
            properties.Add(new PropertyBase("전화번호", "010-1234-5678"));
            properties.Add(new PropertyBase("상태", "활성"));
            properties.Add(new PropertyBase("설명", "이것은 예시 데이터입니다."));
            // PropertyCollectionView에 데이터 바인딩
            propertyCollectionView1.SetProperties(properties);
        }
    }
}
