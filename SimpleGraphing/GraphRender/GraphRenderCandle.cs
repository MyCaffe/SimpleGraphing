using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing.GraphRender
{
    public class GraphRenderCandle : GraphRenderBase, IGraphPlotRender
    {
        public GraphRenderCandle(ConfigurationPlot config, GraphAxis gx, GraphAxis gy, GraphPlotStyle style)
            : base(config, gx, gy, style)
        {
        }

        public string Name
        {
            get { return "CANDLE"; }
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

                    if (plot.Active)
                    {
                        float fOpen = (float)((plot.Y_values.Count == 1) ? plot.Y : plot.Y_values[0]);
                        float fHigh = (float)((plot.Y_values.Count == 1) ? plot.Y : plot.Y_values[1]);
                        float fLow = (float)((plot.Y_values.Count == 1) ? plot.Y : plot.Y_values[2]);
                        float fClose = (float)((plot.Y_values.Count == 1) ? plot.Y : plot.Y_values[3]);

                        bool bPositive = (fClose > fOpen) ? true : false;
                        Color clrFill = (bPositive) ? Color.White : Color.Black;
                        Color clrLine = (bPositive) ? Color.Black : Color.Black;

                        if (nIdx > 0 && fClose < plots[nIdx - 1].Y)
                        {
                            clrFill = Color.Firebrick;
                            clrLine = Color.Firebrick;
                        }

                        float fHspace = m_gx.Configuration.PlotSpacing / 2;
                        float fX1 = fX - fHspace;
                        float fX2 = fX + fHspace;
                        float fWid = m_gx.Configuration.PlotSpacing;
                        float fTop = m_gy.ScaleValue(fHigh, true);
                        float fBottom = m_gy.ScaleValue(fLow, true);
                        float fOpen1 = m_gy.ScaleValue(fOpen, true);
                        float fClose1 = m_gy.ScaleValue(fClose, true);
                        float fTop1 = Math.Min(fOpen1, fClose1);
                        float fBottom1 = Math.Max(fOpen1, fClose1);
                        float fHt = Math.Abs(fBottom1 - fTop1);

                        RectangleF rc = new RectangleF(fX1, fTop1, fWid - 1, fHt);

                        if (!rgPens.ContainsKey(clrLine))
                            rgPens.Add(clrLine, new Pen(clrLine, 1.0f));

                        if (!rgBrushes.ContainsKey(clrFill))
                            rgBrushes.Add(clrFill, new SolidBrush(clrFill));

                        Pen pLine = rgPens[clrLine];
                        Brush brFill = rgBrushes[clrFill];

                        float fTop2 = Math.Min(fTop, fBottom);
                        float fBottom2 = Math.Max(fTop, fBottom);

                        if (isValid(rc))
                        {
                            g.DrawLine(pLine, fX, fTop2, fX, fBottom2);
                            g.DrawLine(pLine, rc.Left, rc.Top, rc.Right, rc.Top);
                            g.FillRectangle(brFill, rc);
                            g.DrawRectangle(pLine, rc.X, rc.Y, rc.Width, rc.Height);
                        }
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

        private bool isValid(RectangleF rc)
        {
            if (double.IsNaN(rc.Width) || double.IsInfinity(rc.Width))
                return false;

            if (double.IsNaN(rc.Height) || double.IsInfinity(rc.Width))
                return false;

            return true;
        }
    }
}
