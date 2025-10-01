using LCP_280;
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.BarcodeReader;
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

        #region Barcder
        private OpticonBarcodeReader BarcoderReader;
        #endregion

        #region Simulation Mapping Support
        // Simulation 모드에서 MappingSensor()를 슬롯 단위로 안정적으로 에뮬레이션하기 위한 상태
        private int _simLastMappingSlot = -1;
        private HashSet<int> _simPresentSlots;          // 존재한다고 가정할 슬롯 인덱스 집합
        private bool _simSimMappingInitialized = false; // 초기화 여부
        private int _currentSlotID;
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
            BindBarcodeReader();
        }
        #endregion

        #region Barcoder Test
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
                Log.Write(this, "BarcoderReader is not initialized");
                return string.Empty;
            }

            try
            {
                string barcode;
                int result = BarcoderReader.Read(out barcode);

                Log.Write(this, $"BarcoderReader Read: {barcode}");
                return barcode;
            }
            catch (Exception ex)
            {
                Log.Write(this, $"BarcoderReader Read Error: {ex.Message}");
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

        public bool IsWaferReadyForLoading()
        {
            bool bRet = false;

            // 확인 필요.
            //MaterialCassette material = GetMaterialCassette();
            //if (material != null)
            //{
            //    if (material.ProcessSatate == Material.MaterialProcessSatate.Ready)
            //    {
            //        foreach (var v in material.Slots)
            //        {
            //            if (v.Presence == Material.MaterialPresence.Exist)
            //            {
            //                if (v.ProcessSatate == MaterialWafer.MaterialProcessSatate.Ready)
            //                {
            //                    bRet = true;
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}
            return bRet;
        }

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                ret = -1;
            }
            if (this.RunUnitStatus == UnitStatus.Running)
            {
                return 0;
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
        #endregion

        public int GetCurrectSlotID()
        {
            return _currentSlotID;
        }
        public bool IsScanCompleted()
        {
            bool bRet = false;
            MaterialCassette material = GetMaterialCassette();
            if (material != null)
            {
                if (material.ProcessSatate == Material.MaterialProcessSatate.Ready)
                {
                    foreach(var v in material.Slots)
                    {
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
                Log.Write(this, "MoveToScanStartPosition Failed");
                return nRtn;
            }
            if (this.IsStop)
            {
                Log.Write(this, "InputFeeder Stop");
                return -1;
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

                    if (v.ProcessSatate == MaterialWafer.MaterialProcessSatate.Ready)
                    {
                        nRtn = MoveToSlot(v.SlotIndex, bFineSpeed);
                        {
                            if (nRtn != 0)
                            {
                                Log.Write(this, "MoveToSlot Failed");
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
        public int MoveToSlot(int slotIndex, bool bFineSpeed = false)
        {
            int nRet = 0;
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


            if(InputFeeder.IsInterlockOKWithCassete() == false)
            {
                WaferLifterZ.EmgStop();
                PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                Log.Write(this, "Feeder Y Axis is not in Safety Position");
                return -1;
            }
            if (slotIndex < 0 || slotIndex >= base.Config.SlotCount)
            {
                Log.Write(this, $"Invalid Slot Index {slotIndex}");
                return -1;
            }
            Log.Write(this, $"MoveToSlot {slotIndex + 1}");
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

                    if (!InputFeeder.IsInterlockOKWithCassete())
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
            this._currentSlotID = slotIndex;
            return nRet;
        }
        public Task<int> MoveToSlotAsync(int slotIndex)
        {
            return Task.Run(() =>
            {
                MoveToSlot(slotIndex);
                return 0;
            });
        }

        #endregion
    }
}