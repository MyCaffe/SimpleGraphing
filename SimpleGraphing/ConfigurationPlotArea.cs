﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationPlotArea
    {
        List<ConfigurationTimeZone> m_rgTimeZones = new List<ConfigurationTimeZone>();
        Color m_clrGrid = Color.FromArgb(244, 244, 244);
        Color m_clrBack = Color.White;
        Color m_clrZeroLine = Color.Gray;
        Font m_fontLabels = new Font("Century Gothic", 8.0f, FontStyle.Regular);
        int m_nLookahead = 0;

        public ConfigurationPlotArea()
        {
        }

        public bool Compare(ConfigurationPlotArea c)
        {
            if (m_clrGrid != c.m_clrGrid)
                return false;

            if (m_clrBack != c.m_clrBack)
                return false;

            if (m_clrZeroLine != c.m_clrZeroLine)
                return false;

            if (m_fontLabels.Name != c.m_fontLabels.Name || m_fontLabels.Size != c.m_fontLabels.Size || m_fontLabels.Style != c.m_fontLabels.Style)
                return false;

            if (m_rgTimeZones.Count != c.m_rgTimeZones.Count)
                return false;

            for (int i = 0; i < m_rgTimeZones.Count; i++)
            {
                if (!m_rgTimeZones[i].Compare(c.m_rgTimeZones[i]))
                    return false;
            }

            return true;
        }

        public int Lookahead
        {
            get { return m_nLookahead; }
            set { m_nLookahead = value; }
        }

        public Color GridColor
        {
            get { return m_clrGrid; }
            set { m_clrGrid = value; }
        }

        public Color BackColor
        {
            get { return m_clrBack; }
            set { m_clrBack = value; }
        }

        public Color ZeroLine
        {
            get { return m_clrZeroLine; }
            set { m_clrZeroLine = value; }
        }

        public Font LabelFont
        {
            get { return m_fontLabels; }
            set { m_fontLabels = value; }
        }


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ConfigurationTimeZone> TimeZones
        {
            get { return m_rgTimeZones; }
            set { m_rgTimeZones = value; }
        }


        public void Serialize(SerializeToXml ser)
        {
            ser.Open("PlotArea");
            ser.Add("GridColor", m_clrGrid);
            ser.Add("BackColor", m_clrBack);
            ser.Add("ZeroLineColor", m_clrZeroLine);
            ser.Add("LabelFont", m_fontLabels);

            foreach (ConfigurationTimeZone tz in m_rgTimeZones)
            {
                tz.Serialize(ser);
            }

            ser.Close();
        }
    }
}
