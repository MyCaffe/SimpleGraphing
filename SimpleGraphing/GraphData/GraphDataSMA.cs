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

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx)
        {
            PlotCollection data = dataset[nDataIdx];
            PlotCollection data1 = new PlotCollection(data.Name + " SMA");
            double dfSma = 0;
            double dfInc = 1.0 / m_config.Interval;

            for (int i = 0; i < data.Count; i++)
            {
                dfSma = (dfSma * (1 - dfInc)) + data[i].Y * dfInc;
                data1.Add(dfSma, (i >= m_config.Interval) ? true : false, data[i].Index);
            }

            return new PlotCollectionSet(new List<PlotCollection>() { data1 });
        }
    }
}
