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

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx)
        {
            PlotCollection data = dataset[nDataIdx];
            PlotCollection data1 = new PlotCollection(data.Name + " EMA");
            double dfTotal = 0;
            double dfEma = 0;
            double dfMult = 2.0 / (m_config.Interval + 1);
            int i = 0;

            while (i < data.Count && i<m_config.Interval)
            {
                dfTotal += data[i].Y;
                data1.Add(dfTotal / (i + 1), false);
                i++;
            }

            dfEma = dfTotal / m_config.Interval;

            while (i < data.Count)
            {
                double dfVal = data[i].Y;
                dfEma = (dfVal - dfEma) * dfMult + dfEma;
                data1.Add(dfEma, true);
                i++;
            }

            return new PlotCollectionSet(new List<PlotCollection>() { data1 });
        }
    }
}
