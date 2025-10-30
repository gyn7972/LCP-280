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
using System.Windows;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// OutputCassetteLifter Unit
    ///  - Z 축 리프팅 Teaching Position
    ///  - Cassette / RingJut / Mapping 센서 상태 제공
    ///  - OutputStage 와 유사한 구조 (Axis / IO / Teaching / Lifecycle)
    /// </summary>
    public class OutputCassetteLifter : BaseUnit<OutputCassetteLifterConfig>
    {
        public delegate void UpdateUICassette(MaterialCassette Cassette);

        public event UpdateUICassette EventUpdateUICassette;


        public enum AlarmKeys
        {
            eBinProtrusionDetected = 5001,
            eFeederYSafetyPosition = 5002,
            eCassetteChangeRequired = 5003,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eBinProtrusionDetected;
            alarm.Title = "돌출 감지 센서가 감지 되었습니다.";
            alarm.Cause = "카세트 맵핑 하는데 돌출 감지 센서가 감지 되었습니다. 카세트를 점검 하고 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eFeederYSafetyPosition;
            alarm.Title = "Feeder Y축이 안전 위치에 있지 않습니다.";
            alarm.Cause = "Feeder Y축이 안전 위치에 있지 않습니다. Feeder Y축을 안전 위치로 이동 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eCassetteChangeRequired;
            alarm.Title = "Cassette 교체 필요";
            alarm.Cause = "Cassette에 남은 Wafer가 없습니다. Cassette를 교체 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //AlarmRegister((int)AlarmKeys.eBinProtrusionDetected,
            //                "Bin Protrusion Detected",
            //                "Bin protrusion detected by sensor during operation. Please check and clear the obstruction before retrying.",
            //                "Error");

        }
        #endregion

        public OutputFeeder OutputFeeder { get; private set; }

        public OutputStage OutputStage { get; private set; }

        #region Axis
        private MotionAxis _BinLiftZ;
        public MotionAxis BinLifterZ => _BinLiftZ;
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



        #region Barcder
        private OpticonBarcodeReader BarcoderReader;
        #endregion


        #region ctor / Initialization
        public OutputCassetteLifter(OutputCassetteLifterConfig config = null) 
            : base(new OutputCassetteLifterConfig())
        {
            
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
            BindBarcodeReader();
        }
        #endregion

        #region Barcoder Test
        private void BindBarcodeReader()
        {
            BarcoderReader = Equipment.Instance?.BarcoderReader1;

            if (BarcoderReader == null)
                Log.Write("OutputCassetteLifter", "[BindBarcodeReader] BarcoderReader null");
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

        protected override void OnBindUnit()
        {
            base.OnBindUnit();

            OutputFeeder = Equipment.Instance.GetUnit("OutputFeeder") as OutputFeeder;
            OutputStage = Equipment.Instance.GetUnit("OutputStage") as OutputStage;
        }


        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputCassetteLifter", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.BinLifterZ, ref _BinLiftZ);
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
        //    var tp = Config.GetTeachingPosition(tpName);
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
            Config.SetTeachingPosition(tp);
        }
        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = Config.GetTeachingPosition(positionName);
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
        //    var tp = Config.GetTeachingPosition(positionName);
        //    if (tp == null) return false;
        //    double z = tp.AxisPositions.TryGetValue("Bin Lifter Z Axis", out var vz) ? vz : 0;
        //    return InPos(_BinLiftZ, z);
        //}
        #endregion

        #region IO / Sensors
        public bool IsCassettePresent0()
        {
            if(Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }

            return this.ReadInput(OutputCassetteLifterConfig.IO.CASSETTE_CHECK0);
        }
        public bool IsCassettePresent1()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }

            return this.ReadInput(OutputCassetteLifterConfig.IO.CASSETTE_CHECK1);
        }
        public bool IsCassettePresentAll() => IsCassettePresent0() && IsCassettePresent1();
        public bool IsAnyCassettePresent() => IsCassettePresent0() || IsCassettePresent1();
        public bool RingJut() => this.ReadInput(OutputCassetteLifterConfig.IO.RING_JUT_CHECK);
        public bool IsBinProtrusionDetectionSensor()
        {
            bool sensorstate = RingJut();
            return !sensorstate;
        }
        public bool MappingSensor()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                // 시뮬레이션: 축 위치 기반 슬롯 단위 펄스 생성
                InitSimMappingIfNeeded();

                double pos = BinLifterZ?.GetPosition() ?? 0.0;
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

            return  this.ReadInput(OutputCassetteLifterConfig.IO.MAPPING_SENSOR);
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
                    //Log.Write(this, "Bin Protrusion Detected - Simulation");
                }
                else if (this.IsBinProtrusionDetectionSensor())
                {
                    this.BinLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
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
            return MoveTeachingPositionOnce((int)OutputCassetteLifterConfig.TeachingPositionName.MappingStart, isFine);
        }

        public int MoveToScanEndPosition(bool isFine = false)
        {
            Task<int> task = MoveToScanEndPositionAsync();
            while (IsEndTask(task))
            {
                if (this.IsBinProtrusionDetectionSensor())
                {
                    this.BinLifterZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }
        public int OnMoveToScanEndPosition(bool isFine = false)
        {
            var axisPos = GetTeachingPositionValue(OutputCassetteLifterConfig.TeachingPositionName.MappingStart, this.BinLifterZ.Name);
            axisPos -= base.Config.SlotPitch * (base.Config.SlotCount);


            bool IsAuto = false;
            if (RunMode == UnitRunMode.Auto)
                IsAuto = true;
            else
                IsAuto = false;
            int ret = this.BinLifterZ.MoveAbs(axisPos, IsAuto, isFine);

            Thread.Sleep(10);
            if (ret == 0)
            {
                while (this.BinLifterZ.IsMoveDone() == false)
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

        public double GetTeachingPositionValue(OutputCassetteLifterConfig.TeachingPositionName pos, string axis)
        {
            return GetTP(pos.ToString(), axis);
        }

        public bool IsBinReadyForLoading()
        {
            return true;// this.IsBinReadyForloading;
        }


        #region seq signals
        public bool IsBinReadyForUnloding { get; set; } = false;
        public bool RequestStageLoading { get; set; } = false;

        #endregion

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
                    foreach (var v in material.Slots)
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

            //InputCassetteLifter
            this.SequencePlayers.Add(ScanBin);
            this.SequencePlayers.Add(MoveToNextSlot);
        }

        #region Seq 단위 동작 함수
        public int ScanBin(bool bFineSpeed = false)
        {
            int nRtn = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ScanBin;
            }

            Log.Write(this, "Start ScanBin");

            if (Config.IsSimulation || Config.IsDryRun)
            {
                ResetSimMapping();
                //Log.Write(this, "Bin Protrusion Detected - Simulation");
            }
            else if (IsBinProtrusionDetectionSensor())
            {
                this.BinLifterZ.EmgStop();
                Log.Write(this, "Bin Protrusion Detected");
                PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
                return -1;
            }

            if (!OutputFeeder.IsFeederYSafetyPosition())
            {
                BinLifterZ.EmgStop();
                PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                Log.Write(this, "Feeder Y Axis is not in Safety Position");
                return -1;
            }

            if (IsCassettePresentAll() == false)
            {
                BinLifterZ.EmgStop();
                PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
                Log.Write(this, "Cassette Sensor is not Detected");
                return -1;
            }

            MaterialCassette material = GetMaterialCassette();
            int nSlotCount = base.Config.SlotCount;
            material.Slots = new List<MaterialWafer>();
            for (int iter = 0; iter < nSlotCount; iter++)
            {
                material.Slots.Add(new MaterialWafer());
            }
            material.Presence = Material.MaterialPresence.Exist;

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
                else if (IsBinProtrusionDetectionSensor())
                {
                    this.BinLifterZ.EmgStop();
                    Log.Write(this, "Bin Protrusion Detected");
                    PostAlarm((int)AlarmKeys.eBinProtrusionDetected);

                    return -1;
                }

                if (!OutputFeeder.IsFeederYSafetyPosition())
                {
                    BinLifterZ.EmgStop();
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
                    double dPos = BinLifterZ.GetPosition();
                    double dSlotPitch = base.Config.SlotPitch;
                    double dStartPos = GetTP(OutputCassetteLifterConfig.TeachingPositionName.MappingStart.ToString(), AxisNames.BinLifterZ);
                    int slot = (int)(Math.Abs(dPos - dStartPos) / base.Config.SlotPitch);
                    Log.Write(this.UnitName, "Start : " + dStartPos.ToString() + " Current :  " + dPos.ToString("3f_ Slot : ") + slot.ToString());
                    if (slot >= 0 && slot < material.Slots.Count)
                    {
                        MaterialWafer Bin = material.Slots[slot];
                        if (Bin == null ||
                            Bin.Presence == Material.MaterialPresence.Unknown ||
                            Bin.Presence == Material.MaterialPresence.NotExist)
                        {
                            Bin = new MaterialWafer() { Presence = Material.MaterialPresence.Exist };
                        }
                        Bin.ProcessSatate = MaterialWafer.MaterialProcessSatate.Ready;

                        Bin.SlotIndex = slot;
                        material.SetWafer(slot, Bin);
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

            EventUpdateUICassette?.BeginInvoke(material, null, null);

            material.ProcessSatate = Material.MaterialProcessSatate.Ready;
            Log.Write(this, "End ScanBin");
            return nRtn;
        }
        public Task<int> ScanBinAsync(bool bFineSpeed = false)
        {
            return Task.Run(() => ScanBin(bFineSpeed));
        }
        public int MoveToNextSlot(bool bFineSpeed = false)
        {
            int nRtn = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = MoveToNextSlot;
            }

            try
            {
                MaterialCassette material = GetMaterialCassette();
                if (material != null)
                {
                    foreach (var v in GetMaterialCassette().Slots)
                    {
                        if (v.Presence == Material.MaterialPresence.NotExist 
                         || v.Presence == Material.MaterialPresence.Unknown)
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
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            return nRtn;
        }
        public int MoveToSlot(int slotIndex, bool bFineSpeed = false)
        {
            int nRet = 0;
            if (!Config.IsSimulation && !Config.IsDryRun)
            {
                if (IsBinProtrusionDetectionSensor())
                {
                    this.BinLifterZ.EmgStop();
                    Log.Write(this, "Bin Protrusion Detected");
                    PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
                    return -1;
                }
            }

            if (OutputFeeder.IsInterlockOKWithCassete() == false)
            {
                BinLifterZ.EmgStop();
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
            double dPos = GetTP(OutputCassetteLifterConfig.TeachingPositionName.CassetteSlot_1.ToString(), BinLifterZ.Name);
            
            //Todo : 첫 위치가 어디냐에 따라 달라짐.
            //dPos += base.Config.SlotPitch * slotIndex;
            dPos -= base.Config.SlotPitch * slotIndex;

            MoveAxisOnce(BinLifterZ, dPos);
            while (!InPos(BinLifterZ, dPos))
            {
                if (!Config.IsSimulation && !Config.IsDryRun)
                {
                    if (IsBinProtrusionDetectionSensor())
                    {
                        BinLifterZ.EmgStop();
                        Log.Write(this, "Wafer Protrusion Detected");
                        PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
                        return -1;
                    }

                    if (!OutputFeeder.IsInterlockOKWithCassete())
                    {
                        BinLifterZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eFeederYSafetyPosition);
                        Log.Write(this, "Feeder Y Axis is not in Safety Position");
                        return -1;
                    }

                    Thread.Sleep(0);
                }
            }
            this.IsBinReadyForUnloding = true;
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

        private bool _cassetteAllCompletedAlarmRaised = false;
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
            // 카세트가 없으면 플래그 리셋
            if (IsCassettePresentAll() == false)
            {
                _cassetteAllCompletedAlarmRaised = false;
                return 0;
            }
            bool bCheck = IsCassetteAllCompleted();
            if (_cassetteAllCompletedAlarmRaised == false && bCheck)
            {
                PostAlarm((int)AlarmKeys.eCassetteChangeRequired);
                _cassetteAllCompletedAlarmRaised = true;
                return 1;
            }

            return 0;
        }
        #endregion
    }
}