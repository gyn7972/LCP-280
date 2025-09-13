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
        public Material()
        {
        }

        public MaterialPresence Presence = MaterialPresence.Unknown;
        public string Name { get; set; } = string.Empty;
        public DateTime ArrivedTime { get; set; } = DateTime.MinValue;
        public object Tag { get; set; } = null;
    }
}
