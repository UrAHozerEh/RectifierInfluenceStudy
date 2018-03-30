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

        private int mNumSets;
        private double mOn;
        private double mOff;
        private double mDelay;

        public MultiSetInterruptionCycle(string pName, double pOn, double pOff, double pDelay, string[] pSetNames)
        {
            Name = pName;
            mNumSets = pSetNames.Length;
            mOn = pOn;
            mOff = pOff;
            mDelay = pDelay;
            mSets = new RectifierSet[pSetNames.Length + 2];
            mSets[0] = new RectifierSet("All On");
            for (int i = 0; i < pSetNames.Length; ++i)
            {
                mSets[i + 1] = new RectifierSet(pSetNames[i]);
            }
            mSets[mSets.Length - 1] = new RectifierSet("All Off");
            long length = (long)(mOn + mOff + (mOff + mDelay) * mNumSets);
            mLength = new TimeSpan(length * TimeSpan.TicksPerSecond);
        }

        public override DateTime GetCycleStart(DateTime pCurrentTime)
        {
            double curSet = (((pCurrentTime.Ticks - mOff * OFFSET) % (TimeSpan.TicksPerDay * 365) % mLength.Ticks)) / TimeSpan.TicksPerSecond * -1;
            return pCurrentTime.AddSeconds(curSet);
        }

        public override int GetSetPosition(DateTime pCurrentTime)
        {
            if (pCurrentTime == null)
                return 0;

            // How many seconds we are into the cycle.
            double curSet = ((double)(pCurrentTime.Ticks % (TimeSpan.TicksPerDay * 365) % mLength.Ticks)) / TimeSpan.TicksPerSecond;
            double setLength = mDelay + mOff;

            if (curSet <= mOff)
            {
                return mNumSets + 1; // We are in the all off portion of the cycle.
            }
            curSet -= mOff; // Shifting our timeframe down so we are always working with 0 being our next part of the cycle to check.
            if (curSet <= mOn)
            {
                return 0; // We are at the all on portion of the cycle.
            }
            curSet -= mOn; // Shifting again.

            // We are somewhere in the cycle where we are alternating all on and a single set off.
            int set = (int)Math.Floor(curSet / setLength) + 1; // This is the set we are on. But we could be in the all on portion.
            if (curSet % setLength < mOff) // So lets check if we are in the set off portion.
            {
                return set;
            }
            return 0; // And if not we are in the all on.
        }

        public override double GetGraphTime(DateTime pGraphStart, DateTime pCurrentTime, int pNumCycles)
        {
            double diffFromStart = (double)(pCurrentTime.Ticks - GetCycleStart(pGraphStart).Ticks) / TimeSpan.TicksPerSecond;
            return diffFromStart % (pNumCycles * ((double)mLength.Ticks / TimeSpan.TicksPerSecond));
        }
    }
}
