using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    [FormOrder(2)]
    public partial class Operator_Main : Form
    {
        // Units
        private InputStage InputStage { get; set; }
        private IndexUnloadAligner IndexUnloadAligner { get; set; }
        private OutputStage OutputStage { get; set; }

        // State
        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부
        private bool _isLayoutEditMode;

        public Operator_Main() : this(
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<OutputStage>("OutputStage")){ 
        }

        public Operator_Main(InputStage inputStage, IndexUnloadAligner indexUnloadAligner, OutputStage outputStage)
        {
            InitializeComponent();

            InputStage = inputStage;
            IndexUnloadAligner = indexUnloadAligner;
            OutputStage = outputStage;

            Load += Vision_Manual_Load;
        }
        private static T TryGetUnit<T>(string unitName) where T : class
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq?.Units != null && eq.Units.TryGetValue(unitName, out var u))
                    return u as T;
            }
            catch { }
            return null;
        }

        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var handle = Handle; // 강제 Handle 생성
        }

        private void Vision_Manual_Load(object sender, EventArgs e) => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                BeginInvoke(new Action(StartDeferredInit)); // 무거운 초기화 지연
            }
            catch (Exception ex)
            {
                try
                {
                    Controls.Add(new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = $"Init 실패: {ex.Message}",
                        ForeColor = System.Drawing.Color.Red,
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                    });
                }
                catch { }
            }
        }

        private async void StartDeferredInit()
        {
            if (_deferredInitDone) return;
            _deferredInitDone = true;
            await Task.Delay(30); // 첫 Paint 후 실행 유도
            if (IsDisposed || Disposing) return;
            try
            {
                BindCamera();
            }
            catch { }
        }

        #region Camera
        private void BindCamera()
        {
            try
            {
                //Input Stage Camera
                if (InputWaferCamera != null && InputStage?.StageCamera != null)
                {
                    if (InputWaferCamera.Camera != InputStage.StageCamera)
                        InputWaferCamera.Camera = InputStage.StageCamera;
                    try { InputStage.StageCamera.StartLive(); } catch { }
                    try { InputWaferCamera.StartUpdateTask(); } catch { }
                }

                //Index Output Camera
                if (IndexOutputCamera != null && IndexUnloadAligner?.IndexOutCamera != null)
                {
                    if (IndexOutputCamera.Camera != IndexUnloadAligner.IndexOutCamera)
                        IndexOutputCamera.Camera = IndexUnloadAligner.IndexOutCamera;
                    try { IndexUnloadAligner.IndexOutCamera.StartLive(); } catch { }
                    try { IndexOutputCamera.StartUpdateTask(); } catch { }
                }

                //Output Wafer Camera
                if (OutputWaferCamera != null && OutputStage?.OutStageCamera != null)
                {
                    if (OutputWaferCamera.Camera != OutputStage.OutStageCamera)
                        OutputWaferCamera.Camera = OutputStage.OutStageCamera;
                    try { OutputStage.OutStageCamera.StartLive(); } catch { }
                    try { OutputWaferCamera.StartUpdateTask(); } catch { }
                }

            }
            catch { }
        }
        #endregion
    }
}
