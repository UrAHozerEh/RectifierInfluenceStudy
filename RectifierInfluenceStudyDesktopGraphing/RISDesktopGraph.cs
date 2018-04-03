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
        public RISDataSet DataSet;
        private SKPaint mTextPaint;
        private SKPaint mGraphStartPaint;
        private SKPaint mBackgroundPaint;
        private Dictionary<string, SKPaint> mPaints;
        private SKRect mGraphSize;

        public RISDesktopGraph()
        {
            InitializePaint();
            InitializeGraph();
        }

        public RISDesktopGraph(RISDataSet pDataSet)
        {
            DataSet = pDataSet;
            InitializePaint();
            InitializeGraph();
        }

        private void InitializePaint()
        {
            mPaints = new Dictionary<string, SKPaint>();

            mTextPaint = new SKPaint()
            {
                IsAntialias = true,
                IsLinearText = true,
                Color = SKColors.Black,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Center,
                TextSize = 25
            };

            mGraphStartPaint = new SKPaint()
            {
                IsAntialias = true,
                Color = SKColors.LightGray,
                IsStroke = true
            };

            mBackgroundPaint = new SKPaint()
            {
                IsAntialias = false,
                Color = SKColors.White,
                IsStroke = false
            };
        }

        private void InitializeGraph()
        {
            mGraphSize = new SKRect(0, 0, (float)DataSet.GraphLength, -2);
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);
            SKPath graphStart = new SKPath();
            SKRect view = new SKRect()
            {
                Location = new SKPoint(10, 10),
                Right = e.Info.Rect.Width - 10,
                Bottom = e.Info.Rect.Height - 10
            };
            canvas.DrawRect(view, mBackgroundPaint);
            graphStart.MoveTo((float)DataSet.GraphTimeStart, 0);
            graphStart.LineTo((float)DataSet.GraphTimeStart, -2);
            canvas.DrawPath(GetScaledPath(graphStart, mGraphSize, view), mGraphStartPaint);
            //canvas.DrawRect(0, 0, 25, 25, mBlackPaint);
            //if (mPath != null)
            //    canvas.DrawPath(GetScaledPath(mPath, 0, 120, -2, 0, e.Info.Rect), mBlackPaint);
            foreach (var paths in DataSet.GetPaths())
            {
                canvas.DrawPath(GetScaledPath(paths.Item2, mGraphSize, view), paths.Item1);
            }
            canvas.DrawText(DataSet.FileName, view.Width / 2 + view.Left, mTextPaint.TextSize + view.Top, mTextPaint);
            canvas.Flush();
        }

        private SKPath GetScaledPath(SKPath pPath, SKRect pGraphRect, SKRect pCanvasRect)
        {
            SKPath newPath = new SKPath(pPath);
            newPath.Transform(SKMatrix.MakeTranslation(0, pGraphRect.Height * -1));
            newPath.Transform(SKMatrix.MakeScale(pCanvasRect.Width / (pGraphRect.Width), pCanvasRect.Height / (pGraphRect.Height) * -1));
            newPath.Transform(SKMatrix.MakeTranslation(pCanvasRect.Left, pCanvasRect.Top));
            return newPath;
        }
    }
}
