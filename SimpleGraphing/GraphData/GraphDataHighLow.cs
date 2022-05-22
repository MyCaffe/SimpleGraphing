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

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            PlotCollection data = dataset[nDataIdx];

            List<PlotCollection> rgPlots = new List<PlotCollection>();
            PlotCollection plotHigh1 = getHighPoints(data, 1, nLookahead);
            PlotCollection plotLow1 = getLowPoints(data, 1, nLookahead);
            PlotCollection plotHigh2 = getHighPoints(plotHigh1, 2, nLookahead);
            PlotCollection plotLow2 = getLowPoints(plotLow1, 2, nLookahead);
            PlotCollection plotHigh3 = getHighPoints(plotHigh2, 3, nLookahead);
            PlotCollection plotLow3 = getLowPoints(plotLow2, 3, nLookahead);

            if (bAddToParams)
            {
                data = dataset[nDataIdx];

                for (int i = 0; i < data.Count; i++)
                {
                    if (plotLow1 != null && i < plotLow1.Count && plotLow1[i].Active)
                        data[i].SetParameter(plotLow1.Name, plotLow1[i].Y);

                    if (plotLow2 != null && i < plotLow2.Count && plotLow2[i].Active)
                        data[i].SetParameter(plotLow2.Name, plotLow2[i].Y);

                    if (plotLow3 != null && i < plotLow3.Count && plotLow3[i].Active)
                        data[i].SetParameter(plotLow3.Name, plotLow3[i].Y);

                    if (plotHigh1 != null && i < plotHigh1.Count && plotHigh1[i].Active)
                        data[i].SetParameter(plotHigh1.Name, plotHigh1[i].Y);

                    if (plotHigh2 != null && i < plotHigh2.Count && plotHigh2[i].Active)
                        data[i].SetParameter(plotHigh2.Name, plotHigh2[i].Y);

                    if (plotHigh3 != null && i < plotHigh3.Count && plotHigh3[i].Active)
                        data[i].SetParameter(plotHigh3.Name, plotHigh3[i].Y);
                }
            }

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

        private PlotCollection getHighPoints(PlotCollection data, int nLevel, int nLookahead)
        {
            if (data == null || data.Count < 3)
                return null;

            PlotCollection dataHigh = new PlotCollection(data.Name + " H" + nLevel.ToString());

            //int nOpen = data[0].PrimaryIndexY;
            int nHigh = data[0].PrimaryIndexY;
            //int nLow = data[0].PrimaryIndexY;
            //int nClose = data[0].PrimaryIndexY;

            if (data[0].Y_values.Length == 4)
            {
                nHigh = 1;
                //nLow = 2;
                //nClose = 3;
            }

            List<Tuple<int, Plot>> rgActive = new List<Tuple<int, Plot>>();

            for (int i = 0; i < data.Count - nLookahead; i++)
            {
                if (data[i].Active)
                    rgActive.Add(new Tuple<int, Plot>(i, data[i]));
            }

            if (rgActive.Count < 3)
                return null;

            MinMax minmax = new MinMax();

            int nIdx = 1;
            for (int i = 0; i < data.Count; i++)
            {
                int nIdxCurrent = rgActive[nIdx].Item1;

                if (i == nIdxCurrent && nIdx < rgActive.Count - 1)
                {
                    if (nIdxCurrent >= data.Count)
                        continue;

                    Plot plotCurrent = data[nIdxCurrent];

                    int nIdxPast = rgActive[nIdx - 1].Item1;
                    if (nIdxPast >= data.Count)
                        continue;

                    Plot plotPast = data[nIdxPast];

                    int nIdxFuture = rgActive[nIdx + 1].Item1;
                    if (nIdxFuture >= data.Count)
                        continue;

                    Plot plotFuture = data[nIdxFuture];

                    //double dfOpen = (plotCurrent.Y_values.Length == 1) ? plotCurrent.Y : plotCurrent.Y_values[nOpen];
                    //double dfClose = (plotCurrent.Y_values.Length == 1) ? plotCurrent.Y : plotCurrent.Y_values[nClose];
                    double dfHigh1 = (plotCurrent.Y_values.Length == 1) ? plotCurrent.Y : plotCurrent.Y_values[nHigh];
                    //double dfLow1 = (plotCurrent.Y_values.Length == 1) ? plotCurrent.Y : plotCurrent.Y_values[nLow];

                    double dfHigh0 = (plotPast.Y_values.Length == 1) ? plotPast.Y : plotPast.Y_values[nHigh];
                    //double dfLow0 = (plotPast.Y_values.Length == 1) ? plotPast.Y : plotPast.Y_values[nLow];
                    double dfHigh2 = (plotFuture.Y_values.Length == 1) ? plotFuture.Y : plotFuture.Y_values[nHigh];

                    bool bHigh = false;

                    if (dfHigh1 > dfHigh0 && dfHigh1 > dfHigh2)
                        bHigh = true;

                    minmax.Add(dfHigh1);

                    dataHigh.Add(new Plot(data[nIdxCurrent].X, dfHigh1, null, bHigh, data[nIdxCurrent].Index));
                    nIdx++;
                }
                else
                {
                    double dfHigh = (data[i].Y_values.Length == 1) ? data[i].Y : data[i].Y_values[nHigh];

                    dataHigh.Add(new Plot(data[i].X, dfHigh, null, false, data[i].Index));
                }
            }

            dataHigh.SetMinMax(minmax);

            return dataHigh;
        }

        private PlotCollection getLowPoints(PlotCollection data, int nLevel, int nLookahead)
        {
            if (data == null || data.Count < 3)
                return null;

            PlotCollection dataLow = new PlotCollection(data.Name + " L" + nLevel.ToString());

            //int nOpen = data[0].PrimaryIndexY;
            //int nHigh = data[0].PrimaryIndexY;
            int nLow = data[0].PrimaryIndexY;
            //int nClose = data[0].PrimaryIndexY;

            if (data[0].Y_values.Length == 4)
            {
                //nHigh = 1;
                nLow = 2;
                //nClose = 3;
            }

            List<Tuple<int, Plot>> rgActive = new List<Tuple<int, Plot>>();

            for (int i = 0; i < data.Count - nLookahead; i++)
            {
                if (data[i].Active)
                    rgActive.Add(new Tuple<int, Plot>(i, data[i]));
            }

            if (rgActive.Count < 3)
                return null;

            MinMax minmax = new MinMax();

            int nIdx = 1;
            for (int i = 0; i < data.Count; i++)
            {
                int nIdxCurrent = rgActive[nIdx].Item1;

                if (i == nIdxCurrent && nIdx < rgActive.Count - 1)
                {
                    if (nIdxCurrent >= data.Count)
                        continue;

                    Plot plotCurrent = data[nIdxCurrent];

                    int nIdxPast = rgActive[nIdx - 1].Item1;
                    if (nIdxPast >= data.Count)
                        continue;
                    
                    Plot plotPast = data[nIdxPast];

                    int nIdxFuture = rgActive[nIdx + 1].Item1;
                    if (nIdxFuture >= data.Count)
                        continue;

                    Plot plotFuture = data[nIdxFuture];

                    //double dfOpen = (plotCurrent.Y_values.Length == 1) ? plotCurrent.Y : plotCurrent.Y_values[nOpen];
                    //double dfClose = (plotCurrent.Y_values.Length == 1) ? plotCurrent.Y : plotCurrent.Y_values[nClose];
                    //double dfHigh1 = (plotCurrent.Y_values.Length == 1) ? plotCurrent.Y : plotCurrent.Y_values[nHigh];
                    double dfLow1 = (plotCurrent.Y_values.Length == 1) ? plotCurrent.Y : plotCurrent.Y_values[nLow];

                    //double dfHigh0 = (plotPast.Y_values.Length == 1) ? plotPast.Y : plotPast.Y_values[nHigh];
                    double dfLow0 = (plotPast.Y_values.Length == 1) ? plotPast.Y : plotPast.Y_values[nLow];

                    //double dfHigh2 = (plotFuture.Y_values.Length == 1) ? plotFuture.Y : plotFuture.Y_values[nHigh];
                    double dfLow2 = (plotFuture.Y_values.Length == 1) ? plotFuture.Y : plotFuture.Y_values[nLow];

                    bool bLow = false;

                    if (dfLow1 < dfLow0 && dfLow1 < dfLow2)
                        bLow = true;

                    minmax.Add(dfLow1);

                    dataLow.Add(new Plot(data[nIdxCurrent].X, dfLow1, null, bLow, data[nIdxCurrent].Index));
                    nIdx++;
                }
                else
                {
                    double dfLow = (data[i].Y_values.Length == 1) ? data[i].Y : data[i].Y_values[nLow];

                    dataLow.Add(new Plot(data[i].X, dfLow, null, false, data[i].Index));
                }
            }

            dataLow.SetMinMax(minmax);

            return dataLow;
        }
    }
}
