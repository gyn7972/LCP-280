using System;
using System.Collections.Generic;
using QMC.Common.Vision;

namespace QMC.LCP_280.Process.Component
{
    public class PatternMatchInfo
    {
        public double X;
        public double Y;
        public double AngleDeg;
        public double Score;
        public int TrainW;
        public int TrainH;
    }

    public class PatternMarksFoundEventArgs : EventArgs
    {
        public bool Suspended;
        public VisionImage Image { get; set; }
        public List<PatternMatchInfo> Marks { get; set; } = new List<PatternMatchInfo>();
        public int RepresentativeIndex { get; set; } = -1;
    }

    public interface IPatternMarkSource
    {
        event EventHandler<PatternMarksFoundEventArgs> MarksFound;
    }
}