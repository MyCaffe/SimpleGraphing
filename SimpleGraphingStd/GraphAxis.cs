using System;
using System.Collections.Generic;
using System.Globalization;
using SimpleGraphingStd;
using SkiaSharp;

namespace SimpleGraphingStd
{
    public class GraphAxis : IDisposable
    {
        protected ConfigurationAxis m_config = new ConfigurationAxis();
        protected GraphAxisStyle m_style = null;
        protected SKRect m_rcBounds;
        protected double m_dfMin = double.MaxValue;
        protected double m_dfMax = -double.MaxValue;
        protected double m_dfInc = -1;
        protected List<int> m_rgTickPositions = new List<int>();
        protected List<TickValue> m_rgTickValues = new List<TickValue>();
        protected PlotCollectionSet m_data;
        protected int m_nZeroPosition = -1;
        protected int m_nStartPosition = 0;
        protected int m_nScrollOffset = 0;
        protected int m_nDayLast = -1;
        protected int m_nDayCount = 0;
        protected double m_dfDataMin = double.MaxValue;
        protected double m_dfDataMax = -double.MaxValue;
        protected double? m_dfScaleMin = null;
        protected double? m_dfScaleMax = null;

        public GraphAxis()
        {
        }

        protected virtual void dispose()
        {
        }

        public void Dispose()
        {
            if (m_style != null)
            {
                m_style.Dispose();
                m_style = null;
            }

            dispose();
        }

        public double Min => m_dfMin;
        public double Max => m_dfMax;
        public double ActiveMin => m_dfScaleMin ?? m_dfMin;
        public double ActiveMax => m_dfScaleMax ?? m_dfMax;
        public double DataMin { get => m_dfDataMin; set => m_dfDataMin = value; }
        public double DataMax { get => m_dfDataMax; set => m_dfDataMax = value; }
        public ConfigurationAxis Configuration => m_config;

        public virtual void SetMinMax(double dfMin, double dfMax)
        {
            if (!double.IsNaN(dfMin) && !double.IsNaN(dfMax) &&
                !double.IsInfinity(dfMin) && !double.IsInfinity(dfMax))
            {
                m_dfMin = Math.Min(dfMin, m_config.InitialMinimum);
                m_dfMax = Math.Max(dfMax, m_config.InitialMaximum);
            }
            else
            {
                m_dfMin = m_config.InitialMinimum;
                m_dfMax = m_config.InitialMaximum;
            }
        }

        public int StartPosition
        {
            get => Math.Max(m_nStartPosition - m_nScrollOffset, 0);
            set
            {
                m_nScrollOffset = 0;
                m_nStartPosition = value;
            }
        }

        public int ZeroLinePosition => m_nZeroPosition;
        public virtual int Width => 0;
        public virtual int Height => 0;
        public SKRect Bounds { get => m_rcBounds; set => m_rcBounds = value; }
        public List<int> TickPositions => m_rgTickPositions;
        public List<TickValue> TickValues => m_rgTickValues;

        protected virtual float plot_min => 0;
        protected virtual float plot_max => 1;

        public virtual float ScaleValue(double dfVal, bool bInvert)
        {
            double dfMin = m_dfMin;
            double dfMax = m_dfMax;

            if (m_dfScaleMin.HasValue && m_dfScaleMax.HasValue)
            {
                dfMin = m_dfScaleMin.Value;
                dfMax = m_dfScaleMax.Value;
            }

            float fPlotMin = plot_min;
            float fPlotMax = plot_max;
            float fDataMin = (float)dfMin;
            float fDataMax = (float)dfMax;
            float fPlotRange = fPlotMax - fPlotMin;
            float fDataRange = fDataMax - fDataMin;

            float fVal = (float)dfVal;

            fVal = (fDataRange == 0) ? 0 : (fVal - fDataMin) / fDataRange;
            fVal *= fPlotRange;

            return bInvert ? fPlotMax - fVal : fPlotMin + fVal;
        }

        public virtual void BuildGraph(ConfigurationAxis config, PlotCollectionSet data)
        {
            m_data = data;
            m_style = createStyle(config);
        }

        private GraphAxisStyle createStyle(ConfigurationAxis c)
        {
            if (m_style != null && m_config != null && m_config.Compare(c))
                return m_style;

            if (m_style != null)
                m_style.Dispose();

            m_config = c;
            return new GraphAxisStyle(m_config);
        }

        public virtual void Resize(int nX, int nY, int nWidth, int nHeight)
        {
        }

        public virtual void Render(SKCanvasEx canvas)
        {
            if (m_config.IsStyleDirty)
            {
                m_style?.Dispose();
                m_style = new GraphAxisStyle(m_config);
                m_config.ClearStyleDirty();
            }
        }

        public virtual void Scroll(double dfPct)
        {
        }
    }

    public class TickValue
    {
        string m_strValue;
        ConfigurationAxis m_config;
        Plot m_plot;
        double m_dfVal;
        double m_dfLastVal;
        int m_nDayLast = -1;
        int m_nDayCount = 0;
        SKFontStyle m_style = SKFontStyle.Normal;
        bool m_bNewHour = false;
        bool m_bNewMinute = false;
        DateTime m_dtLast;
        DateTime m_dt;
        TYPE m_type;

        public enum TYPE
        {
            X,
            Y
        }

        public TickValue(double dfVal, ConfigurationAxis config)
        {
            m_config = config;
            m_plot = null;
            m_dfVal = dfVal;
            m_strValue = GetValueString();
        }

        public TickValue(Plot p, TYPE type, ConfigurationAxis config, double dfLastVal, DateTime dtLast, ref int nDayCount, ref int nDayLast)
        {
            m_config = config;
            m_plot = p;
            m_type = type;
            m_dfVal = (type == TYPE.X) ? p.X : p.Y;
            m_nDayCount = nDayCount;
            m_nDayLast = nDayLast;
            m_dfLastVal = dfLastVal;

            if (config.ValueType == ConfigurationAxis.VALUE_TYPE.TIME)
            {
                try
                {
                    m_dt = DateTime.FromFileTime((long)m_dfVal);
                }
                catch (Exception)
                {
                    m_dt = DateTime.MinValue;
                }

                if (p.Tag != null && p.Tag is DateTime)
                    m_dt = (DateTime)p.Tag;
            }

            m_dtLast = dtLast;
            m_strValue = GetValueString();

            nDayCount = DayCount;
            nDayLast = DayLast;
        }

        public TickValue Clone()
        {
            int nDayCount = DayCount;
            int nDayLast = DayLast;
            return new TickValue(m_plot.Clone(true), m_type, m_config.Clone(), m_dfLastVal, m_dtLast, ref nDayCount, ref nDayLast);
        }

        public string UpdateValueString(bool bLabelVisible, DateTime? dtLastVisible)
        {
            m_strValue = GetValueString(bLabelVisible, dtLastVisible);
            return m_strValue;
        }

        public DateTime TimeStamp
        {
            get { return m_dt; }
        }

        public double Value
        {
            get { return m_dfVal; }
        }

        public Plot Plot
        {
            get { return m_plot; }
        }

        public override string ToString()
        {
            string strNewHour = (m_bNewHour) ? "NEW" : "OLD";
            string strShowHourBars = (m_config.ShowHourSeparators) ? "Yes" : "No";
            return m_strValue + " (hour: " + strNewHour + ", show hrsep: " + strShowHourBars + ")";
        }

        private string formatDayMonth(DateTime dt, bool? bLabelVisible, DateTime? dtLastVisible)
        {
            string str = "";

            if (dt == DateTime.MinValue || dt == m_dtLast || !bLabelVisible.GetValueOrDefault(true))
                return str;

            DateTime dtLast = m_dtLast;
            if (dtLastVisible.HasValue)
                dtLast = dtLastVisible.Value;

            if (dt.Year != dtLast.Year)
            {
                str += dt.Year.ToString();
                str += " ";
            }

            if (dt.Month != dtLast.Month)
            {
                str += CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dt.Month);
                str += " ";
            }

            str += dt.Day.ToString();

            return str;
        }

        public string GetValueString(bool? bLabelVisible = null, DateTime? dtLastVisible = null)
        {
            ConfigurationAxis config = m_config;
            double dfVal = m_dfVal;

            if (config.ValueType == ConfigurationAxis.VALUE_TYPE.TIME)
            {
                if (m_dfLastVal != 0)
                {
                    if (m_dt.Hour != m_dtLast.Hour)
                        m_bNewHour = true;
                    else
                        m_bNewHour = false;

                    if (m_dt.Minute != m_dtLast.Minute)
                        m_bNewMinute = true;
                    else
                        m_bNewMinute = false;

                    if (m_config.ShowHourSeparators && m_bNewHour)
                        m_style = SKFontStyle.Bold;

                    if (m_config.ShowMinuteSeparators && m_bNewMinute)
                        m_style = SKFontStyle.Bold;
                }

                if (config.ValueResolution == ConfigurationAxis.VALUE_RESOLUTION.DAY)
                    return m_dt.ToShortDateString();
                else if (config.ValueResolution == ConfigurationAxis.VALUE_RESOLUTION.DAY_MONTH)
                    return formatDayMonth(m_dt, bLabelVisible, dtLastVisible);
                else
                {
                    if (config.TimeOffsetInHours > 0)
                        m_dt += TimeSpan.FromHours(config.TimeOffsetInHours);

                    if (m_dt.Day != m_nDayLast)
                    {
                        m_nDayLast = m_dt.Day;
                        m_nDayCount = 0;
                    }

                    m_nDayCount++;

                    if (m_nDayCount <= 2)
                    {
                        m_style = SKFontStyle.Bold;
                        m_nDayLast = m_dt.Day;
                        string strVal = m_dt.Day.ToString("00") + "  " + m_dt.Hour.ToString("00") + ":" + m_dt.Minute.ToString("00");

                        if (config.ShowSeconds)
                            strVal += ":" + m_dt.Second.ToString("00");

                        return strVal;
                    }
                    else
                    {
                        string strVal = m_dt.Hour.ToString("00") + ":" + m_dt.Minute.ToString("00");

                        if (config.ShowSeconds)
                            strVal += ":" + m_dt.Second.ToString("00");

                        return strVal;
                    }
                }
            }
            else
            {
                string strFmt = "N" + config.Decimals.ToString();
                dfVal = Math.Round(dfVal, m_config.Decimals);
                return dfVal.ToString(strFmt);
            }
        }

        public int DayCount
        {
            get { return m_nDayCount; }
        }

        public int DayLast
        {
            get { return m_nDayLast; }
        }

        public string ValueString
        {
            get { return m_strValue; }
        }

        public SKFontStyle Style
        {
            get { return m_style; }
        }

        public bool NewHour
        {
            get { return m_bNewHour; }
        }

        public bool NewMinute
        {
            get { return m_bNewMinute; }
        }
    }

    public class GraphAxisStyle : IDisposable
    {
        SKPaint m_penZeroLine;
        SKPaint m_penTick;
        SKPaint m_brLabel;
        SKPaint m_brHourLabel;

        public GraphAxisStyle(ConfigurationAxis c)
        {
            m_penZeroLine = new SKPaint { Color = c.ZeroLineColor, StrokeWidth = 1.0f };
            m_penTick = new SKPaint { Color = c.TickColor, StrokeWidth = 1.0f };
            m_brLabel = new SKPaint { Color = c.LabelColor };
            m_brHourLabel = new SKPaint { Color = SKColors.Lavender.WithAlpha(128) };
        }

        public SKPaint ZeroLinePen => m_penZeroLine;
        public SKPaint TickPen => m_penTick;
        public SKPaint LabelBrush => m_brLabel;
        public SKPaint HourLabel => m_brHourLabel;

        public void Dispose()
        {
            m_penZeroLine?.Dispose();
            m_penTick?.Dispose();
            m_brLabel?.Dispose();
            m_brHourLabel?.Dispose();
        }
    }

    public static class Extensions
    {
        public static SKColor ToSKColor(this System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }
    }
}
