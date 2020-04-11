﻿using SimpleGraphing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleGraphingDebug
{
    public partial class FormPlotCollection : Form
    {
        SimpleGraphingControl simpleGraphingControl1;
        PlotCollectionSet m_set = new PlotCollectionSet();

        public FormPlotCollection(PlotCollection col)
        {
            m_set.Add(col);
            InitializeComponent();
        }

        public FormPlotCollection(PlotCollectionSet set)
        {
            m_set = set;
            InitializeComponent();
        }

        private void FormPlotCollection2_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            simpleGraphingControl1 = new SimpleGraphingControl();
            simpleGraphingControl1.Name = "SimpleGraphing";
            this.Controls.Add(simpleGraphingControl1);
            simpleGraphingControl1.Dock = DockStyle.Fill;

            simpleGraphingControl1.Configuration = new Configuration();
            simpleGraphingControl1.Configuration.Frames.Add(new ConfigurationFrame());
            simpleGraphingControl1.EnableCrossHairs = true;
            simpleGraphingControl1.Configuration.Frames[0].XAxis.LabelFont = new Font("Century Gothic", 7.0f);
            simpleGraphingControl1.Configuration.Frames[0].XAxis.Visible = true;
            simpleGraphingControl1.Configuration.Frames[0].XAxis.Margin = 100;
            simpleGraphingControl1.Configuration.Frames[0].YAxis.LabelFont = new Font("Century Gothic", 7.0f);
            simpleGraphingControl1.Configuration.Frames[0].YAxis.Decimals = 3;

            for (int i = 0; i < m_set.Count; i++)
            {
                ConfigurationPlot plotConfig = new ConfigurationPlot();
                plotConfig.DataIndexOnRender = i;

                simpleGraphingControl1.Configuration.Frames[0].Plots.Add(plotConfig);

                if (m_set[0].Count > 0 && m_set[0][0].Y_values.Length == 4)
                {
                    simpleGraphingControl1.Configuration.Frames[0].Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.CANDLE;
                    simpleGraphingControl1.Configuration.Frames[0].XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.TIME;
                }
                else
                {
                    simpleGraphingControl1.Configuration.Frames[0].Plots[0].PlotType = ConfigurationPlot.PLOTTYPE.LINE;
                    simpleGraphingControl1.Configuration.Frames[0].XAxis.ValueType = ConfigurationAxis.VALUE_TYPE.NUMBER;
                }
            }

            simpleGraphingControl1.Configuration.Frames[0].EnableRelativeScaling(true, true);

            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>() { m_set };
            simpleGraphingControl1.BuildGraph(rgSet);
            simpleGraphingControl1.Invalidate();
            simpleGraphingControl1.ScrollToEnd(true);
        }

        private void FormPlotCollection_Resize(object sender, EventArgs e)
        {
            simpleGraphingControl1.ScrollToEnd(true);
        }
    }
}
