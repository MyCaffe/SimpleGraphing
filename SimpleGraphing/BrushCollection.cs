using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class BrushCollection : IDisposable 
    {
        Dictionary<Color, Brush> m_rgBrushes = new Dictionary<Color, Brush>();

        public BrushCollection()
        {
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Color, Brush> kv in m_rgBrushes)
            {
                kv.Value.Dispose();
            }

            m_rgBrushes.Clear();
        }

        public void Add(Color clr)
        {
            if (!m_rgBrushes.ContainsKey(clr))
                m_rgBrushes.Add(clr, new SolidBrush(clr));
        }

        public Brush this[Color clr]
        {
            get { return m_rgBrushes[clr]; }
        }

        public bool Contains(Color clr)
        {
            return m_rgBrushes.ContainsKey(clr);
        }
    }
}
