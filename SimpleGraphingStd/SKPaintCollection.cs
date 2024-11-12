using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SimpleGraphingStd
{
    public class SKPaintCollection : IDisposable
    {
        private Dictionary<SKColor, SKPaint> paintCollection = new Dictionary<SKColor, SKPaint>();

        public SKPaint this[SKColor color]
        {
            get
            {
                if (!paintCollection.ContainsKey(color))
                {
                    // Create a new SKPaint if it doesn't exist in the collection
                    paintCollection[color] = new SKPaint
                    {
                        Color = color,
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill // Default to fill; can be modified based on usage
                    };
                }
                return paintCollection[color];
            }
        }

        // Optionally, add a method to specify a paint style directly.
        public SKPaint GetPaint(SKColor color, SKPaintStyle style = SKPaintStyle.Fill)
        {
            if (!paintCollection.ContainsKey(color))
            {
                paintCollection[color] = new SKPaint
                {
                    Color = color,
                    IsAntialias = true,
                    Style = style
                };
            }
            else
            {
                paintCollection[color].Style = style;
            }
            return paintCollection[color];
        }

        public void Dispose()
        {
            foreach (var paint in paintCollection.Values)
            {
                paint.Dispose();
            }
            paintCollection.Clear();
        }
    }
}

