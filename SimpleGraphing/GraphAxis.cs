﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class GraphAxis : IDisposable
    {
        protected ConfigurationAxis m_config = new ConfigurationAxis();
        protected GraphAxisStyle m_style = null;
        protected Rectangle m_rcBounds;
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

        public ConfigurationAxis Configuration
        {
            get { return m_config; }
        }

        public virtual void SetMinMax(double dfMin, double dfMax)
        {
            if (double.IsNaN(dfMin))
                throw new Exception("The minimum is an invalid NAN!");

            if (double.IsNaN(dfMax))
                throw new Exception("The maximum is an invalid NAN!");

            if (double.IsInfinity(dfMin))
                throw new Exception("The minimum is an invalid Infinity!");

            if (double.IsInfinity(dfMax))
                throw new Exception("the maximum is an invalid Infinity!");

            m_dfMin = Math.Min(dfMin, m_config.InitialMinimum);
            m_dfMax = Math.Max(dfMax, m_config.InitialMaximum);
        }

        public int StartPosition
        {
            get
            {
                int nPos =  m_nStartPosition - m_nScrollOffset;
                if (nPos < 0)
                    nPos = 0;

                return nPos;
            }
            set
            {
                m_nScrollOffset = 0;
                m_nStartPosition = value;
            }
        }

        public int ZeroLinePosition
        {
            get { return m_nZeroPosition; }
        }

        public virtual int Width
        {
            get { return 0; }
        }

        public virtual int Height
        {
            get { return 0; }
        }

        public Rectangle Bounds
        {
            get { return m_rcBounds; }
            set { m_rcBounds = value; }
        }

        public List<int> TickPositions
        {
            get { return m_rgTickPositions; }
        }

        public List<TickValue> TickValues
        {
            get { return m_rgTickValues; }
        }

        protected virtual float plot_min
        {
            get { return 0; }
        }

        protected virtual float plot_max
        {
            get { return 1; }
        }

        public virtual float ScaleValue(double dfVal, bool bInvert)
        {
            float fPlotMin = plot_min;
            float fPlotMax = plot_max;
            float fDataMin = (float)m_dfMin;
            float fDataMax = (float)m_dfMax;
            float fPlotRange = fPlotMax - fPlotMin;
            float fDataRange = fDataMax - fDataMin;

            float fVal = (float)dfVal;

            fVal = (fDataRange == 0) ? 0 : (fVal - fDataMin) / fDataRange;
            fVal *= fPlotRange;

            if (bInvert)
                fVal = fPlotMax - fVal;
            else
                fVal = fPlotMin + fVal;

            return fVal;
        }

        public static int ConvertRange(
            int originalStart, int originalEnd, // original range
            int newStart, int newEnd, // desired range
            int value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (int)(newStart + ((value - originalStart) * scale));
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
            return new SimpleGraphing.GraphAxisStyle(m_config);
        }

        public virtual void Resize(int nX, int nY, int nWidth, int nHeight)
        {
        }

        public virtual void Render(Graphics g)
        {
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
        FontStyle m_style = FontStyle.Regular;
        bool m_bNewHour = false;
        DateTime m_dtLast;
        DateTime m_dt;

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

        public TickValue(Plot p, TYPE type, ConfigurationAxis config, double dfLastVal, ref int nDayCount, ref int nDayLast)
        {
            m_config = config;
            m_plot = p;
            m_dfVal = (type == TYPE.X) ? p.X : p.Y;
            m_nDayCount = nDayCount;
            m_nDayLast = nDayLast;
            m_dfLastVal = dfLastVal;
            m_dt = DateTime.FromFileTime((long)m_dfVal);

            if (m_dfLastVal > 0)
                m_dtLast = DateTime.FromFileTime((long)m_dfLastVal);

            m_strValue = GetValueString();

            nDayCount = DayCount;
            nDayLast = DayLast;
        }

        public override string ToString()
        {
            string strNewHour = (m_bNewHour) ? "NEW" : "OLD";
            string strShowHourBars = (m_config.ShowHourSeparators) ? "Yes" : "No";
            return m_strValue + " (hour: " + strNewHour + ", show hrsep: " + strShowHourBars + ")"; 
        }

        public string GetValueString()
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

                    if (m_config.ShowHourSeparators && m_bNewHour)
                        m_style = FontStyle.Bold;                       
                }

                if (config.ValueResolution == ConfigurationAxis.VALUE_RESOLUTION.DAY)
                    return m_dt.ToShortDateString();
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
                        m_style = FontStyle.Bold;
                        m_nDayLast = m_dt.Day;
                        string strVal = m_dt.Day.ToString("00") + " " + m_dt.Hour.ToString("00") + ":" + m_dt.Minute.ToString("00");

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

        public FontStyle Style
        {
            get { return m_style; }
        }

        public bool NewHour
        {
            get { return m_bNewHour; }
        }
    }

    public class GraphAxisStyle : IDisposable
    {
        Pen m_penZeroLine;
        Pen m_penTick;
        Brush m_brLabel;
        Brush m_brHourLabel;

        public GraphAxisStyle(ConfigurationAxis c)
        {
            m_penZeroLine = new Pen(c.ZeroLineColor, 1.0f);
            m_penTick = new Pen(c.TickColor, 1.0f);
            m_brLabel = new SolidBrush(c.LabelColor);

            Color clr = Color.FromArgb(128, Color.Lavender);
            m_brHourLabel = new SolidBrush(clr);
        }

        public Pen ZeroLinePen
        {
            get { return m_penZeroLine; }
        }

        public Pen TickPen
        {
            get { return m_penTick; }
        }

        public Brush LabelBrush
        {
            get { return m_brLabel; }
        }

        public Brush HourLabel
        {
            get { return m_brHourLabel; }
        }

        public void Dispose()
        {
            if (m_penZeroLine != null)
            {
                m_penZeroLine.Dispose();
                m_penZeroLine = null;
            }

            if (m_penTick != null)
            {
                m_penTick.Dispose();
                m_penTick = null;
            }

            if (m_brLabel != null)
            {
                m_brLabel.Dispose();
                m_brLabel = null;
            }

            if (m_brHourLabel != null)
            {
                m_brHourLabel.Dispose();
                m_brHourLabel = null;
            }
        }
    }
}
