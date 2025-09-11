using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Work
{
    internal class ChipLoader
    {
        //waferRingStage
        //waferEjector
        //dietransfer
        //index

        //Seq
        //ChipUp_dieTransfer_PicZ
        //ChipDown_dieTransfer_PlaceZ





        //아래 pinniddleZ, pickZ => 동시 구동 함수.
        public static bool MovePinToWaiting(InputStageEjector ejector, double velScale, out string error)
        {
            error = null;
            if (ejector == null || ejector.AxisPinZ == null)
            {
                error = "Ejector/PinZ axis not ready"; return false;
            }
            try
            {
                var waitPos = ejector.InputStageEjectorConfig.GetPositionWithOffset(InputStageEjectorConfig.TeachingPositionName.EjectPinReady.ToString()).pinZ;
                double cur = ejector.AxisPinZ.GetPosition();
                double tol = (ejector.AxisPinZ.Config?.InposTolerance ?? 0.005) * 2;
                if (Math.Abs(cur - waitPos) <= tol) return true; // already there

                var cfg = ejector.AxisPinZ.Config;
                double v = (cfg?.MaxVelocity ?? 10) * (velScale > 0 && velScale <= 1 ? velScale : 1);
                double acc = cfg?.RunAcc ?? 10;
                double dec = cfg?.RunDec ?? acc;
                double jerk = cfg?.AccJerkPercent ?? 50;
                ejector.AxisPinZ.MoveAbs(waitPos, v, acc, dec, jerk);
                ejector.AxisPinZ.WaitMoveDone(-1);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message; return false;
            }
        }

        /// <summary>
        /// Apply delta (Offset - Waiting) to PinZ (relative from current - assumed at Waiting) and same delta to PickZ.
        /// If not at Waiting, caller should call MovePinToWaiting first.
        /// </summary>
        public static bool ApplyOffsetDeltaAndSyncPick(InputStageEjector ejector, InputDieTransfer transfer, double velScale, out string error)
        {
            error = null;
            if (ejector?.AxisPinZ == null || transfer?.PickZ == null)
            {
                error = "Axis not ready"; return false;
            }
            try
            {
                var wait = ejector.InputStageEjectorConfig.GetPositionWithOffset(InputStageEjectorConfig.TeachingPositionName.EjectPinReady.ToString()).pinZ;
                var off = ejector.InputStageEjectorConfig.GetPositionWithOffset(InputStageEjectorConfig.TeachingPositionName.EjectPinOffset.ToString()).pinZ;
                double delta = off - wait;

                double curPin = ejector.AxisPinZ.GetPosition();
                double curPick = transfer.PickZ.GetPosition();
                double targetPin = curPin + delta;
                double targetPick = curPick + delta;

                var pinCfg = ejector.AxisPinZ.Config; var pickCfg = transfer.PickZ.Config;
                double maxV = Math.Min(pinCfg?.MaxVelocity ?? 10, pickCfg?.MaxVelocity ?? 10);
                if (maxV <= 0) maxV = 10;
                double v = maxV * (velScale > 0 && velScale <= 1 ? velScale : 1);
                double accPin = pinCfg?.RunAcc ?? 10; double decPin = pinCfg?.RunDec ?? accPin; double jerkPin = pinCfg?.AccJerkPercent ?? 50;
                double accPick = pickCfg?.RunAcc ?? 10; double decPick = pickCfg?.RunDec ?? accPick; double jerkPick = pickCfg?.AccJerkPercent ?? 50;

                transfer.PickZ.MoveAbs(targetPick, v, accPick, decPick, jerkPick);
                ejector.AxisPinZ.MoveAbs(targetPin, v, accPin, decPin, jerkPin);
                transfer.PickZ.WaitMoveDone(-1);
                ejector.AxisPinZ.WaitMoveDone(-1);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message; return false;
            }
        }

        /// <summary>
        /// Full operation: optionally first move to Waiting then apply delta.
        /// </summary>
        public static bool ExecuteFull(InputStageEjector ejector, InputDieTransfer transfer, bool moveToWaitingFirst, double velScale, out string error)
        {
            error = null;
            if (moveToWaitingFirst)
            {
                if (!MovePinToWaiting(ejector, velScale, out error)) return false;
            }
            return ApplyOffsetDeltaAndSyncPick(ejector, transfer, velScale, out error);
        }

        /// <summary>
        /// PickUp(PickZ) 과 Niddle Pin(PinZ)을 동시에 절대 위치로 이동.
        /// </summary>
        public static bool MovePinAndPickSyncAbs(InputStageEjector ejector, InputDieTransfer transfer,
                                                 double targetPinZ, double targetPickZ, double velScale,
                                                 out string error)
        {
            error = null;
            if (ejector?.AxisPinZ == null || transfer?.PickZ == null)
            {
                error = "Axis not ready"; return false;
            }
            try
            {
                var pinCfg = ejector.AxisPinZ.Config; var pickCfg = transfer.PickZ.Config;
                double maxV = Math.Min(pinCfg?.MaxVelocity ?? 10, pickCfg?.MaxVelocity ?? 10);
                if (maxV <= 0) maxV = 10;
                double v = maxV * (velScale > 0 && velScale <= 1 ? velScale : 1);
                double accPin = pinCfg?.RunAcc ?? 10; double decPin = pinCfg?.RunDec ?? accPin; double jerkPin = pinCfg?.AccJerkPercent ?? 50;
                double accPick = pickCfg?.RunAcc ?? 10; double decPick = pickCfg?.RunDec ?? accPick; double jerkPick = pickCfg?.AccJerkPercent ?? 50;

                transfer.PickZ.MoveAbs(targetPickZ, v, accPick, decPick, jerkPick);
                ejector.AxisPinZ.MoveAbs(targetPinZ, v, accPin, decPin, jerkPin);
                transfer.PickZ.WaitMoveDone(-1);
                ejector.AxisPinZ.WaitMoveDone(-1);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message; return false;
            }
        }

        /// <summary>
        /// PickUp(PickZ) 과 Niddle Pin(PinZ)을 동시에 상대 이동 (현재 위치 + delta).
        /// </summary>
        public static bool MovePinAndPickSyncRel(InputStageEjector ejector, InputDieTransfer transfer,
                                                 double deltaPinZ, double deltaPickZ, double velScale,
                                                 out string error)
        {
            error = null;
            if (ejector?.AxisPinZ == null || transfer?.PickZ == null)
            {
                error = "Axis not ready"; return false;
            }
            try
            {
                double targetPin = ejector.AxisPinZ.GetPosition() + deltaPinZ;
                double targetPick = transfer.PickZ.GetPosition() + deltaPickZ;
                return MovePinAndPickSyncAbs(ejector, transfer, targetPin, targetPick, velScale, out error);
            }
            catch (Exception ex)
            {
                error = ex.Message; return false;
            }
        }

    }

}
