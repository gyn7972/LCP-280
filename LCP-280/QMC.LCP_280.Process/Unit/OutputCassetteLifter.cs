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
    /// OutputCassetteLifter Unit
    ///  - Z 축 리프팅 Teaching Position
    ///  - Cassette / RingJut / Mapping 센서 상태 제공
    ///  - OutputStage 와 유사한 구조 (Axis / IO / Teaching / Lifecycle)
    /// </summary>
    public class OutputCassetteLifter : BaseUnit<OutputCassetteLifterConfig>
    {
        public enum AlarmKeys
        {
            eBinProtrusionDetected = 5001,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eBinProtrusionDetected;
            alarm.Title = "돌출 감지 센서가 감지 되었습니다.";
            alarm.Cause = "카세트 맵핑 하는데 돌출 감지 센서가 감지 되었습니다.\n 카세트를 점검 하고 다시 시작 하십시요.";
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
        public MotionAxis AxisBinLiftZ => _BinLiftZ;

        public bool IsBinReadyForUnloding { get; private set; }
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

        public bool IsBinReadyForLoading()
        {
            return true;// this.IsBinReadyForloading;
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
            var tp = Config.GetTeachingPosition(tpName);
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
        public bool InPosTeaching(string positionName)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return false;
            double z = tp.AxisPositions.TryGetValue("Bin Lifter Z Axis", out var vz) ? vz : 0;
            return InPos(_BinLiftZ, z);
        }
        #endregion

        #region IO / Sensors
        public bool ReadInput(string name)
        {
            var hi = Config.HardInputs?.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }

        public bool IsCassettePresent0() => ReadInput(OutputCassetteLifterConfig.IO.CASSETTE_CHECK0);
        public bool IsCassettePresent1() => ReadInput(OutputCassetteLifterConfig.IO.CASSETTE_CHECK1);
        public bool IsCassettePresentAll() => IsCassettePresent0() && IsCassettePresent1();
        public bool IsAnyCassettePresent() => IsCassettePresent0() || IsCassettePresent1();
        public bool RingJut() => !ReadInput(OutputCassetteLifterConfig.IO.RING_JUT_CHECK);
        public bool IsBinProtrusionDetectionSensor()
        {
            bool sensorstate = RingJut();
            return !sensorstate;
        }
        public bool MappingSensor() => ReadInput(OutputCassetteLifterConfig.IO.MAPPING_SENSOR);
        #endregion

        public int MoveToScanStartPosition(bool isFine = false)
        {
            Task<int> task = MoveToScanStartPositionAsync();
            while (IsEndTask(task) == false)
            {
                if (Config.IsSimulation || Config.IsDryRun)
                {
                    Log.Write(this, "Bin Protrusion Detected - Simulation");
                }
                else if (this.IsBinProtrusionDetectionSensor())
                {
                    this.AxisBinLiftZ.EmgStop();
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
                    this.AxisBinLiftZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }
        public int OnMoveToScanEndPosition(bool isFine = false)
        {
            var axisPos = GetTeachingPositionValue(OutputCassetteLifterConfig.TeachingPositionName.MappingStart, this.AxisBinLiftZ.Name);
            axisPos -= base.Config.SlotPitch * (base.Config.SlotCount);
            int ret = this.AxisBinLiftZ.MoveAbs(axisPos, isFine);

            Thread.Sleep(10);
            if (ret == 0)
            {
                while (this.AxisBinLiftZ.IsMoveDone() == false)
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


        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop || this.Status == UnitRunStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                return 1;
            }

            switch (State)
            {
                case ProcessState.Ready:
                    ret = OnRunReady();
                    break;
                case ProcessState.Work:
                    ret = OnRunWork();
                    break;
                case ProcessState.Complete:
                    ret = OnRunComplete();
                    break;
                default:
                    this.IsBinReadyForUnloding = false;
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
            base.OnStop();
            return ret;
        }

        protected override int OnRunReady()
        {
            int ret = 0;
            MaterialCassette material = GetMaterialCassette();
            if (material.Presence == Material.MaterialPresence.Exist)
            {
                if (material.ProcessSatate == MaterialCassette.MaterialProcessSatate.Unknown)
                {
                    ret = ScanBin();
                    if (ret != 0)
                    {
                        Log.Write(this, "ScanWafer Failed");
                        return -1;
                    }
                }
                State = ProcessState.Work;
            }
            else
            {
                State = ProcessState.None;
            }
            return 0;
        }

        protected override int OnRunWork()
        {
            MaterialCassette material = GetMaterialCassette();
            if (material.Presence == Material.MaterialPresence.NotExist)
            {
                State = ProcessState.Complete;
                return 0;
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
                            if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                            {
                                continue;
                            }
                            else
                            {
                                if (OutputStage.IsStatus_RequestBin)
                                {
                                    MoveToNextSlot();
                                }
                                else
                                {
                                    MaterialWafer StageBin = OutputStage.GetWaferMaterial();
                                    if (StageBin == null || StageBin.Presence == Material.MaterialPresence.NotExist)
                                    {
                                        // Stage wafer is not exist
                                        return 0;
                                    }
                                    else
                                    {
                                        if (StageBin.SlotIndex != wafer.SlotIndex)
                                        {
                                            // Stage wafer slot index is different
                                            return 0;
                                        }
                                    }
                                    if (StageBin.ProcessSatate == Material.MaterialProcessSatate.Processing)
                                    {
                                        return 0;
                                    }
                                    else if (StageBin.ProcessSatate == Material.MaterialProcessSatate.Completed)
                                    {
                                        MoveToSlot(StageBin.SlotIndex);

                                    }
                                    else
                                    {
                                        // Stage wafer is not ready
                                        return 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        protected override int OnRunComplete()
        {
            return 0;
        }
        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(ScanBin);
            this.SequencePlayers.Add(MoveToNextSlot);
            this.SequencePlayers.Add(BinLoadingBeforeStage);
            this.SequencePlayers.Add(BinLoadingFeeder);
            this.SequencePlayers.Add(BinLoadingAfterStage);
            this.SequencePlayers.Add(BinUnloadingBeforeStage);
            this.SequencePlayers.Add(BinUnloadingFeeder);
            this.SequencePlayers.Add(BinUnloadingAfterStage);

        }


        #region Seq 단위 동작 함수
        public int ScanBin(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = ScanBin;

            Log.Write(this, "Start ScanBin");

            if (Config.IsSimulation || Config.IsDryRun)
            {
                Log.Write(this, "Bin Protrusion Detected - Simulation");
            }
            else if (IsBinProtrusionDetectionSensor())
            {
                this.AxisBinLiftZ.EmgStop();
                Log.Write(this, "Bin Protrusion Detected");
                PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
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
                    Log.Write(this, "Wafer Protrusion Detected - Simulation");
                }
                else if (IsBinProtrusionDetectionSensor())
                {
                    this.AxisBinLiftZ.EmgStop();
                    Log.Write(this, "Bin Protrusion Detected");
                    PostAlarm((int)AlarmKeys.eBinProtrusionDetected);

                    return -1;
                }

                if (Config.IsSimulation || Config.IsDryRun)
                {
                    Log.Write(this, "Bin Protrusion Detected - Simulation");

                    if (bDetected == true)
                    {
                        Thread.Sleep(0);
                        continue;
                    }
                    bDetected = true;
                    double dPos = AxisBinLiftZ.GetPosition();
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
                else if (MappingSensor())
                {
                    if (bDetected == true)
                    {
                        Thread.Sleep(0);
                        continue;
                    }
                    bDetected = true;
                    double dPos = AxisBinLiftZ.GetPosition();
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
            material.ProcessSatate = Material.MaterialProcessSatate.Ready;
            Log.Write(this, "End ScanWafer");
            return nRtn;
        }
        public Task<int> ScanWaferAsync(bool bFineSpeed = false)
        {
            return Task.Run(() => ScanBin(bFineSpeed));
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
                            //this.IsWaferReadyForloading = true;
                            //this.IsWaferReadyForUnloding = false;
                        }
                    }
                }
            }
            return nRtn;
        }
        private int MoveToSlot(int slotIndex, bool bFineSpeed = false)
        {
            if (IsBinProtrusionDetectionSensor())
            {
                this.AxisBinLiftZ.EmgStop();
                Log.Write(this, "Wafer Protrusion Detected");
                PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
                return -1;
            }

            double dPos = GetTP(OutputCassetteLifterConfig.TeachingPositionName.CassetteSlot_1.ToString(), AxisNames.BinLifterZ);
            dPos += base.Config.SlotPitch * slotIndex;
            MoveAxisOnce(AxisBinLiftZ, dPos);
            while (!InPos(AxisBinLiftZ, dPos))
            {
                if (IsBinProtrusionDetectionSensor())
                {
                    AxisBinLiftZ.EmgStop();
                    Log.Write(this, "Wafer Protrusion Detected");
                    PostAlarm((int)AlarmKeys.eBinProtrusionDetected);
                    return -1;
                }
                Thread.Sleep(0);
            }
            this.IsBinReadyForUnloding = true;
            return 0;
        }
        public Task<int> MoveToSlotAsync(int slotIndex)
        {
            return Task.Run(() =>
            {
                MoveToSlot(slotIndex);
                return 0;
            });
        }

        private int BinLoadingBeforeStage(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = BinLoadingBeforeStage;
            MaterialCassette material = GetMaterialCassette();

            nRtn = OutputStage.LoadingBinPrepare();
            if (nRtn != 0)
            {
                Log.Write(this, "OutputStage LoadingBinPrepare Failed");
                return -1;
            }

            return nRtn;
        }

        public int BinLoadingFeeder(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = BinLoadingFeeder;

            nRet = OutputFeeder.BinLoading();
            if (nRet != 0)
            {
                Log.Write(this, "InputFeeder WaferLoading Failed");
                return -1;
            }

            return nRet;
        }


        private int BinLoadingAfterStage(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = BinLoadingAfterStage;
            MaterialCassette material = GetMaterialCassette();

            nRtn = OutputStage.LoadingBinComplete();
            if (nRtn != 0)
            {
                Log.Write(this, "OutputStage LoadingBinComplete Failed");
            }

            return nRtn;
        }

        private int BinUnloadingBeforeStage(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = BinUnloadingBeforeStage;
            MaterialCassette material = GetMaterialCassette();


            return nRtn;
        }

        public int BinUnloadingFeeder(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = BinUnloadingFeeder;

            //nRet = OutputFeeder.BinUnloading();
            //if (nRet != 0)
            //{
            //    Log.Write(this, "InputFeeder WaferUnloading Failed");
            //    return -1;
            //}

            return nRet;
        }

        private int BinUnloadingAfterStage(bool bFineSpeed = false)
        {
            int nRtn = 0;
            this.CurrentFunc = BinUnloadingAfterStage;
            MaterialCassette material = GetMaterialCassette();


            return nRtn;
        }
        #endregion
    }
}