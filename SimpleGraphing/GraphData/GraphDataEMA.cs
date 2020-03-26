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

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            PlotCollection data = dataset[nDataIdx];
            PlotCollection data1 = new PlotCollection(data.Name + " EMA");
            double dfTotal = 0;
            double dfEma = 0;
            double dfMult = 2.0 / (m_config.Interval + 1);
            int i = 0;

            MinMax minmax = new MinMax();
            int nIdx = 0;

            while (i < data.Count && nIdx < m_config.Interval)
            {
                if (data[i].Active)
                {
                    dfTotal += data[i].Y;
                    nIdx++;
                    data1.Add(data[i].X, dfTotal / (nIdx + 1), false, data[i].Index);
                }
                else
                {
                    data1.Add(data[i].X, data[i].Y, false, data[i].Index);
                }

                i++;
            }

            dfEma = dfTotal / m_config.Interval;

            while (i < data.Count)
            {
                bool bActive = true;

                if (i < data.Count - nLookahead)
                {
                    double dfVal = data[i].Y;
                    dfEma = (dfVal - dfEma) * dfMult + dfEma;
                }
                else
                {
                    bActive = false;
                }

                data1.Add(dfEma, bActive, data[i].Index);

                if (bAddToParams && bActive)
                    data[i].SetParameter(data1.Name, (float)dfEma);

                i++;

                minmax.Add(dfEma);
            }

            data1.SetMinMax(minmax);

            return new PlotCollectionSet(new List<PlotCollection>() { data1 });
        }
    }
}
