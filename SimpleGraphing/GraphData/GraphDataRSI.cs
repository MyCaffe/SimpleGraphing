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
        /// <returns>The new plot data containing the RSI calculation is returned.</returns>
        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx)
        {
            PlotCollection data = dataset[nDataIdx];

            if (data.Count < m_config.Interval * 5)
            {
                Trace.WriteLine("There is not enough data for an RSI calculation!");
                return null;
            }

            PlotCollection data1 = new PlotCollection(data.Name + " RSI");
            List<double> rgGain = new List<double>();
            List<double> rgLoss = new List<double>();

            data1.Add(new Plot(data[0].X, 0, null, false, data[0].Index));

            for (int i = 1; i < data.Count; i++)
            {
                double dfC0 = data[i - 1].Y_values[3];
                double dfC1 = data[i].Y_values[3];
                double dfChange = dfC1 - dfC0;
                double dfRSI = 0;
                bool bActive = false;

                if (dfChange < 0)
                {
                    rgLoss.Add(dfChange);
                    rgGain.Add(0);
                }
                else
                {
                    rgLoss.Add(0);
                    rgGain.Add(dfChange);
                }

                if (i > m_config.Interval)
                {
                    double dfAveGain = rgGain.Average(p => p);
                    double dfAveLoss = Math.Abs(rgLoss.Average(p => p));
                    double dfRS = (dfAveLoss == 0) ? 0 : dfAveGain / dfAveLoss;
                    dfRSI = 100.0 - (100 / (1 + dfRS));
                    bActive = true;

                    rgGain.RemoveAt(0);
                    rgLoss.RemoveAt(0);
                }

                data1.Add(new Plot(data[i].X, dfRSI, null, bActive, data[i].Index));
            }

            return new PlotCollectionSet(new List<PlotCollection>() { data1 });
        }
    }
}
