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
        int m_nDataCount = 600;
        FormMovingAverages m_dlgMovAve = null;
        Random m_random = new Random();
        List<PlotCollectionSet> m_rgLastData = new List<PlotCollectionSet>();
        Size m_szMinBounds;
        Size m_szMaxBounds;
        List<PlotCollectionSet> m_rgSet = null;

        public FormMain()
        {
            InitializeComponent();
            List<string> rgstrNames = simpleGraphingControl1.LoadModuleCache();

            foreach (string strName in rgstrNames)
            {
                IGraphPlotDataEx idata = simpleGraphingControl1.CustomModules.Find(strName, false);
                if (idata != null)
                {
                    IGraphPlotUserEdit iedit = idata.CreateUserEdit();
                    ToolStripItem item = testToolStripMenuItem.DropDownItems.Add(iedit.Name + "...");
                    item.Tag = iedit;
                    item.Click += Item_Click;

                    ConfigurationPlot plotConfig = new ConfigurationPlot();
                    plotConfig.PlotType = ConfigurationPlot.PLOTTYPE.CUSTOM;
                    plotConfig.CustomName = idata.Name;
                    simpleGraphingControl1.Configuration.Frames[0].Plots.Add(plotConfig);

                    plotConfig = new ConfigurationPlot();
                    plotConfig.PlotType = ConfigurationPlot.PLOTTYPE.CUSTOM;
                    plotConfig.CustomName = idata.Name;
                    plotConfig.DataIndex = 1;
                    simpleGraphingControl1.Configuration.Frames[1].Plots.Add(plotConfig);
                }
            }
        }

        private void Item_Click(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            if (item == null)
                return;

            IGraphPlotUserEdit iedit = item.Tag as IGraphPlotUserEdit;
            if (iedit == null)
                return;

            iedit.Edit(this, simpleGraphingControl1);
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
            timerData.Enabled = false;
            toolStrip1.Visible = false;

            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>();
            int nCount = m_nDataCount;

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

            updateGraph(rgSet);
        }

        private void candleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>();
            int nCount = m_nDataCount;
            double dfInc = TimeSpan.FromDays(1).TotalMinutes;
            DateTime dtStart = DateTime.Today - TimeSpan.FromDays(nCount);
            double dfTimeStart = dtStart.ToFileTime();
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
                        double dfC = dfVal + (-1 + (2 * m_random.NextDouble()));
                        double dfH = Math.Max(dfO, dfC) + (Math.Abs(dfC - dfO) * m_random.NextDouble());
                        double dfL = Math.Min(dfO, dfC) - (Math.Abs(dfC - dfO) * m_random.NextDouble());
                        List<double> rgdfVal = new List<double>() { dfO, dfH, dfL, dfC };

                        plots.Add(new Plot(dfTime, rgdfVal));

                        dtStart += TimeSpan.FromDays(1);
                        dfTime = dtStart.ToFileTime();
                        dfVal += -1 + (2 * m_random.NextDouble());
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

            updateGraph(rgSet);
        }

        private void updateGraph(List<PlotCollectionSet> rgSet)
        {
            m_rgSet = rgSet;
            simpleGraphingControl1.BuildGraph(rgSet);
            simpleGraphingControl1.Invalidate();
            simpleGraphingControl1.ScrollToEnd();
        }

        private void configureCandleCharts()
        {
            timerData.Enabled = false;
            toolStrip1.Visible = true;

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

                    if (frame.Plots.Count > 1)
                    {
                        frame.Plots[1].PlotType = ConfigurationPlot.PLOTTYPE.HIGHLOW;
                        frame.Plots[1].DataName = "RSI";
                        frame.Plots[1].DataIndex = 1;
                        frame.Plots[1].Visible = true;
                    }

                    if (frame.Plots.Count > 2)
                    {
                        frame.Plots[2].Visible = false;

                        for (int j = 3; j < frame.Plots.Count; j++)
                        {
                            frame.Plots[j].Visible = true;
                        }
                    }

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

                if (frame.Plots.Count > 1)
                {
                    frame.Plots[1].PlotType = ConfigurationPlot.PLOTTYPE.SMA;
                    frame.Plots[1].DataName = null;
                    frame.Plots[1].DataIndex = 0;
                    frame.Plots[1].Visible = true;
                }

                if (frame.Plots.Count > 2)
                {
                    frame.Plots[2].Visible = true;

                    for (int j = 3; j < frame.Plots.Count; j++)
                    {
                        frame.Plots[j].Visible = false;
                    }
                }

                frame.XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.NUMBER;
                frame.XAxis.ValueResolution = ConfigurationAxis.VALUE_RESOLUTION.NUMBER;
                frame.YAxis.Decimals = 0;
                frame.TargetLines.Clear();
                frame.YAxis.InitialMaximum = 1;
                frame.YAxis.InitialMinimum = 0;
            }

            if (simpleGraphingControl1.Configuration.Frames[0].Plots.Count > 4)
                simpleGraphingControl1.Configuration.Frames[0].Plots[4].Visible = false;
        }

        private void movingAveragesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dlgMovAve == null)
            {
                m_dlgMovAve = new FormMovingAverages();
                m_dlgMovAve.OnChange += m_dlgMovAve_OnChange;
                m_dlgMovAve.OnClosingWindow += m_dlgMovAve_OnClosing;
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

        private void houghLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            m_szMinBounds = MinimumSize;
            m_szMaxBounds = MaximumSize;
            m_nDataCount = simpleGraphingControl1.VisiblePlotCount;
        }

        private void timerUI_Tick(object sender, EventArgs e)
        {
            btnRun.Enabled = !timerData.Enabled;
            btnStop.Enabled = timerData.Enabled;
            btnReDraw.Enabled = (m_rgSet != null) ? true : false;
        }

        private void stepPrev()
        {
            PlotCollectionSet lastData = simpleGraphingControl1.GetLastData(true);
            if (lastData == null)
                return;

            m_rgLastData.Add(lastData);
        }

        private void stepNext()
        {
            try
            { 
                if (m_rgLastData.Count > 0)
                {
                    PlotCollectionSet lastData1 = m_rgLastData[m_rgLastData.Count - 1];
                    m_rgLastData.RemoveAt(m_rgLastData.Count - 1);
                    simpleGraphingControl1.AddData(lastData1, true);
                    return;
                }

                PlotCollectionSet newData = new PlotCollectionSet();
                PlotCollectionSet lastData = simpleGraphingControl1.GetLastData();
                if (lastData == null)
                    return;

                for (int i = 0; i < lastData.Count; i++)
                {
                    PlotCollection frameData = lastData[i];
                    PlotCollection frameNewData = new PlotCollection(frameData.Name);

                    for (int j = 0; j < frameData.Count; j++)
                    {
                        double dfTime = frameData[j].X;
                        DateTime dtStart = DateTime.FromFileTime((long)dfTime);
                        dtStart += TimeSpan.FromDays(1);
                        dfTime = dtStart.ToFileTime();

                        double dfVal = frameData[j].Y + (-1 + (2 * m_random.NextDouble()));
                        double dfO = dfVal;
                        double dfC = dfVal + (-1 + (2 * m_random.NextDouble()));
                        double dfH = Math.Max(dfO, dfC) + (Math.Abs(dfC - dfO) * m_random.NextDouble());
                        double dfL = Math.Min(dfO, dfC) - (Math.Abs(dfC - dfO) * m_random.NextDouble());
                        List<double> rgdfVal = new List<double>() { dfO, dfH, dfL, dfC };

                        frameNewData.Add(new Plot(dfTime, rgdfVal));
                    }

                    newData.Add(frameNewData);
                }

                simpleGraphingControl1.AddData(newData, true);
            }
            finally
            {
            }
        }

        private void timerData_Tick(object sender, EventArgs e)
        {
            stepNext();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            MaximumSize = Size;
            MinimumSize = Size;
            btnStepPrev.Enabled = false;
            btnStepNext.Enabled = false;
            timerData.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timerData.Enabled = false;
            MaximumSize = m_szMaxBounds;
            MinimumSize = m_szMinBounds;
            btnStepPrev.Enabled = true;
            btnStepNext.Enabled = true;
        }

        private void btnStepPrev_Click(object sender, EventArgs e)
        {
            MaximumSize = Size;
            MinimumSize = Size;
            stepPrev();
            MaximumSize = m_szMaxBounds;
            MinimumSize = m_szMinBounds;
        }

        private void btnStepNext_Click(object sender, EventArgs e)
        {
            MaximumSize = Size;
            MinimumSize = Size;
            stepNext();
            MaximumSize = m_szMaxBounds;
            MinimumSize = m_szMinBounds;
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            m_nDataCount = simpleGraphingControl1.VisiblePlotCount;
        }

        private void btnReDraw_Click(object sender, EventArgs e)
        {
            if (m_rgSet != null)
                updateGraph(m_rgSet);
        }
    }
}