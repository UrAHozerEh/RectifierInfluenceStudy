using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public class MultiSetInterruptionCycle : InterruptionCycle
    {
        private RectifierSet[] mCycles;
        public override RectifierSet[] Sets => mCycles;
        public override TimeSpan Length => throw new NotImplementedException();

        public override DateTime GetCycleEnd(DateTime pCurrentTime)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetCycleStart(DateTime pCurrentTime)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetNextCycleStart(DateTime pCurrentTime)
        {
            throw new NotImplementedException();
        }
    }
}
