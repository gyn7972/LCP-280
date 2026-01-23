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
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// IndexUnloadAligner Unit
    ///  - 다축(Align/Index 등) Teaching Positions
    ///  - OutputStage 패턴과 유사한 구조 (Axis / Teaching / Lifecycle)
    /// </summary>
    public class IndexUnloadAligner : BaseUnit<IndexUnloadAlignerConfig>, IPatternMarkSource
    {
        public event EventHandler<PatternMarksFoundEventArgs> MarksFound;

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
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eVisionSearch;
            alarm.Title = "Vision Search Fail";
            alarm.Cause = "Vision pattern search failed.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        OutputStage OutputStage { get; set; }
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
            OutputStage = Equipment.Instance.GetUnit(UnitKeys.OutputStage) as OutputStage;
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

        #endregion

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
            this.SequencePlayers.Add(RunAlignSocketOnceReady);
            this.SequencePlayers.Add(RunAlignSocketOnce);

        }

        private string CameraKey => IndexOutCameraKey; // 통일된 키 사용
        public PatternMatchingRunner _pmRunner;
        // Pattern Matching Runner (간소화: Recipe 자동 관리)
        public PatternMatchingRunner PmRunner
        {
            get
            {
                if (_pmRunner == null)
                {
                    _pmRunner = VisionRunnerHub.GetOrCreate(IndexOutCameraKey);
                }
                return _pmRunner;
            }
        }

        public double PixelSizeXmm { get; set; } = 0.005;
        public double PixelSizeYmm { get; set; } = 0.005;
        public double ImageOriginX { get; set; } = double.NaN;
        public double ImageOriginY { get; set; } = double.NaN;
        public bool UseImageCenterAsOrigin { get; set; } = true;
        public double MaxXYOffsetMm { get; set; } = 2.0;   // XY 최대 보정 허용치 (mm)


        #region Seq

        public bool IsStatus_AlignDoneXY { get; set; }
        public bool IsAlignResult { get; set; }
        public double dLastFoundX { get; set; }
        public double dLastFoundY { get; set; }
        public double dLastFoundAngle { get; private set; }

        public int AlignXY(bool bFineSpeed = false)
        {
            int nRet = 0;

            IsStatus_AlignDoneXY = false;
            IsAlignResult = false;
            dLastFoundX = 0.0;
            dLastFoundY = 0.0;
            dLastFoundAngle = 0.0;

            MaterialDie die = this.Rotary.GetUnloadSocketMaterial();
            if (die == null || die.Presence != Material.MaterialPresence.Exist)
            {
                Log.Write(UnitName, "Align", "Skip: No die on unload socket");
                return 0;
            }

            if (Config.IsSimulation || this.Config.IsDryRun)
            {
                IsAlignResult = true;
                IsStatus_AlignDoneXY = true;
                return 0;
            }

            // 카메라/러너 가드
            if (IndexOutCamera == null)
            {
                Log.Write(UnitName, "AlignXY", "Fail: IndexOutCamera null");
                return -1;
            }

            if (IndexOutCamera.IsLiveOn)
            {
                IndexOutCamera.StopLive();
                Thread.Sleep(50);
            }

            try
            {
                // 1) Recipe 보장 (InputStage 쪽 스타일)
                //    - VisionRunnerHub.GetOrCreate(key)로 얻은 러너는 내부적으로 카메라별 재사용함
                //    - LoadRecipe 실패해도 Search는 실행될 수 있으나 실패율이 큼
                try
                {
                    PmRunner.LoadRecipe();
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, "AlignXY", "LoadRecipe error: " + ex.Message);
                }

                // 2) Grab
                VisionImage img = null;
                IndexOutCamera.SuspendedImageDisplay = true;

                int rcGrab = IndexOutCamera.GrabSync(out img);
                if (rcGrab != 0 || img == null || img.GetImage() == null)
                {
                    Log.Write(UnitName, "AlignXY", $"Fail: GrabSync rc={rcGrab}, img null");
                    return -1;
                }

                // 3) Search
                var result = PmRunner.Search(img, save: false);

                // 4) 성공시 overlay 이벤트 (InputStage와 동일 컨셉)
                if (result != null && result.Success && result.Matches != null && result.Matches.Count > 0)
                {
                    int repIdx =
                        (result.ReferenceIndex >= 0 && result.ReferenceIndex < result.Matches.Count)
                            ? result.ReferenceIndex
                            : 0;

                    RaiseMarks(img, result.Matches.ToArray(), repIdx);
                }

                // 5) 결과값 반영 (기존 로직 유지하되 안전하게)
                if (result != null && result.Success)
                {
                    IsAlignResult = true;

                    double dX = result.X;
                    double dY = result.Y;
                    double dAngle = result.R;

                    PointD pt = GetPixelToMmScale(dX, dY);
                    dLastFoundX = pt.X;
                    dLastFoundY = pt.Y;
                    dLastFoundAngle = dAngle;

                    Log.Write(UnitName, "AlignXY",
                        $"VisionX={dLastFoundX:F4}, VisionY={dLastFoundY:F4}, VisionAngle={dLastFoundAngle:F4}");
                }
                else
                {
                    IsAlignResult = false;

                    Log.Write(UnitName, "AlignXY",
                        $"Vision Search Fail. reason={(result != null ? result.FailReason : "result null")}");
                }

                // (선택) 이미지 저장은 필요할 때만 하거나 try-catch로 보호 권장
                // try { img?.Save(VisionImage.FileFilter.bmp); } catch { }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                IsStatus_AlignDoneXY = true;
                try { IndexOutCamera.SuspendedImageDisplay = false; } catch { }
            }

        }

        public int RunAlignSocketOnceReady(bool bFineSpeed = false)
        {
            int nRet = 0;
            //this.CurrentFunc = AlignSocketOnceReady;
            //Log.Write(UnitName, "Align Start");
            //if (PrepareForAlign(out var _img) != 0)
            //{
            //    Log.Write(UnitName, "Fail: Prepare for align");
            //    return -1;
            //}
            //var res = CenterSearchViaRunner();
            //if (!res.ok)
            //{
            //    if(!Config.IsSimulation)
            //    {
            //        PostAlarm((int)AlarmKeys.eVisionSearch);
            //        Log.Write(UnitName, "XY_Align", "Fail: Vision offset search");
            //        return -1;
            //    }
            //}
            //IsStatus_LastFoundDx = res.x;
            //IsStatus_LastFoundDy = res.y;

            return nRet;
        }

        public int RunAlignSocketOnce(bool bFineSpeed = false)
        {
            int nRet = 0;
                this.CurrentFunc = RunAlignSocketOnce;
             
           
            Log.Write(UnitName, "Align Start");


            int nIndex = this.GetUnloaderAlignIndexNo();
            bool bUseSocket  = this.Rotary.Config.GetUseSocket(nIndex);
            if(bUseSocket == false)
            {
                Log.Write(UnitName, "Align", "Skip: No socket at unload align position");
                return 0;
            }
            MaterialDie die = this.Rotary.GetUnloadSocketMaterial();
            if (die == null || die.Presence != Material.MaterialPresence.Exist)
            {
                Log.Write(UnitName, "Align", "Skip: No die on unload socket");
                return 0;
            }

            var socket = this.Rotary.GetSocket(nIndex);
            socket.SetState(Rotary.RotarySocketState.VAligning);

            //초기화 후 시작하자.
            IsStatus_AlignDoneXY = false;
            IsAlignResult = false;

            nRet = AlignXY();
            if (nRet != 0)
            {
                Log.Write(UnitName, "RunAlignSocketOnce", "Fail: Prepare for align");
                try
                {
                    var ctx = Equipment.Instance.SummaryContext;
                    ctx.GetCurrentSummaryOrNull()?.AddIndexVisionAsMiss();
                }
                catch (Exception ex)
                { Log.Write(ex); }

                die.UnloadAlignOffsetX = 0.0;
                die.UnloadAlignOffsetY = 0.0;
                die.UnloadAlignOffsetT = 0.0;
                //우선 실패해도 그냥 진행하자.
                //return -1;
                nRet = 0;
            }

            //pixel Data
            die.UnloadAlignOffsetX = dLastFoundX;
            die.UnloadAlignOffsetY = dLastFoundY;
            die.UnloadAlignOffsetT = dLastFoundAngle;
            
            die.State = DieProcessState.Inspected;
            SetMaterial(die);
            socket.SetState(Rotary.RotarySocketState.VAligned);

            return nRet;
        }

        public int GetUnloaderAlignIndexNo()
        {
            if (Rotary == null)
                return 0;

            int loadIndex = Rotary.GetLoadIndexNo();

            // 반시계 방향으로 2칸 이동
            int probeIndex = (loadIndex - this.Config.IndexOfOutAlign + Rotary.GetIndexCount()) % Rotary.GetIndexCount();

            return probeIndex;

        }

        // 클래스 내부에 추가
        public void ResetForNewRun(bool waitRotaryIdle = true, bool clearVisionResult = true)
        {
            // 1) 상태/플래그 초기화
            IsAlignResult = false;
            IsStatus_AlignDoneXY = false;
            dLastFoundX = 0; 
            dLastFoundY = 0;
            dLastFoundAngle = 0;
            
            // 2) 비전 리소스 정리(선택)
            if (clearVisionResult && IndexOutCamera != null)
            {
                try
                {
                    var img = IndexOutCamera.LatestImage;
                    img?.Dispose();
                    IndexOutCamera.LatestImage = null;
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] clear vision failed: {ex.Message}");
                }
            }
        }

        #endregion

        PointD GetPixelToMmScale(double dX, double dY)
        {
            double mmPerPixelX = (dX - IndexOutCamera.CameraConfig.Resolution.Width / 2) * IndexOutCamera.CameraConfig.Scale.X;
            double mmPerPixelY = (dY - IndexOutCamera.CameraConfig.Resolution.Height / 2) * IndexOutCamera.CameraConfig.Scale.Y;
            return new PointD(mmPerPixelX, mmPerPixelY);
        }


        private void RaiseMarks(VisionImage img,
                            QMC.Common.Vision.Tools.PatternMatchingResult.PatternMatchingResultValue[] matches,
                            int representativeIndex)
        {
            int trainW = 0, trainH = 0;

            if (matches == null || matches.Length == 0)
            {
                try { MarksFound?.Invoke(this, new PatternMarksFoundEventArgs { Image = img, RepresentativeIndex = representativeIndex }); }
                catch { }
                return;
            }

            if (representativeIndex < 0) 
                representativeIndex = 0;
            if (matches != null && representativeIndex >= matches.Length) 
                representativeIndex = 0;

            try
            {
                var ti = PmRunner?.Parameters?.TrainImages?
                         .FirstOrDefault(t => t?.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                if (ti != null) { trainW = ti.Header.Width; trainH = ti.Header.Height; }
            }
            catch { }

            var e = new PatternMarksFoundEventArgs
            {
                Image = img,
                RepresentativeIndex = representativeIndex
            };
            foreach (var m in matches)
            {
                e.Marks.Add(new PatternMatchInfo
                {
                    X = m.X,
                    Y = m.Y,
                    AngleDeg = m.R,
                    Score = m.Score,
                    TrainW = trainW,
                    TrainH = trainH
                });
            }

            try { MarksFound?.Invoke(this, e); } catch { }
        }

        private void TrySearchAndRaiseMarks()
        {
            try
            {
                if (PmRunner == null) return;
                var res = PmRunner.Search(false); // 내부 AcquireImage 사용
                if (!res.Success || res.Matches == null || res.Matches.Count == 0) return;

                var img = Equipment.Instance?.Cameras != null && Equipment.Instance.Cameras.TryGetValue(CameraKey, out var cam)
                            ? cam?.LatestImage
                            : null;

                int repIdx = (res.ReferenceIndex >= 0 && res.ReferenceIndex < res.Matches.Count) ? res.ReferenceIndex : 0;
                RaiseMarks(img, res.Matches.ToArray(), repIdx);
            }
            catch { }
        }

        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            IndexUnloadAlignerConfig.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            int nIndex = -1;
            switch (en)
            {
                //case IndexUnloadAlignerConfig.TeachingPositionName.AlignZ_Index1_Up: 
                //    nIndex = 0; 
                //    return MovePositionAlignZUp(nIndex, isFine);
            
                default:
                    return -1;
            }

            return 0;
        }


    }
}