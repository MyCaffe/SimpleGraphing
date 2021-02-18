using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    [Serializable]
    public class PlotCollectionSet : IEnumerable<PlotCollection>
    {
        Dictionary<string, string> m_rgUserProperties = new Dictionary<string, string>();
        List<PlotCollection> m_rgSet = new List<PlotCollection>();
        double m_dfMarginPct = 0;

        public PlotCollectionSet(PlotCollection col)
        {
            m_rgSet.Add(col);
        }

        public PlotCollectionSet(List<PlotCollection> rgPlots = null)
        {
            if (rgPlots != null && rgPlots.Count > 0)
                m_rgSet.AddRange(rgPlots);
        }

        public PlotCollectionSet(PlotCollectionSet set, int nStart)
        {
            for (int i = nStart; i < set.Count; i++)
            {
                m_rgSet.Add(set[i]);
            }
        }

        public Dictionary<string, string> UserProperties
        {
            get { return m_rgUserProperties; }
        }

        public void SynchronizeStart()
        {
            if (m_rgSet.Count <= 1)
                return;

            if (m_rgSet[0].Count == 0)
                return;

            DateTime dtFirst = DateTime.FromFileTime((long)m_rgSet[0][0].X);
            for (int i = 1; i < m_rgSet.Count; i++)
            {
                if (m_rgSet[i].Count > 0)
                {
                    DateTime dt = DateTime.FromFileTime((long)m_rgSet[i][0].X);                    
                    if (dt > dtFirst)
                        dtFirst = dt;
                }
            }

            for (int i = 0; i < m_rgSet.Count; i++)
            {
                while (m_rgSet[i].Count > 0)
                {
                    DateTime dt = DateTime.FromFileTime((long)m_rgSet[i][0].X);
                    if (dt < dtFirst)
                        m_rgSet[i].RemoveAt(0);
                    else
                        break;
                }
            }
        }

        public PlotCollectionSet Clone()
        {
            PlotCollectionSet col = new PlotCollectionSet();

            col.m_dfMarginPct = m_dfMarginPct;

            foreach (PlotCollection pc in m_rgSet)
            {
                col.Add(pc.Clone());
            }

            return col;
        }

        public void SetMinMaxTarget(PlotCollection.MINMAX_TARGET minmaxTarget)
        {
            foreach (PlotCollection pc in m_rgSet)
            {
                pc.MinMaxTarget = minmaxTarget;
            }
        }

        public void SetMinMax(int nStartIdx = 0, bool? bLock = null)
        {
            foreach (PlotCollection pc in m_rgSet)
            {
                pc.SetMinMax(nStartIdx, bLock);
            }
        }

        private int findStart(PlotCollectionSet set, params string[] rgstrContains)
        {
            for (int i = 0; i < set.Count; i++)
            {
                string strName = set.m_rgSet[i].Name;

                for (int j = 0; j < rgstrContains.Length; j++)
                {
                    string strTarget = rgstrContains[j];

                    if (strName.IndexOf(strTarget) >= 0)
                        return i;
                }
            }

            return 0;
        }

        public PlotCollectionSet(PlotCollectionSet set, params string[] rgstrContains)
        {
            int nStart = findStart(set, rgstrContains);

            for (int i = nStart; i < set.Count; i++)
            {
                m_rgSet.Add(set[i]);
            }
        }

        public PlotCollection Find(string str)
        {
            foreach (PlotCollection col in m_rgSet)
            {
                if (col.Name == str)
                    return col;
            }

            return null;
        }

        public Tuple<PlotCollectionSet, PlotCollectionSet> Split(int nCount, bool bSetDateOnTag = false, bool bAppendSplitCountToName = false)
        {
            PlotCollectionSet p1 = new PlotCollectionSet();
            PlotCollectionSet p2 = new PlotCollectionSet();

            foreach (PlotCollection plots in m_rgSet)
            {
                Tuple<PlotCollection, PlotCollection> p = plots.Split(nCount, bSetDateOnTag, bAppendSplitCountToName);
                p1.Add(p.Item1);
                p2.Add(p.Item2);
            }

            return new Tuple<PlotCollectionSet, PlotCollectionSet>(p1, p2);
        }

        public void SetMarginPercent(double dfPct)
        {
            m_dfMarginPct = dfPct;
        }

        public double MarginPercent
        {
            get { return m_dfMarginPct; }
        }

        public void ExcludeFromMinMax(bool bExcludeFromMinMax)
        {
            foreach (PlotCollection plots in m_rgSet)
            {
                if (plots != null)
                    plots.ExcludeFromMinMax = bExcludeFromMinMax;
            }
        }

        public void ClearData()
        {
            foreach (PlotCollection data in m_rgSet)
            {
                data.Clear();
            }
        }

        public void RemoveAllButFirst()
        {
            if (m_rgSet.Count > 1)
                m_rgSet = new List<PlotCollection>() { m_rgSet[0] };
        }

        public bool Contains(PlotCollection plots)
        {
            if (m_rgSet.Count == 0)
                return false;

            foreach (PlotCollection plots0 in m_rgSet)
            {
                if (!plots0.ComparePlots(plots))
                    return false;
            }

            return true;
        }

        public PlotCollectionSet GetPlotsContaining(string strName)
        {
            List<PlotCollection> rgPlots = m_rgSet.Where(p => p.Name.Contains(strName)).ToList();
            return new PlotCollectionSet(rgPlots);
        }

        public void GetAbsMinMax(int nLastIdx, int nDataStartIdx, out double dfAbsMinY, out double dfAbsMaxY)
        {
            dfAbsMinY = double.MaxValue;
            dfAbsMaxY = -double.MaxValue;

            for (int i = 0; i < m_rgSet.Count && i <= nLastIdx; i++)
            {
                if (!m_rgSet[i].ExcludeFromMinMax)
                {
                    if (m_rgSet[i].MinMaxTarget == PlotCollection.MINMAX_TARGET.PARAMS)
                    {
                        dfAbsMinY = 0;
                        dfAbsMaxY = 1;
                    }
                    else
                    {
                        if (nDataStartIdx == 0)
                        {
                            dfAbsMinY = Math.Min(dfAbsMinY, m_rgSet[i].AbsoluteMinYVal);
                            dfAbsMaxY = Math.Max(dfAbsMaxY, m_rgSet[i].AbsoluteMaxYVal);
                        }
                        else
                        {
                            for (int j = nDataStartIdx; j < m_rgSet[i].Count; j++)
                            {
                                dfAbsMinY = Math.Min(dfAbsMinY, m_rgSet[i][j].Y_values.Min());
                                dfAbsMaxY = Math.Max(dfAbsMaxY, m_rgSet[i][j].Y_values.Max());
                            }
                        }
                    }
                }
            }
        }

        public void GetMinMaxOverWindow(int nStartIdx, int nCount, out double dfMinX, out double dfMinY, out double dfMaxX, out double dfMaxY, out double dfAbsMinY, out double dfAbsMaxY)
        {
            dfMinX = double.MaxValue;
            dfMaxX = -double.MaxValue;
            dfMinY = double.MaxValue;
            dfMaxY = -double.MaxValue;
            dfAbsMinY = double.MaxValue;
            dfAbsMaxY = -double.MaxValue;

            for (int i = 0; i < m_rgSet.Count; i++)
            {
                if (m_rgSet[i] == null || m_rgSet[i].ExcludeFromMinMax)
                    continue;

                double dfMinX1;
                double dfMaxX1;
                double dfMinY1;
                double dfMaxY1;

                m_rgSet[i].GetMinMaxOverWindow(nStartIdx, nCount, out dfMinX1, out dfMinY1, out dfMaxX1, out dfMaxY1);

                dfMinX = Math.Min(dfMinX, dfMinX1);
                dfMaxX = Math.Max(dfMaxX, dfMaxX1);
                dfMinY = Math.Min(dfMinY, dfMinY1);
                dfMaxY = Math.Max(dfMaxY, dfMaxY1);

                dfAbsMinY = Math.Min(dfAbsMinY, m_rgSet[i].AbsoluteMinYVal);
                dfAbsMaxY = Math.Max(dfAbsMaxY, m_rgSet[i].AbsoluteMaxYVal);
            }

            if (m_dfMarginPct > 0)
            {
                dfAbsMinY -= (dfAbsMinY * m_dfMarginPct);
                dfAbsMaxY += (dfAbsMaxY * m_dfMarginPct);
            }
        }

        public int Count
        {
            get { return m_rgSet.Count; }
        }

        public PlotCollection this[int nIdx]
        {
            get { return m_rgSet[nIdx]; }
            set { m_rgSet[nIdx] = value; }
        }

        public void Add(PlotCollection rg)
        {
            m_rgSet.Add(rg);
        }

        public int Add(PlotCollectionSet set, bool bUniqueOnly = false, bool bCopyUserProperties = true)
        {
            if (!bUniqueOnly)
            {
                m_rgSet.AddRange(set.m_rgSet);
                return set.m_rgSet[0].Count;
            }

            int nCount = (m_rgSet.Count == 0) ? 0 : m_rgSet[0].Count;

            foreach (PlotCollection plots in set)
            {
                if (!m_rgSet.Contains(plots))
                    m_rgSet.Add(plots);
            }

            int nNewCount = m_rgSet[0].Count;

            if (bCopyUserProperties)
            {
                List<KeyValuePair<string, string>> rg = set.UserProperties.ToList();

                foreach (KeyValuePair<string, string> kv in rg)
                {
                    if (!UserProperties.ContainsKey(kv.Key))
                        UserProperties.Add(kv.Key, kv.Value);
                    else
                        UserProperties[kv.Key] = kv.Value;
                }
            }

            return nNewCount - nCount;
        }

        public bool Remove(PlotCollection rg)
        {
            return m_rgSet.Remove(rg);
        }

        public void RemoveAt(int nIdx)
        {
            m_rgSet.RemoveAt(nIdx);
        }

        public void RemoveLastDataItems(DateTime dt)
        {
            foreach (PlotCollection col in m_rgSet)
            {
                int nIdx = col.Count - 1;
                while (nIdx >= 0 && col.Count > 0)
                {
                    DateTime dt1 = (DateTime)col[nIdx].Tag;
                    if (dt1 >= dt)
                        col.RemoveAt(nIdx);
                    else
                        break;

                    nIdx--;
                }
            }
        }

        public void Clear()
        {
            m_rgSet.Clear();
        }

        public int GetIndex(string strName, bool bContains = false)
        {
            for (int i = 0; i < m_rgSet.Count; i++)
            {
                if ((!bContains && m_rgSet[i].Name == strName) ||
                    (bContains && m_rgSet[i].Name.Contains(strName)))
                    return i;
            }

            throw new Exception("Could not find a set item with a name containing '" + strName + "'!");
        }

        public Plot GetLastActive(string strNameActive, string strNameData)
        {
            int nActiveIdx = GetIndex(strNameActive);
            int nPlotIdx = GetIndex(strNameData);
            PlotCollection colActive = m_rgSet[nActiveIdx];
            PlotCollection colPlot = m_rgSet[nPlotIdx];

            for (int i = colActive.Count - 1; i >= 0; i--)
            {
                if (colActive[i].Active)
                    return colPlot[i];
            }

            return null;
        }

        public IEnumerator<PlotCollection> GetEnumerator()
        {
            return m_rgSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgSet.GetEnumerator();
        }

        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(m_rgSet.Count);

                for (int i = 0; i < m_rgSet.Count; i++)
                {
                    byte[] rgData = m_rgSet[i].Save();

                    bw.Write(rgData.Length);
                    bw.Write(rgData);
                }

                ms.Flush();
                return ms.ToArray();
            }
        }

        public static PlotCollectionSet Load(byte[] rgData, int nMax = int.MaxValue)
        {
            PlotCollectionSet set = new PlotCollectionSet();

            using (MemoryStream ms = new MemoryStream(rgData))
            using (BinaryReader br = new BinaryReader(ms))
            {
                int nCount = br.ReadInt32();
                if (nCount > nMax)
                    nCount = nMax;

                for (int i = 0; i < nCount; i++)
                {
                    int nLen = br.ReadInt32();
                    byte[] rgData1 = br.ReadBytes(nLen);

                    set.Add(PlotCollection.Load(rgData1));
                }
            }

            return set;
        }

        public static byte[] SaveList(List<PlotCollectionSet> rgSet)
        {
            if (rgSet == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(rgSet.Count);

                for (int i = 0; i < rgSet.Count; i++)
                {
                    byte[] rgData = rgSet[i].Save();

                    bw.Write(rgData.Length);
                    bw.Write(rgData);
                }

                ms.Flush();
                return ms.ToArray();
            }
        }

        public static List<PlotCollectionSet> LoadList(byte[] rgData, int nMaxList = int.MaxValue, int nMaxSet = int.MaxValue)
        {
            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>();

            using (MemoryStream ms = new MemoryStream(rgData))
            using (BinaryReader br = new BinaryReader(ms))
            {
                int nCount = br.ReadInt32();
                if (nCount > nMaxList)
                    nCount = nMaxList;

                for (int i = 0; i < nCount; i++)
                {
                    int nLen = br.ReadInt32();
                    byte[] rgData1 = br.ReadBytes(nLen);

                    rgSet.Add(PlotCollectionSet.Load(rgData1, nMaxSet));
                }
            }

            return rgSet;
        }
    }
}
