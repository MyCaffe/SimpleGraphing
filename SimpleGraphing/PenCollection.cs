using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class PenCollection : IDisposable 
    {
        Dictionary<Color, Pen> m_rgPens = new Dictionary<Color, Pen>();

        public PenCollection()
        {
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Color, Pen> kv in m_rgPens)
            {
                kv.Value.Dispose();
            }

            m_rgPens.Clear();
        }

        public void Add(Color clr)
        {
            if (!m_rgPens.ContainsKey(clr))
                m_rgPens.Add(clr, new Pen(clr, 1.0f));
        }

        public Pen this[Color clr]
        {
            get { return m_rgPens[clr]; }
        }
    }
}
