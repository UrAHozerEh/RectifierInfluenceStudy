using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public class SingleSetInterruptionCycle : InterruptionCycle
    {
        private RectifierSet[] mCycles;
        public override RectifierSet[] Sets => mCycles;

        public override TimeSpan Length => throw new NotImplementedException();

        public SingleSetInterruptionCycle(string pName, double pTimeOn, double pTimeOff)
        {
            Name = pName;
            mCycles = new RectifierSet[] { new RectifierSet("All On"), new RectifierSet("All Off") };

        }

        public override DateTime GetNextCycleStart(DateTime pCurrentTime)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetCycleStart(DateTime pCurrentTime)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetCycleEnd(DateTime pCurrentTime)
        {
            throw new NotImplementedException();
        }
    }
}
