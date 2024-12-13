using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
{
    public class GraphDataZLEMA : IGraphPlotData
    {
        ConfigurationPlot m_config;

        public GraphDataZLEMA(ConfigurationPlot config)
        {
            m_config = config;
        }

        public ConfigurationPlot Configuration
        {
            get { return m_config; }
        }

        public string Name
        {
            get { return "ZLEMA"; }
        }

        public string RenderType
        {
            get { return "LINE"; }
        }

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        public ZlemaData Pre(PlotCollectionSet dataset, int nDataIdx, PlotCollection dataDst = null)
        {
            PlotCollection dataSrc = dataset[nDataIdx];

            if (dataDst == null)
                dataDst = new PlotCollection(dataSrc.Name + " ZLEMA" + m_config.Interval.ToString());

            return new ZlemaData(dataSrc, dataDst, m_config.Interval);
        }

        public double Process(ZlemaData data, int i, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false, PlotCollection plotsPrimary = null)
        {
            bool bActive;
            return Process(data, i, out bActive, minmax, nLookahead, bAddToParams, false, plotsPrimary);
        }

        public double Process(ZlemaData data, int i, out bool bActive, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false, bool bIgnoreDst = false, PlotCollection plotsPrimary = null)
        {
            bActive = false;

            PlotCollection dataSrc = data.SrcData;
            PlotCollection dataDst = data.DstData;
            double dfMult = data.Multiplier;
            int nLag = data.Lag;

            if (i < dataSrc.Count)
            {
                if (data.Index < m_config.Interval)
                {
                    if (data.SrcData[i].Active)
                    {
                        data.Total += dataSrc[i].Y;
                        data.Index++;
                        if (dataDst != null && !bIgnoreDst)
                            dataDst.Add(dataSrc[i].X, data.Total / (data.Index + 1), false, dataSrc[i].Index, true);
                    }
                    else
                    {
                        if (dataDst != null && !bIgnoreDst)
                            dataDst.Add(dataSrc[i].X, dataSrc[i].Y, false, dataSrc[i].Index, true);
                    }
                }
                else
                {
                    if (data.ZLEMA == 0)
                        data.ZLEMA = data.Total / m_config.Interval;

                    if (i < dataSrc.Count - nLookahead)
                    {
                        // Get lag price (use current price if not enough history)
                        double lagPrice = (i >= nLag) ? dataSrc[i - nLag].Y : dataSrc[i].Y;

                        // Calculate zero-lag price
                        double zeroLagPrice = dataSrc[i].Y + (dataSrc[i].Y - lagPrice);

                        // Calculate ZLEMA
                        data.ZLEMA = (zeroLagPrice - data.ZLEMA) * data.Multiplier + data.ZLEMA;
                        bActive = true;
                    }
                    else
                        bActive = false;

                    if (dataDst != null && !bIgnoreDst)
                        dataDst.Add(data.ZLEMA, bActive, dataSrc[i].Index, true);

                    if (bAddToParams && bActive)
                    {
                        string strName = dataDst.Name.Trim();
                        if (!string.IsNullOrEmpty(m_config.Name))
                            strName = m_config.Name;

                        if (plotsPrimary != null)
                            plotsPrimary[i].SetParameter(strName, (float)data.ZLEMA);
                        dataSrc[i].SetParameter(strName, (float)data.ZLEMA);
                    }

                    if (minmax != null)
                        minmax.Add(data.ZLEMA);
                }
            }

            return data.ZLEMA;
        }

        public ZlemaData GetZlemaData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false)
        {
            ZlemaData data = Pre(dataset, nDataIdx);
            MinMax minmax = new MinMax();

            for (int i = 0; i < data.SrcData.Count; i++)
            {
                Process(data, i, minmax, nLookahead, bAddToParams, (nDataIdx != 0) ? dataset[0] : null);
            }

            data.DstData.SetMinMax(minmax);

            return data;
        }

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            ZlemaData data = GetZlemaData(dataset, nDataIdx, nLookahead, bAddToParams);
            return new PlotCollectionSet(new List<PlotCollection>() { data.DstData });
        }
    }

    public class ZlemaData
    {
        PlotCollection m_src;
        PlotCollection m_dst;
        double m_dfZlema;
        double m_dfMult;
        double m_dfTotal;
        int m_nIdx = 0;
        int m_nLag;

        public ZlemaData(PlotCollection src, PlotCollection dst, uint nInterval)
        {
            m_src = src;
            m_dst = dst;
            m_dfZlema = 0;
            m_dfTotal = 0;
            m_dfMult = 2.0 / (nInterval + 1);
            m_nLag = (int)(nInterval - 1) / 2;
        }

        public PlotCollection SrcData
        {
            get { return m_src; }
            set { m_src = value; }
        }

        public PlotCollection DstData
        {
            get { return m_dst; }
        }

        public double ZLEMA
        {
            get { return m_dfZlema; }
            set { m_dfZlema = value; }
        }

        public double Total
        {
            get { return m_dfTotal; }
            set { m_dfTotal = value; }
        }

        public int Index
        {
            get { return m_nIdx; }
            set { m_nIdx = value; }
        }

        public double Multiplier
        {
            get { return m_dfMult; }
        }

        public int Lag
        {
            get { return m_nLag; }
        }
    }
}