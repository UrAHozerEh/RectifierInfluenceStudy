using System;

using UIKit;
using RectifierInfluenceStudy;
using System.IO;
using SkiaSharp.Views.iOS;
using SkiaSharp;
using Foundation;

namespace RectifierInfluenceStudyiOS
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

		public override bool PrefersStatusBarHidden()
		{
			return false;
		}

		public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            if (RISCanvasView != null)
            {
                string[] cycleNames = {"TEG Unit #10","TEG Unit #9","TEG Unit #8","TEG Unit #7","Pole Rectifier",
                    "TEG Unit #6","TEG Unit #5","TEG Unit #4","TEG Unit #3","TEG Unit #2","TEG Unit #1","MP 156.13","MP 153.5","MP 148.5"};
                InterruptionCycle cycle = new MultiSetInterruptionCycle("Testing Set 1", 17, 5, 2, cycleNames);
                string[] files = NSBundle.GetPathsForResources(".csv",NSBundle.MainBundle.BundlePath);
                //string[] files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "*.csv", SearchOption.AllDirectories);
                //string[] files = Directory.GetFiles(@"C:\Users\kcron\Desktop\RIS\Phase 1\SET 2\", "*.csv");
                if (files.Length > 0)
                {
                    foreach(string file in files)
                    {
                        var set = new RISDataSet(file, cycle);
                        var graph = new RISGraph(set);
                        RISCanvasView.AddGraph(graph);
                    }
                    //_Graph = graph;
                }
                //RISCanvasView.SetNeedsDisplay();
            }
        }

        void HandleAction()
        {
        }


        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
