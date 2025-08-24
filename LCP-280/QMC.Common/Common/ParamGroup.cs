using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]
    public class ParamGroup
    {
        protected List<Param> m_ListParams;
        protected List<ParamGroup> m_ListGroups;
        public string Name { get; set; }
        public ParamGroup() : this("")
        {

        }
        public ParamGroup(string strGroupName)
        {
            Name = strGroupName;
            if(m_ListParams == null)
            {
                m_ListParams = new List<Param>();
            }

            if(m_ListGroups == null)
            {
                m_ListGroups = new List<ParamGroup>();
            }
        }

        public void AddParam(Param param)
        {
            m_ListParams.Add(param);
            param.Index = m_ListParams.Count - 1;
        }

        public List<Param> GetListParam()
        {
            return m_ListParams;
        }

        public Param GetParam(int index)
        {
            Param param = null;

            if(index >= 0 && index < m_ListParams.Count)
            {
                param = m_ListParams[index];
            }

            return param;
        }

        public int GetParamCount()
        {
            return m_ListParams.Count;
        }

        public void AddGroup(ParamGroup group)
        {
            m_ListGroups.Add(group);
        }

        public List<ParamGroup> GetListGroup()
        {
            return m_ListGroups;
        }

        public ParamGroup GetGroup(int index)
        {
            ParamGroup group = null;

            if (index >= 0 && index < m_ListGroups.Count)
            {
                group = m_ListGroups[index];
            }

            return group;
        }

        public ParamGroup GetGroup(string strGroupName)
        {
            ParamGroup group = null;

            foreach(ParamGroup g in m_ListGroups)
            {
                if(g.Name == strGroupName)
                {
                    group = g;
                    break;
                }
            }

            return group;
        }

        public ListParam ToListParam()
        {
            ListParam param = new ListParam();
            foreach(ParamGroup group in m_ListGroups)
            {
                param.SetGroup(group);
            }

            return param;
        }

        public void SetGroup(ParamGroup paramGroup)
        {
            bool bModify = false;
            for(int iter = 0; iter < m_ListGroups.Count; iter++)
            {
                ParamGroup group = m_ListGroups[iter];
                if (group.Name == paramGroup.Name)
                {
                    m_ListGroups[iter] = paramGroup;
                    bModify = true;
                    break;
                }
            }

            if(!bModify)
            {
                m_ListGroups.Add(paramGroup);
            }
        }
        public void ClearGroup()
        {
            m_ListGroups.Clear();
        }

        public int GetGroupCount()
        {
            return m_ListGroups.Count;
        }
    }
}
