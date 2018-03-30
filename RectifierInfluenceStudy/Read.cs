using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public class Read : IComparable<Read>
    {
        private double mValue;
        public double Value => mValue;
        private DateTime mUTCTime;
        public DateTime UTCTime => mUTCTime;

        public Read(double pValue, DateTime pUTCTime)
        {
            mValue = pValue;
            mUTCTime = pUTCTime;
        }

        public int CompareTo(Read other)
        {
            return mUTCTime.CompareTo(other.UTCTime);
        }
    }
}
