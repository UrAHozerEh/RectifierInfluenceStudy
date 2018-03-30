using System;
using System.Collections.Generic;
using System.Text;

namespace RectifierInfluenceStudy
{
    public class RectifierSet
    {
        private string mName;
        public string Name => mName;

        private List<Rectifier> mRectifiers;
        public List<Rectifier> Rectifier => mRectifiers;

        public RectifierSet(string pName)
        {
            mName = pName;
            mRectifiers = new List<Rectifier>();
        }
    }
}
