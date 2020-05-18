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

            if (data.Count < m_config.Interval * 1)
            {
                Trace.WriteLine("There is not enough data for an RSI calculation!");
                return null;
            }

            PlotCollection data1 = new PlotCollection(data.Name + " RSI");
            float fAveGain = 0;
            float fAveLoss = 0;
            float fRs = 0;
            float fRsi = 0;

            for (int i = 0; i < data.Count; i++)
            {
                bool bActive = false;
                data1.Add(new Plot(data[i].X, 0, null, false, data[i].Index, data[i].Action1Active, data[i].Action2Active));

                if (i > 0)
                {
                    float fChange = data[i].Y - data[i - 1].Y;

                    if (i <= m_config.Interval + 1)
                    {
                        if (fChange < 0)
                            fAveLoss += (fChange * -1);
                        else
                            fAveGain += fChange;

                        if (i == m_config.Interval + 1)
                        {
                            fAveLoss /= m_config.Interval;
                            fAveGain /= m_config.Interval;
                            fRs = (fAveLoss == 0) ? 0 : fAveGain / fAveLoss;
                            fRsi = 100 - (100 / (1 + fRs));
                            bActive = true;
                        }
                    }
                    else if (i > m_config.Interval + 1)
                    {
                        if (fChange < 0)
                        {
                            fAveLoss = ((fAveLoss * (m_config.Interval - 1)) + (-1 * fChange)) / m_config.Interval;
                            fAveGain = ((fAveGain * (m_config.Interval - 1)) + (0)) / m_config.Interval;
                        }
                        else
                        {
                            fAveLoss = ((fAveLoss * (m_config.Interval - 1)) + (0)) / m_config.Interval;
                            fAveGain = ((fAveGain * (m_config.Interval - 1)) + (fChange)) / m_config.Interval;
                        }

                        fRs = (fAveLoss == 0) ? 0 : fAveGain / fAveLoss;
                        fRsi = 100 - (100 / (1 + fRs));
                        bActive = true;
                    }
                }

                data1[i].Y = fRsi;
                data1[i].Active = bActive;

                if (bAddToParams)
                    data[i].SetParameter(data1.Name, fRsi);
            }

            MinMax minmax = new MinMax();
            minmax.Add(0);
            minmax.Add(100);

            data1.SetMinMax(minmax);

            return new PlotCollectionSet(new List<PlotCollection>() { data1 });
        }
    }
}
