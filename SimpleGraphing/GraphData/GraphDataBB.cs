using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
{
    public class GraphDataBB : IGraphPlotData
    {
        double m_dfStdDev = 2;
        ConfigurationPlot m_config;
        CalculationArray m_caVal;
        CalculationArray m_caValExt;
        TARGET m_target = TARGET.DEFAULT;

        enum TARGET
        {
            DEFAULT,
            BAR,
            RANGE
        }

        public GraphDataBB(ConfigurationPlot config)
        {
            m_config = config;
            m_dfStdDev = config.GetExtraSetting("StdDev", 2.0);
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
            return Pre(dataset[nDataIdx]);
        }

        public BbData Pre(PlotCollection dataset)
        {
            m_caVal = new CalculationArray((int)m_config.Interval);
            m_caValExt = new CalculationArray((int)m_config.Interval);
            PlotCollection dataSrc = dataset;
            PlotCollection dataDst = new PlotCollection(dataSrc.Name + " BB" + m_config.Interval.ToString());

            if (m_config.GetExtraSetting("BbTarget:BarRange", 0) == 1)
                m_target = TARGET.BAR;
            else if (m_config.GetExtraSetting("BbTarget:TotalRange", 0) == 1)
                m_target = TARGET.RANGE;

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
        public Tuple<long, double, double, double, double, double> Process(BbData data, int i, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false)
        {
            bool bActive = data.SrcData[i].Active;

            Plot plot = new Plot(data.SrcData[i].X, new float[] { 0, 0, 0 }, null, false, data.SrcData[i].Index, data.SrcData[i].Action1Active, data.SrcData[i].Action2Active);
            plot.Tag = data.SrcData[i].Tag;
            data.DstData.Add(plot, false);
            
            float fTypicalValue = (data.SrcData[i].Y_values.Length == 4) ? (data.SrcData[i].Y_values[1] + data.SrcData[i].Y_values[2] + data.SrcData[i].Y_values[3]) / 3 : data.SrcData[i].Y;

            if (data.SrcData[i].Y_values.Length == 4 && m_target != TARGET.DEFAULT)
            {
                if (m_target == TARGET.BAR)
                {
                    float fVal = (data.SrcData[i].Y_values[0] - data.SrcData[i].Y);
                    m_caValExt.Add(fVal, null, true);
                }
                else if (m_target == TARGET.RANGE)
                {
                    float fVal = (data.SrcData[i].Y_values[1] - data.SrcData[i].Y_values[2]);
                    m_caValExt.Add(fVal, null, true);
                }
            }

            if (m_caVal.Add(fTypicalValue, null, false))
            {
                data.Ave = (float)m_caVal.Average;
                float fStdevTp = (float)m_caVal.StdDev;

                if (m_target != TARGET.DEFAULT)
                    fStdevTp = (float)m_caValExt.StdDev;

                double dfStdDvTp = (m_dfStdDev * fStdevTp);
                data.BbAbove = data.Ave + dfStdDvTp;
                data.BbBelow = data.Ave - dfStdDvTp;
                plot.SetYValues(new float[] { (float)data.BbBelow, (float)data.Ave, (float)data.BbAbove }, true);

                double dfAboveBelow = data.BbAbove - data.BbBelow;
                data.BbPctb = 0;
                if (dfAboveBelow != 0)
                    data.BbPctb = (data.SrcData[i].Y - data.BbBelow) / dfAboveBelow;

                data.BbWid = 0;
                if (data.Ave != 0)
                    data.BbWid = dfAboveBelow / data.Ave;

                if (bAddToParams && bActive)
                {
                    data.SrcData[i].SetParameter(data.DstData.Name + " Below", data.BbBelow);
                    data.SrcData[i].SetParameter(data.DstData.Name + " Ave", data.Ave);
                    data.SrcData[i].SetParameter(data.DstData.Name + " Above", data.BbAbove);
                    data.SrcData[i].SetParameter(data.DstData.Name + " %b", data.BbPctb);
                    data.SrcData[i].SetParameter(data.DstData.Name + " BandWidth", data.BbWid);
                }

                if (minmax != null)
                {
                    minmax.Add(data.BbBelow);
                    minmax.Add(data.Ave);
                    minmax.Add(data.BbAbove);
                }
            }

            data.Count++;

            return new Tuple<long, double, double, double, double, double>((long)data.SrcData[i].X, data.BbBelow, data.Ave, data.BbAbove, data.BbPctb, data.BbWid);
        }

        public BbData GetBbData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false, int nStartIdx = 0)
        {
            BbData data = Pre(dataset, nDataIdx);
            MinMax minmax = new MinMax();

            for (int i = nStartIdx; i < data.SrcData.Count; i++)
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
        double m_dfPctB;
        double m_dfBbWid;

        public BbData(PlotCollection src, PlotCollection dst, uint nInterval)
        {
            m_src = src;
            m_dst = dst;
            m_nCount = 0;
            m_nInterval = (int)nInterval;
            m_dfAve = 0;
            m_dfBbAbove = 0;
            m_dfBbBelow = 0;
            m_dfPctB = 0;
            m_dfBbWid = 0;
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

        public double BbPctb
        {
            get { return m_dfPctB; }
            set { m_dfPctB = value; }
        }

        public double BbWid
        {
            get { return m_dfBbWid; }
            set { m_dfBbWid = value; }
        }

        public int Interval
        {
            get { return m_nInterval; }
        }
    }
}
