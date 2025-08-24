/*
 * Purpose
 *      카메라들을 관리하는 스위치(Switch)를 정의한다.
 * 
 * Revision
 *      1. Created: 2018.02.12 by JUNG.CY
 *      
 * 
 */

using System;
using System.Timers;
using System.Drawing;
using System.Collections.ObjectModel;


namespace QMC.Common.Cameras
{
    #region CameraSwitch
    public sealed class CameraSwitch
    {
        #region Define
        public class OwnedCameraCollection : Collection<Camera>
        {
            #region Field
            private CameraSwitch m_Owner;
            #endregion

            #region Constructor
            public OwnedCameraCollection(CameraSwitch owner)
            {
            }
            #endregion

            #region Property
            public CameraSwitch Owner
            {
                get { return this.m_Owner; }
                private set { this.m_Owner = value; }
            }
            #endregion

            #region Collection Members
            protected override void ClearItems()
            {
                base.ClearItems();
            }

            protected override void InsertItem(int index, Camera item)
            {
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, Camera item)
            {
                base.SetItem(index, item);
            }
            #endregion
        }
        #endregion

        #region Field
        private const string LogServiceName = "CameraSwitch";

        private OwnedCameraCollection m_Cameras;
        private int m_SelectCameraIndex;
        private bool m_Enable;
        private bool m_IsBeforeChangeCheck;
        #endregion

        #region Event
        public event CameraChangeEventHandler BeforeChange;
        public event CameraChangeEventHandler AfterChange;
        #endregion

        #region Constructor
        public CameraSwitch()
        {
            this.Cameras = new OwnedCameraCollection(this);
            this.SelectCameraIndex = -1;
            this.Enable = true;
            this.IsBeforeChangeCheck = true;
        }
        #endregion

        #region Property
        /// <summary>
        /// CameraSwith가 관리할 Camera들을 가져온다.
        /// </summary>
        public OwnedCameraCollection Cameras
        {
            get { return this.m_Cameras; }
            private set { this.m_Cameras = value; }
        }

        /// <summary>
        /// CameraSwith가 선택한 Camera의 Index를 가져온다.
        /// </summary>
        public int SelectCameraIndex
        {
            get { return this.m_SelectCameraIndex; }
            private set { this.m_SelectCameraIndex = value; }
        }

        /// <summary>
        /// CameraSwith의 상태를 설정하거나 가져온다.
        /// </summary>
        /// <value>true = On , false = Off</value>
        public bool Enable
        {
            get { return this.m_Enable; }
            set
            {
                if (this.m_Enable == value) return;
                //Log.Write(CameraSwitch.LogServiceName, string.Format("Enable : {0} -> {1}", this.m_Enable, value));
                Console.WriteLine(string.Format("Enable : {0} -> {1}", this.m_Enable, value));
                this.m_Enable = value;
            }
        }

        /// <summary>
        /// CameraSwith의 BeforeChange 이벤트 발생여부를 설정하거나 가져온다.
        /// </summary>
        /// <value>true = On , false = Off</value>
        public bool IsBeforeChangeCheck
        {
            get { return this.m_IsBeforeChangeCheck; }
            set
            {
                if (this.m_IsBeforeChangeCheck == value) return;
                //Log.Write(CameraSwitch.LogServiceName, string.Format("BeforeChangeCheck : {0} -> {1}", this.m_IsBeforeChangeCheck, value));
                Console.WriteLine(string.Format("BeforeChangeCheck : {0} -> {1}", this.m_IsBeforeChangeCheck, value));
                this.m_IsBeforeChangeCheck = value;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Change하려는 Camera의 상태를 체크하는 이벤트가 발생합니다.
        /// </summary>
        /// <param name="e"></param>
        private void OnBeforeChange(CameraChangeEventArgs e)
        {
            if (this.IsBeforeChangeCheck == false) return;
            if (this.BeforeChange != null)
                this.BeforeChange(this, e);
        }

        /// <summary>
        /// Change된 Camera의 상태를 알려주는 이벤트가 발생합니다.
        /// </summary>
        /// <param name="e"></param>
        private void OnAfterChange(CameraChangeEventArgs e)
        {
            if (this.AfterChange != null)
                this.AfterChange(this, e);
        }

        #region Change()
        /// <summary>
        /// 선택된 카메라로 변경.
        /// </summary>
        /// <param name="liveOn">True일 경우 카메라의 상태가 StartLive가 된다.</param>
        /// <param name="option">True일 경우 카메라 이벤트를 무조건 실행시킨다.</param>
        /// <returns></returns>
        public int Change(int index, bool? liveOn, bool? option)
        {
            int ret = 0;
            string text = "";
            CameraChangeEventArgs e = null;

            if (this.Enable == false) return ret;

            if (index < 0 || this.Cameras.Count <= index)
            {
                text = string.Format("index does not exist. index = [{0}]", index);
                //return ErrorManager.Register(text);
                Console.WriteLine(text);
                return -1;
            }

            //if (this.Cameras[index].Simulation.IsSimulatedWithoutResource() == true) return ret;

            // 2018.03.30 Option이 True일 경우 조건 생략 후 진행.
            if (option != null && option == false)
            {
                if (liveOn == this.Cameras[index].IsLiveOn)
                {
                    if (this.SelectCameraIndex == index && this.Cameras[index].AutoSleepEnable == true && this.Cameras[index].Sleep == false)
                    {
                        //Log.Write(CameraSwitch.LogServiceName, string.Format("Change() : {0} and {1} equal", this.SelectCameraIndex, index));
                        Console.WriteLine(string.Format("Change() : {0} and {1} equal", this.SelectCameraIndex, index));
                        return ret;
                    }
                }
            }

            e = new CameraChangeEventArgs(this.Cameras[index]);
            this.OnBeforeChange(e);
            if (e.Result != 0) return e.Result;

            this.SelectCameraIndex = index;
            //Log.Write(CameraSwitch.LogServiceName, string.Format("Change() : {0} -> {1}", this.SelectCameraIndex, index));
            Console.WriteLine(string.Format("Change() : {0} -> {1}", this.SelectCameraIndex, index));

            if (liveOn != null)
            {
                if (liveOn == true)
                {
                    if ((ret = this.Cameras[index].StartLive()) != 0) return ret;
                }
                else
                {
                    if ((ret = this.Cameras[index].StopLive()) != 0) return ret;
                }
            }

            e = new CameraChangeEventArgs(this.Cameras[index]);
            this.OnAfterChange(e);
            if (e.Result != 0) return e.Result;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int Change(int index)
        {
            return this.Change(index, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public int Change(Camera camera)
        {
            int ret = 0;
            int index = 0;

            index = this.Cameras.IndexOf(camera);
            if (index < 0 || this.Cameras.Count <= index)
            {
                //return ErrorManager.Register("Camera does not exist.");
                Console.WriteLine("Camera does not exist.");
                return -1;
            }
                

            if ((ret = this.Change(index)) != 0) return ret;

            return ret;
        }

        /// <summary>
        /// 선택된 카메라로 변경.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="liveOn">True일 경우 카메라의 상태가 StartLive가 된다.</param>
        /// <param name="option">True일 경우 카메라 이벤트를 무조건 실행시킨다.</param>
        /// <returns></returns>
        public int Change(Camera camera, bool liveOn, bool option)
        {
            int ret = 0;
            int index = 0;

            index = this.Cameras.IndexOf(camera);
            if (index < 0 || this.Cameras.Count <= index)
            {
                //return ErrorManager.Register("Camera does not exist.");
                Console.WriteLine("Camera does not exist.");
                return -1;
            }
            

            if ((ret = this.Change(index, liveOn, option)) != 0) return ret;

            return ret;
        }
        #endregion

        #region ChangeNext()
        public int ChangeNext(bool liveOn, bool option)
        {
            int index = 0;

            index = this.SelectCameraIndex;
            if (index < 0 || this.Cameras.Count <= index + 1)
                index = 0;
            else
                index += 1;

            return this.Change(index, liveOn, option);
        }
        public int ChangeNext()
        {
            return this.ChangeNext(false, false);
        }
        #endregion

        #region ChangePrevious()
        public int ChangePrevious(bool liveOn, bool option)
        {
            int index = 0;

            index = this.SelectCameraIndex;
            if (index -1 < 0)
                index = this.Cameras.Count - 1;
            else
                index -= 1;

            return this.Change(index, liveOn, option);
        }

        public int ChangePrevious()
        {
            return this.ChangePrevious(false, false);
        }
        #endregion

        #region AllLiveStart()
        public int AllLiveStart()
        {
            int ret = 0;

            for(int i = 0; i < this.Cameras.Count; i++)
            {
                this.Cameras[i].StartLive();
            }

            return ret;
        }
        #endregion

        #region AllLiveStop()
        public int AllLiveStop()
        {
            int ret = 0;

            for (int i = 0; i < this.Cameras.Count; i++)
            {
                this.Cameras[i].StopLive();
            }

            return ret;
        }
        #endregion
        #endregion
    }
    #endregion

    #region CameraChangeEventArgs
    [Serializable]
    public class CameraChangeEventArgs : EquipmentEventArgs
    {
        #region Field
        private Camera m_Camera;
        #endregion

        #region Constructor
        /// <summary>
        /// CameraSwitch의 이벤트 데이터가 들어 있는 클래스를 나타냅니다.
        /// </summary>
        /// <param name="camera"></param>
        /// <exception cref = "CameraNotExist"></exception>
        public CameraChangeEventArgs(Camera camera)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            this.Camera = camera;
        }
        public CameraChangeEventArgs() : this(null) { }
        #endregion

        #region Property
        public Camera Camera
        {
            get { return this.m_Camera; }
            private set { this.m_Camera = value; }
        }
        #endregion
    }

    public delegate void CameraChangeEventHandler(object sender, CameraChangeEventArgs e);
    #endregion
}