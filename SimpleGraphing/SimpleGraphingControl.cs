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
        List<PlotCollectionSet> m_output;
        Configuration m_config = new Configuration();
        GraphSurface m_surface;
        bool m_bScrolling = false;
        Size m_szOriginal;

        public SimpleGraphingControl()
        {
            InitializeComponent();
            m_cache = new ModuleCache();
            m_surface = new GraphSurface(m_cache);
            m_output = m_surface.BuildGraph(m_config, null);
        }

        public void SetLookahead(int nLookahead)
        {
            foreach (ConfigurationFrame frame in m_config.Frames)
            {
                frame.PlotArea.Lookahead = nLookahead;
            }
        }

        public Bitmap Image
        {
            get
            {
                return new Bitmap(pbImage.Image);
            }
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

        public int VisiblePlotCount
        {
            get
            {
                if (m_config.Frames.Count == 0)
                    return 0;

                return m_surface.Bounds.Width / m_config.Frames[0].XAxis.PlotSpacing;
            }
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
                    PlotCollection framePlots = dataFrame[j];
                    if (framePlots.Count == 0)
                        return null;

                    Plot last = framePlots[framePlots.Count - 1];
                    if (last.Name == null)
                        last.Name = framePlots.Name;

                    plots.Add(last);

                    if (bRemove)
                        framePlots.RemoveAt(framePlots.Count - 1);
                }

                lastData.Add(plots);
            }

            if (bRemove)
            {
                m_output = m_surface.BuildGraph(m_config, m_data);
                SimpleGraphingControl_Resize(this, new EventArgs());
                ScrollToEnd();
            }

            return lastData;
        }

        public List<PlotCollectionSet> GetLastOutput(int nSequenceLength = 1)
        {
            List<PlotCollectionSet> rgOutput = new List<PlotCollectionSet>();

            if (m_output == null || m_output.Count == 0)
                return rgOutput;

            int nCount = m_output[0][0].Count;

            int nStart = nCount - nSequenceLength;
            if (nStart < 0)
            {
                nStart = 0;
                nSequenceLength = nCount;
            }

            for (int k = nStart; k < nStart + nSequenceLength; k++)
            {
                PlotCollectionSet lastData = new PlotCollectionSet();
                List<PlotCollection> rgPlots = new List<PlotCollection>();

                for (int i = 0; i < m_output.Count; i++)
                {
                    PlotCollectionSet dataFrame = m_output[i];

                    if (dataFrame.Count > 0)
                    {
                        PlotCollection plots = new PlotCollection("Frame " + i.ToString());

                        for (int j = 0; j < dataFrame.Count; j++)
                        {
                            PlotCollection framePlots = dataFrame[j];
                            if (framePlots.Count == nCount)
                            {
                                Plot last = framePlots[k];
                                last.Name = framePlots.Name;
                                plots.Add(last);
                            }
                        }

                        lastData.Add(plots);
                    }
                }

                rgOutput.Add(lastData);
            }

            return rgOutput;
        }

        public void AddData(PlotCollectionSet data, bool bMaintainCount)
        {
            if (data.Count != m_data.Count)
                throw new Exception("The number of plot collections must match the number of plot sets used by the graph.");

            List<string> rgUpdated = new List<string>();

            for (int i = 0; i < data.Count; i++)
            {
                PlotCollectionSet dataFrame = m_data[i];

                if (rgUpdated.Contains(dataFrame[0].Name))
                    continue;

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

                rgUpdated.Add(dataFrame[0].Name);
            }

            m_output = m_surface.BuildGraph(m_config, m_data);
            SimpleGraphingControl_Resize(this, new EventArgs());
            ScrollToEnd();
        }

        public void ClearGraph()
        {
            foreach (PlotCollectionSet set in m_data)
            {
                set.ClearData();
            }
        }

        public List<PlotCollectionSet> BuildGraph(List<PlotCollectionSet> data = null)
        {
            if (data != null)
                m_data = data;

            m_output = m_surface.BuildGraph(m_config, m_data);
            SimpleGraphingControl_Resize(this, new EventArgs());

            return m_output;
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
            m_output = m_surface.BuildGraph(m_config, m_data);
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
