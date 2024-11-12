using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphingStd.GraphData
{
    public class GraphDataSMA : IGraphPlotData 
    {
        ConfigurationPlot m_config;

        public GraphDataSMA(ConfigurationPlot config)
        {
            m_config = config;
        }

        public string Name
        {
            get { return "SMA"; }
        }

        public string RenderType
        {
            get { return "LINE"; }
        }

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        public SmaData Pre(PlotCollectionSet dataset, int nDataIdx)
        {
            PlotCollection dataSrc = dataset[nDataIdx];
            PlotCollection dataDst = new PlotCollection(dataSrc.Name + " SMA" + m_config.Interval.ToString());
            return new SmaData(dataSrc, dataDst, m_config.Interval);
        }

        public double Process(SmaData data, int i, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false, PlotCollection plotsPrimary = null)
        {
            bool bActive = data.SrcData[i].Active;

            PlotCollection dataSrc = data.SrcData;
            PlotCollection dataDst = data.DstData;
            double dfInc = data.Increment;

            if (bActive)
            {
                if (data.Count < m_config.Interval)
                {
                    data.SMA += dataSrc[i].Y * dfInc;

                    if (dataDst != null)
                        dataDst.Add(dataSrc[i].X, dataSrc[i].Y, false, dataSrc[i].Index, true);
                }
                else
                {
                    if (i < dataSrc.Count - nLookahead)
                        data.SMA = (data.SMA * (1 - dfInc)) + dataSrc[i].Y * dfInc;

                    if (dataDst != null)
                        dataDst.Add(dataSrc[i].X, data.SMA, true, dataSrc[i].Index, true);

                    if (bAddToParams)
                    {
                        string strName = dataDst.Name.Trim();
                        if (!string.IsNullOrEmpty(m_config.Name))
                            strName = m_config.Name;

                        if (plotsPrimary != null)
                            plotsPrimary[i].SetParameter(strName, (float)data.SMA);
                        dataSrc[i].SetParameter(strName, (float)data.SMA);
                    }
                }

                if (minmax != null)
                    minmax.Add(data.SMA);

                data.Count++;
            }
            else
            {
                if (dataDst != null)
                    dataDst.Add(dataSrc[i].X, dataSrc[i].Y, false, dataSrc[i].Index, true);
            }

            return data.SMA;
        }

        public SmaData GetSmaData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false)
        {
            SmaData data = Pre(dataset, nDataIdx);
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
            SmaData data = GetSmaData(dataset, nDataIdx, nLookahead, bAddToParams);
            return new PlotCollectionSet(new List<PlotCollection>() { data.DstData });
        }
    }

    public class SmaData
    {
        PlotCollection m_src;
        PlotCollection m_dst;
        int m_nCount;
        double m_dfSma;
        double m_dfInc;

        public SmaData(PlotCollection src, PlotCollection dst, uint nInterval)
        {
            m_src = src;
            m_dst = dst;
            m_nCount = 0;
            m_dfSma = 0;
            m_dfInc = 1.0 / nInterval;
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

        public double SMA
        {
            get { return m_dfSma; }
            set { m_dfSma = value; }
        }

        public double Increment
        {
            get { return m_dfInc; }
        }
    }
}
