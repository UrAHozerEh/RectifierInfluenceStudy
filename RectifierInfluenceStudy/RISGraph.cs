using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace RectifierInfluenceStudy
{
    public class RISGraph
    {
        public RISDataSet DataSet { get; set; }
        private Dictionary<string, SKPaint> mPaints;
        public SKRect GraphSize { get; set; }
        public string Output { get { return DataSet.Output; }}

        public RISGraph(RISDataSet pDataSet)
        {
            DataSet = pDataSet;
            GraphSize = new SKRect(0, 0, 120, -2);
            InitializePaint();
        }

        private void InitializePaint()
        {
            mPaints = new Dictionary<string, SKPaint>();
            SKPaint paint;

            paint = new SKPaint()
            {
                IsAntialias = true,
                IsLinearText = true,
                Color = SKColors.Black,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Center,
                TextSize = 25
            };
            mPaints.Add("TitleText", paint);

            paint = new SKPaint()
            {
                IsAntialias = true,
                Color = SKColors.LightSeaGreen,
                IsStroke = true
            };
            mPaints.Add("DataStartLine", paint);

            paint = new SKPaint()
            {
                IsAntialias = false,
                Color = SKColors.White,
                IsStroke = false
            };
            mPaints.Add("GraphBackground", paint);

            paint = new SKPaint()
            {
                IsAntialias = true,
                Color = SKColors.LightGray,
                StrokeWidth = 2,
                IsStroke = true
            };
            mPaints.Add("MajorGridLine", paint);

            paint = new SKPaint()
            {
                IsAntialias = true,
                Color = new SKColor(0xd3, 0xd3, 0xd3, 0x7f), // Half alpha of LightGray
                StrokeWidth = 1,
                IsStroke = true
            };
            mPaints.Add("MinorGridLine", paint);
        }

        public void DrawGraph(SKCanvas pCanvas, SKRect pCanvasRect)
        {
            pCanvas.Clear(SKColors.Black);
            SKPath graphStart = new SKPath();
            SKRect view = new SKRect()
            {
                Location = new SKPoint(10, 10),
                Right = pCanvasRect.Width - 10,
                Bottom = pCanvasRect.Height - 10
            };
            pCanvas.DrawRect(view, mPaints["GraphBackground"]);
            graphStart.MoveTo((float)DataSet.GraphTimeStart, 0);
            graphStart.LineTo((float)DataSet.GraphTimeStart, -2);
            pCanvas.DrawPath(GetScaledPath(graphStart, GraphSize, view), mPaints["DataStartLine"]);
            DrawHorizontalGridLines(pCanvas, GraphSize, view, 10, 5);
            //canvas.DrawRect(0, 0, 25, 25, mBlackPaint);
            //if (mPath != null)
            //    canvas.DrawPath(GetScaledPath(mPath, 0, 120, -2, 0, e.Info.Rect), mBlackPaint);
            foreach (var paths in DataSet.GetPaths())
            {
                pCanvas.DrawPath(GetScaledPath(paths.Item2, GraphSize, view), paths.Item1);
            }
            pCanvas.DrawText(DataSet.FileName, view.Width / 2 + view.Left, mPaints["TitleText"].TextSize + view.Top, mPaints["TitleText"]);
            pCanvas.Flush();
        }

        private void DrawHorizontalGridLines(SKCanvas pCanvas, SKRect pGraphRect, SKRect pView, int pMajorCount, int pMinorCount)
        {
            SKPath majorPath = new SKPath();
            SKPath minorPath = new SKPath();
            float majorY, minorY;
            float minX = pGraphRect.Left;
            float maxX = pGraphRect.Right;
            float majorOffset = pGraphRect.Height / pMajorCount;
            float minorOffset = majorOffset / pMinorCount;
            for (int i = 0; i <= pMajorCount; ++i)
            {
                majorY = pGraphRect.Top + majorOffset * i;
                majorPath.MoveTo(minX, majorY);
                majorPath.LineTo(maxX, majorY);
                if (i < pMajorCount)
                {
                    for (int j = 0; j <= pMinorCount; ++j)
                    {
                        minorY = majorY + minorOffset * j;
                        minorPath.MoveTo(minX, minorY);
                        minorPath.LineTo(maxX, minorY);
                    }
                }
            }
            pCanvas.DrawPath(GetScaledPath(minorPath, pGraphRect, pView), mPaints["MinorGridLine"]);
            pCanvas.DrawPath(GetScaledPath(majorPath, pGraphRect, pView), mPaints["MajorGridLine"]);
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
