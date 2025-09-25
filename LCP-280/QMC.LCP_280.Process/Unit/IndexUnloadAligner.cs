using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.Common.Vision;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// IndexUnloadAligner Unit
    ///  - 다축(Align/Index 등) Teaching Positions
    ///  - OutputStage 패턴과 유사한 구조 (Axis / Teaching / Lifecycle)
    /// </summary>
    public class IndexUnloadAligner : BaseUnit<IndexUnloadAlignerConfig>
    {
        public enum AlarmKeys
        {
            eRotaryNotSafe = 4001,
            eVisionSearch = 4002,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotaryNotSafe;
            alarm.Title = "Rotary Not Safe";
            alarm.Cause = "Rotary axis is not in safe position.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eVisionSearch;
            alarm.Title = "Vision Search Fail";
            alarm.Cause = "Vision pattern search failed.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        #endregion

        #region ctor / Initialization
        public IndexUnloadAligner(IndexUnloadAlignerConfig config = null) : base(new IndexUnloadAlignerConfig())
        {
            AddComponents();
        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
        }

        public override void AddComponents()
        {
            base.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            base.Config.InitializeDefaultTeachingPositions();
            BindCamera();
        }
        #endregion


        #region Camera Binding
        public HIKGigECamera IndexOutCamera { get; private set; }
        public string IndexOutCameraKey => "Index_Unloader";       
        private void BindCamera()
        {
            var eq = Equipment.Instance; 
            if (eq == null) 
                return;
            if (eq.Cameras != null && eq.Cameras.TryGetValue(IndexOutCameraKey, out var cam))
                IndexOutCamera = cam as HIKGigECamera;
            else
                IndexOutCamera = eq.IndexUnloaderCam; // fallback
        }
        #endregion

        #region seq signals
        public bool CompleteUnloadAligner { get; internal set; } = false;
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
                ret = 1;
            }
            else
            {
                switch (State)
                {
                    case ProcessState.Ready:
                        if (Rotary.RequestUnloaderAligner)
                        {
                            CompleteUnloadAligner = false;
                            ret = OnRunReady();
                        }
                        break;
                    case ProcessState.Work:
                        ret = OnRunWork();
                        break;
                    case ProcessState.Complete:
                        ret = OnRunComplete();
                        if (ret == 0)
                        {
                            CompleteUnloadAligner = true;
                        }
                        break;
                    default:
                        this.State = ProcessState.Ready;
                        break;
                }
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
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(AlignSocketOnceReady);
            this.SequencePlayers.Add(AlignSocketOnce);

        }

        private string CameraKey => IndexOutCameraKey; // 통일된 키 사용
        public double PixelSizeXmm { get; set; } = 0.005;
        public double PixelSizeYmm { get; set; } = 0.005;
        public double ImageOriginX { get; set; } = double.NaN;
        public double ImageOriginY { get; set; } = double.NaN;
        public bool UseImageCenterAsOrigin { get; set; } = true;
        public double MaxXYOffsetMm { get; set; } = 2.0;   // XY 최대 보정 허용치 (mm)

        public bool IsStatus_AlignDone { get; set; }
        public double IsStatus_LastFoundDx { get; set; }
        public double IsStatus_LastFoundDy { get; set; }
        
        private (bool ok, double x, double y) CenterSearchViaRunner()
        {
            var res = VisionRunnerHub.SearchCenterOffset(
                CameraKey,
                PixelSizeXmm,
                PixelSizeYmm,
                ImageOriginX,
                ImageOriginY,
                UseImageCenterAsOrigin);

            if (!res.ok)
            {
                Log.Write(UnitName, "CenterSearchViaRunner", "Fail: " + res.error);
                return (false, 0, 0);
            }
            return (true, res.dxMm, res.dyMm);
        }

        #region Seq
        private int PrepareForAlign(out VisionImage img)
        {
            int nRtn = 0;
            
            img = null;
            int grabRc;
            try
            {
                // 4) 카메라 그랩
                if (IndexOutCamera == null)
                {
                    Log.Write(UnitName, "Align", "Fail: Camera null");
                    return -1;
                }
                grabRc = IndexOutCamera.GrabSync(out img);
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "Align", "Exception: " + ex.Message);
                return -1;
            }

            // || Config.IsDryRun
            if (!Config.IsSimulation && !Config.IsDryRun)
            {
                if (grabRc != 0 || img == null || img.RawData == null)
                {
                    Log.Write(UnitName, "Align", $"Fail: Grab fail rc={grabRc}");
                    img?.Dispose();
                    img = null;
                    return -1;
                }
            }

            IndexOutCamera.LatestImage = img;
            Log.Write(UnitName, "Align", "Grab OK");
            return nRtn;
        }

        public int AlignSocketOnceReady(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = AlignSocketOnceReady;

            Log.Write(UnitName, "Align Start");

            if (PrepareForAlign(out var _img) != 0)
            {
                Log.Write(UnitName, "Fail: Prepare for align");
                return -1;
            }

            var res = CenterSearchViaRunner();
            if (!res.ok)
            {
                PostAlarm((int)AlarmKeys.eVisionSearch);
                Log.Write(UnitName, "XY_Align", "Fail: Vision offset search");
                return -1;
            }

            IsStatus_LastFoundDx = res.x;
            IsStatus_LastFoundDy = res.y;

            return nRet;
        }

        public int AlignSocketOnce(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = AlignSocketOnce;

            double dx = IsStatus_LastFoundDx;
            double dy = IsStatus_LastFoundDy;

            if (Math.Abs(dx) < 0.0001 && Math.Abs(dy) < 0.0001)
            {
                Log.Write(UnitName, "XY_Align", "Skip: offset under threshold");
                IsStatus_AlignDone = true;
                return 0;
            }
            if (Math.Abs(dx) > MaxXYOffsetMm || Math.Abs(dy) > MaxXYOffsetMm)
            {
                Log.Write(UnitName, "Align",
                    $"Fail: Over limit dx={dx:F4} dy={dy:F4} limit={MaxXYOffsetMm}");
                return -1;
            }

            // OutStage T축 , XY축 보정 적용 필.

            return nRet;
        }

        public int GetUnloadIndexNo()
        {
            if (Rotary == null)
                return 0;

            int loadIndex = Rotary.GetLoadIndexNo();

            // 반시계 방향으로 2칸 이동
            int probeIndex = (loadIndex - this.Config.IndexOfOutAlign + Rotary.GetIndexCount()) % Rotary.GetIndexCount();

            return probeIndex;

        }

        #endregion
    }
}