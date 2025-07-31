using System;

namespace QMC.Common
{
    public class AlarmInfo
    {
        public int Id { get; }
        public string Message { get; }
        public DateTime OccurredTime { get; }
        public bool IsActive { get; internal set; }

        public AlarmInfo(int id, string message)
        {
            Id = id;
            Message = message;
            OccurredTime = DateTime.Now;
            IsActive = true;
        }
    }
}