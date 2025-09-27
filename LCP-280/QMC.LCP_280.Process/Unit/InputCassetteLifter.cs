using LCP_280;
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
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
        public enum AlarmKeys
        {
            eWaferProtrusionDetected = 1001,
            eFeederYSafetyPosition,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eWaferProtrusionDetected;
            alarm.Title = "돌출 감지 센서가 감지 되었습니다.";
            alarm.Cause = "카세트 맵핑 하는데 돌출 감지 센서가 감지 되었습니다.\n 카세트를 점검 하고 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eFeederYSafetyPosition
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eFeederYSafetyPosition;
            alarm.Title = "eFeederY SafetyPosition이 아닙니다.";
            alarm.Cause = "FeederY Axis 확인바랍니다.\n FeederY Axis 점검 하고 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Config / Teaching

        #endregion

        public InputFeeder InputFeeder { get; private set; }

        public InputStage InputStage { get; private set; }

        #region Axis
        private MotionAxis _waferLifterZ; // 단일 리프터 축 (Y 혹은 Z)
        public MotionAxis WaferLifterZ => _waferLifterZ;

        public bool IsRequestReturnWafer { get; private set; }
        public bool IsWaferReadyForUnloding { get; private set; } = false;
        public bool IsWaferReadyForloading { get; private set; } = false;
        #endregion

        #region Simulation Mapping Support
        // Simulation 모드에서 MappingSensor()를 슬롯 단위로 안정적으로 에뮬레이션하기 위한 상태
        private int _simLastMappingSlot = -1;
        private HashSet<int> _simPresentSlots;          // 존재한다고 가정할 슬롯 인덱스 집합
        private bool _simSimMappingInitialized = false; // 초기화 여부
        private readonly object _simMapLock = new object();

        private void InitSimMappingIfNeeded()
        {
            if (!Config.IsSimulation && !Config.IsDryRun)
            {
                return;
            }

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

            InputFeeder = Equipment.Instance.GetUnit("InputFeeder") as InputFeeder;
            InputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
        }

        public override void AddComponents()
        {
            base.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            base.Config.InitializeDefaultTeachingPositions();
            BindAxes();
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

        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(string tpName, string axisName)
        {
            var tp = base.Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
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
        public bool InPosTeaching(string positionName)
        {
            var tp = base.Config.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }
        #endregion

        #region IO / Sensors
        public bool ReadInput(string name)
        {
            var hi = base.Config.HardInputs?.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }

        public bool IsCassettePresent0()
        {
            if(Config.IsSimulation)
            {
                return true;
            }

            return ReadInput(InputCassetteLifterConfig.IO.CASSETTE_CHECK0);
        }
        public bool IsCassettePresent1()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return ReadInput(InputCassetteLifterConfig.IO.CASSETTE_CHECK1);
        }
        public bool IsCassettePresentAll() => IsCassettePresent0() && IsCassettePresent1();
        public bool IsAnyCassettePresent() => IsCassettePresent0() || IsCassettePresent1();
        //public bool IsWaferProtrusionDetectionSensor() => !ReadInput(InputCassetteLifterConfig.IO.WAFER_PROTRUSION_DETECTION_SENSOR);
       
        public bool IsWaferProtrusionDetectionSensor()
        {
            bool sensorState = ReadInput(InputCassetteLifterConfig.IO.WAFER_PROTRUSION_DETECTION_SENSOR);
            return !sensorState;
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

            return ReadInput(InputCassetteLifterConfig.IO.MAPPING_SENSOR);
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
            }
            return cd;
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

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                    Log.Write(this, "Feeder Y Axis is not in Safety Position");
                    return -1;
                }

                Thread.Sleep(0);
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

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                    Log.Write(this, "Feeder Y Axis is not in Safety Position");
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }
        public int OnMoveToScanEndPosition(bool isFine = false)
        {
            var axisPos = GetTeachingPositionValue(InputCassetteLifterConfig.TeachingPositionName.MappingStart, this.WaferLifterZ.Name);
            axisPos -= base.Config.SlotPitch * (base.Config.SlotCount);
            int ret = this.WaferLifterZ.MoveAbs(axisPos, isFine);

            Thread.Sleep(10);
            if (ret == 0)
            {
                while (this.WaferLifterZ.IsMoveDone() == false)
                {
                    Thread.Sleep(0);
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

        public double GetTeachingPositionValue(InputCassetteLifterConfig.TeachingPositionName pos, string axis)
        {
            return GetTP(pos.ToString(), axis);
        }


        #region Seq Signals
       
        private static bool WaitIf(System.Func<IfState> get, IfState target, int timeoutMs = 10000, System.Threading.CancellationToken? ct = null, int pollMs = 5)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                if (ct.HasValue && ct.Value.IsCancellationRequested) return false;
                if (get() == target) return true;
                if (timeoutMs >= 0 && sw.ElapsedMilliseconds > timeoutMs) return false;
                System.Threading.Thread.Sleep(pollMs);
            }
        }
        #endregion


        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;

            if (this.RunUnitStatus == UnitStatus.Stopped || 
                this.RunUnitStatus == UnitStatus.Stopping || 
                this.RunUnitStatus == UnitStatus.CycleStop ||
                this.RunUnitStatus == UnitStatus.Running)
            {
                this.State = ProcessState.Stop;
                return 1;
            }

            switch (State)
            {
                case ProcessState.Ready:
                    
                    break;
                case ProcessState.Work:
                    
                    break;
                case ProcessState.Complete:
                    
                    break;
                default:
                    this.IsWaferReadyForUnloding = false;
                    this.State = ProcessState.Ready;
                    break;
            }
            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }

            return ret;
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }

        protected override int OnRunReady()
        {
            int nRtn = 0;
            var ct = this.CalcelToken != null ? (System.Threading.CancellationToken?)this.CalcelToken.Token : null;

            if (IfScan == IfState.Request)
            {
                MaterialCassette material = GetMaterialCassette();
                if (material.Presence == Material.MaterialPresence.Exist)
                {
                    if (material.ProcessSatate == MaterialCassette.MaterialProcessSatate.Unknown)
                    {
                        IfScan = IfState.Busy;
                        nRtn = ScanWafer();
                        if (nRtn != 0)
                        {
                            Log.Write(this, "ScanWafer Failed");
                            return -1;
                        }
                        IfScan = IfState.Complete;
                    }
                    State = ProcessState.Work;
                }
                else
                {
                    State = ProcessState.None;
                }
            }

            return nRtn;
        }

        protected override int OnRunWork()
        {
            int nRtn = 0;
            var ct = this.CalcelToken != null ? (System.Threading.CancellationToken?)this.CalcelToken.Token : null;

            MaterialCassette material = GetMaterialCassette();
            if (material.Presence == Material.MaterialPresence.NotExist)
            {
                State = ProcessState.Complete;
                return nRtn;
            }
            else if (material.Presence == Material.MaterialPresence.Exist)
            {
                if (material.ProcessSatate == Material.MaterialProcessSatate.Unknown)
                {
                    Log.Write(this, "Material Process State Unknown in Work State");
                    return -1;
                }
                else if (material.ProcessSatate == Material.MaterialProcessSatate.Ready)
                {
                    foreach (var wafer in material.Slots)
                    {
                        if (wafer == null || wafer.Presence == Material.MaterialPresence.NotExist)
                        {
                            continue;
                        }
                        else
                        {
                            //if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)    //Unknown
                            if (wafer.ProcessSatate != Material.MaterialProcessSatate.Ready)
                            {
                                continue;
                            }
                            else
                            {
                                if (IfMoveToNextSlot == IfState.Request)
                                {
                                    IfMoveToNextSlot = IfState.Busy;
                                    nRtn = MoveToNextSlot();
                                    if (nRtn != 0)
                                    {
                                        Log.Write(this, "MoveToNextSlot Failed");
                                        return -1;
                                    }
                                    IfMoveToNextSlot = IfState.Complete;
                                }
                                else
                                {
                                    MaterialWafer Stagewafer = InputStage.GetMaterialWafer();
                                    if (Stagewafer == null || Stagewafer.Presence == Material.MaterialPresence.NotExist)
                                    {
                                        // Stage wafer is not exist
                                        return nRtn;
                                    }
                                    else
                                    {
                                        if (Stagewafer.SlotIndex != wafer.SlotIndex)
                                        {
                                            // Stage wafer slot index is different
                                            return nRtn;
                                        }
                                    }
                                    if (Stagewafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                                    {
                                        return nRtn;
                                    }
                                    else if (Stagewafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                                    {
                                        MoveToSlot(Stagewafer.SlotIndex);
                                        Log.Write(this, "MoveToNextSlot");
                                    }
                                    else
                                    {
                                        // Stage wafer is not ready
                                        return nRtn;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return nRtn;
        }
        protected override int OnRunComplete()
        {
            return 0;
        }
        #endregion


        public bool IsWaferReadyForLoading()
        {
            return this.IsWaferReadyForloading;
        }


        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(ScanWafer);
            this.SequencePlayers.Add(MoveToNextSlot);
            this.SequencePlayers.Add(WaferLoadingBeforeStage);
            this.SequencePlayers.Add(WaferLoadingFeeder);
            this.SequencePlayers.Add(WaferLoadingAfterStage);

            this.SequencePlayers.Add(WaferAlignT);
            this.SequencePlayers.Add(WaferAlignXY);
            this.SequencePlayers.Add(WaferDieMapping);

            this.SequencePlayers.Add(WaferUnloadingBeforeStage);
            this.SequencePlayers.Add(WaferUnloadingFeeder);
            this.SequencePlayers.Add(WaferUnloadingAfterStage);
        }

        #region seq 단위 동작
        public int ScanWafer(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = ScanWafer;

            Log.Write(this, "Start ScanWafer");

            if (Config.IsSimulation || Config.IsDryRun)
            {
                // Simulation Mapping 상태 리셋
                ResetSimMapping();
                //Log.Write(this, "Wafer Protrusion Detected - Simulation");
            }
            else if (IsWaferProtrusionDetectionSensor())
            {
                WaferLifterZ.EmgStop();
                Log.Write(this, "Wafer Protrusion Detected");
                PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                return -1;
            }

            if (!InputFeeder.IsFeederYSafetyPosition())
            {
                WaferLifterZ.EmgStop();
                PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                Log.Write(this, "Feeder Y Axis is not in Safety Position");
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
                return nRtn;
            }
            Task<int> taskMoveEndPos = MoveToScanEndPositionAsync(bFineSpeed);
            bool bDetected = false;
            while (true)
            {
                if (IsEndTask(taskMoveEndPos))
                {
                    nRtn = taskMoveEndPos.Result;
                    if (nRtn != 0)
                    {
                        Log.Write(this, "MoveToScanEndPositionAsync Failed");
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
                    Log.Write(this, "Wafer Protrusion Detected");
                    PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);

                    return -1;
                }

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    WaferLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                    Log.Write(this, "Feeder Y Axis is not in Safety Position");
                    return -1;
                }

                if (MappingSensor())
                {
                    if (bDetected == true)
                    {
                        Thread.Sleep(0);
                        continue;
                    }
                    bDetected = true;
                    double dPos = WaferLifterZ.GetPosition();
                    double dSlotPitch = base.Config.SlotPitch;
                    double dStartPos = GetTP(InputCassetteLifterConfig.TeachingPositionName.MappingStart.ToString(), AxisNames.WaferLifterZ);
                    int slot = (int)(Math.Abs(dPos - dStartPos) / base.Config.SlotPitch);
                    Log.Write(this.UnitName, "Start : " + dStartPos.ToString() + " Current :  " + dPos.ToString("3f_ Slot : ") + slot.ToString());
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
                        Log.Write(this, $"Mapping Sensor Detected at Slot {slot + 1} Position {dPos:F3}");
                    }
                    else
                    {
                        Log.Write(this, $"Mapping Sensor Detected at Invalid Slot {slot + 1} Position {dPos:F3}");
                    }
                }
                else
                {
                    bDetected = false;
                }
                Thread.Sleep(0);
            }
            material.ProcessSatate = Material.MaterialProcessSatate.Ready;
            Log.Write(this, "End ScanWafer");
            return nRtn;
        }
        public Task<int> ScanWaferAsync(bool bFineSpeed = false)
        {
            return Task.Run(() => ScanWafer(bFineSpeed));
        }

        public int MoveToNextSlot(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = MoveToNextSlot;

            MaterialCassette material = GetMaterialCassette();
            if (material != null)
            {
                foreach (var v in GetMaterialCassette().Slots)
                {
                    if (v.Presence == Material.MaterialPresence.NotExist || v.Presence == Material.MaterialPresence.Unknown)
                    {
                        continue;
                    }

                    if (v.ProcessSatate != MaterialWafer.MaterialProcessSatate.Completed)
                    {
                        nRtn = MoveToSlot(v.SlotIndex, bFineSpeed);
                        {
                            if (nRtn != 0)
                            {
                                Log.Write(this, "MoveToSlot Failed");
                                return -1;
                            }
                            this.IsWaferReadyForloading = true;
                            this.IsWaferReadyForUnloding = false;

                            //한 번 움직이면 우선 나가자.
                            break;
                        }
                    }
                }
            }
            return nRtn;
        }
        private int MoveToSlot(int slotIndex, bool bFineSpeed = false)
        {
            int nRtn = 0;
            if (!Config.IsSimulation && !Config.IsDryRun)
            {
                if (IsWaferProtrusionDetectionSensor())
                {
                    WaferLifterZ.EmgStop();
                    Log.Write(this, "Wafer Protrusion Detected");
                    PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                    return -1;
                }
            }

            double dPos = GetTP(InputCassetteLifterConfig.TeachingPositionName.CassetteSlot_1.ToString(), AxisNames.WaferLifterZ);
            dPos += base.Config.SlotPitch * slotIndex;
            MoveAxisOnce(WaferLifterZ, dPos);
            while (!InPos(WaferLifterZ, dPos))
            {
                if (!Config.IsSimulation && !Config.IsDryRun)
                {
                    if (IsWaferProtrusionDetectionSensor())
                    {
                        WaferLifterZ.EmgStop();
                        Log.Write(this, "Wafer Protrusion Detected");
                        PostAlarm((int)AlarmKeys.eWaferProtrusionDetected);
                        return -1;
                    }

                    if(!InputFeeder.IsFeederYSafetyPosition())
                    {
                        WaferLifterZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                        Log.Write(this, "Feeder Y Axis is not in Safety Position");
                        return -1;
                    }

                    Thread.Sleep(0);
                }  
            }
            this.IsWaferReadyForUnloding = true;
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

        private int WaferLoadingBeforeStage(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = WaferLoadingBeforeStage;
            MaterialCassette material = GetMaterialCassette();

            nRtn = InputStage.LoadingWaferPrepare();
            if (nRtn != 0)
            {
                Log.Write(this, "InputStage LoadingWaferPrepare Failed");
                return -1;
            }
            return nRtn;
        }

        public int WaferLoadingFeeder(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = WaferLoadingFeeder;
            nRet = InputFeeder.WaferLoading();
            if (nRet != 0)
            {
                Log.Write(this, "InputFeeder WaferLoading Failed");
                return -1;
            }

            return nRet;
        }
        private int WaferLoadingAfterStage(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = WaferLoadingAfterStage;
            MaterialCassette material = GetMaterialCassette();

            nRtn = InputStage.LoadingWaferComplete();
            if (nRtn != 0)
            {
                Log.Write(this, "InputStage LoadingWaferComplete Failed");
            }

            return nRtn;
        }

        private int WaferAlignT(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = WaferAlignT;
            MaterialCassette material = GetMaterialCassette();

            nRtn = InputStage.AlignT();
            if (nRtn != 0)
            {
                Log.Write(this, "InputStage AlignT Failed");
            }

            return nRtn;
        }

        private int WaferAlignXY(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = WaferAlignXY;
            MaterialCassette material = GetMaterialCassette();

            nRtn = InputStage.AlignXY();
            if (nRtn != 0)
            {
                Log.Write(this, "InputStage AlignXY Failed");
            }

            return nRtn;
        }

        private int WaferDieMapping(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = WaferDieMapping;
            MaterialCassette material = GetMaterialCassette();

            //nRtn = InputStage.PerformChipMapping();
            //if (nRtn != 0)
            //{
            //    Log.Write(this, "InputStage AlignXY Failed");
            //}

            return nRtn;
        }

        private int WaferUnloadingBeforeStage(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = WaferUnloadingBeforeStage;
            MaterialCassette material = GetMaterialCassette();


            return nRtn;
        }

        public int WaferUnloadingFeeder(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = WaferUnloadingFeeder;

            //nRet = InputFeeder.WaferUnloading();
            //if (nRet != 0)
            //{
            //    Log.Write(this, "InputFeeder WaferUnloading Failed");
            //    return -1;
            //}

            return nRet;
        }

        private int WaferUnloadingAfterStage(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = WaferUnloadingAfterStage;
            MaterialCassette material = GetMaterialCassette();


            return nRtn;
        }

        ////Wafer Loading
        //ScanWafer_Cassette      -> Ready
        //MoveTonextSlot_Cassette -> work

        //PlateDown_Stage
        //ClampBackwordDown_Stage
        //MoveToLaod_Stage

        //MoveToReady_Feeder
        //MoveToLoadPort_Feeder
        //ClampDownGripper_Feeder
        //MoveToBarcode_Feeder
        //BarcodeRead
        //MoveToLoadStage_Feeder

        //PlateUp_Stage
        //ClampUpForword_Stage

        //UnClampGripper_Feeder
        //ClampUpfeeder
        //MoveToSafety_Feeder

        //MoveToCenter_Stage
        //AlignT_Stage
        //AlignXY_Stage
        //Mapping_Stage
        //Complete

        ////Wafer Unloading
        //WaitToSlot_Cassette (LoadingComp Slot or Empty slot)

        //MoveToUnlaod_Stage
        //ClampBackwordDown_Stage
        //PlateDown_Stage

        //ClampDown_Feeder
        //MoveToUnloadStage_Feeder
        //ClampGripper_Feeder
        //MovoToUnloadPort_Feeder
        //UnClampGripper_Feeder
        //MoveToReady_Feeder
        //Complete


        /// ///////////////////////////////////////////////////////////
        public int CassetteLoading(bool bFineSpeed = false)
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int WaferMapping(bool bFineSpeed = false)
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int CassetteUnloading(bool bFineSpeed = false)
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        
        #endregion
    }
}