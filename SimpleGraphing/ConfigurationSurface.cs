using System;
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
    public class ConfigurationSurface
    {
        Color m_clrBack = Color.SkyBlue;
        bool m_bEnableSmoothing = true;

        public ConfigurationSurface()
        {
        }

        public Color BackColor
        {
            get { return m_clrBack; }
            set { m_clrBack = value; }
        }

        public bool EnableSmoothing
        {
            get { return m_bEnableSmoothing; }
            set { m_bEnableSmoothing = value; }
        }

        public bool Compare(ConfigurationSurface c)
        {
            if (m_clrBack != c.m_clrBack)
                return false;

            if (m_bEnableSmoothing != c.m_bEnableSmoothing)
                return false;

            return true;
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("Surface");
            ser.Add("BackColor", m_clrBack);
            ser.Add("EnableSmoothing", m_bEnableSmoothing.ToString());
            ser.Close();
        }
    }
}
