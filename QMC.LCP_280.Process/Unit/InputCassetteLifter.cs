using LCP_280;
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.BarcodeReader;
using QMC.Common.Cameras;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputCassetteLifter Unit
    ///  - Wafer Lifter (Input) 단일 축 + Teaching Positions
    ///  - Cassette / RingJut / Mapping 센서 상태 제공
    ///  - OutputStage 스타일 Region/메서드 구조
    /// </summary>
    public class InputCassetteLifter : BaseUnit<InputCassetteLifterConfig>
    {
        public delegate void UpdateUICassette(MaterialCassette Cassette);

        public event UpdateUICassette EventUpdateUICassette;


        public enum AlarmKeys
        {
            eWaferProtrusionDetected = 1001,
            eFeederYSafetyPosition,
            eCassetteNotDetected,
            eCassetteChangeRequired,
            eMoveToSlotFailed,
            eSlotMappingMismatch
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eWaferProtrusionDetected;
            alarm.Title = "돌출 감지 센서가 감지 되었습니다.";
            alarm.Cause = "카세트 맵핑 하는데 돌출 감지 센서가 감지 되었습니다. 카세트를 점검 하고 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eFeederYSafetyPosition
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eFeederYSafetyPosition;
            alarm.Title = "eFeederY SafetyPosition이 아닙니다.";
            alarm.Cause = "FeederY Axis 확인바랍니다. FeederY Axis 점검 하고 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eCassetteNotDetected
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eCassetteNotDetected;
            alarm.Title = "eCassetteNotDetected Sensor 아닙니다.";
            alarm.Cause = "eCassetteNotDetected 확인바랍니다. eCassetteNotDetected 점검 하고 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eCassetteChangeRequired;
            alarm.Title = "Cassette 교체 필요";
            alarm.Cause = "Cassette 내 모든 웨이퍼 처리가 완료되었습니다. Cassette를 교체해 주십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eMoveToSlotFailed
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eMoveToSlotFailed;
            alarm.Title = "슬롯 이동 실패";
            alarm.Cause = "슬롯 이동 중 오류가 발생하였습니다. 장비 상태를 확인해 주십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eSlotMappingMismatch;
            alarm.Title = "입/출력 카세트 슬롯 맵 불일치";
            alarm.Cause = "Input/Output Cassette의 Wafer 존재 슬롯 패턴이 다릅니다. 두 Cassette를 점검 후 재스캔 하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms[alarm.Code] = alarm;
        }
        #endregion

        #region Config / Teaching

        #endregion

        public InputFeeder InputFeederUnit { get; set; }

        public InputStage InputStageUnit { get; set; }

        #region Axis
        private MotionAxis _waferLifterZ; // 단일 리프터 축 (Y 혹은 Z)
        public MotionAxis WaferLifterZ => _waferLifterZ;

        #region Barcder
        private OpticonBarcodeReader BarcoderReader;
        #endregion





        public bool IsRequestReturnWafer { get; set; }
        public bool IsWaferReadyForUnloding { get; set; } = false;
        public bool IsWaferReadyForloading { get; set; } = false;
        #endregion

        private int _currentSlotID;
        public int GetCurrectSlotID()
        {
            return _currentSlotID;
        }


        private bool _cassetteAllCompletedAlarmRaised = false;


        #region Simulation Mapping Support
        // Simulation 모드에서 MappingSensor()를 슬롯 단위로 안정적으로 에뮬레이션하기 위한 상태
        private int _simLastMappingSlot = -1;
        private HashSet<int> _simPresentSlots;          // 존재한다고 가정할 슬롯 인덱스 집합
        private bool _simSimMappingInitialized = false; // 초기화 여부
        
        private readonly object _simMapLock = new object();
        private void InitSimMappingIfNeeded()
        {
            lock (_simMapLock)
            {
                if (_simSimMappingInitialized)
                    return;

                _simPresentSlots = new HashSet<int>();
                // 모든 슬롯 존재로 가정 (필요 시 패턴 변경 가능)
                for (int i = 0; i < Config.SlotCount; i++)
                    _simPresentSlots.Add(i);

                _simLastMappingSlot = -1;
                _simSimMappingInitialized = true;
            }
        }
        private void ResetSimMapping()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                lock (_simMapLock)
                {
                    _simLastMappingSlot = -1;
                    _simSimMappingInitialized = false;
                }
                InitSimMappingIfNeeded();
            }
        }
        #endregion


        #region ctor / Initialization
        public InputCassetteLifter(InputCassetteLifterConfig config = null)
            : base(config ?? new InputCassetteLifterConfig())
        {
            AddComponents();
        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();

            InputFeederUnit = Equipment.Instance.GetUnit("InputFeeder") as InputFeeder;
            InputStageUnit = Equipment.Instance.GetUnit("InputStage") as InputStage;
        } 

        public override void AddComponents()
        {
            base.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            base.Config.InitializeDefaultTeachingPositions();

            BindAxes();
            BindBarcodeReader();
        }
        #endregion

        #region Barcoder
        private void BindBarcodeReader()
        {
            BarcoderReader = Equipment.Instance?.BarcoderReader2;

            if (BarcoderReader == null)
                Log.Write("InputCassetteLifter", "[BindBarcodeReader] BarcoderReader null");
        }

        public string ReadBarcoder()
        {
            if (BarcoderReader == null)
            {
                Log.Write(UnitName, "ReadBarcoder", "BarcoderReader is not initialized");
                return string.Empty;
            }

            try
            {
                string barcode = string.Empty;
                int result = 0;
                if (Config.UseBarcode)
                {
                    result = BarcoderReader.Read(out barcode);
                    if (result != 0)
                    {
                        Log.Write(UnitName, "ReadBarcoder", "Read Fail.");
                        barcode = string.Empty;
                    }
                }
                else
                {
                    // 년월일시간분 추가 (예: NotUseBarcode_20251121_1537)
                    var now = DateTime.Now;
                    barcode = "NotUseBarcode_" + now.ToString("yyyyMMddHHmm"); // yyyyMMddHHmm 도 가능
                    result = 0;
                }

                Log.Write(UnitName, "ReadBarcoder", $"BarcoderReader Read: {barcode}");
                return barcode;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "ReadBarcoder", $"BarcoderReader Read Error: {ex.Message}");
                return string.Empty;
            }
        }
        #endregion

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputCassetteLifter", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.WaferLifterZ, ref _waferLifterZ);
        }
        
        public void MoveAxisOnce(MotionAxis ax, double target, bool isFine = false)
        {
            if (ax == null) 
                return;

            bool IsAuto = false;
            if (RunMode == UnitRunMode.Auto)
                IsAuto = true;
            else
                IsAuto = false;

            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, IsAuto, isFine);
                //ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        //public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        //public double GetTP(string tpName, string axisName)
        //{
        //    var tp = base.Config.GetTeachingPosition(tpName);
        //    if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
        //    return 0.0;
        //}
        #endregion

        #region Teaching Helpers
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            base.Config.SetTeachingPosition(tp);
        }
        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = base.Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            int result = 0;
            foreach (var axisKey in tp.AxisPositions.Keys)
            {
                if (Axes.TryGetValue(axisKey, out var axis))
                {
                    double pos = tp.AxisPositions[axisKey];
                    int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
                    if (r != 0) result = r;
                }
            }
            return result;
        }
        //public bool InPosTeaching(string positionName)
        //{
        //    var tp = base.Config.GetTeachingPosition(positionName);
        //    if (tp == null) return false;
        //    foreach (var kv in tp.AxisPositions)
        //        if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
        //    return true;
        //}
        #endregion

        #region IO / Sensors
        public bool IsCassettePresent0()
        {
            if(Config.IsSimulation || Config.IsDryRun)
            {
                //return true;
                return GetMaterial() is MaterialCassette;
            }

            return this.ReadInput(InputCassetteLifterConfig.IO.CASSETTE_CHECK0);
        }
        public bool IsCassettePresent1()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                //return true;
                return GetMaterial() is MaterialCassette;
            }

            return this.ReadInput(InputCassetteLifterConfig.IO.CASSETTE_CHECK1);
        }
        public bool IsCassettePresentAll() => IsCassettePresent0() && IsCassettePresent1();
        public bool IsAnyCassettePresent() => IsCassettePresent0() || IsCassettePresent1();
        public bool IsWaferProtrusionDetectionSensor()
        {
            bool sensorState = false;
            if (Config.IsSimulation == false && Config.IsDryRun == false)
            {
                sensorState = this.ReadInput(InputCassetteLifterConfig.IO.WAFER_PROTRUSION_DETECTION_SENSOR);
                return !sensorState;
            }
            else
            {
                return sensorState;
            }
        }
        public bool MappingSensor()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                // 시뮬레이션: 축 위치 기반 슬롯 단위 펄스 생성
                InitSimMappingIfNeeded();

                double pos = WaferLifterZ?.GetPosition() ?? 0.0;
                double start = GetTP(InputCassetteLifterConfig.TeachingPositionName.MappingStart.ToString(), AxisNames.WaferLifterZ);
                double traveled = Math.Abs(pos - start);
                if (Config.SlotPitch <= 0)
                    return false;

                int slot = (int)(traveled / Config.SlotPitch);
                if (slot < 0 || slot >= Config.SlotCount)
                    return false;

                bool emit = false;
                lock (_simMapLock)
                {
                    if (_simPresentSlots != null &&
                        _simPresentSlots.Contains(slot) &&
                        slot != _simLastMappingSlot)
                    {
                        // 새 슬롯 진입 → 한 번 true
                        _simLastMappingSlot = slot;
                        emit = true;
                    }
                }
                return emit;
            }

            return this.ReadInput(InputCassetteLifterConfig.IO.MAPPING_SENSOR);
        }
        #endregion

        public MaterialCassette GetMaterialCassette()
        {
            MaterialCassette cd = GetMaterial() as MaterialCassette;
            if (cd == null)
            {
                cd = new MaterialCassette();
                SetMaterial((Material)cd);
            }
            if (IsCassettePresentAll())
            {
                cd.Presence = Material.MaterialPresence.Exist;
                cd.Name = "Cassette"; // TODO: 실제 캐리어 명칭
                cd.ArrivedTime = DateTime.Now;
            }
            else
            {
                cd.Presence = Material.MaterialPresence.NotExist;
                cd.ProcessSatate = Material.MaterialProcessSatate.Unknown;
                _cassetteAllCompletedAlarmRaised = false; // ← Cassette 제거 시 리셋
            }
            return cd;
        }

        #region Move Func.
        public double GetTeachingPositionValue(InputCassetteLifterConfig.TeachingPositionName pos, string axis)
        {
            return GetTP(pos.ToString(), axis);
        }


        public int MoveToScanStartPosition(bool isFine = false)
        {
            Task<int> task = MoveToScanStartPositionAsync();
            while (IsEndTask(task) == false)
            {
                if (Config.IsSimulation || Config.IsDryRun)
                {
                    //Log.Write(this, "Wafer Protrusion Detected - Simulation");
                }
                else if (this.IsWaferProtrusionDetectionSensor())
                {
                    this.WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                    return -1;
                }

                if (!InputFeederUnit.IsPositionFeederYSafety())
                {
                    WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                    Log.Write(this, "Feeder Y Axis is not in Safety Position");
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToScanStartPositionAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMoveToScanStartPosition(isFine);
                return 0;
            });
        }
        public int OnMoveToScanStartPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputCassetteLifterConfig.TeachingPositionName.MappingStart, isFine);
        }
        
        public int MoveToScanEndPosition(bool isFine = false)
        {
            Task<int> task = MoveToScanEndPositionAsync();
            while (IsEndTask(task))
            {
                if (this.IsWaferProtrusionDetectionSensor())
                {
                    this.WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                    return -1;
                }

                if (!InputFeederUnit.IsPositionFeederYSafety())
                {
                    WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                    Log.Write(this, "Feeder Y Axis is not in Safety Position");
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public int OnMoveToScanEndPosition(bool isFine = false)
        {
            var axisPos = GetTeachingPositionValue(InputCassetteLifterConfig.TeachingPositionName.MappingStart, this.WaferLifterZ.Name);
            axisPos -= base.Config.SlotPitch * (base.Config.SlotCount);

            bool IsAuto = false;
            if (RunMode == UnitRunMode.Auto)
                IsAuto = true;
            else
                IsAuto = false;
            int ret = this.WaferLifterZ.MoveAbs(axisPos, IsAuto, isFine);
            Thread.Sleep(10);
            if (ret == 0)
            {
                while (this.WaferLifterZ.IsMoveDone() == false)
                {
                    Thread.Sleep(1);
                }
            }
            return ret;
        }
        public Task<int> MoveToScanEndPositionAsync(bool bFine = false)
        {
            return Task.Run(() =>
            {
                OnMoveToScanEndPosition(bFine);
                return 0;
            });
        }

        #endregion

        
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.WaferLifterZ)
            {
                if (this.InputFeederUnit.IsInterlockOKWithCassete() == false)
                {
                    WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                    Log.Write(this, "Feeder Y Axis is not in Safety Position");
                    return false;

                }
                if (IsWaferProtrusionDetectionSensor())
                {
                    this.WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                    return false;
                }
            }
            return bRet;
        }

        public bool IsWaferReadyForLoading()
        {
            bool bRet = false;

            var material = GetMaterialCassette();
            if (material == null) 
                return false;
            if (material.Presence != Material.MaterialPresence.Exist) 
                return false;
            if (material.Slots == null || material.Slots.Count == 0) 
                return false;

            foreach (var w in material.Slots)
            {
                if (w != null &&
                    w.Presence == Material.MaterialPresence.Exist &&
                    w.ProcessSatate == Material.MaterialProcessSatate.Ready)
                {
                    return true;
                }
            }

            //언로딩인 경우 확인
            foreach (var w in material.Slots)
            {
                if (w != null &&
                    w.Presence == Material.MaterialPresence.Exist &&
                    w.ProcessSatate == Material.MaterialProcessSatate.Completed)
                {
                    return true;
                }
            }

            return bRet;
        }

        // 모든 존재(Exist) 슬롯이 Completed 인지 검사 (적어도 1개 이상의 Exist 슬롯이 있었을 때만 true)
        public bool IsCassetteAllCompleted()
        {
            var material = GetMaterialCassette();
            if (material == null || material.Slots == null || material.Slots.Count == 0)
                return false;

            bool sawAnyExist = false;
            for (int i = 0; i < material.Slots.Count; i++)
            {
                var w = material.Slots[i];
                if (w != null && w.Presence == Material.MaterialPresence.Exist)
                {
                    sawAnyExist = true;
                    if (w.ProcessSatate != Material.MaterialProcessSatate.Completed)
                        return false;
                }
            }
            return sawAnyExist;
        }
        // 한 번만 알람 발생. 새 카세트/재스캔 시 리셋.
        public int CheckCassetteCompletedAndAlarmOnce()
        {
            int nRet = 0;
            // 카세트가 없으면 플래그 리셋
            if (IsCassettePresentAll() == false)
            {
                _cassetteAllCompletedAlarmRaised = false;
                return 0;
            }
            else
            {
                _cassetteAllCompletedAlarmRaised = false;
            }

            bool bCheck = IsCassetteAllCompleted();
            if (_cassetteAllCompletedAlarmRaised == false && bCheck)
            {
                PostAlarm((int)AlarmKeys.eCassetteChangeRequired);
                //var mb = new MessageBoxOk();
                //mb.ShowDialog("Warring", "Cassette Change!!");
                _cassetteAllCompletedAlarmRaised = true;
                return -1;
            }
            return 0;
        }

        public bool IsHaveMoreProcessWafer()
        {
            bool bRet = false;
            MaterialCassette material = GetMaterialCassette();
            if (material != null)
            {
                if (material.ProcessSatate == Material.MaterialProcessSatate.Ready)
                {
                    foreach (var v in material.Slots)
                    {
                        if (v == null)
                            continue;

                        if (v.Presence == Material.MaterialPresence.Exist)
                        {
                            if (v.ProcessSatate == MaterialWafer.MaterialProcessSatate.Ready)
                            {
                                bRet = true;
                                break;
                            }
                        }
                    }
                }
            }
            return bRet;
        }
        public bool IsSlotEmpty(int nSlot)
        {
            bool bRtn = false;
            MaterialCassette material = GetMaterialCassette();
            if (material != null)
            {
                if (nSlot >= 0 && nSlot < material.Slots.Count)
                {
                    MaterialWafer wafer = material.Slots[nSlot];
                    if (wafer == null ||
                        wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                    {
                        bRtn = true; // Empty
                    }
                    else
                    {
                        bRtn = false; // Not Empty
                    }
                }
            }
            return bRtn;

        }

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
               this.RunUnitStatus == UnitStatus.Stopping ||
               this.RunUnitStatus == UnitStatus.CycleStop ||
               this.RunUnitStatus == UnitStatus.ManualRunning)
            {
                this.State = ProcessState.Stop;
                return 0;
            }
           
            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }
            return ret;
        }
        protected override int OnStart()
        {
            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion


        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(ScanWafer);
            this.SequencePlayers.Add(MoveToNextSlot);
        }

        #region seq 단위 동작
        public int ScanWafer(bool bFineSpeed = false)
        {
            int nRtn = 0;
                this.CurrentFunc = ScanWafer;
           
            Log.Write(UnitName, "ScanWafer", "Start ScanWafer");

            if (RunMode == UnitRunMode.Auto)
            {
                if (this.IsScanCompleted())
                {
                    return 0;
                }
            }

            BeginMapping(); // 추가

            // 새 스캔 시 알람 1회 플래그 리셋
            _cassetteAllCompletedAlarmRaised = false;
            if (Config.IsSimulation || Config.IsDryRun)
            {
                // Simulation Mapping 상태 리셋
                ResetSimMapping();
                //Log.Write(this, "Wafer Protrusion Detected - Simulation");
            }
            else if (IsWaferProtrusionDetectionSensor())
            {
                WaferLifterZ.EmgStop();
                Log.Write(UnitName, "ScanWafer", "Wafer Protrusion Detected");
                PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                return -1;
            }

            if (!InputFeederUnit.IsPositionFeederYSafety())
            {
                WaferLifterZ.EmgStop();
                PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                Log.Write(UnitName, "ScanWafer", "Feeder Y Axis is not in Safety Position");
                return -1;
            }

            if (IsCassettePresentAll() == false)
            {
                WaferLifterZ.EmgStop();
                PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                Log.Write(UnitName, "ScanWafer", "Feeder Y Axis is not in Safety Position");
                return -1;
            }

            MaterialCassette material = GetMaterialCassette();
            int nSlotCount = base.Config.SlotCount;
            material.Slots = new List<MaterialWafer>();
            for (int iter = 0; iter < nSlotCount; iter++)
            {
                material.Slots.Add(new MaterialWafer());
            }
            nRtn = MoveToScanStartPosition(bFineSpeed);
            if (nRtn != 0)
            {
                WaferLifterZ.EmgStop();
                Log.Write(UnitName, "ScanWafer", "MoveToScanStartPosition Failed");
                return nRtn;
            }

            if (RunMode == UnitRunMode.Auto)
            {
                if (this.IsStop) { return 0; }
            }

            Task<int> taskMoveEndPos = MoveToScanEndPositionAsync(bFineSpeed);
            bool bDetected = false;
            while (true)
            {
                if(IsStop)
                {
                    Log.Write(UnitName, "ScanWafer", "ScanWafer Stop");
                    return 0;
                }

                if (IsEndTask(taskMoveEndPos))
                {
                    nRtn = taskMoveEndPos.Result;
                    if (nRtn != 0)
                    {
                        Log.Write(UnitName, "ScanWafer", "MoveToScanEndPositionAsync Failed");
                        return -1;
                    }
                    break;
                }

                if (Config.IsSimulation || Config.IsDryRun)
                {
                    //Log.Write(this, "Wafer Protrusion Detected - Simulation");
                }
                else if (IsWaferProtrusionDetectionSensor())
                {
                    this.WaferLifterZ.EmgStop();
                    Log.Write(UnitName, "ScanWafer", "Wafer Protrusion Detected");
                    PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);

                    return -1;
                }

                if (!InputFeederUnit.IsPositionFeederYSafety())
                {
                    WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                    Log.Write(UnitName, "ScanWafer", "Feeder Y Axis is not in Safety Position");
                    return -1;
                }

                if (MappingSensor())
                {
                    if (bDetected == true)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    bDetected = true;
                    double dPos = WaferLifterZ.GetPosition();
                    double dSlotPitch = base.Config.SlotPitch;
                    double dStartPos = GetTP(InputCassetteLifterConfig.TeachingPositionName.MappingStart.ToString(), AxisNames.WaferLifterZ);
                    int slot = (int)(Math.Abs(dPos - dStartPos) / base.Config.SlotPitch);
                    Log.Write(UnitName, "ScanWafer", "Start : " + dStartPos.ToString() + " Current :  " + dPos.ToString("3f_ Slot : ") + slot.ToString());
                    if (slot >= 0 && slot < material.Slots.Count)
                    {
                        MaterialWafer wafer = material.Slots[slot];
                        if (wafer == null ||
                            wafer.Presence == Material.MaterialPresence.Unknown ||
                            wafer.Presence == Material.MaterialPresence.NotExist)
                        {
                            wafer = new MaterialWafer() { Presence = Material.MaterialPresence.Exist };
                        }
                        wafer.ProcessSatate = MaterialWafer.MaterialProcessSatate.Ready;

                        wafer.SlotIndex = slot;
                        material.SetWafer(slot, wafer);
                        Log.Write(UnitName, "ScanWafer", $"Mapping Sensor Detected at Slot {slot + 1} Position {dPos:F3}");
                    }
                    else
                    {
                        Log.Write(UnitName, "ScanWafer", $"Mapping Sensor Detected at Invalid Slot {slot + 1} Position {dPos:F3}");
                    }
                }
                else
                {
                    bDetected = false;
                }
                Thread.Sleep(1);
            }

            EventUpdateUICassette?.BeginInvoke(material, null, null);
            material.ProcessSatate = Material.MaterialProcessSatate.Ready;

            // 기존 EnforceSlotSyncWithOutput() 제거
            nRtn = EndMapping(); // 양쪽 완료 시 교집합 처리
            if (nRtn != 0)
            {
                //내부에서 알람 발생.
                this.WaferLifterZ.EmgStop();
                Log.Write(this, "EndMapping Error");
                return -1;
            }

            Log.Write(UnitName, "ScanWafer", "End ScanWafer");
            return nRtn;
        }

        public Task<int> ScanWaferAsync(bool bFineSpeed = false)
        {
            return Task.Run(() => ScanWafer(bFineSpeed));
        }
        public bool IsScanCompleted()
        {
            bool bRet = false;
            MaterialCassette material = GetMaterialCassette();
            if (material != null)
            {
                if (material.ProcessSatate == Material.MaterialProcessSatate.Ready)
                {
                    foreach (var v in material.Slots)
                    {
                        if (v == null)
                            continue;

                        if (v.Presence == Material.MaterialPresence.Exist)
                        {
                            bRet = true;
                            break;
                        }
                    }
                }
            }
            return bRet;
        }

        public int MoveToNextSlot(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = MoveToNextSlot;
            
            try
            {
                MaterialCassette material = GetMaterialCassette();
                if (material == null || material.Slots == null) 
                    return -1;

                if (material != null)
                {
                    //foreach (var v in GetMaterialCassette().Slots)
                    foreach (var v in material.Slots)
                    {
                        if (v == null) continue;
                        if (v.Presence != Material.MaterialPresence.Exist) continue;
                        if (v.ProcessSatate != Material.MaterialProcessSatate.Ready) continue;

                        // 양쪽 모두 존재하는 슬롯만 허용
                        if (!IsSlotActiveBothSides(v.SlotIndex)) continue;

                        if (v.ProcessSatate == MaterialWafer.MaterialProcessSatate.Ready)
                        {
                            // 선택 슬롯은 반드시 객체가 존재해야 함
                            if (material.GetWafer(v.SlotIndex) == null)
                            {
                                var w = new MaterialWafer
                                {
                                    SlotIndex = v.SlotIndex,
                                    CarrierId = material.CarrierId,
                                    Presence = Material.MaterialPresence.Exist,
                                    ProcessSatate = Material.MaterialProcessSatate.Ready
                                };
                                material.SetWafer(v.SlotIndex, w);
                            }

                            nRtn = MoveToSlot(v.SlotIndex, bFineSpeed);
                            {
                                if (nRtn != 0)
                                {
                                    PostAlarm((int)AlarmKeys.eMoveToSlotFailed);
                                    Log.Write(UnitName, "MoveToNextSlot", "MoveToSlot Failed");
                                    return -1;
                                }
                                return nRtn;
                            }
                        }
                    }
                    nRtn = -1;
                }
                return nRtn;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            return nRtn;
        }
        public Task<int> MoveToSlotAsync(int slotIndex)
        {
            return Task.Run(() =>
            {
                MoveToSlot(slotIndex);
                return 0;
            });
        }
        public int MoveToSlot(int slotIndex, bool bFineSpeed = false)
        {
            int nRet = 0;
            if (!Config.IsSimulation && !Config.IsDryRun)
            {
                if (IsWaferProtrusionDetectionSensor())
                {
                    WaferLifterZ.EmgStop();
                    Log.Write(UnitName, "MoveToSlot", "Wafer Protrusion Detected");
                    PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                    return -1;
                }
            }

            if (InputFeederUnit.IsInterlockOKWithCassete() == false)
            {
                WaferLifterZ.EmgStop();
                PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                Log.Write(UnitName, "MoveToSlot", "Feeder Y Axis is not in Safety Position");
                return -1;
            }

            if (slotIndex < 0 || slotIndex >= base.Config.SlotCount)
            {
                Log.Write(UnitName, "MoveToSlot", $"Invalid Slot Index {slotIndex}");
                return -1;
            }

            Log.Write(this, $"MoveToSlot {slotIndex + 1}");
            double dPos = GetTP(InputCassetteLifterConfig.TeachingPositionName.CassetteSlot_1.ToString(), AxisNames.WaferLifterZ);

            //Todo : 시컨스 수정
            //첫번째 스타트 웨이퍼 어디인지에 따라 위로 아래로 피치 이동 필요
            //dPos += base.Config.SlotPitch * slotIndex;
            dPos -= base.Config.SlotPitch * slotIndex;

            MoveAxisOnce(WaferLifterZ, dPos);
            while (InPos(WaferLifterZ, dPos) == false)
            {
                if (!Config.IsSimulation && !Config.IsDryRun)
                {
                    if (IsWaferProtrusionDetectionSensor())
                    {
                        WaferLifterZ.EmgStop();
                        Log.Write(UnitName, "MoveToSlot", "Wafer Protrusion Detected");
                        PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                        return -1;
                    }

                    if (!InputFeederUnit.IsInterlockOKWithCassete())
                    {
                        WaferLifterZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                        Log.Write(UnitName, "MoveToSlot", "Feeder Y Axis is not in Safety Position");
                        return -1;
                    }

                    Thread.Sleep(1);
                }
            }
            this.IsWaferReadyForUnloding = true;
            this._currentSlotID = slotIndex;
            return nRet;
        }
        #endregion


        #region Init & Reset
        public void ResetForNewRun(bool resetSimulationMapping = true)
        {
            // 1) 런타임 플래그/인덱스 초기화
            IsRequestReturnWafer = false;
            IsWaferReadyForUnloding = false;
            IsWaferReadyForloading = false;
            _currentSlotID = -1;

            // 2) Cassette 완료 알람 1회 플래그 초기화
            _cassetteAllCompletedAlarmRaised = false;

            // 3) 시뮬레이션 매핑 상태 초기화
            if (resetSimulationMapping)
                ResetSimMapping();

            // 4) Material/Cassette 상태 초기화
            //    - 센서 존재 여부는 그대로 반영 (GetMaterialCassette 사용)
            //    - 슬롯은 비우고, ProcessState를 Unknown으로 돌림 → 재스캔 필요 상태
            var material = GetMaterialCassette();
            if (material != null)
            {
                material.ProcessSatate = Material.MaterialProcessSatate.Unknown;
                material.Slots = new List<MaterialWafer>(Config.SlotCount);
                for (int i = 0; i < Config.SlotCount; i++)
                    material.Slots.Add(null);

                // UI 갱신
                EventUpdateUICassette?.BeginInvoke(material, null, null);
            }
        }
        #endregion


        // === 맵핑 완료/교집합 동기화 지원 추가 ===
        public bool IsMappingCompleted { get; private set; }
        private readonly object _mappingSyncLock = new object();

        private void BeginMapping()
        {
            IsMappingCompleted = false;
        }

        private int EndMapping()
        {
            int nRet = 0;
            IsMappingCompleted = true;
            nRet = TryFinalizeMappingSync();
            return nRet;
        }

        private int TryFinalizeMappingSync()
        {
            int nRet = 0;
            var output = Equipment.Instance.GetUnit("OutputCassetteLifter") as OutputCassetteLifter;
            if (output == null) return -1;
            //if (!IsMappingCompleted || !output.IsMappingCompleted) return; // 양쪽 모두 완료 필요
            if (!IsMappingCompleted || !output.IsMappingCompleted)
            {
                //타임아웃 걸어야함.
                while (output.IsMappingCompleted == false)
                {
                    if (IsStop)
                    {
                        return 0;
                    }

                    if (output.IsMappingCompleted)
                    {
                        break;
                    }
                }
                //return;
            }
            nRet = PerformMappingIntersection(output);
            return nRet;
        }

        // 양쪽 Cassette 슬롯 존재 패턴 교집합 적용
        public bool Mismatch { get; set; }
        private int PerformMappingIntersection(OutputCassetteLifter output)
        {
            int nRet = 0;
            lock (_mappingSyncLock)
            {
                var inMat = GetMaterialCassette();
                var outMat = output.GetMaterialCassette();
                if (inMat?.Slots == null || outMat?.Slots == null) 
                    return -1;

                int n = Math.Min(inMat.Slots.Count, outMat.Slots.Count);
                bool mismatch = false;

                for (int i = 0; i < n; i++)
                {
                    bool inExist = inMat.Slots[i]?.Presence == Material.MaterialPresence.Exist;
                    bool outExist = outMat.Slots[i]?.Presence == Material.MaterialPresence.Exist;

                    if (inExist && outExist) continue; // 교집합 OK

                    if (inExist != outExist)
                    {
                        mismatch = true;
                        if (inMat.Slots[i] != null)
                        {
                            inMat.Slots[i].Presence = Material.MaterialPresence.NotExist;
                            inMat.Slots[i].ProcessSatate = Material.MaterialProcessSatate.Unknown;
                        }
                        if (outMat.Slots[i] != null)
                        {
                            outMat.Slots[i].Presence = Material.MaterialPresence.NotExist;
                            outMat.Slots[i].ProcessSatate = Material.MaterialProcessSatate.Unknown;
                        }
                    }
                }

                if (mismatch)
                {
                    Mismatch = mismatch;
                    PostAlarm((int)AlarmKeys.eSlotMappingMismatch);
                    output.PostAlarm((int)OutputCassetteLifter.AlarmKeys.eSlotMappingMismatch);
                    return -1;
                }

                // 필요 시 교집합 결과 UI 갱신
                EventUpdateUICassette?.BeginInvoke(inMat, null, null);
                output.RequestUiCassetteUpdate(true);

                Log.Write(UnitName, "[PerformMappingIntersection] Sync Done");
                Mismatch = mismatch;

                if(output.Mismatch)
                {
                    return -1;
                }
                return nRet;
            }
        }

        // 출력쪽도 맵핑 완료되어야 작업 가능
        private bool IsSlotActiveBothSides(int slotIndex)
        {
            var output = Equipment.Instance.GetUnit("OutputCassetteLifter") as OutputCassetteLifter;
            if (output == null) return false;
            if (!IsMappingCompleted || !output.IsMappingCompleted) return false;

            var inMat = GetMaterialCassette();
            var outMat = output.GetMaterialCassette();
            if (inMat?.Slots == null || outMat?.Slots == null) return false;
            if (slotIndex < 0 || slotIndex >= inMat.Slots.Count || slotIndex >= outMat.Slots.Count) return false;

            return inMat.Slots[slotIndex]?.Presence == Material.MaterialPresence.Exist
                && outMat.Slots[slotIndex]?.Presence == Material.MaterialPresence.Exist;
        }

        protected virtual void OnUpdateUICassette(MaterialCassette cassette, bool async = false)
        {
            var handler = EventUpdateUICassette;
            if (handler == null) return;

            if (!async)
            {
                foreach (UpdateUICassette d in handler.GetInvocationList())
                {
                    try { d(cassette); }
                    catch (Exception ex) { Log.Write(UnitName, $"[OnUpdateUICassette] {ex.Message}"); }
                }
            }
            else
            {
                foreach (UpdateUICassette d in handler.GetInvocationList())
                {
                    Task.Run(() =>
                    {
                        try { d(cassette); }
                        catch (Exception ex) { Log.Write(UnitName, $"[OnUpdateUICassette-Async] {ex.Message}"); }
                    });
                }
            }
        }

        public void RequestUiCassetteUpdate(bool async = false)
        {
            OnUpdateUICassette(GetMaterialCassette(), async);
        }

    }
}