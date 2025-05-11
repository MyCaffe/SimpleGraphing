// SimpleGraphing Open-Source Project
// License: Apache 2.0. See LICENSE file in root directory.
// https://github.com/MyCaffe/SimpleGraphing/blob/master/LICENSE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    [Serializable]
    public class Plot
    {
        int m_nIndex = 0;
        bool m_bActive;
        bool m_bAction1Active = false;
        bool m_bAction2Active = false;
        bool m_bLookaheadActive = true;
        bool m_bUseOverrideColors = false;
        string m_strName;
        float[] m_rgfY;
        double[] m_rgdfY;
        long? m_lCount = null;
        double m_dfX;
        short m_nIdxPrimaryY = 0;
        object m_tag = null;
        object m_tagEx = null;
        Dictionary<string, float> m_rgParams = null;
        Dictionary<string, string> m_rgParamsTxt = null;
        bool m_bScaled = false;
        bool m_bClipped = false;
        object m_syncObj = new object();
        BASETYPE m_baseType = BASETYPE.FLOAT;

        public enum BASETYPE
        {
            FLOAT,
            DOUBLE
        }

        public Plot(double dfX, double dfY, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false, BASETYPE baseType = BASETYPE.FLOAT)
        {
            m_strName = strName;
            m_dfX = dfX;
            m_bActive = bActive;
            m_nIdxPrimaryY = 0;
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
            m_baseType = baseType;

            if (baseType == BASETYPE.FLOAT)
                m_rgfY = new float[1] { (float)dfY };
            else
                m_rgdfY = new double[1] { dfY };
        }

        public Plot(double dfX, List<double> rgdfY, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false, BASETYPE baseType = BASETYPE.FLOAT)
        {
            m_strName = strName;
            m_dfX = dfX;

            if (baseType == BASETYPE.FLOAT)
            {
                m_rgfY = new float[rgdfY.Count];
                for (int i = 0; i < rgdfY.Count; i++)
                {
                    m_rgfY[i] = (float)rgdfY[i];
                }
            }
            else
            {
                m_rgdfY = new double[rgdfY.Count];
                for (int i = 0; i < rgdfY.Count; i++)
                {
                    m_rgdfY[i] = rgdfY[i];
                }
            }

            m_baseType = baseType;
            m_bActive = bActive;
            m_nIdxPrimaryY = (short)(rgdfY.Count - 1);
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public Plot(double dfX, List<double> rgdfY, long lCount, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false, BASETYPE baseType = BASETYPE.FLOAT)
        {
            m_strName = strName;
            m_lCount = lCount;
            m_dfX = dfX;

            if (baseType == BASETYPE.FLOAT)
            {
                m_rgfY = new float[rgdfY.Count];
                for (int i = 0; i < rgdfY.Count; i++)
                {
                    m_rgfY[i] = (float)rgdfY[i];
                }
            }
            else
            {
                m_rgdfY = new double[rgdfY.Count];
                for (int i = 0; i < rgdfY.Count; i++)
                {
                    m_rgdfY[i] = rgdfY[i];
                }
            }

            m_baseType = baseType;
            m_bActive = bActive;
            m_nIdxPrimaryY = (short)(rgdfY.Count - 1);
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public Plot(double dfX, float[] rgfY, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_dfX = dfX;
            m_rgfY = rgfY;
            m_baseType = BASETYPE.FLOAT;
            m_bActive = bActive;
            m_nIdxPrimaryY = (short)(rgfY.Length - 1);
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public Plot(double dfX, float[] rgfY, long lCount, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_lCount = lCount;
            m_dfX = dfX;
            m_rgfY = rgfY;
            m_baseType = BASETYPE.FLOAT;
            m_bActive = bActive;
            m_nIdxPrimaryY = (short)(rgfY.Length - 1);
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public Plot(double dfX, double[] rgfY, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_dfX = dfX;
            m_rgdfY = rgfY;
            m_baseType = BASETYPE.DOUBLE;
            m_bActive = bActive;
            m_nIdxPrimaryY = (short)(rgfY.Length - 1);
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public Plot(double dfX, double[] rgfY, long lCount, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_lCount = lCount;
            m_dfX = dfX;
            m_rgdfY = rgfY;
            m_baseType = BASETYPE.DOUBLE;
            m_bActive = bActive;
            m_nIdxPrimaryY = (short)(rgfY.Length - 1);
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public BASETYPE BaseType
        {
            get { return m_baseType; }
        }

        public void Save(BinaryWriter bw)
        {
            string str = "$v.01.01";
            byte[] rgb = Encoding.UTF8.GetBytes(str);
            bw.Write(rgb);
            bw.Write(m_nIndex);
            bw.Write(m_bActive);
            bw.Write(m_bAction1Active);
            bw.Write(m_bAction2Active);
            bw.Write(m_bLookaheadActive);

            bw.Write((m_strName != null) ? true : false);
            if (m_strName != null)
                bw.Write(m_strName);

            int nLen = (m_rgfY == null) ? 0 : m_rgfY.Length;
            bw.Write(nLen);
            if (nLen > 0)
            {
                for (int i = 0; i < m_rgfY.Length; i++)
                {
                    double df = m_rgfY[i];
                    bw.Write(df);
                }
            }

            nLen = (m_rgdfY == null) ? 0 : m_rgdfY.Length;
            bw.Write(nLen);
            if (nLen > 0)
            {
                for (int i = 0; i < m_rgdfY.Length; i++)
                {
                    double df = m_rgdfY[i];
                    bw.Write(df);
                }
            }

            bw.Write((m_lCount.HasValue) ? true : false);
            if (m_lCount.HasValue)
                bw.Write(m_lCount.Value);

            bw.Write(m_dfX);
            int nIdxPrimaryY = m_nIdxPrimaryY;
            bw.Write(nIdxPrimaryY);

            bw.Write((m_tag != null) ? true : false);
            if (m_tag != null)
                bw.Write(m_tag.ToString());

            lock (m_syncObj)
            {
                if (m_rgParams == null)
                {
                    bw.Write(0);
                }
                else
                {
                    bw.Write(m_rgParams.Count);
                    for (int i = 0; i < m_rgParams.Count; i++)
                    {
                        KeyValuePair<string, float> kv = m_rgParams.ElementAt(i);
                        bw.Write(kv.Key);
                        double df = kv.Value;
                        bw.Write(df);
                    }
                }
            }

            bw.Write(m_bScaled);
        }

        public static byte[] PeekBytes(BinaryReader reader, int count)
        {
            // Save the current position
            long originalPosition = reader.BaseStream.Position;

            // Read the bytes
            byte[] bytes = reader.ReadBytes(count);

            // Reset the position back to where it was
            reader.BaseStream.Position = originalPosition;

            return bytes;
        }

        public static Plot Load(BinaryReader br)
        {
            byte[] rgb = PeekBytes(br, 8);
            string strV = Encoding.UTF8.GetString(rgb);
            if (strV == "$v.01.01")
                return LoadEx(br);

            int nIdx = br.ReadInt32();
            bool bActive = br.ReadBoolean();
            bool bAction1Active = br.ReadBoolean();
            bool bAction2Active = br.ReadBoolean();
            bool bLookaheadActive = br.ReadBoolean();
            string strName = null;

            if (br.ReadBoolean())
                strName = br.ReadString();
          
            int nCount = br.ReadInt32();
            float[] rgfY = null;
            if (nCount > 0)
            {
                rgfY = new float[nCount];
                for (int i = 0; i < nCount; i++)
                {
                    double df = br.ReadDouble();
                    rgfY[i] = (float)df;
                }
            }

            long? lCount = null;
            if (br.ReadBoolean())
                lCount = br.ReadInt64();

            double dfX = br.ReadDouble();
            int nIdxPrimaryY = br.ReadInt32();

            object tag = null;
            if (br.ReadBoolean())
            {
                string strTag = br.ReadString();
                DateTime dt;

                if (DateTime.TryParse(strTag, out dt))
                    tag = dt;
                else
                    tag = strTag;
            }

            Dictionary<string, float> rgParam = null;
            nCount = br.ReadInt32();
            if (nCount > 0)
            {
                rgParam = new Dictionary<string, float>();

                for (int i = 0; i < nCount; i++)
                {
                    string str = br.ReadString();
                    double df = br.ReadDouble();
                    rgParam.Add(str, (float)df);
                }
            }

            bool bScaled = br.ReadBoolean();

            Plot p = new Plot(dfX, rgfY, strName, bActive, nIdx, bAction1Active, bAction2Active);
            p.LookaheadActive = bLookaheadActive;
            p.Count = lCount;

            if (tag == null)
            {
                DateTime dt = DateTime.FromFileTimeUtc((long)dfX);
                tag = dt;
            }

            p.Tag = tag;
            p.Parameters = rgParam;
            p.Scaled = bScaled;

            return p;
        }

        public static Plot LoadEx(BinaryReader br)
        {
            byte[] rgb = br.ReadBytes(8);
            string strV = Encoding.UTF8.GetString(rgb);        
            if (strV != "$v.01.01")
                throw new Exception("Invalid version!");
            int nIdx = br.ReadInt32();
            bool bActive = br.ReadBoolean();
            bool bAction1Active = br.ReadBoolean();
            bool bAction2Active = br.ReadBoolean();
            bool bLookaheadActive = br.ReadBoolean();
            string strName = null;

            if (br.ReadBoolean())
                strName = br.ReadString();

            int nCount = br.ReadInt32();
            float[] rgfY = null;
            if (nCount > 0)
            {
                rgfY = new float[nCount];
                for (int i = 0; i < nCount; i++)
                {
                    double df = br.ReadDouble();
                    rgfY[i] = (float)df;
                }
            }

            nCount = br.ReadInt32();
            double[] rgdfY = null;
            if (nCount > 0)
            {
                rgdfY = new double[nCount];
                for (int i = 0; i < nCount; i++)
                {
                    double df = br.ReadDouble();
                    rgdfY[i] = df;
                }
            }

            long? lCount = null;
            if (br.ReadBoolean())
                lCount = br.ReadInt64();

            double dfX = br.ReadDouble();
            int nIdxPrimaryY = br.ReadInt32();

            object tag = null;
            if (br.ReadBoolean())
            {
                string strTag = br.ReadString();
                DateTime dt;

                if (DateTime.TryParse(strTag, out dt))
                    tag = dt;
                else
                    tag = strTag;
            }

            Dictionary<string, float> rgParam = null;
            nCount = br.ReadInt32();
            if (nCount > 0)
            {
                rgParam = new Dictionary<string, float>();

                for (int i = 0; i < nCount; i++)
                {
                    string str = br.ReadString();
                    double df = br.ReadDouble();
                    rgParam.Add(str, (float)df);
                }
            }

            bool bScaled = br.ReadBoolean();

            Plot p = null;

            if (rgfY != null)
                p = new Plot(dfX, rgfY, strName, bActive, nIdx, bAction1Active, bAction2Active);
            else
                p = new Plot(dfX, rgdfY, strName, bActive, nIdx, bAction1Active, bAction2Active);

            p.LookaheadActive = bLookaheadActive;
            p.Count = lCount;

            if (tag == null)
            {
                DateTime dt = DateTime.FromFileTimeUtc((long)dfX);
                tag = dt;
            }

            p.Tag = tag;
            p.Parameters = rgParam;
            p.Scaled = bScaled;

            return p;
        }

        public bool Compare(Plot p, bool bValuesOnly = false)
        {
            if (!bValuesOnly && m_nIndex != p.Index)
                return false;

            if (m_bActive != p.Active)
                return false;

            if (!bValuesOnly && m_bAction1Active != p.Action1Active)
                return false;

            if (!bValuesOnly && m_bAction2Active != p.Action2Active)
                return false;

            if (m_lCount != p.m_lCount)
                return false;

            if (!bValuesOnly && m_dfX != p.X)
                return false;

            if (m_rgfY != null)
            {
                if (m_rgfY.Length != p.m_rgfY.Length)
                    return false;

                for (int i = 0; i < m_rgfY.Length; i++)
                {
                    if (m_rgfY[i] != p.m_rgfY[i])
                        return false;
                }
            }

            if (m_rgdfY != null)
            {
                if (m_rgdfY.Length != p.m_rgdfY.Length)
                    return false;

                for (int i = 0; i < m_rgdfY.Length; i++)
                {
                    if (m_rgdfY[i] != p.m_rgdfY[i])
                        return false;
                }
            }

            return true;
        }

        public bool UseOverrideColors
        {
            get { return m_bUseOverrideColors; }
            set { m_bUseOverrideColors = value; }
        }

        public void SetYValue(float fVal, bool? bActive = null)
        {
            if (m_rgfY != null)
                m_rgfY[m_nIdxPrimaryY] = fVal;
            else
                m_rgdfY[m_nIdxPrimaryY] = fVal;

            if (bActive.HasValue)
                m_bActive = bActive.Value;
        }

        public void SetYValues(float[] rg, bool? bActive = null)
        {
            if (m_baseType == BASETYPE.FLOAT)
            {
                m_rgfY = rg;
            }
            else
            {
                m_rgdfY = new double[rg.Length];
                for (int i = 0; i < rg.Length; i++)
                {
                    m_rgdfY[i] = rg[i];
                }
            }

            if (bActive.HasValue)
                m_bActive = bActive.Value;

            if (m_nIdxPrimaryY > rg.Length)
                m_nIdxPrimaryY = 0;
        }

        public void SetYValue(double dfVal, bool? bActive = null)
        {
            if (m_rgfY != null)
                m_rgfY[m_nIdxPrimaryY] = (float)dfVal;
            else
                m_rgdfY[m_nIdxPrimaryY] = dfVal;

            if (bActive.HasValue)
                m_bActive = bActive.Value;
        }

        public void SetYValues(double[] rg, bool? bActive = null)
        {
            if (m_baseType == BASETYPE.DOUBLE)
            {
                m_rgdfY = rg;
            }
            else
            {
                m_rgfY = new float[rg.Length];
                for (int i = 0; i < rg.Length; i++)
                {
                    m_rgfY[i] = (float)rg[i];
                }
            }

            if (bActive.HasValue)
                m_bActive = bActive.Value;

            if (m_nIdxPrimaryY > rg.Length)
                m_nIdxPrimaryY = 0;
        }

        public bool Scaled
        {
            get { return m_bScaled; }
            set { m_bScaled = value; }
        }

        public bool Clipped
        {
            get { return m_bClipped; }
            set { m_bClipped = value; }
        }

        [IgnoreDataMember]
        public object Tag
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        [IgnoreDataMember]
        public object TagEx
        {
            get { return m_tagEx; }
            set { m_tagEx = value; }
        }

        public void CopyParameters(Plot p)
        {
            if (p.m_rgParams != null)
            {
                if (m_rgParams == null)
                    m_rgParams = new Dictionary<string, float>();

                foreach (KeyValuePair<string, float> kv in p.m_rgParams)
                {
                    m_rgParams[kv.Key] = kv.Value;
                }
            }

            if (p.m_rgParamsTxt != null)
            {
                if (m_rgParamsTxt == null)
                    m_rgParamsTxt = new Dictionary<string, string>();

                foreach (KeyValuePair<string, string> kv in p.m_rgParamsTxt)
                {
                    m_rgParamsTxt[kv.Key] = kv.Value;
                }
            }
        }

        public bool SetParameter(string strParam, double df)
        {
            return SetParameter(strParam, (float)df);
        }

        public bool SetParameter(string strParam, float df)
        {
            lock (m_syncObj)
            {
                if (m_rgParams == null)
                    m_rgParams = new Dictionary<string, float>();

                if (!m_rgParams.ContainsKey(strParam))
                {
                    m_rgParams.Add(strParam, df);
                    return false;
                }
                else
                {
                    m_rgParams[strParam] = df;
                    return true;
                }
            }
        }

        public bool SetParameter(string strParam, string strVal)
        {
            lock (m_syncObj)
            {
                if (m_rgParamsTxt == null)
                    m_rgParamsTxt = new Dictionary<string, string>();
                if (!m_rgParamsTxt.ContainsKey(strParam))
                {
                    m_rgParamsTxt.Add(strParam, strVal);
                    return false;
                }
                else
                {
                    m_rgParamsTxt[strParam] = strVal;
                    return true;
                }
            }
        }

        public void AddToParameter(string strParam, double df)
        {
            AddToParameter(strParam, (float)df);
        }

        public void AddToParameter(string strParam, float df)
        {
            lock (m_syncObj)
            {
                if (m_rgParams == null)
                    m_rgParams = new Dictionary<string, float>();

                if (!m_rgParams.ContainsKey(strParam))
                    m_rgParams.Add(strParam, df);
                else
                    m_rgParams[strParam] += df;
            }
        }

        public float? GetParameter(string strParam)
        {
            lock (m_syncObj)
            {
                if (m_rgParams == null)
                    m_rgParams = new Dictionary<string, float>();

                if (!m_rgParams.ContainsKey(strParam))
                    return null;

                return m_rgParams[strParam];
            }
        }

        public string GetParameterText(string strParam)
        {
            lock (m_syncObj)
            {
                if (m_rgParamsTxt == null)
                    m_rgParamsTxt = new Dictionary<string, string>();
                if (!m_rgParamsTxt.ContainsKey(strParam))
                    return null;
                return m_rgParamsTxt[strParam];
            }
        }

        public float? GetParameterContaining(string strParam)
        {
            lock (m_syncObj)
            {
                if (m_rgParams == null)
                    return null;

                foreach (KeyValuePair<string, float> kv in m_rgParams)
                {
                    if (kv.Key.Contains(strParam))
                        return kv.Value;
                }

                return null;
            }
        }

        public float? GetParameterContaining(string strParam, out string strName)
        {
            lock (m_syncObj)
            {
                strName = null;
                if (m_rgParams == null)
                    return null;

                foreach (KeyValuePair<string, float> kv in m_rgParams)
                {
                    if (kv.Key.Contains(strParam))
                    {
                        strName = kv.Key;
                        return kv.Value;
                    }
                }

                return null;
            }
        }

        public void DeleteParameter(string strParam)
        {
            lock (m_syncObj)
            {
                if (m_rgParams == null)
                    return;

                m_rgParams.Remove(strParam);
            }
        }

        public Dictionary<string, float> Parameters
        {
            get { return m_rgParams; }
            set { m_rgParams = value; }
        }

        public Dictionary<string, string> ParametersText
        {
            get { return m_rgParamsTxt; }
        }

        public string FindParameterNameContaining(string str)
        {
            lock (m_syncObj)
            {
                if (m_rgParams == null || m_rgParams.Count == 0)
                    return null;

                foreach (KeyValuePair<string, float> kv in m_rgParams)
                {
                    string strKey = kv.Key.ToLower();
                    if (strKey.Contains(str.ToLower()))
                        return kv.Key;
                }
            }

            return null;
        }

        public Plot Clone(bool bCopyData = false, bool bCopyParams = true, BASETYPE? baseType = null)
        {
            if (m_baseType == BASETYPE.FLOAT)
            {
                if (baseType == null || baseType == m_baseType)
                {
                    float[] rgf = m_rgfY;

                    if (bCopyData)
                    {
                        rgf = new float[m_rgfY.Length];
                        for (int i = 0; i < m_rgfY.Length; i++)
                        {
                            rgf[i] = m_rgfY[i];
                        }
                    }

                    return Clone(rgf, m_bActive, m_nIdxPrimaryY, bCopyParams);
                }
                else
                {
                    double[] rgdf = new double[m_rgfY.Length];
                    for (int i = 0; i < m_rgfY.Length; i++)
                    {
                        rgdf[i] = m_rgfY[i];
                    }
                    return Clone(rgdf, m_bActive, m_nIdxPrimaryY, bCopyParams);
                }
            }
            else
            {
                if (baseType == null || baseType == m_baseType)
                {
                    double[] rgdf = m_rgdfY;

                    if (bCopyData)
                    {
                        rgdf = new double[m_rgdfY.Length];
                        for (int i = 0; i < m_rgdfY.Length; i++)
                        {
                            rgdf[i] = m_rgdfY[i];
                        }
                    }

                    return Clone(rgdf, m_bActive, m_nIdxPrimaryY, bCopyParams);
                }
                else
                {
                    float[] rgf = new float[m_rgdfY.Length];
                    for (int i = 0; i < m_rgdfY.Length; i++)
                    {
                        rgf[i] = (float)m_rgdfY[i];
                    }
                    return Clone(rgf, m_bActive, m_nIdxPrimaryY, bCopyParams);
                }
            }
        }

        public Plot Clone(List<double> rgY, bool bActive, int nPrimaryIdx, bool bCopyParams = true)
        {
            if (m_baseType == BASETYPE.FLOAT)
            {
                float[] rgfY = new float[rgY.Count];
                for (int i = 0; i < rgY.Count; i++)
                {
                    rgfY[i] = (float)rgY[i];
                }

                return Clone(rgfY, bActive, nPrimaryIdx, bCopyParams);
            }
            else
            {
                double[] rgdfY = new double[rgY.Count];
                for (int i = 0; i < rgY.Count; i++)
                {
                    rgdfY[i] = rgY[i];
                }
                return Clone(rgdfY, bActive, nPrimaryIdx, bCopyParams);
            }
        }

        public Plot Clone(float[] rgY, bool bActive, int nPrimaryIdx, bool bCopyParams = true)
        {
            Plot p = new Plot(m_dfX, rgY, m_strName, bActive, m_nIndex);
            p.m_nIdxPrimaryY = (short)nPrimaryIdx;
            p.Action1Active = Action1Active;
            p.Action2Active = Action2Active;
            p.LookaheadActive = LookaheadActive;
            p.Count = Count;
            p.m_tag = m_tag;
            p.m_tagEx = m_tagEx;
            p.m_nIndex = m_nIndex;

            if (bCopyParams)
            {
                lock (m_syncObj)
                {
                    if (m_rgParams != null)
                    {
                        foreach (KeyValuePair<string, float> kv in m_rgParams)
                        {
                            p.SetParameter(kv.Key, kv.Value);
                        }
                    }

                    if (m_rgParamsTxt != null)
                    {
                        foreach (KeyValuePair<string, string> kv in m_rgParamsTxt)
                        {
                            p.SetParameter(kv.Key, kv.Value);
                        }
                    }
                }
            }

            return p;
        }

        public Plot Clone(double[] rgY, bool bActive, int nPrimaryIdx, bool bCopyParams = true)
        {
            Plot p = new Plot(m_dfX, rgY, m_strName, bActive, m_nIndex);
            p.m_nIdxPrimaryY = (short)nPrimaryIdx;
            p.Action1Active = Action1Active;
            p.Action2Active = Action2Active;
            p.LookaheadActive = LookaheadActive;
            p.Count = Count;
            p.m_tag = m_tag;
            p.m_tagEx = m_tagEx;
            p.m_nIndex = m_nIndex;

            if (bCopyParams)
            {
                lock (m_syncObj)
                {
                    if (m_rgParams != null)
                    {
                        foreach (KeyValuePair<string, float> kv in m_rgParams)
                        {
                            p.SetParameter(kv.Key, kv.Value);
                        }
                    }

                    if (m_rgParamsTxt != null)
                    {
                        foreach (KeyValuePair<string, string> kv in m_rgParamsTxt)
                        {
                            p.SetParameter(kv.Key, kv.Value);
                        }
                    }
                }
            }

            return p;
        }

        public int Index
        {
            get { return m_nIndex; }
            set { m_nIndex = value; }
        }

        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public long? Count
        {
            get { return m_lCount; }
            set { m_lCount = value; }
        }

        public double X
        {
            get { return m_dfX; }
            set { m_dfX = value; }
        }

        public float Y
        {
            get
            {
                if (m_baseType == BASETYPE.FLOAT)
                    return m_rgfY[m_nIdxPrimaryY];
                else
                    return (float)m_rgdfY[m_nIdxPrimaryY];
            }
            set
            {
                if (m_baseType == BASETYPE.FLOAT)
                    m_rgfY[m_nIdxPrimaryY] = value;
                else
                    m_rgdfY[m_nIdxPrimaryY] = value;
            }
        }

        public float[] Y_values
        {
            get
            {
                if (m_baseType == BASETYPE.FLOAT)
                    return m_rgfY;
                else
                {
                    float[] rgf = new float[m_rgdfY.Length];
                    for (int i = 0; i < m_rgdfY.Length; i++)
                    {
                        rgf[i] = (float)m_rgdfY[i];
                    }
                    return rgf;
                }
            }
        }

        public double Yd
        {
            get
            {
                if (m_baseType == BASETYPE.FLOAT)
                    return m_rgfY[m_nIdxPrimaryY];
                else
                    return m_rgdfY[m_nIdxPrimaryY];
            }
            set
            {
                if (m_baseType == BASETYPE.FLOAT)
                    m_rgfY[m_nIdxPrimaryY] = (float)value;
                else
                    m_rgdfY[m_nIdxPrimaryY] = value;
            }
        }

        public double[] Yd_values
        {
            get
            {
                if (m_baseType == BASETYPE.DOUBLE)
                    return m_rgdfY;
                else
                {
                    double[] rgf = new double[m_rgfY.Length];
                    for (int i = 0; i < m_rgfY.Length; i++)
                    {
                        rgf[i] = m_rgfY[i];
                    }
                    return rgf;
                }
            }
        }

        public int PrimaryIndexY
        {
            get { return m_nIdxPrimaryY; }
            set { m_nIdxPrimaryY = (short)value; }
        }

        public bool Active
        {
            get { return m_bActive; }
            set { m_bActive = value; }
        }

        public bool Action1Active
        {
            get { return m_bAction1Active; }
            set { m_bAction1Active = value; }
        }

        public bool Action2Active
        {
            get { return m_bAction2Active; }
            set { m_bAction2Active = value; }
        }

        public bool LookaheadActive
        {
            get { return m_bLookaheadActive; }
            set { m_bLookaheadActive = value; }
        }

        public override string ToString()
        {
            string strTag = (m_tag != null) ? m_tag.ToString() : "";
            string str = m_strName + " " + strTag + " " + m_bActive.ToString() + ", " + m_bAction1Active.ToString() + ", " + m_bAction2Active.ToString() + " { ";

            str += X.ToString() + " }x{ ";

            foreach (double df in Y_values)
            {
                str += df.ToString() + ", ";
            }

            str = str.TrimEnd(',', ' ');
            str += " }";

            if (m_rgParams != null && m_rgParams.Count > 0)
                str += " " + m_rgParams.Count.ToString() + " params";

            return str;
        }

        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                Save(bw);
                ms.Flush();
                return ms.ToArray();
            }
        }

        public static Plot FromBytes(byte[] rg)
        {
            using (MemoryStream ms = new MemoryStream(rg))
            using (BinaryReader br = new BinaryReader(ms))
            {
                return Load(br);
            }
        }
    }
}
