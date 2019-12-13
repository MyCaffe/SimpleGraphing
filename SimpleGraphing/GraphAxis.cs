using System;
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
        protected List<string> m_rgTickValues = new List<string>();
        protected PlotCollectionSet m_data;
        protected int m_nZeroPosition = -1;
        protected int m_nStartPosition = 0;
        protected int m_nScrollOffset = 0;
        int m_nDayLast = -1;
        int m_nDayCount = 0;

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

        public void SetMinMax(double dfMin, double dfMax)
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

        public List<string> TickValues
        {
            get { return m_rgTickValues; }
        }

        protected string getValueString(double dfVal, ConfigurationAxis config)
        {
            if (config.ValueType == ConfigurationAxis.VALUE_TYPE.TIME)
            {
                DateTime dt = DateTime.FromFileTime((long)dfVal);

                if (config.ValueResolution == ConfigurationAxis.VALUE_RESOLUTION.DAY)
                    return dt.ToShortDateString();
                else
                {
                    if (config.TimeOffsetInHours > 0)
                        dt += TimeSpan.FromHours(config.TimeOffsetInHours);

                    if (dt.Day != m_nDayLast)
                    {
                        m_nDayLast = dt.Day;
                        m_nDayCount = 0;
                    }

                    m_nDayCount++;

                    if (m_nDayCount <= 2)
                    {
                        m_nDayLast = dt.Day;
                        return dt.Day.ToString("00") + " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00");
                    }
                    else
                    {
                        return dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00");
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

    public class GraphAxisStyle : IDisposable
    {
        Pen m_penZeroLine;
        Pen m_penTick;
        Brush m_brLabel;

        public GraphAxisStyle(ConfigurationAxis c)
        {
            m_penZeroLine = new Pen(c.ZeroLineColor, 1.0f);
            m_penTick = new Pen(c.TickColor, 1.0f);
            m_brLabel = new SolidBrush(c.LabelColor);    
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
        }
    }
}
