using System;
using System.Collections.Generic;
using System.ComponentModel;
using SimpleGraphingStd;
using System.Xml.Linq;
using SkiaSharp;

namespace SimpleGraphingStd
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationPlot
    {
        SKColor m_clrLine = SKColors.Black;
        SKColor m_clrPlotFill = SKColors.Cyan;
        SKColor m_clrPlotLine = SKColors.Black;
        SKColor m_clrPlotLineOverride = SKColors.Transparent;
        SKColor m_clrAction1Active = SKColors.Transparent;
        SKColor m_clrAction2Active = SKColors.Transparent;
        int m_nActionActiveAlpha = 32;
        float m_fLineWidth = 1.0f;
        bool m_bEnableFlag = true;
        bool m_bEnableTopMostFlag = false;
        bool m_bEnableLabel = true;
        SKColor m_clrFlag = SKColors.Cyan;
        SKColor m_clrFlagBorder = SKColors.Black;
        SKColor m_clrFlagText = SKColors.Black;
        string m_strDataParam = null;
        int m_nDataIdx = 0;
        int m_nDataIdxOnRender = 0;
        string m_strDataName = null;
        string m_strName = "";
        bool m_bVisible = true;
        uint m_nInterval = 20;
        PLOTTYPE m_plotType = PLOTTYPE.LINE;
        string m_strCustomName = "";
        bool m_bExcludeFromMinMax = false;
        bool m_bExcludeFromRender = false;
        bool m_bLookaheadActive = true;
        double m_dfMarginPercent = 0;
        double m_dfTransparency = 0;
        double m_dfMidPoint = 0;
        Dictionary<string, double> m_rgExtraSettings = new Dictionary<string, double>();
        PLOTSHAPE m_plotShape = PLOTSHAPE.ELLIPSE;

        public event EventHandler<CustomBuildArgs> OnCustomBuild;

        [NonSerialized]
        GETDATAORDER m_customBuildOrder = GETDATAORDER.NONE;

        [NonSerialized]
        Guid? m_guid = null;

        [NonSerialized]
        PropertyBag m_properties = new PropertyBag();

        public enum PLOTTYPE
        {
            LINE,
            SMA,
            EMA,
            CANDLE,
            RSI,
            HIGHLOW,
            CUSTOM,
            VOLUME,
            LINE_FILL,
            ZONE,
            BOLLINGERBANDS,
            LRSI,
            HMA,
            ZLEMA
        }

        public enum PLOTSHAPE
        {
            ELLIPSE = 0x0001,
            SQUARE = 0x0002,
            ARROW_DOWN = 0x0004,
            ARROW_UP = 0x0008
        }

        public ConfigurationPlot()
        {
            m_guid = Guid.NewGuid();
        }

        public ConfigurationPlot(ConfigurationPlot p)
        {
            m_clrLine = p.m_clrLine;
            m_clrPlotFill = p.m_clrPlotFill;
            m_clrPlotLine = p.m_clrPlotLine;
            m_clrPlotLineOverride = p.m_clrPlotLineOverride;
            m_clrAction1Active = p.m_clrAction1Active;
            m_clrAction2Active = p.m_clrAction2Active;
            m_nActionActiveAlpha = p.m_nActionActiveAlpha;
            m_fLineWidth = p.m_fLineWidth;
            m_bEnableFlag = p.m_bEnableFlag;
            m_bEnableTopMostFlag = p.m_bEnableTopMostFlag;
            m_bEnableLabel = p.m_bEnableLabel;
            m_clrFlag = p.m_clrFlag;
            m_clrFlagBorder = p.m_clrFlagBorder;
            m_clrFlagText = p.m_clrFlagText;
            m_strDataParam = p.m_strDataParam;
            m_strDataName = p.m_strDataName;
            m_nDataIdx = p.m_nDataIdx;
            m_nDataIdxOnRender = p.m_nDataIdxOnRender;
            m_strName = p.m_strName;
            m_bVisible = p.m_bVisible;
            m_nInterval = p.m_nInterval;
            m_plotType = p.m_plotType;
            m_strCustomName = p.m_strCustomName;
            m_bExcludeFromMinMax = p.m_bExcludeFromMinMax;
            m_bExcludeFromRender = p.m_bExcludeFromRender;
            m_bLookaheadActive = p.m_bLookaheadActive;
            m_dfMarginPercent = p.m_dfMarginPercent;
            m_dfTransparency = p.m_dfTransparency;
            m_dfMidPoint = p.m_dfMidPoint;
            m_plotShape = p.m_plotShape;

            foreach (var kv in p.m_rgExtraSettings)
            {
                m_rgExtraSettings.Add(kv.Key, kv.Value);
            }

            m_customBuildOrder = p.m_customBuildOrder;

            if (p.m_properties != null)
            {
                foreach (PropertyValue prop in p.m_properties)
                {
                    m_properties.Add(prop);
                }
            }
        }

        public ConfigurationPlot(Guid guid)
        {
            m_guid = guid;
        }

        public ConfigurationPlot Clone()
        {
            return new ConfigurationPlot(this);
        }

        public bool HasCustomBuild => OnCustomBuild != null;

        public bool TryCustomBuild(PlotCollectionSet dataOut)
        {
            if (OnCustomBuild == null)
                return false;

            var args = new CustomBuildArgs(dataOut);
            OnCustomBuild(this, args);

            return true;
        }

        public PLOTSHAPE PlotShape
        {
            get => m_plotShape;
            set => m_plotShape = value;
        }

        public double MarginPercent
        {
            get => m_dfMarginPercent;
            set => m_dfMarginPercent = value;
        }

        public double MidPoint
        {
            get => m_dfMidPoint;
            set => m_dfMidPoint = value;
        }

        public Guid? ID => m_guid;

        public virtual bool Compare(ConfigurationPlot c)
        {
            return m_clrLine == c.m_clrLine &&
                   m_clrPlotFill == c.m_clrPlotFill &&
                   m_clrPlotLine == c.m_clrPlotLine &&
                   m_fLineWidth == c.m_fLineWidth &&
                   m_bEnableFlag == c.m_bEnableFlag &&
                   m_bEnableTopMostFlag == c.m_bEnableTopMostFlag &&
                   m_bEnableLabel == c.m_bEnableLabel &&
                   m_clrFlag == c.m_clrFlag &&
                   m_clrFlagBorder == c.m_clrFlagBorder &&
                   m_clrFlagText == c.m_clrFlagText &&
                   m_nDataIdx == c.m_nDataIdx &&
                   m_nDataIdxOnRender == c.m_nDataIdxOnRender &&
                   m_strName == c.m_strName &&
                   m_bVisible == c.m_bVisible &&
                   m_nInterval == c.m_nInterval &&
                   m_plotType == c.m_plotType &&
                   m_bExcludeFromMinMax == c.m_bExcludeFromMinMax &&
                   m_bExcludeFromRender == c.m_bExcludeFromRender &&
                   m_clrAction1Active == c.m_clrAction1Active &&
                   m_clrAction2Active == c.m_clrAction2Active &&
                   m_nActionActiveAlpha == c.m_nActionActiveAlpha &&
                   m_bLookaheadActive == c.m_bLookaheadActive &&
                   m_strDataParam == c.m_strDataParam &&
                   m_dfTransparency == c.m_dfTransparency &&
                   (m_rgExtraSettings == null && c.m_rgExtraSettings == null || m_rgExtraSettings?.Count == c.m_rgExtraSettings?.Count);
        }

        [Description("Specifies the transparency as a % value within the range [0.0,1.0]")]
        public double Transparency
        {
            get => m_dfTransparency;
            set => m_dfTransparency = value;
        }

        public bool LookaheadActive
        {
            get => m_bLookaheadActive;
            set => m_bLookaheadActive = value;
        }

        public GETDATAORDER BuildOrder
        {
            get
            {
                if (m_plotType == PLOTTYPE.CUSTOM && m_customBuildOrder != GETDATAORDER.NONE)
                    return m_customBuildOrder;

                return m_plotType == PLOTTYPE.ZONE ? GETDATAORDER.POST : GETDATAORDER.PRE;
            }
        }

        public void SetCustomBuildOrder(GETDATAORDER order)
        {
            m_customBuildOrder = order;
        }

        public PLOTTYPE PlotType
        {
            get => m_plotType;
            set => m_plotType = value;
        }

        public string CustomName
        {
            get => m_strCustomName;
            set => m_strCustomName = value;
        }

        public string DataName
        {
            get => m_strDataName;
            set => m_strDataName = value;
        }

        public bool VirtualPlot => m_plotType != PLOTTYPE.LINE;

        public uint Interval
        {
            get => m_nInterval;
            set => m_nInterval = value;
        }

        public bool Visible
        {
            get => m_bVisible;
            set => m_bVisible = value;
        }

        public string Name
        {
            get => m_strName;
            set => m_strName = value;
        }

        public SKColor LineColor
        {
            get => m_clrLine;
            set => m_clrLine = value;
        }

        public SKColor PlotFillColor
        {
            get => m_clrPlotFill;
            set => m_clrPlotFill = value;
        }

        public SKColor PlotLineColor
        {
            get => m_clrPlotLine;
            set => m_clrPlotLine = value;
        }

        public SKColor PlotLineColorOverride
        {
            get => m_clrPlotLineOverride;
            set => m_clrPlotLineOverride = value;
        }

        public float LineWidth
        {
            get => m_fLineWidth;
            set => m_fLineWidth = value;
        }

        public bool EnableFlag
        {
            get => m_bEnableFlag;
            set => m_bEnableFlag = value;
        }

        public bool EnableTopMostFlag
        {
            get => m_bEnableTopMostFlag;
            set => m_bEnableTopMostFlag = value;
        }

        public bool EnableLabel
        {
            get => m_bEnableLabel;
            set => m_bEnableLabel = value;
        }

        public SKColor FlagColor
        {
            get => m_clrFlag;
            set => m_clrFlag = value;
        }

        public SKColor FlagBorderColor
        {
            get => m_clrFlagBorder;
            set => m_clrFlagBorder = value;
        }

        public SKColor FlagTextColor
        {
            get => m_clrFlagText;
            set => m_clrFlagText = value;
        }

        public string DataParam
        {
            get => m_strDataParam;
            set => m_strDataParam = value;
        }

        public int DataIndex
        {
            get => m_nDataIdx;
            set => m_nDataIdx = value;
        }

        public int DataIndexOnRender
        {
            get => m_nDataIdxOnRender;
            set => m_nDataIdxOnRender = value;
        }

        public PropertyBag Properties
        {
            get => m_properties;
            set => m_properties = value;
        }

        public bool ExcludeFromMinMax
        {
            get => m_bExcludeFromMinMax;
            set => m_bExcludeFromMinMax = value;
        }

        public bool ExcludeFromRender
        {
            get => m_bExcludeFromRender;
            set => m_bExcludeFromRender = value;
        }

        public SKColor ActionActive1Color
        {
            get => m_clrAction1Active;
            set => m_clrAction1Active = value;
        }

        public SKColor ActionActive2Color
        {
            get => m_clrAction2Active;
            set => m_clrAction2Active = value;
        }

        public int ActionActiveColorAlpha
        {
            get => m_nActionActiveAlpha;
            set => m_nActionActiveAlpha = value;
        }

        public Dictionary<string, double> ExtraSettings
        {
            get => m_rgExtraSettings;
            set => m_rgExtraSettings = value;
        }

        public static double SKColorToDouble(SKColor color)
        {
            return color.Red << 24 | color.Green << 16 | color.Blue << 8 | color.Alpha;
        }

        public static SKColor DoubleToSKColor(double value)
        {
            byte red = (byte)((int)value >> 24 & 0xFF);
            byte green = (byte)((int)value >> 16 & 0xFF);
            byte blue = (byte)((int)value >> 8 & 0xFF);
            byte alpha = (byte)((int)value & 0xFF);

            return new SKColor(red, green, blue, alpha);
        }

        public SKColor GetExtraSetting(string strName, SKColor colorDefault)
        {
            if (m_rgExtraSettings != null && m_rgExtraSettings.ContainsKey(strName))
            {
                double storedValue = m_rgExtraSettings[strName];
                return DoubleToSKColor(storedValue); // Convert double back to SKColor
            }
            return colorDefault;
        }

        public void SetExtraSetting(string strName, SKColor colorValue)
        {
            double colorAsDouble = SKColorToDouble(colorValue);
            SetExtraSetting(strName, colorAsDouble);
        }

        public double GetExtraSetting(string strName, double dfDefault)
        {
            return m_rgExtraSettings != null && m_rgExtraSettings.ContainsKey(strName)
                ? m_rgExtraSettings[strName]
                : dfDefault;
        }

        public void SetExtraSetting(string strName, double dfValue)
        {
            if (m_rgExtraSettings == null)
                m_rgExtraSettings = new Dictionary<string, double>();

            if (!m_rgExtraSettings.ContainsKey(strName))
                m_rgExtraSettings.Add(strName, dfValue);
            else
                m_rgExtraSettings[strName] = dfValue;
        }

        public virtual void Serialize(SerializeToXml ser)
        {
            ser.Open("Plot");
            ser.Add("LineColor", m_clrLine);
            ser.Add("PlotFillColor", m_clrPlotFill);
            ser.Add("PlotLineColor", m_clrPlotLine);
            ser.Add("ActionActive1Color", m_clrAction1Active);
            ser.Add("ActionActive2Color", m_clrAction2Active);
            ser.Add("ActionActiveColorAlpha", m_nActionActiveAlpha);
            ser.Add("LineWidth", m_fLineWidth);
            ser.Add("EnableFlag", m_bEnableFlag);
            ser.Add("EnableTopMostFlag", m_bEnableTopMostFlag);
            ser.Add("FlagColor", m_clrFlag);
            ser.Add("FlagBorderColor", m_clrFlagBorder);
            ser.Add("FlagTextColor", m_clrFlagText);
            ser.Add("DataParam", m_strDataParam);
            ser.Add("DataIndex", m_nDataIdx);
            ser.Add("DataIndexOnRender", m_nDataIdxOnRender);
            ser.Add("DataName", m_strDataName);
            ser.Add("Name", m_strName);
            ser.Add("Visible", m_bVisible);
            ser.Add("Interval", (int)m_nInterval);
            ser.Add("PlotType", m_plotType.ToString());
            ser.Add("CustomName", m_strCustomName);
            ser.Add("ExcludeFromMinMax", m_bExcludeFromMinMax);
            ser.Add("LookaheadActive", m_bLookaheadActive);
            ser.Add("MarginPercent", m_dfMarginPercent);
            ser.Add("Transparency", m_dfTransparency);
            ser.Add("MidPoint", m_dfMidPoint);

            ser.Add("ExtraCount", m_rgExtraSettings?.Count ?? 0);

            if (m_rgExtraSettings != null)
            {
                foreach (var kv in m_rgExtraSettings)
                {
                    ser.Open("Extra");
                    ser.Add("ExtraName", kv.Key);
                    ser.Add("ExtraValue", kv.Value);
                    ser.Close();
                }
            }

            ser.Add("EnableLabel", m_bEnableLabel);

            ser.Close();
        }

        public static List<ConfigurationPlot> Deserialize(IEnumerable<XElement> elms)
        {
            var rgPlot = new List<ConfigurationPlot>();
            var rgElm = SerializeToXml.GetElements(elms, "Plot");

            foreach (var elm in rgElm)
            {
                rgPlot.Add(ConfigurationPlot.Deserialize(elm));
            }

            return rgPlot;
        }

        public static ConfigurationPlot Deserialize(XElement elm)
        {
            var plot = new ConfigurationPlot
            {
                LineColor = SerializeToXml.LoadColor(elm, "LineColor").Value,
                PlotFillColor = SerializeToXml.LoadColor(elm, "PlotFillColor").Value,
                PlotLineColor = SerializeToXml.LoadColor(elm, "PlotLineColor").Value,
                ActionActive1Color = SerializeToXml.LoadColor(elm, "ActionActive1Color").Value,
                ActionActive2Color = SerializeToXml.LoadColor(elm, "ActionActive2Color").Value,
                ActionActiveColorAlpha = SerializeToXml.LoadInt(elm, "ActionActiveColorAlpha").Value,
                LineWidth = (float)SerializeToXml.LoadDouble(elm, "LineWidth").Value,
                EnableFlag = SerializeToXml.LoadBool(elm, "EnableFlag").Value,
                FlagColor = SerializeToXml.LoadColor(elm, "FlagColor").Value,
                FlagBorderColor = SerializeToXml.LoadColor(elm, "FlagBorderColor").Value,
                FlagTextColor = SerializeToXml.LoadColor(elm, "FlagTextColor").Value,
                DataParam = SerializeToXml.LoadText(elm, "DataParam"),
                DataIndex = SerializeToXml.LoadInt(elm, "DataIndex").Value,
                DataIndexOnRender = SerializeToXml.LoadInt(elm, "DataIndexOnRender").Value,
                DataName = SerializeToXml.LoadText(elm, "DataName"),
                Name = SerializeToXml.LoadText(elm, "Name"),
                Visible = SerializeToXml.LoadBool(elm, "Visible").Value,
                Interval = (uint)SerializeToXml.LoadInt(elm, "Interval").Value,
                PlotType = plotTypeFromString(SerializeToXml.LoadText(elm, "PlotType")),
                CustomName = SerializeToXml.LoadText(elm, "CustomName"),
                ExcludeFromMinMax = SerializeToXml.LoadBool(elm, "ExcludeFromMinMax").Value,
                LookaheadActive = SerializeToXml.LoadBool(elm, "LookaheadActive").Value,
                MarginPercent = SerializeToXml.LoadDouble(elm, "MarginPercent").Value,
                Transparency = SerializeToXml.LoadDouble(elm, "Transparency").Value,
                MidPoint = SerializeToXml.LoadDouble(elm, "MidPoint").Value,
                ExtraSettings = new Dictionary<string, double>()
            };

            var rgExtra = SerializeToXml.GetElements(elm.Descendants(), "Extra");
            foreach (var elm1 in rgExtra)
            {
                string strName = SerializeToXml.LoadText(elm1, "ExtraName");
                string strVal = SerializeToXml.LoadText(elm1, "ExtraValue");

                if (double.TryParse(strVal, out var dfVal))
                    plot.ExtraSettings.Add(strName, dfVal);
            }

            plot.EnableTopMostFlag = SerializeToXml.LoadBool(elm, "EnableTopMostFlag") ?? plot.EnableTopMostFlag;
            plot.EnableLabel = SerializeToXml.LoadBool(elm, "EnableLabel") ?? plot.EnableLabel;

            return plot;
        }

        private static PLOTTYPE plotTypeFromString(string str)
        {
            return Enum.TryParse(str, out PLOTTYPE type) ? type : throw new Exception("Unknown plot type '" + str + "'!");
        }

        public override string ToString()
        {
            return m_plotType.ToString();
        }
    }

    public class CustomBuildArgs : EventArgs
    {
        PlotCollectionSet m_data;

        public CustomBuildArgs(PlotCollectionSet data)
        {
            m_data = data;
        }

        public PlotCollectionSet Data => m_data;
    }
}
