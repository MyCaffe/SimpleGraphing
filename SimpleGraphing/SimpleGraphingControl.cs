using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SimpleGraphing
{
    public partial class SimpleGraphingControl : UserControl
    {
        ModuleCache m_cache;
        List<PlotCollectionSet> m_data;
        List<PlotCollectionSet> m_output;
        Configuration m_config = new Configuration();
        GraphSurface m_surface;
        bool m_bScrolling = false;
        Size m_szOriginal;
        Crosshairs m_crosshairs = new Crosshairs();
        double m_dfScrollPct = 0;
        bool m_bResizing = false;
        bool m_bUpdating = false;

        public event EventHandler<PaintEventArgs> OnUserPaint;
        public event EventHandler<PaintEventExArgs> OnUserPaintEx;
        public event EventHandler<MouseEventArgs> OnUserMouseMove;
        public event EventHandler<MouseEventArgs> OnUserMouseDown;
        public event EventHandler<MouseEventArgs> OnUserMouseUp;
        public event EventHandler<ScrollEventArgs> OnUserScroll;
        public event EventHandler<MouseEventArgs> OnUserMouseClick;
        public event EventHandler<MouseEventArgs> OnUserMouseDoubleClick;

        public SimpleGraphingControl()
        {
            InitializeComponent();
            m_cache = new ModuleCache();
            m_surface = new GraphSurface(m_cache);
            m_output = m_surface.BuildGraph(m_config, null);            
        }

        public List<PlotCollectionSet> Data
        {
            get { return m_data; }
        }

        public GraphSurface Surface
        {
            get { return m_surface; }
        }

        public void SaveConfiguration(string strFile)
        {
            string strExt = Path.GetExtension(strFile).ToLower();

            if (strExt == ".xml")
            {
                Configuration.SaveToFile(strFile);
            }
            else
            {
                IFormatter formatter = new BinaryFormatter();

                using (Stream strm = new FileStream(strFile, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(strm, Configuration);
                }
            }
        }

        public void LoadConfiguration(string strFile)
        {
            if (string.IsNullOrEmpty(strFile))
                throw new Exception("No configuration file specified!");

            if (!File.Exists(strFile))
                throw new Exception("Could not find the configuration file '" + strFile + "'!");

            string strExt = Path.GetExtension(strFile).ToLower();
            bool bLoaded = false;

            if (strExt == ".cfg")
            {
                try
                {
                    IFormatter formatter = new BinaryFormatter();

                    using (Stream strm = new FileStream(strFile, FileMode.Open, FileAccess.Read))
                    {
                        Configuration = formatter.Deserialize(strm) as Configuration;
                    }

                    bLoaded = true;
                }
                catch
                {
                }
            }

            if (!bLoaded)
            {
                if (strExt != ".xml")
                    strFile = Path.GetDirectoryName(strFile) + "\\" + Path.GetFileNameWithoutExtension(strFile) + ".xml";

                Configuration = Configuration.LoadFromFile(strFile);
            }
        }

        public void SetLookahead(int nLookahead)
        {
            foreach (ConfigurationFrame frame in m_config.Frames)
            {
                frame.PlotArea.Lookahead = nLookahead;
            }
        }

        public bool UserUpdateCrosshairs
        {
            get { return m_crosshairs.UserUpdate; }
            set { m_crosshairs.UserUpdate = value; }
        }

        public bool EnableCrossHairs
        {
            get { return m_crosshairs.EnableCrosshairs; }
            set { m_crosshairs.EnableCrosshairs = value; }
        }

        public bool EnableCrossHairXTickSnap
        {
            get { return m_crosshairs.EnableSnapToXTicks; }
            set { m_crosshairs.EnableSnapToXTicks = value; }
        }

        public Bitmap Image
        {
            get { return new Bitmap(pbImage.Image); }
        }

        public List<string> LoadModuleCache()
        {
            return m_cache.Load();
        }

        public ModuleCache CustomModules
        {
            get { return m_cache; }
        }

        public Configuration Configuration
        {
            get { return m_config; }
            set { m_config = value; }
        }

        public int VisiblePlotCount
        {
            get
            {
                if (m_config.Frames.Count == 0)
                    return 0;

                return m_surface.Bounds.Width / m_config.Frames[0].XAxis.PlotSpacing;
            }
        }

        public PlotCollectionSet GetLastData(int nLookahead = 0, bool bRemove = false)
        {
            if (m_data.Count == 0)
                return null;

            if (nLookahead > 0 && bRemove)
                throw new Exception("Removing data is not supported when retrieving data with a lookahead.");

            PlotCollectionSet lastData = new PlotCollectionSet();
            List<PlotCollection> rgPlots = new List<PlotCollection>();

            for (int i = 0; i < m_data.Count; i++)
            {
                PlotCollectionSet dataFrame = m_data[i];

                if (dataFrame.Count == 0)
                    return null;

                PlotCollection plots = new PlotCollection("Frame " + i.ToString());

                for (int j = 0; j < dataFrame.Count; j++)
                {
                    PlotCollection framePlots = dataFrame[j];
                    if (framePlots.Count == 0)
                        return null;

                    Plot last = framePlots[framePlots.Count - (1 + nLookahead)];
                    if (last.Name == null)
                        last.Name = framePlots.Name;

                    plots.Add(last);

                    if (bRemove)
                        framePlots.RemoveAt(framePlots.Count - 1);
                }

                lastData.Add(plots);
            }

            if (bRemove)
            {
                m_output = m_surface.BuildGraph(m_config, m_data);
                SimpleGraphingControl_Resize(this, EventArgs.Empty);
                ScrollToEnd(false);
            }

            return lastData;
        }

        public List<PlotCollectionSet> GetLastOutput(int nSequenceLength = 1)
        {
            List<PlotCollectionSet> rgOutput = new List<PlotCollectionSet>();

            if (m_output == null || m_output.Count == 0)
                return rgOutput;

            int nCount = m_output[0][0].Count;

            int nStart = nCount - nSequenceLength;
            if (nStart < 0)
            {
                nStart = 0;
                nSequenceLength = nCount;
            }

            for (int k = nStart; k < nStart + nSequenceLength; k++)
            {
                PlotCollectionSet lastData = new PlotCollectionSet();
                List<PlotCollection> rgPlots = new List<PlotCollection>();

                for (int i = 0; i < m_output.Count; i++)
                {
                    PlotCollectionSet dataFrame = m_output[i];

                    if (dataFrame.Count > 0)
                    {
                        PlotCollection plots = new PlotCollection("Frame " + i.ToString());

                        for (int j = 0; j < dataFrame.Count; j++)
                        {
                            PlotCollection framePlots = dataFrame[j];
                            if (framePlots.Count == nCount)
                            {
                                Plot last = framePlots[k];
                                last.Name = framePlots.Name;
                                plots.Add(last);
                            }
                        }

                        lastData.Add(plots);
                    }
                }

                rgOutput.Add(lastData);
            }

            return rgOutput;
        }

        public void AddData(PlotCollectionSet data, bool bMaintainCount, bool bRender = false)
        {
            if (data.Count != m_data.Count)
                throw new Exception("The number of plot collections must match the number of plot sets used by the graph.");

            List<string> rgUpdated = new List<string>();

            for (int i = 0; i < data.Count; i++)
            {
                PlotCollectionSet dataFrame = m_data[i];

                if (rgUpdated.Contains(dataFrame[0].Name))
                    continue;

                PlotCollection dataToAdd = data[i];

                if (dataFrame.Count != dataToAdd.Count)
                    throw new Exception("The number of data items to add must match the number of plot collections in the frame!");

                for (int j = 0; j < dataFrame.Count; j++)
                {
                    PlotCollection dataFrameItems = dataFrame[j];
                    Plot plot = dataToAdd[j];

                    dataFrameItems.Add(plot);

                    if (bMaintainCount)
                        dataFrameItems.RemoveAt(0);
                }

                rgUpdated.Add(dataFrame[0].Name);
            }

            m_output = m_surface.BuildGraph(m_config, m_data);
            SimpleGraphingControl_Resize(this, EventArgs.Empty);
            ScrollToEnd(bRender);
        }

        public void ClearGraph()
        {
            if (m_data == null)
                return;

            foreach (PlotCollectionSet set in m_data)
            {
                set.ClearData();
            }
        }

        public List<PlotCollectionSet> BuildGraph(List<PlotCollectionSet> data = null, bool bResize = true, bool bAddToParams = false)
        {
            if (data != null)
                m_data = data;

            m_output = m_surface.BuildGraph(m_config, m_data, bAddToParams);

            if (bResize)
            {
                SimpleGraphingControl_Resize(this, EventArgs.Empty);
                List<PlotCollectionSet> output = m_surface.BuildGraphPost(m_config, data);
                int nIdx = 0;

                for (int i = 0; i < m_output.Count; i++)
                {
                    if (m_output[i] != null && nIdx < output.Count)
                    {
                        for (int j = 0; j < output[nIdx].Count; j++)
                        {
                            string strName = output[nIdx][j].Name;
                            bool bFound = false;

                            for (int k = 0; k < m_output[i].Count; k++)
                            {
                                if (m_output[i][k] != null)
                                {
                                    if (m_output[i][k].Name == strName)
                                    {
                                        bFound = true;
                                        break;
                                    }
                                }
                            }

                            if (!bFound && output[nIdx][j] != null)
                                m_output[i].Add(output[nIdx][j]);
                        }

                        nIdx++;
                    }
                }
            }

            if (m_surface.Frames.Count > 0)
            {
                m_crosshairs.SetAxes(m_surface.Frames[0].XAxis, m_surface.Frames[0].YAxis);
            }

            return m_output;
        }

        public bool ShowScrollBar
        {
            get { return hScrollBar1.Visible; }
            set
            {
                hScrollBar1.Visible = value;

                if (!value)
                    pbImage.Size = Size;
                else
                    pbImage.Size = m_szOriginal;

                m_surface.Resize(pbImage.Width, pbImage.Height);
            }
        }

        public void UpdateGraph(bool bBuildGraph = true, bool bRefresh = false)
        {
            if (m_bResizing)
                return;

            if (m_bUpdating)
                return;

            try
            {
                m_bUpdating = true;

                if (bBuildGraph)
                    m_output = m_surface.BuildGraph(m_config, m_data);
                SimpleGraphingControl_Resize(this, EventArgs.Empty);
                Invalidate(true);

                if (bRefresh)
                    Refresh();
            }
            finally
            {
                m_bUpdating = false;
            }
        }

        private void SimpleGraphingControl_Resize(object sender, EventArgs e)
        {
            if (DesignMode || m_surface == null || m_bResizing)
                return;

            try
            {
                m_bResizing = true;
                m_surface.Resize(pbImage.Width, pbImage.Height);
                m_szOriginal = pbImage.Size;
            }
            finally
            {
                m_bResizing = false;
            }
        }

        private void SimpleGraphingControl_Paint(object sender, PaintEventArgs e)
        {
            if (DesignMode)
                return;

            //if (m_crosshairs != null)
            //{
            //    int nMargin = 0;
            //    if (m_surface.Frames.Count > 0)
            //        nMargin = m_surface.Frames[0].YAxis.Bounds.Width;

            //    m_crosshairs.HandlePaint(e, pbImage.Image, nMargin + 5);
            //}

            pbImage.Image = m_surface.Render();

            if (OnUserPaintEx != null)
                OnUserPaintEx(sender, new PaintEventExArgs(e, pbImage.Image));
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (DesignMode)
                return;

            if (m_bScrolling)
                return;

            try
            {
                m_bScrolling = true;

                m_dfScrollPct = e.NewValue / (double)(hScrollBar1.Maximum - (hScrollBar1.LargeChange - 1));

                m_surface.Scroll(m_dfScrollPct);
                m_surface.Resize(pbImage.Width, pbImage.Height);

                pbImage.Image = m_surface.Render();

                if (OnUserScroll != null)
                    OnUserScroll(sender, e);
            }
            finally
            {
                m_bScrolling = false;
            }
        }

        public double ScrollPercent
        {
            get { return m_dfScrollPct; }
            set
            {
                m_dfScrollPct = value;
                m_surface.Scroll(m_dfScrollPct);
                m_surface.Resize(pbImage.Width, pbImage.Height);
                pbImage.Image = m_surface.Render();
            }
        }

        public Image ScrollToEnd(bool bRender)
        {
            hScrollBar1.Value = hScrollBar1.Maximum;
            m_surface.Scroll(1.0);
            m_surface.Resize(pbImage.Width, pbImage.Height);

            if (bRender)
            {
                pbImage.Image = m_surface.Render();
                return pbImage.Image;
            }

            return null;
        }

        public Image Render(int nWidth = 0, int nHeight = 0)
        {
            if (nWidth == 0)
                nWidth = pbImage.Width;

            if (nHeight == 0)
                nHeight = pbImage.Height;

            m_surface.Scroll(1.0);
            m_surface.Resize(nWidth, nHeight, true);
            return m_surface.Render();
        }

        private void pbImage_Paint(object sender, PaintEventArgs e)
        {
            if (DesignMode)
                return;

            if (m_crosshairs != null)
            {
                int nMargin = 0;
                if (m_surface.Frames.Count > 0)
                    nMargin = m_surface.Frames[0].YAxis.Bounds.Width;

                m_crosshairs.CrossHairColor = m_config.Surface.CrossHairColor;
                m_crosshairs.HandlePaint(e, pbImage.Image, nMargin + 5);
            }

            if (OnUserPaint != null)
                OnUserPaint(sender, e);
        }

        private void pbImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (DesignMode)
                return;

            if (m_crosshairs != null)
                m_crosshairs.HandleMouseMove(e, pbImage, this);

            if (OnUserMouseMove != null)
                OnUserMouseMove(sender, e);
        }

        private void pbImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (DesignMode)
                return;

            if (OnUserMouseDown != null)
                OnUserMouseDown(sender, e);
        }

        private void pbImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (DesignMode)
                return;

            if (OnUserMouseUp != null)
                OnUserMouseUp(sender, e);
        }

        private void pbImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (DesignMode)
                return;

            if (OnUserMouseClick != null)
                OnUserMouseClick(sender, e);
        }

        private void pbImage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (DesignMode)
                return;

            if (OnUserMouseDoubleClick != null)
                OnUserMouseDoubleClick(sender, e);
        }


        public void SetCrossHairsLocation(Point pt)
        {
            if (m_crosshairs != null)
                m_crosshairs.SetLocation(pt, pbImage);
        }

        public void UpdateCrosshairs()
        {
            pbImage.Invalidate();
        }

        public static double GetTimeZoneOffset()
        {
            return TimeZoneEx.GetTimeZoneOffset();
        }

        public Configuration SetConfigurationToQuickRenderDefault(string strName, string strTag, int nValCount = 1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, bool bUseTimeResolutionForValueType = false)
        {
            Configuration = SetConfigurationToQuickRenderDefaultEx(strName, strTag, nValCount, bConvertToEastern, timeResolution, bUseTimeResolutionForValueType);
            EnableCrossHairs = true;
            return Configuration;
        }

        public static Configuration SetConfigurationToQuickRenderDefaultEx(string strName, string strTag, int nValCount = 1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, bool bUseTimeResolutionForValueType = false)
        {
            double dfTimeOffsetInHours = 0;

            if (bConvertToEastern)
                dfTimeOffsetInHours = GetTimeZoneOffset();

            Configuration cfg = new Configuration();
            cfg.Frames.Add(new ConfigurationFrame());
            cfg.Frames[0].XAxis.LabelFont = new Font("Century Gothic", 7.0f);
            cfg.Frames[0].XAxis.Visible = true;
            cfg.Frames[0].XAxis.Margin = 100;
            cfg.Frames[0].XAxis.TimeOffsetInHours = dfTimeOffsetInHours;
            cfg.Frames[0].YAxis.LabelFont = new Font("Century Gothic", 7.0f);
            cfg.Frames[0].YAxis.Decimals = 3;
            cfg.Frames[0].Plots.Add(new ConfigurationPlot());

            if (timeResolution.HasValue)
            {
                cfg.Frames[0].XAxis.ValueResolution = timeResolution.Value;
                cfg.Frames[0].XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.TIME;
            }

            if (strTag != null)
                strName += " " + strTag;

            if (!string.IsNullOrEmpty(strName))
            {
                cfg.Frames[0].Name = strName;
                cfg.Frames[0].TitleColor = Color.Black;
                cfg.Frames[0].TitleFont = new Font("Century Gothic", 12.0f, FontStyle.Bold);
            }

            if (nValCount == 4)
            {
                cfg.Frames[0].Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.CANDLE;
                cfg.Frames[0].XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.TIME;
            }
            else
            {
                cfg.Frames[0].Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.LINE;

                if (!bUseTimeResolutionForValueType)
                    cfg.Frames[0].XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.NUMBER;
            }

            cfg.Frames[0].EnableRelativeScaling(true, true, 0);

            return cfg;
        }

        public static Image QuickRender(PlotCollection plots, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, string strCfgXmlFile = null, bool bIncludeTitle = true, List<ConfigurationTargetLine> rgTargetLines = null, bool bUseTimeResolutionForValueType = false, float[] rgPlotRange = null, int? nMinPtRange = null)
        {
            PlotCollectionSet set = new PlotCollectionSet();
            set.Add(plots);
            return QuickRender(set, nWidth, nHeight, bConvertToEastern, timeResolution, strCfgXmlFile, bIncludeTitle, rgTargetLines, bUseTimeResolutionForValueType, rgPlotRange, null, null, nMinPtRange);
        }

        public static Image QuickRender(PlotCollectionSet set, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, string strCfgXmlFile = null, bool bIncludeTitle = true, List<ConfigurationTargetLine> rgTargetLines = null, bool bUseTimeResolutionForValueType = false, float[] rgPlotRange = null, string strTitle = null, int? nMargin = null, int? nMinPtRange = null, bool bShowMinuteSeparators = false)
        {
            foreach (PlotCollection col in set)
            {
                if (col.AbsoluteMinYVal == double.MaxValue || col.AbsoluteMaxYVal == -double.MaxValue)
                    col.SetMinMax();
            }

            SimpleGraphingControl simpleGraphingControl1 = new SimpleGraphingControl();
            simpleGraphingControl1.Name = "SimpleGraphing";

            int nValCount = 1;
            if (set.Count > 0 && set[0].Count > 0)
                nValCount = set[0][0].Y_values.Length;

            if (strTitle == null)
                strTitle = set[0].Name;

            simpleGraphingControl1.SetConfigurationToQuickRenderDefault(strTitle, (string)set[0].Tag, nValCount, bConvertToEastern, timeResolution, bUseTimeResolutionForValueType);

            if (set.Count > 1)
            {
                List<Color> rgColor = new List<Color>() { Color.Red, Color.Blue, Color.Green, Color.Purple, Color.Orange, Color.Aquamarine, Color.Fuchsia, Color.OrangeRed, Color.Lavender, Color.Navy, Color.Cyan, Color.DarkCyan };
                for (int i = 0; i < set.Count; i++)
                {
                    int nClrIdx = i % rgColor.Count;
                    Color clr = rgColor[nClrIdx];

                    ConfigurationPlot plotConfig;

                    if (i > 0)
                    {
                        plotConfig = new ConfigurationPlot();
                        simpleGraphingControl1.Configuration.Frames[0].Plots.Add(plotConfig);
                    }

                    double? dfClr = set[i].GetParameter("ColorOverride");
                    if (dfClr.HasValue)
                        clr = Color.FromArgb((int)dfClr.Value);

                    double? dfClrAlpha = set[i].GetParameter("ColorAlpha");
                    if (dfClrAlpha.HasValue && dfClrAlpha > 0 && dfClrAlpha < 255)
                        clr = Color.FromArgb((int)dfClrAlpha.Value, clr);

                    plotConfig = simpleGraphingControl1.Configuration.Frames[0].Plots[i];
                    plotConfig.LineColor = clr;
                    plotConfig.PlotLineColor = Color.Transparent;
                    plotConfig.PlotFillColor = Color.Transparent;
                    plotConfig.PlotType = ConfigurationPlot.PLOTTYPE.LINE;
                    plotConfig.Visible = true;
                    plotConfig.EnableLabel = true;
                    plotConfig.EnableFlag = false;
                    plotConfig.FlagColor = clr;
                    plotConfig.Name = set[i].Name;
                    plotConfig.DataIndexOnRender = i;
                }

                simpleGraphingControl1.Configuration.Frames[0].EnableRelativeScaling(true, true);
                if (nMargin.HasValue)
                    simpleGraphingControl1.Configuration.Frames[0].XAxis.Margin = (uint)nMargin.Value;
            }

            if (nMinPtRange.HasValue)
                simpleGraphingControl1.Configuration.Frames[0].MinimumYRange = nMinPtRange.Value;

            if (bShowMinuteSeparators)
                simpleGraphingControl1.Configuration.Frames[0].XAxis.ShowMinuteSeparators = true;

            if (strCfgXmlFile != null && File.Exists(strCfgXmlFile))
            {
                simpleGraphingControl1.LoadModuleCache();
                simpleGraphingControl1.LoadConfiguration(strCfgXmlFile);
            }

            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>() { set };

            if (!bIncludeTitle)
                simpleGraphingControl1.Configuration.Frames[0].Name = "";

            if (rgTargetLines != null && rgTargetLines.Count > 0)
                simpleGraphingControl1.Configuration.Frames[0].TargetLines.AddRange(rgTargetLines);

            simpleGraphingControl1.BuildGraph(rgSet);

            if (nWidth <= 0)
                nWidth = 600;

            if (nHeight <= 0)
                nHeight = 300;

            simpleGraphingControl1.ScrollToEnd(false);
            
            Image img = simpleGraphingControl1.Render(nWidth, nHeight);

            if (rgPlotRange != null && rgPlotRange.Length == 2)
            {
                rgPlotRange[0] = (float)simpleGraphingControl1.Surface.Frames[0].YAxis.ActiveMin;
                rgPlotRange[1] = (float)simpleGraphingControl1.Surface.Frames[0].YAxis.ActiveMax;
            }

            return img;
        }

        public static Image QuickRender(List<PlotCollectionSet> rgData, Configuration cfg, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, bool bResize = false)
        {
            SimpleGraphingControl simpleGraphingControl1 = new SimpleGraphingControl();
            simpleGraphingControl1.Name = "SimpleGraphing";
            simpleGraphingControl1.LoadModuleCache();
            simpleGraphingControl1.Configuration = cfg;

            simpleGraphingControl1.BuildGraph(rgData, bResize, true);

            if (nWidth <= 0)
                nWidth = 600;

            if (nHeight <= 0)
                nHeight = 300;

            simpleGraphingControl1.ScrollToEnd(false);
            return simpleGraphingControl1.Render(nWidth, nHeight);
        }

        public static Image QuickRenderEx(PlotCollectionSet set, Configuration cfg, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, bool bIncludeTitle = true, List<ConfigurationTargetLine> rgTargetLines = null, bool bUseTimeResolutionForValueType = false)
        {
            foreach (PlotCollection col in set)
            {
                if (col == null)
                    return null;

                if (col.AbsoluteMinYVal == double.MaxValue || col.AbsoluteMaxYVal == -double.MaxValue)
                    col.SetMinMax();
            }

            SimpleGraphingControl simpleGraphingControl1 = new SimpleGraphingControl();
            simpleGraphingControl1.Name = "SimpleGraphing";

            int nValCount = 1;
            if (set.Count > 0 && set[0].Count > 0)
                nValCount = set[0][0].Y_values.Length;

            simpleGraphingControl1.SetConfigurationToQuickRenderDefault(set[0].Name, (string)set[0].Tag, nValCount, bConvertToEastern, timeResolution, bUseTimeResolutionForValueType);

            if (set.Count > 1)
            {
                List<Color> rgColor = new List<Color>() { Color.Red, Color.Blue, Color.Green, Color.Purple, Color.Orange, Color.Aquamarine, Color.Fuchsia, Color.OrangeRed, Color.Lavender, Color.Navy, Color.Cyan, Color.DarkCyan };
                for (int i = 0; i < set.Count; i++)
                {
                    int nClrIdx = i % rgColor.Count;
                    Color clr = rgColor[nClrIdx];

                    ConfigurationPlot plotConfig;

                    if (i > 0)
                    {
                        plotConfig = new ConfigurationPlot();
                        simpleGraphingControl1.Configuration.Frames[0].Plots.Add(plotConfig);
                    }

                    plotConfig = simpleGraphingControl1.Configuration.Frames[0].Plots[i];
                    plotConfig.LineColor = clr;
                    plotConfig.PlotLineColor = Color.Transparent;
                    plotConfig.PlotFillColor = Color.Transparent;
                    plotConfig.PlotType = ConfigurationPlot.PLOTTYPE.LINE;
                    plotConfig.Visible = true;
                    plotConfig.EnableLabel = true;
                    plotConfig.EnableFlag = false;
                    plotConfig.FlagColor = clr;
                    plotConfig.Name = set[i].Name;
                    plotConfig.DataIndexOnRender = i;
                }

                simpleGraphingControl1.Configuration.Frames[0].EnableRelativeScaling(true, true);
            }

            simpleGraphingControl1.LoadModuleCache();
            simpleGraphingControl1.Configuration = cfg;

            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>() { set };

            if (!bIncludeTitle)
                simpleGraphingControl1.Configuration.Frames[0].Name = "";

            if (rgTargetLines != null && rgTargetLines.Count > 0)
                simpleGraphingControl1.Configuration.Frames[0].TargetLines.AddRange(rgTargetLines);

            simpleGraphingControl1.BuildGraph(rgSet, false, true);

            if (nWidth <= 0)
                nWidth = 600;

            if (nHeight <= 0)
                nHeight = 300;

            simpleGraphingControl1.ScrollToEnd(false);
            
            Image img = simpleGraphingControl1.Render(nWidth, nHeight);
            img.Tag = simpleGraphingControl1.VisiblePlotCount - 12;

            return img;
        }

        public static Configuration GetQuickRenderConfiguration(string strName, int nValCount, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, string strCfgXmlFile = null, bool bIncludeTitle = true, List<ConfigurationTargetLine> rgTargetLines = null, bool bUseTimeResolutionForValueType = false)
        {
            SimpleGraphingControl simpleGraphingControl1 = new SimpleGraphingControl();
            simpleGraphingControl1.Name = "SimpleGraphing";
            simpleGraphingControl1.SetConfigurationToQuickRenderDefault(strName, "", nValCount, bConvertToEastern, timeResolution, bUseTimeResolutionForValueType);
            return simpleGraphingControl1.Configuration;
        }
    }

    public class Crosshairs : IDisposable
    {
        bool m_bEnableCrosshairs = false;
        bool m_bSnapToXAxisTicks = true;
        GraphAxisX m_xAxis = null;
        GraphAxisY m_yAxis = null;
        Flag m_yAxisFlag = new Flag();
        Point m_ptMouse;
        Point m_ptMouseOld;
        bool m_bUserUpdateCrosshairs = false;
        Color m_clrCrossHair = Color.FromArgb(64, 0, 0, 255);
        Pen m_penCrossHair = null;

        public Crosshairs()
        {
        }

        public void Dispose()
        {
            if (m_penCrossHair != null)
            {
                m_penCrossHair.Dispose();
                m_penCrossHair = null;
            }
        }

        public Color CrossHairColor
        {
            get { return m_clrCrossHair; }
            set { m_clrCrossHair = value; }
        }

        public bool EnableCrosshairs
        {
            get { return m_bEnableCrosshairs; }
            set { m_bEnableCrosshairs = value; }
        }

        public bool UserUpdate
        {
            get { return m_bUserUpdateCrosshairs; }
            set { m_bUserUpdateCrosshairs = value; }
        }

        public bool EnableSnapToXTicks
        {
            get { return m_bSnapToXAxisTicks; }
            set { m_bSnapToXAxisTicks = value; }
        }

        public bool EnableYAxisFlag
        {
            get { return m_yAxisFlag.Enabled; }
            set { m_yAxisFlag.Enabled = value; }
        }

        public void SetAxes(GraphAxisX xAxis, GraphAxisY yAxis)
        {
            m_xAxis = xAxis;
            m_yAxis = yAxis;
            m_yAxis.CustomFlagsPost.Add(m_yAxisFlag);
        }

        public void HandleMouseMove(MouseEventArgs e, Control ctrl, SimpleGraphingControl grph)
        {
            if (!m_bEnableCrosshairs)
                return;

            m_ptMouse = e.Location;

            if (m_bSnapToXAxisTicks && m_xAxis != null)
            {
                int nPos = -1;
                for (int i = 0; i < m_xAxis.TickPositions.Count - 1; i++)
                {
                    if (i == 0 && m_ptMouse.X < m_xAxis.TickPositions[i])
                    {
                        nPos = m_xAxis.TickPositions[i];
                        break;
                    }
                    else if (m_ptMouse.X >= m_xAxis.TickPositions[i] && m_ptMouse.X < m_xAxis.TickPositions[i + 1])
                    {
                        int nDiff1 = (int)Math.Abs(m_ptMouse.X - m_xAxis.TickPositions[i]);
                        int nDiff2 = (int)Math.Abs(m_ptMouse.X - m_xAxis.TickPositions[i + 1]);

                        if (nDiff1 < nDiff2)
                            nPos = m_xAxis.TickPositions[i];
                        else
                            nPos = m_xAxis.TickPositions[i + 1];
                        break;
                    }
                    else if (i == m_xAxis.TickPositions.Count - 2 && m_ptMouse.X >= m_xAxis.TickPositions[i + 1])
                    {
                        nPos = m_xAxis.TickPositions[i + 1];
                        break;
                    }
                }

                if (nPos >= 0)
                    m_ptMouse = new Point(nPos, m_ptMouse.Y);
            }
            
            m_yAxisFlag.YPosition = m_ptMouse.Y;

            if (!m_bUserUpdateCrosshairs)
                ctrl.Invalidate();

            //if (m_yAxis != null && m_yAxisFlag.Enabled)
            //    grph.Invalidate(m_yAxis.Bounds);

            grph.Refresh();
        }

        public void SetLocation(Point pt, Control ctrl)
        {
            m_ptMouse = pt;
            ctrl.Invalidate();
        }

        public void HandlePaint(PaintEventArgs e, Image imgBack, int nMargin)
        {
            if (!m_bEnableCrosshairs)
                return;

            Graphics gimg = e.Graphics;
            Point pt = m_ptMouse;

            if (m_penCrossHair == null)
            {
                m_penCrossHair = new Pen(m_clrCrossHair, 1.0f);
                m_penCrossHair.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            }
            else if (m_penCrossHair.Color != m_clrCrossHair)
            {
                m_penCrossHair.Dispose();
                m_penCrossHair = new Pen(m_clrCrossHair);
                m_penCrossHair.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            }

            Pen p = m_penCrossHair;
            gimg.DrawLine(p, new Point(0, pt.Y), new Point(imgBack.Width - nMargin, pt.Y));
            gimg.DrawLine(p, new Point(pt.X, 0), new Point(pt.X, imgBack.Height));

            m_ptMouseOld = m_ptMouse;
        }
    }

    public class PaintEventExArgs
    {
        PaintEventArgs m_args;
        Image m_img;

        public PaintEventExArgs(PaintEventArgs e, Image img)
        {
            m_args = e;
            m_img = img;
        }

        public PaintEventArgs Args
        {
            get { return m_args; }
        }

        public Image Image
        {
            get { return m_img; }
        }
    }
}
