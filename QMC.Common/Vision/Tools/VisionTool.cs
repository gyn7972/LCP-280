/*
 * Purpose
 *      Vision에서 특정 목적(Search, Processing)을 위해서 사용하는 행위 객체의 최상위 클래스를 정의한다.
 * 
 * Revision
 *      1. Created: 2017.09.27 by LIM.WT
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.Windows.Forms;


namespace QMC.Common.Vision.Tools
{
    #region VisionTool
    [Serializable]
    public abstract class VisionTool : IDisposable
    {
        #region Field
        [NonSerialized]
        private bool m_Disposed;
        [NonSerialized]
        private bool m_ChekedLicense;
        [NonSerialized]
        private object m_Owner;
        [NonSerialized]
        private VisionImage m_InputImage;
        [NonSerialized]
        private VisionImage m_OutputImage;
        [NonSerialized]
        private VisionResult m_Result;
        private VisionToolCollection m_SubTools;
        private VisionToolParameter m_Parameter;
        private string m_Name;
        #endregion

        #region Constructor
        public VisionTool(string name)
        {
            this.InputImage = new VisionImage();
            this.OutputImage = new VisionImage();
            this.SubTools = new VisionToolCollection();
            this.Name = name;
        }
        public VisionTool() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// VisionTool의 Sub Tool들을 가져오거나 설정한다.
        /// </summary>
        [Browsable(false)]
        public VisionToolCollection SubTools
        {
            get { return this.m_SubTools; }
            set { this.m_SubTools = value; }
        }

        /// <summary>
        /// VisionTool의 Parameter를 가져오거나 설정한다.
        /// </summary>
        public VisionToolParameter Parameter
        {
            get { return this.m_Parameter; }
            set
            {
                if (this.m_Parameter == value) return;
                this.m_Parameter = value;
                this.Parameter.HasChanged = true;
            }
        }

        /// <summary>
        /// VisionTool의 이름을 가져오거나 설정한다.
        /// </summary>
        public string Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }

        /// <summary>
        /// Dispose의 여부를 가져온다.
        /// </summary>
        [Browsable(false)]
        public bool Disposed
        {
            get { return this.m_Disposed; }
            protected set { this.m_Disposed = value; }
        }

        /// <summary>
        /// 영상 처리를 위한 Tool의 License 체크 여부를 가져온다.
        /// </summary>
        [Browsable(false)]
        public bool ChekedLicense
        {
            get { return this.m_ChekedLicense; }
            private set { this.m_ChekedLicense = value; }
        }

        /// <summary>
        /// 영상 처리를 위한 이미지를 가져오거나 설정한다.
        /// </summary>
        [Browsable(false)]
        public VisionImage InputImage
        {
            get { return this.m_InputImage; }
            set { this.m_InputImage = value; }
        }

        /// <summary>
        /// 영상 처리한 이미지를 가져온다.
        /// </summary>
        [Browsable(false)]
        public VisionImage OutputImage
        {
            get { return this.m_OutputImage; }
            protected set { this.m_OutputImage = value; }
        }

        /// <summary>
        /// 영상 처리의 결과를 가져온다.
        /// </summary>
        [Browsable(false)]
        public VisionResult Result
        {
            get { return this.m_Result; }
            protected set { this.m_Result = value; }
        }
        #endregion

        #region Method
        /// <summary>
        /// VisionTool을 실행한다.
        /// </summary>
        /// <returns></returns>
        public int Run()
        {
            int ret = 0;
            VisionToolLog.Write(this, "Start Run()");
            //VisionImage image = null;
            try
            {
                if (this.ChekedLicense == false)
                {
                    if ((ret = this.CheckedLicense()) != 0)
                    {
                        return ret;
                    }
                }

                if ((ret = this.SubToolExecute()) != 0) return ret;

               if (this.Parameter.RepeatCount == 0)
                    this.Parameter.RepeatCount = 1;

                for (int i = 0; i < this.Parameter.RepeatCount; i++)
                {
                    if ((ret = this.OnRun()) != 0) return ret;

                    if (i == this.Parameter.RepeatCount - 1) continue;
                    this.InputImage = this.OutputImage;
                }
            }
            catch (Exception ex)
            {
                //if (SafeThread.IsThreadInterrupted(ex) == true)
                //    throw ex;
                this.Result.ErrorCode = ex.HResult;
                this.Result.ResultMessage = ex.Message;
                VisionToolLog.Write(this, string.Format("[Fail]ErrorMessage : {0}", ex.Message));
                return -1;
            }

            VisionToolLog.Write(this, "End Run()");

            return ret;
        }
        protected abstract int OnRun();

        /// <summary>
        /// SubVisionTool들을 실행시킨다.
        /// </summary>
        /// <returns></returns>
        public int SubToolExecute()
        {
            int ret = 0;

            this.Result.SubVisionResults.Clear();

            if (this.SubTools == null) return ret;

            VisionToolLog.Write(this, "SubTool Start Execute()");

            try
            {
                if (this.SubTools.InputImage != null && this.SubTools.Count == 0)
                {
                    this.SubTools.OutputImage = this.SubTools.InputImage;
                    return ret;
                }
                else if (this.SubTools.InputImage != null && this.SubTools.Count != 0)
                {
                    this.SubTools[0].InputImage = this.SubTools.InputImage;
                }
                else if (this.SubTools.InputImage == null && this.SubTools.Count == 0) return ret;

                for (int i = 0; i < this.SubTools.Count; i++)
                {
                    if ((ret = this.SubTools[i].Run()) != 0) return ret;
                    this.Result.SubVisionResults.Add(this.SubTools[i].Result);
                    if (i == this.SubTools.Count - 1) continue;
                    this.SubTools[i + 1].InputImage = this.SubTools[i].OutputImage;
                }

                this.SubTools.OutputImage = this.SubTools[this.SubTools.Count - 1].OutputImage;
            }
            finally
            {
                VisionToolLog.Write(this, "SubTool End Execute()");
            }

            return ret;
        }

        /// <summary>
        /// VisionTool의 License를 체크한다.
        /// </summary>
        /// <returns></returns>
        public int CheckedLicense()
        {
            int ret = 0;

            if ((ret = this.OnCheckedLicense()) != 0)
            {
                this.ChekedLicense = false;
                return ret;
            }

            this.ChekedLicense = true;
            return ret;
        }
        protected abstract int OnCheckedLicense();

        public virtual int Prepare()
        {
            this.Parameter.HasChanged = true;
            return this.OnPrepare();
        }

        protected abstract int OnPrepare();
        #endregion

        #region IDisposable Member
        /// <summary>
        /// 관리되지 않는 리소스의 확보, 해제 또는 다시 설정과 관련된 응용 프로그램 정의 작업을 수행합니다.
        /// </summary>
        public void Dispose()
        {
            if (this.Disposed == false)
            {
                this.OnDispose();

                this.Disposed = true;
            }
        }
        protected abstract void OnDispose();
        #endregion

        #region IOwned Members
        /// <summary>
        /// VisionTool의 Owner를 가져온다.
        /// </summary>
        [Browsable(false)]
        public object Owner
        {
            get { return this.m_Owner; }
            internal set { this.m_Owner = value; }
        }
        #endregion
    }
    #endregion

    #region VisionToolParameter
    [Serializable]
    [TypeConverter(typeof(VisionToolParameterConverter))]
    public class VisionToolParameter
    {
        #region Field
        [NonSerialized]
        private bool m_HasChanged;
        private int m_RepeatCount;
        #endregion

        #region Constructor
        public VisionToolParameter()
        {
            this.RepeatCount = 1;
        }
        #endregion

        #region Property
        public bool HasChanged
        {
            get { return this.m_HasChanged; }
            set { this.m_HasChanged = value; }
        }

        public int RepeatCount
        {
            get { return this.m_RepeatCount; }
            set { this.m_RepeatCount = value; }
        }
        #endregion

        #region Method
        public new string ToString()
        {
            StringBuilder parameter = new StringBuilder();

            FieldInfo[] parameterInfo = this.GetType().GetFields();

            foreach (FieldInfo info in parameterInfo)
            {
                parameter.AppendLine(info.ToString());
            }

            return parameter.ToString();
        }
        #endregion
    }

    internal class VisionToolParameterConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(String))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(String))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is VisionToolParameter)
            {
                VisionToolParameter specialized = value as VisionToolParameter;

                return string.Empty;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    
    #endregion

    #region VisionToolCollection
    [Serializable]
    public class VisionToolCollection : Collection<VisionTool>
    {
        #region Field
        [NonSerialized]
        private VisionImage m_InputImage;
        [NonSerialized]
        private VisionImage m_OutputImage;
        #endregion

        #region Constructor
        public VisionToolCollection(IList<VisionTool> tools) : base(tools)
        {
            this.InputImage = new VisionImage();
            this.OutputImage = new VisionImage();
        }
        public VisionToolCollection() : base()
        {
            this.InputImage = new VisionImage();
            this.OutputImage = new VisionImage();
        }
        #endregion

        #region Property
        public VisionTool First
        {
            get
            {
                if (this.Count == 0) return null;
                return this[0];
            }
        }

        public VisionTool Last
        {
            get
            {
                if (this.Count == 0) return null;
                return this[this.Count - 1];
            }
        }

        public VisionImage InputImage
        {
            get { return this.m_InputImage; }
            set { this.m_InputImage = value; }
        }

        public VisionImage OutputImage
        {
            get { return this.m_OutputImage; }
            set { this.m_OutputImage = value; }
        }
        #endregion

        #region Method
        public VisionTool this[string name]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Name == name)
                        return this[i];
                }
                return null;
            }
        }

        public VisionTool this[Enum name]
        {
            get { return this[name.ToString()]; }
        }

        public VisionTool GetLastTool()
        {
            return this[this.Count - 1];
        }
        #endregion

        #region Collection<VisionTool> Members
        public bool Contains(Enum name)
        {
            return this.Contains(name.ToString());
        }

        public bool Contains(string name)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Name == name)
                    return true;
            }
            return false;
        }
        #endregion
    }
    #endregion

    #region VisionToolReadOnlyCollection
    [Serializable]
    public class VisionToolReadOnlyCollection : ReadOnlyCollection<VisionTool>
    {
        #region Constructor
        public VisionToolReadOnlyCollection(IList<VisionTool> tools) : base(tools)
        {
        }
        public VisionToolReadOnlyCollection(VisionTool tool) : this(new VisionTool[] { tool })
        {
        }
        public VisionToolReadOnlyCollection() : this(new VisionTool[0]) { }
        #endregion
    }
    #endregion

    #region VisionToolLog
    public static class VisionToolLog
    {
        #region Define
        [Serializable]
        public enum Library
        {
            None,
            EureSys,
            Cognex,
        }
        private const string FileName = "VisionTool";
        #endregion

        #region Method
        public static void Write(VisionTool tool, Library library, string message)
        {
            //Log.Write(VisionToolLog.FileName, string.Format("{0}_{1} -> {2}", library, tool.Name, message));
            //Console.WriteLine(string.Format("{0}_{1} -> {2}", library, tool.Name, message));
        }
        public static void Write(VisionTool tool, string message)
        {
            //Log.Write(VisionToolLog.FileName, string.Format("{0} -> {1}", tool.Name, message));
            //Console.WriteLine(string.Format("{0} -> {1}", tool.Name, message));
        }
        //public static void Write(VisionToolExecutor executer, string message)
        //{
        //    Log.Write(VisionToolLog.FileName, string.Format("{0} -> {1}", executer.Name, message));
        //}
        #endregion
    }
    #endregion
}