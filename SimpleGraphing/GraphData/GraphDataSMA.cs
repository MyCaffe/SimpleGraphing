using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
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

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            PlotCollection data = dataset[nDataIdx];
            PlotCollection data1 = new PlotCollection(data.Name + " SMA");
            double dfSma = 0;
            double dfInc = 1.0 / m_config.Interval;
            int nCount = 0;

            MinMax minmax = new MinMax();

            for (int i = 0; i < data.Count; i++)
            {
                bool bActive = data[i].Active;

                if (bActive)
                {
                    if (nCount < m_config.Interval)
                    {
                        dfSma += data[i].Y * dfInc;   
                        data1.Add(data[i].X, data[i].Y, false, data[i].Index);
                    }
                    else
                    {
                        if (i < data.Count - nLookahead)
                            dfSma = (dfSma * (1 - dfInc)) + data[i].Y * dfInc;

                        data1.Add(data[i].X, dfSma, true, data[i].Index);

                        if (bAddToParams)
                            data[i].SetParameter(data1.Name, dfSma);
                    }

                    minmax.Add(dfSma);

                    nCount++;
                }
                else
                {
                    data1.Add(data[i].X, data[i].Y, false, data[i].Index);
                }
            }

            data1.SetMinMax(minmax);

            return new PlotCollectionSet(new List<PlotCollection>() { data1 });
        }
    }
}
