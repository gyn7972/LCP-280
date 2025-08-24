using QMC.Common.Motions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    public partial class TestGyn : Form
    {
        private readonly MotionAxisManager _axisManager;   // 기존 매니저를 받아서 사용


        public TestGyn()
        {
            InitializeComponent();

            _axisManager = Equipment.Instance.AxisManager;
        }

        // 폼 로드시 축 이름 바인딩
        private void TestGyn_Load(object sender, EventArgs e)
        {
            try
            {
                if (_axisManager == null)
                {
                    MessageBox.Show("AxisManager가 초기화되지 않았습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 1) 유닛 목록 바인딩: GetKeys() → "Unit||Axis" 에서 Unit만 추출
                var unitNames = _axisManager
                    .GetKeys()
                    .Select(k => {
                        int idx = k.IndexOf("||", StringComparison.Ordinal);
                        return (idx >= 0) ? k.Substring(0, idx) : k;
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                comboUnit.DataSource = unitNames;

                // 2) 첫 유닛 선택 시 그 유닛의 축 목록 바인딩
                if (unitNames.Count > 0)
                {
                    BindAxesForUnit(unitNames[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"축 목록 로딩 오류:\n{ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void comboUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            var unit = comboUnit.SelectedItem as string;
            if (!string.IsNullOrWhiteSpace(unit))
            {
                BindAxesForUnit(unit);
            }
        }

        private void BindAxesForUnit(string unitName)
        {
            // Manager의 공식 API 사용
            var axisNames = _axisManager.GetAxisNames(unitName) ?? Array.Empty<string>();
            comboAxis.DataSource = axisNames.ToList();
            if (axisNames.Length > 0) comboAxis.SelectedIndex = 0;
        }

        // ★ 여기서 '축 하나 가져오기'
        private MotionAxis GetSelectedAxis()
        {
            var unit = comboUnit.SelectedItem as string;
            var axis = comboAxis.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(axis))
                return null;

            // 복합키 기반 공식 조회 API
            return _axisManager.Get(unit, axis);
        }

        // 테스트 버튼: +d 이동 → -d 복귀 (왕복)
        private async void button_TestGyn_Test_Click(object sender, EventArgs e)
        {
            if (_axisManager == null)
            {
                MessageBox.Show("AxisManager가 없습니다.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var axis = GetSelectedAxis();
            if (axis == null)
            {
                MessageBox.Show("유닛/축을 올바르게 선택하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dist = (double)numDist.Value;

            button_TestGyn_Test.Enabled = false;
            try
            {
                // ⚠️ 실제 프로젝트의 메서드명으로 치환하세요:
                // ServoOn / MoveRelative / IsBusy / Stop
                axis.Servo(true);

                axis.MoveAbs(dist);
                while (axis.InPosition(dist)) await Task.Delay(20);

                axis.MoveAbs(-dist);
                while (axis.InPosition(-dist)) await Task.Delay(20);

                MessageBox.Show($"[{axis.Name}] ({comboUnit.SelectedItem})  ±{dist:F3} mm 이동 완료.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                try { axis?.Stop(); } catch { /* ignore */ }
                MessageBox.Show($"축 테스트 중 오류:\n{ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button_TestGyn_Test.Enabled = true;
            }
        }

        private void button_TestGyn_Alarm_Click(object sender, EventArgs e)
        {
            if (_axisManager == null)
            {
                MessageBox.Show("AxisManager가 없습니다.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var axis = GetSelectedAxis();
            if (axis == null)
            {
                MessageBox.Show("유닛/축을 올바르게 선택하세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            axis.AlarmPost(MotionAxis.AlarmKey.Axis_XXXXXXXXX1_Fail); // 실제 AlarmKey로 변경
        }
    }
}
