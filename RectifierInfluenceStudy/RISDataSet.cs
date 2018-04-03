using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SkiaSharp;

namespace RectifierInfluenceStudy
{
    public class RISDataSet
    {
        private double mLatitude;
        public double Latitude => mLatitude;
        private double mLongitude;
        public double Longitude => mLongitude;
        private double mAltitude;
        public double Altitude => mAltitude;

        private string mFileName;
        public string FileName => mFileName;

        private TimeSpan mDuration;
        public TimeSpan Duration => mDuration;
        private DateTime mDataStart;
        public DateTime DataStart => mDataStart;
        private DateTime mDataEnd;
        public DateTime DataEnd => mDataEnd;
        private double mMaxValue;
        public double MaxValue => mMaxValue;
        private double mMinValue;
        public double MinValue => mMinValue;

        private List<Read> mReads;
        public List<Read> Reads => mReads;
        private InterruptionCycle mCycle;
        private DateTime mGraphStart;
        public double GraphLength => (double)mCycle.Length.Ticks / TimeSpan.TicksPerSecond * mNumCycles;
        public double GraphTimeStart => mCycle.GetGraphTime(mGraphStart, mGraphStart, mNumCycles);
        private DateTime mGraphEnd;
        private int mNumCycles;
        private bool mIsMidCycleStartAllowed;
        private Dictionary<int, List<GraphRead>> mGraphReads;
        public Dictionary<int, List<GraphRead>> GraphReads => mGraphReads;

        public RISDataSet(string pFilePath, InterruptionCycle pCycle)
        {
            if (!File.Exists(pFilePath))
                throw new ArgumentException("File does not exist!\n" + pFilePath);
            mCycle = pCycle;
            if (mCycle is MultiSetInterruptionCycle)
                mNumCycles = 2;
            else
                mNumCycles = 20;
            mIsMidCycleStartAllowed = true;
            mGraphReads = new Dictionary<int, List<GraphRead>>();
            for (int i = 0; i < mCycle.Sets.Length; ++i)
            {
                mGraphReads.Add(i, new List<GraphRead>());
            }
            mFileName = Path.GetFileNameWithoutExtension(pFilePath);
            mReads = new List<Read>();
            mMaxValue = double.MinValue;
            mMinValue = double.MaxValue;

            switch (Path.GetExtension(pFilePath))
            {
                case ".csv":
                    ReadMcMilleriBTVMFile(pFilePath);
                    break;
                default:
                    break;
            }
        }

        private void ReadMcMilleriBTVMFile(string pFilePath)
        {
            string line;
            string[] split;
            int lineCount = 0;
            DateTime time;
            double readValue;
            Read read;
            int setPosition;
            using (StreamReader sr = new StreamReader(pFilePath))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    ++lineCount;

                    split = line.Split(',');
                    if (!DateTime.TryParseExact(split[0], "M/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
                        throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nTime is incorrect format.");
                    if (!double.TryParse(split[1], out readValue))
                        throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nRead is not double.");
                    if (lineCount == 1)
                    {
                        if (split.Length != 7)
                            throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nMissing GPS Data.");
                        if (!double.TryParse(split[3], out mLatitude))
                            throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nLatitude is not double.");
                        if (!double.TryParse(split[4], out mLongitude))
                            throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nLongitude is not double.");
                        if (!double.TryParse(split[5], out mAltitude))
                            throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nAltitude is not double.");
                    }
                    read = new Read(readValue, time);
                    mReads.Add(read);

                    if (read.Value < mMinValue)
                    {
                        mMinValue = read.Value;
                    }
                    if (read.Value > mMaxValue)
                    {
                        mMaxValue = read.Value;
                    }
                }
            }
            mDataStart = mReads[0].UTCTime;
            mDataEnd = mReads[mReads.Count - 1].UTCTime;
            mDuration = mDataEnd - mDataStart;
            CreateGraphReads();
            mReads.Sort();
        }

        public void CreateGraphReads()
        {

            if (!mIsMidCycleStartAllowed)
            {
                mGraphStart = mCycle.GetNextCycleStart(mReads[0].UTCTime);
            }
            else
            {
                mGraphStart = mReads[0].UTCTime;
            }
            mGraphEnd = mGraphStart;
            for (int i = 0; i < mNumCycles; ++i)
                mGraphEnd = mGraphEnd.Add(mCycle.Length);

            int setPosition;
            DateTime time;
            double readValue;
            bool hasReachedEnd = false;
            bool repeatToFill = true;
            TimeSpan offset = new TimeSpan();
            do
            {
                for (int i = 0; i < mReads.Count; ++i)
                {
                    time = mReads[i].UTCTime.Add(offset);
                    readValue = mReads[i].Value;

                    if (time >= mGraphStart && time <= mGraphEnd)
                    {
                        setPosition = mCycle.GetSetPosition(time);
                        mGraphReads[setPosition].Add(new GraphRead(readValue,
                            mCycle.GetGraphTime(mGraphStart, time, mNumCycles)));
                    }
                    if (time > mGraphEnd)
                        hasReachedEnd = true;
                }
                offset = offset.Add(mCycle.Length);
            } while (!hasReachedEnd && repeatToFill);
        }

        public SKPath GetPath()
        {
            SKPath path = null;
            List<GraphRead> combined = new List<GraphRead>();
            double prev = 0;

            foreach (int i in mGraphReads.Keys)
            {
                combined.AddRange(mGraphReads[i]);
            }
            combined.Sort();
            foreach (GraphRead read in combined)
            {
                if (path == null)
                {
                    path = new SKPath();
                    path.MoveTo((float)read.Time, (float)read.Value);
                    prev = read.Time;
                    continue;
                }
                if(Math.Abs(read.Time - prev) > 1)
                {
                    path.MoveTo((float)read.Time, (float)read.Value);
                    prev = read.Time;
                    continue;
                }
                path.LineTo((float)read.Time, (float)read.Value);
            }
            return path;
        }

        public List<Tuple<SKPaint, SKPath>> GetPaths()
        {
            var paths = new List<Tuple<SKPaint, SKPath>>();

            SKPaint goodPaint = new SKPaint()
            {
                Color = SKColors.Green,
                StrokeWidth = 2,
                IsStroke = false,
                IsAntialias = true
            };
            SKPaint badPaint = new SKPaint()
            {
                Color = SKColors.Red,
                StrokeWidth = 2,
                IsStroke = false,
                IsAntialias = true
            };
            SKPaint paint;
            SKPath path = null;
            paint = new SKPaint()
            {
                Color = SKColors.Blue,
                StrokeWidth = 1,
                IsStroke = true,
                IsAntialias = true
            };
            paths.Add(new Tuple<SKPaint, SKPath>(paint, GetPath()));
            List<GraphRead> reads;
            float median = 0;
            float otherMedian = 0;
            float first = 0;
            float last = 0;
            double prev = 0;

            foreach (int i in mGraphReads.Keys)
            {
                reads = new List<GraphRead>(mGraphReads[i]);
                if (i == 0)
                {
                    reads.Sort((r1, r2) => r1.Value.CompareTo(r2.Value));
                    median = (float)reads[reads.Count / 2].Value;
                    continue;
                }
                reads.Sort();
                foreach (GraphRead read in reads)
                {
                    if (path == null)
                    {
                        path = new SKPath();
                        first = (float)read.Time;
                        prev = read.Time;
                        path.MoveTo((float)read.Time, (float)read.Value);
                        continue;
                    }
                    if (Math.Abs(read.Time - prev) > 1)
                    {
                        path.LineTo(last, median);
                        path.LineTo(first, median);
                        path.Close();
                        first = (float)read.Time;
                        prev = read.Time;
                        path.MoveTo((float)read.Time, (float)read.Value);
                        continue;
                    }
                    last = (float)read.Time;
                    path.LineTo((float)read.Time, (float)read.Value);
                    prev = read.Time;
                }

                reads.Sort((r1, r2) => r1.Value.CompareTo(r2.Value));
                otherMedian = (float)reads[reads.Count / 2].Value;

                path.LineTo(last, median);
                path.LineTo(first, median);
                path.Close();
                paint = (otherMedian > median ? goodPaint : badPaint);
                if (Math.Abs(otherMedian - median) > 0.005)
                    paths.Add(new Tuple<SKPaint, SKPath>(paint, path));
                path = null;
            }

            return paths;
        }
    }
}
