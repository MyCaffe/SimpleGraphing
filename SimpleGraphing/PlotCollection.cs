using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    [Serializable]
    public class PlotCollection //: IEnumerable<Plot>
    {
        string m_strSrcName = "";
        string m_strName;
        List<Plot> m_rgPlot = null;
        double m_dfXIncrement = 1.0;
        double m_dfXPosition = 0;
        int m_nMax;
        double m_dfMinVal = double.MaxValue;
        double m_dfMaxVal = -double.MaxValue;
        object m_tag1 = null;
        object m_tag2 = null;
        double? m_dfCalculatedEndY = null;
        bool m_bExcludeFromMinMax = false;
        MINMAX_TARGET m_minmaxTarget = MINMAX_TARGET.VALUES;
        bool m_bLockMinMax = false;
        Dictionary<string, double> m_rgParam = new Dictionary<string, double>();
        Dictionary<string, object> m_rgParamEx = new Dictionary<string, object>();
        DateTime m_dtLastUpdate = DateTime.MinValue;
        object m_syncObj = new object();
        int? m_nStartOffsetFromEnd = null;

        public event EventHandler<PlotUpdateArgs> OnUpdatePlot;

        public enum MINMAX_TARGET
        {
            VALUES,
            COUNT,
            PARAMS
        }

        public PlotCollection()
        {
            m_rgPlot = new List<Plot>();
            m_nMax = int.MaxValue;
            m_dfXIncrement = 1.0;
            m_strName = "";
        }

        public PlotCollection(string strName, int nMax = int.MaxValue, double dfXInc = 1.0)
        {
            m_nMax = nMax;
            m_dfXIncrement = dfXInc;
            m_strName = strName;
            
            int nCapacity = Math.Min(nMax, 2500);
            m_rgPlot = new List<Plot>(nCapacity);
        }

        public int StartIndex
        {
            get
            {
                int nIdx = 0;

                if (m_nStartOffsetFromEnd.HasValue)
                {
                    nIdx = m_rgPlot.Count - m_nStartOffsetFromEnd.Value;
                    if (nIdx < 0)
                        nIdx = 0;
                }

                return nIdx;
            }
        }

        public int GetStartIndex(int nStartIdx)
        {
            int nStartIdx1 = StartIndex;
            if (nStartIdx < nStartIdx1)
                return nStartIdx1;
            else
                return nStartIdx;
        }

        public Plot First()
        {
            int nStartIdx = StartIndex;
            if (nStartIdx >= m_rgPlot.Count)
                return null;

            return m_rgPlot[nStartIdx];
        }

        public long SumItemCount()
        {
            long lCount = 0;

            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Active)
                    lCount += m_rgPlot[i].Count.GetValueOrDefault();
            }

            return lCount;
        }

        public MinMax GetMinMaxY()
        {
            MinMax minmax = new MinMax();

            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Active)
                    minmax.Add(m_rgPlot[i].Y);
            }

            return minmax;
        }

        public int? StartOffsetFromEnd
        {
            get { return m_nStartOffsetFromEnd; }
            set 
            {
                if (value < m_rgPlot.Count)
                    m_nStartOffsetFromEnd = value;
                else
                    m_nStartOffsetFromEnd = null;
            }
        }

        public DateTime LastUpdateTime
        {
            get { return m_dtLastUpdate; }
            set { m_dtLastUpdate = value; }
        }

        public Dictionary<string, double> Parameters
        {
            get { return m_rgParam; }
        }

        public Dictionary<string, object> ParametersEx
        {
            get { return m_rgParamEx; }
        }

        public double? GetParameter(string strName)
        {
            if (m_rgParam.ContainsKey(strName))
                return m_rgParam[strName];
            else
                return null;
        }

        public void SetParameter(string strName, double dfVal)
        {
            if (m_rgParam.ContainsKey(strName))
                m_rgParam[strName] = dfVal;
            else
                m_rgParam.Add(strName, dfVal);
        }

        public Plot Last()
        {
            if (m_rgPlot.Count == 0)
                return null;

            return m_rgPlot[m_rgPlot.Count - 1];
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(m_strName);
                bw.Write(m_strSrcName);
                bw.Write(m_dfXIncrement);
                bw.Write(m_dfXPosition);
                bw.Write(m_nMax);
                bw.Write(m_dfMinVal);
                bw.Write(m_dfMaxVal);

                bw.Write((m_tag1 != null) ? true : false);
                if (m_tag1 != null)
                    bw.Write(m_tag1.ToString());

                bw.Write((m_tag2 != null) ? true : false);
                if (m_tag2 != null)
                    bw.Write(m_tag2.ToString());

                bw.Write((m_dfCalculatedEndY.HasValue) ? true : false);
                if (m_dfCalculatedEndY.HasValue)
                    bw.Write(m_dfCalculatedEndY.Value);

                bw.Write(m_bExcludeFromMinMax);
                bw.Write((int)m_minmaxTarget);

                bw.Write(m_rgPlot.Count);

                for (int i = 0; i < m_rgPlot.Count; i++)
                {
                    m_rgPlot[i].Save(bw);
                }

                bw.Write(m_rgParam.Count);

                foreach (KeyValuePair<string, double> kv in m_rgParam)
                {
                    bw.Write(kv.Key);
                    bw.Write(kv.Value);
                }

                bw.Flush();
                return ms.ToArray();
            }
        }

        public static PlotCollection Load(byte[] rg, PlotCollection col = null)
        {
            using (MemoryStream ms = new MemoryStream(rg))
            using (BinaryReader br = new BinaryReader(ms))
            {
                string strName = br.ReadString();
                string strSrc = br.ReadString();
                double dfXInc = br.ReadDouble();
                double dfXPos = br.ReadDouble();
                int nMax = br.ReadInt32();
                double dfMin = br.ReadDouble();
                double dfMax = br.ReadDouble();

                object tag1 = null;
                if (br.ReadBoolean())
                    tag1 = br.ReadString();

                object tag2 = null;
                if (br.ReadBoolean())
                    tag2 = br.ReadString();

                double? dfCalculatedEndY = null;
                if (br.ReadBoolean())
                    dfCalculatedEndY = br.ReadDouble();

                bool bExcludeMinMax = br.ReadBoolean();
                MINMAX_TARGET minmaxTarget = (MINMAX_TARGET)br.ReadInt32();

                if (col == null)
                {
                    col = new PlotCollection(strName, nMax, dfXInc);
                }
                else
                {
                    col.m_rgPlot.Clear();
                    col.m_rgParam.Clear();
                }

                col.SourceName = strSrc;
                col.m_dfXPosition = dfXPos;
                col.m_dfMinVal = dfMin;
                col.m_dfMaxVal = dfMax;
                col.Tag = tag1;
                col.Tag2 = tag2;
                col.CalculatedEndY = dfCalculatedEndY;

                int nCount = br.ReadInt32();
                for (int i = 0; i < nCount; i++)
                {
                    col.Add(Plot.Load(br), false, false);
                }

                nCount = br.ReadInt32();
                for (int i = 0; i < nCount; i++)
                {
                    string strKey = br.ReadString();
                    double dfVal = br.ReadDouble();
                    col.Parameters.Add(strKey, dfVal);
                }

                return col;
            }
        }

        public PlotCollection RemoveInactive()
        {
            PlotCollection col = new PlotCollection(m_strName, m_nMax, m_dfXIncrement);

            for (int i=StartIndex; i<m_rgPlot.Count; i++)
            {
                Plot p = m_rgPlot[i];
                if (p.Active)
                    col.Add(p);
            }

            return col;
        }
        
        public Plot GetFirstActive()
        {
            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Active)
                    return m_rgPlot[i];
            }

            return null;
        }

        public void ScaleParametersToCount(bool bCalculateMinMax, bool bOnlyUnscaled, double? dfParamMin, double? dfParamMax, double dfSqueezePct, string strScaledKeyPostfix, params string[] rgstrParams)
        {
            double dfMin = m_dfMinVal;
            double dfMax = m_dfMaxVal;

            if (bCalculateMinMax)
            {
                dfMin = 0;
                dfMax = m_rgPlot.Max(p => p.Count.GetValueOrDefault());
            }

            double dfRange = dfMax - dfMin;

            if (dfParamMin == null)
                dfParamMin = m_rgPlot.Min(p => p.Parameters.Min(p1 => p1.Value));

            if (dfParamMax == null)
                dfParamMax = m_rgPlot.Max(p => p.Parameters.Max(p1 => p1.Value));

            double dfParamRange = (dfParamMax.Value - dfParamMin.Value);

            if (dfSqueezePct > 0)
                dfParamRange *= (1.0 + dfSqueezePct);
            
            for (int i=StartIndex; i<m_rgPlot.Count; i++)
            {
                Plot plot = m_rgPlot[i];
                if (plot.Active && !bOnlyUnscaled || !plot.Scaled)
                {
                    for (int j = 0; j < rgstrParams.Length; j++)
                    {
                        string strKey = rgstrParams[j];
                        if (plot.Parameters != null && plot.Parameters.ContainsKey(strKey))
                        {
                            double dfVal = plot.Parameters[strKey];
                            if (dfRange == 0 || dfParamRange == 0)
                                dfVal = 0;
                            else
                                dfVal = (((dfVal - dfParamMin.Value) / dfParamRange) * dfRange) + dfMin;

                            strKey += strScaledKeyPostfix;

                            if (!plot.Parameters.ContainsKey(strKey))
                                plot.Parameters.Add(strKey, (float)dfVal);
                            else
                                plot.Parameters[strKey] = (float)dfVal;

                            plot.Scaled = true;
                        }
                    }
                }
            }
        }

        public int ClipUpToDate(DateTime dt)
        {
            int nClipCount = 0;
            
            while (m_rgPlot.Count > 0)
            {
                if (!(m_rgPlot[0].Tag is DateTime))
                    return 0;

                if ((DateTime)m_rgPlot[0].Tag < dt)
                {
                    nClipCount++;                    
                    m_rgPlot.RemoveAt(0);
                }
                else
                    break;
            }

            return nClipCount;
        }

        public PlotCollection ClipToFirstActive(int nMinConsecutiveCount)
        {            
            int nIdx = 0;
            int nConsecutiveCount = 0;          

            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Active)
                {
                    if (nConsecutiveCount == 0)
                        nIdx = i;

                    nConsecutiveCount++;

                    if (nConsecutiveCount == nMinConsecutiveCount)
                        break;
                }
                else
                {
                    nConsecutiveCount = 0;
                }
            }

            if (nIdx == 0)
                return this;

            return Clone(nIdx);
        }

        public void ClipEndInactive()
        {
            while (m_rgPlot.Count > 0 && !m_rgPlot[m_rgPlot.Count - 1].Active)
            {
                m_rgPlot.RemoveAt(m_rgPlot.Count - 1);
            }
        }
        
        public PlotCollection FillInactive(int nConsecutiveInactiveMax)
        {
            int nLastActiveIdx = -1;

            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Active)
                    nLastActiveIdx = i;
                else if (nLastActiveIdx >= 0)
                {
                    int nEndActiveIdx = nLastActiveIdx + nConsecutiveInactiveMax;
                    if (nEndActiveIdx >= m_rgPlot.Count)
                        nEndActiveIdx = m_rgPlot.Count - 1;

                    for (int j = nLastActiveIdx + 1; j <= nEndActiveIdx; j++)
                    {
                        if (m_rgPlot[j].Active)
                            break;

                        m_rgPlot[j].Active = true;

                        for (int k = 0; k < m_rgPlot[j].Y_values.Length; k++)
                        {
                            m_rgPlot[j].Y_values[k] = m_rgPlot[j - 1].Y;
                        }

                        m_rgPlot[j].Count = 0;

                        i++;
                    }
                }
            }

            return this;
        }

        public PlotCollection CloneNew(string strName, int nValCount, int nStartIdx = 0, bool? bActive = null, bool bCopyData = false)
        {
            PlotCollection col = new PlotCollection(strName, m_nMax, m_dfXIncrement);

            col.m_strSrcName = m_strSrcName;
            col.m_dfXPosition = m_dfXPosition;
            col.m_dfMinVal = m_dfMinVal;
            col.m_dfMaxVal = m_dfMaxVal;
            col.m_tag1 = m_tag1;
            col.m_tag2 = m_tag2;
            col.m_dfCalculatedEndY = m_dfCalculatedEndY;
            col.m_bExcludeFromMinMax = m_bExcludeFromMinMax;
            col.m_minmaxTarget = m_minmaxTarget;
            col.m_bLockMinMax = m_bLockMinMax;
            col.m_dtLastUpdate = m_dtLastUpdate;

            nStartIdx = GetStartIndex(nStartIdx);

            lock (m_syncObj)
            {
                for (int i = nStartIdx; i < m_rgPlot.Count; i++)
                {
                    Plot p = m_rgPlot[i].Clone(bCopyData);

                    if (bActive.HasValue)
                        p.Active = bActive.Value;

                    if (nValCount < 0)
                        nValCount = p.Y_values.Length;

                    p.SetYValues(new float[nValCount]);
                    col.Add(p, false);
                }

                for (int i = 0; i < Parameters.Count; i++)
                {
                    KeyValuePair<string, double> kv = Parameters.ElementAt(i);
                    col.Parameters.Add(kv.Key, kv.Value);
                }
            }

            return col;
        }

        public PlotCollection Clone(int nStartIdx = 0, bool bCalculateMinMax = true, int? nPrimaryIndexY = null, bool? bActive = null, bool bSetDateOnTag = false, int? nCount = null, bool bCopyData = false)
        {
            PlotCollection col = new PlotCollection(m_strName, m_nMax, m_dfXIncrement);

            col.m_strSrcName = m_strSrcName;
            col.m_dfXPosition = m_dfXPosition;
            col.m_dfMinVal = m_dfMinVal;
            col.m_dfMaxVal = m_dfMaxVal;
            col.m_tag1 = m_tag1;
            col.m_tag2 = m_tag2;
            col.m_dfCalculatedEndY = m_dfCalculatedEndY;
            col.m_bExcludeFromMinMax = m_bExcludeFromMinMax;
            col.m_minmaxTarget = m_minmaxTarget;
            col.m_bLockMinMax = m_bLockMinMax;
            col.m_dtLastUpdate = m_dtLastUpdate;
            col.m_nStartOffsetFromEnd = m_nStartOffsetFromEnd;

            lock (m_syncObj)
            {
                if (!nCount.HasValue || nStartIdx + nCount.Value > m_rgPlot.Count)
                    nCount = m_rgPlot.Count - nStartIdx;

                for (int i = nStartIdx; i < nStartIdx + nCount.Value && i < m_rgPlot.Count; i++)
                {
                    Plot p = m_rgPlot[i].Clone(bCopyData);
                    if (nPrimaryIndexY.HasValue)
                        p.PrimaryIndexY = nPrimaryIndexY.Value;

                    if (bActive.HasValue)
                        p.Active = bActive.Value;

                    if (bSetDateOnTag && p.Tag == null)
                        p.Tag = DateTime.FromFileTimeUtc((long)p.X);

                    col.Add(p, false);
                }
                
                for (int i=0; i<Parameters.Count; i++)
                {
                    KeyValuePair<string, double> kv = Parameters.ElementAt(i);
                    col.Parameters.Add(kv.Key, kv.Value);
                }
            }

            if (bCalculateMinMax)
                col.SetMinMax();

            return col;
        }

        public PlotCollection CloneFromEnd(int nCount, bool bResetStartIndex = true)
        {
            PlotCollection plots = null;

            if (m_rgPlot.Count < nCount)
                plots = Clone(0, false);
            else
                plots = Clone(m_rgPlot.Count - nCount, false);

            if (bResetStartIndex)
                plots.StartOffsetFromEnd = null;

            return plots;
        }

        public PlotCollection SimpleCloneEnd(int nCount)
        {
            if (m_rgPlot.Count < nCount)
                return SimpleClone(false);

            PlotCollection plots = SimpleClone(false);
            plots.StartOffsetFromEnd = nCount;

            return plots;
        }

        public PlotCollection SimpleClone(bool bCalculateMinMax, MINMAX_TARGET? minmax = null)
        {
            PlotCollection col = new PlotCollection(m_strName, m_nMax, m_dfXIncrement);

            if (!minmax.HasValue)
                minmax = m_minmaxTarget;

            col.m_strSrcName = m_strSrcName;
            col.m_dfXPosition = m_dfXPosition;
            col.m_dfMinVal = m_dfMinVal;
            col.m_dfMaxVal = m_dfMaxVal;
            col.m_tag1 = m_tag1;
            col.m_tag2 = m_tag2;
            col.m_dfCalculatedEndY = m_dfCalculatedEndY;
            col.m_bExcludeFromMinMax = m_bExcludeFromMinMax;
            col.m_minmaxTarget = minmax.Value;
            col.m_bLockMinMax = m_bLockMinMax;
            col.m_dtLastUpdate = m_dtLastUpdate;
            col.m_nStartOffsetFromEnd = m_nStartOffsetFromEnd;

            lock (m_syncObj)
            {
                col.m_rgPlot = m_rgPlot;
            }

            if (bCalculateMinMax)
                col.SetMinMax(0, true);

            return col;
        }

        public void TransferParameters(PlotCollection col, string strParam)
        {
            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                double? dfVal = m_rgPlot[i].GetParameter(strParam);
                if (dfVal.HasValue)
                {
                    col[i].SetParameter(strParam, (float)dfVal.Value);
                    m_rgPlot[i].DeleteParameter(strParam);
                }
            }
        }

        public void GetParamMinMax(string strParam, out double dfMin, out double dfMax)
        {
            dfMin = double.MaxValue;
            dfMax = -double.MaxValue;

            if (string.IsNullOrEmpty(strParam))
                return;

            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Active)
                {
                    double? dfP = m_rgPlot[i].GetParameter(strParam);
                    if (dfP.HasValue)
                    {
                        if (!double.IsNaN(dfP.Value) && !double.IsInfinity(dfP.Value))
                        {
                            dfMin = Math.Min(dfMin, dfP.Value);
                            dfMax = Math.Max(dfMax, dfP.Value);
                        }
                    }
                }
            }
        }

        public Tuple<PlotCollection, PlotCollection> Split(int nCount, bool bSetDateOnTag = false, bool bAppendSplitCountToName = false)
        {
            string strName = m_strName + ((bAppendSplitCountToName) ? "_1" : "");
            PlotCollection p1 = new PlotCollection(strName, m_nMax, m_dfXIncrement);
            strName = m_strName + ((bAppendSplitCountToName) ? "_2" : "");
            PlotCollection p2 = new PlotCollection(strName, m_nMax, m_dfXIncrement);

            if (nCount < 0)
                nCount = m_rgPlot.Count + nCount;

            lock (m_syncObj)
            {
                for (int i = 0; i < nCount && i < m_rgPlot.Count; i++)
                {
                    Plot p = m_rgPlot[i];
                    if (bSetDateOnTag && p.Tag == null)
                        p.Tag = DateTime.FromFileTimeUtc((long)p.X);

                    p1.Add(p);
                }

                for (int i = nCount; i < m_rgPlot.Count; i++)
                {
                    Plot p = m_rgPlot[i];
                    if (bSetDateOnTag && p.Tag == null)
                        p.Tag = DateTime.FromFileTimeUtc((long)p.X);

                    p2.Add(p);
                }
            }

            p1.Tag = Tag;
            p1.Tag2 = Tag2;
            p2.Tag = Tag;
            p2.Tag2 = Tag2;

            return new Tuple<PlotCollection, PlotCollection>(p1, p2);
        }

        public MINMAX_TARGET MinMaxTarget
        {
            get { return m_minmaxTarget; }
            set { m_minmaxTarget = value; }
        }

        public void SetMinMax(int nStartIdx = 0, bool? bLock = null)
        {
            nStartIdx = GetStartIndex(nStartIdx);

            if (nStartIdx >= m_rgPlot.Count)
                return;

            if (!bLock.HasValue && m_bLockMinMax)
                return;

            if (m_rgPlot.Count <= 1)
                return;

            m_dfMinVal = double.MaxValue;
            m_dfMaxVal = -double.MaxValue;

            if (m_minmaxTarget == MINMAX_TARGET.PARAMS)
            {
                m_dfMinVal = 0;
                m_dfMaxVal = 1;
                return;
            }

            for (int i = nStartIdx; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Active)
                {
                    if (m_minmaxTarget == MINMAX_TARGET.VALUES)
                    {
                        m_dfMinVal = Math.Min(m_dfMinVal, m_rgPlot[i].Y_values.Min());
                        m_dfMaxVal = Math.Max(m_dfMaxVal, m_rgPlot[i].Y_values.Max());
                    }
                    else
                    {
                        long nCount = m_rgPlot[i].Count.GetValueOrDefault();
                        m_dfMinVal = Math.Min(m_dfMinVal, nCount);
                        m_dfMaxVal = Math.Max(m_dfMaxVal, nCount);
                    }
                }
            }

            if (bLock.HasValue)
                m_bLockMinMax = bLock.Value;
        }

        public void SetMinMax(MinMax minmax)
        {
            if (m_bLockMinMax)
                return;

            m_dfMinVal = minmax.Min;
            m_dfMaxVal = minmax.Max;
        }

        public void SetMinMaxForce(MinMax minmax)
        {
            m_dfMinVal = minmax.Min;
            m_dfMaxVal = minmax.Max;
        }

        public void SetMinMaxForce(double dfMin, double dfMax)
        {
            m_dfMinVal = dfMin;
            m_dfMaxVal = dfMax;
        }

        public bool ExcludeFromMinMax
        {
            get { return m_bExcludeFromMinMax; }
            set { m_bExcludeFromMinMax = value; }
        }

        public int MaximumCount
        {
            get { return m_nMax; }
        }

        public double? CalculatedEndY
        {
            get { return m_dfCalculatedEndY; }
            set { m_dfCalculatedEndY = value; }
        }

        public void ShiftLeft(int nCount)
        {
            if (nCount > m_rgPlot.Count)
                nCount = m_rgPlot.Count;

            for (int i = 0; i < nCount; i++)
            {
                m_rgPlot.RemoveAt(0);
            }

            ReIndex();
        }

        public void ReIndex()
        {
            for (int i = 0; i < m_rgPlot.Count; i++)
            {
                m_rgPlot[i].Index = i;
            }
        }

        public double? GetSlope(out double dfEndY, out int nIdxStart, out int nIdxEnd)
        {
            int nStartIdx = StartIndex;

            nIdxStart = m_rgPlot[nStartIdx].Index;
            nIdxEnd = m_rgPlot[m_rgPlot.Count-1].Index;
            dfEndY = 0;

            if (nIdxStart < 0 || nIdxEnd <= nIdxStart)
                return null;
            
            double dfY0 = m_rgPlot[nStartIdx].Y;
            double dfY1 = m_rgPlot[m_rgPlot.Count-1].Y;
            int nSteps = nIdxEnd - nIdxStart;

            dfEndY = dfY1;

            return (dfY1 - dfY0) / nSteps;
        }

        public int FirstActiveIndex
        {
            get
            {
                for (int i = StartIndex; i < m_rgPlot.Count; i++)
                {
                    if (m_rgPlot[i].Active)
                        return i;
                }

                return -1;
            }
        }

        public int LastActiveIndex
        {
            get
            {
                for (int i = m_rgPlot.Count - 1; i >= StartIndex; i--)
                {
                    if (m_rgPlot[i].Active)
                        return i;
                }

                return -1;
            }
        }

        public bool ComparePlots(PlotCollection plots)
        {
            if (m_rgPlot.Count != plots.Count)
                return false;

            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i].Compare(plots[i]))
                    return false;
            }

            return true;
        }

        public void GetMinMaxOverWindow(int nStartIdx, int nCount, out double dfMinX, out double dfMinY, out double dfMaxX, out double dfMaxY, int nValIdx = -1)
        {
            dfMinX = double.MaxValue;
            dfMaxX = -double.MaxValue;
            dfMinY = double.MaxValue;
            dfMaxY = -double.MaxValue;

            if (nCount <= 0)
                nCount = m_rgPlot.Count;

            for (int i = StartIndex; i < nCount; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < m_rgPlot.Count && m_rgPlot[nIdx].Active)
                {
                    double dfValX = m_rgPlot[nIdx].X;
                    dfMinX = Math.Min(dfMinX, dfValX);
                    dfMaxX = Math.Max(dfMaxX, dfValX);

                    double dfValY = 0;

                    if (m_minmaxTarget == MINMAX_TARGET.VALUES)
                    {
                        if (nValIdx < 0)
                            dfValY = m_rgPlot[nIdx].Y;
                        else
                            dfValY = m_rgPlot[nIdx].Y_values[nValIdx];
                    }
                    else if (m_minmaxTarget == MINMAX_TARGET.COUNT)
                    {
                        dfValY = m_rgPlot[nIdx].Count.GetValueOrDefault();
                    }

                    dfMinY = Math.Min(dfMinY, dfValY);
                    dfMaxY = Math.Max(dfMaxY, dfValY);
                }
            }

            if (m_minmaxTarget == MINMAX_TARGET.PARAMS)
            {
                dfMinY = 0.0;
                dfMaxY = 1.0;
            }
        }

        public object Tag
        {
            get { return m_tag1; }
            set { m_tag1 = value; }
        }

        public object Tag2
        {
            get { return m_tag2; }
            set { m_tag2 = value; }
        }

        public string SourceName
        {
            get { return m_strSrcName; }
            set { m_strSrcName = value; }
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public double IncrementX
        {
            get { return m_dfXIncrement; }
            set { m_dfXIncrement = value; }
        }

        public int FullCount
        {
            get { return m_rgPlot.Count; }
        }

        public int Count
        {
            get { return m_rgPlot.Count - StartIndex; }
        }

        public int ActiveCount
        {
            get
            {
                int nCount = 0;

                for (int i = StartIndex; i < m_rgPlot.Count; i++)
                {
                    if (m_rgPlot[i].Active)
                        nCount++;
                }

                return nCount;
            }
        }

        public int GetActiveCountFromEnd(int nEndIdx = -1)
        {
            int nCount = 0;

            if (nEndIdx == -1)
                nEndIdx = m_rgPlot.Count - 1;

            for (int i = nEndIdx; i >= StartIndex; i--)
            {
                if (m_rgPlot[i].Active)
                    nCount++;
                else
                    break;
            }

            return nCount;
        }

        public Plot this[int nIdx]
        {
            get 
            {
                return m_rgPlot[StartIndex + nIdx]; 
            }
            set 
            {
                m_rgPlot[StartIndex + nIdx] = value; 
            }
        }

        public double AbsoluteMinYVal
        {
            get { return m_dfMinVal; }
        }

        public double AbsoluteMaxYVal
        {
            get { return m_dfMaxVal; }
        }

        public float[] convert(List<double> rgdf)
        {
            float[] rg = new float[rgdf.Count];
            for (int i = 0; i < rgdf.Count; i++)
            {
                rg[i] = (float)rgdf[i];
            }

            return rg;
        }

        private void setMinMax(Plot last, float[] rgfY)
        {
            if (m_bLockMinMax)
                return;

            m_dfMinVal = double.MaxValue;
            m_dfMaxVal = -double.MaxValue;

            if (rgfY == null)
                return;

            double dfMin = (rgfY.Length == 1) ? rgfY[0] : rgfY.Min(p => p);
            double dfMax = (rgfY.Length == 1) ? rgfY[0] : rgfY.Max(p => p);

            if (last == null)
            {
                m_dfMinVal = Math.Min(m_dfMinVal, dfMin);
                m_dfMaxVal = Math.Max(m_dfMaxVal, dfMax);
                return;
            }

            if (m_minmaxTarget == MINMAX_TARGET.VALUES)
            {
                double dfMin0 = (last.Y_values.Length == 1) ? last.Y : last.Y_values.Min(p => p);
                double dfMax0 = (last.Y_values.Length == 1) ? last.Y : last.Y_values.Max(p => p);

                if (dfMin0 > m_dfMinVal && dfMax0 < m_dfMaxVal)
                {
                    m_dfMinVal = Math.Min(m_dfMinVal, dfMin);
                    m_dfMaxVal = Math.Max(m_dfMaxVal, dfMax);
                    return;
                }

                if (m_rgPlot.Count > 1)
                {
                    Plot last1 = m_rgPlot[0];
                    double dfMin1 = (last1.Y_values.Length == 1) ? last.Y : last1.Y_values.Min(p => p);
                    double dfMax1 = (last1.Y_values.Length == 1) ? last.Y : last1.Y_values.Max(p => p);

                    if (dfMin0 == dfMin1 && dfMax0 == dfMax1)
                    {
                        m_dfMinVal = Math.Min(m_dfMinVal, dfMin);
                        m_dfMaxVal = Math.Max(m_dfMaxVal, dfMax);
                        return;
                    }
                }

                for (int i = StartIndex; i < m_rgPlot.Count; i++)
                {
                    if (m_rgPlot[i].Active)
                    {
                        m_dfMinVal = Math.Min(m_dfMinVal, m_rgPlot[i].Y_values.Min(p => p));
                        m_dfMaxVal = Math.Max(m_dfMaxVal, m_rgPlot[i].Y_values.Max(p => p));
                    }
                }
            }
            else if (m_minmaxTarget == MINMAX_TARGET.COUNT)
            {
                double dfMin0 = last.Count.GetValueOrDefault();
                double dfMax0 = last.Count.GetValueOrDefault();

                if (dfMin0 > m_dfMinVal && dfMax0 < m_dfMaxVal)
                {
                    m_dfMinVal = Math.Min(m_dfMinVal, dfMin);
                    m_dfMaxVal = Math.Max(m_dfMaxVal, dfMax);
                    return;
                }

                if (m_rgPlot.Count > 1)
                {
                    Plot last1 = m_rgPlot[0];
                    double dfMin1 = last.Count.GetValueOrDefault();
                    double dfMax1 = last.Count.GetValueOrDefault();

                    if (dfMin0 == dfMin1 && dfMax0 == dfMax1)
                    {
                        m_dfMinVal = Math.Min(m_dfMinVal, dfMin);
                        m_dfMaxVal = Math.Max(m_dfMaxVal, dfMax);
                        return;
                    }
                }

                for (int i = StartIndex; i < m_rgPlot.Count; i++)
                {
                    if (m_rgPlot[i].Active)
                    {
                        m_dfMinVal = Math.Min(m_dfMinVal, m_rgPlot[i].Count.GetValueOrDefault());
                        m_dfMaxVal = Math.Max(m_dfMaxVal, m_rgPlot[i].Count.GetValueOrDefault());
                    }
                }
            }
            else
            {
                m_dfMinVal = 0;
                m_dfMaxVal = 1;
            }
        }

        public Plot Add(double dfY, bool bActive = true, int nIdx = 0)
        {
            lock (m_syncObj)
            {
                Plot p = new SimpleGraphing.Plot(m_dfXPosition, dfY, null, bActive, nIdx);
                m_rgPlot.Add(p);
                m_dfXPosition += m_dfXIncrement;
                Plot last = getLast();

                if (bActive)
                    setMinMax(last, new float[] { (float)dfY });

                return p;
            }
        }

        public Plot Add(double dfX, double dfY, bool bActive = true, int nIdx = 0)
        {
            lock (m_syncObj)
            {
                Plot p = new SimpleGraphing.Plot(dfX, dfY, null, bActive, nIdx);
                m_rgPlot.Add(p);
                Plot last = getLast();

                if (bActive)
                    setMinMax(last, new float[] { (float)dfY });

                return p;
            }
        }

        public Plot Add(List<double> rgdfY, bool bActive = true)
        {
            return Add(convert(rgdfY), bActive);
        }

        public Plot Add(double dfX, List<double> rgdfY, bool bActive = true)
        {
            return Add(dfX, convert(rgdfY), bActive);
        }

        public Plot Add(List<double> rgdfY, long lCount, bool bActive = true)
        {
            return Add(convert(rgdfY), lCount, bActive);
        }

        public Plot Add(double dfX, List<double> rgdfY, long lCount, bool bActive = true)
        {
            return Add(dfX, convert(rgdfY), lCount, bActive);
        }

        public Plot Add(float[] rgfY, bool bActive = true)
        {
            lock (m_syncObj)
            {
                Plot p = new SimpleGraphing.Plot(m_dfXPosition, rgfY, null, bActive);
                m_rgPlot.Add(p);
                m_dfXPosition += m_dfXIncrement;
                Plot last = getLast();

                if (bActive)
                    setMinMax(last, rgfY);

                return p;
            }
        }

        public Plot Add(double dfX, float[] rgfY, bool bActive = true)
        {
            lock (m_syncObj)
            {
                Plot p = new SimpleGraphing.Plot(dfX, rgfY, null, bActive);
                m_rgPlot.Add(p);
                Plot last = getLast();

                if (bActive)
                    setMinMax(last, rgfY);

                return p;
            }
        }

        public Plot Add(float[] rgfY, long lCount, bool bActive = true)
        {
            lock (m_syncObj)
            {
                Plot p = new Plot(m_dfXPosition, rgfY, lCount, null, bActive);
                m_rgPlot.Add(p);
                m_dfXPosition += m_dfXIncrement;
                Plot last = getLast();

                if (bActive)
                {
                    if (m_minmaxTarget == MINMAX_TARGET.VALUES)
                        setMinMax(last, rgfY);
                    else if (m_minmaxTarget == MINMAX_TARGET.COUNT)
                        setMinMax(last, new float[] { lCount });
                }

                return p;
            }
        }

        public Plot Add(double dfX, float[] rgdfY, long lCount, bool bActive = true)
        {
            lock (m_syncObj)
            {
                Plot p = new Plot(dfX, rgdfY, lCount, null, bActive);
                m_rgPlot.Add(p);
                Plot last = getLast();

                if (bActive)
                {
                    if (m_minmaxTarget == MINMAX_TARGET.VALUES)
                        setMinMax(last, rgdfY);
                    else if (m_minmaxTarget == MINMAX_TARGET.COUNT)
                        setMinMax(last, new float[] { lCount });
                }

                return p;
            }
        }

        public Plot Add(Plot p, bool bCalculateMinMax = true, bool bReplaceIfExists = false)
        {
            lock (m_syncObj)
            {
                bool bAdd = true;

                if (bReplaceIfExists)
                {
                    int nIdx = m_rgPlot.Count - 1;
                    while (nIdx >= 0 && m_rgPlot[nIdx].X > p.X)
                    {
                        nIdx--;
                    }

                    if (nIdx >= 0 && m_rgPlot[nIdx].X == p.X)
                    {
                        bAdd = false;

                        if (OnUpdatePlot != null)
                            OnUpdatePlot(this, new PlotUpdateArgs(m_rgPlot[nIdx], p));
                        else
                            m_rgPlot[nIdx] = p;
                    }
                }

                if (bAdd)
                    m_rgPlot.Add(p);

                Plot last = getLast();
                if (bCalculateMinMax)
                {
                    if (p.Active)
                    {
                        if (m_minmaxTarget == MINMAX_TARGET.VALUES)
                            setMinMax(last, p.Y_values);
                        else if (m_minmaxTarget == MINMAX_TARGET.COUNT)
                            setMinMax(last, new float[] { p.Count.GetValueOrDefault() });
                        else
                            setMinMax(last, null);
                    }
                }

                return p;
            }
        }

        public PlotCollection Add(PlotCollection col, bool bCalculateMinMax = true, bool bActiveOnly = false, bool bReplaceIfExists = true, bool bMaintainCount = false, DateTime? dtSync = null)
        {
            lock (m_syncObj)
            {
                PlotCollection colRemoved = new PlotCollection(col.Name);
                int nCount = m_rgPlot.Count;

                List<Plot> rgPlotsToAdd = new List<Plot>();
                for (int i = col.Count - 1; i >= 0; i--)
                {
                    if (col[i].X >= m_rgPlot[m_rgPlot.Count - 1].X)
                    {
                        rgPlotsToAdd.Insert(0, col[i]);
                    }
                }

                for (int i = 0; i < rgPlotsToAdd.Count; i++)
                {
                    if (!bActiveOnly || col[i].Active)
                        Add(rgPlotsToAdd[i], false, bReplaceIfExists);
                }

                if (bMaintainCount)
                {
                    while (m_rgPlot.Count > nCount && (!dtSync.HasValue || (m_rgPlot[0].Tag is DateTime && (DateTime)m_rgPlot[0].Tag < dtSync.Value)))
                    {
                        colRemoved.Add(m_rgPlot[0], false);
                        m_rgPlot.RemoveAt(0);
                    }

                    if (bCalculateMinMax)
                        colRemoved.SetMinMax();
                }

                if (bCalculateMinMax)
                    SetMinMax();

                return colRemoved;
            }
        }

        private Plot getLast()
        {
            Plot last = null;

            if (m_rgPlot.Count > m_nMax)
            {
                while (m_rgPlot.Count > 0)
                {
                    if (m_rgPlot[0].Active)
                    {
                        last = m_rgPlot[0];
                        m_rgPlot.RemoveAt(0);
                        return last;
                    }

                    m_rgPlot.RemoveAt(0);
                }
            }

            return last;
        }

        public void AddToStart(Plot p)
        {
            lock (m_syncObj)
            {                
                m_rgPlot.Insert(0, p);
            }
        }

        public bool Remove(Plot p)
        {
            lock (m_syncObj)
            {
                return m_rgPlot.Remove(p);
            }
        }

        public void RemoveAt(int nIdx)
        {
            lock (m_syncObj)
            {
                m_rgPlot.RemoveAt(nIdx);
            }
        }

        public void Clear()
        {
            lock (m_syncObj)
            {
                m_rgPlot.Clear();
            }
        }

        public int Find(DateTime dt, int nStartIdx = 0)
        {
            nStartIdx = GetStartIndex(nStartIdx);
            for (int i = nStartIdx; i < m_rgPlot.Count; i++)
            {
                DateTime dt1 = (DateTime)m_rgPlot[i].Tag;

                if (dt1 >= dt)
                    return i;
            }

            return -1;
        }

        public int FindFromEnd(DateTime dt)
        {
            for (int i = m_rgPlot.Count-1; i>=StartIndex; i--)
            {
                DateTime dt1 = (DateTime)m_rgPlot[i].Tag;

                if (dt1 <= dt)
                    return i;
            }

            return -1;
        }

        public void SetAllActive(bool bActivate)
        {
            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                m_rgPlot[i].Active = bActivate;
            }
        }

        public void SetAllActive(bool bActive, string strParam, bool bExact = true)
        {
            for (int i=StartIndex; i<m_rgPlot.Count; i++)
            {
                Plot p = m_rgPlot[i]; 
                bool bFoundParam = false;

                if (p.Parameters != null)
                {
                    foreach (KeyValuePair<string, float> kv in p.Parameters)
                    {
                        if (bExact)
                        {
                            if (kv.Key == strParam)
                            {
                                bFoundParam = true;
                                break;
                            }
                        }
                        else
                        {
                            if (kv.Key.Contains(strParam))
                            {
                                bFoundParam = true;
                                break;
                            }
                        }
                    }
                }

                p.Active = bFoundParam;
            }
        }

        public PlotCollection Overlay(PlotCollection col2, int? nPrimaryYIdx, out int nFoundCount, params string[] rgstrParamsToCopy)
        {
            PlotCollection col = Clone(0, false, nPrimaryYIdx, false);
            int nIdx = 0;
            nFoundCount = 0;

            int nStartIdx = StartIndex;
            if (col2.StartIndex != nStartIdx)
                throw new Exception("The overlay plot collection must have the same start index as the destination plot collection.");
           
            for (int i = nStartIdx; i < col.Count; i++)
            {
                if (col[i].Tag == null)
                    col[i].Tag = DateTime.FromFileTime((long)col[i].X);

                if (col2[nIdx].Tag == null)
                    col2[nIdx].Tag = DateTime.FromFileTime((long)col2[nIdx].X);

                DateTime dt = (DateTime)col[i].Tag;
                DateTime dt0 = (DateTime)col2[nIdx].Tag;

                if (dt == dt0)
                {
                    col[i].SetYValues(col2[nIdx].Y_values);
                    col[i].PrimaryIndexY = col2[nIdx].PrimaryIndexY;
                    col[i].Active = col2[nIdx].Active;

                    if (rgstrParamsToCopy != null)
                    {
                        for (int j = 0; j < rgstrParamsToCopy.Length; j++)
                        {
                            string strParam = rgstrParamsToCopy[j];
                            float? fVal = col2[nIdx].GetParameter(strParam);

                            if (fVal.HasValue)
                                col[i].SetParameter(strParam, fVal.Value);
                        }
                    }

                    nFoundCount++;
                    nIdx++;

                    if (nIdx == col2.Count)
                        break;
                }
            }

            return col;
        }

        //public IEnumerator<Plot> GetEnumerator()
        //{
        //    return m_rgPlot.GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return m_rgPlot.GetEnumerator();
        //}

        public override string ToString()
        {
            string strOut = m_strName;

            if (m_rgPlot.Count == 0)
            {
                strOut += " ~ empty";
            }
            else
            {
                strOut += " ~ [" + m_rgPlot.Count.ToString() + "] ";

                string strDates = "";
                int nStartIdx = StartIndex;

                if (m_rgPlot[nStartIdx].Tag is DateTime)
                    strDates += ((DateTime)m_rgPlot[nStartIdx].Tag).ToString();

                if (m_rgPlot[m_rgPlot.Count - 1].Tag is DateTime)
                {
                    if (strDates.Length > 0)
                        strDates += " - ";
                    strDates += ((DateTime)m_rgPlot[m_rgPlot.Count - 1].Tag).ToString();
                }

                strOut += strDates;
            }

            return strOut;
        }

        /// <summary>
        /// Calculate the A and B regression values.
        /// </summary>
        /// <param name="nDataIdx">Optionally, specifies the data index into Y_values to use (default = -1, which uses Y).</param>
        /// <param name="nStartIdx">Optionally, specifies the start index in the data (default = 0).</param>
        /// <param name="nEndIdx">Optionally, specifies the end index in the data (default = count-1).</param>
        /// <param name="strParamName">Optionally, specifies to calculate regression of a parameter instead of the Y value at the data index.</param>
        /// <remarks>
        /// @see [Linear Regresssion: Simple Steps](https://www.statisticshowto.datasciencecentral.com/probability-and-statistics/regression-analysis/find-a-linear-regression-equation/)
        /// </remarks>
        /// <returns>A tuple containing the A and B values is returned.</returns>
        public Tuple<double, double> CalculateLinearRegressionAB(int nDataIdx = -1, int nStartIdx = 0, int nEndIdx = -1, string strParamName = null)
        {
            double dfSumX = 0;
            double dfSumY = 0;
            double dfSumX2 = 0;
            double dfSumXY = 0;
            int nN = 0;

            if (nEndIdx < 0 || nEndIdx > m_rgPlot.Count-1)
                nEndIdx = m_rgPlot.Count - 1;

            nStartIdx = GetStartIndex(nStartIdx);            
            for (int i = nStartIdx; i <= nEndIdx; i++)
            {
                int nIdx = i + 1;
                int nIdx0 = nIdx - nStartIdx;
                Plot p = m_rgPlot[i];

                if (p.Active)
                {
                    dfSumX += nIdx0;

                    double dfY = 0;

                    if (!string.IsNullOrEmpty(strParamName))
                    {
                        float? fVal = p.GetParameterContaining(strParamName);
                        if (!fVal.HasValue)
                            return null;

                        dfY = fVal.Value;
                    }
                    else
                    {
                        dfY = (nDataIdx >= 0 && nDataIdx < p.Y_values.Length) ? p.Y_values[nDataIdx] : (p.Y_values.Length == 1) ? p.Y_values[0] : p.Y;
                    }

                    dfSumY += dfY;
                    dfSumX2 += nIdx0 * nIdx0;
                    dfSumXY += nIdx0 * dfY;
                    nN++;
                }
            }

            double dfA1 = (dfSumY * dfSumX2) - (dfSumX * dfSumXY);
            double dfB1 = (nN * dfSumXY) - (dfSumX * dfSumY);
            double dfDiv = (nN * dfSumX2) - Math.Pow(dfSumX, 2.0);
            double dfA = dfA1 / dfDiv;
            double dfB = dfB1 / dfDiv;

            return new Tuple<double, double>(dfA, dfB);
        }

        /// <summary>
        /// Calculate the linear regression y value when given an x (the index) value.
        /// </summary>
        /// <param name="dfX">Specifies the x (index) value of a plot.</param>
        /// <param name="dfA">Specifies the linear regression A value.</param>
        /// <param name="dfB">Specifies the linear regression B value.</param>
        /// <returns>The Y value of the linear regression plot is returned.</returns>
        public double CalculateLinearRegressionY(double dfX, double dfA, double dfB)
        {
            return dfA + dfB * dfX;
        }

        /// <summary>
        /// Calculate the linear regression line with confidence width lines set as 'Confidence+' and 'Confidence-' parameters.
        /// </summary>
        /// <param name="dfSlope">Returns the regression line slope.</param>
        /// <param name="dfConfidenceWidth">Returns the regression line confidence width.</param>
        /// <param name="nDataIdx">Optionally, specifies the data index into Y_values to use (default = -1, which uses Y).</param>
        /// <param name="nStartIdx">Optionally, specifies the start index in the data (default = 0).</param>
        /// <param name="nEndIdx">Optionally, specifies the end index in the data (default = count-1).</param>
        /// <param name="strParamName">Optionally, specifies to calculate regression of a parameter instead of the Y value at the data index.</param>
        /// <returns>The regression line is returned.</returns>
        public PlotCollection CalculateLinearRegressionLines(out double dfSlope, out double dfConfidenceWidth, int nDataIdx = -1, int nStartIdx = 0, int nEndIdx = -1, string strParamName = null)
        {
            dfSlope = 0;
            dfConfidenceWidth = 0;

            nStartIdx = GetStartIndex(nStartIdx);
            Tuple<double, double> ab = CalculateLinearRegressionAB(nDataIdx, nStartIdx, nEndIdx, strParamName);
            if (ab == null)
                return null;

            PlotCollection col = new PlotCollection(Name + " Regresssion Line");
            List<float> rgRegY = new List<float>();
            List<float> rgError = new List<float>();

            if (nEndIdx < 0)
                nEndIdx = m_rgPlot.Count - 1;

            if (nEndIdx >= m_rgPlot.Count)
                nEndIdx = m_rgPlot.Count - 1;

            for (int i = nStartIdx; i <= nEndIdx; i++)
            {
                Plot p0 = m_rgPlot[i];
                
                float fY = (float)CalculateLinearRegressionY(i, ab.Item1, ab.Item2);

                rgRegY.Add(fY);

                if (p0.Active)
                {
                    float fErr = p0.Y - fY;
                    rgError.Add(fErr * fErr);
                }

                Plot p = new Plot(p0.X, fY, null, true, i);
                p.Tag = p0.Tag;

                col.Add(p);
            }

            dfSlope = (col[col.Count - 1].Y - col[0].Y) / col.Count;
            double dfVar = Math.Sqrt(rgError.Sum() / rgError.Count);
            double df95 = dfVar * 1.96;

            for (int i = 0; i < col.Count; i++)
            {
                col[i].SetParameter("Confidence+", col[i].Y + df95);
                col[i].SetParameter("Confidence-", col[i].Y - df95);
                col[i].SetParameter("Slope", dfSlope);
            }

            dfConfidenceWidth = df95 * 2;

            return col;
        }

        public RangeStatisticsCollection CalculateRangeStatistics(int nStartIdx = 0, int nCount = 20, bool bFullRange = false)
        {
            CalculationArray ca = new CalculationArray(nCount);
            CalculationArray caTotal = new CalculationArray(nCount);
            RangeStatisticsCollection rgCol = new RangeStatisticsCollection();

            nStartIdx = GetStartIndex(nStartIdx);
            for (int i = nStartIdx; i < m_rgPlot.Count; i++)
            {
                Plot p = m_rgPlot[i];

                if (p.Active)
                {
                    int nO = (bFullRange) ? 1 : 0;
                    int nC = (bFullRange) ? 2 : 3;
                    double dfA = p.Y_values[nC];
                    double dfB = p.Y_values[nO];
                    double dfRange = dfA - dfB;
                    
                    DateTime? dt = null;
                    if (p.Tag != null)
                        dt = (DateTime)p.Tag;

                    if (ca.Add(dfRange, dt))
                        rgCol.Add(new RangeStatistics(ca.Average, ca.StdDev, ca.GetLast(0), ca.GetLast(-1), ca.GetLast(-2), ca.LastRaw, ca.TimeStamp));
                }
            }

            return rgCol;
        }

        public float CalculateCorrelationCoefficient(PlotCollection plots, int nDataIdx = -1, int nStartIdx = 0, int nEndIdx = -1)
        {
            if (plots.Count != Count)
                throw new Exception("The two plot collections must have the same counts!");

            nStartIdx = GetStartIndex(nStartIdx);
            if (nEndIdx < 0 || nEndIdx > m_rgPlot.Count - 1)
                nEndIdx = m_rgPlot.Count - 1;

            float fV0Total = 0;
            float fV1Total = 0;
            int nCount = 0;
            for (int i = nStartIdx; i <= nEndIdx; i++)
            {
                float fV0 = (nDataIdx >= 0) ? m_rgPlot[i].Y_values[nDataIdx] : m_rgPlot[i].Y;
                float fV1 = (nDataIdx >= 0) ? plots.m_rgPlot[i].Y_values[nDataIdx] : plots.m_rgPlot[i].Y;
                fV0Total += fV0;
                fV1Total += fV1;
                nCount++;
            }

            float fV0Ave = fV0Total / nCount;
            float fV1Ave = fV1Total / nCount;

            float fSumA = 0;
            float fSumB = 0;
            float fSumC = 0;

            for (int i = nStartIdx; i <= nEndIdx; i++)
            {
                float fV0 = (nDataIdx >= 0) ? m_rgPlot[i].Y_values[nDataIdx] : m_rgPlot[i].Y;
                float fV1 = (nDataIdx >= 0) ? plots.m_rgPlot[i].Y_values[nDataIdx] : plots.m_rgPlot[i].Y;

                float fV0Var = fV0 - fV0Ave;
                float fV1Var = fV1 - fV1Ave;

                fSumA += fV0Var * fV1Var;
                fSumB += fV0Var * fV0Var;
                fSumC += fV1Var * fV1Var;
            }

            return fSumA / (float)Math.Sqrt(fSumB * fSumC);
        }

        public void Normalize(bool bSetMinMax = false, PlotCollection col = null)
        {
            double dfNewMin = 0;
            double dfNewMax = 1;

            SetMinMax();

            if (col != null)
            {
                col.SetMinMax();
                dfNewMin = col.AbsoluteMinYVal;
                dfNewMax = col.AbsoluteMaxYVal;
            }

            double dfOldMin = AbsoluteMinYVal;
            double dfOldMax = AbsoluteMaxYVal;
            double dfOldRange = dfOldMax - dfOldMin;
            double dfNewRange = dfNewMax - dfNewMin;
            double dfRatio = (dfOldRange == 0) ? 0 : dfNewRange / dfOldRange;

            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                for (int j = 0; j < m_rgPlot[i].Y_values.Length; j++)
                {
                    m_rgPlot[i].Y_values[j] = (float)(((m_rgPlot[i].Y_values[j] - dfOldMin) * dfRatio) + dfNewMin);
                }
            }

            if (bSetMinMax)
                SetMinMax();
        }

        public void Scale(double dfScale, double dfShift, bool bSetMinMax = true)
        {
            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                m_rgPlot[i].Y *= (float)dfScale;
                m_rgPlot[i].Y += (float)dfShift;
            }

            if (bSetMinMax)
                SetMinMax();
        }

        public void Average(int nPeriod, bool bSetMinMax = true)
        {
            List<float> rgVal = new List<float>();

            for (int i = StartIndex; i < m_rgPlot.Count; i++)
            {
                rgVal.Add(m_rgPlot[i].Y);
                if (rgVal.Count > nPeriod)
                    rgVal.RemoveAt(0);

                m_rgPlot[i].Y = (float)rgVal.Average();
            }

            if (bSetMinMax)
                SetMinMax();
        }

        public PlotCollection Clip(int nStartIdx, int nEndIdx, bool bSetMinMax = true)
        {
            PlotCollection col = new PlotCollection(Name);

            foreach (KeyValuePair<string, double> kv in m_rgParam)
            {
                col.Parameters.Add(kv.Key, kv.Value);
            }

            col.Tag = Tag;
            col.Tag2 = Tag2;
            col.LastUpdateTime = m_dtLastUpdate;

            for (int i = nStartIdx; i <= nEndIdx && i < m_rgPlot.Count; i++)
            {
                col.Add(m_rgPlot[i]);
            }

            if (bSetMinMax)
                col.SetMinMax();

            return col;
        }

        public int FindX(double dfX)
        {
            for (int i = StartIndex + 1; i < m_rgPlot.Count; i++)
            {
                if (m_rgPlot[i - 1].X <= (float)dfX && m_rgPlot[i].X > dfX)
                    return i - 1;
            }

            return -1;
        }

        public PlotCollection ClipAround(double dfX, int nSpread, bool bSetMinMax = true)
        {
            int nIdx = FindX(dfX);

            if (nIdx < 0)
                return this;

            PlotCollection col = new PlotCollection(Name);

            foreach (KeyValuePair<string, double> kv in m_rgParam)
            {
                col.Parameters.Add(kv.Key, kv.Value);
            }

            col.Tag = Tag;
            col.Tag2 = Tag2;

            int nStartIdx = nIdx - nSpread;
            if (nStartIdx < 0)
                nStartIdx = 0;

            int nEndIdx = nIdx + nSpread;
            if (nEndIdx >= Count)
                nEndIdx = Count - 1;

            for (int i = nStartIdx; i <= nEndIdx; i++)
            {
                col.Add(m_rgPlot[i]);
            }

            if (bSetMinMax)
                col.SetMinMax();

            return col;
        }

        public double? GetLikelyTimePeriodInSeconds(int nMinTest = 20)
        {
            Dictionary<double, int> rgTp = new Dictionary<double, int>();

            if (m_rgPlot.Count < nMinTest)
                return null;

            for (int i = StartIndex + 1; i < nMinTest; i++)
            {
                if (m_rgPlot[i].Tag != null &&
                    m_rgPlot[i - 1].Tag != null)
                {
                    DateTime dt1 = (DateTime)m_rgPlot[i].Tag;
                    DateTime dt0 = (DateTime)m_rgPlot[i - 1].Tag;
                    TimeSpan ts = dt1 - dt0;
                    double dfS = ts.TotalSeconds;

                    if (!rgTp.ContainsKey(dfS))
                        rgTp.Add(dfS, 1);
                    else
                        rgTp[dfS]++;
                }
            }

            if (rgTp.Count == 0)
                return null;

            return rgTp.OrderByDescending(p => p.Value).ToList()[0].Key;
        }
    }

    public class PlotUpdateArgs : EventArgs
    {
        Plot m_plotOriginal;
        Plot m_plotNew;

        public PlotUpdateArgs(Plot pOriginal, Plot pNew)
        {
            m_plotOriginal = pOriginal;
            m_plotNew = pNew;
        }

        public Plot OriginalPlot
        {
            get { return m_plotOriginal; }
        }

        public Plot NewPlot
        {
            get { return m_plotNew; }
        }
    }

    public class RangeStatisticsCollection : IEnumerable<RangeStatistics>
    {
        List<RangeStatistics> m_rgItems = new List<RangeStatistics>();

        public RangeStatisticsCollection()
        {
        }

        public int Count
        {
            get { return m_rgItems.Count; }
        }

        public RangeStatistics this[int nIdx]
        {
            get { return m_rgItems[nIdx]; }
        }

        public void Add(RangeStatistics r)
        {
            m_rgItems.Add(r);
        }

        public IEnumerator<RangeStatistics> GetEnumerator()
        {
            return m_rgItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgItems.GetEnumerator();
        }
    }

    public class RangeStatistics
    {
        DateTime? m_dt;
        double m_dfLastRaw;
        double m_dfRangeAve;
        double m_dfRangeStdDev;
        double m_dfRangeLast0;
        double m_dfRangeLast1;
        double m_dfRangeLast2;

        public RangeStatistics(double dfRangeAve, double dfRangeStdDev, double dfRangeLast0, double dfRangeLast1, double dfRangeLast2, double dfLastRaw, DateTime? dt)
        {
            m_dt = dt;
            m_dfLastRaw = dfLastRaw;
            m_dfRangeAve = dfRangeAve;
            m_dfRangeStdDev = dfRangeStdDev;
            m_dfRangeLast0 = dfRangeLast0;
            m_dfRangeLast1 = dfRangeLast1;
            m_dfRangeLast2 = dfRangeLast2;
        }

        public bool IsValid
        {
            get
            {
                if (m_dfRangeAve == 0 || m_dfRangeStdDev == 0)
                    return false;

                return true;
            }
        }

        public DateTime? TimeStamp
        {
            get { return m_dt; }
        }

        public double LastRaw
        {
            get { return m_dfLastRaw; }
        }

        public double RangeAverage
        {
            get { return m_dfRangeAve; }
        }

        public double RangeStdDev
        {
            get { return m_dfRangeStdDev; }
        }

        public double RangeLast0
        {
            get { return m_dfRangeLast0; }
        }

        public double RangeLast1
        {
            get { return m_dfRangeLast1; }
        }

        public double RangeLast2
        {
            get { return m_dfRangeLast2; }
        }

        public double GetRangeLastMax(int nCount)
        {
            if (nCount <= 0 || nCount > 3)
                throw new Exception("The nCount must be in the range [1,3].");

            List<double> rg = new List<double>() { m_dfRangeLast0, m_dfRangeLast1, m_dfRangeLast2 };
            double dfMax = 0;

            for (int i = 0; i < nCount; i++)
            {
                dfMax = Math.Max(dfMax, rg[i]);
            }

            return dfMax;
        }
    }
}
