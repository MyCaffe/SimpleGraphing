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

        public PlotCollection GetData(PlotCollection data)
        {
            PlotCollection data1 = new PlotCollection(data.Name);
            double dfSma = 0;
            double dfInc = 1.0 / m_config.Interval;

            for (int i = 0; i < data.Count; i++)
            {
                dfSma = (dfSma * (1 - dfInc)) + data[i].Y * dfInc;
                data1.Add(dfSma, (i >= m_config.Interval) ? true : false);
            }

            return data1;
        }
    }
}
