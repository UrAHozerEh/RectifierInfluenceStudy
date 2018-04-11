using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public class Read : IComparable<Read>
    {
        private float mValue;
        public float Value => mValue;
        private DateTime mUTCTime;
        public DateTime UTCTime => mUTCTime;

        public Read(float pValue, DateTime pUTCTime)
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
