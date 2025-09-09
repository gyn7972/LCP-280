using QMC.Common;
using QMC.Common.Spectrometer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// CassetteLoadingElevator Unit의 Config 폼
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class InputDieTransferUnit_Config : Form
    {
        private const string UNIT_NAME = "InputDieTransferUnit";
        private Equipment Equipment => Equipment.Instance;
        private InputDieTransfer InputDieTransferUnit { get; set; }
        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        public InputDieTransferUnit_Config()
        {
            InitializeComponent();
            this.SuspendLayout();
            _designerSize = this.Size;
            InitializeUI();
            this.ResumeLayout(true);

            Console.WriteLine($"✅ InputDieTransferUnit_Config 생성자 완료");
        }

        private void InitializeUnit()
        {
            try
            {
                if (Equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    InputDieTransferUnit = unit as InputDieTransfer;
                }

                if (InputDieTransferUnit == null)
                {
                    MessageBox.Show($"{UNIT_NAME} Unit을 찾을 수 없습니다.\nEquipment에 Unit이 등록되어 있는지 확인하세요.",
                        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Console.WriteLine($"{UNIT_NAME} Unit 연결 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unit 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_Test_Click(object sender, EventArgs e)
        {
            TestGyn testGyn = new TestGyn();
            testGyn.ShowDialog();
        }

        public void SetPanelSize(int width, int height)
        {
            // 디자이너 값과 다른 경우 경고(1회)
            if (!_sizeMismatchWarned && (width != _designerSize.Width || height != _designerSize.Height))
            {
                string formName = this.GetType().Name;
                string msg =
                    $"폼: {formName}\n" +
                    $"디자이너 크기: {_designerSize.Width} x {_designerSize.Height}\n" +
                    $"전달 크기(SetPanelSize): {width} x {height}\n\n" +
                    "크기가 일치하지 않습니다.";
#if DEBUG
                Debug.WriteLine($"[SizeMismatch] {msg}");
#endif
                try { MessageBox.Show(this, msg, "크기 불일치", MessageBoxButtons.OK, MessageBoxIcon.Warning); } catch { /* ignore */ }
                _sizeMismatchWarned = true;
            }

            try
            {
                this.SuspendLayout();
                this.Size = new Size(width, height);
                this.Invalidate();
                this.Update();
            }
            finally
            {
                this.ResumeLayout(true);
            }

            Console.WriteLine($"📐 {nameof(InputCassetteLifterUnit_Config)}.SetPanelSize → {width}x{height}");
        }

        private void btnCurrentPos_Click(object sender, EventArgs e)
        {
            try
            {
                //const string UNIT = "IndexChipProbeController";
                //var equipment = Equipment.Instance;

                if (!Equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var controller = unit as IndexChipProbeController;
                if (controller == null)
                {
                    MessageBox.Show("Unit 형식 오류", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 현재 선택된 Teaching Position 인덱스 가져오기
                int selIndex = -1;
                try
                {
                    var pi = positionItemView.GetType().GetProperty("SelectedIndex");
                    if (pi != null)
                    {
                        object val = pi.GetValue(positionItemView, null);
                        if (val is int) selIndex = (int)val;
                    }
                }
                catch { selIndex = -1; }

                var cfg = controller.IndexChipProbeControllerConfig;
                if (selIndex < 0 || cfg == null || cfg.TeachingPositions == null || selIndex >= cfg.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = cfg.TeachingPositions[selIndex];

                // 현재 위치 읽어서 AxisPositions 맵 갱신(표시용)
                var updatedPositions = new Dictionary<string, double>();
                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double fallback = kv.Value;

                    QMC.Common.Motions.MotionAxis axis = null;

                    // 1) TP 내부 바인딩 축
                    if (tp.Axes != null) tp.Axes.TryGetValue(axisKey, out axis);

                    // 2) Unit의 축 사전 키로 찾기
                    if (axis == null && controller.Axes != null)
                    {
                        QMC.Common.Motions.MotionAxis directAxis;
                        if (controller.Axes.TryGetValue(axisKey, out directAxis))
                            axis = directAxis;
                    }

                    // 3) 축 Name으로 매칭
                    if (axis == null && controller.Axes != null)
                    {
                        foreach (var pair in controller.Axes)
                        {
                            var a = pair.Value;
                            if (a != null && string.Equals(a.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = a;
                                break;
                            }
                        }
                    }

                    // 위치 읽기
                    double pos = fallback;
                    if (axis != null)
                    {
                        try { pos = axis.GetPosition(); } catch { pos = fallback; }
                    }
                    updatedPositions[axisKey] = pos;
                }

                // 에디터에 표시 갱신
                var editorProperties = new PropertyCollection();
                editorProperties.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)"));
                editorProperties.Add(new StringProperty("Description", tp.Description ?? ""));

                foreach (var ap in updatedPositions)
                    editorProperties.Add(new DoubleProperty($"{ap.Key} Position (mm)", ap.Value));

                foreach (var extra in tp.ExtraInfo)
                    editorProperties.Add(new StringProperty($"Extra: {extra.Key}", extra.Value?.ToString() ?? ""));

                positionEditorView?.SetProperties(editorProperties);
            }
            catch (Exception ex)
            {
                MessageBox.Show("현재 위치 읽기 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}