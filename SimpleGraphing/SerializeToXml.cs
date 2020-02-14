using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace SimpleGraphing
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
            for (int i = config.Count-1; i>=0; i--)
            {
                m_rgNodes.Push(config.Item(i));
            }
        }

        public void Save(string strFile)
        {
            m_doc.Save(strFile);
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

        public void Add(string strName, Color clr)
        {
            Add(strName, clr.ToArgb().ToString());
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

        public static Color? LoadColor(XElement elm, string strName)
        {
            string str = LoadText(elm, strName);
            if (str == null)
                return null;

            int nRgb = int.Parse(str);
            return Color.FromArgb(nRgb);
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

        public static Font LoadFont(XElement elm, string strName)
        {
            XElement child = GetElement(elm.Descendants(), strName);
            if (child == null)
                return null;

            string strFont = LoadText(child, "FontName");
            string strSize = LoadText(child, "FontSize");
            string strStyle = LoadText(child, "FontStyle");

            return new Font(strFont, float.Parse(strSize), styleFromText(strStyle));
        }

        private static FontStyle styleFromText(string str)
        {
            if (str == FontStyle.Bold.ToString())
                return FontStyle.Bold;

            if (str == FontStyle.Italic.ToString())
                return FontStyle.Italic;

            return FontStyle.Regular;
        }

        public void Add(string strName, Font font)
        {
            Open(strName);
            Add("FontName", font.Name);
            Add("FontSize", font.Size);
            Add("FontStyle", font.Style.ToString());
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
