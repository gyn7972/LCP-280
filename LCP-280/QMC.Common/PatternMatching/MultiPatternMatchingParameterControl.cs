using QMC.Common;
using QMC.Common.Vision;
using QMC.Common.VisionPart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common
{
    #region Define
    public delegate void MultiParameterValueChangeEventHandle();
    public delegate void MultiParameterButtonClickEventHandle();
    public delegate void MultiParameterImageChangedEventHandlerHandler(int nIndex, VisionImage image);
    #endregion

    public partial class MultiPatternMatchingParameterControl : UserControl
    {
        #region Define
        public event MultiParameterValueChangeEventHandle MultiParameterValueChangeEvent;
        public event MultiParameterValueChangeEventHandle MultiParameterButtonClickEvent;
        public event MultiParameterImageChangedEventHandlerHandler MultiImageChangeEvent;
        public event EventHandler TrainImageListChanged; // 새 이벤트: 리스트 구조 변경 알림
        #endregion

        #region Property
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MultiPatternMatchingParameters Parameters { get; set; }
        public VisionImage TrainImage { get; set; }
        public VisionImage LearnImage { get; set; }
        public double Tolerance { get; set; }
        public bool DuplicateChecked { get; set; }
        public int MaxInstnce { get; set; }
        public double MinScore { get; set; }
        public bool UseMaskImage { get; set; }

        public int SelectedIndex { get; set; }
        #endregion

        #region Field
        private RectangleD m_StartRectAngle;
        private RectangleD m_EndRectAngle;
        private RectangleD m_RectAngle;
        //protected List<int> m_listIndexes;
        private bool m_IsChecked;
        #endregion

        #region Constructor
        public MultiPatternMatchingParameterControl(MultiPatternMatchingParameters parameters)
        {
            if (parameters != null)
            {
                this.Parameters = parameters;
            }
            if (Parameters == null)
            {
                Parameters = new MultiPatternMatchingParameters();
            }
            this.m_IsChecked = false;
            this.UseMaskImage = false;
            m_StartRectAngle = new RectangleD();
            m_EndRectAngle = new RectangleD();
            m_RectAngle = new RectangleD();
            //m_listIndexes = new List<int>();
            SelectedIndex = 0;
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.pictureBoxMultiTraimImage.BackColor = Color.White;
            this.pictureBoxMultiTraimImage.ImageChanged += ChangeTrainImage;
            //this.pictureBoxMultiTraimImage.ResizeControl(285, 237);
            this.UpdateStyles();

            UpdateParameters();
        }

        public MultiPatternMatchingParameterControl() : this(new MultiPatternMatchingParameters())
        {

        }
        #endregion

        #region Method
        private void ChangeTrainImage()
        {
            int curIndex = this.baseListBoxTrainList.SelectedIndex;
            if (MultiImageChangeEvent != null && curIndex >= 0)
            {
                MultiImageChangeEvent(curIndex, this.pictureBoxMultiTraimImage.GetImage());
            }
        }

        public void UpdateParameters()
        {
            if (Parameters == null) return; // 방어
            this.UseMaskImage = Parameters.UseMaskImage;
            this.checkBoxDupCheck.Checked = Parameters.DuplicateChecked;
            this.TextBoxTolerance.Text = Parameters.MaxTolerance.ToString();
            this.TextMaxInstance.Text = Parameters.MaxInstance.ToString();
            this.TextMinScore.Text = Parameters.MinScore.ToString();
            this.ToggleButtonUseMaskImage.UpdateToggleStatus(Parameters.UseMaskImage);

            try
            {
                if (Parameters.TrainImages.Count != 0)
                {
                    UpdateTrainList();
                }
                if (SelectedIndex >= 0 && SelectedIndex < Parameters.TrainImages.Count)
                {
                    this.pictureBoxMultiTraimImage.SetImage(Parameters.TrainImages[SelectedIndex].GetImage());
                }
            }
            catch (Exception ex)
            {
                //Todo : Log쓰기.
            }

        }

        public void UpdateParameters(MultiPatternMatchingParameters parameter)
        {
            Parameters = parameter ?? new MultiPatternMatchingParameters();
            UpdateParameters();
        }

        private void UpdateTrainList()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = Parameters.TrainImages;
            baseListBoxTrainList.DisplayMember = "Tag";
            baseListBoxTrainList.DataSource = bs;
        }

        public void SetTrainImage(VisionImage image)
        {
            this.pictureBoxMultiTraimImage.SetImage(image.GetImage());
        }

        public void SetTrainList()
        {
            if (this.Parameters.TrainImages != null)
            {
                    UpdateTrainList();
            }
        }

        public void SwapList<T>(List<T> list, int from, int to)
        {
            if (from < 0 || to < 0)
            {
                return;
            }

            try
            {
                if (from < list.Count && to < list.Count)
                {
                    T tmp = list[from];
                    list[from] = list[to];
                    list[to] = tmp;
                }
            }
            catch (Exception ex)
            {
                // Todo: 미래의 성균이가 Log 작성할겁니다.
            }
        }

        private void OnTrainImageListChanged()
        {
            try
            {
                if (Parameters?.TrainImages != null)
                {
                    for (int i = 0; i < Parameters.TrainImages.Count; i++)
                    {
                        if (Parameters.TrainImages[i] != null && (Parameters.TrainImages[i].Tag == null || string.IsNullOrWhiteSpace(Parameters.TrainImages[i].Tag.ToString())))
                            Parameters.TrainImages[i].Tag = i.ToString();
                    }
                }
            }
            catch { }
            try { TrainImageListChanged?.Invoke(this, EventArgs.Empty); } catch { }
        }
        #endregion

        #region EventHandler
        private void checkBoxDupCheck_CheckedChanged(object sender, EventArgs e)
        {
            m_IsChecked = this.checkBoxDupCheck.Checked;
            Parameters.DuplicateChecked = m_IsChecked;
        }

        private void ChangeParametersTolerance(object sender, EventArgs e)
        {
            if (Parameters != null)
            {
                try
                {
                    Parameters.MaxTolerance = double.Parse(TextBoxTolerance.Text);
                    Parameters.MinTolerance = double.Parse(TextBoxTolerance.Text) * -1;
                }
                catch (Exception ex)
                {

                }

            }
        }

        private void ChangeParametersMaxInstnce(object sender, EventArgs e)
        {
            if (Parameters != null)
            {
                try
                {
                    Parameters.MaxInstance = int.Parse(TextMaxInstance.Text);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void ChangeParametersMinScore(object sender, EventArgs e)
        {
            if (Parameters != null)
            {
                double dValue = 0.0;
                double.TryParse(TextMinScore.Text, out dValue);
                Parameters.MinScore = dValue;
            }
        }

        private void baseToggleButton_Click(object sender, EventArgs e)
        {
            bool bOn = ToggleButtonUseMaskImage.GetButtonStatus();
            if (bOn == false)
            {
                ToggleButtonUseMaskImage.UpdateToggleStatus(true);

                this.pictureBoxMultiTraimImage.MouseDown += pictureBoxMultiTrainImage_MouseDown;
                this.pictureBoxMultiTraimImage.MouseUp += pictureBoxMultiTrainImage_MouseUp;
                this.pictureBoxMultiTraimImage.MouseMove += pictureBoxMultiTrainImage_MouseMove;
                this.pictureBoxMultiTraimImage.Paint += pictureBoxMultiTrainImage_Paint;
            }

            else if (bOn == true)
            {
                ToggleButtonUseMaskImage.UpdateToggleStatus(false);
                this.pictureBoxMultiTraimImage.MouseDown += null;
                this.pictureBoxMultiTraimImage.MouseUp += null;
                this.pictureBoxMultiTraimImage.MouseMove += null;
                this.pictureBoxMultiTraimImage.Paint += null;
            }
            Parameters.UseMaskImage = ToggleButtonUseMaskImage.GetButtonStatus();
        }

        private void pictureBoxMultiTrainImage_MouseDown(object sender, MouseEventArgs e)
        {
            m_StartRectAngle = new RectangleD(e.X, e.Y, 0, 0);
        }

        private void pictureBoxMultiTrainImage_MouseUp(object sender, MouseEventArgs e)
        {
            m_EndRectAngle = new RectangleD(e.X, e.Y, 0, 0);
        }

        private void pictureBoxMultiTrainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_RectAngle = new RectangleD(m_StartRectAngle.X, m_StartRectAngle.Y,
                    Math.Max(e.X - m_StartRectAngle.Y, m_EndRectAngle.Y - m_StartRectAngle.Y),
                    Math.Max(e.Y - m_StartRectAngle.X, m_EndRectAngle.X - m_StartRectAngle.X));
                this.Refresh();
            }
            Parameters.MaskRegion = m_RectAngle;
        }

        private void pictureBoxMultiTrainImage_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Red, 2))
            {
                Brush brush = new SolidBrush(Color.Red);
                e.Graphics.DrawRectangle(pen, m_RectAngle);
                e.Graphics.FillRectangle(brush, m_RectAngle);
            }
        }

        private void baseButtonAdd_Click(object sender, EventArgs e)
        {
            VisionImage newimage = new VisionImage();
            newimage.Tag = this.Parameters.TrainImages.Count.ToString();
            this.Parameters.TrainImages.Add(newimage);
            UpdateTrainList();
            OnTrainImageListChanged();
        }

        private void baseButtonRemove_Click(object sender, EventArgs e)
        {
            int sel = baseListBoxTrainList.SelectedIndex;
            if (sel >= 0 && sel < this.Parameters.TrainImages.Count)
            {
                this.Parameters.TrainImages.RemoveAt(sel);
                UpdateTrainList();
                OnTrainImageListChanged();
            }
        }

        private void baseButtonClear_Click(object sender, EventArgs e)
        {
            this.Parameters.TrainImages.Clear();
            UpdateTrainList();
            OnTrainImageListChanged();
        }

        private void baseButtonUp_Click(object sender, EventArgs e)
        {
            int sel = baseListBoxTrainList.SelectedIndex;

            if (sel > 0)
            {
                SwapList<VisionImage>(this.Parameters.TrainImages, sel, sel - 1);
                UpdateTrainList();
                baseListBoxTrainList.SetSelected(sel - 1, true);
                OnTrainImageListChanged();
            }
            else
            {
                return;
            }
        }

        private void baseButtonDown_Click(object sender, EventArgs e)
        {
            int sel = baseListBoxTrainList.SelectedIndex;

            if (sel < this.Parameters.TrainImages.Count - 1)
            {
                SwapList<VisionImage>(this.Parameters.TrainImages, sel, sel + 1);

                UpdateTrainList();
                baseListBoxTrainList.SetSelected(sel + 1, true);
                OnTrainImageListChanged();
            }
            else
            {
                return;
            }
        }

        private void baseListBoxTrainList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string listBox = sender as string;
            int curIndex = this.baseListBoxTrainList.SelectedIndex;

            try
            {
                if (curIndex > -1)
                {
                    this.SelectedIndex = curIndex;
                    this.pictureBoxMultiTraimImage.SetImage(Parameters.TrainImages[SelectedIndex].GetImage());
                }
                OnTrainImageListChanged();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
                
        }
        #endregion
    }
}
