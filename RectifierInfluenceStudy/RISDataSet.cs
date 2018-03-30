﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

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

        public RISDataSet(string pFilePath)
        {
            if (!File.Exists(pFilePath))
                throw new ArgumentException("File does not exist!\n" + pFilePath);
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
            using (StreamReader sr = new StreamReader(pFilePath))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    ++lineCount;

                    split = line.Split(',');
                    if (split.Length == 7)
                    {
                        if (!double.TryParse(split[3], out mLatitude))
                            throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nLatitude is not double.");
                        if (!double.TryParse(split[4], out mLongitude))
                            throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nLongitude is not double.");
                        if (!double.TryParse(split[5], out mAltitude))
                            throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nAltitude is not double.");
                    }
                    if (!DateTime.TryParseExact(split[0], "M/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
                        throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nTime is incorrect format.");
                    if (!double.TryParse(split[1], out readValue))
                        throw new ArgumentException($"Format of file {mFileName} is incorrect on line {lineCount}\nRead is not double.");
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
            mReads.Sort();
        }
    }
}