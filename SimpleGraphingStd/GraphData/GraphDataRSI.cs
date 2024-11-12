using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphingStd.GraphData
{
    public class GraphDataRSI : IGraphPlotData 
    {
        ConfigurationPlot m_config;

        public GraphDataRSI(ConfigurationPlot config)
        {
            m_config = config;
        }

        public string Name
        {
            get { return "RSI"; }
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
                dataDst = new PlotCollection(dataSrc.Name + " RSI" + m_config.Interval.ToString());

            return new RsiData(dataSrc, dataDst, m_config.Interval);
        }

        /// <summary>
        /// Calculate the RSI based on Wilder's formula.
        /// </summary>
        /// <remarks>
        /// @see [Relative Strength Index](https://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi)
        /// </remarks>
        /// <param name="data">Specifies the RSI data from the previous cycle.</param>
        /// <param name="i">Specifies the current data index.</param>
        /// <param name="bActive">Output whether or not the data created is active.</param>
        /// <param name="minmax">Currently, not used here.</param>
        /// <param name="nLookahead">Specifies the look ahead value if any.</param>
        /// <param name="bAddToParams">Optionally, specifies whether or not to add the RSI to the parameters of the original data.</param>
        /// <param name="bIgnoreDst">Ignore the destination (default = false).</param>
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

            if (i > 0)
            {
                double dfChange = data.SrcData[i].Y - data.SrcData[i - 1].Y;

                if (i <= m_config.Interval + 1)
                {
                    if (dfChange < 0)
                        data.AveLoss += (dfChange * -1);
                    else
                        data.AveGain += dfChange;

                    if (i == m_config.Interval + 1)
                    {
                        data.AveLoss /= data.Interval;
                        data.AveGain /= data.Interval;
                        data.Rs = (data.AveLoss == 0) ? 0 : data.AveGain / data.AveLoss;
                        data.RSI = 100 - (100 / (1 + data.Rs));
                        bActive = true;
                    }
                }
                else if (i > data.Interval + 1)
                {
                    if (dfChange < 0)
                    {
                        data.AveLoss = ((data.AveLoss * (data.Interval - 1)) + (-1 * dfChange)) / data.Interval;
                        data.AveGain = ((data.AveGain * (data.Interval - 1)) + (0)) / data.Interval;
                    }
                    else
                    {
                        data.AveLoss = ((data.AveLoss * (data.Interval - 1)) + (0)) / data.Interval;
                        data.AveGain = ((data.AveGain * (data.Interval - 1)) + (dfChange)) / data.Interval;
                    }

                    data.Rs = (data.AveLoss == 0) ? 0 : data.AveGain / data.AveLoss;
                    data.RSI = 100 - (100 / (1 + data.Rs));
                    bActive = true;
                }
            }

            if (!bIgnoreDst)
            {
                data.DstData[i].Y = (float)data.RSI;
                data.DstData[i].Active = bActive;
            }

            if (bAddToParams && bActive)
            {
                string strName = data.DstData.Name.Trim();
                if (!string.IsNullOrEmpty(m_config.Name))
                    strName = m_config.Name;

                data.SrcData[i].SetParameter(strName, (float)data.RSI);
            }

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

    public class RsiData
    {
        PlotCollection m_src;
        PlotCollection m_dst;
        int m_nCount;
        int m_nInterval;
        double m_dfRsi;
        double m_dfAveGain = 0;
        double m_dfAveLoss = 0;
        double m_dfRs = 0;

        public RsiData(PlotCollection src, PlotCollection dst, uint nInterval)
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
                bw.Write(m_dfAveGain);
                bw.Write(m_dfAveLoss);
                bw.Write(m_dfRs);

                byte[] rgb = m_src.Save();
                bw.Write(rgb.Length);
                bw.Write(rgb);

                rgb = m_dst.Save();
                bw.Write(rgb.Length);
                bw.Write(rgb);

                return ms.ToArray();
            }
        }

        public static RsiData Load(byte[] rgb, RsiData data = null)
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

                    data = new RsiData(src, dst, (uint)nInterval);
                }

                data.m_nCount = nCount;
                data.m_dfAveGain = dfAveGain;
                data.m_dfAveLoss = dfAveLoss;
                data.m_dfRs = dfRs;
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

        public double AveGain
        {
            get { return m_dfAveGain; }
            set { m_dfAveGain = value; }
        }

        public double AveLoss
        {
            get { return m_dfAveLoss; }
            set { m_dfAveLoss = value; }
        }

        public double Rs
        {
            get { return m_dfRs; }
            set { m_dfRs = value; }
        }

        public int Interval
        {
            get { return m_nInterval; }
        }
    }
}
