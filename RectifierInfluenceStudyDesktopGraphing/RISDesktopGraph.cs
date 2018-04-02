using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace RectifierInfluenceStudy.DesktopGraphing
{
    public class RISDesktopGraph : SKControl
    {
        private SKPaint mBlackPaint;
        private SKPaint mSetPaint;
        private SKPath mPath;
        private List<SKPath> mPaths;
        private List<SKPaint> mPaints;

        public RISDesktopGraph()
        {
            mBlackPaint = new SKPaint()
            {
                Color = SKColors.Black,
                StrokeWidth = 2,
                IsStroke = true,
                IsAntialias = true
            };
        }

        public RISDesktopGraph(Dictionary<int, List<GraphRead>> pGraphReads)
        {
            mBlackPaint = new SKPaint()
            {
                Color = SKColors.Black,
                StrokeWidth = 2,
                IsStroke = true
            };
            List<GraphRead> combined = new List<GraphRead>();

            foreach (int i in pGraphReads.Keys)
            {
                combined.AddRange(pGraphReads[i]);
            }
            combined.Sort();
            foreach (GraphRead read in combined)
            {
                if (mPath == null)
                {
                    mPath = new SKPath();
                    mPath.MoveTo((float)read.Time, (float)read.Value);
                    continue;
                }
                mPath.LineTo((float)read.Time, (float)read.Value);
            }
            //mPath.LineTo(0, 0);
            mPath.MoveTo(0, 0);
            mPath.LineTo(120, 0);
            mPath.MoveTo(0, -1);
            mPath.LineTo(120, -1);
            mPath.MoveTo(0, -2);
            mPath.LineTo(120, -2);
        }

        private SKPath CreatePath(List<GraphRead> pReads)
        {
            List<GraphRead> copy = new List<GraphRead>(pReads);
            copy.Sort();
            SKPath path = null;
            double lastTime = 0;
            foreach(GraphRead read in pReads)
            {
                if(path == null)
                {
                    path = new SKPath();
                    mPath.MoveTo((float)read.Time, (float)read.Value);
                    lastTime = read.Time;
                    continue;
                }
                if(read.Time - lastTime < )
                mPath.LineTo((float)read.Time, (float)read.Value);
            }

            return path;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);
            //canvas.DrawRect(0, 0, 25, 25, mBlackPaint);
            if (mPath != null)
                canvas.DrawPath(GetScaledPath(mPath, 0, 120, -2, 0, e.Info.Rect), mBlackPaint);
            canvas.Flush();
        }

        private SKPath GetScaledPath(SKPath pPath, float pMinX, float pMaxX, float pMinY, float pMaxY, SKRect pRect)
        {
            SKPath newPath = new SKPath(pPath);
            newPath.Transform(SKMatrix.MakeTranslation(0, pMaxY - pMinY));
            newPath.Transform(SKMatrix.MakeScale(pRect.Width / (pMaxX - pMinX), pRect.Height / (pMaxY - pMinY)));
            return newPath;
        }
    }
}
