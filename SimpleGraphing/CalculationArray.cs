using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class CalculationArray
    {
        int m_nMax = 0;
        protected List<double> m_rgdf;
        double m_dfAve = 0;
        double m_dfSum = 0;
        protected List<double> m_rgdfEwm;
        DateTime? m_dt;
        double m_dfLast = 0;
        double? m_dfEwmAlpha = null;

        public CalculationArray(int nMax, bool bEnableEwm = false)
        {
            if (bEnableEwm)
                m_dfEwmAlpha = 1.0 - Math.Exp(-Math.Log(2.0) / nMax);

            UpdateMax(nMax);
        }

        public void UpdateMax(int nMax)
        {
            m_nMax = nMax;

            if (nMax == int.MaxValue)
                nMax = 1000;

            m_rgdf = new List<double>(nMax);

            if (m_dfEwmAlpha.HasValue)
                m_rgdfEwm = new List<double>(nMax);
        }

        public List<double> Items
        {
            get { return m_rgdf; }
        }

        public void Clear()
        {
            m_rgdf.Clear();
            m_dfAve = 0;
            m_dfSum = 0;
            m_dfLast = 0;
        }

        public bool IsFull
        {
            get { return (m_rgdf.Count == m_nMax) ? true : false; }
        }

        public void ReplaceLast(double df)
        {
            if (m_rgdf.Count == 0)
                return;

            double dfLast = m_rgdf[m_rgdf.Count - 1];

            m_dfAve -= dfLast / m_nMax;
            m_dfAve += df / m_nMax;

            m_dfSum -= dfLast;
            m_dfSum += df;

            m_rgdf[m_rgdf.Count - 1] = df;
            m_dfLast = df;
        }

        public bool Add(double df, DateTime? dt, bool bAbs = true)
        {
            double dfVal = (bAbs) ? Math.Abs(df) : df;
            double dfFirst = 0;

            if (m_rgdf.Count == m_nMax)
            {
                dfFirst = m_rgdf[0];
                m_dfAve -= (dfFirst / m_nMax);
                m_dfSum -= dfFirst;
                m_rgdf.RemoveAt(0);
            }

            m_dfLast = df;
            m_dt = dt;
            m_dfAve += (dfVal / m_nMax);
            m_dfSum += dfVal;
            m_rgdf.Add(dfVal);

            if (m_dfEwmAlpha.HasValue)
            {
                if (m_rgdfEwm.Count == 0)
                {
                    m_rgdfEwm.Add(dfVal);
                }
                else
                {
                    double dfEwm = (m_dfEwmAlpha.Value * dfVal) + ((1 - m_dfEwmAlpha.Value) * m_rgdfEwm[m_rgdfEwm.Count - 1]);
                    m_rgdfEwm.Add(dfEwm);
                }

                if (m_rgdfEwm.Count > m_nMax)
                    m_rgdfEwm.RemoveAt(0);
            }

            if (m_rgdf.Count == m_nMax)
                return true;

            return false;
        }

        public int Count
        {
            get { return m_rgdf.Count; }
        }

        public DateTime? TimeStamp
        {
            get { return m_dt; }
        }

        public double GetLast(int nOffset)
        {
            if (nOffset > 0)
                throw new Exception("The offset from the back must be <= 0.");

            int nIdx = m_rgdf.Count + (nOffset - 1);
            if (nIdx < 0)
                throw new Exception("There are not enough items in the list to reach the specified offset from the back.");

            return m_rgdf[nIdx];
        }

        public double FirstVal
        {
            get { return (m_rgdf.Count == 0) ? 0 : m_rgdf[0]; }
        }

        public double LastVal
        {
            get { return (m_rgdf.Count == 0) ? 0 : m_rgdf[m_rgdf.Count - 1]; }
        }

        public double LastRaw
        {
            get { return m_dfLast; }
        }

        public double Sum
        {
            get { return m_dfSum; }
        }

        public double Average
        {
            get { return m_dfAve; }
        }

        public double AverageEwm
        {
            get { return (m_rgdfEwm != null && m_rgdfEwm.Count > 0) ? m_rgdfEwm[m_rgdfEwm.Count-1] : 0; }
        }

        public void UpdateAverage()
        {
            m_dfAve = 0;

            for (int i = 0; i < m_rgdf.Count; i++)
            {
                m_dfAve += m_rgdf[i];
            }

            m_dfAve /= m_rgdf.Count;
        }

        public double Variance
        {
            get
            {
                double dfVar;
                CalculateStdDev(m_rgdf, Count, false, out dfVar);
                return dfVar;
            }
        }

        public double StdDev
        {
            get 
            {
                double dfVar;
                return CalculateStdDev(m_rgdf, Count, false, out dfVar); 
            }
        }

        public double StdDevEwm
        {
            get 
            {
                double dfVar;
                return CalculateStdDev(m_rgdfEwm, Count, true, out dfVar); 
            }
        }

        public double CalculateStdDev(List<double> rg, int nOffset, bool bEwm, out double dfVar)
        {
            dfVar = 0;

            if (rg == null || rg.Count == 0)
                return 0;

            if (rg.Count < nOffset)
                return 0;

            double dfTotal = 0;
            double dfAverage = (bEwm) ? AverageEwm : Average;

            for (int i = rg.Count - nOffset; i < rg.Count; i++)
            {
                double dfDiff = (rg[i] - dfAverage);
                dfTotal += (dfDiff * dfDiff);
            }

            dfVar = dfTotal / nOffset;

            return Math.Sqrt(dfVar);
        }

        public double MaxVal
        {
            get { return m_rgdf.Max(); }
        }

        public double MinVal
        {
            get { return m_rgdf.Min(); }
        }

        /// <summary>
        /// Calculates the T-Test between this array and the input array.
        /// </summary>
        /// <param name="rg">Specifies the second array to test against.</param>
        /// <returns>A tuple containing the T-statistic, and  P-value is returned.</returns>
        /// <exception cref="Exception">An exception is thrown if the two array counts do not match.</exception>
        public Tuple<double, double> CalculateTTest(CalculationArray rg)
        {
            if (rg.Count != Count)
                throw new Exception("The two arrays must have the same number of items to calculate a t-test.");

            double dfMean1 = Average;
            double dfMean2 = rg.Average;
            double dfVar1;
            double dfVar2;
            double dfT = 0;

            CalculateStdDev(m_rgdf, Count, false, out dfVar1);
            CalculateStdDev(rg.Items, Count, false, out dfVar2);

            if (dfVar1 == 0 || dfVar2 == 0)
                return new Tuple<double, double>(0, 0);

            dfT = (dfMean1 - dfMean2) / Math.Sqrt((dfVar1 / Count) + (dfVar2 / Count));

            return new Tuple<double, double>(dfT, dfT);
        }

        /// <summary>
        /// Approximates the cumulative distribution function (CDF) for the Student's t-distribution.
        /// </summary>
        /// <param name="dfT">The t-value at which to evaluate the CDF. This is the calculated t-statistic from the t-test.</param>
        /// <param name="nDf">Degrees of freedom for the distribution, usually calculated as the sum of the sample sizes minus 2.</param>
        /// <returns>The approximate probability of the t-distributed random variable being less than or equal to the given t-value.</returns>
        public double CalculateStudentT_CDF(double dfT, int nDf)
        {
            double dfX = dfT * dfT;
            double dfDf = nDf;
            double dfNum = 1.0;
            double dfDenom = 1.0;

            for (int i = 1; i <= nDf / 2 - 1; i++)
            {
                dfNum *= (2 * i + 1);
                dfDenom *= 2 * i;
            }

            double dfResult = 0.5 + (dfX / (dfDf + dfX)) * (dfNum / dfDenom);

            return dfResult;
        }

        /// <summary>
        /// Calculates the Mann-Whitney U test for two independent samples.
        /// </summary>
        /// <param name="ca">The second sample array.</param>
        /// <returns>A tuple containing the U statistic, the Z-score, and the approximate p-value.</returns>
        public Tuple<double, double, double> CalculateMannWhitneyU(CalculationArray ca)
        {
            return CalculateMannWhitneyU(m_rgdf.ToArray(), ca.Items.ToArray());
        }

        /// <summary>
        /// Calculates the Mann-Whitney U test for two independent samples.
        /// </summary>
        /// <param name="sample1">The first sample array.</param>
        /// <param name="sample2">The second sample array.</param>
        /// <returns>A tuple containing the U statistic, the Z-score, and the approximate p-value.</returns>
        // Updated CalculateMannWhitneyU function using the RankedValue class
        public static Tuple<double, double, double> CalculateMannWhitneyU(double[] sample1, double[] sample2)
        {
            // Combine the samples and assign a group label to each sample
            var combined = sample1.Select(x => new RankedSample { Value = x, Group = 1 })
                                  .Concat(sample2.Select(x => new RankedSample { Value = x, Group = 2 }))
                                  .ToList();

            // Sort by Value
            combined.Sort((a, b) => a.Value.CompareTo(b.Value));

            // Initialize rank sum for the first sample
            double sumRanks = 0;

            // Process the combined list to assign ranks
            int i = 0;
            while (i < combined.Count)
            {
                int start = i;
                int end = i;

                // Determine the extent of ties
                while (end < combined.Count - 1 && combined[end + 1].Value == combined[start].Value)
                    end++;

                // Calculate the average rank for the ties
                double averageRank = (start + end + 2) / 2.0;  // +2 because ranks are 1-based

                // Assign rank and accumulate rank sums for sample1
                for (int j = start; j <= end; j++)
                {
                    combined[j].Rank = averageRank;  // Assign the computed average rank
                    if (combined[j].Group == 1)
                        sumRanks += averageRank;
                }

                // Move to the next group of values
                i = end + 1;
            }

            // Calculate the U statistics
            double U1 = sumRanks - (sample1.Length * (sample1.Length + 1)) / 2.0;
            double U2 = sample1.Length * sample2.Length - U1;
            double U = Math.Min(U1, U2);

            // Calculate the mean and standard deviation of U for the normal approximation
            double meanU = sample1.Length * sample2.Length / 2.0;
            double sigmaU = Math.Sqrt(sample1.Length * sample2.Length * (sample1.Length + sample2.Length + 1) / 12.0);

            // Calculate the z-score, with continuity correction of 0.5
            double z = (U - meanU + 0.5) / sigmaU;

            // Calculate the p-value using the normal distribution approximation
            double p = 2 * (1 - NormalCdf(z));  // Two-tailed test

            return Tuple.Create(U, z, p);
        }

        private static double NormalCdf(double z)
        {
            double t = 1.0 / (1.0 + 0.2316419 * Math.Abs(z));
            double y = 1.0 - 1.0 / Math.Sqrt(2 * Math.PI) * Math.Exp(-0.5 * z * z) *
                       (0.319381530 * t - 0.356563782 * t * t + 1.781477937 * t * t * t - 1.821255978 * t * t * t * t + 1.330274429 * t * t * t * t * t);
            return z < 0 ? 1.0 - y : y;
        }
    }

    // Define the RankedSample class that will be used to store values and their ranks
    public class RankedSample
    {
        public double Value { get; set; }
        public int Group { get; set; }
        public double Rank { get; set; } // Property to store rank
    }
}
