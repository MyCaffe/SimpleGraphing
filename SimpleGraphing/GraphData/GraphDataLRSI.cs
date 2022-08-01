using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphData
{
    public class GraphDataLRSI : IGraphPlotData 
    {
        double m_dfGamma = 0.5;
        ConfigurationPlot m_config;
        List<float[]> m_rgAl;

        public GraphDataLRSI(ConfigurationPlot config)
        {
            m_dfGamma = config.GetExtraSetting("Gamma", 0.5);
            m_config = config;
        }

        public string Name
        {
            get { return "LRSI"; }
        }

        public string RenderType
        {
            get { return "LINE"; }
        }

        public string RequiredDataName
        {
            get { return m_config.DataName; }
        }

        public RsiData Pre(PlotCollectionSet dataset, int nDataIdx, PlotCollection dataDst = null)
        {
            PlotCollection dataSrc = dataset[nDataIdx];
            
            if (dataDst == null)
                dataDst = new PlotCollection(dataSrc.Name + " LRSI" + m_config.Interval.ToString());

            m_rgAl = new List<float[]>();
            m_rgAl.Add(new float[4]);
            m_rgAl.Add(new float[4]);

            return new RsiData(dataSrc, dataDst, m_config.Interval);
        }

        /// <summary>
        /// Calculate the Laguerre RSI based.
        /// </summary>
        /// <remarks>
        /// @see [Cybernetic Analysis for Stocks and Futures](https://www.amazon.com/Cybernetic-Analysis-Stocks-Futures-Cutting-Edge/dp/0471463078) page 222-224, by John F. Ehlers, Whiley Trading, 2004
        /// </remarks>
        /// <param name="data">Specifies the RSI data from the previous cycle.</param>
        /// <param name="i">Specifies the current data index.</param>
        /// <param name="bActive">Output whether or not the data created is active.</param>
        /// <param name="minmax">Currently, not used here.</param>
        /// <param name="nLookahead">Specifies the look ahead value if any.</param>
        /// <param name="bAddToParams">Optionally, specifies whether or not to add the RSI to the parameters of the original data.</param>
        /// <returns>The new RSI value is returned.</returns>
        public double Process(RsiData data, int i, out bool bActive, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false, bool bIgnoreDst = false)
        {
            bActive = false;

            if (!bIgnoreDst)
            {
                Plot plot = new Plot(data.SrcData[i].X, 0, null, false, data.SrcData[i].Index, data.SrcData[i].Action1Active, data.SrcData[i].Action2Active);
                plot.Tag = data.SrcData[i].Tag;
                data.DstData.Add(plot, false);
            }

            m_rgAl.Insert(0, new float[4]);
            m_rgAl.RemoveAt(m_rgAl.Count - 1);

            m_rgAl[0][0] = (float)((1.0 - m_dfGamma) * data.SrcData[i].Y + m_dfGamma * m_rgAl[1][0]);
            m_rgAl[0][1] = (float)(-m_dfGamma * m_rgAl[0][0] + m_rgAl[1][0] + m_dfGamma * m_rgAl[1][1]);
            m_rgAl[0][2] = (float)(-m_dfGamma * m_rgAl[0][1] + m_rgAl[1][1] + m_dfGamma * m_rgAl[1][2]);
            m_rgAl[0][3] = (float)(-m_dfGamma * m_rgAl[0][2] + m_rgAl[1][2] + m_dfGamma * m_rgAl[1][3]);

            float fCU = 0;
            float fCD = 0;

            if (m_rgAl[0][0] >= m_rgAl[0][1])
                fCU = m_rgAl[0][0] - m_rgAl[0][1];
            else
                fCD = m_rgAl[0][1] - m_rgAl[0][0];

            if (m_rgAl[0][1] >= m_rgAl[0][2])
                fCU += m_rgAl[0][1] - m_rgAl[0][2];
            else
                fCD += m_rgAl[0][2] - m_rgAl[0][1];

            if (m_rgAl[0][2] >= m_rgAl[0][3])
                fCU += m_rgAl[0][2] - m_rgAl[0][3];
            else
                fCD += m_rgAl[0][3] - m_rgAl[0][2];

            float fRSI = 0;
            if (fCU + fCD != 0)
                fRSI = fCU / (fCU + fCD);

            data.RSI = fRSI * 100;
            bActive = true;

            if (!bIgnoreDst)
            {
                data.DstData[i].Y = (float)data.RSI;
                data.DstData[i].Active = bActive;
            }

            if (bAddToParams && bActive)
                data.SrcData[i].SetParameter(data.DstData.Name, (float)data.RSI);

            return data.RSI;
        }

        public RsiData GetRsiData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false)
        {
            RsiData data = Pre(dataset, nDataIdx);
            bool bActive;

            for (int i = 0; i < data.SrcData.Count; i++)
            {
                Process(data, i, out bActive, null, nLookahead, bAddToParams);
            }

            MinMax minmax = new MinMax();
            minmax.Add(0);
            minmax.Add(100);

            data.DstData.SetMinMax(minmax);

            return data;
        }

        public PlotCollectionSet GetData(PlotCollectionSet dataset, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false)
        {
            RsiData data = GetRsiData(dataset, nDataIdx, nLookahead, bAddToParams);
            return new PlotCollectionSet(new List<PlotCollection>() { data.DstData });
        }
    }
}
