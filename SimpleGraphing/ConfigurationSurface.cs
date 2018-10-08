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

        public ConfigurationSurface()
        {
        }

        public Color BackColor
        {
            get { return m_clrBack; }
            set { m_clrBack = value; }
        }

        public bool Compare(ConfigurationSurface c)
        {
            if (m_clrBack != c.m_clrBack)
                return false;

            return true;
        }

        public void Serialize(SerializeToXml ser)
        {
            ser.Open("Surface");
            ser.Add("BackColor", m_clrBack);
            ser.Close();
        }
    }
}
