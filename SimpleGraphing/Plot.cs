using System;
using System.Collections.Generic;
using System.Linq;
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
        string m_strName;
        List<double> m_rgdfY = new List<double>();
        long? m_lCount = null;
        double m_dfX;
        int m_nIdxPrimaryY = 0;
        object m_tag = null;
        Dictionary<string, double> m_rgParams = null;

        public Plot(double dfX, double dfY, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_dfX = dfX;
            m_rgdfY.Add(dfY);
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
            m_rgdfY = new List<double>(rgdfY);
            m_bActive = bActive;
            m_nIdxPrimaryY = rgdfY.Count - 1;
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public Plot(double dfX, List<double> rgdfY, long lCount, string strName = null, bool bActive = true, int nIdx = 0, bool bAction1Active = false, bool bAction2Active = false)
        {
            m_strName = strName;
            m_lCount = lCount;
            m_dfX = dfX;
            m_rgdfY = new List<double>(rgdfY);
            m_bActive = bActive;
            m_nIdxPrimaryY = rgdfY.Count - 1;
            m_nIndex = nIdx;
            m_bAction1Active = bAction1Active;
            m_bAction2Active = bAction2Active;
        }

        public bool Compare(Plot p)
        {
            if (m_nIndex != p.Index)
                return false;

            if (m_bActive != p.Active)
                return false;

            if (m_bAction1Active != p.Action1Active)
                return false;

            if (m_bAction2Active != p.Action2Active)
                return false;

            if (m_lCount != p.m_lCount)
                return false;

            if (m_dfX != p.X)
                return false;

            if (m_rgdfY.Count != p.m_rgdfY.Count)
                return false;

            for (int i = 0; i < m_rgdfY.Count; i++)
            {
                if (m_rgdfY[i] != p.m_rgdfY[i])
                    return false;
            }

            return true;
        }

        public object Tag
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        public void SetParameter(string strParam, double df)
        {
            if (m_rgParams == null)
                m_rgParams = new Dictionary<string, double>();

            if (!m_rgParams.ContainsKey(strParam))
                m_rgParams.Add(strParam, df);
            else
                m_rgParams[strParam] = df;
        }

        public double? GetParameter(string strParam)
        {
            if (m_rgParams == null)
                m_rgParams = new Dictionary<string, double>();

            if (!m_rgParams.ContainsKey(strParam))
                return null;

            return m_rgParams[strParam];
        }

        public void DeleteParameter(string strParam)
        {
            if (m_rgParams == null)
                return;

            m_rgParams.Remove(strParam);
        }

        public Dictionary<string, double> Parameters
        {
            get { return m_rgParams; }
            set { m_rgParams = value; }
        }

        public Plot Clone()
        {
            return Clone(m_rgdfY, m_bActive, m_nIdxPrimaryY);
        }

        public Plot Clone(List<double> rgY, bool bActive, int nPrimaryIdx)
        {
            Plot p = new Plot(m_dfX, rgY, m_strName, bActive, m_nIndex);
            p.m_nIdxPrimaryY = nPrimaryIdx;
            p.Action1Active = Action1Active;
            p.Action2Active = Action2Active;
            p.LookaheadActive = LookaheadActive;
            p.Count = Count;
            p.m_tag = m_tag;

            if (m_rgParams != null)
            {
                foreach (KeyValuePair<string, double> kv in m_rgParams)
                {
                    p.SetParameter(kv.Key, kv.Value);
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

        public double Y
        {
            get { return m_rgdfY[m_nIdxPrimaryY]; }
            set { m_rgdfY[m_nIdxPrimaryY] = value; }
        }

        public List<double> Y_values
        {
            get { return m_rgdfY; }
        }

        public int PrimaryIndexY
        {
            get { return m_nIdxPrimaryY; }
            set { m_nIdxPrimaryY = value; }
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
    }
}
