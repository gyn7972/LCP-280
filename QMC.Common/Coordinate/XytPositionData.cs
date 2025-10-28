using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]
    public enum TargetType
    {
        Base,
        Offset,
    }


    [Serializable]
    public class XyzLDzzxzULzzxzPositionData
    {
        protected XyzLDzzxzULzzxzCoordinate m_coordinate;
        public string Name { get; set; }
        public TargetType Type { set; get; }
        [Browsable(false)]
        public XyzLDzzxzULzzxzCoordinate Coordinate
        {
            set
            {
                m_coordinate = value;
            }
            get
            {
                return m_coordinate;
            }
        }
        public double X
        {
            set
            {
                m_coordinate.X = value;
            }
            get
            {
                return m_coordinate.X;
            }
        }
        public double Y
        {
            set
            {
                m_coordinate.Y = value;
            }
            get
            {
                return m_coordinate.Y;
            }
        }
        public double Z
        {
            set
            {
                m_coordinate.Z = value;
            }
            get
            {
                return m_coordinate.Z;
            }
        }
        public double MASK_Y
        {
            set
            {
                m_coordinate.Z = value;
            }
            get
            {
                return m_coordinate.Z;
            }
        }
        public double LD_SZ0
        {
            set
            {
                m_coordinate.LD_SZ0 = value;
            }
            get
            {
                return m_coordinate.LD_SZ0;
            }
        }
        public double LD_SZ1
        {
            set
            {
                m_coordinate.LD_SZ1 = value;
            }
            get
            {
                return m_coordinate.LD_SZ1;
            }
        }
        public double LD_TRX
        {
            set
            {
                m_coordinate.LD_TRX = value;
            }
            get
            {
                return m_coordinate.LD_TRX;
            }
        }
        public double LD_TRZ
        {
            set
            {
                m_coordinate.LD_TRZ = value;
            }
            get
            {
                return m_coordinate.LD_TRZ;
            }
        }
        public double UL_SZ0
        {
            set
            {
                m_coordinate.UL_SZ0 = value;
            }
            get
            {
                return m_coordinate.UL_SZ0;
            }
        }
        public double UL_SZ1
        {
            set
            {
                m_coordinate.UL_SZ1 = value;
            }
            get
            {
                return m_coordinate.UL_SZ1;
            }
        }
        public double UL_TRX
        {
            set
            {
                m_coordinate.UL_TRX = value;
            }
            get
            {
                return m_coordinate.UL_TRX;
            }
        }
        public double UL_TRZ
        {
            set
            {
                m_coordinate.UL_TRZ = value;
            }
            get
            {
                return m_coordinate.UL_TRZ;
            }
        }


        public XyzLDzzxzULzzxzPositionData()
        {
            Name = "";
            Type = TargetType.Base;
            Coordinate = new XyzLDzzxzULzzxzCoordinate();
        }

        public void SetData(SettingParameterCollection parameters)
        {
            if (m_coordinate == null)
                m_coordinate = new XyzLDzzxzULzzxzCoordinate();
            if (parameters != null && parameters.Count >= 7)
            {
                m_coordinate.X = parameters[0].DoubleValue;
                m_coordinate.Y = parameters[1].DoubleValue;
                m_coordinate.Z = parameters[2].DoubleValue;
                m_coordinate.LD_SZ0 = parameters[3].DoubleValue;
                m_coordinate.LD_SZ1 = parameters[4].DoubleValue;
                m_coordinate.LD_TRX = parameters[5].DoubleValue;
                m_coordinate.LD_TRZ = parameters[6].DoubleValue;
                m_coordinate.UL_SZ0 = parameters[7].DoubleValue;
                m_coordinate.UL_SZ1 = parameters[8].DoubleValue;
                m_coordinate.UL_TRX = parameters[9].DoubleValue;
                m_coordinate.UL_TRZ = parameters[10].DoubleValue;
            }


        }
    }
    [Serializable]
    public class XyzLDzzxzULzzxzPositionDataCollection : Collection<XyzLDzzxzULzzxzPositionData>
    {
        public List<string> GetPositionList()
        {
            List<string> list = new List<string>();
            foreach (XyzLDzzxzULzzxzPositionData data in this)
            {
                if (data.Type == TargetType.Base)
                {
                    list.Add(data.Name);
                }
            }
            return list;
        }

        public List<XyzLDzzxzULzzxzPositionData> GetPositionDatas(string strName)
        {
            List<XyzLDzzxzULzzxzPositionData> list = new List<XyzLDzzxzULzzxzPositionData>();
            foreach (XyzLDzzxzULzzxzPositionData data in this)
            {
                if (data.Name == strName)
                {
                    list.Add(data);
                }
            }
            return list;
        }

        public XyzLDzzxzULzzxzCoordinate GetPositionCoordinate(string strName)
        {
            XyzLDzzxzULzzxzCoordinate coord = new XyzLDzzxzULzzxzCoordinate();
            List<XyzLDzzxzULzzxzPositionData> list = GetPositionDatas(strName);
            if (list != null && list.Count > 0)
            {
                foreach (XyzLDzzxzULzzxzPositionData data in list)
                {
                    coord += data.Coordinate;
                }
            }
            return coord;
        }

        //public XyCoordinate GetPositionCoordinate_UVW(string strName)
        //{
        //    XyCoordinate coord = new XyCoordinate();
        //    List<XyzLDzzxzULzzxzPositionData> list = GetPositionDatas(strName);
        //    if (list != null && list.Count > 0)
        //    {
        //        foreach (XyzLDzzxzULzzxzPositionData data in list)
        //        {
        //            coord.X += data.Coordinate.U;
        //            coord.Y += data.Coordinate.V;
        //        }
        //    }
        //    return coord;
        //}

        public XyCoordinate GetPositionCoordinate_XY(string strName)
        {
            XyCoordinate coord = new XyCoordinate();
            List<XyzLDzzxzULzzxzPositionData> list = GetPositionDatas(strName);
            if (list != null && list.Count > 0)
            {
                foreach (XyzLDzzxzULzzxzPositionData data in list)
                {
                    coord.X += data.Coordinate.X;
                    coord.Y += data.Coordinate.Y;
                }
            }
            return coord;
        }
    }
}
