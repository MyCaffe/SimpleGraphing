using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using SkiaSharp;

namespace SimpleGraphingStd
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationTimeZone
    {
        DateTime m_dtStart = DateTime.MinValue;
        DateTime m_dtEnd = DateTime.MaxValue;
        SKColor m_clrBackground = new SKColor(233, 233, 233);
        bool m_bRelative = false;

        public ConfigurationTimeZone() { }

        public ConfigurationTimeZone(ConfigurationTimeZone zone)
        {
            m_dtStart = zone.m_dtStart;
            m_dtEnd = zone.m_dtEnd;
            m_clrBackground = zone.m_clrBackground;
            m_bRelative = zone.m_bRelative;
        }

        public ConfigurationTimeZone(DateTime dtStart, DateTime dtEnd, SKColor clr, bool bRelative)
        {
            m_dtStart = dtStart;
            m_dtEnd = dtEnd;
            m_clrBackground = clr;
            m_bRelative = bRelative;
        }

        public DateTime StartTime
        {
            get => m_dtStart;
            set => m_dtStart = value;
        }

        public DateTime EndTime
        {
            get => m_dtEnd;
            set => m_dtEnd = value;
        }

        public SKColor BackColor
        {
            get => m_clrBackground;
            set => m_clrBackground = value;
        }

        public bool Relative
        {
            get => m_bRelative;
            set => m_bRelative = value;
        }

        public bool Compare(ConfigurationTimeZone t)
        {
            return m_dtStart == t.m_dtStart &&
                   m_dtEnd == t.m_dtEnd &&
                   m_clrBackground == t.m_clrBackground &&
                   m_bRelative == t.m_bRelative;
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("TimeZone");
            ser.Add("Start", m_dtStart.ToString("o")); // ISO 8601 format
            ser.Add("End", m_dtEnd.ToString("o"));
            ser.Add("BackColor", m_clrBackground);
            ser.Add("Relative", m_bRelative);
            ser.Close();
        }

        public static List<ConfigurationTimeZone> Deserialize(IEnumerable<XElement> elms)
        {
            List<ConfigurationTimeZone> rgTz = new List<ConfigurationTimeZone>();
            List<XElement> rgElm = SerializeToXml.GetElements(elms, "TimeZone");

            foreach (XElement elm in rgElm)
            {
                ConfigurationTimeZone tz = ConfigurationTimeZone.Deserialize(elm);
                rgTz.Add(tz);
            }

            return rgTz;
        }

        public static ConfigurationTimeZone Deserialize(XElement elm)
        {
            ConfigurationTimeZone tz = new ConfigurationTimeZone();

            tz.StartTime = SerializeToXml.LoadDateTime(elm, "Start").Value;
            tz.EndTime = SerializeToXml.LoadDateTime(elm, "End").Value;
            tz.BackColor = SerializeToXml.LoadColor(elm, "BackColor").Value;
            tz.Relative = SerializeToXml.LoadBool(elm, "Relative").Value;

            return tz;
        }
    }
}
