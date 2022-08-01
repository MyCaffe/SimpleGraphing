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

        public LRsiData Pre(PlotCollectionSet dataset, int nDataIdx, PlotCollection dataDst = null)
        {
            PlotCollection dataSrc = dataset[nDataIdx];
            
            if (dataDst == null)
                dataDst = new PlotCollection(dataSrc.Name + " LRSI" + m_config.Interval.ToString());

            m_rgAl = new List<float[]>();
            m_rgAl.Add(new float[4]);
            m_rgAl.Add(new float[4]);

            return new LRsiData(dataSrc, dataDst, m_config.Interval);
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
        public double Process(LRsiData data, int i, out bool bActive, MinMax minmax = null, int nLookahead = 0, bool bAddToParams = false, bool bIgnoreDst = false)
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

        public LRsiData GetRsiData(PlotCollectionSet dataset, int nDataIdx, int nLookahead = 0, bool bAddToParams = false)
        {
            LRsiData data = Pre(dataset, nDataIdx);
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
            LRsiData data = GetRsiData(dataset, nDataIdx, nLookahead, bAddToParams);
            return new PlotCollectionSet(new List<PlotCollection>() { data.DstData });
        }
    }

    public class LRsiData
    {
        PlotCollection m_src;
        PlotCollection m_dst;
        int m_nCount;
        int m_nInterval;
        double m_dfRsi;

        public LRsiData(PlotCollection src, PlotCollection dst, uint nInterval)
        {
            m_src = src;
            m_dst = dst;
            m_nCount = 0;
            m_nInterval = (int)nInterval;
            m_dfRsi = 0;
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(m_nCount);
                bw.Write(m_nInterval);
                bw.Write(m_dfRsi);

                byte[] rgb = m_src.Save();
                bw.Write(rgb.Length);
                bw.Write(rgb);

                rgb = m_dst.Save();
                bw.Write(rgb.Length);
                bw.Write(rgb);

                return ms.ToArray();
            }
        }

        public static LRsiData Load(byte[] rgb, LRsiData data = null)
        {
            using (MemoryStream ms = new MemoryStream(rgb))
            using (BinaryReader br = new BinaryReader(ms))
            {
                int nCount = br.ReadInt32();
                int nInterval = br.ReadInt32();
                double dfRsi = br.ReadDouble();
                double dfAveGain = br.ReadDouble();
                double dfAveLoss = br.ReadDouble();
                double dfRs = br.ReadDouble();

                if (data == null)
                {
                    int nLen = br.ReadInt32();
                    byte[] rgb2 = br.ReadBytes(nLen);
                    PlotCollection src = PlotCollection.Load(rgb2);

                    nLen = br.ReadInt32();
                    rgb2 = br.ReadBytes(nLen);
                    PlotCollection dst = PlotCollection.Load(rgb2);

                    data = new LRsiData(src, dst, (uint)nInterval);
                }

                data.m_nCount = nCount;
                data.m_dfRsi = dfRsi;

                return data;
            }
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

        public double RSI
        {
            get { return m_dfRsi; }
            set { m_dfRsi = value; }
        }

        public int Interval
        {
            get { return m_nInterval; }
        }
    }
}
