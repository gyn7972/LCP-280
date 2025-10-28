using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QMC.Common;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace QMC.Common.Alarm
{
    [Serializable]
    public class AlarmInfo
    {
        public enum AlarmType
        {
            Error,
            Inform,
            Warning
        }
        private Image m_StateImage;
        public string Title { get; set; }
        public string Grade { get; set; }
        public Image StateImage
        {
            get 
            {
                
                return m_StateImage; }
            set { m_StateImage = value; }
        }
        public string Source { get; set; }
        public string PartServiceState { get; }
        public int Code { get; set; }
        public string Cause { get; set; }

        public DateTime GeneratedTime { get; set; }



        public AlarmInfo()
        {
            SetDefaultValues();
        }


        public void SetDefaultValues()
        {
            Title = "";
            Grade = AlarmType.Error.ToString();
            Source = "";
            Code = 0;
            Cause = "";
            GeneratedTime = DateTime.Now;
            StateImage = null;
        }
       

    }

    [Serializable]
    public class AlarmCollection : Collection<AlarmInfo>
    {

    }
}

