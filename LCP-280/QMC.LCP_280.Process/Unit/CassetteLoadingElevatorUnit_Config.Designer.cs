using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class CassetteLoadingElevatorUnit_Config
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private PropertyCollectionView propertyCollectionView;
        private IOPropertyCollectionView IOPropertyCollectionView;
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.propertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.IOPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.SuspendLayout();
            // 
            // propertyCollectionView
            // 
            this.propertyCollectionView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyCollectionView.GroupName = "Position Item";
            this.propertyCollectionView.Location = new System.Drawing.Point(25, 39);
            this.propertyCollectionView.Name = "propertyCollectionView";
            this.propertyCollectionView.Size = new System.Drawing.Size(326, 365);
            this.propertyCollectionView.TabIndex = 0;
            this.propertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            // 
            // IOPropertyCollectionView
            // 
            this.IOPropertyCollectionView.GroupName = "IO Property Group";
            this.IOPropertyCollectionView.Location = new System.Drawing.Point(409, 98);
            this.IOPropertyCollectionView.Name = "IOPropertyCollectionView";
            this.IOPropertyCollectionView.Size = new System.Drawing.Size(323, 178);
            this.IOPropertyCollectionView.TabIndex = 1;
            this.propertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            // 
            // CassetteLoadingElevatorUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.propertyCollectionView);
            this.Controls.Add(this.IOPropertyCollectionView);
            this.Name = "CassetteLoadingElevatorUnit_Config";
            this.Text = "CassetteLoadingElevatorUnit_Config";
            this.ResumeLayout(false);

        }

        private void InitializeUI()
        {
            try
            {
                // Properties 설정 - 이제 propertyCollectionView는 이미 생성되어 있음
                var properties = CreatePropertyCollection();
                propertyCollectionView?.SetProperties(properties);

                // IO Properties 설정
                var ioProperties = CreatePropertyCollection2();
                IOPropertyCollectionView?.SetProperties(ioProperties);
            }
            catch (Exception ex)
            {
                // 디버그 모드에서만 오류 표시
#if DEBUG
                MessageBox.Show($"커스텀 컴포넌트 초기화 오류: {ex.Message}");
#endif
            }
        }

        private PropertyCollection CreatePropertyCollection()
        {
            var properties = new PropertyCollection
            {
                IsInputParameter = false
            };
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
            return properties;
        }

        private PropertyCollection CreatePropertyCollection2()
        {
            var ioProperties = new PropertyCollection()
            {
                ShowNoColumn = true // 0열 표시 옵션
            };
            ioProperties.Add(new TitleOnlyProperty("No", "Name", "State")); // title 행 표시
            ioProperties.Add(new PropertyState("X00", "X00 Item Name", true));
            ioProperties.Add(new PropertyState("Y01", "Y01 Item Name", true));
            ioProperties.Add(new PropertyState("X02", "X02 Item Name", false));
            ioProperties.Add(new PropertyState("Y02", "Y02 Item Name", false));
            ioProperties.Add(new PropertyState("X02", "X02 Item Name", false));
            ioProperties.Add(new PropertyState("Y02", "Y02 Item Name", false));
            this.IOPropertyCollectionView.SetProperties(ioProperties);
            return ioProperties;
        }
        #endregion
    }
}