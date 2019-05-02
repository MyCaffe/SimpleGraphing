﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class Plot
    {
        int m_nIndex = 0;
        bool m_bActive;
        bool m_bActionActive = false;
        bool m_bLookaheadActive = true;
        string m_strName;
        List<double> m_rgdfY = new List<double>();
        double m_dfX;
        int m_nIdxPrimaryY = 0;
        object m_tag = null;

        public Plot(double dfX, double dfY, string strName = null, bool bActive = true, int nIdx = 0, bool bActionActive = false)
        {
            m_strName = strName;
            m_dfX = dfX;
            m_rgdfY.Add(dfY);
            m_bActive = bActive;
            m_nIdxPrimaryY = 0;
            m_nIndex = nIdx;
            m_bActionActive = bActionActive;
        }

        public Plot(double dfX, List<double> rgdfY, string strName = null, bool bActive = true, int nIdx = 0, bool bActionActive = false)
        {
            m_strName = strName;
            m_dfX = dfX;
            m_rgdfY = new List<double>(rgdfY);
            m_bActive = bActive;
            m_nIdxPrimaryY = rgdfY.Count - 1;
            m_nIndex = nIdx;
            m_bActionActive = bActionActive;
        }

        public bool Compare(Plot p)
        {
            if (m_nIndex != p.Index)
                return false;

            if (m_bActive != p.Active)
                return false;

            if (m_bActionActive != p.ActionActive)
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

        public Plot Clone(List<double> rgY, bool bActive, int nPrimaryIdx)
        {
            Plot p = new Plot(m_dfX, rgY, m_strName, bActive, m_nIndex);
            p.m_nIdxPrimaryY = nPrimaryIdx;
            p.ActionActive = ActionActive;
            p.LookaheadActive = LookaheadActive;
            p.m_tag = m_tag;
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

        public bool ActionActive
        {
            get { return m_bActionActive; }
            set { m_bActionActive = value; }
        }

        public bool LookaheadActive
        {
            get { return m_bLookaheadActive; }
            set { m_bLookaheadActive = value; }
        }

        public override string ToString()
        {
            string strTag = (m_tag != null) ? m_tag.ToString() : "";
            string str = m_strName + " " + strTag + " " + m_bActive.ToString() + ", " + m_bActionActive.ToString() + " { ";

            str += X.ToString() + " }x{ ";

            foreach (double df in Y_values)
            {
                str += df.ToString() + ", ";
            }

            str = str.TrimEnd(',', ' ');
            str += " }";

            return str;
        }
    }
}
