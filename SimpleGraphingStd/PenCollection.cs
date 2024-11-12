using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SimpleGraphingStd
{
    public class PenCollection : IDisposable
    {
        private Dictionary<SKColor, SKPaint> m_rgPens = new Dictionary<SKColor, SKPaint>();

        public PenCollection()
        {
        }

        public void Dispose()
        {
            foreach (KeyValuePair<SKColor, SKPaint> kv in m_rgPens)
            {
                kv.Value.Dispose();
            }

            m_rgPens.Clear();
        }

        public SKPaint Add(SKColor color)
        {
            if (!m_rgPens.ContainsKey(color))
            {
                var paint = new SKPaint
                {
                    Color = color,
                    StrokeWidth = 1.0f,
                    Style = SKPaintStyle.Stroke
                };
                m_rgPens.Add(color, paint);
            }

            return m_rgPens[color];
        }

        public SKPaint this[SKColor color]
        {
            get { return m_rgPens[color]; }
        }
    }
}
