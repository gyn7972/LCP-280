using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace QMC.Common.Motion.Ajin
{
    public static class AXDEV
    {
        #region Define
        public const string LibraryFileName = "AXL.dll";
        #endregion

        #region Dll Imports
        // 지정 축에 32bit byte Setting
        [DllImport(LibraryFileName)]
        private static extern uint AxmSetCommandData32Qi(int nAxisNo, byte sCommand, uint uData);
        // 지정 축에 32bit byte 가져오기
        [DllImport(LibraryFileName)]
        private static extern uint AxmGetCommandData32Qi(int nAxisNo, byte sCommand, ref uint upData);

        // 지정 축에 스크립트 내부 Queue Index를 Clear 시킨다.
        // uSelect IP. 
        // uSelect(0): 스크립트 Queue Index 를 Clear한다.
        //        (1): 캡션 Queue를 Index Clear한다.

        // uSelect QI. 
        // uSelect(0): 스크립트 Queue 1 Index 을 Clear한다.
        //        (1): 스크립트 Queue 2 Index 를 Clear한다.
        [DllImport(LibraryFileName)]
        private static extern uint AxmSetScriptCaptionQueueClear(int lAxisNo, uint uSelect);

	    // 지정 축에 스크립트를 설정한다. - QI
	    // sc    : 스크립트 번호 (1 - 4)
	    // event : 발생할 이벤트 SCRCON 을 정의한다.
	    //         이벤트 설정 축갯수설정, 이벤트 발생할 축, 이벤트 내용 1,2 속성 설정한다.
	    // cmd   : 어떤 내용을 바꿀것인지 선택 SCRCMD를 정의한다.
	    // data  : 어떤 Data를 바꿀것인지 선택
        [DllImport(LibraryFileName)]
        private static extern uint AxmSetScriptCaptionQi(int lAxisNo, int sc, uint occurredEvent, uint cmd, uint data);

        #endregion

        #region Script 함수
        public static int SetScriptCaptionQueueClear(int axisNo, uint select)
        {
            int ret = 0;
            if ((ret = AXL.CheckErrorCode("AXDEV.AxmSetScriptCaptionQueueClear", AXDEV.AxmSetScriptCaptionQueueClear(axisNo, select))) != 0) return ret;
            return ret;
        }

        public static int SetScriptCaptionQi(int axisNo, int sc, uint occurredEvent, uint cmd, uint data)
        {
            int ret = 0;
            if ((ret = AXL.CheckErrorCode("AXDEV.AxmSetScriptCaptionQi", AXDEV.AxmSetScriptCaptionQi(axisNo, sc, occurredEvent, cmd, data))) != 0) return ret;
            return ret;
        }
        #endregion

        #region 지정축 trigger setting
        public static int SetCommandData32Qi(int axisNo, byte command, uint data)
        {
            int ret = 0;
            if ((ret = AXL.CheckErrorCode("AXDEV.AxmSetCommandData32Qi", AXDEV.AxmSetCommandData32Qi(axisNo, command, data))) != 0) return ret;
            return ret;
        }

        public static int GetCommandData32Qi(int axisNo, byte command, ref uint data)
        {
            int ret = 0;
            if ((ret = AXL.CheckErrorCode("AXDEV.AxmGetCommandData32Qi", AXDEV.AxmGetCommandData32Qi(axisNo, command, ref data))) != 0) return ret;
            return ret;
        }
        #endregion
    }
}
