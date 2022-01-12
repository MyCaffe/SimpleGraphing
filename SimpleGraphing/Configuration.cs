using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SimpleGraphing
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Configuration : ISerializable
    {
        ConfigurationSurface m_configSurface = new ConfigurationSurface();
        List<ConfigurationFrame> m_rgFrameConfig = new List<ConfigurationFrame>();

        public Configuration()
        {
        }

        public ConfigurationSurface Surface
        {
            get { return m_configSurface; }
            set { m_configSurface = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ConfigurationFrame> Frames
        {
            get { return m_rgFrameConfig; }
            set { m_rgFrameConfig = value; }
        }

        public Configuration(SerializationInfo info, StreamingContext context)
        {
            m_configSurface = (ConfigurationSurface)info.GetValue("configSurface", typeof(ConfigurationSurface));
            int nCount = info.GetInt32("frameCount");
            m_rgFrameConfig = new List<ConfigurationFrame>();

            for (int i = 0; i < nCount; i++)
            {
                ConfigurationFrame frame = (ConfigurationFrame)info.GetValue("frame_" + i.ToString(), typeof(ConfigurationFrame));
                m_rgFrameConfig.Add(frame);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("configSurface", m_configSurface, typeof(ConfigurationSurface));
            info.AddValue("frameCount", m_rgFrameConfig.Count);

            for (int i = 0; i < m_rgFrameConfig.Count; i++)
            {
                info.AddValue("frame_" + i.ToString(), m_rgFrameConfig[i], typeof(ConfigurationFrame));
            }
        }

        public void SaveToFile(string strFile)
        {
            SerializeToXml ser = new SerializeToXml();

            m_configSurface.Serialize(ser);

            foreach (ConfigurationFrame frame in m_rgFrameConfig)
            {
                frame.Serialize(ser);
            }

            ser.Save(strFile);
        }

        public static Configuration LoadFromFile(string strFile)
        {
            Configuration config = new Configuration();
            string strXml;

            using (FileStream fs = new FileStream(strFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                strXml = sr.ReadToEnd();
            }

            if ((int)strXml[0] == 65533 && (int)strXml[1] == 65533)
                strXml = strXml.Substring(3);

            XDocument doc = XDocument.Parse(strXml);
            
            config.Surface = ConfigurationSurface.Deserialize(doc.Descendants());
            config.Frames = ConfigurationFrame.Deserialize(doc.Descendants());

            return config;
        }

        public void SetLookahead(int nLookahead)
        {
            foreach (ConfigurationFrame frame in m_rgFrameConfig)
            {
                frame.PlotArea.Lookahead = nLookahead;
            }
        }

        public void SetCalculationLookahead(int nLookahead)
        {
            foreach (ConfigurationFrame frame in m_rgFrameConfig)
            {
                frame.PlotArea.CalculationLookahead = nLookahead;
            }
        }
    }
}
