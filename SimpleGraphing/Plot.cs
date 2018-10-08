using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public class Plot
    {
        bool m_bActive;
        string m_strName;
        double m_dfY;
        double m_dfX;

        public Plot(double dfX, double dfY, string strName = null, bool bActive = true)
        {
            m_strName = strName;
            m_dfX = dfX;
            m_dfY = dfY;
            m_bActive = bActive;
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
            get { return m_dfY; }
            set { m_dfY = value; }
        }

        public bool Active
        {
            get { return m_bActive; }
            set { m_bActive = value; }
        }
    }
}
