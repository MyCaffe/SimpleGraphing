using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleGraphing;

namespace SimpleGraphingApp
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void showDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<PlotCollectionSet> rgSet = new List<PlotCollectionSet>();
            int nCount = 300;

            for (int i = 0; i < 4; i++)
            {
                PlotCollectionSet set = new PlotCollectionSet();
                PlotCollection plots;

                plots = new PlotCollection("plot_1 - " + i.ToString());
                for (int j = 0; j < nCount; j++)
                {
                    plots.Add(new Plot(j, j * Math.Sin(j)));
                }

                set.Add(plots);
                plots = new PlotCollection("plot_2 - " + i.ToString());
                for (int j = 0; j < nCount; j++)
                {
                    plots.Add(new Plot(j, j * Math.Sin(j) * Math.Cos(j)));
                }

                set.Add(plots);
                rgSet.Add(set);
            }

            simpleGraphingControl1.BuildGraph(rgSet);
            simpleGraphingControl1.Invalidate();
            simpleGraphingControl1.ScrollToEnd();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            simpleGraphingControl1.Configuration.SaveToFile("c:\\temp\\foo.xml");
        }
    }
}
