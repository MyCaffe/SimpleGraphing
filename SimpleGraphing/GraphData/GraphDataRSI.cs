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

        public RsiData Pre(PlotCollectionSet dataset, int nDataIdx)
        {
            PlotCollection dataSrc = dataset[nDataIdx];
            PlotCollection dataDst = new PlotCollection(dataSrc.Name + " RSI" + m_config.Interval.ToString());
            return new RsiData(dataSrc, dataDst, m_config.Interval);
        }

        /// <summary>
        /// Calculate the RSI based on Wilder's formula.
        /// </summary>
        /// <remarks>
        /// @see [Relative Strength Index](https://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi)
        /// </remarks>
        /// <param name="data">Specifies the RSI data from the previous cycle.</param>
        /// <param name="i">Specifies the current data index.</param>
        /// <param name="minmax">Currently, not used here.</param>
        /// <param name="nLookahead">Specifies the look ahead value if any.</param>
        /// <param name="bAddToParams">Optionally, specifies whether or not to add the RSI to the parameters of the original data.</param>
        /// <returns>The new RSI value is returned.</returns>
        public double Process(RsiData data, int i, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false)
        {
            bool bActive = false;

            Plot plot = new Plot(data.SrcData[i].X, 0, null, false, data.SrcData[i].Index, data.SrcData[i].Action1Active, data.SrcData[i].Action2Active);
            data.DstData.Add(plot, false);

            if (i > 0)
            {
                double dfChange = data.SrcData[i].Y - data.SrcData[i - 1].Y;

                if (i <= m_config.Interval + 1)
                {
                    if (dfChange < 0)
                        data.AveLoss += (dfChange * -1);
                    else
                        data.AveGain += dfChange;

                    if (i == m_config.Interval + 1)
                    {
                        data.AveLoss /= data.Interval;
                        data.AveGain /= data.Interval;
                        data.Rs = (data.AveLoss == 0) ? 0 : data.AveGain / data.AveLoss;
                        data.RSI = 100 - (100 / (1 + data.Rs));
                        bActive = true;
                    }
                }
                else if (i > data.Interval + 1)
                {
                    if (dfChange < 0)
                    {
                        data.AveLoss = ((data.AveLoss * (data.Interval - 1)) + (-1 * dfChange)) / data.Interval;
                        data.AveGain = ((data.AveGain * (data.Interval - 1)) + (0)) / data.Interval;
                    }
                    else
                    {
                        data.AveLoss = ((data.AveLoss * (data.Interval - 1)) + (0)) / data.Interval;
                        data.AveGain = ((data.AveGain * (data.Interval - 1)) + (dfChange)) / data.Interval;
                    }

                    data.Rs = (data.AveLoss == 0) ? 0 : data.AveGain / data.AveLoss;
                    data.RSI = 100 - (100 / (1 + data.Rs));
                    bActive = true;
                }
            }

            data.DstData[i].Y = (float)data.RSI;
            data.DstData[i].Active = bActive;

            if (bAddToParams && bActive)
                data.SrcData[i].SetParameter(data.DstData.Name, (float)data.RSI);

            return data.RSI;
        }

        public RsiData GetRsiData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false)
        {
            RsiData data = Pre(dataset, nDataIdx);

            for (int i = 0; i < data.SrcData.Count; i++)
            {
                Process(data, i, null, nLookahead, bAddToParams);
            }

            MinMax minmax = new MinMax();
            minmax.Add(0);
            minmax.Add(100);

            data.DstData.SetMinMax(minmax);

            return data;
        }

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            RsiData data = GetRsiData(dataset, nDataIdx, nLookahead, bAddToParams);
            return new PlotCollectionSet(new List<PlotCollection>() { data.DstData });
        }
    }

    public class RsiData
    {
        PlotCollection m_src;
        PlotCollection m_dst;
        int m_nCount;
        int m_nInterval;
        double m_dfRsi;
        double m_dfAveGain = 0;
        double m_dfAveLoss = 0;
        double m_dfRs = 0;

        public RsiData(PlotCollection src, PlotCollection dst, uint nInterval)
        {
            m_src = src;
            m_dst = dst;
            m_nCount = 0;
            m_nInterval = (int)nInterval;
            m_dfRsi = 0;
        }

        public PlotCollection SrcData
        {
            get { return m_src; }
        }

        public PlotCollection DstData
        {
            get { return m_dst; }
        }

        public int Count
        {
            get { return m_nCount; }
            set { m_nCount = value; }
        }

        public double RSI
        {
            get { return m_dfRsi; }
            set { m_dfRsi = value; }
        }

        public double AveGain
        {
            get { return m_dfAveGain; }
            set { m_dfAveGain = value; }
        }

        public double AveLoss
        {
            get { return m_dfAveLoss; }
            set { m_dfAveLoss = value; }
        }

        public double Rs
        {
            get { return m_dfRs; }
            set { m_dfRs = value; }
        }

        public int Interval
        {
            get { return m_nInterval; }
        }
    }
}
