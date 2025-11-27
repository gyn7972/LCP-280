
using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;

namespace QMC.Common.Alarm
{
    public partial class Form_Alarm : Form
    {        
        public AlarmCollection Alarms { get; set; }
        AlarmInfo Alarm { get; set; }

        private Size ConfirmButton = new Size(220, 130);
        protected Size m_imagesize = new Size(30, 25);

        protected Button m_BaseButton;

        public string m_path = "";

        //  2025. 04. 27.  SCH : 아래 코드가 원래 코드인데, 현재 디렉토리의 상위 디렉토리가 루트에 가까우면 null 이 반환될 수 있다. 그래서 생성자에서 예외처리해줌. 
        //public string m_path = System.IO.Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName;               

        public Form_Alarm()
        {
            InitializeComponent();

            // 현재 디렉터리 가져오기
            var currentDirectory = System.Environment.CurrentDirectory;

            // 상위 디렉터리 확인 및 처리
            var parentDirectory = System.IO.Directory.GetParent(currentDirectory)?.Parent;

            if (parentDirectory != null)
            {
                m_path = parentDirectory.FullName; // 상위 디렉터리의 상위 디렉터리 경로
            }
            else
            {
                m_path = currentDirectory; // 기본값으로 현재 디렉터리 경로 사용
            }

            Alarms = AlarmManager.Instance.Alarms;

            this.StartPosition = FormStartPosition.CenterScreen;
            this.VisibleChanged += FormNew_Alarm_VisibleChanged;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 엔터 또는 스페이스 키 눌렀을 때 무시
            if (keyData == Keys.Enter || keyData == Keys.Space)
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FormNew_Alarm_Load(object sender, EventArgs e)
        {
            InitDataGridViewColumn();

            Alarms = AlarmManager.Instance.Alarms;
            if (Alarms != null && Alarms.Count > 0)
            {
                baseDataGridViewAlarm.DataSource = null;
                baseDataGridViewAlarm.DataSource = Alarms;
                Button baseButton = new Button();
                baseButton.Text = "Comfirm";
                baseButton.Size = ConfirmButton;
                baseButton.TextAlign = ContentAlignment.MiddleCenter;
                baseButton.FlatStyle = FlatStyle.Flat;
                baseButton.Dock = DockStyle.Fill;
                baseButton.Click += ButtonComfirm_Click;

                this.panelComfirm.Controls.Add(baseButton);
            }
        }

        public void InitDataGridViewColumn()
        {
            baseDataGridViewAlarm.Columns.Clear();
            baseDataGridViewAlarm.AutoGenerateColumns = false;
            baseDataGridViewAlarm.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.None);
            //  baseDataGridViewAlarm.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
            // 알람이 들어왔을때는 위에꺼
            baseDataGridViewAlarm.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            {
                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
                imageColumn.DataPropertyName = "StateImage";
                imageColumn.Name = "State";
                imageColumn.Width = 130;
                baseDataGridViewAlarm.Columns.Add(imageColumn);
            }

            {
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "GeneratedTime";
                column.Name = "Generated Time";
                column.Width = 250;
                baseDataGridViewAlarm.Columns.Add(column);
            }

            {
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Source";
                column.Name = "Source";
                column.Width = 250;
                baseDataGridViewAlarm.Columns.Add(column);
            }

            {
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Grade";
                column.Name = "Grade";
                column.Width = 200;
                baseDataGridViewAlarm.Columns.Add(column);
            }

            {
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Title";
                column.Name = "Title";
                column.Width = 1000;
                baseDataGridViewAlarm.Columns.Add(column);
            }
        }

        private void ButtonComfirm_Click(object sender, EventArgs e)
        {
            if (Alarms != null && Alarms.Count > 0)
            {
                if (baseDataGridViewAlarm.SelectedCells != null)
                {
                    try
                    {
                        AlarmInfo alarm = baseDataGridViewAlarm.Rows[baseDataGridViewAlarm.SelectedCells[0].RowIndex].DataBoundItem as AlarmInfo;
                        foreach (AlarmInfo alarm1 in Alarms)
                        {
                            if (alarm == alarm1)
                            {
                                Alarms.Remove(alarm1);
                                baseDataGridViewAlarm.DataSource = null;
                                baseDataGridViewAlarm.DataSource = Alarms;
                                if (Alarms.Count > 0)
                                {
                                    baseDataGridViewAlarm.Rows[0].Cells[1].Selected = true;
                                }
                                break;
                            }
                        }
                    }catch(Exception ex)
                    {
                        Log.Write(ex);
                    }
                    
                    //알람 지우기
                }
            }
        }

        private void FormNew_Alarm_VisibleChanged(object sender, EventArgs e)
        {
            Alarms = AlarmManager.Instance.Alarms;
            if (Alarms != null && Alarms.Count > 0)
            {
                baseDataGridViewAlarm.DataSource = null;
                baseDataGridViewAlarm.DataSource = Alarms;
                for (int i = 0; i < Alarms.Count; i++)
                {
                    if (Alarms[i].Grade == AlarmInfo.AlarmType.Inform.ToString())
                    {
                        //Image img = QMC.Common.Properties.Resources.AlarmInform;
                        //Bitmap imgbitmap = new Bitmap(img);
                        //img = FormMaintDigitalIO.resizeImage(imgbitmap, m_imagesize);
                        //Alarms[i].StateImage = img;
                    }
                    if (Alarms[i].Grade == AlarmInfo.AlarmType.Error.ToString())
                    {
                        //Image img = QMC.Common.Properties.Resources.AlarmError;
                        //Bitmap imgbitmap = new Bitmap(img);
                        //img = FormMaintDigitalIO.resizeImage(imgbitmap, m_imagesize);
                        //Alarms[i].StateImage = img;
                    }
                    /*
                    if (Alarms[i].Grade == Alarm.AlarmType.Warning.ToString())
                    {
                        Image img = Properties.Resources.AlarmWarning;
                        Bitmap imgbitmap = new Bitmap(img);
                        img = FormMaintDigitalIO.resizeImage(imgbitmap, m_imagesize);
                        Alarms[i].StateImage = img;
                    }
                    */
                }

                Button baseButton = new Button();
                baseButton.Text = "Comfirm";
                baseButton.Size = ConfirmButton;
                baseButton.TextAlign = ContentAlignment.MiddleCenter;
                baseButton.FlatStyle = FlatStyle.Flat;
                baseButton.Click += ButtonComfirm_Click;

                this.panelComfirm.Controls.Add(baseButton);

                //20250618-Add
                this.Refresh();
            }
        }

        private void baseDataGridViewAlarm_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //if(e.ColumnIndex <0|| e.RowIndex <0)
            //{
            //    return;
            //}
            //Alarm = baseDataGridViewAlarm.Rows[e.RowIndex].DataBoundItem as Alarm;

            //baseTextBoxAlarmTitle.Text = Alarm.Title;
            //baseTextBoxCause.Text = Alarm.Cause;
            //baseTextBoxCode.Text = Alarm.Code.ToString();
            //baseTextBoxGrade.Text = Alarm.Grade.ToString();//이넘?
            //baseTextBoxSource.Text = Alarm.Source.ToString();

        }

        private void baseDataGridViewAlarm_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }

            Alarm = baseDataGridViewAlarm.Rows[e.RowIndex].DataBoundItem as AlarmInfo;

            baseTextBoxAlarmTitle.Text = Alarm.Title;
            baseTextBoxCause.Text = Alarm.Cause;
            baseTextBoxCode.Text = Alarm.Code.ToString();
            baseTextBoxGrade.Text = Alarm.Grade.ToString();//이넘?
            baseTextBoxSource.Text = Alarm.Source.ToString();
        }

        private void baseDataGridViewAlarm_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {

            Alarm = baseDataGridViewAlarm.Rows[0].DataBoundItem as AlarmInfo;
            baseTextBoxAlarmTitle.Text = Alarm.Title;
            baseTextBoxCause.Text = Alarm.Cause;
            baseTextBoxCode.Text = Alarm.Code.ToString();
            baseTextBoxGrade.Text = Alarm.Grade.ToString();//이넘?
            baseTextBoxSource.Text = Alarm.Source.ToString();
        }

        private void baseDataGridViewAlarm_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (Alarms.Count == 0)
            {
                baseTextBoxAlarmTitle.Text = "";
                baseTextBoxCause.Text = "";
                baseTextBoxCode.Text = "";
                baseTextBoxGrade.Text = "";//이넘?
                baseTextBoxSource.Text = "";
            }
        }

        private void button_Alarm_Buzz_Off_Click(object sender, EventArgs e)
        {
            if (EquipmentLocator.Instance.m_bBuzzerOff == true)
            {
                EquipmentLocator.Instance.m_bBuzzerOff = false;
            }
            else
            {
                EquipmentLocator.Instance.m_bBuzzerOff = true;
            }
        }

        private void FormNew_Alarm_Activated(object sender, EventArgs e)
        {
            int a = 0;
            a = 0;
        }

        public void RefreshAlarmView()
        {
            if (Alarms == null || Alarms.Count == 0)
                return;

            baseDataGridViewAlarm.DataSource = null;
            baseDataGridViewAlarm.DataSource = Alarms;

            // 이미지 갱신
            for (int i = 0; i < Alarms.Count; i++)
            {
                if (Alarms[i].Grade == AlarmInfo.AlarmType.Inform.ToString())
                {
                    //Image img = QMC.Common.Properties.Resources.AlarmInform;
                    //Alarms[i].StateImage = FormMaintDigitalIO.resizeImage(new Bitmap(img), m_imagesize);
                }
                else if (Alarms[i].Grade == AlarmInfo.AlarmType.Error.ToString())
                {
                    //Image img = SLD200.Properties.Resources.AlarmError;
                    //Alarms[i].StateImage = FormMaintDigitalIO.resizeImage(new Bitmap(img), m_imagesize);
                }
            }

            // 첫 항목 자동 선택 및 상세 표시
            if (Alarms.Count > 0)
            {
                baseDataGridViewAlarm.Rows[0].Selected = true;
                Alarm = Alarms[0];

                baseTextBoxAlarmTitle.Text = Alarm.Title;
                baseTextBoxCause.Text = Alarm.Cause;
                baseTextBoxCode.Text = Alarm.Code.ToString();
                baseTextBoxGrade.Text = Alarm.Grade.ToString();
                baseTextBoxSource.Text = Alarm.Source.ToString();
            }

            this.Refresh(); // 강제 리렌더링
        }

        /// <summary>
        /// 지정된 알람을 Alarms 리스트에서 제거합니다.
        /// </summary>
        /// <param name="alarm">제거할 알람 객체</param>
        public void ClearAlarm(AlarmInfo alarm)
        {
            if (alarm == null || Alarms == null)
                return;

            try
            {
                // 완전 일치하는 항목 제거
                //Alarms.RemoveAll(a => a == alarm);
                Alarms.Remove(alarm);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

    }
}
