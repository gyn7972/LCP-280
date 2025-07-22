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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 예시 데이터 생성
            var properties = new PropertyCollection();
            properties.Add(new PropertyState("X00", "X00 Item Name", true));
            properties.Add(new PropertyState("X01", "X01 Item Name", true));
            properties.Add(new PropertyState("X02", "X02 Item Name", false));
            propertyCollectionView1.SetPropertiesWithState(properties);

            // PropertyCollectionView에 데이터 바인딩
        }
    }
}
