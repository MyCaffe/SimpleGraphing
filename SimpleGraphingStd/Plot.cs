﻿// SimpleGraphing Open-Source Project
// License: Apache 2.0. See LICENSE file in root directory.
// https://github.com/MyCaffe/SimpleGraphing/blob/master/LICENSE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphingStd
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

        public Plot(double dfX, double dfY, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_dfX = dfX;
            m_rgfY = new float[1] { (float)dfY };
            m_bActive = bActive;
            m_nIdxPrimaryY = 0;
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public Plot(double dfX, List<double> rgdfY, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_dfX = dfX;

            m_rgfY = new float[rgdfY.Count];
            for (int i = 0; i < rgdfY.Count; i++)
            {
                m_rgfY[i] = (float)rgdfY[i];
            }

            m_bActive = bActive;
            m_nIdxPrimaryY = (short)(rgdfY.Count - 1);
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public Plot(double dfX, List<double> rgdfY, long lCount, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_lCount = lCount;
            m_dfX = dfX;

            m_rgfY = new float[rgdfY.Count];
            for (int i = 0; i < rgdfY.Count; i++)
            {
                m_rgfY[i] = (float)rgdfY[i];
            }

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
            m_bActive = bActive;
            m_nIdxPrimaryY = (short)(rgfY.Length - 1);
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(m_nIndex);
            bw.Write(m_bActive);
            bw.Write(m_bAction1Active);
            bw.Write(m_bAction2Active);
            bw.Write(m_bLookaheadActive);

            bw.Write((m_strName != null) ? true : false);
            if (m_strName != null)
                bw.Write(m_strName);

            int nLen = m_rgfY.Length;
            bw.Write(nLen);
            for (int i = 0; i < m_rgfY.Length; i++)
            {
                double df = m_rgfY[i];
                bw.Write(df);
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

        public static Plot Load(BinaryReader br)
        {
            int nIdx = br.ReadInt32();
            bool bActive = br.ReadBoolean();
            bool bAction1Active = br.ReadBoolean();
            bool bAction2Active = br.ReadBoolean();
            bool bLookaheadActive = br.ReadBoolean();
            string strName = null;

            if (br.ReadBoolean())
                strName = br.ReadString();

            int nCount = br.ReadInt32();
            float[] rgfY = new float[nCount];            
            for (int i = 0; i < nCount; i++)
            {
                double df = br.ReadDouble();
                rgfY[i] = (float)df;
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

            if (m_rgfY.Length != p.m_rgfY.Length)
                return false;

            for (int i = 0; i < m_rgfY.Length; i++)
            {
                if (m_rgfY[i] != p.m_rgfY[i])
                    return false;
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
            m_rgfY[m_nIdxPrimaryY] = fVal;
            if (bActive.HasValue)
                m_bActive = bActive.Value;
        }

        public void SetYValues(float[] rg, bool? bActive = null)
        {
            m_rgfY = rg;

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

        public Plot Clone(bool bCopyData = false, bool bCopyParams = true)
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

        public Plot Clone(List<double> rgY, bool bActive, int nPrimaryIdx, bool bCopyParams = true)
        {
            float[] rgfY = new float[rgY.Count];
            for (int i = 0; i < rgY.Count; i++)
            {
                rgfY[i] = (float)rgY[i];
            }

            return Clone(rgfY, bActive, nPrimaryIdx, bCopyParams);
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
            get { return m_rgfY[m_nIdxPrimaryY]; }
            set { m_rgfY[m_nIdxPrimaryY] = value; }
        }

        public float[] Y_values
        {
            get { return m_rgfY; }
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
