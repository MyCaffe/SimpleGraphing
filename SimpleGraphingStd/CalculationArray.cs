using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleGraphingStd
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

        public Tuple<double, double> CalculateTTest(CalculationArray rg)
        {
            if (rg.Count != Count)
                throw new Exception("The two arrays must have the same number of items to calculate a t-test.");

            // Calculate means
            double mean1 = Average;
            double mean2 = rg.Average;

            // Calculate variances
            double variance1 = 0;
            double variance2 = 0;

            // Calculate variance for first array
            for (int i = 0; i < Count; i++)
            {
                variance1 += Math.Pow(m_rgdf[i] - mean1, 2);
            }
            variance1 /= (Count - 1);  // Use n-1 for sample variance

            // Calculate variance for second array
            for (int i = 0; i < Count; i++)
            {
                variance2 += Math.Pow(rg.m_rgdf[i] - mean2, 2);
            }
            variance2 /= (Count - 1);  // Use n-1 for sample variance

            // Check for zero variance
            if (variance1 == 0 || variance2 == 0)
                return new Tuple<double, double>(0, 0);

            // Calculate standard error
            double standardError = Math.Sqrt((variance1 / Count) + (variance2 / Count));

            // Calculate t-statistic
            double tStat = (mean1 - mean2) / standardError;

            // Calculate degrees of freedom
            // Fix 1: Changed from 2 * Count - 2 to Count + Count - 2
            int df = Count + Count - 2;

            // Calculate p-value
            double pValue = 2 * (1 - StudentT_CDF(Math.Abs(tStat), df));
            return new Tuple<double, double>(tStat, pValue);
        }

        private double StudentT_CDF(double t, int df)
        {
            double x = df / (df + t * t);
            return 1 - 0.5 * IncompleteBeta(x, df / 2.0, 0.5);
        }

        private double IncompleteBeta(double x, double a, double b)
        {
            if (x == 0) return 0;
            if (x == 1) return 1;

            // Fix 2: Complete implementation of incomplete beta function
            return BetaIncomplete(a, b, x);
        }

        private double BetaIncomplete(double a, double b, double x)
        {
            // Using continued fraction method for more accurate results
            double bt = Math.Exp(LogGamma(a + b) - LogGamma(a) - LogGamma(b) +
                        a * Math.Log(x) + b * Math.Log(1 - x));

            if (x < (a + 1) / (a + b + 2))
                return bt * BetaCF(a, b, x) / a;
            else
                return 1 - bt * BetaCF(b, a, 1 - x) / b;
        }

        private double BetaCF(double a, double b, double x)
        {
            int maxIterations = 200;
            double epsilon = 3.0e-7;
            double qab = a + b;
            double qap = a + 1.0;
            double qam = a - 1.0;
            double c = 1.0;
            double d = 1.0 - qab * x / qap;

            if (Math.Abs(d) < double.MinValue) d = double.MinValue;
            d = 1.0 / d;
            double h = d;

            for (int m = 1; m <= maxIterations; m++)
            {
                int m2 = 2 * m;
                double aa = m * (b - m) * x / ((qam + m2) * (a + m2));
                d = 1.0 + aa * d;
                if (Math.Abs(d) < double.MinValue) d = double.MinValue;
                c = 1.0 + aa / c;
                if (Math.Abs(c) < double.MinValue) c = double.MinValue;
                d = 1.0 / d;
                h *= d * c;
                aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2));
                d = 1.0 + aa * d;
                if (Math.Abs(d) < double.MinValue) d = double.MinValue;
                c = 1.0 + aa / c;
                if (Math.Abs(c) < double.MinValue) c = double.MinValue;
                d = 1.0 / d;
                double del = d * c;
                h *= del;
                if (Math.Abs(del - 1.0) < epsilon) break;
            }

            return h;
        }

        private double LogGamma(double z)
        {
            // Your existing LogGamma implementation is correct
            double[] c = {
                76.18009172947146,
                -86.50532032941677,
                24.01409824083091,
                -1.231739572450155,
                0.1208650973866179e-2,
                -0.5395239384953e-5
            };
            double x = z;
            double tmp = x + 5.5;
            tmp -= (x + 0.5) * Math.Log(tmp);
            double ser = 1.000000000190015;
            for (int j = 0; j < 6; j++)
            {
                x += 1;
                ser += c[j] / x;
            }
            return -tmp + Math.Log(2.5066282746310005 * ser / z);
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
            int currentRank = 1;  // Start with rank 1
            int i = 0;

            while (i < combined.Count)
            {
                int start = i;
                int end = i;

                // Determine the extent of ties
                while (end < combined.Count - 1 && combined[end + 1].Value == combined[start].Value)
                    end++;

                // Calculate the average rank for the ties
                double averageRank = (currentRank + (currentRank + (end - start))) / 2.0;

                // Assign rank and accumulate rank sums for sample1
                for (int j = start; j <= end; j++)
                {
                    combined[j].Rank = averageRank;
                    if (combined[j].Group == 1)
                        sumRanks += averageRank;
                }

                // Move to the next group of values
                i = end + 1;
                currentRank += (end - start + 1);  // Increment rank by number of tied values
            }

            // Rest of your code remains the same
            double U1 = sumRanks - (sample1.Length * (sample1.Length + 1)) / 2.0;
            double U2 = sample1.Length * sample2.Length - U1;
            double U = Math.Min(U1, U2);

            double meanU = sample1.Length * sample2.Length / 2.0;
            double sigmaU = Math.Sqrt(sample1.Length * sample2.Length * (sample1.Length + sample2.Length + 1) / 12.0);

            double z = (U - meanU) / sigmaU;
            double p = 2 * (1 - NormalCdf(Math.Abs(z)));

            return Tuple.Create(U, z, p);
        }

        static double NormalCdf(double z)
        {
            return 0.5 * (1.0 + Erf(z / Math.Sqrt(2.0)));
        }

        static double Erf(double x)
        {
            // Constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = Math.Sign(x);
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
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
