using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using SkiaSharp;

namespace SimpleGraphingStd
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConfigurationSurface
    {
        SKColor m_clrCrossHair = new SKColor(0, 0, 255, 64);
        SKColor m_clrBack = SKColors.SkyBlue;
        bool m_bEnableSmoothing = true;
        bool m_bStyleDirty = false;

        public ConfigurationSurface() { }

        public void ClearStyleDirty()
        {
            m_bStyleDirty = false;
        }

        public bool IsStyleDirty => m_bStyleDirty;

        public SKColor CrossHairColor
        {
            get => m_clrCrossHair;
            set
            {
                m_clrCrossHair = value;
                m_bStyleDirty = true;
            }
        }

        public SKColor BackColor
        {
            get => m_clrBack;
            set
            {
                m_clrBack = value;
                m_bStyleDirty = true;
            }
        }

        public bool EnableSmoothing
        {
            get => m_bEnableSmoothing;
            set => m_bEnableSmoothing = value;
        }

        public bool Compare(ConfigurationSurface c)
        {
            return m_clrBack == c.m_clrBack &&
                   m_clrCrossHair == c.m_clrCrossHair &&
                   m_bEnableSmoothing == c.m_bEnableSmoothing;
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

            SKColor? clr = SerializeToXml.LoadColor(elm, "CrossHairColor");
            if (clr.HasValue)
                surface.m_clrCrossHair = clr.Value;

            return surface;
        }
    }
}
