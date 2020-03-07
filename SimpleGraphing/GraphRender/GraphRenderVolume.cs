using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderVolume : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderVolume(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name
        {
            get { return "VOLUME"; }
        }

        public void RenderActions(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            renderActions(g, dataset, nLookahead);
        }

        public void PreRender(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
        }

        public void Render(Graphics g, PlotCollectionSet dataset, int nLookahead)
        {
            PlotCollection plots = dataset[m_config.DataIndexOnRender];
            List<int> rgX = m_gx.TickPositions;
            int nStartIdx = m_gx.StartPosition;
            Dictionary<Color, Pen> rgPens = new Dictionary<Color, Pen>();
            Dictionary<Color, Brush> rgBrushes = new Dictionary<Color, Brush>();

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nIdx];
                    float fX = rgX[i];

                    if (plot.Active && plot.Count.HasValue)
                    {
                        float fVol = plot.Count.Value;
                        float fOpen = (float)plot.Y_values[0];
                        float fClose = (float)plot.Y_values[3];
                        bool bPositive = (fClose > fOpen) ? true : false;
                        Color clrFill = (bPositive) ? Color.FromArgb(128, Color.Green) : Color.FromArgb(128, Color.Maroon);
                        Color clrLine = Color.FromArgb(128, Color.Black);

                        float fHspace = m_gx.Configuration.PlotSpacing / 2;
                        float fX1 = fX - fHspace;
                        float fX2 = fX + fHspace;
                        float fWid = m_gx.Configuration.PlotSpacing;
                        float fTop = m_gy.ScaleValue(fVol, true);
                        float fBottom = m_gy.ScaleValue(0.0, true);
                        float fHt = Math.Abs(fBottom - fTop);

                        RectangleF rc = new RectangleF(fX1, fTop, fWid, fHt);

                        if (!rgPens.ContainsKey(clrLine))
                            rgPens.Add(clrLine, new Pen(clrLine, 1.0f));

                        if (!rgBrushes.ContainsKey(clrFill))
                            rgBrushes.Add(clrFill, new SolidBrush(clrFill));

                        Pen pLine = rgPens[clrLine];
                        Brush brFill = rgBrushes[clrFill];

                        g.FillRectangle(brFill, rc);
                        g.DrawRectangle(pLine, rc.X, rc.Y, rc.Width, rc.Height);
                    }
                }
            }

            foreach (KeyValuePair<Color, Pen> kv in rgPens)
            {
                kv.Value.Dispose();
            }

            foreach (KeyValuePair<Color, Brush> kv in rgBrushes)
            {
                kv.Value.Dispose();
            }
        }
    }
}
