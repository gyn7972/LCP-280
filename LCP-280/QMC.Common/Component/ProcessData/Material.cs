using System;

namespace QMC.Common
{
    public class Material
    {
        public enum MaterialPresence : byte
        {
            Unknown = byte.MaxValue,
            NotExist = 0,
            Exist = 1
        }
        public enum MaterialProcessSatate : byte
        {
            Unknown = byte.MaxValue,
            Ready = 0,
            Processing = 1,
            Completed = 2,
        }
        public Material()
        {
        }

        public MaterialPresence Presence = MaterialPresence.Unknown;

        public MaterialProcessSatate ProcessSatate { get; set; } = MaterialProcessSatate.Unknown;

        public string Name { get; set; } = string.Empty;
        public DateTime ArrivedTime { get; set; } = DateTime.MinValue;
        public object Tag { get; set; } = null;
    }
}
