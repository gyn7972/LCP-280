using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]
    public class ListParam
    {
        protected Dictionary<string, ParamGroup> m_dicParams;
        public ListParam()
        {
            Init();
        }
        private void Init()
        {
            if(m_dicParams == null)
            {
                m_dicParams = new Dictionary<string, ParamGroup>();
            }
            m_dicParams.Clear();
        }

        public void AddParam(Param param)
        {
            if(m_dicParams.ContainsKey(param.Group))
            {
                m_dicParams[param.Group].AddParam(param);
            }
            else
            {
                ParamGroup group = new ParamGroup(param.Group);
                group.AddParam(param);
                m_dicParams.Add(param.Group, group);
            }
        }
        
        public List<string> GetGroupList()
        {
            return m_dicParams.Keys.ToList();
        }

        public void SetGroup(ParamGroup group)
        {
            if (group == null)
            {

                return;
            }

            string strGroup = group.Name;
            if (m_dicParams.ContainsKey(strGroup))
            {
                m_dicParams[strGroup] = group;
            }
            else
            {
                m_dicParams.Add(strGroup, group);
            }
        }
        public ParamGroup GetGroup(string strGroup)
        {
            ParamGroup paramGroup = null;

            if(m_dicParams.ContainsKey(strGroup))
            {
                paramGroup = m_dicParams[strGroup];
            }

            return paramGroup;
        }

        public ParamGroup GetGroup(int nIndex = 0)
        {
            if(m_dicParams.Keys.Count > 0)
            {
                string strKey = m_dicParams.Keys.ElementAt(nIndex);
                return GetGroup(strKey);
            }

            return null;
        }

        public List<Param> GetParams()
        {
            List<Param> list = new List<Param>();

            foreach(ParamGroup group in m_dicParams.Values)
            {
                foreach(var param in group.GetListParam())
                {
                    list.Add(param);
                }
            }

            return list;
        }

        public int Count()
        {
            return this.m_dicParams.Count();
        }
    }
}
