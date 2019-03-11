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
        ModuleCache m_cache;
        List<PlotCollectionSet> m_data;
        Configuration m_config = new Configuration();
        GraphSurface m_surface;
        bool m_bScrolling = false;
        Size m_szOriginal;

        public SimpleGraphingControl()
        {
            InitializeComponent();
            m_cache = new ModuleCache();
            m_surface = new GraphSurface(m_cache);
            m_surface.BuildGraph(m_config, null);
        }

        public List<string> LoadModuleCache()
        {
            return m_cache.Load();
        }

        public ModuleCache CustomModules
        {
            get { return m_cache; }
        }

        public Configuration Configuration
        {
            get { return m_config; }
            set { m_config = value; }
        }

        public PlotCollectionSet GetLastData(bool bRemove = false)
        {
            if (m_data.Count == 0)
                return null;

            PlotCollectionSet lastData = new PlotCollectionSet();
            List<PlotCollection> rgPlots = new List<PlotCollection>();

            for (int i = 0; i < m_data.Count; i++)
            {
                PlotCollectionSet dataFrame = m_data[i];

                if (dataFrame.Count == 0)
                    return null;

                PlotCollection plots = new PlotCollection("Frame " + i.ToString());

                for (int j = 0; j < dataFrame.Count; j++)
                {
                    PlotCollection framePlots = dataFrame[dataFrame.Count - 1];
                    if (framePlots.Count == 0)
                        return null;

                    plots.Add(framePlots[framePlots.Count - 1]);

                    if (bRemove)
                        framePlots.RemoveAt(framePlots.Count - 1);
                }

                lastData.Add(plots);
            }

            if (bRemove)
            {
                m_surface.BuildGraph(m_config, m_data);
                SimpleGraphingControl_Resize(this, new EventArgs());
                ScrollToEnd();
            }

            return lastData;
        }

        public void AddData(PlotCollectionSet data, bool bMaintainCount)
        {
            if (data.Count != m_data.Count)
                throw new Exception("The number of plot collections must match the number of plot sets used by the graph.");

            for (int i = 0; i < data.Count; i++)
            {
                PlotCollectionSet dataFrame = m_data[i];
                PlotCollection dataToAdd = data[i];

                if (dataFrame.Count != dataToAdd.Count)
                    throw new Exception("The number of data items to add must match the number of plot collections in the frame!");

                for (int j = 0; j < dataFrame.Count; j++)
                {
                    PlotCollection dataFrameItems = dataFrame[j];
                    Plot plot = dataToAdd[j];

                    dataFrameItems.Add(plot);

                    if (bMaintainCount)
                        dataFrameItems.RemoveAt(0);
                }
            }

            m_surface.BuildGraph(m_config, m_data);
            SimpleGraphingControl_Resize(this, new EventArgs());
            ScrollToEnd();
        }

        public void BuildGraph(List<PlotCollectionSet> data)
        {
            m_data = data;
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

        public void UpdateGraph()
        {
            m_surface.BuildGraph(m_config, m_data);
            SimpleGraphingControl_Resize(this, new EventArgs());
            Invalidate(true);
        }

        private void SimpleGraphingControl_Resize(object sender, EventArgs e)
        {
            if (DesignMode || m_surface == null)
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
