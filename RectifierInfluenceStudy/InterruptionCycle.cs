using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public abstract class InterruptionCycle
    {
        protected static double OFFSET = 1.2;
        public string Name { get; set; }
        public abstract RectifierSet[] Sets { get; }
        public abstract TimeSpan Length { get; }

        public abstract int GetSetPosition(DateTime pCurrentTime);
        public DateTime GetNextCycleStart(DateTime pCurrentTime)
        {
            return GetCycleStart(pCurrentTime).Add(Length);
        }
        public abstract DateTime GetCycleStart(DateTime pCurrentTime);
        public DateTime GetCycleEnd(DateTime pCurrentTime)
        {
            return GetCycleStart(pCurrentTime).Add(Length);
        }
        public DateTime GetNextCycleEnd(DateTime pCurrentTime)
        {
            return GetNextCycleStart(pCurrentTime).Add(Length);
        }
        public double GetPercentComplete(DateTime pStart, DateTime pCurrentTime)
        {
            TimeSpan span = pCurrentTime - pStart;
            return span.Ticks / Length.Ticks;
        }
        public abstract double GetGraphTime(DateTime pGraphStart, DateTime pCurrentTime, int pNumCycles);
    }
}