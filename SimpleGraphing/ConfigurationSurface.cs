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
    public class ConfigurationSurface
    {
        Color m_clrCrossHair = Color.FromArgb(64, 0, 0, 255);
        Color m_clrBack = Color.SkyBlue;
        bool m_bEnableSmoothing = true;
        bool m_bStyleDirty = false;

        public ConfigurationSurface()
        {
        }

        public void ClearStyleDirty()
        {
            m_bStyleDirty = false;
        }

        public bool IsStyleDirty
        {
            get { return m_bStyleDirty; }
        }

        public Color CrossHairColor
        {
            get { return m_clrCrossHair; }
            set 
            { 
                m_clrCrossHair = value;
                m_bStyleDirty = true;
            }
        }

        public Color BackColor
        {
            get { return m_clrBack; }
            set 
            { 
                m_clrBack = value;
                m_bStyleDirty = true;
            }
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

            if (m_clrCrossHair != c.m_clrCrossHair)
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
            ser.Add("CrossHairColor", m_clrCrossHair);
            ser.Close();
        }

        public static ConfigurationSurface Deserialize(IEnumerable<XElement> elms)
        {
            ConfigurationSurface surface = new ConfigurationSurface();

            XElement elm = SerializeToXml.GetElement(elms, "Surface");
            surface.m_clrBack = SerializeToXml.LoadColor(elm, "BackColor").Value;
            surface.m_bEnableSmoothing = SerializeToXml.LoadBool(elm, "EnableSmoothing").Value;
            Color? clr = SerializeToXml.LoadColor(elm, "CrossHairColor").Value;
            if (clr.HasValue)
                surface.m_clrCrossHair = clr.Value;

            return surface;
        }
    }
}
