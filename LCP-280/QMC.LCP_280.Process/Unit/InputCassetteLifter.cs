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
using static System.Windows.Forms.AxHost;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputCassetteLifter Unit
    ///  - Wafer Lifter (Input) 단일 축 + Teaching Positions
    ///  - Cassette / RingJut / Mapping 센서 상태 제공
    ///  - OutputStage 스타일 Region/메서드 구조
    /// </summary>
    public class InputCassetteLifter : BaseUnit
    {
        public enum AlarmKeys
        {
            eWaferProtrusionDetected = 1001,
        }
        public enum CassetteLifterState
        {
            None = 0,
            Stop = 1,
            Ready = 2,
            Work = 3,
            Complete = 4,
        }
        #region Config / Teaching
        public InputCassetteLifterConfig InputCassetteLifterConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();
        #endregion

        public InputStage InputStage { get; private set; }
        #region Axis
        private MotionAxis _waferLifterZ; // 단일 리프터 축 (Y 혹은 Z)
        public MotionAxis WaferLifterZ => _waferLifterZ;

        public UnitRunStatus Status { get; private set; }
        public CassetteLifterState State { get; private set; }
        public bool IsRequestReturnWafer { get; private set; }
        #endregion
        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eWaferProtrusionDetected;
            alarm.Title = "돌출 감지 센서가 감지 되었습니다.";
            alarm.Cause = "카세트 맵핑 하는데 돌출 감지 센서가 감지 되었습니다.\n 카세트를 점검 하고 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

        }
        #endregion
        #region ctor / Initialization
        public InputCassetteLifter(InputCassetteLifterConfig config = null) : base("InputCassetteLifterConfig")
        {
            InputCassetteLifterConfig = config ?? new InputCassetteLifterConfig();
            AddComponents();

        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
        }

        public override void AddComponents()
        {
            InputCassetteLifterConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputCassetteLifterConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in InputCassetteLifterConfig.TeachingPositions)
                TeachingPositions.Add(tp);
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
            var tp = InputCassetteLifterConfig.GetTeachingPosition(tpName);
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
            InputCassetteLifterConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = InputCassetteLifterConfig.GetTeachingPosition(positionName);
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
            var tp = InputCassetteLifterConfig.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }
        #endregion

        #region IO / Sensors
        public bool ReadInput(string name)
        {
            var hi = InputCassetteLifterConfig.HardInputs?.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }

        public bool IsCassettePresent0() => ReadInput(InputCassetteLifterConfig.IO.CASSETTE_CHECK0);
        public bool IsCassettePresent1() => ReadInput(InputCassetteLifterConfig.IO.CASSETTE_CHECK1);
        public bool IsCassettePresentAll() => IsCassettePresent0() && IsCassettePresent1();
        public bool IsAnyCassettePresent() => IsCassettePresent0() || IsCassettePresent1();
        public bool IsWaferProtrusionDetectionSensor() => !ReadInput(InputCassetteLifterConfig.IO.WAFER_PROTRUSION_DETECTION_SENSOR);

        public bool MappingSensor() => ReadInput(InputCassetteLifterConfig.IO.MAPPING_SENSOR);
        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop || this.Status == UnitRunStatus.CycleStop)
            {
                this.State = CassetteLifterState.Stop;
                return 1;
            }

            switch (State)
            {
                case CassetteLifterState.Ready:
                    ret = OnRunReady();
                    break;
                case CassetteLifterState.Work:
                    ret = OnRunWork();
                    break;
                case CassetteLifterState.Complete:
                    ret = OnRunComplete();
                    break;
                default:
                    break;
            }

            return ret;
        }

        public CassetteMaterial GetCassetteMaterial()
        {
            CassetteMaterial cd = GetMaterial() as CassetteMaterial;
            if (cd == null)
            {
                cd = new CassetteMaterial();
                SetMaterial((Material)cd);
                if (IsCassettePresentAll())
                {
                    cd.Presence = Material.MaterialPresence.Exist;
                    cd.Name = "Cassette"; // TODO: 실제 캐리어 명칭
                    cd.ArrivedTime = DateTime.Now;
                }
                else
                {
                    cd.Presence = Material.MaterialPresence.NotExist;
                }
            }
            return cd;
        }

        protected override int OnRunComplete()
        {
            return 0;
        }

        protected override int OnRunWork()
        {
            CassetteMaterial material = GetCassetteMaterial();
            if (material.Presence == Material.MaterialPresence.NotExist)
            {
                State = CassetteLifterState.Complete;
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
                                if (InputStage.IsRequestWafer)
                                {
                                    MoveToNextSlot();
                                }
                                else
                                {
                                    MaterialWafer Stagewafer = InputStage.GetWaferMaterial();
                                    if (Stagewafer == null || Stagewafer.Presence == Material.MaterialPresence.NotExist)
                                    {
                                        // Stage wafer is not exist
                                        return 0;
                                    }
                                    else
                                    {
                                        if (Stagewafer.SlotIndex != wafer.SlotIndex)
                                        {
                                            // Stage wafer slot index is different
                                            return 0;
                                        }
                                    }
                                    if (Stagewafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                                    {
                                        return 0;
                                    }
                                    else if (Stagewafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                                    {
                                        MoveToSlot(Stagewafer.SlotIndex);

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
        
        private int MoveToSlot(int slotIndex)
        {
            if(IsWaferProtrusionDetectionSensor())
            {
                Log.Write(this, "Wafer Protrusion Detected");
                AlarmPost((int)AlarmKeys.eWaferProtrusionDetected);
                return -1;
            }
            double dPos = GetTP(InputCassetteLifterConfig.TeachingPositionName.CassetteSlot_1.ToString(), AxisNames.WaferLifterZ);
            dPos += InputCassetteLifterConfig.SlotPitch * slotIndex;
            MoveAxisOnce(WaferLifterZ, dPos);
            while (!InPos(WaferLifterZ, dPos))
            {
                if (IsWaferProtrusionDetectionSensor())
                {
                    WaferLifterZ.EmgStop();
                    Log.Write(this, "Wafer Protrusion Detected");
                    AlarmPost((int)AlarmKeys.eWaferProtrusionDetected);
                    return -1;
                }
                Thread.Sleep(0);
            }
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

        private void MoveToNextSlot()
        {
            CassetteMaterial material = GetCassetteMaterial();
            if(material != null)
            {

                foreach (var v in GetCassetteMaterial().Slots)
                {
                    if(v.Presence == Material.MaterialPresence.NotExist || v.Presence == Material.MaterialPresence.Unknown)
                    {
                        continue;
                    }

                    if (v.ProcessSatate != MaterialWafer.MaterialProcessSatate.Completed)
                    {
                        MoveToSlot(v.SlotIndex);
                        return;
                    }
                }
            }
        }

        protected override int OnRunReady()
        {
            int ret = 0;
            CassetteMaterial material = GetCassetteMaterial();
            if (material.Presence == Material.MaterialPresence.Exist)
            {
                State = CassetteLifterState.Work;
                if (material.ProcessSatate == CassetteMaterial.MaterialProcessSatate.Unknown)
                {
                    ret = ScanWafer();
                    if (ret != 0)
                    {
                        Log.Write(this, "ScanWafer Failed");
                        return -1;
                    }
                }
            }
            else
            {
                State = CassetteLifterState.None;
            }
            return 0;
        }
        public bool IsEndTask(Task<int> task)
        {
            return task.IsCompleted || task.IsFaulted || task.IsCanceled;
        }
        public int ScanWafer()
        {
            int ret = 0;
            Log.Write(this, "Start ScanWafer");
            if (IsWaferProtrusionDetectionSensor())
            {
                Log.Write(this, "Wafer Protrusion Detected");
                AlarmPost((int)AlarmKeys.eWaferProtrusionDetected);
                return -1;
            }

            CassetteMaterial material = GetCassetteMaterial();
            for (int iter = 0; iter < material.Slots.Count; iter++)
            {
                material.Slots[iter] = new MaterialWafer();

            }
            MoveToScanStartPosition();
            
            Task<int> taskMoveEndPos = MoveToScanEndPositionAsync();
            while (true)
            {
                if (IsEndTask(taskMoveEndPos))
                {
                    ret = taskMoveEndPos.Result;
                    if (ret != 0)
                    {
                        Log.Write(this, "MoveToScanEndPositionAsync Failed");
                        return -1;
                    }
                    break;
                }
                if (IsWaferProtrusionDetectionSensor())
                {
                    Log.Write(this, "Wafer Protrusion Detected");
                    AlarmPost((int)AlarmKeys.eWaferProtrusionDetected);
                    return -1;
                }
                if (MappingSensor())
                {
                    double dPos = WaferLifterZ.GetPosition();
                    double dSlotPitch = InputCassetteLifterConfig.SlotPitch;
                    int slot = (int)((dPos - GetTP(InputCassetteLifterConfig.TeachingPositionName.MappingStart.ToString(), AxisNames.WaferLifterZ)) / InputCassetteLifterConfig.SlotPitch);
                    if (slot >= 0 && slot < material.Slots.Count)
                    {
                        MaterialWafer wafer = new MaterialWafer() { Presence = Material.MaterialPresence.Exist };
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

                Thread.Sleep(0);



            }
            Log.Write(this, "End ScanWafer");
            return ret;
        }

        public int MoveToScanStartPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputCassetteLifterConfig.TeachingPositionName.MappingStart, isFine);
        }

        public Task<int> MoveToScanStartPositionAsync()
        {
            return Task.Run(() =>
            {

                MoveToScanStartPosition();
                return 0;
            });
        }

        public int MoveToScanEndPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputCassetteLifterConfig.TeachingPositionName.MappingEnd, isFine);
        }

        public Task<int> MoveToScanEndPositionAsync()
        {
            return Task.Run(() => { MoveToScanEndPosition(); return 0; });
        }
        
        public int MoveToTeachingPosition(InputCassetteLifterConfig.TeachingPositionName pos, bool isCouseSpeed)
        {
            return MoveToTeachingPosition(pos.ToString());
        }

        public Task<int> ScanWaferAsync()
        {
            return Task.Run(() => ScanWafer());
        }

        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        #endregion

        #region Seq 단위 동작 함수
        public int CassetteLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int WaferMapping()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int WaferLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int WaferUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int CassetteUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }



        #endregion


    }
}