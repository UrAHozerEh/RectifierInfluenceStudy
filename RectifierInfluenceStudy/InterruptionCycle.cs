using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public abstract class InterruptionCycle
    {
        public string Name { get; set; }
        public abstract RectifierSet[] Sets { get; }
        public abstract TimeSpan Length { get; }

        public abstract DateTime GetNextCycleStart(DateTime pCurrentTime);
        public abstract DateTime GetCycleStart(DateTime pCurrentTime);
        public abstract DateTime GetCycleEnd(DateTime pCurrentTime);

        public double GetPercentComplete(DateTime pStart, DateTime pCurrentTime)
        {
            TimeSpan span = pCurrentTime - pStart;
            return span.Ticks / Length.Ticks;
        }
    }
}