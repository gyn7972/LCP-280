using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// CassetteLoadingElevator Unit의 Config 폼 예시
    /// </summary>
    public partial class CassetteLoadingElevatorUnit_Config : Form
    {
        private PropertyCollectionView propertyCollectionView;
        
        public CassetteLoadingElevatorUnit_Config()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            propertyCollectionView = new PropertyCollectionView("Position Item");

            var properties = new PropertyCollection();
            properties.IsInputParameter = false;
            properties.Add(new TitleOnlyProperty("Common"));
            properties.Add(new ComboBoxProperty("ROI Visible", "Enable", new List<string> { "Enable", "Disable" }));
            properties.Add(new ComboBoxProperty("Cross Visible", "Enable", new List<string> { "Enable", "Disable" }));
            properties.Add(new TitleOnlyProperty("Lens Scale"));
            properties.Add(new PropertyBase("Lens Scale X", "1.000"));
            properties.Add(new PropertyBase("Lens Scale Y", "1.000"));
            properties.Add(new TitleOnlyProperty("Gain & Offset"));
            properties.Add(new PropertyBase("Gain", "1.000"));
            properties.Add(new PropertyBase("Position", "0.000"));
            properties.Add(new PropertyBase("Offset", "0.000"));
            this.propertyCollectionView.SetProperties(properties);

            this.Controls.Add(propertyCollectionView);

            // PropertyCollectionView에 데이터 바인딩
            this.propertyCollectionView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
| System.Windows.Forms.AnchorStyles.Left)
| System.Windows.Forms.AnchorStyles.Right)));
        }
    }
}