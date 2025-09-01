using QMC.Common.Motions.CKD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class CKDMotor_Setup : Form
    {
        private Equipment equipment => Equipment.Instance;
        private CKDMotorDriver motorDriver => equipment.CKDMotor;

        public CKDMotor_Setup()
        {
            InitializeComponent();
            InitializeUI();
            motorDriver.OnMotorStateUpdated += MotorDriver_OnMotorStateUpdated;
        }

        private void CKDMotor_Setup_FormClosed(object sender, FormClosedEventArgs e)
        {
            motorDriver.OnMotorStateUpdated -= MotorDriver_OnMotorStateUpdated;
        }

        private void InitializeUI()
        {
            cbProgramNo.Items.Clear();
            for (int i = 0; i < 16; i++)
                cbProgramNo.Items.Add(i.ToString());
        }

        private void MotorDriver_OnMotorStateUpdated(object sender, EventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke((Action)UpdateInputDataUI);
            else
                UpdateInputDataUI();
        }

        private void UpdateInputDataUI()
        {
            // PDO Debugging
            lbInSig1.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputSignal1);
            lbInSig2.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputSignal2);
            lbInData1.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputData1);
            lbInData2.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputData2);
            lbInData3.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputData3);
            lbInData4.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputData4);
            lbInData5.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputData5);
            lbInCmd1.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputCommand1);
            lbInCmd2.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputCommand2);
            lbInCmd3.Text = ConvertBytesToHexString(motorDriver.TxPdoData.InputCommand3);

            // Status
            lbPosDegreeValue.Text = motorDriver.GetPositionDegree().ToString() + " deg";
            lbPosPulseValue.Text = motorDriver.GetPositionPulse().ToString() + " pulse";
            lbPosErrorPulseValue.Text = motorDriver.GetErrorPulse().ToString() + " pulse";
            lbVelocityValue.Text = motorDriver.GetVelocity().ToString() + " rpm";
            lbProgramNoValue.Text = motorDriver.GetProgramNo().ToString();
        }

        private void UpdateOutputDataUI()
        {
            // PDO Debugging
            lbOutSig1.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputSignal1);
            lbOutSig2.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputSignal2);
            lbOutData1.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputData1);
            lbOutData2.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputData2);
            lbOutData3.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputData3);
            lbOutData4.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputData4);
            lbOutData5.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputData5);
            lbOutCmd1.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputCommand1);
            lbOutCmd2.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputCommand2);
            lbOutCmd3.Text = ConvertBytesToHexString(motorDriver.RxPdoData.OutputCommand3);
        }

        private string ConvertBytesToHexString(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            byte[] reversedData = data.Reverse().ToArray();
            return BitConverter.ToString(reversedData).Replace("-", " ");
        }

        private void btnRunProgram_Click(object sender, EventArgs e)
        {
            int selectedProgramNo = cbProgramNo.SelectedIndex;
            if (!motorDriver.IsServoOn())
            {
                MessageBox.Show("서보가 ON 상태여야 합니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!motorDriver.IsReady())
            {
                MessageBox.Show("모터가 준비 상태여야 합니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!motorDriver.IsPositionComplete())
            {
                MessageBox.Show("현재 모터가 구동 중입니다.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("선택한 프로그램을 실행하시겠습니까?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                motorDriver.RunProgram(selectedProgramNo);
                UpdateOutputDataUI();
            }
        }
    }
}
