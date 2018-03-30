using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public class GraphRead : IComparable<GraphRead>
    {
        private double mValue;
        public double Value => mValue;
        private double mTime;
        public double Time => mTime;

        public GraphRead(double pValue, double pTime)
        {
            mValue = pValue;
            mTime = pTime;
        }

        public int CompareTo(GraphRead other)
        {
            return mTime.CompareTo(other.Time);
        }

        public override string ToString()
        {
            return $"{mTime.ToString("N3")}, {mValue.ToString("N3")}";
        }
    }
}
