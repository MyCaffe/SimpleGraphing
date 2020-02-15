using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
        Crosshairs m_crosshairs = new Crosshairs();
        double m_dfScrollPct = 0;

        public event EventHandler<PaintEventArgs> OnUserPaint;
        public event EventHandler<MouseEventArgs> OnUserMouseMove;
        public event EventHandler<ScrollEventArgs> OnUserScroll;

        public SimpleGraphingControl()
        {
            InitializeComponent();
            m_cache = new ModuleCache();
            m_surface = new GraphSurface(m_cache);
            m_output = m_surface.BuildGraph(m_config, null);
        }

        public void SaveConfiguration(string strFile)
        {
            string strExt = Path.GetExtension(strFile).ToLower();

            if (strExt == ".xml")
            {
                Configuration.SaveToFile(strFile);
            }
            else
            {
                IFormatter formatter = new BinaryFormatter();

                using (Stream strm = new FileStream(strFile, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(strm, Configuration);
                }
            }
        }

        public void LoadConfiguration(string strFile)
        {
            string strExt = Path.GetExtension(strFile).ToLower();
            bool bLoaded = false;

            if (strExt == ".cfg")
            {
                try
                {
                    IFormatter formatter = new BinaryFormatter();

                    using (Stream strm = new FileStream(strFile, FileMode.Open, FileAccess.Read))
                    {
                        Configuration = formatter.Deserialize(strm) as Configuration;
                    }

                    bLoaded = true;
                }
                catch
                {
                }
            }

            if (!bLoaded)
            {
                if (strExt != ".xml")
                    strFile = Path.GetDirectoryName(strFile) + "\\" + Path.GetFileNameWithoutExtension(strFile) + ".xml";

                Configuration = Configuration.LoadFromFile(strFile);
            }
        }

        public void SetLookahead(int nLookahead)
        {
            foreach (ConfigurationFrame frame in m_config.Frames)
            {
                frame.PlotArea.Lookahead = nLookahead;
            }
        }

        public bool UserUpdateCrosshairs
        {
            get { return m_crosshairs.UserUpdate; }
            set { m_crosshairs.UserUpdate = value; }
        }

        public bool EnableCrossHairs
        {
            get { return m_crosshairs.EnableCrosshairs; }
            set { m_crosshairs.EnableCrosshairs = value; }
        }

        public Bitmap Image
        {
            get { return new Bitmap(pbImage.Image); }
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

        public PlotCollectionSet GetLastData(int nLookahead = 0, bool bRemove = false)
        {
            if (m_data.Count == 0)
                return null;

            if (nLookahead > 0 && bRemove)
                throw new Exception("Removing data is not supported when retrieving data with a lookahead.");

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

                    Plot last = framePlots[framePlots.Count - (1 + nLookahead)];
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
                ScrollToEnd(false);
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

        public void AddData(PlotCollectionSet data, bool bMaintainCount, bool bRender = false)
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
            ScrollToEnd(bRender);
        }

        public void ClearGraph()
        {
            if (m_data == null)
                return;

            foreach (PlotCollectionSet set in m_data)
            {
                set.ClearData();
            }
        }

        public List<PlotCollectionSet> BuildGraph(List<PlotCollectionSet> data = null, bool bResize = true, bool bAddToParams = false)
        {
            if (data != null)
                m_data = data;

            m_output = m_surface.BuildGraph(m_config, m_data, bAddToParams);

            if (bResize)
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

                m_dfScrollPct = e.NewValue / (double)(hScrollBar1.Maximum - (hScrollBar1.LargeChange - 1));

                m_surface.Scroll(m_dfScrollPct);
                m_surface.Resize(pbImage.Width, pbImage.Height);

                pbImage.Image = m_surface.Render();

                if (OnUserScroll != null)
                    OnUserScroll(sender, e);
            }
            finally
            {
                m_bScrolling = false;
            }
        }

        public double ScrollPercent
        {
            get { return m_dfScrollPct; }
            set
            {
                m_dfScrollPct = value;
                m_surface.Scroll(m_dfScrollPct);
                m_surface.Resize(pbImage.Width, pbImage.Height);
                pbImage.Image = m_surface.Render();
            }
        }

        public Image ScrollToEnd(bool bRender)
        {
            hScrollBar1.Value = hScrollBar1.Maximum;
            m_surface.Scroll(1.0);
            m_surface.Resize(pbImage.Width, pbImage.Height);

            if (bRender)
            {
                pbImage.Image = m_surface.Render();
                return pbImage.Image;
            }

            return null;
        }

        public Image Render(int nWidth, int nHeight)
        {
            m_surface.Scroll(1.0);
            m_surface.Resize(nWidth, nHeight, true);
            return m_surface.Render();
        }

        private void pbImage_Paint(object sender, PaintEventArgs e)
        {
            if (DesignMode)
                return;

            if (m_crosshairs != null)
                m_crosshairs.HandlePaint(e, pbImage.Image);

            if (OnUserPaint != null)
                OnUserPaint(sender, e);
        }

        private void pbImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (DesignMode)
                return;

            if (m_crosshairs != null)
                m_crosshairs.HandleMouseMove(e, pbImage);

            if (OnUserMouseMove != null)
                OnUserMouseMove(sender, e);
        }

        public void SetCrossHairsLocation(Point pt)
        {
            if (m_crosshairs != null)
                m_crosshairs.SetLocation(pt, pbImage);
        }

        public void UpdateCrosshairs()
        {
            pbImage.Invalidate();
        }

        public static double GetTimeZoneOffset()
        {
            TimeZone zone = TimeZone.CurrentTimeZone;
            TimeZoneInfo eastern;
            DateTime dtNow = DateTime.Now;
            if (zone.IsDaylightSavingTime(dtNow))
                eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Daylight Time");
            else
                eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            TimeSpan localOffset = zone.GetUtcOffset(dtNow);
            TimeSpan easternOffset = eastern.GetUtcOffset(dtNow);
            TimeSpan diff = easternOffset - localOffset;
            return diff.TotalHours;
        }

        public static Image QuickRender(PlotCollection col, int nWidth = -1, int nHeight = -1, bool bConvertToEastern = false)
        {
            double dfTimeOffsetInHours = 0;

            if (bConvertToEastern)
                dfTimeOffsetInHours = GetTimeZoneOffset();

            SimpleGraphingControl simpleGraphingControl1 = new SimpleGraphingControl();
            simpleGraphingControl1.Name = "SimpleGraphing";

            simpleGraphingControl1.Configuration = new Configuration();
            simpleGraphingControl1.Configuration.Frames.Add(new ConfigurationFrame());
            simpleGraphingControl1.EnableCrossHairs = true;
            simpleGraphingControl1.Configuration.Frames[0].XAxis.LabelFont = new Font("Century Gothic", 7.0f);
            simpleGraphingControl1.Configuration.Frames[0].XAxis.Visible = true;
            simpleGraphingControl1.Configuration.Frames[0].XAxis.Margin = 100;
            simpleGraphingControl1.Configuration.Frames[0].XAxis.TimeOffsetInHours = dfTimeOffsetInHours;
            simpleGraphingControl1.Configuration.Frames[0].YAxis.LabelFont = new Font("Century Gothic", 7.0f);
            simpleGraphingControl1.Configuration.Frames[0].YAxis.Decimals = 3;
            simpleGraphingControl1.Configuration.Frames[0].Plots.Add(new ConfigurationPlot());

            if (col.Count > 0 && col[0].Y_values.Count == 4)
            {
                simpleGraphingControl1.Configuration.Frames[0].Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.CANDLE;
                simpleGraphingControl1.Configuration.Frames[0].XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.TIME;
            }
            else
            {
                simpleGraphingControl1.Configuration.Frames[0].Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.LINE;
                simpleGraphingControl1.Configuration.Frames[0].XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.NUMBER;
            }

            simpleGraphingControl1.Configuration.Frames[0].EnableRelativeScaling(true, true, 0);

            PlotCollectionSet set = new PlotCollectionSet();
            set.Add(col);
            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>() { set };
            simpleGraphingControl1.BuildGraph(rgSet);

            if (nWidth <= 0)
                nWidth = 600;

            if (nHeight <= 0)
                nHeight = 300;

            return simpleGraphingControl1.Render(nWidth, nHeight);
        }
    }

    public class Crosshairs
    {
        bool m_bEnableCrosshairs = false;
        Bitmap m_bmpHoriz = null;
        Bitmap m_bmpVert = null;
        Point m_ptMouse;
        Point m_ptMouseOld;
        bool m_bUserUpdateCrosshairs = false;

        public Crosshairs()
        {
        }

        public bool EnableCrosshairs
        {
            get { return m_bEnableCrosshairs; }
            set { m_bEnableCrosshairs = value; }
        }

        public bool UserUpdate
        {
            get { return m_bUserUpdateCrosshairs; }
            set { m_bUserUpdateCrosshairs = value; }
        }

        public void HandleMouseMove(MouseEventArgs e, Control ctrl)
        {
            if (!m_bEnableCrosshairs)
                return;

            m_ptMouse = e.Location;

            if (!m_bUserUpdateCrosshairs)
                ctrl.Invalidate();
        }

        public void SetLocation(Point pt, Control ctrl)
        {
            m_ptMouse = pt;
            ctrl.Invalidate();
        }

        public void HandlePaint(PaintEventArgs e, Image imgBack)
        {
            if (!m_bEnableCrosshairs)
                return;

            if (imgBack == null)
                return;

            Graphics gimg = e.Graphics;
            Point pt = m_ptMouse;

            if (!m_ptMouseOld.IsEmpty)
            {
                if (m_bmpHoriz != null)
                    gimg.DrawImage(m_bmpHoriz, new PointF(0, m_ptMouseOld.Y));

                if (m_bmpVert != null)
                    gimg.DrawImage(m_bmpVert, new PointF(m_ptMouseOld.X, 0));
            }

            if (m_bmpHoriz == null)
                m_bmpHoriz = new Bitmap(imgBack.Width, 1);

            if (m_bmpVert == null)
                m_bmpVert = new Bitmap(1, imgBack.Height);

            using (Graphics g = Graphics.FromImage(m_bmpVert))
            {
                g.DrawImage(imgBack, new Rectangle(0, 0, m_bmpVert.Width, m_bmpVert.Height), new Rectangle(pt.X, 0, 1, imgBack.Height), GraphicsUnit.Pixel);
            }

            using (Graphics g = Graphics.FromImage(m_bmpHoriz))
            {
                g.DrawImage(imgBack, new Rectangle(0, 0, m_bmpHoriz.Width, m_bmpHoriz.Height), new Rectangle(0, pt.Y, imgBack.Width, 1), GraphicsUnit.Pixel);
            }

            Pen p = new Pen(Color.FromArgb(64, 0, 0, 255), 1.0f);
            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            gimg.DrawLine(p, new Point(0, pt.Y), new Point(imgBack.Width, pt.Y));
            gimg.DrawLine(p, new Point(pt.X, 0), new Point(pt.X, imgBack.Height));
            p.Dispose();

            m_ptMouseOld = m_ptMouse;
        }
    }
}
