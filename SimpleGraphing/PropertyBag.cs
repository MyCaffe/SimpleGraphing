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
        object m_objValue = null;
        bool m_bDirty = false;

        public PropertyValue(string strName = "", double dfVal = 0, string strValue = null)
        {
            m_strName = strName;
            m_dfVal = dfVal;
            m_strValue = strValue;
        }

        public bool IsDirty
        {
            get { return m_bDirty; }
        }

        public void ClearDirty()
        {
            m_bDirty = false;
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public double Value
        {
            get { return m_dfVal; }
            set 
            { 
                m_dfVal = value;
                m_bDirty = true;
            }
        }

        public object GenericValue
        {
            get { return m_objValue; }
            set 
            { 
                m_objValue = value;
                m_bDirty = true;
            }
        }

        public string TextValue
        {
            get { return m_strValue; }
            set 
            { 
                m_strValue = value;
                m_bDirty = true;
            }
        }

        public bool Compare(PropertyValue pv)
        {
            if (m_strName != pv.m_strName)
                return false;

            if (m_dfVal != pv.m_dfVal)
                return false;

            if (m_strValue != pv.m_strValue)
                return false;

            return true;
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

        public string ToParseString()
        {
            return m_strName + "^" + m_dfVal.ToString() + "^" + m_strName;
        }

        public static PropertyValue FromParseString(string str)
        {
            string[] rgstr = str.Split('^');
            if (rgstr.Length != 3)
                throw new Exception("String format incorrect, expected 'name~double~string'");

            double dfVal = 0;
            double.TryParse(rgstr[1], out dfVal);

            return new PropertyValue(rgstr[0], dfVal, rgstr[2]);
        }
    }

    public class PropertyBag : IEnumerable<PropertyValue>
    {
        List<PropertyValue> m_rgProperties = new List<PropertyValue>();

        public PropertyBag()
        {
        }

        public bool Compare(PropertyBag pb)
        {
            if (m_rgProperties.Count != pb.Count)
                return false;

            for (int i = 0; i < m_rgProperties.Count; i++)
            {
                if (!m_rgProperties[i].Compare(pb.m_rgProperties[i]))
                    return false;
            }

            return true;
        }

        public bool IsDirty
        {
            get
            {
                foreach (PropertyValue pv in m_rgProperties)
                {
                    if (pv.IsDirty)
                        return true;
                }

                return false;
            }
        }

        public void ClearDirty()
        {
            foreach (PropertyValue pv in m_rgProperties)
            {
                pv.ClearDirty();
            }
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

        public string GetProperty(string strName, string strDefault)
        {
            PropertyValue val = Find(strName);
            if (val == null)
                return strDefault;

            return val.TextValue;
        }

        public void SetProperty(string strName, double dfVal)
        {
            PropertyValue val = Find(strName);
            if (val == null)
                Add(strName, dfVal);
            else
                val.Value = dfVal;
        }

        public void SetProperty(string strName, string strVal)
        {
            PropertyValue val = Find(strName);
            if (val == null)
                Add(strName, strVal);
            else
                val.TextValue = strVal;
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

        public void Add(string strName, string strVal)
        {
            Add(new PropertyValue(strName, 0, strVal));
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

        public string ToParseString()
        {
            string str = "";

            foreach (PropertyValue val in m_rgProperties)
            {
                str += val.ToParseString() + "$";
            }

            return str.TrimEnd('$');
        }

        public static PropertyBag FromParseString(string str)
        {
            PropertyBag bag = new PropertyBag();
            if (!string.IsNullOrEmpty(str))
            {
                string[] rgstr = str.Split('$');

                foreach (string str1 in rgstr)
                {
                    if (!string.IsNullOrEmpty(str1))
                    {
                        PropertyValue val = PropertyValue.FromParseString(str1);
                        bag.Add(val);
                    }
                }
            }

            return bag;
        }
    }
}
