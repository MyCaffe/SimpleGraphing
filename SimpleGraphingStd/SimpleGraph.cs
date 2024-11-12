using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace SimpleGraphingStd
{
    public class SimpleGraph
    {
        Configuration m_cfg;
        GraphSurface m_surface;

        public SimpleGraph(Configuration cfg = null)
        {
            if (cfg == null)
                cfg = createDefaultConfiguration();

            m_cfg = cfg;
        }

        private Configuration createDefaultConfiguration()
        {
            Configuration cfg = new Configuration();



            return cfg;
        }

        public List<PlotCollectionSet> BuildGraph(List<PlotCollectionSet> rgSet, bool bAddToParams = true)
        {
            ModuleCache cache = new ModuleCache();
            // Disable cache load, currently not supported.
            //cache.Load(); 

            m_surface = new GraphSurface(cache);
            return m_surface.BuildGraph(m_cfg, rgSet, bAddToParams);
        }

        public SKImage Render(int nWidth = 600, int nHeight = 600)
        {
            m_surface.Scroll(1.0);
            m_surface.Resize(nWidth, nHeight, true);
            return m_surface.Render();
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
                        m_cfg = formatter.Deserialize(strm) as Configuration;
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

                m_cfg = Configuration.LoadFromFile(strFile);
            }
        }

        public Configuration Configuration
        {
            get { return m_cfg; }
        }

        public GraphSurface Surface
        {
            get { return m_surface; }
        }

        public static double GetTimeZoneOffset()
        {
            return TimeZoneEx.GetTimeZoneOffset();
        }

        public static Configuration GetQuickRenderConfiguration(string strName, int nValCount, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, bool bIncludeTitle = true, List<ConfigurationTargetLine> rgTargetLines = null, bool bUseTimeResolutionForValueType = false)
        {
            Configuration cfg = SetConfigurationToQuickRenderDefault(strName, "", nValCount, bConvertToEastern, timeResolution, bUseTimeResolutionForValueType);

            if (bIncludeTitle)
                cfg.Frames[0].TitleColor = SKColors.Black;
            else
                cfg.Frames[0].TitleColor = SKColors.Transparent;

            if (rgTargetLines != null && rgTargetLines.Count > 0 && cfg.Frames.Count > 0)
                cfg.Frames[0].TargetLines.AddRange(rgTargetLines);

            return cfg;
        }

        public static Configuration SetConfigurationToQuickRenderDefault(
            string strName,
            string strTag,
            int nValCount = 1,
            bool bConvertToEastern = false,
            ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null,
            bool bUseTimeResolutionForValueType = false)
        {
            double dfTimeOffsetInHours = 0;

            if (bConvertToEastern)
                dfTimeOffsetInHours = GetTimeZoneOffset();

            Configuration cfg = new Configuration();
            cfg.Frames.Add(new ConfigurationFrame());

            // Setting up the X Axis
            cfg.Frames[0].XAxis.LabelFont = new SKFont(SKTypeface.FromFamilyName("Century Gothic"), 7.0f);
            cfg.Frames[0].XAxis.Visible = true;
            cfg.Frames[0].XAxis.Margin = 100;
            cfg.Frames[0].XAxis.TimeOffsetInHours = dfTimeOffsetInHours;

            // Setting up the Y Axis
            cfg.Frames[0].YAxis.LabelFont = new SKFont(SKTypeface.FromFamilyName("Century Gothic"), 7.0f);
            cfg.Frames[0].YAxis.Decimals = 3;

            // Adding a plot configuration
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
                cfg.Frames[0].TitleColor = SKColors.Black;
                // Create an SKTypeface with a bold style
                SKTypeface typeface = SKTypeface.FromFamilyName("Century Gothic", SKFontStyle.Bold);

                // Use the typeface to create an SKFont with a specified size
                cfg.Frames[0].TitleFont = new SKFont(typeface, 12.0f);
            }

            // Configure plot type based on value count
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

        public static SKImage QuickRender(PlotCollection plots, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, string strCfgXmlFile = null, bool bIncludeTitle = true, List<ConfigurationTargetLine> rgTargetLines = null, bool bUseTimeResolutionForValueType = false, float[] rgPlotRange = null, int? nMinPtRange = null)
        {
            PlotCollectionSet set = new PlotCollectionSet();
            set.Add(plots);
            return QuickRender(set, nWidth, nHeight, bConvertToEastern, timeResolution, strCfgXmlFile, bIncludeTitle, rgTargetLines, bUseTimeResolutionForValueType, rgPlotRange, null, null, nMinPtRange);
        }

        public static SKImage QuickRender(PlotCollectionSet set, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, string strCfgXmlFile = null, bool bIncludeTitle = true, List<ConfigurationTargetLine> rgTargetLines = null, bool bUseTimeResolutionForValueType = false, float[] rgPlotRange = null, string strTitle = null, int? nMargin = null, int? nMinPtRange = null, bool bShowMinuteSeparators = false)
        {
            foreach (PlotCollection col in set)
            {
                if (col.AbsoluteMinYVal == double.MaxValue || col.AbsoluteMaxYVal == -double.MaxValue)
                    col.SetMinMax();
            }

            int nValCount = 1;
            if (set.Count > 0 && set[0].Count > 0)
                nValCount = set[0][0].Y_values.Length;

            if (strTitle == null)
                strTitle = set[0].Name;

            Configuration cfg = SetConfigurationToQuickRenderDefault(strTitle, (string)set[0].Tag, nValCount, bConvertToEastern, timeResolution, bUseTimeResolutionForValueType);

            if (set.Count > 1)
            {
                // Define colors using SkiaSharp SKColor
                List<SKColor> rgColor = new List<SKColor>()
                {
                    SKColors.Red, SKColors.Blue, SKColors.Green, SKColors.Purple, SKColors.Orange,
                    SKColors.Aquamarine, SKColors.Fuchsia, SKColors.OrangeRed, SKColors.Lavender,
                    SKColors.Navy, SKColors.Cyan, SKColors.DarkCyan
                };

                for (int i = 0; i < set.Count; i++)
                {
                    int nClrIdx = i % rgColor.Count;
                    SKColor clr = rgColor[nClrIdx];

                    ConfigurationPlot plotConfig;

                    if (i > 0)
                    {
                        plotConfig = new ConfigurationPlot();
                        cfg.Frames[0].Plots.Add(plotConfig);
                    }

                    // Override color if specified in parameters
                    double? dfClr = set[i].GetParameter("ColorOverride");
                    if (dfClr.HasValue)
                        clr = new SKColor((uint)dfClr.Value);

                    // Override alpha if specified in parameters
                    double? dfClrAlpha = set[i].GetParameter("ColorAlpha");
                    if (dfClrAlpha.HasValue && dfClrAlpha > 0 && dfClrAlpha < 255)
                        clr = clr.WithAlpha((byte)dfClrAlpha.Value);

                    // Configure plot settings
                    plotConfig = cfg.Frames[0].Plots[i];
                    plotConfig.LineColor = clr;
                    plotConfig.PlotLineColor = SKColors.Transparent;
                    plotConfig.PlotFillColor = SKColors.Transparent;
                    plotConfig.PlotType = ConfigurationPlot.PLOTTYPE.LINE;
                    plotConfig.Visible = true;
                    plotConfig.EnableLabel = true;
                    plotConfig.EnableFlag = false;
                    plotConfig.FlagColor = clr;
                    plotConfig.Name = set[i].Name;
                    plotConfig.DataIndexOnRender = i;
                }

                cfg.Frames[0].EnableRelativeScaling(true, true);
                if (nMargin.HasValue)
                    cfg.Frames[0].XAxis.Margin = (uint)nMargin.Value;
            }

            if (nMinPtRange.HasValue)
                cfg.Frames[0].MinimumYRange = nMinPtRange.Value;

            if (bShowMinuteSeparators)
                cfg.Frames[0].XAxis.ShowMinuteSeparators = true;

            //if (strCfgXmlFile != null && File.Exists(strCfgXmlFile))
            //{
            //    simpleGraphingControl1.LoadModuleCache();
            //    simpleGraphingControl1.LoadConfiguration(strCfgXmlFile);
            //}

            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>() { set };

            if (!bIncludeTitle)
                cfg.Frames[0].Name = "";

            if (rgTargetLines != null && rgTargetLines.Count > 0)
                cfg.Frames[0].TargetLines.AddRange(rgTargetLines);

            SimpleGraph sg = new SimpleGraph(cfg);
            sg.BuildGraph(rgSet);

            if (nWidth <= 0)
                nWidth = 600;

            if (nHeight <= 0)
                nHeight = 300;

            SKImage img = sg.Render(nWidth, nHeight);

            if (rgPlotRange != null && rgPlotRange.Length == 2)
            {
                rgPlotRange[0] = (float)sg.Surface.Frames[0].YAxis.ActiveMin;
                rgPlotRange[1] = (float)sg.Surface.Frames[0].YAxis.ActiveMax;
            }

            return img;
        }

        public static SKImage QuickRender(List<PlotCollectionSet> rgData, Configuration cfg, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, bool bResize = false)
        {
            SimpleGraph sg = new SimpleGraph(cfg);

            sg.BuildGraph(rgData);

            if (nWidth <= 0)
                nWidth = 600;

            if (nHeight <= 0)
                nHeight = 300;

            return sg.Render(nWidth, nHeight);
        }

        public static SKImage QuickRenderEx(PlotCollectionSet set, Configuration cfg, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false, ConfigurationAxis.VALUE_RESOLUTION? timeResolution = null, bool bIncludeTitle = true, List<ConfigurationTargetLine> rgTargetLines = null, bool bUseTimeResolutionForValueType = false)
        {
            foreach (PlotCollection col in set)
            {
                if (col == null)
                    return null;

                if (col.AbsoluteMinYVal == double.MaxValue || col.AbsoluteMaxYVal == -double.MaxValue)
                    col.SetMinMax();
            }

            int nValCount = 1;
            if (set.Count > 0 && set[0].Count > 0)
                nValCount = set[0][0].Y_values.Length;

            cfg = SetConfigurationToQuickRenderDefault(set[0].Name, (string)set[0].Tag, nValCount, bConvertToEastern, timeResolution, bUseTimeResolutionForValueType);
            SimpleGraph sg = new SimpleGraph(cfg);

            if (set.Count > 1)
            {
                // Define colors using SkiaSharp SKColor
                List<SKColor> rgColor = new List<SKColor>()
                {
                    SKColors.Red, SKColors.Blue, SKColors.Green, SKColors.Purple, SKColors.Orange,
                    SKColors.Aquamarine, SKColors.Fuchsia, SKColors.OrangeRed, SKColors.Lavender,
                    SKColors.Navy, SKColors.Cyan, SKColors.DarkCyan
                };

                for (int i = 0; i < set.Count; i++)
                {
                    int nClrIdx = i % rgColor.Count;
                    SKColor clr = rgColor[nClrIdx];

                    ConfigurationPlot plotConfig;

                    if (i > 0)
                    {
                        plotConfig = new ConfigurationPlot();
                        cfg.Frames[0].Plots.Add(plotConfig);
                    }

                    // Override color if specified in parameters
                    double? dfClr = set[i].GetParameter("ColorOverride");
                    if (dfClr.HasValue)
                        clr = new SKColor((uint)dfClr.Value);

                    // Override alpha if specified in parameters
                    double? dfClrAlpha = set[i].GetParameter("ColorAlpha");
                    if (dfClrAlpha.HasValue && dfClrAlpha > 0 && dfClrAlpha < 255)
                        clr = clr.WithAlpha((byte)dfClrAlpha.Value);

                    // Configure plot settings
                    plotConfig = cfg.Frames[0].Plots[i];
                    plotConfig.LineColor = clr;
                    plotConfig.PlotLineColor = SKColors.Transparent;
                    plotConfig.PlotFillColor = SKColors.Transparent;
                    plotConfig.PlotType = ConfigurationPlot.PLOTTYPE.LINE;
                    plotConfig.Visible = true;
                    plotConfig.EnableLabel = true;
                    plotConfig.EnableFlag = false;
                    plotConfig.FlagColor = clr;
                    plotConfig.Name = set[i].Name;
                    plotConfig.DataIndexOnRender = i;
                }

                cfg.Frames[0].EnableRelativeScaling(true, true);
            }

            //simpleGraphingControl1.LoadModuleCache();
            //simpleGraphingControl1.Configuration = cfg;

            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>() { set };

            if (!bIncludeTitle)
                cfg.Frames[0].Name = "";

            if (rgTargetLines != null && rgTargetLines.Count > 0)
                cfg.Frames[0].TargetLines.AddRange(rgTargetLines);

            sg.BuildGraph(rgSet);

            if (nWidth <= 0)
                nWidth = 600;

            if (nHeight <= 0)
                nHeight = 300;

            return sg.Render(nWidth, nHeight);
        }
    }
}
