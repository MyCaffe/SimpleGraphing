using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimpleGraphing
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationPlot
    {
        Color m_clrLine = Color.Black;
        Color m_clrPlotFill = Color.Cyan;
        Color m_clrPlotLine = Color.Black;
        Color m_clrAction1Active = Color.Transparent;
        Color m_clrAction2Active = Color.Transparent;
        int m_nActionActiveAlpha = 32;
        float m_fLineWidth = 1.0f;
        bool m_bEnableFlag = true;
        Color m_clrFlag = Color.Cyan;
        Color m_clrFlagBorder = Color.Black;
        Color m_clrFlagText = Color.Black;
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
        bool m_bLookaheadActive = true;
        double m_dfMarginPercent = 0;
        double m_dfTransparency = 0;
        double m_dfMidPoint = 0;
        Dictionary<string, double> m_rgExtraSettings = new Dictionary<string, double>();

        [NonSerialized]
        GETDATAORDER m_custiomBuildOrder = GETDATAORDER.NONE;

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
            ZONE
        }

        public ConfigurationPlot()
        {
            m_guid = Guid.NewGuid();
        }

        public ConfigurationPlot(Guid guid)
        {
            m_guid = guid;
        }

        public double MarginPercent
        {
            get { return m_dfMarginPercent; }
            set { m_dfMarginPercent = value; }
        }

        public double MidPoint
        {
            get { return m_dfMidPoint; }
            set { m_dfMidPoint = value; }
        }

        public Guid? ID
        {
            get { return m_guid; }
        }

        public virtual bool Compare(ConfigurationPlot c)
        {
            if (m_clrLine != c.m_clrLine)
                return false;

            if (m_clrPlotFill != c.m_clrPlotFill)
                return false;

            if (m_clrPlotLine != c.m_clrPlotLine)
                return false;

            if (m_fLineWidth != c.m_fLineWidth)
                return false;

            if (m_bEnableFlag != c.m_bEnableFlag)
                return false;

            if (m_clrFlag != c.m_clrFlag)
                return false;

            if (m_clrFlagBorder != c.m_clrFlagBorder)
                return false;

            if (m_clrFlagText != c.m_clrFlagText)
                return false;

            if (m_nDataIdx != c.m_nDataIdx)
                return false;

            if (m_nDataIdxOnRender != c.m_nDataIdxOnRender)
                return false;

            if (m_strName != c.m_strName)
                return false;

            if (m_bVisible != c.m_bVisible)
                return false;

            if (m_nInterval != c.m_nInterval)
                return false;

            if (m_plotType != c.m_plotType)
                return false;

            if (m_bExcludeFromMinMax != c.m_bExcludeFromMinMax)
                return false;

            if (m_clrAction1Active != c.m_clrAction1Active)
                return false;

            if (m_clrAction2Active != c.m_clrAction2Active)
                return false;

            if (m_nActionActiveAlpha != c.m_nActionActiveAlpha)
                return false;

            if (m_bLookaheadActive != c.m_bLookaheadActive)
                return false;

            if (m_strDataParam != c.m_strDataParam)
                return false;

            if (m_dfTransparency != c.m_dfTransparency)
                return false;

            if (m_rgExtraSettings == null && c.m_rgExtraSettings != null)
                return false;

            if (m_rgExtraSettings != null && c.m_rgExtraSettings == null)
                return false;

            if (m_rgExtraSettings != null && c.m_rgExtraSettings != null)
            {
                if (m_rgExtraSettings.Count != c.m_rgExtraSettings.Count)
                    return false;
            }

            return true;
        }

        [Description("Specifies the transparency as a % value within the range [0.0,1.0]")]
        public double Transparency
        {
            get { return m_dfTransparency; }
            set { m_dfTransparency = value; }
        }

        public bool LookaheadActive
        {
            get { return m_bLookaheadActive; }
            set { m_bLookaheadActive = value; }
        }

        public GETDATAORDER BuildOrder
        {
            get
            {
                if (m_plotType == PLOTTYPE.CUSTOM && m_custiomBuildOrder != GETDATAORDER.NONE)
                    return m_custiomBuildOrder;

                if (m_plotType == PLOTTYPE.ZONE)
                    return GETDATAORDER.POST;
                else
                    return GETDATAORDER.PRE;
            }
        }

        public void SetCustomBuildOrder(GETDATAORDER order)
        {
            m_custiomBuildOrder = order;
        }

        public PLOTTYPE PlotType
        {
            get { return m_plotType; }
            set { m_plotType = value; }
        }

        public string CustomName
        {
            get { return m_strCustomName; }
            set { m_strCustomName = value; }
        }

        public string DataName
        {
            get { return m_strDataName; }
            set { m_strDataName = value; }
        }

        public bool VirtualPlot
        {
            get { return (m_plotType == PLOTTYPE.LINE) ? false : true; }
        }

        public uint Interval
        {
            get { return m_nInterval; }
            set { m_nInterval = value; }
        }

        public bool Visible
        {
            get { return m_bVisible; }
            set { m_bVisible = value; }
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public Color LineColor
        {
            get { return m_clrLine; }
            set { m_clrLine = value; }
        }

        public Color PlotFillColor
        {
            get { return m_clrPlotFill; }
            set { m_clrPlotFill = value; }
        }

        public Color PlotLineColor
        {
            get { return m_clrPlotLine; }
            set { m_clrPlotLine = value; }
        }

        public float LineWidth
        {
            get { return m_fLineWidth; }
            set { m_fLineWidth = value; }
        }

        public bool EnableFlag
        {
            get { return m_bEnableFlag; }
            set { m_bEnableFlag = value; }
        }

        public Color FlagColor
        {
            get { return m_clrFlag; }
            set { m_clrFlag = value; }
        }

        public Color FlagBorderColor
        {
            get { return m_clrFlagBorder; }
            set { m_clrFlagBorder = value; }
        }

        public Color FlagTextColor
        {
            get { return m_clrFlagText; }
            set { m_clrFlagText = value; }
        }

        public string DataParam
        {
            get { return m_strDataParam; }
            set { m_strDataParam = value; }
        }

        public int DataIndex
        {
            get { return m_nDataIdx; }
            set { m_nDataIdx = value; }
        }

        public int DataIndexOnRender
        {
            get { return m_nDataIdxOnRender; }
            set { m_nDataIdxOnRender = value; }
        }

        public PropertyBag Properties
        {
            get { return m_properties; }
            set { m_properties = value; }
        }

        public bool ExcludeFromMinMax
        {
            get { return m_bExcludeFromMinMax; }
            set { m_bExcludeFromMinMax = value; }
        }

        public Color ActionActive1Color
        {
            get { return m_clrAction1Active; }
            set { m_clrAction1Active = value; }
        }

        public Color ActionActive2Color
        {
            get { return m_clrAction2Active; }
            set { m_clrAction2Active = value; }
        }

        public int ActionActiveColorAlpha
        {
            get { return m_nActionActiveAlpha; }
            set { m_nActionActiveAlpha = value; }
        }

        public Dictionary<string, double> ExtraSettings
        {
            get { return m_rgExtraSettings; }
            set { m_rgExtraSettings = value; }
        }

        public double GetExtraSetting(string strName, double dfDefault)
        {
            if (m_rgExtraSettings == null)
                return dfDefault;

            if (!m_rgExtraSettings.ContainsKey(strName))
                return dfDefault;

            return m_rgExtraSettings[strName];
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
            ser.Add("FlagColor", m_clrFlag);
            ser.Add("FlagBorderColor", m_clrFlagBorder);
            ser.Add("FlagTextColor", m_clrFlagText);
            ser.Add("DataParam", m_strDataParam);
            ser.Add("DataIndex", m_nDataIdx);
            ser.Add("DataIndexOnRender", m_nDataIdxOnRender);
            ser.Add("DataName", m_strDataName);
            ser.Add("Name", m_strName);
            ser.Add("Visible", m_bVisible);
            ser.Add("Interval", m_nInterval);
            ser.Add("PlotType", m_plotType.ToString());
            ser.Add("CustomName", m_strCustomName);
            ser.Add("ExcludeFromMinMax", m_bExcludeFromMinMax);
            ser.Add("LookaheadActive", m_bLookaheadActive);
            ser.Add("MarginPercent", m_dfMarginPercent);
            ser.Add("Transparency", m_dfTransparency);
            ser.Add("MidPoint", m_dfMidPoint);

            ser.Add("ExtraCount", (m_rgExtraSettings == null) ? 0 : m_rgExtraSettings.Count);

            if (m_rgExtraSettings != null)
            {
                int nIdx = 0;
                foreach (KeyValuePair<string, double> kv in m_rgExtraSettings)
                {
                    ser.Open("Extra");
                    ser.Add("ExtraName" + nIdx.ToString(), kv.Key);
                    ser.Add("ExtraValue" + nIdx.ToString(), kv.Value);
                    ser.Close();
                    nIdx++;
                }
            }

            ser.Close();
        }

        public static List<ConfigurationPlot> Deserialize(IEnumerable<XElement> elms)
        {
            List<ConfigurationPlot> rgPlot = new List<ConfigurationPlot>();
            List<XElement> rgElm = SerializeToXml.GetElements(elms, "Plot");

            foreach (XElement elm in rgElm)
            {
                ConfigurationPlot plot = ConfigurationPlot.Deserialize(elm);
                rgPlot.Add(plot);
            }

            return rgPlot;
        }

        public static ConfigurationPlot Deserialize(XElement elm)
        {
            ConfigurationPlot plot = new ConfigurationPlot();

            plot.LineColor = SerializeToXml.LoadColor(elm, "LineColor").Value;
            plot.PlotFillColor = SerializeToXml.LoadColor(elm, "PlotFillColor").Value;
            plot.PlotLineColor = SerializeToXml.LoadColor(elm, "PlotLineColor").Value;
            plot.ActionActive1Color = SerializeToXml.LoadColor(elm, "ActionActive1Color").Value;
            plot.ActionActive2Color = SerializeToXml.LoadColor(elm, "ActionActive2Color").Value;
            plot.ActionActiveColorAlpha = SerializeToXml.LoadInt(elm, "ActionActiveColorAlpha").Value;
            plot.LineWidth = (float)SerializeToXml.LoadDouble(elm, "LineWidth").Value;
            plot.EnableFlag = SerializeToXml.LoadBool(elm, "EnableFlag").Value;
            plot.FlagColor = SerializeToXml.LoadColor(elm, "FlagColor").Value;
            plot.FlagBorderColor = SerializeToXml.LoadColor(elm, "FlagBorderColor").Value;
            plot.FlagTextColor = SerializeToXml.LoadColor(elm, "FlagTextColor").Value;
            plot.DataParam = SerializeToXml.LoadText(elm, "DataParam");
            plot.DataIndex = SerializeToXml.LoadInt(elm, "DataIndex").Value;
            plot.DataIndexOnRender = SerializeToXml.LoadInt(elm, "DataIndexOnRender").Value;
            plot.DataName = SerializeToXml.LoadText(elm, "DataName");
            plot.Name = SerializeToXml.LoadText(elm, "Name");
            plot.Visible = SerializeToXml.LoadBool(elm, "Visible").Value;
            plot.Interval = (uint)SerializeToXml.LoadInt(elm, "Interval").Value;
            plot.PlotType = plotTypeFromString(SerializeToXml.LoadText(elm, "PlotType"));
            plot.CustomName = SerializeToXml.LoadText(elm, "CustomName");
            plot.ExcludeFromMinMax = SerializeToXml.LoadBool(elm, "ExcludeFromMinMax").Value;
            plot.LookaheadActive = SerializeToXml.LoadBool(elm, "LookaheadActive").Value;
            plot.MarginPercent = SerializeToXml.LoadDouble(elm, "MarginPercent").Value;
            plot.Transparency = SerializeToXml.LoadDouble(elm, "Transparency").Value;
            plot.MidPoint = SerializeToXml.LoadDouble(elm, "MidPoint").Value;
            plot.ExtraSettings = new Dictionary<string, double>();

            List<XElement> rgExtra = SerializeToXml.GetElements(elm.Descendants(), "Extra");
            foreach (XElement elm1 in rgExtra)
            {
                string strName = SerializeToXml.LoadText(elm1, "ExtraName");
                string strVal = SerializeToXml.LoadText(elm1, "ExtraValue");
                double dfVal = double.Parse(strVal);
                plot.ExtraSettings.Add(strName, dfVal);
            }

            return plot;
        }

        private static PLOTTYPE plotTypeFromString(string str)
        {
            if (str == PLOTTYPE.CANDLE.ToString())
                return PLOTTYPE.CANDLE;

            else if (str == PLOTTYPE.CUSTOM.ToString())
                return PLOTTYPE.CUSTOM;

            else if (str == PLOTTYPE.EMA.ToString())
                return PLOTTYPE.EMA;

            else if (str == PLOTTYPE.HIGHLOW.ToString())
                return PLOTTYPE.HIGHLOW;

            else if (str == PLOTTYPE.LINE.ToString())
                return PLOTTYPE.LINE;

            else if (str == PLOTTYPE.LINE_FILL.ToString())
                return PLOTTYPE.LINE_FILL;

            else if (str == PLOTTYPE.RSI.ToString())
                return PLOTTYPE.RSI;

            else if (str == PLOTTYPE.SMA.ToString())
                return PLOTTYPE.SMA;

            else if (str == PLOTTYPE.VOLUME.ToString())
                return PLOTTYPE.VOLUME;

            else if (str == PLOTTYPE.ZONE.ToString())
                return PLOTTYPE.ZONE;

            throw new Exception("Unknown plot type '" + str + "'!");
        }

        public override string ToString()
        {
            return m_plotType.ToString();
        }
    }

    public class PropertyValue
    {
        string m_strName;
        double m_dfVal;

        public PropertyValue(string strName = "", double dfVal = 0)
        {
            m_strName = strName;
            m_dfVal = dfVal;
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public double Value
        {
            get { return m_dfVal; }
            set { m_dfVal = value; }
        }
    }

    public class PropertyBag : IEnumerable<PropertyValue>
    {
        List<PropertyValue> m_rgProperties = new List<PropertyValue>();

        public PropertyBag()
        {
        }

        public int Count
        {
            get { return m_rgProperties.Count; }
        }

        public PropertyValue this[int nIdx]
        {
            get { return m_rgProperties[nIdx]; }
            set { m_rgProperties[nIdx] = value; }
        }

        public double GetProperty(string strName, double dfDefault)
        {
            PropertyValue val = Find(strName);
            if (val == null)
                return dfDefault;

            return val.Value;
        }

        public PropertyValue Find(string strName)
        {
            foreach (PropertyValue val in m_rgProperties)
            {
                if (val.Name == strName)
                    return val;
            }

            return null;
        }

        public void Add(string strName, double dfVal)
        {
            Add(new PropertyValue(strName, dfVal));
        }

        public void Add(PropertyValue val)
        {
            PropertyValue existing = Find(val.Name);
            if (existing != null)
            {
                existing.Value = val.Value;
                return;
            }

            m_rgProperties.Add(val);
        }

        public bool Remove(PropertyValue val)
        {
            return m_rgProperties.Remove(val);
        }

        public void RemoveAt(int nIdx)
        {
            m_rgProperties.RemoveAt(nIdx);
        }

        public void Clear()
        {
            m_rgProperties.Clear();
        }

        public IEnumerator<PropertyValue> GetEnumerator()
        {
            return m_rgProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgProperties.GetEnumerator();
        }
    }
}
