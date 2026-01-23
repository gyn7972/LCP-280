using QMC.Common;
using QMC.Common.CustomControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class ltfTestConditionSetPage
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lbSetNameValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSaveSet = new QMC.Common.IndividualMenuButton();
            this.btnNewSet = new QMC.Common.IndividualMenuButton();
            this.btnOpenSet = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.btnItemDelete = new QMC.Common.IndividualMenuButton();
            this.btnItemCopy = new QMC.Common.IndividualMenuButton();
            this.btnItemPaste = new QMC.Common.IndividualMenuButton();
            this.btnItemDown = new QMC.Common.IndividualMenuButton();
            this.btnItemUp = new QMC.Common.IndividualMenuButton();
            this.btnItemInsert = new QMC.Common.IndividualMenuButton();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.pcvItem = new QMC.Common.PropertyCollectionView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btnItemClear = new QMC.Common.IndividualMenuButton();
            this.btnItemModify = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 325F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.lbSetNameValue, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.23338F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94.76662F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(919, 694);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // lbSetNameValue
            // 
            this.lbSetNameValue.BackColor = System.Drawing.Color.Black;
            this.lbSetNameValue.BorderColor = System.Drawing.Color.Black;
            this.lbSetNameValue.BorderWidth = 1;
            this.lbSetNameValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSetNameValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbSetNameValue.ForeColor = System.Drawing.Color.Lime;
            this.lbSetNameValue.Location = new System.Drawing.Point(3, 3);
            this.lbSetNameValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbSetNameValue.Name = "lbSetNameValue";
            this.lbSetNameValue.Size = new System.Drawing.Size(913, 30);
            this.lbSetNameValue.TabIndex = 21;
            this.lbSetNameValue.Text = " - ";
            this.lbSetNameValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.78947F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 84.21053F));
            this.tableLayoutPanel3.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.dataGrid, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 39);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(913, 652);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel6);
            this.panel2.Controls.Add(this.tableLayoutPanel5);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(138, 646);
            this.panel2.TabIndex = 2;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.btnSaveSet, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.btnNewSet, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.btnOpenSet, 0, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 519);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(138, 127);
            this.tableLayoutPanel6.TabIndex = 37;
            // 
            // btnSaveSet
            // 
            this.btnSaveSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSaveSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSaveSet.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSaveSet.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveSet.CustomForeColor = System.Drawing.Color.Black;
            this.btnSaveSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveSet.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveSet.ForeColor = System.Drawing.Color.Black;
            this.btnSaveSet.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSaveSet.Location = new System.Drawing.Point(3, 87);
            this.btnSaveSet.Name = "btnSaveSet";
            this.btnSaveSet.Size = new System.Drawing.Size(132, 37);
            this.btnSaveSet.TabIndex = 36;
            this.btnSaveSet.TabStop = false;
            this.btnSaveSet.Text = "Save";
            this.btnSaveSet.UseVisualStyleBackColor = false;
            this.btnSaveSet.Click += new System.EventHandler(this.btnSaveSet_Click);
            // 
            // btnNewSet
            // 
            this.btnNewSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnNewSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnNewSet.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnNewSet.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnNewSet.CustomForeColor = System.Drawing.Color.Black;
            this.btnNewSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNewSet.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewSet.ForeColor = System.Drawing.Color.Black;
            this.btnNewSet.ImageSize = new System.Drawing.Size(45, 45);
            this.btnNewSet.Location = new System.Drawing.Point(3, 3);
            this.btnNewSet.Name = "btnNewSet";
            this.btnNewSet.Size = new System.Drawing.Size(132, 36);
            this.btnNewSet.TabIndex = 35;
            this.btnNewSet.TabStop = false;
            this.btnNewSet.Text = "New";
            this.btnNewSet.UseVisualStyleBackColor = false;
            this.btnNewSet.Click += new System.EventHandler(this.btnNewSet_Click);
            // 
            // btnOpenSet
            // 
            this.btnOpenSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnOpenSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnOpenSet.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnOpenSet.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnOpenSet.CustomForeColor = System.Drawing.Color.Black;
            this.btnOpenSet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenSet.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenSet.ForeColor = System.Drawing.Color.Black;
            this.btnOpenSet.ImageSize = new System.Drawing.Size(45, 45);
            this.btnOpenSet.Location = new System.Drawing.Point(3, 45);
            this.btnOpenSet.Name = "btnOpenSet";
            this.btnOpenSet.Size = new System.Drawing.Size(132, 36);
            this.btnOpenSet.TabIndex = 34;
            this.btnOpenSet.TabStop = false;
            this.btnOpenSet.Text = "Open";
            this.btnOpenSet.UseVisualStyleBackColor = false;
            this.btnOpenSet.Click += new System.EventHandler(this.btnOpenSet_Click);
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.btnItemDelete, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.btnItemCopy, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.btnItemPaste, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.btnItemDown, 0, 5);
            this.tableLayoutPanel5.Controls.Add(this.btnItemUp, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.btnItemInsert, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 6;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(138, 237);
            this.tableLayoutPanel5.TabIndex = 36;
            // 
            // btnItemDelete
            // 
            this.btnItemDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemDelete.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemDelete.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnItemDelete.CustomForeColor = System.Drawing.Color.Black;
            this.btnItemDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItemDelete.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnItemDelete.ForeColor = System.Drawing.Color.Black;
            this.btnItemDelete.ImageSize = new System.Drawing.Size(45, 45);
            this.btnItemDelete.Location = new System.Drawing.Point(3, 42);
            this.btnItemDelete.Name = "btnItemDelete";
            this.btnItemDelete.Size = new System.Drawing.Size(132, 33);
            this.btnItemDelete.TabIndex = 28;
            this.btnItemDelete.TabStop = false;
            this.btnItemDelete.Text = "Item Delete";
            this.btnItemDelete.UseVisualStyleBackColor = false;
            this.btnItemDelete.Click += new System.EventHandler(this.btnItemDelete_Click);
            // 
            // btnItemCopy
            // 
            this.btnItemCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemCopy.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemCopy.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemCopy.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnItemCopy.CustomForeColor = System.Drawing.Color.Black;
            this.btnItemCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItemCopy.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnItemCopy.ForeColor = System.Drawing.Color.Black;
            this.btnItemCopy.ImageSize = new System.Drawing.Size(45, 45);
            this.btnItemCopy.Location = new System.Drawing.Point(3, 81);
            this.btnItemCopy.Name = "btnItemCopy";
            this.btnItemCopy.Size = new System.Drawing.Size(132, 33);
            this.btnItemCopy.TabIndex = 29;
            this.btnItemCopy.TabStop = false;
            this.btnItemCopy.Text = "Item Copy";
            this.btnItemCopy.UseVisualStyleBackColor = false;
            this.btnItemCopy.Click += new System.EventHandler(this.btnItemCopy_Click);
            // 
            // btnItemPaste
            // 
            this.btnItemPaste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemPaste.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemPaste.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemPaste.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnItemPaste.CustomForeColor = System.Drawing.Color.Black;
            this.btnItemPaste.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItemPaste.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnItemPaste.ForeColor = System.Drawing.Color.Black;
            this.btnItemPaste.ImageSize = new System.Drawing.Size(45, 45);
            this.btnItemPaste.Location = new System.Drawing.Point(3, 120);
            this.btnItemPaste.Name = "btnItemPaste";
            this.btnItemPaste.Size = new System.Drawing.Size(132, 33);
            this.btnItemPaste.TabIndex = 30;
            this.btnItemPaste.TabStop = false;
            this.btnItemPaste.Text = "Item Paste";
            this.btnItemPaste.UseVisualStyleBackColor = false;
            this.btnItemPaste.Click += new System.EventHandler(this.btnItemPaste_Click);
            // 
            // btnItemDown
            // 
            this.btnItemDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemDown.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemDown.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnItemDown.CustomForeColor = System.Drawing.Color.Black;
            this.btnItemDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItemDown.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnItemDown.ForeColor = System.Drawing.Color.Black;
            this.btnItemDown.ImageSize = new System.Drawing.Size(45, 45);
            this.btnItemDown.Location = new System.Drawing.Point(3, 198);
            this.btnItemDown.Name = "btnItemDown";
            this.btnItemDown.Size = new System.Drawing.Size(132, 36);
            this.btnItemDown.TabIndex = 32;
            this.btnItemDown.TabStop = false;
            this.btnItemDown.Text = "▼";
            this.btnItemDown.UseVisualStyleBackColor = false;
            this.btnItemDown.Click += new System.EventHandler(this.btnItemDown_Click);
            // 
            // btnItemUp
            // 
            this.btnItemUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemUp.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemUp.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnItemUp.CustomForeColor = System.Drawing.Color.Black;
            this.btnItemUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItemUp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnItemUp.ForeColor = System.Drawing.Color.Black;
            this.btnItemUp.ImageSize = new System.Drawing.Size(45, 45);
            this.btnItemUp.Location = new System.Drawing.Point(3, 159);
            this.btnItemUp.Name = "btnItemUp";
            this.btnItemUp.Size = new System.Drawing.Size(132, 33);
            this.btnItemUp.TabIndex = 31;
            this.btnItemUp.TabStop = false;
            this.btnItemUp.Text = "▲";
            this.btnItemUp.UseVisualStyleBackColor = false;
            this.btnItemUp.Click += new System.EventHandler(this.btnItemUp_Click);
            // 
            // btnItemInsert
            // 
            this.btnItemInsert.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemInsert.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemInsert.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemInsert.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnItemInsert.CustomForeColor = System.Drawing.Color.Black;
            this.btnItemInsert.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItemInsert.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnItemInsert.ForeColor = System.Drawing.Color.Black;
            this.btnItemInsert.ImageSize = new System.Drawing.Size(45, 45);
            this.btnItemInsert.Location = new System.Drawing.Point(3, 3);
            this.btnItemInsert.Name = "btnItemInsert";
            this.btnItemInsert.Size = new System.Drawing.Size(132, 33);
            this.btnItemInsert.TabIndex = 27;
            this.btnItemInsert.TabStop = false;
            this.btnItemInsert.Text = "Item Insert";
            this.btnItemInsert.UseVisualStyleBackColor = false;
            this.btnItemInsert.Click += new System.EventHandler(this.btnItemInsert_Click);
            // 
            // dataGrid
            // 
            this.dataGrid.AllowUserToAddRows = false;
            this.dataGrid.AllowUserToDeleteRows = false;
            this.dataGrid.AllowUserToResizeColumns = false;
            this.dataGrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGrid.Location = new System.Drawing.Point(147, 3);
            this.dataGrid.MultiSelect = false;
            this.dataGrid.Name = "dataGrid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGrid.RowHeadersVisible = false;
            this.dataGrid.RowTemplate.Height = 23;
            this.dataGrid.Size = new System.Drawing.Size(763, 646);
            this.dataGrid.TabIndex = 0;
            this.dataGrid.SelectionChanged += new System.EventHandler(this.dataGrid_SelectionChanged);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.pcvItem, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(925, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 83.30976F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.69024F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(325, 700);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // pcvItem
            // 
            this.pcvItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcvItem.FastBuild = true;
            this.pcvItem.GroupName = "Item Editor";
            this.pcvItem.Location = new System.Drawing.Point(0, 0);
            this.pcvItem.Margin = new System.Windows.Forms.Padding(0);
            this.pcvItem.Name = "pcvItem";
            this.pcvItem.Size = new System.Drawing.Size(325, 583);
            this.pcvItem.SuppressResizeInvalidation = true;
            this.pcvItem.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel7);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 586);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(319, 111);
            this.panel1.TabIndex = 1;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.btnItemClear, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.btnItemModify, 0, 1);
            this.tableLayoutPanel7.Location = new System.Drawing.Point(170, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(146, 84);
            this.tableLayoutPanel7.TabIndex = 27;
            // 
            // btnItemClear
            // 
            this.btnItemClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemClear.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemClear.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnItemClear.CustomForeColor = System.Drawing.Color.Black;
            this.btnItemClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItemClear.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnItemClear.ForeColor = System.Drawing.Color.Black;
            this.btnItemClear.ImageSize = new System.Drawing.Size(45, 45);
            this.btnItemClear.Location = new System.Drawing.Point(3, 3);
            this.btnItemClear.Name = "btnItemClear";
            this.btnItemClear.Size = new System.Drawing.Size(140, 36);
            this.btnItemClear.TabIndex = 26;
            this.btnItemClear.TabStop = false;
            this.btnItemClear.Text = "Clear";
            this.btnItemClear.UseVisualStyleBackColor = false;
            this.btnItemClear.Click += new System.EventHandler(this.btnItemClear_Click);
            // 
            // btnItemModify
            // 
            this.btnItemModify.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemModify.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnItemModify.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnItemModify.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnItemModify.CustomForeColor = System.Drawing.Color.Black;
            this.btnItemModify.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnItemModify.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnItemModify.ForeColor = System.Drawing.Color.Black;
            this.btnItemModify.ImageSize = new System.Drawing.Size(45, 45);
            this.btnItemModify.Location = new System.Drawing.Point(3, 45);
            this.btnItemModify.Name = "btnItemModify";
            this.btnItemModify.Size = new System.Drawing.Size(140, 36);
            this.btnItemModify.TabIndex = 25;
            this.btnItemModify.TabStop = false;
            this.btnItemModify.Text = "Modify";
            this.btnItemModify.UseVisualStyleBackColor = false;
            this.btnItemModify.Click += new System.EventHandler(this.btnItemModify_Click);
            // 
            // TestConditionSetPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TestConditionSetPage";
            this.Size = new System.Drawing.Size(1250, 700);
            this.Load += new System.EventHandler(this.TestConditionSetPage_Load);
            this.VisibleChanged += new System.EventHandler(this.TestConditionSetPage_VisibleChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private CustomBorderLabel lbSetNameValue;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel2;
        private IndividualMenuButton btnNewSet;
        private IndividualMenuButton btnOpenSet;
        private IndividualMenuButton btnItemDown;
        private IndividualMenuButton btnItemUp;
        private IndividualMenuButton btnItemPaste;
        private IndividualMenuButton btnItemCopy;
        private IndividualMenuButton btnItemDelete;
        private IndividualMenuButton btnItemInsert;
        private System.Windows.Forms.DataGridView dataGrid;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private PropertyCollectionView pcvItem;
        private System.Windows.Forms.Panel panel1;
        private IndividualMenuButton btnItemClear;
        private IndividualMenuButton btnItemModify;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private IndividualMenuButton btnSaveSet;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
    }
}
