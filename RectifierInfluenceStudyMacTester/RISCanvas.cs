using System;
using RectifierInfluenceStudy;

namespace RectifierInfluenceStudyMacTester
{
    public class RISCanvas : SkiaSharp.Views.Mac.SKCanvasView
    {
        public RISDataSet DataSet
        {
            get
            {
                return DataSet;
            }
            set
            {
                DataSet = value;
                this.NeedsDisplay = true;
            }
        }


    }
}