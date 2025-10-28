using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]
    public enum ParamIlluminationDataSetKey
    {
        Name,
        Values,
    }
    [Serializable]
    public class IlluminationDataSet 
    {
        public string Name { set; get; }
        public List<IlluminationChannel> Values { set; get; }
        public IlluminationDataSet(string strName)
        {
            Name = strName;
            Values = new List<IlluminationChannel>();
        }

        public void SetIlluminationChannel(List<IlluminationChannel> channels)
        {
            List<IlluminationChannel> newValues = new List<IlluminationChannel>();
            foreach(IlluminationChannel channel in channels)
            {
                IlluminationChannel origin = GetValue(channel.Channel);
                if(origin == null)
                {
                    origin = new IlluminationChannel(channel.ChannelName);
                }
                origin.ChannelName = channel.ChannelName;
                origin.Channel = channel.Channel;
                origin.Min = channel.Min;
                origin.Max = channel.Max;

                newValues.Add(origin);
            }

            Values = newValues;
        }

        public void DeleteData(int nIndex)
        {
            Values.RemoveAt(nIndex);
        }
        public IlluminationChannel GetValue(int nChannel)
        {
            IlluminationChannel ret = null;
            foreach (IlluminationChannel channel in Values)
            {
                if(channel.Channel == nChannel)
                {
                    ret = channel;
                    break;
                }
            }

            return ret;
        }

        public IlluminationChannel GetAt(int index)
        {
            IlluminationChannel ret = null;
            if(index >=0 && index < Values.Count)
            {
                ret = Values[index];
            }
            return ret;
        }

        public IlluminationDataList ToList()
        {
            IlluminationDataList list = new IlluminationDataList();
            list.Add(this);
            return list; 
        }
    }
    [Serializable]
    public class IlluminationDataList : List<IlluminationDataSet>
    {
        public IlluminationDataList GetIlluminationDatas(string strName)
        {
            IlluminationDataList listResult = new IlluminationDataList();
            foreach(IlluminationDataSet illuminationData in this)
            {
                if(illuminationData.Name == strName)
                {
                    listResult.Add(illuminationData);
                }
            }

            return listResult;
        }

        public IlluminationDataSet GetIlluminationDataSet(string strName)
        {
            IlluminationDataSet dataSet = null;
            foreach (IlluminationDataSet illuminationData in this)
            {
                if (illuminationData.Name == strName)
                {
                    dataSet = illuminationData;
                    break;
                }
            }

            return dataSet;
        }

        public void DeepCopy(IlluminationDataList illuminationDatas)
        {
            Clear();
            foreach(IlluminationDataSet data in this)
            {
                illuminationDatas.Add(data);
            }
        }
        public void SetIlluminationChannel(List<IlluminationChannel> illuminationChannels)
        {
            if(Count == 0)
            {
                this.Add(new IlluminationDataSet("Empty"));
            }
            foreach(IlluminationDataSet dataSet in this)
            {
                if(dataSet != null)
                    dataSet.SetIlluminationChannel(illuminationChannels);                
            }
        }
    }
}
