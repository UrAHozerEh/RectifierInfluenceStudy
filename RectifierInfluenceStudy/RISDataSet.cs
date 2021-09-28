using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

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

        public Dictionary<int, double> Averages { get; private set; }

        public string Output = "";
        public string FullFileName;
        public double Offset = 0;

        public RISDataSet(string pFilePath, InterruptionCycle pInterruptionCycle, double pOffset = 0)
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
            Averages = new Dictionary<int, double>();
            for (int i = 0; i < InterruptionCycle.Sets.Length; ++i)
                mGraphReads.Add(i, new List<GraphRead>());
            FullFileName = pFilePath;
            FileName = Path.GetFileNameWithoutExtension(pFilePath);
            _DataReads = new List<Read>();
            MaxValueData = float.MinValue;
            MinValueData = float.MaxValue;

            switch (Path.GetExtension(pFilePath).ToLower())
            {
                case ".csv":
                    ReadMcMilleriBTVMFile(pFilePath);
                    break;
                default:
                    break;
            }
        }

        public double GuessOffset(double averageOffsetWeight = 1, double averageStdDevWeight = 3, double allOnStdDevWeight = 5)
        {
            var startingOffset = Offset;

            var setLength = InterruptionCycle.Length.TotalSeconds;
            var min = 0 - (setLength / 2);
            var max = setLength + min;
            var bestScore = double.MaxValue;
            var bestOffset = startingOffset;

            for (double curOffset = min; curOffset < max; curOffset += 0.05)
            {
                Offset = curOffset;
                var averages = GenerateAverages();
                var allOnAverage = averages[0];
                var allOffAverage = averages[averages.Count - 1];
                var allOffDifference = allOnAverage - allOffAverage;
                var inCycleDifference = 0.0;
                for (int i = 1; i < averages.Count - 1; ++i)
                {
                    var curCycleDifference = allOnAverage - averages[i];
                    inCycleDifference += curCycleDifference;
                }
                var averageOffset = Math.Abs(inCycleDifference - allOffDifference);
                var averageStdDev = CalculateAverageStandardDeviation();
                var allOnStdDev = CalculateStandardDeviation(GraphReads[0]);
                var score = averageOffset * averageOffsetWeight + averageStdDev * averageStdDevWeight + allOnStdDev * allOnStdDevWeight;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestOffset = curOffset;
                }
            }

            Offset = startingOffset;
            return bestOffset;
        }

        private double CalculateAverageStandardDeviation()
        {
            CreateGraphReads();
            var total = 0.0;
            var count = 0;

            foreach (var readsPair in GraphReads)
            {
                var std = CalculateStandardDeviation(readsPair.Value);
                total += std;
                ++count;
            }

            return total / count;
        }

        private double CalculateStandardDeviation(IEnumerable<GraphRead> values)
        {
            double standardDeviation = 0;

            if (values.Any())
            {
                // Compute the average.     
                double avg = values.Average(v => v.Value);

                // Perform the Sum of (value-avg)_2_2.      
                double sum = values.Sum(d => Math.Pow(d.Value - avg, 2));

                // Put it all together.      
                standardDeviation = Math.Sqrt((sum) / (values.Count() - 1));
            }

            return standardDeviation;
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
            mGraphReads = new Dictionary<int, List<GraphRead>>();
            for (int i = 0; i < InterruptionCycle.Sets.Length; ++i)
                mGraphReads.Add(i, new List<GraphRead>());
            TimeSpan offset = new TimeSpan(TimeSpan.TicksPerMillisecond * (long)(Offset * 1000));
            if (!mIsMidCycleStartAllowed)
            {
                GraphStart = InterruptionCycle.GetNextCycleStart(_DataReads[0].UTCTime);
            }
            else
            {
                GraphStart = _DataReads[0].UTCTime.Add(offset);
            }
            GraphEnd = GraphStart.AddSeconds(InterruptionCycle.Length.TotalSeconds * mNumCycles);

            int setPosition;
            DateTime time, lastAdded = new DateTime();
            double readValue;
            bool hasReachedEnd = false;
            bool repeatToFill = false;
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

        public Dictionary<int, double> GenerateAverages()
        {
            Dictionary<int, double> averages = new Dictionary<int, double>();
            Dictionary<int, double> sum = new Dictionary<int, double>();
            Dictionary<int, double> count = new Dictionary<int, double>();
            TimeSpan offset = new TimeSpan(TimeSpan.TicksPerMillisecond * (long)(Offset * 1000));
            DateTime time;
            int setPosition;
            double readValue;
            for (int i = 0; i < _DataReads.Count; ++i)
            {
                time = _DataReads[i].UTCTime.Add(offset);
                readValue = _DataReads[i].Value;
                setPosition = InterruptionCycle.GetSetPosition(time);
                if (count.ContainsKey(setPosition))
                {
                    ++count[setPosition];
                    sum[setPosition] += readValue;
                }
                else
                {
                    count.Add(setPosition, 1);
                    sum.Add(setPosition, readValue);
                }
            }

            foreach (int set in count.Keys)
            {
                averages.Add(set, sum[set] / count[set]);
            }
            return averages;
        }

        public Read GetReadFromGraphTime(double pGraphTime)
        {
            double min = 9999, difference;
            double otherGraphTime;
            Read minRead = null;
            foreach (Read read in _DataReads)
            {
                otherGraphTime = InterruptionCycle.GetGraphTime(GraphStart, read.UTCTime, mNumCycles);
                difference = Math.Abs(otherGraphTime - pGraphTime);
                if (difference < min)
                {
                    minRead = read;
                    min = difference;
                }
            }
            return minRead;
        }
    }
}
