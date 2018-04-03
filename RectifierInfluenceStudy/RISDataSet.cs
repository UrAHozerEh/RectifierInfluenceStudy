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
                mNumCycles = 1;
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
                        if (!mIsMidCycleStartAllowed)
                        {
                            mGraphStart = mCycle.GetNextCycleStart(time);
                        }
                        else
                        {
                            mGraphStart = time;
                        }
                        mGraphEnd = mGraphStart;
                        for (int i = 0; i < mNumCycles; ++i)
                            mGraphEnd = mGraphEnd.Add(mCycle.Length);
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

                    if (time >= mGraphStart && time <= mGraphEnd)
                    {
                        setPosition = mCycle.GetSetPosition(time);
                        mGraphReads[setPosition].Add(new GraphRead(readValue,
                            mCycle.GetGraphTime(mGraphStart, time, mNumCycles)));
                    }
                }
            }
            mDataStart = mReads[0].UTCTime;
            mDataEnd = mReads[mReads.Count - 1].UTCTime;
            mDuration = mDataEnd - mDataStart;
            if (mGraphEnd > mDataEnd)
                throw new ArgumentException($"{mFileName} is not long enough.");
            mReads.Sort();
        }

        public SKPath GetPath()
        {
            SKPath path = null;
            List<GraphRead> combined = new List<GraphRead>();

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
                    continue;
                }
                path.LineTo((float)read.Time, (float)read.Value);
            }
            return path;
        }
    }
}
