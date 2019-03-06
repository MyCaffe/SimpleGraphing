using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleGraphing;

namespace SimpleGraphingApp
{
    public partial class FormMain : Form
    {
        FormMovingAverages m_dlgMovAve = null;

        public FormMain()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            simpleGraphingControl1.Configuration.SaveToFile("c:\\temp\\foo.xml");
        }

        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>();
            int nCount = 300;

            for (int i = 0; i < 4; i++)
            {
                PlotCollectionSet set = new PlotCollectionSet();
                PlotCollection plots;

                plots = new PlotCollection("plot_1 - " + i.ToString());
                for (int j = 0; j < nCount; j++)
                {
                    plots.Add(new Plot(j, j * Math.Sin(j)));
                }

                set.Add(plots);
                plots = new PlotCollection("plot_2 - " + i.ToString());
                for (int j = 0; j < nCount; j++)
                {
                    plots.Add(new Plot(j, j * Math.Sin(j) * Math.Cos(j)));
                }

                set.Add(plots);
                rgSet.Add(set);
            }

            configureLineCharts();

            simpleGraphingControl1.BuildGraph(rgSet);
            simpleGraphingControl1.Invalidate();
            simpleGraphingControl1.ScrollToEnd();
        }

        private void candleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>();
            int nCount = 300;
            double dfInc = TimeSpan.FromDays(1).TotalMinutes;
            DateTime dtStart = DateTime.Today - TimeSpan.FromDays(nCount);
            double dfTimeStart = dtStart.ToFileTime();
            Random rand = new Random();
            PlotCollection plotsLast = null;

            for (int i = 0; i < 4; i++)
            {
                PlotCollectionSet set = new PlotCollectionSet();
                PlotCollection plots;
                double dfTime = dfTimeStart;
                double dfVal = 10;

                if (plotsLast != null)
                {
                    plots = plotsLast;
                }
                else
                {
                    plots = new PlotCollection("plot_1 - " + i.ToString(), int.MaxValue, dfInc);
                    for (int j = 0; j < nCount; j++)
                    {
                        double dfO = dfVal;
                        double dfC = dfVal + (-1 + (2 * rand.NextDouble()));
                        double dfH = Math.Max(dfO, dfC) + (Math.Abs(dfC - dfO) * rand.NextDouble());
                        double dfL = Math.Min(dfO, dfC) - (Math.Abs(dfC - dfO) * rand.NextDouble());
                        List<double> rgdfVal = new List<double>() { dfO, dfH, dfL, dfC };

                        plots.Add(new Plot(dfTime, rgdfVal));

                        dtStart += TimeSpan.FromDays(1);
                        dfTime = dtStart.ToFileTime();
                        dfVal += -1 + (2 * rand.NextDouble());
                    }
                }

                set.Add(plots);
                rgSet.Add(set);

                if (i % 2 == 0)
                    plotsLast = plots;
                else
                    plotsLast = null;
            }

            configureCandleCharts();

            simpleGraphingControl1.BuildGraph(rgSet);
            simpleGraphingControl1.Invalidate();
            simpleGraphingControl1.ScrollToEnd();
        }

        private void configureCandleCharts()
        {
            simpleGraphingControl1.Configuration.Surface.EnableSmoothing = false;

            for (int i = 0; i < simpleGraphingControl1.Configuration.Frames.Count; i++)
            {
                ConfigurationFrame frame = simpleGraphingControl1.Configuration.Frames[i];

                if (i == 0)
                {
                    frame.Visible = true;
                    frame.Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.CANDLE;
                }
                else if (i == 1)
                {
                    frame.Visible = true;
                    frame.Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.RSI;
                    frame.Plots[0].Interval = 14;
                    frame.Plots[0].LineColor = Color.DarkGreen;
                    frame.Plots[0].LineWidth = 2.0f;
                    frame.Plots[0].PlotFillColor = Color.Transparent;
                    frame.Plots[0].PlotLineColor = Color.Transparent;
                    frame.Plots[1].Visible = false;
                    frame.TargetLines.Add(new ConfigurationTargetLine(30, Color.Maroon));
                    frame.TargetLines.Add(new ConfigurationTargetLine(70, Color.Green));
                    frame.YAxis.InitialMaximum = 100;
                    frame.YAxis.InitialMinimum = 0;
                }
                else
                    frame.Visible = false;

                frame.XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.TIME;
                frame.XAxis.ValueResolution = ConfigurationAxis.VALUE_RESOLUTION.DAY;
                frame.YAxis.Decimals = 2;
            }
        }

        private void configureLineCharts()
        {
            simpleGraphingControl1.Configuration.Surface.EnableSmoothing = true;

            foreach (ConfigurationFrame frame in simpleGraphingControl1.Configuration.Frames)
            {
                frame.Visible = true;
                frame.Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.LINE;
                frame.Plots[0].Interval = 20;
                frame.Plots[0].LineColor = Color.Black;
                frame.Plots[0].LineWidth = 1.0f;
                frame.Plots[0].PlotFillColor = Color.Cyan;
                frame.Plots[0].PlotLineColor = Color.Black;
                frame.Plots[1].Visible = true;
                frame.XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.NUMBER;
                frame.XAxis.ValueResolution = ConfigurationAxis.VALUE_RESOLUTION.NUMBER;
                frame.YAxis.Decimals = 0;
                frame.TargetLines.Clear();
                frame.YAxis.InitialMaximum = 1;
                frame.YAxis.InitialMinimum = 0;
            }
        }

        private void movingAveragesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dlgMovAve == null)
            {
                m_dlgMovAve = new FormMovingAverages();
                m_dlgMovAve.OnChange += m_dlgMovAve_OnChange;
                m_dlgMovAve.OnClosing += m_dlgMovAve_OnClosing;
            }

            m_dlgMovAve.Show(this);
        }

        private void m_dlgMovAve_OnClosing(object sender, EventArgs e)
        {
            m_dlgMovAve = null;
        }

        private void m_dlgMovAve_OnChange(object sender, MovingAverageChangeArgs e)
        {
            simpleGraphingControl1.Configuration.Frames[0].Plots[1].Interval = (uint)e.SMAInterval;
            simpleGraphingControl1.Configuration.Frames[0].Plots[2].Interval = (uint)e.EMAInterval;
            simpleGraphingControl1.UpdateGraph();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }
    }
}