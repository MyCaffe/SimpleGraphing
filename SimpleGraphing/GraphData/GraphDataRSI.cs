using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
{
    public class GraphDataRSI : IGraphPlotData 
    {
        ConfigurationPlot m_config;

        public GraphDataRSI(ConfigurationPlot config)
        {
            m_config = config;
        }

        public string Name
        {
            get { return "RSI"; }
        }

        public string RenderType
        {
            get { return "LINE"; }
        }

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        /// <summary>
        /// Calculate the RSI based on Wilder's formula.
        /// </summary>
        /// <remarks>
        /// @see [Relative Strength Index](https://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi)
        /// </remarks>
        /// <param name="dataset">Specifies the original plot data.</param>
        /// <param name="nDataIdx">Specifies the data index of the plot data to use.</param>
        /// <param name="nLookahead">Specifies the look ahead value if any.</param>
        /// <param name="guid">Specifies the unique GUID for the data.</param>
        /// <param name="bAddToParams">Optionally, specifies whether or not to add the RSI to the parameters of the original data.</param>
        /// <returns>The new plot data containing the RSI calculation is returned.</returns>
        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            PlotCollection data = dataset[nDataIdx];

            if (data.Count < m_config.Interval * 5)
            {
                Trace.WriteLine("There is not enough data for an RSI calculation!");
                return null;
            }

            PlotCollection data1 = new PlotCollection(data.Name + " RSI");
            List<double> rgChange = new List<double>();
            double dfRSI = 0;
            double dfRSILast = 0;
            double dfSumLoss = 0;
            double dfSumGain = 0;

            MinMax minmax = new MinMax();
            minmax.Add(0);
            minmax.Add(100);

            data1.Add(new Plot(data[0].X, 0, null, false, data[0].Index, data[0].Action1Active));

            for (int i = 1; i < data.Count; i++)
            {
                double dfC0 = (data[i - 1].Y_values.Count == 1) ? data[i - 1].Y : data[i - 1].Y_values[3];
                double dfC1 = (data[i].Y_values.Count == 1) ? data[i].Y : data[i].Y_values[3];
                double dfChange = dfC1 - dfC0;
                bool bActive = false;

                rgChange.Add(dfChange);

                if (dfChange < 0)
                    dfSumLoss += dfChange;
                else
                    dfSumGain += dfChange;

                if (i > m_config.Interval)
                {
                    dfChange = rgChange[0];
                    rgChange.RemoveAt(0);

                    if (dfChange < 0)
                        dfSumLoss -= dfChange;
                    else
                        dfSumGain -= dfChange;

                    double dfAveGain = dfSumGain / m_config.Interval;
                    double dfAveLoss = Math.Abs(dfSumLoss / m_config.Interval);
                    double dfRS = (dfAveLoss == 0) ? 0 : dfAveGain / dfAveLoss;

                    // make sure to only calculate a new RSI if we are before the lookahead 
                    // so as to avoid impacting the last RSI with future data.
                    if (i < data.Count - nLookahead)
                    {
                        dfRSI = 100.0 - (100 / (1 + dfRS));
                        bActive = true;
                    }

                    if (dfRSI == 0)
                        dfRSI = dfRSILast;

                    dfRSILast = dfRSI;
                }

                data1.Add(new Plot(data[i].X, dfRSI, null, bActive, data[i].Index, data[i].Action1Active, data[i].Action2Active));

                if (bAddToParams && bActive)
                    data[i].SetParameter(data1.Name, dfRSI);
            }

            data1.SetMinMax(minmax);

            return new PlotCollectionSet(new List<PlotCollection>() { data1 });
        }
    }
}
