using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RectifierInfluenceStudy.DesktopGraphing
{
    public class RISDesktopGraph : SKControl
    {
        public RISGraph Graph;
        private IGraphWatcher _Watcher;
        private SKRectI _Size;

        public RISDesktopGraph(RISGraph pGraph = null, IGraphWatcher pWatcher = null)
        {
            Graph = pGraph;
            MouseMove += MouseMoved;
            _Watcher = pWatcher;
        }

        private void MouseMoved(object sender, MouseEventArgs e)
        {
            if (_Watcher == null || Graph == null)
                return;
            int x = e.X;
            if (x <= 10 || x >= _Size.Width - 10)
                return;
            x = x - 10;
            int actualWidth = _Size.Width - 20;
            double percent = ((double)x) / (actualWidth);
            double graphTime = percent * Graph.DataSet.GraphLength.TotalSeconds;
            Read read = Graph.DataSet.GetReadFromGraphTime(graphTime);
            int cycleIndex = Graph.DataSet.InterruptionCycle.GetSetPosition(read.UTCTime);
            _Watcher.UpdateValue(null, $"{Graph.DataSet.InterruptionCycle.Sets[cycleIndex].Name}\r\nTime: {read.UTCTime.ToLocalTime()}\r\nValue: {read.Value}");
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            if (Graph == null)
                return;
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);
            Graph.DrawGraph(canvas, e.Info.Rect);
            _Size = e.Info.Rect;
            canvas.Flush();
        }
    }

    public interface IGraphWatcher
    {
        void UpdateValue(Read pRead, string pCycleName);
    }
}
