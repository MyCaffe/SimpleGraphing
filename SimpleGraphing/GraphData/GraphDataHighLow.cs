using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
{
    public class GraphDataHighLow : IGraphPlotData 
    {
        ConfigurationPlot m_config;

        public GraphDataHighLow(ConfigurationPlot config)
        {
            m_config = config;
        }

        public string Name
        {
            get { return "HIGHLOW"; }
        }

        public string RenderType
        {
            get { return "HIGHLOW"; }
        }

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx)
        {
            PlotCollection data = dataset[nDataIdx];
            List<PlotCollection> rgPlots = new List<PlotCollection>();
            PlotCollection plotHigh1 = getHighPoints(data, 1);
            PlotCollection plotLow1 = getLowPoints(data, 1);
            PlotCollection plotHigh2 = getHighPoints(plotHigh1, 2);
            PlotCollection plotLow2 = getLowPoints(plotLow1, 2);
            PlotCollection plotHigh3 = getHighPoints(plotHigh2, 3);
            PlotCollection plotLow3 = getLowPoints(plotLow2, 3);

            rgPlots.Add(plotLow1);
            rgPlots.Add(plotHigh1);
            rgPlots.Add(plotLow2);
            rgPlots.Add(plotHigh2);
            rgPlots.Add(plotLow3);
            rgPlots.Add(plotHigh3);

            return new PlotCollectionSet(rgPlots);
        }

        private PlotCollection combine(string strName, PlotCollection p1, PlotCollection p2)
        {
            PlotCollection p = new PlotCollection(strName + " high/low");

            if (p1.Count != p2.Count)
                throw new Exception("The two plot collections must have the same number of items!");

            for (int i = 0; i < p1.Count; i++)
            {
                if (p1[i].Active)
                    p.Add(p1[i]);
                else if (p2[i].Active)
                    p.Add(p2[i]);
            }

            return p;
        }

        private PlotCollection getHighPoints(PlotCollection data, int nLevel)
        {
            if (data == null || data.Count < 3)
                return null;

            PlotCollection dataHigh = new PlotCollection(data.Name + " H" + nLevel.ToString());

            int nOpen = data[0].PrimaryIndexY;
            int nHigh = data[0].PrimaryIndexY;
            int nLow = data[0].PrimaryIndexY;
            int nClose = data[0].PrimaryIndexY;

            if (data[0].Y_values.Count == 4)
            {
                nHigh = 1;
                nLow = 2;
                nClose = 3;
            }

            List<Tuple<int, Plot>> rgActive = new List<Tuple<int, Plot>>();

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Active)
                    rgActive.Add(new Tuple<int, Plot>(i, data[i]));
            }

            if (rgActive.Count < 3)
                return null;

            int nIdx = 1;
            for (int i = 0; i < data.Count; i++)
            {
                int nIdxCurrent = rgActive[nIdx].Item1;

                if (i == nIdxCurrent && nIdx < rgActive.Count-1)
                {
                    double dfOpen = data[nIdxCurrent].Y_values[nOpen];
                    double dfClose = data[nIdxCurrent].Y_values[nClose];
                    double dfHigh1 = data[nIdxCurrent].Y_values[nHigh];
                    double dfLow1 = data[nIdxCurrent].Y_values[nLow];
                    int nIdxPast = rgActive[nIdx - 1].Item1;
                    double dfHigh0 = data[nIdxPast].Y_values[nHigh];
                    double dfLow0 = data[nIdxPast].Y_values[nLow];
                    int nIdxFuture = rgActive[nIdx + 1].Item1;
                    double dfHigh2 = data[nIdxFuture].Y_values[nHigh];
                    double dfLow2 = data[nIdxFuture].Y_values[nLow];
                    bool bHigh = false;

                    if (dfHigh1 > dfHigh0 && dfHigh1 > dfHigh2)
                        bHigh = true;

                    dataHigh.Add(new Plot(data[nIdxCurrent].X, dfHigh1, null, bHigh, data[nIdxCurrent].Index));
                    nIdx++;
                }
                else
                {
                    dataHigh.Add(new Plot(data[i].X, data[i].Y_values[nHigh], null, false, data[i].Index));
                }
            }

            return dataHigh;
        }

        private PlotCollection getLowPoints(PlotCollection data, int nLevel)
        {
            if (data == null || data.Count < 3)
                return null;

            PlotCollection dataLow = new PlotCollection(data.Name + " L" + nLevel.ToString());

            int nOpen = data[0].PrimaryIndexY;
            int nHigh = data[0].PrimaryIndexY;
            int nLow = data[0].PrimaryIndexY;
            int nClose = data[0].PrimaryIndexY;

            if (data[0].Y_values.Count == 4)
            {
                nHigh = 1;
                nLow = 2;
                nClose = 3;
            }

            List<Tuple<int, Plot>> rgActive = new List<Tuple<int, Plot>>();

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Active)
                    rgActive.Add(new Tuple<int, Plot>(i, data[i]));
            }

            if (rgActive.Count < 3)
                return null;

            int nIdx = 1;
            for (int i = 0; i < data.Count; i++)
            {
                int nIdxCurrent = rgActive[nIdx].Item1;

                if (i == nIdxCurrent && nIdx < rgActive.Count - 1)
                {
                    double dfOpen = data[nIdxCurrent].Y_values[nOpen];
                    double dfClose = data[nIdxCurrent].Y_values[nClose];
                    double dfHigh1 = data[nIdxCurrent].Y_values[nHigh];
                    double dfLow1 = data[nIdxCurrent].Y_values[nLow];
                    int nIdxPast = rgActive[nIdx - 1].Item1;
                    double dfHigh0 = data[nIdxPast].Y_values[nHigh];
                    double dfLow0 = data[nIdxPast].Y_values[nLow];
                    int nIdxFuture = rgActive[nIdx + 1].Item1;
                    double dfHigh2 = data[nIdxFuture].Y_values[nHigh];
                    double dfLow2 = data[nIdxFuture].Y_values[nLow];
                    bool bLow = false;

                    if (dfLow1 < dfLow0 && dfLow1 < dfLow2)
                        bLow = true;

                    dataLow.Add(new Plot(data[nIdxCurrent].X, dfLow1, null, bLow, data[nIdxCurrent].Index));
                    nIdx++;
                }
                else
                {
                    dataLow.Add(new Plot(data[i].X, data[i].Y_values[nLow], null, false, data[i].Index));
                }
            }

            return dataLow;
        }
    }
}
