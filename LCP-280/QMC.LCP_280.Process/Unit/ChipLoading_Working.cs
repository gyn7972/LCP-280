using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Sequences; // for ManualSequenceControl
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// ChipLoading Working Form - Provides Teaching Move / Axis Move / IO Control for
    /// InputStage, InputStageEjector, InputDieTransfer units.
    /// 패널을 자유롭게 이동/리사이즈 할 수 있는 layout edit 기능(chkEditLayout) 포함.
    /// </summary>
    public partial class ChipLoader_Working : Form
    {
        private const string Work_NAME = "ChipLoader";
        private Equipment Equipment => Equipment.Instance;
        
        private SeqInputStage SeqInputStage { get; set; }
        private SeqInputChipAlignVision _seqAlignVision;
        private SeqInputChipMappingVision _seqMappingVision;
        private SeqInputDieTransferChipUp _seqDiePick;
        private SeqInputDieTransferChipDown _seqDiePlace;

        private InputStage InputStageUnit { get; set; }
        private InputStageEjector InputStageEjectorUnit { get; set; }
        private InputDieTransfer InputDieTransferUnit { get; set; }

        private bool _isLayoutEditMode = false;

        // === 새 기본 생성자 (FormManager / 디자이너 반사 생성 대응) ===
        public ChipLoader_Working() : this(
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<InputStageEjector>("InputStageEjector"),
            TryGetUnit<InputDieTransfer>("InputDieTransfer"))
        {
            // 기본 생성자는 상위 생성자에서 모든 작업을 처리
        }

        // 기존 의존성 주입 생성자 유지
        public ChipLoader_Working(InputStage inputStage, InputStageEjector inputStageEjector, InputDieTransfer inputDieTransfer)
        {
            InitializeComponent();
            InputStageUnit = inputStage;
            InputStageEjectorUnit = inputStageEjector;
            InputDieTransferUnit = inputDieTransfer;
            Load += ChipLoader_Working_Load;
            FormClosing += ChipLoader_Working_FormClosing;

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

        private void ChipLoader_Working_Load(object sender, System.EventArgs e)
        {
            Text = $"{Work_NAME} Working";

            // TeachingPositionControl 일반화 버전 사용 (RegisterUnit)
            try
            {
                if (teachingPositionControl != null)
                {
                    teachingPositionControl.ClearUnits();

                    if (InputStageUnit != null)
                    {
                        teachingPositionControl.RegisterUnit(
                            "InputStage",
                            InputStageUnit,
                            () => InputStageUnit.InputStageConfig?.TeachingPositions,
                            (name, vel) => InputStageUnit.MoveToTeachingPosition(name, vel: vel),
                            tp => InputStageUnit.InputStageConfig?.SetTeachingPosition(tp),
                            autoReload: false);
                    }

                    if (InputStageEjectorUnit != null)
                    {
                        teachingPositionControl.RegisterUnit(
                            "InputStageEjector",
                            InputStageEjectorUnit,
                            () => InputStageEjectorUnit.InputStageEjectorConfig?.TeachingPositions,
                            (name, vel) => InputStageEjectorUnit.MoveToTeachingPosition(name, vel: vel),
                            tp => InputStageEjectorUnit.InputStageEjectorConfig?.SetTeachingPosition(tp),
                            autoReload: false);
                    }

                    if (InputDieTransferUnit != null)
                    {
                        teachingPositionControl.RegisterUnit(
                            "InputDieTransfer",
                            InputDieTransferUnit,
                            () => InputDieTransferUnit.InputDieTransferConfig?.TeachingPositions,
                            (name, vel) => InputDieTransferUnit.MoveToTeachingPosition(name, vel: vel),
                            tp => InputDieTransferUnit.InputDieTransferConfig?.SetTeachingPosition(tp),
                            autoReload: false);
                    }

                    teachingPositionControl.SetSaveCancelVisible(false, false);

                    teachingPositionControl.RefreshData();
                }
            }
            catch
            {
                // 구버전 컨트롤(일반화 이전) 호환: 기존 SetUnits 호출 (예외 무시)
                try { teachingPositionControl?.SetUnits(InputStageUnit, InputStageEjectorUnit, InputDieTransferUnit, true); } catch { }
            }

            // DIO Control 바인딩 (Cylinder/Vacuum I/O)
            try 
            { 
                if (InputStageUnit != null)
                {
                    // Pass delegates (lambda) 대신에 즉시 호출하지 말고
                    // Vacuum (Input Stage)
                    dioControl?.BindDIOInput(() => InputStageUnit.IsVacuum(), "Vacuum", "StageVac");
                    dioControl?.BindDIOOutput(
                        () => InputStageUnit.VacuumOn(),
                        () => InputStageUnit.VacuumOff(),
                        "Vacuum ON/OFF",
                        () => InputStageUnit.IsVacuum(),
                        "StageVacCtrl");

                    // Clamp Lift Cylinder (Extend/ Retract)
                    dioControl?.BindDIOOutput(
                        () => InputStageUnit.ClampLiftUp(),
                        () => InputStageUnit.ClampLiftDown(),
                        "ClampLift EXT/RET",
                        () => InputStageUnit.IsClamp(),
                        "StageClamp"); // state: use clamp up sensor
                    dioControl?.BindDIOInput(() => InputStageUnit.IsClamp(), "ClampLift UP Sns", "StageClampUp");
                    dioControl?.BindDIOInput(() => InputStageUnit.IsClampDown(), "ClampLift DOWN Sns", "StageClampDn");

                    // Expander Up/Down Cylinder
                    dioControl?.BindDIOOutput(
                        () => InputStageUnit.ExpanderUp(),
                        () => InputStageUnit.ExpanderDown(),
                        "Expander UP/DOWN",
                        () => InputStageUnit.IsExpanderUp(),
                        "StageExp");
                    dioControl?.BindDIOInput(() => InputStageUnit.IsExpanderUp(), "Expander UP Sns", "StageExpUp");
                    dioControl?.BindDIOInput(() => InputStageUnit.IsExpanderDown(), "Expander DOWN Sns", "StageExpDn");

                }
                else 
                    dioControl?.BindUnits(InputStageUnit, InputStageEjectorUnit, InputDieTransferUnit); 
            } 
            catch { }

            // Camera viewer binding
            try
            {
                if (_Cameraviewer != null && InputStageUnit?.StageCamera != null)
                {
                    if (_Cameraviewer.Camera != InputStageUnit.StageCamera)
                        _Cameraviewer.Camera = InputStageUnit.StageCamera;
                    try { InputStageUnit.StageCamera.StartLive(); } catch { }
                    try { _Cameraviewer.StartUpdateTask(); } catch { }
                }
            }
            catch { }

            InitSequences();
        }

        private void InitSequences()
        {
            try
            {
                if (manualSequenceControl != null)
                {
                    manualSequenceControl.ClearSequences();
                }

                // InputStage main sequence
                if (InputStageUnit != null)
                {
                    if (SeqInputStage == null)
                        SeqInputStage = new SeqInputStage(InputStageUnit);

                    manualSequenceControl?.RegisterSequence(
                        "InputStage",
                        SeqInputStage,
                        () => Enum.GetNames(typeof(SeqInputStage.Step)),
                        step => SeqInputStage.StartSingle(step),
                        idx => Enum.GetName(typeof(SeqInputStage.Step), idx),
                        autoSelect: true);
                }

                // Align Vision sequence (manual / single steps not exposed -> run whole sequence)
                if (InputStageUnit != null)
                {
                    if (_seqAlignVision == null)
                        _seqAlignVision = new SeqInputChipAlignVision(InputStageUnit);
                    manualSequenceControl?.RegisterSequence(
                        "AlignVision",
                        _seqAlignVision,
                        () => SeqInputChipAlignVision.GetStepNames(),
                        step => _seqAlignVision.StartSingle(step),
                        idx => Enum.GetName(typeof(SeqInputChipAlignVision.Step), idx));
                }

                // Mapping Vision sequence
                if (InputStageUnit != null)
                {
                    if (_seqMappingVision == null)
                        _seqMappingVision = new SeqInputChipMappingVision(InputStageUnit);
                    manualSequenceControl?.RegisterSequence(
                        "MappingVision",
                        _seqMappingVision,
                        () => SeqInputChipMappingVision.GetStepNames(),
                        step => _seqMappingVision.StartSingle(step),
                        idx => Enum.GetName(typeof(SeqInputChipMappingVision.Step), idx));
                }

                // Die Transfer Pick (Chip Up)
                if (InputDieTransferUnit != null)
                {
                    if (_seqDiePick == null)
                        _seqDiePick = new SeqInputDieTransferChipUp(InputDieTransferUnit);
                    manualSequenceControl?.RegisterSequence(
                        "DiePick",
                        _seqDiePick,
                        () => Enum.GetNames(typeof(SeqInputDieTransferChipUp.Step)),
                        step => {
                            try { return _seqDiePick.Start(); } catch { return false; }
                        },
                        idx => Enum.GetName(typeof(SeqInputDieTransferChipUp.Step), idx));
                }

                // Die Transfer Place (Chip Down)
                if (InputDieTransferUnit != null)
                {
                    if (_seqDiePlace == null)
                        _seqDiePlace = new SeqInputDieTransferChipDown(InputDieTransferUnit);
                    manualSequenceControl?.RegisterSequence(
                        "DiePlace",
                        _seqDiePlace,
                        () => Enum.GetNames(typeof(SeqInputDieTransferChipDown.Step)),
                        step => { try { return _seqDiePlace.Start(); } catch { return false; } },
                        idx => Enum.GetName(typeof(SeqInputDieTransferChipDown.Step), idx));
                }
            }
            catch { }
        }

        private void ChipLoader_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { SeqInputStage?.Stop(); } catch { }
        }
    }
}