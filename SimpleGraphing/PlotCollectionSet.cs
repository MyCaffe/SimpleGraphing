using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class PlotCollectionSet : IEnumerable<PlotCollection>
    {
        public List<PlotCollection> m_rgSet = new List<PlotCollection>();
        double m_dfMarginPct = 0;

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

        public Tuple<PlotCollectionSet, PlotCollectionSet> Split(int nCount)
        {
            PlotCollectionSet p1 = new PlotCollectionSet();
            PlotCollectionSet p2 = new PlotCollectionSet();

            foreach (PlotCollection plots in m_rgSet)
            {
                Tuple<PlotCollection, PlotCollection> p = plots.Split(nCount);
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

        public void GetAbsMinMax(int nLastIdx, out double dfAbsMinY, out double dfAbsMaxY)
        {
            dfAbsMinY = double.MaxValue;
            dfAbsMaxY = -double.MaxValue;

            for (int i = 0; i < m_rgSet.Count && i<=nLastIdx; i++)
            {
                if (!m_rgSet[i].ExcludeFromMinMax)
                {
                    dfAbsMinY = Math.Min(dfAbsMinY, m_rgSet[i].AbsoluteMinYVal);
                    dfAbsMaxY = Math.Max(dfAbsMaxY, m_rgSet[i].AbsoluteMaxYVal);
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

        public void Add(PlotCollectionSet set, bool bUniqueOnly = false)
        {
            if (!bUniqueOnly)
            {
                m_rgSet.AddRange(set.m_rgSet);
                return;
            }

            foreach (PlotCollection plots in set)
            {
                if (!m_rgSet.Contains(plots))
                    m_rgSet.Add(plots);
            }
        }

        public bool Remove(PlotCollection rg)
        {
            return m_rgSet.Remove(rg);
        }

        public void RemoveAt(int nIdx)
        {
            m_rgSet.RemoveAt(nIdx);
        }

        public void Clear()
        {
            m_rgSet.Clear();
        }

        public IEnumerator<PlotCollection> GetEnumerator()
        {
            return m_rgSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgSet.GetEnumerator();
        }
    }
}
