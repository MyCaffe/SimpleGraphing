using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleGraphing
{
    public class SerializeToXml
    {
        XmlDocument m_doc;
        XmlElement m_root;
        Stack<XmlElement> m_rgElm = new Stack<XmlElement>();

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
