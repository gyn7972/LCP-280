using QMC.Common;
using System.Windows.Forms;
using System.Drawing;

namespace QMC.LCP_280.Process.Unit.FormRecipe
{
    partial class Main_Recipe
    {
        private System.ComponentModel.IContainer components = null;

        // UI controls
        private ListBoxItemsView recipeListView;
        private PropertyCollectionView propertyCollectionView;
        private Button btnNew;
        private Button btnOpen;
        private Button btnCopy;
        private Button btnPaste;
        private Button btnSave;
        private Button btnDelete;
        private Panel panelLeftBorder;
        private Panel panelRightBorder;
        private Panel panelVerticalSeparator;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.recipeListView = new QMC.Common.ListBoxItemsView();
            this.propertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnPaste = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.panelLeftBorder = new System.Windows.Forms.Panel();
            this.panelRightBorder = new System.Windows.Forms.Panel();
            this.panelVerticalSeparator = new System.Windows.Forms.Panel();
            this.panelLeftBorder.SuspendLayout();
            this.panelRightBorder.SuspendLayout();
            this.SuspendLayout();
            // 
            // recipeListView
            // 
            this.recipeListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.recipeListView.BorderColor = System.Drawing.Color.Black;
            this.recipeListView.BorderWidth = 2;
            this.recipeListView.GroupBackColor = System.Drawing.Color.White;
            this.recipeListView.GroupForeColor = System.Drawing.Color.Black;
            this.recipeListView.GroupName = "Recipe List";
            this.recipeListView.ItemBackColor = System.Drawing.Color.Black;
            this.recipeListView.ItemForeColor = System.Drawing.Color.Lime;
            this.recipeListView.ListBackColor = System.Drawing.Color.Black;
            this.recipeListView.ListForeColor = System.Drawing.Color.Lime;
            this.recipeListView.Location = new System.Drawing.Point(3, 3);
            this.recipeListView.Name = "recipeListView";
            this.recipeListView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.recipeListView.SelectedForeColor = System.Drawing.Color.Black;
            this.recipeListView.SelectedIndex = -1;
            this.recipeListView.Size = new System.Drawing.Size(297, 560);
            this.recipeListView.TabIndex = 0;
            // 
            // propertyCollectionView
            // 
            this.propertyCollectionView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyCollectionView.FastBuild = true;
            this.propertyCollectionView.GroupName = "Property";
            this.propertyCollectionView.Location = new System.Drawing.Point(3, 3);
            this.propertyCollectionView.Name = "propertyCollectionView";
            this.propertyCollectionView.Size = new System.Drawing.Size(624, 560);
            this.propertyCollectionView.SuppressResizeInvalidation = true;
            this.propertyCollectionView.TabIndex = 1;
            // 
            // btnNew
            // 
            this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNew.Location = new System.Drawing.Point(180, 569);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(120, 48);
            this.btnNew.TabIndex = 2;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOpen.Location = new System.Drawing.Point(3, 569);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(120, 48);
            this.btnOpen.TabIndex = 3;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCopy.Location = new System.Drawing.Point(3, 623);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(87, 48);
            this.btnCopy.TabIndex = 4;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPaste.Location = new System.Drawing.Point(108, 623);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(87, 48);
            this.btnPaste.TabIndex = 5;
            this.btnPaste.Text = "Paste";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(507, 569);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(120, 48);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(213, 623);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(87, 48);
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // panelLeftBorder
            // 
            this.panelLeftBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelLeftBorder.BackColor = System.Drawing.Color.Transparent;
            this.panelLeftBorder.Controls.Add(this.btnPaste);
            this.panelLeftBorder.Controls.Add(this.btnDelete);
            this.panelLeftBorder.Controls.Add(this.btnCopy);
            this.panelLeftBorder.Controls.Add(this.btnOpen);
            this.panelLeftBorder.Controls.Add(this.btnNew);
            this.panelLeftBorder.Controls.Add(this.recipeListView);
            this.panelLeftBorder.Location = new System.Drawing.Point(12, 12);
            this.panelLeftBorder.Name = "panelLeftBorder";
            this.panelLeftBorder.Size = new System.Drawing.Size(312, 680);
            this.panelLeftBorder.TabIndex = 7;
            // 
            // panelRightBorder
            // 
            this.panelRightBorder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRightBorder.BackColor = System.Drawing.Color.Transparent;
            this.panelRightBorder.Controls.Add(this.propertyCollectionView);
            this.panelRightBorder.Controls.Add(this.btnSave);
            this.panelRightBorder.Location = new System.Drawing.Point(338, 12);
            this.panelRightBorder.Name = "panelRightBorder";
            this.panelRightBorder.Size = new System.Drawing.Size(648, 680);
            this.panelRightBorder.TabIndex = 8;
            // 
            // panelVerticalSeparator
            // 
            this.panelVerticalSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelVerticalSeparator.BackColor = System.Drawing.Color.Silver;
            this.panelVerticalSeparator.Location = new System.Drawing.Point(330, 12);
            this.panelVerticalSeparator.Name = "panelVerticalSeparator";
            this.panelVerticalSeparator.Size = new System.Drawing.Size(2, 680);
            this.panelVerticalSeparator.TabIndex = 9;
            // 
            // Main_Recipe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1244, 711);
            this.Controls.Add(this.panelLeftBorder);
            this.Controls.Add(this.panelRightBorder);
            this.Controls.Add(this.panelVerticalSeparator);
            this.Name = "Main_Recipe";
            this.Text = "Main_Recipe";
            this.panelLeftBorder.ResumeLayout(false);
            this.panelRightBorder.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}