using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphPlotCollection : IEnumerable<GraphPlot>, IDisposable
    {
        List<GraphPlot> m_rgPlots = new List<GraphPlot>();

        public GraphPlotCollection()
        {
        }

        public void Dispose()
        {
            foreach (GraphPlot p in m_rgPlots)
            {
                p.Dispose();
            }

            m_rgPlots.Clear();
        }

        public int Count
        {
            get { return m_rgPlots.Count; }
        }

        public GraphPlot this[int nIdx]
        {
            get { return m_rgPlots[nIdx]; }
            set { m_rgPlots[nIdx] = value; }
        }

        public void Add(GraphPlot frame)
        {
            m_rgPlots.Add(frame);
        }

        public bool Remove(GraphPlot frame)
        {
            return m_rgPlots.Remove(frame);
        }

        public void RemoveAt(int nIdx)
        {
            m_rgPlots.RemoveAt(nIdx);
        }

        public void Clear()
        {
            m_rgPlots.Clear();
        }

        public IEnumerator<GraphPlot> GetEnumerator()
        {
            return m_rgPlots.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgPlots.GetEnumerator();
        }
    }
}
