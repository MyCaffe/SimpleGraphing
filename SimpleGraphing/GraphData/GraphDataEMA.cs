using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
{
    public class GraphDataEMA : IGraphPlotData 
    {
        ConfigurationPlot m_config;

        public GraphDataEMA(ConfigurationPlot config)
        {
            m_config = config;
        }

        public ConfigurationPlot Configuration
        {
            get { return m_config; }
        }

        public string Name
        {
            get { return "EMA"; }
        }

        public string RenderType
        {
            get { return "LINE"; }
        }

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        public EmaData Pre(PlotCollectionSet dataset, int nDataIdx)
        {
            PlotCollection dataSrc = dataset[nDataIdx];
            PlotCollection dataDst = new PlotCollection(dataSrc.Name + " EMA" + m_config.Interval.ToString());
            return new EmaData(dataSrc, dataDst, m_config.Interval);
        }

        public double Process(EmaData data, int i, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false)
        {
            bool bActive = data.SrcData[i].Active;

            PlotCollection dataSrc = data.SrcData;
            PlotCollection dataDst = data.DstData;
            double dfMult = data.Multiplier;

            if (i < dataSrc.Count)
            {
                if (data.Index < m_config.Interval)
                {
                    if (bActive)
                    {
                        data.Total += dataSrc[i].Y;
                        data.Index++;
                        if (dataDst != null)
                            dataDst.Add(dataSrc[i].X, data.Total / (data.Index + 1), false, dataSrc[i].Index);
                    }
                    else
                    {
                        if (dataDst != null)
                            dataDst.Add(dataSrc[i].X, dataSrc[i].Y, false, dataSrc[i].Index);
                    }
                }
                else
                {
                    if (data.EMA == 0)
                        data.EMA = data.Total / m_config.Interval;

                    if (i < dataSrc.Count - nLookahead)
                        data.EMA = (dataSrc[i].Y - data.EMA) * data.Multiplier + data.EMA;
                    else
                        bActive = false;

                    if (dataDst != null)
                        dataDst.Add(data.EMA, bActive, dataSrc[i].Index);

                    if (bAddToParams && bActive)
                        dataSrc[i].SetParameter(dataDst.Name, (float)data.EMA);

                    if (minmax != null)
                        minmax.Add(data.EMA);
                }
            }

            return data.EMA;
        }

        public EmaData GetEmaData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false)
        {
            EmaData data = Pre(dataset, nDataIdx);
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
            EmaData data = GetEmaData(dataset, nDataIdx, nLookahead, bAddToParams);
            return new PlotCollectionSet(new List<PlotCollection>() { data.DstData });
        }
    }

    public class EmaData
    {
        PlotCollection m_src;
        PlotCollection m_dst;
        double m_dfEma;
        double m_dfMult;
        double m_dfTotal;
        int m_nIdx = 0;

        public EmaData(PlotCollection src, PlotCollection dst, uint nInterval)
        {
            m_src = src;
            m_dst = dst;
            m_dfEma = 0;
            m_dfTotal = 0;
            m_dfMult = 2.0 / (nInterval + 1);
        }

        public PlotCollection SrcData
        {
            get { return m_src; }
        }

        public PlotCollection DstData
        {
            get { return m_dst; }
        }

        public double EMA
        {
            get { return m_dfEma; }
            set { m_dfEma = value; }
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
    }
}
