using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class PropertyValue
    {
        string m_strName;
        double m_dfVal;
        string m_strValue;

        public PropertyValue(string strName = "", double dfVal = 0, string strValue = null)
        {
            m_strName = strName;
            m_dfVal = dfVal;
            m_strValue = strValue;
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public double Value
        {
            get { return m_dfVal; }
            set { m_dfVal = value; }
        }

        public string TextValue
        {
            get { return m_strValue; }
            set { m_strValue = value; }
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(m_strName);
            bw.Write(m_dfVal);

            bool bEmpty = string.IsNullOrEmpty(m_strValue);
            bw.Write(bEmpty);
            if (!bEmpty)
                bw.Write(m_strValue);
        }

        public static PropertyValue Load(BinaryReader br)
        {
            string strName = br.ReadString();
            double dfVal = br.ReadDouble();
            bool bEmpty = br.ReadBoolean();
            string strVal = null;

            if (!bEmpty)
                strVal = br.ReadString();

            return new PropertyValue(strName, dfVal, strVal);
        }

        public override string ToString()
        {
            return m_strName + " => " + m_dfVal.ToString() + ((m_strValue == null) ? "" : ", " + m_strValue.ToString());
        }
    }

    public class PropertyBag : IEnumerable<PropertyValue>
    {
        List<PropertyValue> m_rgProperties = new List<PropertyValue>();

        public PropertyBag()
        {
        }

        public int Count
        {
            get { return m_rgProperties.Count; }
        }

        public PropertyValue this[int nIdx]
        {
            get { return m_rgProperties[nIdx]; }
            set { m_rgProperties[nIdx] = value; }
        }

        public double GetProperty(string strName, double dfDefault)
        {
            PropertyValue val = Find(strName);
            if (val == null)
                return dfDefault;

            return val.Value;
        }

        public PropertyValue Find(string strName)
        {
            foreach (PropertyValue val in m_rgProperties)
            {
                if (val.Name == strName)
                    return val;
            }

            return null;
        }

        public void Add(string strName, double dfVal)
        {
            Add(new PropertyValue(strName, dfVal));
        }

        public void Add(PropertyValue val)
        {
            PropertyValue existing = Find(val.Name);
            if (existing != null)
            {
                existing.Value = val.Value;
                return;
            }

            m_rgProperties.Add(val);
        }

        public bool Remove(PropertyValue val)
        {
            return m_rgProperties.Remove(val);
        }

        public void RemoveAt(int nIdx)
        {
            m_rgProperties.RemoveAt(nIdx);
        }

        public void Clear()
        {
            m_rgProperties.Clear();
        }

        public IEnumerator<PropertyValue> GetEnumerator()
        {
            return m_rgProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_rgProperties.GetEnumerator();
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(Count);

            foreach (PropertyValue val in m_rgProperties)
            {
                val.Save(bw);
            }
        }

        public static PropertyBag Load(BinaryReader br)
        {
            PropertyBag set = new PropertyBag();

            int nCount = br.ReadInt32();
            for (int i = 0; i < nCount; i++)
            {
                set.m_rgProperties.Add(PropertyValue.Load(br));
            }

            return set;
        }
    }

}
