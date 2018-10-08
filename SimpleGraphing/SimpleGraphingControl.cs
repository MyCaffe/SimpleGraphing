using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleGraphing
{
    public partial class SimpleGraphingControl : UserControl
    {
        Configuration m_config = new Configuration();
        GraphSurface m_surface = new GraphSurface();
        bool m_bScrolling = false;
        Size m_szOriginal;

        public SimpleGraphingControl()
        {
            InitializeComponent();
            m_surface.BuildGraph(m_config, null);
        }

        public Configuration Configuration
        {
            get { return m_config; }
            set { m_config = value; }
        }

        public void BuildGraph(List<PlotCollectionSet> data)
        {
            m_surface.BuildGraph(m_config, data);
            SimpleGraphingControl_Resize(this, new EventArgs());
        }

        public bool ShowScrollBar
        {
            get { return hScrollBar1.Visible; }
            set
            {
                hScrollBar1.Visible = value;

                if (!value)
                    pbImage.Size = Size;
                else
                    pbImage.Size = m_szOriginal;

                m_surface.Resize(pbImage.Width, pbImage.Height);
            }
        }

        private void SimpleGraphingControl_Resize(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            m_surface.Resize(pbImage.Width, pbImage.Height);
            m_szOriginal = pbImage.Size;
        }

        private void SimpleGraphingControl_Paint(object sender, PaintEventArgs e)
        {
            if (DesignMode)
                return;

            pbImage.Image = m_surface.Render();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (DesignMode)
                return;

            if (m_bScrolling)
                return;

            try
            {
                m_bScrolling = true;

                double dfScrollPct = e.NewValue / (double)(hScrollBar1.Maximum - (hScrollBar1.LargeChange - 1));

                m_surface.Scroll(dfScrollPct);
                m_surface.Resize(pbImage.Width, pbImage.Height);

                pbImage.Image = m_surface.Render();
            }
            finally
            {
                m_bScrolling = false;
            }
        }

        public void ScrollToEnd()
        {
            hScrollBar1.Value = hScrollBar1.Maximum;
            m_surface.Scroll(1.0);
            m_surface.Resize(pbImage.Width, pbImage.Height);

            pbImage.Image = m_surface.Render();
        }
    }
}
