using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using SkiaSharp;

namespace SimpleGraphingStd
{
    public class SerializeToXml
    {
        XmlDocument m_doc;
        XmlElement m_root;
        Stack<XmlElement> m_rgElm = new Stack<XmlElement>();
        Stack<XmlNode> m_rgNodes = new Stack<XmlNode>();

        public SerializeToXml()
        {
            m_doc = new XmlDocument();

            XmlDeclaration dec = m_doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            m_root = m_doc.DocumentElement;
            m_doc.InsertBefore(dec, m_root);

            if (m_root == null)
            {
                m_root = m_doc.CreateElement("Configuration");
                m_doc.AppendChild(m_root);
            }

            m_rgElm.Push(m_root);
        }

        public SerializeToXml(string strFile)
        {
            m_doc = new XmlDocument();
            m_doc.Load(strFile);

            XmlNodeList config = m_doc.GetElementsByTagName("Configuration");
            for (int i = config.Count - 1; i >= 0; i--)
            {
                m_rgNodes.Push(config.Item(i));
            }
        }

        public void Save(string strFile)
        {
            string strXml = m_doc.InnerXml;

            using (StreamWriter sw = new StreamWriter(strFile, false, new UTF8Encoding(false)))
            {
                sw.Write(strXml);
            }
        }

        public void Open(string strName)
        {
            XmlElement parent = m_rgElm.Peek();
            XmlElement elm = m_doc.CreateElement(strName);
            parent.AppendChild(elm);
            m_rgElm.Push(elm);
        }

        public void Close()
        {
            m_rgElm.Pop();
        }

        public void Add(string strName, string str)
        {
            XmlElement parent = m_rgElm.Peek();

            XmlElement elm = m_doc.CreateElement(strName);
            XmlNode val = m_doc.CreateTextNode(str);
            elm.AppendChild(val);

            parent.AppendChild(elm);
        }

        public void Add(string strName, SKColor clr)
        {
            Add(strName, clr.ToString()); // Converting SKColor to its string representation
        }

        public static List<XElement> GetElements(IEnumerable<XElement> elms, string strName)
        {
            return elms.Where(p => p.Name == strName).ToList();
        }

        public static XElement GetElement(IEnumerable<XElement> elms, string strName)
        {
            List<XElement> rgElm = GetElements(elms, strName);
            if (rgElm.Count == 0)
                return null;

            return rgElm[0];
        }

        public static string LoadText(XElement elm, string strName)
        {
            XElement child = GetElement(elm.Descendants(), strName);
            if (child == null)
                return null;

            return child.Value;
        }

        public static DateTime? LoadDateTime(XElement elm, string strName)
        {
            string str = LoadText(elm, strName);
            if (str == null)
                return null;

            return DateTime.Parse(str);
        }

        public static SKColor? LoadColor(XElement elm, string strName)
        {
            string str = LoadText(elm, strName);
            if (str == null)
                return null;

            // Assuming the color is stored as an integer in the XML
            if (int.TryParse(str, out int argb))
            {
                return new SKColor((uint)argb);
            }

            // Fallback to parsing the string as a color (in case it's not an integer)
            if (SKColor.TryParse(str, out SKColor color))
            {
                return color;
            }

            return null; // or throw an exception if appropriate
        }

        public static bool? LoadBool(XElement elm, string strName)
        {
            string str = LoadText(elm, strName);
            if (str == null)
                return null;

            return bool.Parse(str);
        }

        public static int? LoadInt(XElement elm, string strName)
        {
            string str = LoadText(elm, strName);
            if (str == null)
                return null;

            return int.Parse(str);
        }

        public static double? LoadDouble(XElement elm, string strName)
        {
            string str = LoadText(elm, strName);
            if (str == null)
                return null;

            if (str == "1.79769313486232E+308")
                return double.MaxValue;

            if (str == "-1.79769313486232E+308")
                return double.MinValue;

            return double.Parse(str);
        }

        public static SKFont LoadFont(XElement elm, string strName)
        {
            XElement child = GetElement(elm.Descendants(), strName);
            if (child == null)
                return null;

            string fontFamily = LoadText(child, "FontName");
            string fontSize = LoadText(child, "FontSize");
            string fontStyle = LoadText(child, "FontStyle");

            return new SKFont(SKTypeface.FromFamilyName(fontFamily), float.Parse(fontSize)); // SkiaSharp font handling
        }

        public void Add(string strName, SKFont font)
        {
            Open(strName);
            Add("FontName", font.Typeface.FamilyName);
            Add("FontSize", font.Size);
            // SkiaSharp SKFont does not have FontStyle directly, so adapt as needed
            Add("FontStyle", "Regular");
            Close();
        }

        public void Add(string strName, int nVal)
        {
            Add(strName, nVal.ToString());
        }

        public void Add(string strName, float fVal)
        {
            Add(strName, fVal.ToString());
        }

        public void Add(string strName, double dfVal)
        {
            Add(strName, dfVal.ToString());
        }

        public void Add(string strName, bool bVal)
        {
            Add(strName, bVal.ToString());
        }
    }
}
