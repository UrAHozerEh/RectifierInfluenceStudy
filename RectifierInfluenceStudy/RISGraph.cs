using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace RectifierInfluenceStudy
{
    public class RISGraph
    {
        public RISDataSet DataSet { get; set; }
        private Dictionary<string, SKPaint> _Paints;
        public SKRect GraphSize { get; set; }

        public RISGraph(RISDataSet pDataSet)
        {
            if (pDataSet == null)
                return;
            DataSet = pDataSet;
            int min = -3;
            while (pDataSet.MinValueData < min)
                min--;
            GraphSize = new SKRect(0, 0, (float)pDataSet.GraphLength.TotalSeconds, min);
            InitializePaint();
        }

        private void InitializePaint()
        {
            _Paints = new Dictionary<string, SKPaint>();
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
            _Paints.Add("TitleText", paint);

            paint = new SKPaint()
            {
                IsAntialias = true,
                Color = SKColors.LightSeaGreen,
                IsStroke = true
            };
            _Paints.Add("DataStartLine", paint);

            paint = new SKPaint()
            {
                IsAntialias = false,
                Color = SKColors.White,
                IsStroke = false
            };
            _Paints.Add("GraphBackground", paint);

            paint = new SKPaint()
            {
                IsAntialias = true,
                Color = SKColors.LightGray,
                StrokeWidth = 2,
                IsStroke = true
            };
            _Paints.Add("MajorGridLine", paint);

            paint = new SKPaint()
            {
                IsAntialias = true,
                Color = new SKColor(0xd3, 0xd3, 0xd3, 0x7f), // Half alpha of LightGray
                StrokeWidth = 1,
                IsStroke = true
            };
            _Paints.Add("MinorGridLine", paint);
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
            pCanvas.DrawRect(view, _Paints["GraphBackground"]);
            graphStart.MoveTo((float)DataSet.GraphTimeStart, 0);
            graphStart.LineTo((float)DataSet.GraphTimeStart, GraphSize.Bottom);
            pCanvas.DrawPath(GetScaledPath(graphStart, GraphSize, view), _Paints["DataStartLine"]);
            DrawGridLines(pCanvas, GraphSize, view, 10, 5, GridLineDirection.Horizontal);
            //canvas.DrawRect(0, 0, 25, 25, mBlackPaint);
            //if (mPath != null)
            //    canvas.DrawPath(GetScaledPath(mPath, 0, 120, -2, 0, e.Info.Rect), mBlackPaint);
            foreach (var paths in DataSet.GetPaths())
            {
                pCanvas.DrawPath(GetScaledPath(paths.Item2, GraphSize, view), paths.Item1);
            }
            //pCanvas.DrawText(DataSet.FileName, view.Width / 2 + view.Left, _Paints["TitleText"].TextSize + view.Top, _Paints["TitleText"]);
            pCanvas.Flush();
        }

        private enum GridLineDirection
        {
            Horizontal,
            Vertical
        }

        private void DrawGridLines(SKCanvas pCanvas, SKRect pGraphRect, SKRect pView, int pMajorCount, int pMinorCount, GridLineDirection pDirection)
        {
            SKPath majorPath = new SKPath();
            SKPath minorPath = new SKPath();
            float major, minor, min, max, majorOffset;
            if (pDirection == GridLineDirection.Horizontal)
            {
                min = pGraphRect.Left;
                max = pGraphRect.Right;
                majorOffset = pGraphRect.Height / pMajorCount;
            }
            else
            {
                min = pGraphRect.Top;
                max = pGraphRect.Bottom;
                majorOffset = pGraphRect.Width / pMajorCount;
            }
            float minorOffset = majorOffset / pMinorCount;
            for (int i = 0; i <= pMajorCount; ++i)
            {
                if (pDirection == GridLineDirection.Horizontal)
                {
                    major = pGraphRect.Top + majorOffset * i;
                    majorPath.MoveTo(min, major);
                    majorPath.LineTo(max, major);
                }
                else
                {
                    major = pGraphRect.Left + majorOffset * i;
                    majorPath.MoveTo(major, min);
                    majorPath.LineTo(major, max);
                }

                if (i < pMajorCount)
                {
                    for (int j = 0; j <= pMinorCount; ++j)
                    {
                        minor = major + minorOffset * j;
                        if (pDirection == GridLineDirection.Horizontal)
                        {
                            minorPath.MoveTo(min, minor);
                            minorPath.LineTo(max, minor);
                        }
                        else
                        {
                            minorPath.MoveTo(minor, min);
                            minorPath.LineTo(minor, max);
                        }
                    }
                }
            }
            pCanvas.DrawPath(GetScaledPath(minorPath, pGraphRect, pView), _Paints["MinorGridLine"]);
            pCanvas.DrawPath(GetScaledPath(majorPath, pGraphRect, pView), _Paints["MajorGridLine"]);
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
