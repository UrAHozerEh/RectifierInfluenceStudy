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
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Altitude { get; private set; }
        public string FileName { get; private set; }
        public DateTime TimeStart { get; private set; }
        public DateTime TimeEnd { get; private set; }
        public TimeSpan DataDuration => TimeEnd - TimeStart;
        public float MaxValueData { get; private set; }
        public float MinValueData { get; private set; }
        private List<Read> _DataReads;
        public IReadOnlyList<Read> DataReads
        {
            get { return _DataReads.AsReadOnly(); }
        }
        public InterruptionCycle InterruptionCycle { get; set; }
        public DateTime GraphStart { get; private set; }
        public DateTime GraphEnd { get; private set; }
        public TimeSpan GraphLength => TimeSpan.FromTicks(InterruptionCycle.Length.Ticks * mNumCycles);
        public double GraphTimeStart => InterruptionCycle.GetGraphTime(GraphStart, GraphStart, mNumCycles);
        private int mNumCycles;
        private bool mIsMidCycleStartAllowed;
        private Dictionary<int, List<GraphRead>> mGraphReads;
        public Dictionary<int, List<GraphRead>> GraphReads => mGraphReads;
        public string Output = "";

        public RISDataSet(string pFilePath, InterruptionCycle pInterruptionCycle)
        {
            if (!File.Exists(pFilePath))
                throw new ArgumentException("File does not exist!\n" + pFilePath);
            InterruptionCycle = pInterruptionCycle;
            if (InterruptionCycle is MultiSetInterruptionCycle)
                mNumCycles = 1;
            else
                mNumCycles = 20;
            mIsMidCycleStartAllowed = true;
            mGraphReads = new Dictionary<int, List<GraphRead>>();
            for (int i = 0; i < InterruptionCycle.Sets.Length; ++i)
                mGraphReads.Add(i, new List<GraphRead>());
            FileName = Path.GetFileNameWithoutExtension(pFilePath);
            _DataReads = new List<Read>();
            MaxValueData = float.MinValue;
            MinValueData = float.MaxValue;

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
            float readValue, curVal;
            Read read;
            using (StreamReader sr = new StreamReader(pFilePath))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    ++lineCount;

                    split = line.Split(',');
                    if (!DateTime.TryParseExact(split[0], "M/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
                        throw new ArgumentException($"Format of file {FileName} is incorrect on line {lineCount}\nTime is incorrect format.");
                    if (!float.TryParse(split[1], out readValue))
                        throw new ArgumentException($"Format of file {FileName} is incorrect on line {lineCount}\nRead is not double.");
                    if (lineCount == 1)
                    {
                        if (split.Length != 7)
                            throw new ArgumentException($"Format of file {FileName} is incorrect on line {lineCount}\nMissing GPS Data.");
                        if (!float.TryParse(split[3], out curVal))
                            throw new ArgumentException($"Format of file {FileName} is incorrect on line {lineCount}\nLatitude is not double.");
                        Latitude = curVal;
                        if (!float.TryParse(split[4], out curVal))
                            throw new ArgumentException($"Format of file {FileName} is incorrect on line {lineCount}\nLongitude is not double.");
                        Longitude = curVal;
                        if (!float.TryParse(split[5], out curVal))
                            throw new ArgumentException($"Format of file {FileName} is incorrect on line {lineCount}\nAltitude is not double.");
                        Altitude = curVal;
                    }
                    read = new Read(readValue, time);
                    _DataReads.Add(read);

                    if (read.Value < MinValueData)
                    {
                        MinValueData = read.Value;
                    }
                    if (read.Value > MaxValueData)
                    {
                        MaxValueData = read.Value;
                    }
                }
            }
            TimeStart = _DataReads[0].UTCTime;
            TimeEnd = _DataReads[_DataReads.Count - 1].UTCTime;
            CreateGraphReads();
            _DataReads.Sort();
        }

        public void CreateGraphReads()
        {

            if (!mIsMidCycleStartAllowed)
            {
                GraphStart = InterruptionCycle.GetNextCycleStart(_DataReads[0].UTCTime);
            }
            else
            {
                GraphStart = _DataReads[0].UTCTime;
            }
            GraphEnd = GraphStart.AddSeconds(InterruptionCycle.Length.TotalSeconds * mNumCycles);

            int setPosition;
            DateTime time, lastAdded = new DateTime();
            double readValue;
            bool hasReachedEnd = false;
            bool repeatToFill = false;
            TimeSpan offset = new TimeSpan();
            do
            {
                for (int i = 0; i < _DataReads.Count; ++i)
                {
                    time = _DataReads[i].UTCTime.Add(offset);
                    readValue = _DataReads[i].Value;

                    if (time >= GraphStart && time <= GraphEnd && time > lastAdded)
                    {
                        setPosition = InterruptionCycle.GetSetPosition(time);
                        mGraphReads[setPosition].Add(new GraphRead(readValue,
                            InterruptionCycle.GetGraphTime(GraphStart, time, mNumCycles)));
                        lastAdded = time;
                    }
                    if (time > GraphEnd)
                        hasReachedEnd = true;
                }
                offset = offset.Add(InterruptionCycle.Length);
            } while (!hasReachedEnd && repeatToFill);
        }

        public Read GetReadFromGraphTime(double pGraphTime)
        {
            double min = 9999, difference;
            double otherGraphTime;
            Read minRead = null;
            foreach(Read read in _DataReads)
            {
                otherGraphTime = InterruptionCycle.GetGraphTime(GraphStart, read.UTCTime, mNumCycles);
                difference = Math.Abs(otherGraphTime - pGraphTime);
                if(difference < min)
                {
                    minRead = read;
                    min = difference;
                }
            }
            return minRead;
        }

        public List<Tuple<SKPaint, SKPath>> GetPaths()
        {
            var paths = new List<Tuple<SKPaint, SKPath>>();
            bool fillUnder = false;
            double prev = 0;
            List<GraphRead> combined = new List<GraphRead>();
            foreach (int i in mGraphReads.Keys)
            {
                combined.AddRange(mGraphReads[i]);
            }
            combined.Sort();
            SKPath path = null;
            foreach (GraphRead read in combined)
            {
                if (path == null)
                {
                    path = new SKPath();
                    path.MoveTo((float)read.Time, (float)read.Value);
                    prev = read.Time;
                    continue;
                }
                if (Math.Abs(read.Time - prev) > 1)
                {
                    path.MoveTo((float)read.Time, (float)read.Value);
                    prev = read.Time;
                    continue;
                }
                prev = read.Time;
                path.LineTo((float)read.Time, (float)read.Value);
            }
            SKPaint paint = new SKPaint()
            {
                Color = SKColors.Blue,
                StrokeWidth = 1,
                IsStroke = true,
                IsAntialias = true
            };
            paths.Add(new Tuple<SKPaint, SKPath>(paint, path));
            path = null;

            paint = new SKPaint()
            {
                Color = SKColors.Orange,
                StrokeWidth = 1,
                IsStroke = true,
                IsAntialias = true
            };
            for (int i = 1; i < mGraphReads.Keys.Count; ++i)
            {
                if (mGraphReads[i].Count == 0)
                    continue;
                foreach (GraphRead read in mGraphReads[i])
                {
                    if (path == null)
                    {
                        path = new SKPath();
                        path.MoveTo((float)read.Time, (float)read.Value);
                        prev = read.Time;
                        continue;
                    }
                    if (Math.Abs(read.Time - prev) > 1)
                    {
                        path.MoveTo((float)read.Time, (float)read.Value);
                        prev = read.Time;
                        continue;
                    }
                    prev = read.Time;
                    path.LineTo((float)read.Time, (float)read.Value);
                }
                paths.Add(new Tuple<SKPaint, SKPath>(paint, path));
                path = null;
            }

            SKPaint goodPaint = new SKPaint()
            {
                Color = SKColors.Green,
                StrokeWidth = 3,
                IsStroke = !fillUnder,
                IsAntialias = true
            };
            SKPaint badPaint = new SKPaint()
            {
                Color = SKColors.Red,
                StrokeWidth = 3,
                IsStroke = !fillUnder,
                IsAntialias = true
            };

            List<GraphRead> reads;
            float median = 0;
            float otherMedian = 0;
            float first = 0;
            float last = 0;

            Output = "";
            if (fillUnder)
            {
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
                            if (fillUnder)
                            {
                                path.LineTo(last, median);
                                path.LineTo(first, median);
                                path.Close();
                            }
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
                    if (reads.Count != 0)
                        otherMedian = (float)reads[reads.Count / 2].Value;
                    else
                        otherMedian = 0;

                    if (fillUnder)
                    {
                        path.LineTo(last, median);
                        path.LineTo(first, median);
                        path.Close();
                    }
                    paint = (otherMedian > median ? goodPaint : badPaint);
                    Output += $"{Math.Round(otherMedian - median, 3) * 1000},";
                    if (Math.Abs(otherMedian - median) > 0.005)
                        paths.Add(new Tuple<SKPaint, SKPath>(paint, path));
                    path = null;
                }
            }
            if (Output.Length != 0)
                Output = Output.Substring(0, Output.Length - 1);
            return paths;
        }
    }
}
