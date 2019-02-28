using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public class MultiSetInterruptionCycle : InterruptionCycle
    {
        private RectifierSet[] mSets;
        public override RectifierSet[] Sets => mSets;
        private TimeSpan mLength;
        public override TimeSpan Length => mLength;

        public readonly int NumSets;
        public readonly double On;
        public readonly double Off;
        public readonly double Delay;

        public MultiSetInterruptionCycle(string pName, double pOn, double pOff, double pDelay, string[] pSetNames)
        {
            Name = pName;
            NumSets = pSetNames.Length;
            On = pOn;
            Off = pOff;
            Delay = pDelay;
            mSets = new RectifierSet[pSetNames.Length + 2];
            mSets[0] = new RectifierSet("All On");
            for (int i = 0; i < pSetNames.Length; ++i)
            {
                mSets[i + 1] = new RectifierSet(pSetNames[i]);
            }
            mSets[mSets.Length - 1] = new RectifierSet("All Off");
            long length = (long)(On + Off + (Off + Delay) * NumSets);
            mLength = new TimeSpan(length * TimeSpan.TicksPerSecond);
        }

        public override DateTime GetCycleStart(DateTime pCurrentTime)
        {
            double curSet = (((pCurrentTime.Ticks) % (TimeSpan.TicksPerDay * 365) % mLength.Ticks)) / TimeSpan.TicksPerSecond * -1 + Off * OFFSET;
            if (curSet > 0)
                curSet -= mLength.Ticks / TimeSpan.TicksPerSecond;
            return pCurrentTime.AddSeconds(curSet);
        }

        public override int GetSetPosition(DateTime pCurrentTime)
        {
            if (pCurrentTime == null)
                return 0;

            // How many seconds we are into the cycle.
            double curSet = ((double)(pCurrentTime.Ticks % (TimeSpan.TicksPerDay * 365) % mLength.Ticks)) / TimeSpan.TicksPerSecond;
            double setLength = Delay + Off;

            if (curSet <= Off)
            {
                return NumSets + 1; // We are in the all off portion of the cycle.
            }
            curSet -= Off; // Shifting our timeframe down so we are always working with 0 being our next part of the cycle to check.
            if (curSet <= On)
            {
                return 0; // We are at the all on portion of the cycle.
            }
            curSet -= On; // Shifting again.

            // We are somewhere in the cycle where we are alternating all on and a single set off.
            int set = (int)Math.Floor(curSet / setLength) + 1; // This is the set we are on. But we could be in the all on portion.
            if (curSet % setLength < Off) // So lets check if we are in the set off portion.
            {
                return set;
            }
            return 0; // And if not we are in the all on.
        }

        public override double GetGraphTime(DateTime pGraphStart, DateTime pCurrentTime, int pNumCycles)
        {
            double diffFromStart = (double)(pCurrentTime.Ticks - GetCycleStart(pGraphStart).Ticks) / TimeSpan.TicksPerSecond;
            if (diffFromStart < 0)
                diffFromStart += ((double)mLength.Ticks / TimeSpan.TicksPerSecond);
            return diffFromStart % (pNumCycles * ((double)mLength.Ticks / TimeSpan.TicksPerSecond));
        }
    }
}
