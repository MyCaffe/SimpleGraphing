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

        public void GetAbsMinMax(out double dfAbsMinY, out double dfAbsMaxY)
        {
            dfAbsMinY = double.MaxValue;
            dfAbsMaxY = -double.MaxValue;

            for (int i = 0; i < m_rgSet.Count; i++)
            {
                dfAbsMinY = Math.Min(dfAbsMinY, m_rgSet[i].AbsoluteMinYVal);
                dfAbsMaxY = Math.Max(dfAbsMaxY, m_rgSet[i].AbsoluteMaxYVal);
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

        public void Add(PlotCollectionSet set)
        {
            m_rgSet.AddRange(set.m_rgSet);
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
