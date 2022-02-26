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

            for (int i = 0; i < rgX.Count; i++)
            {
                int nIdx = nStartIdx + i;

                if (nIdx < plots.Count)
                {
                    Plot plot = plots[nIdx];
                    float fX = rgX[i];

                    if (plot.Active)
                    {
                        float fOpen = (float)((plot.Y_values.Length == 1) ? plot.Y : plot.Y_values[0]);
                        float fHigh = (float)((plot.Y_values.Length == 1) ? plot.Y : plot.Y_values[1]);
                        float fLow = (float)((plot.Y_values.Length == 1) ? plot.Y : plot.Y_values[2]);
                        float fClose = (float)((plot.Y_values.Length == 1) ? plot.Y : plot.Y_values[3]);

                        bool bPositive = (fClose > fOpen) ? true : false;
                        Color clrFill = (bPositive) ? Color.White : Color.Black;
                        Color clrLine = (bPositive) ? Color.Black : Color.Black;

                        int nClr = clrLine.ToArgb();
                        int nClr1 = (int)m_config.GetExtraSetting("LineColorUp", nClr);
                        if (nClr != nClr1)
                            clrLine = Color.FromArgb(nClr1);

                        nClr = clrFill.ToArgb();
                        string strSetting = (bPositive) ? "UpFillColor" : "DnFillColor";
                        nClr1 = (int)m_config.GetExtraSetting(strSetting, nClr);
                        if (nClr != nClr1)
                            clrFill = Color.FromArgb(nClr1);
                       
                        if (nIdx > 0 && fClose < plots[nIdx - 1].Y)
                        {
                            clrFill = Color.Firebrick;
                            clrLine = Color.Firebrick;

                            nClr = clrFill.ToArgb();
                            nClr1 = (int)m_config.GetExtraSetting("DnPrevFillColor", nClr);
                            if (nClr != nClr1)
                            {
                                clrFill = Color.FromArgb(nClr1);
                                clrLine = clrFill;
                            }

                            nClr = clrLine.ToArgb();
                            nClr1 = (int)m_config.GetExtraSetting("LineColorDn", nClr);
                            if (nClr != nClr1)
                                clrLine = Color.FromArgb(nClr1);
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

                        float frcX = fX1;
                        float frcY = fTop1;
                        float frcW = fWid - 1;
                        float frcH = fHt;

                        if (!m_rgPens.ContainsKey(clrLine))
                            m_rgPens.Add(clrLine, new Pen(clrLine, 1.0f));

                        if (!m_rgBrushes.ContainsKey(clrFill))
                            m_rgBrushes.Add(clrFill, new SolidBrush(clrFill));

                        Pen pLine = m_rgPens[clrLine];
                        Brush brFill = m_rgBrushes[clrFill];

                        float fTop2 = Math.Min(fTop, fBottom);
                        float fBottom2 = Math.Max(fTop, fBottom);

                        if (isValid(frcW, frcH))
                        {
                            g.DrawLine(pLine, fX, fTop2, fX, fBottom2);
                            g.DrawLine(pLine, frcX, frcY, frcX + frcW, frcY);
                            g.FillRectangle(brFill, frcX, frcY, frcW, frcH);
                            g.DrawRectangle(pLine, frcX, frcY, frcW, frcH);
                        }
                    }
                }
            }
        }

        private bool isValid(float frcW, float frcH)
        {
            if (double.IsNaN(frcW) || double.IsInfinity(frcW))
                return false;

            if (double.IsNaN(frcH) || double.IsInfinity(frcH))
                return false;

            return true;
        }
    }
}
