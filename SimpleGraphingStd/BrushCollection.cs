using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd
{
    public class BrushCollection : IDisposable
    {
        Dictionary<SKColor, SKPaint> m_rgBrushes = new Dictionary<SKColor, SKPaint>();

        public BrushCollection() { }

        public void Dispose()
        {
            foreach (var kv in m_rgBrushes)
            {
                kv.Value.Dispose();
            }

            m_rgBrushes.Clear();
        }

        public SKPaint Add(SKColor clr)
        {
            if (!m_rgBrushes.ContainsKey(clr))
            {
                m_rgBrushes[clr] = new SKPaint
                {
                    Color = clr,
                    Style = SKPaintStyle.Fill
                };
            }

            return m_rgBrushes[clr];
        }

        public SKPaint this[SKColor clr] => m_rgBrushes[clr];

        public bool Contains(SKColor clr)
        {
            return m_rgBrushes.ContainsKey(clr);
        }
    }
}
