using System;
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
    public class ConfigurationTimeZone
    {
        DateTime m_dtStart = DateTime.MinValue;
        DateTime m_dtEnd = DateTime.MaxValue;
        Color m_clrBackground = Color.FromArgb(233, 233, 233);
        bool m_bRelative = false;

        public ConfigurationTimeZone()
        {
        }

        public ConfigurationTimeZone(DateTime dtStart, DateTime dtEnd, Color clr, bool bRelative)
        {
            m_dtStart = dtStart;
            m_dtEnd = dtEnd;
            m_clrBackground = clr;
            m_bRelative = bRelative;
        }

        public DateTime StartTime
        {
            get { return m_dtStart; }
            set { m_dtStart = value; }
        }

        public DateTime EndTime
        {
            get { return m_dtEnd; }
            set { m_dtEnd = value; }
        }

        public Color BackColor
        {
            get { return m_clrBackground; }
            set { m_clrBackground = value; }
        }

        public bool Relative
        {
            get { return m_bRelative; }
            set { m_bRelative = value; }
        }

        public bool Compare(ConfigurationTimeZone t)
        {
            if (m_dtStart != t.m_dtStart)
                return false;

            if (m_dtEnd != t.m_dtEnd)
                return false;

            if (m_clrBackground != t.m_clrBackground)
                return false;

            if (m_bRelative != t.m_bRelative)
                return false;

            return true;
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("TimeZone");
            ser.Add("Start", m_dtStart.ToString());
            ser.Add("End", m_dtEnd.ToString());
            ser.Add("BackColor", m_clrBackground);
            ser.Add("Relative", m_bRelative);
            ser.Close();
        }

        public static List<ConfigurationTimeZone> Deserialize(IEnumerable<XElement> elms)
        {
            List<ConfigurationTimeZone> rgTz = new List<ConfigurationTimeZone>();
            List<XElement> rgElm = SerializeToXml.GetElements(elms, "TargetLine");

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
