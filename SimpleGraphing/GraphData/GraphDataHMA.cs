using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
{
    /// <summary>
    /// Calculate the Hull Moving Average
    /// </summary>
    /// <remarks>
    /// @see [Hull Moving Average](https://school.stockcharts.com/doku.php?id=technical_indicators:hull_moving_average)
    /// </remarks>
    public class GraphDataHMA : IGraphPlotData 
    {
        ConfigurationPlot m_config;
        GraphDataEMA m_ema1;
        GraphDataEMA m_ema2;
        GraphDataEMA m_ema3;

        public GraphDataHMA(ConfigurationPlot config)
        {
            m_config = config;

            if (m_config.Interval < 4)
                m_config.Interval = 4;

            ConfigurationPlot ema1 = new ConfigurationPlot();
            ema1.PlotType = ConfigurationPlot.PLOTTYPE.EMA;
            ema1.Interval = m_config.Interval / 2;
            m_ema1 = new GraphDataEMA(ema1);

            ConfigurationPlot ema2 = new ConfigurationPlot();
            ema2.PlotType = ConfigurationPlot.PLOTTYPE.EMA;
            ema2.Interval = m_config.Interval;
            m_ema2 = new GraphDataEMA(ema2);

            ConfigurationPlot ema3 = new ConfigurationPlot();
            ema3.PlotType = ConfigurationPlot.PLOTTYPE.EMA;
            ema3.Interval = (uint)Math.Sqrt(m_config.Interval);
            m_ema3 = new GraphDataEMA(ema3);
        }

        public ConfigurationPlot Configuration
        {
            get { return m_config; }
        }

        public string Name
        {
            get { return "HMA"; }
        }

        public string RenderType
        {
            get { return "LINE"; }
        }

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        public HmaData Pre(PlotCollectionSet dataset, int nDataIdx, PlotCollection dataDst = null)
        {
            PlotCollection dataSrc = dataset[nDataIdx];

            if (dataDst == null)
                dataDst = new PlotCollection(dataSrc.Name + " HMA" + m_config.Interval.ToString());

            return new HmaData(dataSrc, dataDst, m_config.Interval);
        }

        public double Process(HmaData data, int i, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false)
        {
            bool bActive;
            return Process(data, i, out bActive, minmax, nLookahead, bAddToParams);
        }

        public double Process(HmaData data, int i, out bool bActive, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false, bool bIgnoreDst = false)
        {
            bool bActiveEma1;
            bool bActiveEma2;

            double dfEma1 = m_ema1.Process(data.EMA1, i, out bActiveEma1, null, nLookahead, false, true);
            double dfEma2 = m_ema2.Process(data.EMA2, i, out bActiveEma2, null, nLookahead, false, true);

            PlotCollection dataSrc = data.SrcData;
            PlotCollection dataDst = data.DstData;

            if (i < dataSrc.Count)
            {
                if (data.SrcData[i].Active && bActiveEma1 && bActiveEma2)
                {
                    double dfRawHma = (2 * dfEma1) - dfEma2;
                    data.TmpData[i].SetYValue((float)dfRawHma, true);
                }
                else
                {
                    data.TmpData[i].SetYValue(dataSrc[i].Y, false);
                }
            }

            data.EMA3.SrcData = data.TmpData;
            data.EMA3.Index = i;
            data.HMA = m_ema3.Process(data.EMA3, i, out bActive, null, 0, false, bIgnoreDst);

            if (i < m_ema2.Configuration.Interval + m_ema3.Configuration.Interval)
            {
                dataDst[i].SetYValue(data.SrcData[i].Y, false);
                data.HMA = data.SrcData[i].Y;
                bActive = false;
            }
            else
            {
                if (minmax != null)
                    minmax.Add(data.HMA);
            }

            if (bAddToParams && bActive)
                dataSrc[i].SetParameter(dataDst.Name.Trim(), data.HMA);

            return data.HMA;
        }

        public HmaData GetHmaData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false)
        {
            HmaData data = Pre(dataset, nDataIdx);
            MinMax minmax = new MinMax();

            for (int i = 0; i < data.SrcData.Count; i++)
            {
                Process(data, i, minmax, nLookahead, bAddToParams);
            }

            data.DstData.SetMinMax(minmax);

            return data;
        }

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            HmaData data = GetHmaData(dataset, nDataIdx, nLookahead, bAddToParams);
            return new PlotCollectionSet(new List<PlotCollection>() { data.DstData });
        }
    }

    public class HmaData
    {
        PlotCollection m_src;
        PlotCollection m_dst;
        PlotCollection m_tmp;
        EmaData m_ema1;
        EmaData m_ema2;
        EmaData m_ema3;
        double m_dfHma;
        int m_nIdx = 0;

        public HmaData(PlotCollection src, PlotCollection dst, uint nInterval)
        {
            m_src = src;
            m_dst = dst;
            m_tmp = src.Clone(0, false, null, null, false, null, true);
            m_ema1 = new EmaData(src, dst, nInterval / 2);
            m_ema2 = new EmaData(src, dst, nInterval);
            m_ema3 = new EmaData(src, dst, (uint)Math.Sqrt(nInterval));
            m_dfHma = 0;
        }

        public PlotCollection SrcData
        {
            get { return m_src; }
        }

        public PlotCollection DstData
        {
            get { return m_dst; }
        }

        public PlotCollection TmpData
        {
            get { return m_tmp; }
        }

        public EmaData EMA1
        {
            get { return m_ema1; }
        }
        
        public EmaData EMA2
        {
            get { return m_ema2; }
        }
        
        public EmaData EMA3
        {
            get { return m_ema3; }
        }

        public double HMA
        {
            get { return m_dfHma; }
            set { m_dfHma = value; }
        }

        public int Index
        {
            get { return m_nIdx; }
            set { m_nIdx = value; }
        }
    }
}
