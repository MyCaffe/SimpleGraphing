using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleGraphingApp
{
    public partial class FormMovingAverages : Form
    {
        public event EventHandler<MovingAverageChangeArgs> OnChange;
        public event EventHandler OnClosing;

        public FormMovingAverages()
        {
            InitializeComponent();
        }

        private void tbSMA_Scroll(object sender, EventArgs e)
        {
            lblSMAValue.Text = tbSMA.Value.ToString();

            if (OnChange != null)
                OnChange(this, new MovingAverageChangeArgs(tbSMA.Value, tbEMA.Value));
        }

        private void tbEMA_Scroll(object sender, EventArgs e)
        {
            lblEMAValue.Text = tbEMA.Value.ToString();

            if (OnChange != null)
                OnChange(this, new MovingAverageChangeArgs(tbSMA.Value, tbEMA.Value));
        }

        private void FormMovingAverages_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (OnClosing != null)
                OnClosing(this, new EventArgs());
        }
    }

    public class MovingAverageChangeArgs : EventArgs
    {
        int m_nSMAInterval;
        int m_nEMAInterval;

        public MovingAverageChangeArgs(int nSMA, int nEMA)
        {
            m_nSMAInterval = nSMA;
            m_nEMAInterval = nEMA;
        }

        public int SMAInterval
        {
            get { return m_nSMAInterval; }
        }

        public int EMAInterval
        {
            get { return m_nEMAInterval; }
        }
    }
}
