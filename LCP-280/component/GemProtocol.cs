using System;

namespace QMC.LCP_280.Process.Component
{
    public static class GemProtocol
    {
        // 1. Tray ID Report (트레이 바코드 인식 보고)
        public static string BuildTrayIdReport(string trayId, string recipeId, string eqpId, string operatorIdSv)
        {
            // Format: Header,TrayIdReport,TrayID,RecipeID,Unused,EqpID,OperatorID
            return $"Header,TrayIdReport,{trayId},{recipeId},x,{eqpId},{operatorIdSv}";
        }

        // 2. PP Selected (레시피 선택 보고)
        public static string BuildPPSelected(string eqpId, string operatorIdSv)
        {
            // Format: Header,PPSelected,x,x,x,EqpID,OperatorID
            return $"Header,PPSelected,x,x,x,{eqpId},{operatorIdSv}";
        }

        // 3. PP Upload Completed (레시피 업로드/변경 완료)
        public static string BuildPPUploadCompleted(string eqpId, string operatorIdSv)
        {
             // Format: Header,PPUploadCompleted,x,x,x,EqpID,OperatorID
            return $"Header,PPUploadCompleted,x,x,x,{eqpId},{operatorIdSv}";
        }

        // 4. Lot Processing Started (작업 시작)
        public static string BuildLotProcessingStarted(string eqpId, string operatorIdSv)
        {
            // Format: Header,LotProcessingStarted,x,x,x,EqpID,OperatorID
            return $"Header,LotProcessingStarted,x,x,x,{eqpId},{operatorIdSv}";
        }

        // 5. Lot Processing Completed (작업 완료 - KeyValue 형식 포함)
        public static string BuildLotProcessingCompletedKv(string lotId, string trayId, string mgzId, string coverId, string eqpId, string operatorIdSv)
        {
            // Key=Value 형태로 결합
            return "Header,LotProcessingCompleted," +
                   $"lotId={lotId}," +
                   $"trayId={trayId}," +
                   $"mgzId={mgzId}," +
                   $"coverId={coverId}," +
                   $"x,{eqpId},{operatorIdSv}";
        }

        // 6. Alarm Set
        public static string BuildAlarmSet(int code)
        {
            return $"AlarmSet,{code}";
        }

        // 7. Alarm Clear
        public static string BuildAlarmClear(int code)
        {
            return $"AlarmClear,{code}";
        }

        // 8. Idle Reason Set
        public static string BuildIdleReasonSet(string idleCode, string operatorId)
        {
            return $"IdleReasonSet,{idleCode},{operatorId}";
        }

        // 9. Idle Reason Reset
        public static string BuildIdleReasonReset(string idleCode, string operatorId)
        {
            return $"IdleReasonReset,{idleCode},{operatorId}";
        }
    }
}