/*
 * Purpose
 *      Vision Processing에 대한 결과를 저장하는 객체를 정의한다.
 *      
 * Revision
 *      1. Created: 2017.09.27 by LIM.WT
 * 
 * 
 */

using System;
using System.Collections.ObjectModel;

namespace QMC.Common.Vision.Tools
{
    #region VisionResult
    [Serializable]
    public class VisionResult
    {
        #region Field
        private int m_ErrorCode;
        private string m_ResultMessage;
        private double m_ProcessingTime;
        private string m_Owner;
        [NonSerialized]
        private VisionResultKeyedCollection m_SubVisionResults;
        #endregion

        #region Constructor
        public VisionResult(string owner)
        {
            this.ProcessingTime = 0.0;
            this.Owner = owner;
            this.SubVisionResults = new VisionResultKeyedCollection();
        }
        public VisionResult()
        {
            this.ProcessingTime = 0.0;
            //this.Owner = owner;
            this.SubVisionResults = new VisionResultKeyedCollection();
        }
        #endregion

        #region Property
        /// <summary>
        /// Error에 코드를 가져오거나 설정한다.
        /// </summary>
        /// <value>에러가 없는 경우는 0의 값을 가지며 있는 0이 아닌 값을 가지며 ErrorManager.GetByUid()를 호출하여 자세한 내용인 있는 Error 객체를 얻을 수 있다.</value>
        public int ErrorCode
        {
            get { return this.m_ErrorCode; }
            set { this.m_ErrorCode = value; }
        }

        /// <summary>
        /// 결과에 대한 Message를 가져오거나 설정한다.
        /// </summary>
        /// <value>에러가 없는 경우는 Accept의 값을 가지며, 있을 경우는 에러에 대한 Message를 가진다.</value>
        public string ResultMessage
        {
            get { return this.m_ResultMessage; }
            set { this.m_ResultMessage = value; }
        }

        /// <summary>
        /// VisionTool이 Processing된 시간을 가져오거나 설정한다.
        /// </summary>
        public double ProcessingTime
        {
            get { return this.m_ProcessingTime; }
            set { this.m_ProcessingTime = value; }
        }

        public string Owner
        {
            get { return this.m_Owner; }
            private set { this.m_Owner = value; }
        }

        public VisionResultKeyedCollection SubVisionResults
        {
            get { return this.m_SubVisionResults; }
            private set { this.m_SubVisionResults = value; }
        }
        #endregion
    }
    #endregion

    #region VisionResultKeyedCollection
    [Serializable]
    public class VisionResultKeyedCollection : KeyedCollection<string, VisionResult>
    {
        #region Method
        public VisionResult this[Enum key]
        {
            get { return base[key.ToString()]; }
        }

        public VisionResult GetLastResult()
        {
            if (this.Count < 1) return null;
            return this[this.Count - 1];
        }
        #endregion

        #region KeyedCollection Members
        protected override string GetKeyForItem(VisionResult item)
        {
            return item.Owner;
        }
        #endregion
    }
    #endregion
}