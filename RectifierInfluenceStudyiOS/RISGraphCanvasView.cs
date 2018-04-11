using System;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using RectifierInfluenceStudy;
using System.ComponentModel;
using Foundation;
using UIKit;
using System.Diagnostics;
using System.Collections.Generic;
using CoreGraphics;

namespace RectifierInfluenceStudy.iOSTester
{
    [Register("RISGraphCanvasView"), DesignTimeVisible(true)]
    public class RISGraphCanvasView : SKCanvasView
    {
        private int _CurrentGraph;
        private List<RISGraph> _Graphs;
        private float _Scale;
        private SKPoint _Offset;
        private SKPoint _PanStartOffset;
        private SKPoint? _PinchStart;

        public RISGraphCanvasView(IntPtr ptr) : base(ptr)
        {
            /*AddGestureRecognizer(new UISwipeGestureRecognizer(sw =>
            {
                ChangeGraph(-1);
            }));
            AddGestureRecognizer(new UISwipeGestureRecognizer(sw =>
            {
                ChangeGraph(1);
            })
            {
                Direction = UISwipeGestureRecognizerDirection.Left
            });*/

            AddGestureRecognizer(new UIPinchGestureRecognizer(PinchGesture));
            AddGestureRecognizer(new UIPanGestureRecognizer(PanGesture));
            _Graphs = new List<RISGraph>();
            _Scale = 1;
            _Offset = new SKPoint();
            _CurrentGraph = 0;
        }

        public override void DrawInSurface(SKSurface surface, SKImageInfo info)
        {
            base.DrawInSurface(surface, info);
            SKCanvas canvas = surface.Canvas;
            canvas.Scale(_Scale);
            canvas.Translate(_Offset);
            if (_Graphs.Count > 0)
                _Graphs[_CurrentGraph].DrawGraph(canvas, info.Rect);
            using (SKPaint text = new SKPaint())
            {
                text.Style = SKPaintStyle.StrokeAndFill;
                text.TextSize = 25;
                text.Color = SKColors.DarkRed;
                //canvas.DrawText(_Scale.ToString(), 10, text.TextSize * 2, text);
                int count = 2;
                foreach (string line in _Graphs[_CurrentGraph].Output.Split('\n'))
                {
                    canvas.DrawText(line, 10, text.TextSize * count, text);
                    ++count;
                }
                if (_PinchStart != null)
                    canvas.DrawCircle((SKPoint)_PinchStart, 2, text);
            }
            canvas.Flush();
        }

        public void AddGraph(RISGraph pGraph)
        {
            _Graphs.Add(pGraph);
        }

        private void PinchGesture(UIPinchGestureRecognizer pPinch)
        {
            if (pPinch.State == UIGestureRecognizerState.Ended)
            {
                _PinchStart = null;
                return;
            }
            if (pPinch.State == UIGestureRecognizerState.Began)
            {
                CGPoint start = pPinch.LocationInView(null);
                _PinchStart = new SKPoint((float)start.X, (float)start.Y);
            }
            CGPoint current = pPinch.LocationInView(null);
            _Offset = new SKPoint(Math.Max((float)(current.X - _PinchStart?.X), 0),
                                  Math.Max((float)(_PinchStart?.Y - current.Y), 0));
            _Scale = Math.Max(Math.Min((float)pPinch.Scale, 5), 1);
            /*if (Math.Abs(_Scale - 1f) < 0.001)
            {
                _Offset = new SKPoint();
                _Scale = 1;
            }*/
            SetNeedsDisplay();
        }

        private void PanGesture(UIPanGestureRecognizer pPan)
        {
            if (pPan.State == UIGestureRecognizerState.Ended)
            {
                if (Math.Abs(_PanStartOffset.X) < 0.001)
                {
                    if (pPan.VelocityInView(this).X > 1000)
                        ChangeGraph(-1);
                    else if (pPan.VelocityInView(this).X < -1000)
                        ChangeGraph(1);
                }
            }
            else if (pPan.State == UIGestureRecognizerState.Began)
            {
                _PanStartOffset = _Offset;
            }
            else if (pPan.State == UIGestureRecognizerState.Changed)
            {
                _Offset = pPan.TranslationInView(null).ToSKPoint() + _PanStartOffset;
                if (_Offset.X > 0)
                    _Offset = new SKPoint(0, _Offset.Y);
                if (_Offset.Y > 0)
                    _Offset = new SKPoint(_Offset.X, 0);
                SetNeedsDisplay();
            }
            //pPan.
        }

        private void ChangeGraph(int pMove)
        {
            if (_Graphs.Count == 0)
                return;
            _Scale = 1;
            _Offset = new SKPoint();
            _PinchStart = null;
            _CurrentGraph += pMove;
            if (_CurrentGraph == -1)
                _CurrentGraph = _Graphs.Count - 1;
            if (_CurrentGraph == _Graphs.Count)
                _CurrentGraph = 0;
            SetNeedsDisplay();
        }
    }
}