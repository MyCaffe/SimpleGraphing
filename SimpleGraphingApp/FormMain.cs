﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleGraphing;
using SimpleGraphingDebug;

namespace SimpleGraphingApp
{
    public partial class FormMain : Form
    {
        bool m_bFullData = false;
        int m_nDataCount = 600;
        FormSpecialSettings m_dlgSpecialSettings = null;
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
                    if (iedit != null)
                    {
                        ToolStripItem item = testToolStripMenuItem.DropDownItems.Add(iedit.Name + "...");
                        item.Tag = iedit;
                        item.Click += Item_Click;
                    }

                    ConfigurationPlot plotConfig = new ConfigurationPlot(Guid.NewGuid());
                    plotConfig.PlotType = ConfigurationPlot.PLOTTYPE.CUSTOM;
                    plotConfig.CustomName = idata.Name;
                    plotConfig.SetCustomBuildOrder(idata.BuildOrder);

                    simpleGraphingControl1.Configuration.Frames[0].Plots.Add(plotConfig);

                    plotConfig = new ConfigurationPlot(Guid.NewGuid());
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

        private void candleWithFullDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bFullData = true;
            m_nDataCount = 600;
            candle(sender);
        }

        private void candleVisibleOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bFullData = false;
            m_nDataCount = simpleGraphingControl1.VisiblePlotCount;
            candle(sender);
        }

        private void candleClipToVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bFullData = true;
            m_nDataCount = 600;
            candle(sender, 100);
        }

        private void candle(object sender, int nClipToVisible = 0)
        {
            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>();
            int nCount = m_nDataCount;
            double dfInc = TimeSpan.FromDays(1).TotalMinutes;
            DateTime dtStart = DateTime.Today - TimeSpan.FromDays(nCount);
            double dfTimeStart = dtStart.ToFileTime();
            PlotCollection plotsLast = null;
            bool bEnableOverlay = false;

            if (sender == candleWithOverlayToolStripMenuItem)
                bEnableOverlay = true;

            for (int i = 0; i < 4; i++)
            {
                PlotCollectionSet set = new PlotCollectionSet();
                PlotCollection plots;
                double dfTime = dfTimeStart;
                double dfVal = 100;

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
                        long lVol = m_random.Next(10000);

                        Plot p = new Plot(dfTime, rgdfVal, lVol);

                        if (bEnableOverlay)
                            p.SetParameter("cos", (float)Math.Cos(j));

                        plots.Add(p);

                        dtStart += TimeSpan.FromMinutes(1);
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

            double dfSlope;
            double dfConfWid;
            PlotCollection colReg = rgSet[0][0].CalculateLinearRegressionLines(out dfSlope, out dfConfWid);
            rgSet[0].Add(colReg);

            int? nCount1 = null;
            if (nClipToVisible > 0)
                nCount1 = simpleGraphingControl1.VisiblePlotCount + nClipToVisible;

            foreach (PlotCollectionSet set1 in rgSet)
            {
                set1.SetStartOffsetFromEnd(nCount1);
            }

            configureCandleCharts(bEnableOverlay, rgSet, false);

            updateGraph(rgSet);
        }

        private void candleFromExternalDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogBin.ShowDialog() != DialogResult.OK)
                return;

            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>();

            if (Path.GetExtension(openFileDialogBin.FileName).ToLower() == ".bin")
            {
                using (FileStream fs = File.OpenRead(openFileDialogBin.FileName))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] rgBytes = br.ReadBytes((int)fs.Length);
                    rgSet = PlotCollectionSet.LoadList(rgBytes);
                }
            }
            else
            {
                PlotCollection colPlots = new PlotCollection("Prices");

                using (StreamReader sr = new StreamReader(openFileDialogBin.FileName))
                {
                    string strLine = sr.ReadLine();
                    strLine = sr.ReadLine();

                    while (strLine != null)
                    {
                        string[] rgstr = strLine.Split(',');

                        int nIdx = 0;
                        DateTime dt;
                        if (!DateTime.TryParse(rgstr[nIdx], out dt))
                        {
                            nIdx++;
                            dt = DateTime.Parse(rgstr[nIdx]);
                        }

                        nIdx++;
                        float fOpen = float.Parse(rgstr[nIdx]);
                        nIdx++;
                        float fHigh = float.Parse(rgstr[nIdx]);
                        nIdx++;
                        float fLow = float.Parse(rgstr[nIdx]);
                        nIdx++;
                        float fClose = float.Parse(rgstr[nIdx]);
                        nIdx++;
                        long lVol = long.Parse(rgstr[nIdx]);

                        colPlots.Add(dt.ToFileTime(), new float[] { fOpen, fHigh, fLow, fClose });
                        colPlots[colPlots.Count - 1].Tag = dt;
                        colPlots[colPlots.Count - 1].Count = lVol;

                        strLine = sr.ReadLine();
                    }
                }

                PlotCollectionSet set = new PlotCollectionSet();
                set.Add(colPlots);
                rgSet.Add(set);
            }

            while (rgSet.Count < 4)
            {
                rgSet.Add(rgSet[0].Clone());
            }

            configureCandleCharts(false, rgSet, true);

            updateGraph(rgSet);
        }

        private void updateGraph(List<PlotCollectionSet> rgSet)
        {
            m_rgSet = rgSet;
            simpleGraphingControl1.BuildGraph(rgSet, true, true);
            simpleGraphingControl1.Invalidate();
            simpleGraphingControl1.ScrollToEnd(true);
        }

        private void configureCandleCharts(bool bEnableOverlay, List<PlotCollectionSet> rgSet, bool bLoadedFromFile)
        {
            timerData.Enabled = false;
            toolStrip1.Visible = true;

            simpleGraphingControl1.Configuration.Surface.EnableSmoothing = false;
            simpleGraphingControl1.Configuration.Frames[0].EnableRelativeScaling(true, btnScaleToVisible.Checked);

            if (!bLoadedFromFile)
            {
                int nIdx1 = 30;
                int nIdx2 = 60;

                if (nIdx1 > rgSet[0][0].Count)
                    nIdx1 = rgSet[0][0].Count - 1;

                if (nIdx2 > rgSet[0][0].Count)
                    nIdx2 = rgSet[0][0].Count - 1;

                DateTime dt0 = DateTime.FromFileTime((long)rgSet[0][0][nIdx1].X);
                DateTime dt1 = DateTime.FromFileTime((long)rgSet[0][0][nIdx2].X);

                simpleGraphingControl1.Configuration.Frames[0].PlotArea.TimeZones = new List<ConfigurationTimeZone>();
                simpleGraphingControl1.Configuration.Frames[0].PlotArea.TimeZones.Add(new ConfigurationTimeZone(dt0, dt1, Color.LightGray, true));
            }
            else
            {
                simpleGraphingControl1.Configuration.Frames[0].MinimumYRange = 5;
            }

            for (int i = 0; i < simpleGraphingControl1.Configuration.Frames.Count; i++)
            {
                ConfigurationFrame frame = simpleGraphingControl1.Configuration.Frames[i];

                if (i == 0)
                {                    
                    frame.Visible = true;
                    frame.Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.CANDLE;

                    for (int j = 2; j < frame.Plots.Count; j++)
                    {
                        if (frame.Plots[j].PlotType == ConfigurationPlot.PLOTTYPE.CUSTOM)
                        {
                            frame.Plots[j].Visible = false;
                        }
                        else if (frame.Plots[j].PlotType == ConfigurationPlot.PLOTTYPE.HIGHLOW)
                        {
                            if (frame.Plots[j].ExtraSettings == null)
                                frame.Plots[j].ExtraSettings = new Dictionary<string, double>();
                            if (!frame.Plots[j].ExtraSettings.ContainsKey("DrawLines"))
                                frame.Plots[j].ExtraSettings.Add("DrawLines", 1.0);
                        }
                        else if (frame.Plots[j].PlotType == ConfigurationPlot.PLOTTYPE.LINE)
                        {
                            if (frame.Plots[j].Name == "Regression" ||
                                frame.Plots[j].Name == "Conf+" ||
                                frame.Plots[j].Name == "Conf-")
                                frame.Plots[j].Visible = true;
                        }

                        //if (frame.Plots[j].PlotType != ConfigurationPlot.PLOTTYPE.BOLLINGERBANDS)
                        //    frame.Plots[j].Visible = false;
                    }

                    frame.Plots[4].Visible = bEnableOverlay;
                    frame.Plots[5].Visible = true;
                }
                else if (i == 1)
                {
                    frame.Visible = true;
                    frame.Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.LRSI;
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

                    for (int j = 2; j < frame.Plots.Count; j++)
                    {
                        if (frame.Plots[j].PlotType == ConfigurationPlot.PLOTTYPE.CUSTOM)
                            frame.Plots[j].Visible = (frame.Plots[0].PlotType == ConfigurationPlot.PLOTTYPE.LRSI) ? false : true;
                        else
                            frame.Plots[j].Visible = false;
                    }

                    frame.TargetLines.Add(new ConfigurationTargetLine(30, Color.Maroon));
                    frame.TargetLines.Add(new ConfigurationTargetLine(70, Color.Green));
                    frame.YAxis.InitialMaximum = 100;
                    frame.YAxis.InitialMinimum = 0;
                }
                else if (i == 2)
                {
                    frame.Visible = true;
                    frame.Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.VOLUME;
                    frame.Plots[0].LineColor = Color.Blue;
                    frame.Plots[0].LineWidth = 1.0f;
                    frame.Plots[0].PlotFillColor = Color.Transparent;
                    frame.Plots[0].PlotLineColor = Color.Transparent;
                    frame.MinMaxTarget = PlotCollection.MINMAX_TARGET.COUNT;
                }
                else
                {
                    frame.Visible = false;
                }

                frame.XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.TIME;
                frame.XAxis.ValueResolution = ConfigurationAxis.VALUE_RESOLUTION.DAY;
                frame.YAxis.Decimals = 2;
            }

            simpleGraphingControl1.SetLookahead(3);
        }

        private void configureLineCharts()
        {
            simpleGraphingControl1.Configuration.Surface.EnableSmoothing = true;
            simpleGraphingControl1.Configuration.Frames[0].EnableRelativeScaling(false, false);

            for (int i=0; i<simpleGraphingControl1.Configuration.Frames.Count; i++)
            {
                ConfigurationFrame frame = simpleGraphingControl1.Configuration.Frames[i];

                frame.Visible = true;
                frame.Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.LINE;
                frame.Plots[0].Interval = 20;
                frame.Plots[0].LineColor = Color.Black;
                frame.Plots[0].LineWidth = 1.0f;
                frame.Plots[0].PlotFillColor = Color.Cyan;
                frame.Plots[0].PlotLineColor = Color.Black;

                if (i == 0)
                    frame.Plots[5].Visible = false;

                if (frame.Plots.Count > 1)
                {
                    frame.Plots[1].PlotType = ConfigurationPlot.PLOTTYPE.SMA;
                    frame.Plots[1].DataName = null;
                    frame.Plots[1].DataIndex = 0;
                    frame.Plots[1].Visible = true;
                }

                for (int j = 2; j < frame.Plots.Count; j++)
                {
                    if (frame.Plots[j].PlotType == ConfigurationPlot.PLOTTYPE.CUSTOM)
                        frame.Plots[j].Visible = false;
                    else
                        frame.Plots[j].Visible = true;
                }

                frame.XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.NUMBER;
                frame.XAxis.ValueResolution = ConfigurationAxis.VALUE_RESOLUTION.DAY;
                frame.YAxis.Decimals = 0;
                frame.TargetLines.Clear();
                frame.YAxis.InitialMaximum = 1;
                frame.YAxis.InitialMinimum = 0;
            }

            if (simpleGraphingControl1.Configuration.Frames[0].Plots.Count > 4)
                simpleGraphingControl1.Configuration.Frames[0].Plots[4].Visible = false;
        }

        private void specialSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dlgSpecialSettings == null)
            {
                m_dlgSpecialSettings = new FormSpecialSettings();
                m_dlgSpecialSettings.OnChange += m_dlgSpecialSettings_OnChange;
                m_dlgSpecialSettings.OnClosingWindow += m_dlgMovAve_OnClosing;
            }

            m_dlgSpecialSettings.Show(this);
        }

        private void m_dlgMovAve_OnClosing(object sender, EventArgs e)
        {
            m_dlgSpecialSettings = null;
        }

        private void m_dlgSpecialSettings_OnChange(object sender, SpecialSettingChangeArgs e)
        {
            simpleGraphingControl1.Configuration.Frames[0].Plots[1].Interval = (uint)e.SMAInterval;
            simpleGraphingControl1.Configuration.Frames[0].Plots[2].Interval = (uint)e.EMAInterval;
            simpleGraphingControl1.Configuration.Frames[0].Plots[11].Interval = (uint)e.HMAInterval;
            simpleGraphingControl1.Configuration.Frames[0].Plots[10].SetExtraSetting("MA", e.BBMa);                
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

            if (Properties.Settings.Default.Width > 0 && Properties.Settings.Default.Height > 0)
                SetBounds(0, 0, (int)Properties.Settings.Default.Width, (int)Properties.Settings.Default.Height);
        }

        private void timerUI_Tick(object sender, EventArgs e)
        {
            btnRun.Enabled = !timerData.Enabled;
            btnStop.Enabled = timerData.Enabled;
            btnReDraw.Enabled = (m_rgSet != null) ? true : false;
        }

        private void stepPrev()
        {
            PlotCollectionSet lastData = simpleGraphingControl1.GetLastData(0, true);
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

                        Plot p = new Plot(dfTime, rgdfVal);
                        p.Action1Active = enableActionStripMenuItem.Checked;
                        p.Action2Active = enableActionStripMenuItem.Checked;

                        frameNewData.Add(p);
                    }

                    newData.Add(frameNewData);
                }

                simpleGraphingControl1.AddData(newData, true, true);
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
            if (m_nDataCount < simpleGraphingControl1.VisiblePlotCount || !m_bFullData)
            {
                m_nDataCount = simpleGraphingControl1.VisiblePlotCount;
                Trace.WriteLine("Visible Plot Count: " + m_nDataCount.ToString());
            }
        }

        private void btnReDraw_Click(object sender, EventArgs e)
        {
            if (m_rgSet != null)
                updateGraph(m_rgSet);
        }

        private void simpleGraphingControl1_Load(object sender, EventArgs e)
        {

        }

        private void enableActionStripMenuItem_Click(object sender, EventArgs e)
        {
            enableActionStripMenuItem.Checked = !enableActionStripMenuItem.Checked;
        }

        private void btnCrossHairs_Click(object sender, EventArgs e)
        {
            simpleGraphingControl1.EnableCrossHairs = btnCrossHairs.Checked;
            simpleGraphingControl1.Invalidate();
        }

        private void testPlotCollectionVisualizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_rgSet == null)
            {
                MessageBox.Show("You must first create the line or candle data.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            PlotCollectionVisualizer.TestShowVisualizer(m_rgSet[0][0]);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Width = (uint)Width;
            Properties.Settings.Default.Height = (uint)Height;
            Properties.Settings.Default.Save();
        }
    }
}