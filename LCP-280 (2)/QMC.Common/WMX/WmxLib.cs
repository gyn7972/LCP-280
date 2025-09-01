using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMX3ApiCLR;
using WMX3ApiCLR.EcApiCLR;
using WMX3ApiCLR.SimuApiCLR;

namespace QMC.Core.Handler.Wmx3
{
    public static class WmxLib
    {



        #region => Fields
        

        private static WMX3Api _api = new WMX3Api();
        private static CoreMotion _cm = new CoreMotion(_api);
        private static Io _io = new Io(_api);
        private static Ecat _ecapi = new Ecat(_api);
        private static Compensation _compen = new Compensation(_api);
        private static int _slaveCount = 0;
        private static ApiBuffer _apiBuffer = new ApiBuffer(_api);

        #endregion


        #region => Property
        public static bool IsSimulatedMode { get; internal set; } = false;
        public static int LastError { get; internal set; } = 0;
        public static bool IsNoHaveTenkey { get; internal set; } = true;
        public static CoreMotion CM => _cm;
        public static Io IO => _io;

        public static Compensation Compen => _compen;



        public static ApiBuffer ApiBuffer => _apiBuffer;




        
        #endregion


        #region => 오픈/클로즈
        public static bool Open(int axisCnt, int pdoDeviceCnt)
        {
            IsSimulatedMode = false;
            _slaveCount = axisCnt + pdoDeviceCnt;

            // DeviceType.DeviceTypeNormal이 가장 우선순위가 높은것, 
            // DeviceTypeNormal 우선순위는 되도록 1개의 device에서만 사용
            LastError = _api.CreateDevice("C:\\Program Files\\SoftServo\\WMX3\\",
                                               DeviceType.DeviceTypeNormal,
                                               0xFFFFFFFF);

            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to CreateDevice() : errcode[{LastError}]");
                return false;
            }

            if (!StopCommunication())
                return false;

            LastError = _api.SetDeviceName("corehandlerwmx3");
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to SetDeviceName() : errcode[{LastError}]");
                return false;
            }

            return true;
        }

        public static bool OpenSimul(int axisCnt, int pdoDeviceCnt)
        {
            IsSimulatedMode = true;
            _slaveCount = axisCnt + pdoDeviceCnt;

            // DeviceType.DeviceTypeNormal이 가장 우선순위가 높은것, 
            // DeviceTypeNormal 우선순위는 되도록 1개의 device에서만 사용
            LastError = _api.CreateDevice("C:\\Program Files\\SoftServo\\WMX3\\",
                                               DeviceType.DeviceTypeNormal,
                                               0xFFFFFFFF);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to CreateDevice() : errcode[{LastError}]");
                return false;
            }

            if (!StopCommunication())
                return false;

            LastError = _api.SetDeviceName("corehandlerwmx3");
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to SetDeviceName() : errcode[{LastError}]");
                return false;
            }
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool Close()
        {
            StopCommunication();
            Thread.Sleep(500);
            _api.CloseDevice();
            return true;
        }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool StartCommunication()
        {
            var status = new CoreMotionStatus();
            LastError = _cm.GetStatus(ref status);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to GetStatus() : errcode[{LastError}]");
                return false;
            }

            //- 이미 통신연결 상태일 경우.. 바로 리턴..
            if (status.EngineState == EngineState.Communicating)
                return true;

            LastError = _api.StartCommunication(0xFFFFFFFF);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to StartCommunication() : errcode[{LastError}]");
                return false;
            }

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool StopCommunication()
        {
            LastError = _api.StopCommunication(0xFFFFFFFF);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to StopCommunication() : errcode[{LastError}]");
                return false;
            }

            return true;
        }



        /// <summary>
        /// wmx_parameters.xml 파일 로딩..
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool LoadPara(string path)
        {
            if (!StopCommunication()) // 통신 해제 상태에서 파라메터 로딩해야함..
                return false;
            Thread.Sleep(2000);

            LastError = _cm.Config.ImportAndSetAll(path);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to ImportAndSetAll() : errcode[{LastError}]");
                return false;
            }

            return true;
        }





        /// <summary>
        /// 해당id의 슬레이브가 살아있는지 확인
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsSlaveAlive(int id)
        {
            if (IsSimulatedMode)
                return true;

            if ((id < 0) || (256 <= id))
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to IsSlaveAlive() : id[{id}] is over range");
                return false;
            }

            var info = new EcMasterInfo();
            LastError = _ecapi.GetMasterInfo(info);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to GetMasterInfo() : errcode[{LastError}]");
                return false;
            }

            for (int i = 0; i < info.NumOfSlaves; i++)
            {
                if (info.Slaves[i].Id == id)
                {
                    if (info.Slaves[i].State == EcStateMachine.Op)
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsSlaveStateOp(int id)
        {
            if (IsSimulatedMode)
                return true;

            var info = new EcMasterInfo();
            LastError = _ecapi.GetMasterInfo(info);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to GetMasterInfo() : errcode[{LastError}]");
                return false;
            }

            if (info.Slaves[id].State == EcStateMachine.Op)
                return true;
            
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool HotConnect()
        {
            if (IsSimulatedMode)
                return true;

            bool isConnected = true;

            var info = new EcMasterInfo();
            LastError = _ecapi.GetMasterInfo(info);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to GetMasterInfo() : errcode[{LastError}]");
                return false;
            }


            if (info.NumOfSlaves != _slaveCount)
                return false;

            int ecApiError = 0;
            for (int i = 0; i < _slaveCount; i++)  
            {
                if (info.Slaves[i].State != EcStateMachine.Op)
                {
                    if (EcStateMachine.PreOp == info.Slaves[i].State)
                        _ecapi.ChangeSlaveState(i, EcStateMachine.Op, ref ecApiError);

                    isConnected = false;
                    break;
                }
            }

            if (isConnected)
                return true;

            LastError = _ecapi.StartHotconnect();
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to StartHotconnect() : errcode[{LastError}]");
                return false;
            }

            return false;
        }


        /// <summary>
        /// 모터의 브레이크 상태를 변경함.
        /// </summary>
        /// <returns></returns>
        public static bool SetBrake(int axisNum, bool brakeOn)
        {
            if (IsSimulatedMode)
                return true;
            var info = new EcMasterInfo();
            LastError = _ecapi.GetMasterInfo(info);
            if (0 != LastError)
            {
                Logger.Log(Logger.Module.Handler, Logger.Type.Error, $"Fail to GetMasterInfo() : errcode[{LastError}]");
                return false;
            }

            int slaveId = -1;
            for (int id = 0; id < 256; id++)
            {
                if (info.Slaves[id].Alias == axisNum)
                {
                    slaveId = id;
                    break;
                }
            }

            if (0 > slaveId)
                return false;

            uint errcode = 0;
            if (brakeOn)
            {
                byte[] sdo = { 0, 1, 0, 0 };
                _ecapi.SdoDownload(slaveId, 0x60fe, 0x01, sdo, ref errcode);
                _ecapi.SdoDownload(slaveId, 0x60fe, 0x02, sdo, ref errcode);
            }
            else
            {
                byte[] sdo = { 0, 0, 0, 0 };
                _ecapi.SdoDownload(slaveId, 0x60fe, 0x01, sdo, ref errcode);
                _ecapi.SdoDownload(slaveId, 0x60fe, 0x02, sdo, ref errcode);
            }


            return true;
        }


        #region => API Buffer 사용 예제...
        //--- Api buffer 주요 함수 ---
        // CreateApiBuffer : api buffer 생성, 최대 255채널 생성 가능
        // Halt : api buffer 정지
        // Clear : api buffer에 저장된 함수 제거
        // FreeApiBuffer : api buffer 채널 제거
        // GetStatus : api buffer 정보(buffer상태, 에러 로그, 저장된 함수 개수....)
        // SetOption : 버퍼 실행 완료시 Rewind, 함수 호출시 에러발생시 buffer정지 옵션...


        public static bool ApiBufferCreate(uint ch)
        {
            var option = new ApiBufferOptions();
            option.AutoRewind  = false;
            option.StopOnError = true;

            _apiBuffer.CreateApiBuffer(ch);
            _apiBuffer.SetOptions(ch, option);
            _apiBuffer.Clear(ch);
            return true;
        }

        public static bool ApiBufferClear(uint ch)
        {
            _apiBuffer.Clear(ch);
            return true;
        }

        public static bool ApiBufferFree(uint ch)
        {
            _apiBuffer.Halt(ch);
            _apiBuffer.FreeApiBuffer(ch);
            return true;
        }

        public static bool ApiBufferBeginRecord(uint ch)
        {
            _apiBuffer.StartRecordBufferChannel(ch);
            return true;
        }

        public static bool ApiBufferEndRecord()
        {
            _apiBuffer.EndRecordBufferChannel();
            return true;
        }

        public static bool ApiBufferExecute(uint ch)
        {
            _apiBuffer.Execute(ch);
            return true;
        }

        public static bool ApiBufferGetStatus(uint ch, ref ApiBufferStatus status)
        {
            _apiBuffer.GetStatus(ch, ref status);
            return true;
        }

        public static bool ApiBufferIsBusy(uint ch)
        {
            var status = new ApiBufferStatus();
            _apiBuffer.GetStatus(ch, ref status);

            return (0 < status.RemainingBlockCount);
        }

        public static bool ApiBufferWait(ApiBufferCondition condition)
        {
            _apiBuffer.Wait(condition);
            return true;
        }

        public static bool ApiBufferWait(int axis)
        {
            _apiBuffer.Wait(axis);
            return true;
        }

        public static bool ApiBufferWait(AxisSelection selection)
        {
            _apiBuffer.Wait(selection);
            return true;
        }

        #endregion
    }
}
