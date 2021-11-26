using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
{
    class GraphDataBB : IGraphPlotData
    {
        int m_nStdDev = 2;
        ConfigurationPlot m_config;
        CalculationArray m_caVal;

        public GraphDataBB(ConfigurationPlot config)
        {
            m_config = config;
        }

        public string Name
        {
            get { return "BollingerBands"; }
        }

        public string RenderType
        {
            get { return "BollingerBands"; }
        }

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        public BbData Pre(PlotCollectionSet dataset, int nDataIdx)
        {
            m_caVal = new CalculationArray((int)m_config.Interval);
            PlotCollection dataSrc = dataset[nDataIdx];
            PlotCollection dataDst = new PlotCollection(dataSrc.Name + " BB" + m_config.Interval.ToString());
            return new BbData(dataSrc, dataDst, m_config.Interval);
        }

        /// <summary>
        /// Calculate the BB based on Investopedia's formula.
        /// </summary>
        /// <remarks>
        /// @see [Bollinger Band Definition](https://www.investopedia.com/terms/b/bollingerbands.asp)
        /// </remarks>
        /// <param name="data">Specifies the BB data from the previous cycle.</param>
        /// <param name="i">Specifies the current data index.</param>
        /// <param name="minmax">Currently, not used here.</param>
        /// <param name="nLookahead">Specifies the look ahead value if any.</param>
        /// <param name="bAddToParams">Optionally, specifies whether or not to add the BB to the parameters of the original data.</param>
        /// <returns>The new BB values are returned.</returns>
        public Tuple<double, double, double> Process(BbData data, int i, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false)
        {
            bool bActive = data.SrcData[i].Active;

            Plot plot = new Plot(data.SrcData[i].X, new float[] { 0, 0, 0 }, null, false, data.SrcData[i].Index, data.SrcData[i].Action1Active, data.SrcData[i].Action2Active);
            data.DstData.Add(plot, false);
            
            float fTypicalValue = (data.SrcData[i].Y_values.Length == 4) ? (data.SrcData[i].Y_values[1] + data.SrcData[i].Y_values[2] + data.SrcData[i].Y_values[3]) / 3 : data.SrcData[i].Y;

            if (m_caVal.Add(fTypicalValue, null, false))
            {
                data.Ave = (float)m_caVal.Average;
                float fStdevTp = (float)m_caVal.StdDev;
                data.BbAbove = data.Ave + (m_nStdDev * fStdevTp);
                data.BbBelow = data.Ave - (m_nStdDev * fStdevTp);
                plot.SetYValues(new float[] { (float)data.BbBelow, (float)data.Ave, (float)data.BbAbove }, true);

                if (bAddToParams && bActive)
                {
                    data.SrcData[i].SetParameter(data.DstData.Name + " Below", data.BbBelow);
                    data.SrcData[i].SetParameter(data.DstData.Name + " Ave", data.Ave);
                    data.SrcData[i].SetParameter(data.DstData.Name + " Above", data.BbAbove);
                }

                if (minmax != null)
                {
                    minmax.Add(data.BbBelow);
                    minmax.Add(data.Ave);
                    minmax.Add(data.BbAbove);
                }
            }

            data.Count++;

            return new Tuple<double, double, double>(data.BbBelow, data.Ave, data.BbAbove);
        }

        public BbData GetBbData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false)
        {
            BbData data = Pre(dataset, nDataIdx);
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
            BbData data = GetBbData(dataset, nDataIdx, nLookahead, bAddToParams);
            return new PlotCollectionSet(new List<PlotCollection>() { data.DstData });
        }
    }

    public class BbData
    {
        PlotCollection m_src;
        PlotCollection m_dst;
        int m_nCount;
        int m_nInterval;
        double m_dfAve;
        double m_dfBbAbove;
        double m_dfBbBelow;

        public BbData(PlotCollection src, PlotCollection dst, uint nInterval)
        {
            m_src = src;
            m_dst = dst;
            m_nCount = 0;
            m_nInterval = (int)nInterval;
            m_dfAve = 0;
            m_dfBbAbove = 0;
            m_dfBbBelow = 0;
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

        public double BbAbove
        {
            get { return m_dfBbAbove; }
            set { m_dfBbAbove = value; }
        }

        public double BbBelow
        {
            get { return m_dfBbBelow; }
            set { m_dfBbBelow = value; }
        }

        public double Ave
        {
            get { return m_dfAve; }
            set { m_dfAve = value; }
        }

        public int Interval
        {
            get { return m_nInterval; }
        }
    }
}
