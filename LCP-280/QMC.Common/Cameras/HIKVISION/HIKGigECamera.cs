using MvCamCtrl.NET;
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras;
using QMC.Common.HIKVISION;
using QMC.Common.Vision;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace QMC.Common.Cameras.HIKVISION
{
    [Serializable]
    public enum GrabMode
    {
        None,
        Live,
        Grab,
        Exporse
    }
    #region HIKGigECamera
    [Serializable]
    public class HIKGigECamera : Camera
    {
        #region Define
        [Serializable]
        public enum AlarmKeys
        {
            eExposeTimeOut = -25,
        }
        #endregion

        #region Field
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        public MyCamera.cbEventdelegateEx EventCallbackExposureEnd;
        public MyCamera.cbEventdelegateEx EventCallback;
        private int m_nRet;
        private string[] m_deviceList;
        private int m_deviceListIndex;
        MyCamera.MV_CC_DEVICE_INFO_LIST stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        private MyCamera m_MyCamera = new MyCamera();
        private string m_CamLog;
        bool m_bGrabbing = false;
        Thread m_hReceiveThread = null;
        MyCamera.MV_FRAME_OUT_INFO_EX m_stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
        private GrabMode m_CurrentMode;

        UInt32 m_nBufSizeForDriver = 0;
        IntPtr m_BufforDriver = IntPtr.Zero;
        private static Object BufforDriverLock = new Object();
        private IntPtr m_ViewerHandler;
        public bool m_ExposeEnd = true;
        public bool m_FrameEnd = true;

        static DateTime StartTime { get; set; }

        public string CameraName { get; set; }                      //  2024. 07. 25.  SCH : 카메라 Serial Number 를 장비 공통 파라미터에서 관리하기 위해 추가됨.
        #endregion

        #region Event

        #endregion

        #region Constructor
        public HIKGigECamera(string strName)
             : base(strName)
        {
            CameraName = strName;

            CameraConfig = new HIKGigECameraConfig();
            InitValue();
            this.ViewerHandler = IntPtr.Zero;
            nRet = new int();
            SerialNumber = "";
            m_CurrentMode = GrabMode.None;

        }
        public HIKGigECamera() : this("Camera") { }//?
        #endregion

        #region Property


        public string CamLog
        {
            get { return this.m_CamLog; }
            set { this.m_CamLog = value; }
        }
        public int nRet { get; set; }
        public IntPtr ViewerHandler
        {
            get { return m_ViewerHandler; }
            set { m_ViewerHandler = value; }
        }
        public string SerialNumber { get; set; }
        public HIKGigECameraConfig MyConfig
        {
            get { return CameraConfig as HIKGigECameraConfig; }
        }

        #endregion

        #region Method
        public override void Load(FileStream fs)
        {
            base.Load(fs);
            HIKGigECameraConfig config = new HIKGigECameraConfig();
            SaveManager.BinaryDeserialize<HIKGigECameraConfig>(fs, out config);

            CameraConfig = config;
        }
        public override void Save(FileStream fs)
        {
            base.Save(fs);
            SaveManager.BinarySerialize(fs, CameraConfig);
        }
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eExposeTimeOut;
            alarm.Title = "Expose Time Out";
            alarm.Cause = "Exposi Time Out";
            alarm.Source = Name;
            alarm.Grade = "Error";
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        public override int Create()
        {
            return base.Create();

        }
        public override int Initialize()
        {
            int ret = base.Initialize();
            try
            {                
                //SetInitializeProgress(0);

                //if (m_Status == RunStatus.Stop) return 1;
                if (m_MyCamera.MV_CC_IsDeviceConnected_NET() == true)
                {
                    this.Close();
                }
                else
                {
                    if (this.Opened == true)
                    {
                        this.Opened = false;
                    }
                }

                //SetInitializeProgress(30);

                if (this.Open() == 0)
                {
                    this.Live();
                }
                else
                {
                    MessageBox.Show(new Form { TopMost = true }, "카메라 연결 실패");
                    ret = -1;
                }

                this.SuspendedImageDisplay = false;

                //SetInitializeProgress(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ret;
        }
        public void EventCallbackFuncExposureEnd(ref MyCamera.MV_EVENT_OUT_INFO pEventInfo, IntPtr pUser)
        {
            if (pEventInfo.EventName == "ExposureEnd")
            {
                long startTime = DateTime.Now.Ticks;
                m_ExposeEnd = true;
                long endTime = DateTime.Now.Ticks;
                Log.Write("ExposureEnd", string.Format("{0},{1} ", ParentUnit.UnitName, endTime - startTime));
                //카메라 찍는게 끝나면

                //DateTime EndTime = DateTime.Now;
                //TimeSpan timeSpan = EndTime - StartTime;
                //MessageBox.Show(String.Format("Expose Time = {0}", timeSpan.TotalMilliseconds));
            }
            else if (pEventInfo.EventName == "FrameEnd")
            {
                m_ExposeEnd = true;
                MyCamera.MV_FRAME_OUT stFrameInfo = new MyCamera.MV_FRAME_OUT();
                m_MyCamera.MV_CC_GetImageBuffer_NET(ref stFrameInfo, 1000);
                VisionImage image = null;

                if (nRet == MyCamera.MV_OK)
                {
                    if (this.Resolution.Width != stFrameInfo.stFrameInfo.nWidth || this.Resolution.Height != stFrameInfo.stFrameInfo.nHeight)
                        this.Resolution = new Size(stFrameInfo.stFrameInfo.nWidth, stFrameInfo.stFrameInfo.nHeight);

                    CreateVisionImage(stFrameInfo.pBufAddr, out image);

                    m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stFrameInfo);
                    LatestImage = image;
                }

                m_FrameEnd = true;
                //long endTime = DateTime.Now.Ticks;
                //Log.Write("FrameEnd", string.Format("{0},{1} ", Owner.Name, endTime - startTime));
            }
            Console.WriteLine(pEventInfo.EventName);

        }
        public void EventCallbackFunc(ref MyCamera.MV_EVENT_OUT_INFO pEventInfo, IntPtr pUser)
        {
            if (pEventInfo.EventName == "ExposureEnd")
            {
                long startTime = DateTime.Now.Ticks;
                m_ExposeEnd = true;
                long endTime = DateTime.Now.Ticks;
                Log.Write("ExposureEnd", string.Format("{0},{1} ", ParentUnit.UnitName, endTime - startTime));
                //카메라 찍는게 끝나면

                //DateTime EndTime = DateTime.Now;
                //TimeSpan timeSpan = EndTime - StartTime;
                //MessageBox.Show(String.Format("Expose Time = {0}", timeSpan.TotalMilliseconds));
            }
            else if (pEventInfo.EventName == "FrameEnd")
            {
                m_ExposeEnd = true;
                MyCamera.MV_FRAME_OUT stFrameInfo = new MyCamera.MV_FRAME_OUT();
                m_MyCamera.MV_CC_GetImageBuffer_NET(ref stFrameInfo, 1000);
                VisionImage image = null;
                if (nRet == MyCamera.MV_OK)
                {
                    if (this.Resolution.Width != stFrameInfo.stFrameInfo.nWidth || this.Resolution.Height != stFrameInfo.stFrameInfo.nHeight)
                        this.Resolution = new Size(stFrameInfo.stFrameInfo.nWidth, stFrameInfo.stFrameInfo.nHeight);

                    CreateVisionImage(stFrameInfo.pBufAddr, out image);

                    m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stFrameInfo);
                    LatestImage = image;
                }

                m_FrameEnd = true;
                //long endTime = DateTime.Now.Ticks;
                //Log.Write("FrameEnd", string.Format("{0},{1} ", Owner.Name, endTime - startTime));
            }
            Console.WriteLine(pEventInfo.EventName);

        }
        public int DeviceSearch(string strSerialNumber, ref DeviceCollection devices)
        {
            stDeviceList.nDeviceNum = 0;

            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref stDeviceList);

            DeviceInformation deviceInformation = new DeviceInformation();

            for (int i = 0; i < stDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                    this.SerialNumber = gigeInfo.chSerialNumber;
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    this.SerialNumber = usbInfo.chSerialNumber;
                }

                if (this.SerialNumber == strSerialNumber)
                {
                    return i;
                }
                else if (strSerialNumber == "")
                {
                    MessageBox.Show(String.Format("Check SerialNumber, Current = {0}", this.SerialNumber));
                }
                devices.Add(deviceInformation);
            }
            if (stDeviceList.nDeviceNum != 0)
            {
                deviceInformation.Number = 0;
            }
            return -1;
        }

        public void ModeChange(GrabMode grabMode)
        {
            if (grabMode == m_CurrentMode)
            {
                return;
            }
            else
            {
                if (m_CurrentMode == GrabMode.Live)
                {
                    StopLive();
                }
                else if (m_CurrentMode == GrabMode.None)
                {
                }
                else
                {
                    int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
                    if (MyCamera.MV_OK != nRet)
                    {
                        CamLog += string.Format("MV_CC_StopGrabbing_NET Fail!", nRet);
                    }
                    nRet = m_MyCamera.MV_CC_SetEnumValueByString_NET("EventNotification", "Off");
                    if (MyCamera.MV_OK != nRet)
                    {
                        Console.WriteLine("Set EventNotification failed!");
                        return;
                    }
                }
            }
            if (grabMode == GrabMode.Grab)
            {

                if (m_bGrabbing)
                {
                    StopLive();
                    m_bGrabbing = false;
                }
                nRet = m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog += string.Format("Grab TriggerMode Fail", nRet);
                }
                //LatestImage = null;
                nRet = m_MyCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog += string.Format("Grab TriggerSource Fail", nRet);
                }
                nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog += string.Format("Grab Fail!", nRet);
                }
            }
            else if (grabMode == GrabMode.Live)
            {
                int nRet = m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                if (MyCamera.MV_OK != nRet)
                {
                    m_bGrabbing = false;
                    CamLog += string.Format(" Live AcquisitionMode Fail!", nRet);
                    return;
                }
                nRet = m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                if (MyCamera.MV_OK != nRet)
                {
                    m_bGrabbing = false;
                    CamLog += string.Format(" Live TriggerMode Fail!", nRet);
                    return;
                }
            }
            else
            {
                //Exporce
                nRet = m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog += string.Format("Exporce AcquisitionMode Fail!", nRet);
                }
                //nRet = m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                //if (MyCamera.MV_OK != nRet)
                //{
                //    CamLog += string.Format("Exporce TriggerMode Fail!", nRet);
                //}


                nRet = m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog += string.Format("Grab TriggerMode Fail", nRet);
                }
                LatestImage = null;
                nRet = m_MyCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog += string.Format("Grab TriggerSource Fail", nRet);
                }

                nRet = m_MyCamera.MV_CC_SetEnumValueByString_NET("EventSelector", "ExposureEnd");
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set EventSelector failed!");
                    return;
                }

                nRet = m_MyCamera.MV_CC_SetEnumValueByString_NET("EventNotification", "On");
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set EventNotification failed!");
                    return;
                }

                nRet = m_MyCamera.MV_CC_SetEnumValueByString_NET("EventSelector", "FrameEnd");
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set EventSelector failed!");
                    return;
                }

                nRet = m_MyCamera.MV_CC_SetEnumValueByString_NET("EventNotification", "On");
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Set EventNotification failed!");
                    return;
                }

                //if (EventCallbackExposureEnd == null)
                //    EventCallbackExposureEnd = new MyCamera.cbEventdelegateEx(EventCallbackFuncExposureEnd);
                if (EventCallback == null)
                    EventCallback = new MyCamera.cbEventdelegateEx(EventCallbackFunc);
                nRet = m_MyCamera.MV_CC_RegisterEventCallBackEx_NET("ExposureEnd", EventCallback, IntPtr.Zero);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Register event callback failed!");
                    return;
                }
                //if (EventCallback== null)
                //    EventCallback = new MyCamera.cbEventdelegateEx(EventCallbackFunc);
                nRet = m_MyCamera.MV_CC_RegisterEventCallBackEx_NET("FrameEnd", EventCallback, IntPtr.Zero);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Register event callback failed!");
                    return;
                }

                nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                    return;
                }
            }
            m_CurrentMode = grabMode;
        }
        public void ReadOut()
        {
            VisionImage image = null;
            OnReadout(out image);

        }
        public int Open(int channel)
        {
            if (channel < 0)
            {
                return -1;
            }
            MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDeviceList.pDeviceInfo[channel], typeof(MyCamera.MV_CC_DEVICE_INFO));

            if (null == m_MyCamera)
            {
                m_MyCamera = new MyCamera();
                if (null == m_MyCamera)
                {
                    return -1;
                }
            }

            int nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                return nRet;
            }

            bool bRet = m_MyCamera.MV_CC_IsDeviceConnected_NET();

            for (int i = 0; i < MyConfig.RetryCount; i++)
            {
                nRet = m_MyCamera.MV_CC_OpenDevice_NET();

                if (MyCamera.MV_OK == nRet)
                {
                    break;
                }
                Thread.Sleep(MyConfig.OpenDelayTime);
            }
            if (MyCamera.MV_OK != nRet)
            {

                m_MyCamera.MV_CC_DestroyDevice_NET();
                CamLog = string.Format("Device Open Fail! : {0}", nRet);
                return nRet;
            }
            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = m_MyCamera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    nRet = m_MyCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    if (nRet != MyCamera.MV_OK)
                    {
                        CamLog = string.Format("Set Packet Size failed!", nRet);
                    }
                }
                else
                {
                    CamLog = string.Format("Get Packet Size failed!", nPacketSize);
                }
            }

            nRet = m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);



            MyCamera.MVCC_FLOATVALUE floatValue = new MyCamera.MVCC_FLOATVALUE();
            MyCamera.MVCC_INTVALUE widthValue = new MyCamera.MVCC_INTVALUE();
            MyCamera.MVCC_INTVALUE heightValue = new MyCamera.MVCC_INTVALUE();

            MyCamera.MVCC_INTVALUE widthMaxValue = new MyCamera.MVCC_INTVALUE();
            MyCamera.MVCC_INTVALUE heightMaxValue = new MyCamera.MVCC_INTVALUE();


            nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", this.MyConfig.ExposureTime);
            if (MyCamera.MV_OK != nRet)
            {
                CamLog = string.Format("Set ExposureTime Failed! : {0}", nRet);
                return nRet;
            }

            nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gain", this.MyConfig.Gain);
            if (MyCamera.MV_OK != nRet)
            {
                CamLog = string.Format("Set Gain Failed! : {0}", nRet);
                //return nRet;
            }

            m_MyCamera.MV_CC_GetIntValue_NET("WidthMax", ref widthMaxValue);
            m_MyCamera.MV_CC_GetIntValue_NET("HeightMax", ref heightMaxValue);

            //Resolution 초기화.

            nRet = m_MyCamera.MV_CC_SetAOIoffsetX_NET(MyConfig.OffsetX);
            if (MyCamera.MV_OK != nRet)
            {
                nRet = m_MyCamera.MV_CC_SetWidth_NET((uint)MyConfig.Resolution.Width);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Resolution Width Failed! : {0}", nRet);
                    return nRet;
                }
                nRet = m_MyCamera.MV_CC_SetAOIoffsetX_NET(MyConfig.OffsetX);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Offset X Failed! : {0}", nRet);
                    return nRet;
                }
            }

            nRet = m_MyCamera.MV_CC_SetAOIoffsetY_NET(MyConfig.OffsetY);
            if (MyCamera.MV_OK != nRet)
            {
                nRet = m_MyCamera.MV_CC_SetHeight_NET((uint)MyConfig.Resolution.Height);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Resolution Height Failed! : {0}", nRet);
                    return nRet;
                }
                nRet = m_MyCamera.MV_CC_SetAOIoffsetY_NET(MyConfig.OffsetY);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Offset Y Failed! : {0}", nRet);
                    return nRet;
                }
            }



            nRet = m_MyCamera.MV_CC_SetWidth_NET((uint)MyConfig.Resolution.Width);
            if (MyCamera.MV_OK != nRet)
            {
                CamLog = string.Format("Set Resolution Width Failed! : {0}", nRet);
                return nRet;
            }

            nRet = m_MyCamera.MV_CC_SetHeight_NET((uint)MyConfig.Resolution.Height);
            if (MyCamera.MV_OK != nRet)
            {
                CamLog = string.Format("Set Resolution Height Failed! : {0}", nRet);
                return nRet;
            }



            nRet = m_MyCamera.MV_CC_GetWidth_NET(ref widthValue);
            if (MyCamera.MV_OK != nRet)
            {
                CamLog = string.Format("Get Resolution Width Failed! : {0}", nRet);
                return nRet;
            }

            nRet = m_MyCamera.MV_CC_GetHeight_NET(ref heightValue);
            if (MyCamera.MV_OK != nRet)
            {
                CamLog = string.Format("Get Resolution Height Failed! : {0}", nRet);
                return nRet;
            }

            //Enable 사용
            //SET WIDTH, hEIHGT, OFFSET
            bool bImageCrop = MyConfig.UseCutImage;    //Config값 넣어줌.

            if (bImageCrop)
            {
                nRet = m_MyCamera.MV_CC_GetWidth_NET(ref widthValue);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Get Resolution Width Failed! : {0}", nRet);
                    return nRet;
                }

                nRet = m_MyCamera.MV_CC_GetHeight_NET(ref heightValue);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Get Resolution Height Failed! : {0}", nRet);
                    return nRet;
                }

                uint unWidth = MyConfig.CutImageWidth;       //MyConfig값 넣어줌.
                uint unHeight = MyConfig.CutImageHeight;      //MyConfig값 넣어줌.
                unWidth = unWidth - (unWidth % 16);
                unHeight = unHeight - (unHeight % 2);

                uint unOffsetX = (widthMaxValue.nCurValue - unWidth) / 2;
                uint unOffsetY = (heightMaxValue.nCurValue - unHeight) / 2;

                unOffsetX = unOffsetX - (unOffsetX % 16);
                unOffsetY = unOffsetY - (unOffsetY % 2);

                //nRet = m_MyCamera.MV_CC_SetHeight_NET(unWidth);
                nRet = m_MyCamera.MV_CC_SetWidth_NET(unWidth);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Resolution Width Failed! : {0}", nRet);
                    return nRet;
                }

                nRet = m_MyCamera.MV_CC_SetHeight_NET(unHeight);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Resolution Height Failed! : {0}", nRet);
                    return nRet;
                }

                nRet = m_MyCamera.MV_CC_SetAOIoffsetX_NET(unOffsetX);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Offset X Failed! : {0}", nRet);
                    return nRet;
                }

                nRet = m_MyCamera.MV_CC_SetAOIoffsetY_NET(unOffsetY);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Offset Y Failed! : {0}", nRet);
                    return nRet;
                }
            }


            this.Resolution = this.MyConfig.Resolution = new Size((int)widthValue.nCurValue, (int)heightValue.nCurValue);
            m_CurrentMode = GrabMode.None;

            return nRet;
        }

        public int Grab()
        {
            ModeChange(GrabMode.Grab);
            // DateTime StartTime = DateTime.Now;
            //Log.Write("TopInspection", "Camera Grab Start");
            nRet = m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != nRet)
            {
                CamLog += string.Format("Grab Fail!", nRet);
            }
            MyCamera.MV_FRAME_OUT stFrameInfo = new MyCamera.MV_FRAME_OUT();

            nRet = m_MyCamera.MV_CC_GetImageBuffer_NET(ref stFrameInfo, 1000);
            //Log.Write("TopInspection", "Camera Grab End");
            if (nRet == MyCamera.MV_OK)
            {
                VisionImage image;

                if (this.ImageRotate == ImageRotateInfo.None)
                {
                    if (this.Resolution.Width != stFrameInfo.stFrameInfo.nWidth || this.Resolution.Height != stFrameInfo.stFrameInfo.nHeight)
                        this.Resolution = new Size(stFrameInfo.stFrameInfo.nWidth, stFrameInfo.stFrameInfo.nHeight);
                }
                else
                {
                    if (this.Resolution.Width != stFrameInfo.stFrameInfo.nHeight || this.Resolution.Height != stFrameInfo.stFrameInfo.nWidth)
                        this.Resolution = new Size(stFrameInfo.stFrameInfo.nWidth, stFrameInfo.stFrameInfo.nHeight);

                }

                CreateVisionImage(stFrameInfo.pBufAddr, out image);
                LatestImage = image;
                m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stFrameInfo);
            }

            //Log.Write("TopInspection", "CreateVisionImage End");
            return nRet;
        }

        public void Close()
        {
            m_bGrabbing = false;
            if (m_hReceiveThread != null)
            {
                m_hReceiveThread.Join();
            }
            
            try
            {
                int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
                if (nRet != MyCamera.MV_OK)
                {
                    CamLog += string.Format("Stop Grabbing Fail", nRet);
                }
                m_MyCamera.MV_CC_CloseDevice_NET();
            }
            catch (DllNotFoundException ex)
            {
                // DLL 로드 실패 시 로그를 남기고 예외를 처리
                CamLog += "DLL not found: " + ex.Message;
            }
            
            this.Opened = false;
        }


        public int OffsetMove(uint m_nOffset_X, uint m_nOffset_Y)
        {
            int nRet = m_MyCamera.MV_CC_SetAOIoffsetX_NET(m_nOffset_X);
            if (MyCamera.MV_OK != nRet)
            {
                nRet = m_MyCamera.MV_CC_SetWidth_NET((uint)MyConfig.Resolution.Width);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Resolution Width Failed! : {0}", nRet);
                    return nRet;
                }
                nRet = m_MyCamera.MV_CC_SetAOIoffsetX_NET(m_nOffset_X);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Offset X Failed! : {0}", nRet);
                    return nRet;
                }
            }

            nRet = m_MyCamera.MV_CC_SetAOIoffsetY_NET(m_nOffset_Y);
            if (MyCamera.MV_OK != nRet)
            {
                nRet = m_MyCamera.MV_CC_SetHeight_NET((uint)MyConfig.Resolution.Height);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Resolution Height Failed! : {0}", nRet);
                    return nRet;
                }
                nRet = m_MyCamera.MV_CC_SetAOIoffsetY_NET(m_nOffset_Y);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Offset Y Failed! : {0}", nRet);
                    return nRet;
                }
            }

            return nRet;
        }

        public uint GetOffsetX()
        {
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            m_MyCamera.MV_CC_GetAOIoffsetX_NET(ref stParam);
            return stParam.nCurValue;
        }

        public uint GetOffsetY()
        {
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            m_MyCamera.MV_CC_GetAOIoffsetY_NET(ref stParam);
            return stParam.nCurValue;
        }

        //public int Expose()
        //{
        //    int ret = 0;
        //    try
        //    {
        //        ModeChange(GrabMode.Exporse);
        //        TimeoutChecker timeoutChecker = new TimeoutChecker(500, true);

        //        m_ExposeEnd = false;
        //        m_FrameEnd = false;
        //        StartTime = DateTime.Now;
        //        nRet = m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
        //        if (MyCamera.MV_OK != nRet)
        //        {
        //            CamLog += string.Format("Expose Fail!", nRet);
        //        }
        //        //while(true)
        //        //{
        //        //    if (m_ExposeEnd)
        //        //        break;
        //        //    Thread.Sleep(1);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        ret = -1;
        //    }

        //    return ret;
        //}

        public int ExposeEnd()
        {
            int ret = 0;
            DateTime start = DateTime.Now;
            TimeSpan ExposseTime;
            TimeoutChecker tc = new TimeoutChecker(1000, true);
            while (true)
            {
                if (m_ExposeEnd)
                    break;

                ExposseTime = DateTime.Now - start;
                if (tc.IsCompleted)
                {
                    ret = -1;
                    break;
                }
                Thread.Sleep(1);
            }
            //UpdateQueue(start, DateTime.Now);
            return ret;
        }

        public void Live()
        {
            if (m_CurrentMode == GrabMode.Live)
            {
                return;
            }
            m_bGrabbing = true;
            ModeChange(GrabMode.Live);

            if (m_hReceiveThread != null && m_hReceiveThread.IsAlive)
            {
                m_hReceiveThread.Abort();
                m_hReceiveThread.Join();
            }
            m_hReceiveThread = new Thread(ReceiveThreadProcess);
            m_hReceiveThread.Start();
            m_stFrameInfo.nFrameLen = 0;
            m_stFrameInfo.enPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;
            nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_bGrabbing = false;
                m_hReceiveThread.Join();
                CamLog = string.Format("Start Live Fail!", nRet);
                Opened = false;
                return;
            }

            IsLiveOn = true;
        }
        public void StopLive()
        {
            m_bGrabbing = false;
            if (m_hReceiveThread != null)
            {
                m_hReceiveThread.Join();
            }

            int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
            if (nRet != MyCamera.MV_OK)
            {
                CamLog = string.Format("Stop Grabbing Fail", nRet);
            }
            IsLiveOn = false;
            m_CurrentMode = GrabMode.None;
        }
        public void ReceiveThreadProcess()
        {
            MyCamera.MV_FRAME_OUT stFrameInfo = new MyCamera.MV_FRAME_OUT();
            int nRet = MyCamera.MV_OK;

            while (m_bGrabbing)
            {
                nRet = m_MyCamera.MV_CC_GetImageBuffer_NET(ref stFrameInfo, 1000);
                if (nRet == MyCamera.MV_OK)
                {
                    VisionImage image;
                    if (this.Resolution.Width != stFrameInfo.stFrameInfo.nWidth || this.Resolution.Height != stFrameInfo.stFrameInfo.nHeight)
                        this.Resolution = new Size(stFrameInfo.stFrameInfo.nWidth, stFrameInfo.stFrameInfo.nHeight);

                    CreateVisionImage(stFrameInfo.pBufAddr, out image);
                    LatestImage = image;

                    if (RemoveCustomPixelFormats(stFrameInfo.stFrameInfo.enPixelType))
                    {
                        m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stFrameInfo);
                        continue;
                    }
                    m_MyCamera.MV_CC_FreeImageBuffer_NET(ref stFrameInfo);
                }
            }
        }

        private bool RemoveCustomPixelFormats(MyCamera.MvGvspPixelType enPixelFormat)
        {
            Int32 nResult = ((int)enPixelFormat) & (unchecked((Int32)0x80000000));
            if (0x80000000 == nResult)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public override void UpdateConfigData() //참고 : Override
        //{

        //    //if (Owner is DieLoader) // 참고 : Loader인지 Unloader인지 parsing
        //    //{
        //    //    DieLoader dieLoader = Owner as DieLoader;
        //    //    CameraConfig = dieLoader.DieLoaderConfig.HIKGigECameraConfig;//?

        //    //}
        //    //else if (Owner is DieUnloader)
        //    //{
        //    //    DieUnloader dieUnloader = Owner as DieUnloader;
        //    //    CameraConfig = dieUnloader.DieUnloaderConfig.HIKGigECameraConfig;
        //    //}
        //    //else if (Owner is DieTransfer)
        //    //{
        //    //    DieTransfer dieTransfer = Owner as DieTransfer;
        //    //    CameraConfig = dieTransfer.DieTransferConfig.HIKGigECameraConfig;
        //    //}
        //    //else { }
        //    //this.Resolution = MyConfig.Resolution;
        //}

        public override int SetGain(double dGain)
        {
            this.MyConfig.Gain = (float)dGain;

            if (this.MyConfig.CameraType == CameraType.Normal)
            {
                int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gain", this.MyConfig.Gain);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Gain Failed! : {0}", nRet);
                    //return nRet;
                }
            }
            else
            {
                uint preampGain = 1250;
                if (this.MyConfig.Gain >= 6)
                {
                    preampGain = 6000;
                }
                else if (this.MyConfig.Gain >= 5.75)
                {
                    preampGain = 5750;
                }
                else if (this.MyConfig.Gain >= 5.5)
                {
                    preampGain = 5500;
                }
                else if (this.MyConfig.Gain >= 5.25)
                {
                    preampGain = 5250;
                }
                else if (this.MyConfig.Gain >= 5.0)
                {
                    preampGain = 5000;
                }
                else if (this.MyConfig.Gain >= 4.75)
                {
                    preampGain = 4750;
                }
                else if (this.MyConfig.Gain >= 4.5)
                {
                    preampGain = 4500;
                }
                else if (this.MyConfig.Gain >= 4.25)
                {
                    preampGain = 4250;
                }
                else if (this.MyConfig.Gain >= 4.00)
                {
                    preampGain = 4000;
                }
                else if (this.MyConfig.Gain >= 3.75)
                {
                    preampGain = 3750;
                }
                else if (this.MyConfig.Gain >= 3.5)
                {
                    preampGain = 3500;
                }
                else if (this.MyConfig.Gain >= 3.25)
                {
                    preampGain = 3250;
                }
                else if (this.MyConfig.Gain >= 3.00)
                {
                    preampGain = 3000;
                }
                else if (this.MyConfig.Gain >= 2.75)
                {
                    preampGain = 2750;
                }
                else if (this.MyConfig.Gain >= 2.5)
                {
                    preampGain = 2500;
                }
                else if (this.MyConfig.Gain >= 2.25)
                {
                    preampGain = 2250;
                }
                else if (this.MyConfig.Gain >= 2.00)
                {
                    preampGain = 2000;
                }
                else if (this.MyConfig.Gain >= 1.75)
                {
                    preampGain = 1750;
                }
                else if (this.MyConfig.Gain >= 1.5)
                {
                    preampGain = 1500;
                }
                else
                {
                    preampGain = 1250;
                }

                nRet = m_MyCamera.MV_CC_SetEnumValue_NET("PreampGain", preampGain);
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog = string.Format("Set Gain Failed! : {0}", nRet);
                    //return nRet;
                }
            }

            //int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gain", this.MyConfig.Gain);
            //nRet = m_MyCamera.MV_CC_SetEnumValue_NET("PreampGain", 1500);
            //MyCamera.MVCC_FLOATVALUE a = new MyCamera.MVCC_FLOATVALUE();
            //nRet = m_MyCamera.MV_CC_GetGain_NET(ref a);
            //if (MyCamera.MV_OK != nRet)
            //{
            //    CamLog = string.Format("Set Gain Failed! : {0}", nRet);
            //    //return nRet;
            //}

            return nRet;
        }
        protected override int OnGetFrameRate(ref double frameRate)
        {
            throw new NotImplementedException();
        }

        protected override int OnSetFrameRate(double frameRate)
        {
            throw new NotImplementedException();
        }

        protected override int OnGetMaxFrameRate(ref RangeD frameRate)
        {
            throw new NotImplementedException();
        }

        protected override int OnGetExposureTime(ref double exposureTime)
        {
            throw new NotImplementedException();
        }

        protected override int OnSetExposureTime(double exposureTime)
        {
            int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", (float)exposureTime);
            if (MyCamera.MV_OK != nRet)
            {
                CamLog = string.Format("Set ExposureTime Failed! : {0}", nRet);
                return nRet;
            }

            return nRet;
        }


        protected override int OnReconnect()
        {
            throw new NotImplementedException();
        }

        protected override int OnStartLive()
        {
            int ret = 0;
            if (m_bGrabbing)
            {
                //라이브중
            }
            else
            {
                this.Live();
            }
            return ret;
        }

        protected override int OnStopLive()
        {
            int ret = 0;
            this.StopLive();
            return ret;
        }

        protected override int OnGrab(out VisionImage image)
        {
            image = null;
            int ret = this.Grab();

            if (LatestImage != null)
            {
                image = LatestImage;
            }
            //image = new VisionImage();
            //? 이미지 어떻게 가져오는지
            return ret;
        }

        protected override int OnOpen()
        {
            int ret = 0;


            //Config에 설정된 시리얼 넘버에 해당하는 Channel 값을 가져온다.

            //Open(Channel) 함수를 호출한다.

            int channel = 0;
            DeviceCollection deviceInformation = new DeviceCollection();

            // equ 사용하는거 수정 필요.
            //  장비 공통 파라미터에 있는 카메라 시리얼 넘버를 사용하는지?
            //if (Equipment.CameraSerialNumberType)
            //{
            //    if ((CameraName == "Upper Camera") && (Equipment.PAKCamera_SerialNumber != ""))              //  Upper Camera 이고, 시리얼 넘버가 있으면?
            //    {
            //        MyConfig.SerialNumber = Equipment.PAKCamera_SerialNumber;
            //        MyConfig.Resolution = new Size(Equipment.PAKCamera_Width, Equipment.PAKCamera_Height);
            //        MyConfig.CameraResolution = new Size(Equipment.PAKCamera_Width, Equipment.PAKCamera_Height);
            //    }

            //    if ((CameraName == "Lower Camera") && (Equipment.WaferCamera_SerialNumber != ""))            //  Lower Camera 이고, 시리얼 넘버가 있으면?
            //    {
            //        MyConfig.SerialNumber = Equipment.WaferCamera_SerialNumber;
            //        MyConfig.Resolution = new Size(Equipment.WaferCamera_Width, Equipment.WaferCamera_Height);
            //        MyConfig.CameraResolution = new Size(Equipment.WaferCamera_Width, Equipment.WaferCamera_Height);
            //    }
            //}

            channel = this.DeviceSearch(MyConfig.SerialNumber, ref deviceInformation);

            ret = this.Open(channel);

            return ret;
            //Channel 값 확인

        }

        protected override int OnClose()
        {
            int ret = 0;
            this.Close();
            return ret;
        }

        protected override int OnExpose()
        {

            int ret = 0;
            try
            {
                ModeChange(GrabMode.Exporse);
                TimeoutChecker timeoutChecker = new TimeoutChecker(500, true);

                m_ExposeEnd = false;
                m_FrameEnd = false;
                StartTime = DateTime.Now;
                nRet = m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                if (MyCamera.MV_OK != nRet)
                {
                    CamLog += string.Format("Expose Fail!", nRet);
                }
                //while(true)
                //{
                //    if (m_ExposeEnd)
                //        break;
                //    Thread.Sleep(1);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ret = -1;
            }

            return ret;

        }

        protected override int OnReadout(out VisionImage image)
        {
            MyCamera.MV_FRAME_OUT stFrameInfo = new MyCamera.MV_FRAME_OUT();
            image = null;
            TimeoutChecker timeoutChecker = new TimeoutChecker(500, true);
            while (true)
            {
                if (m_FrameEnd)
                {
                    break;
                }
                if (timeoutChecker.IsCompleted)
                {
                    return -1;

                }
                Thread.Sleep(1);
            }
            image = LatestImage;

            return 0;
        }
        #endregion

    }
    [Serializable]
    #endregion
    public class DeviceCollection : Collection<DeviceInformation>
    {
        public DeviceCollection()
        {

        }
    }
    [Serializable]
    public class DeviceInformation
    {
        private string m_Name;
        private int m_Number;

        public DeviceInformation()
        {
            this.Name = "";
            this.Number = 0;
        }

        public string Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }

        public int Number
        {
            get { return this.m_Number; }
            set { this.m_Number = value; }
        }
    }

}
