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
    public partial class FormSpecialSettings : Form
    {
        public event EventHandler<SpecialSettingChangeArgs> OnChange;
        public event EventHandler OnClosingWindow;

        public FormSpecialSettings()
        {
            InitializeComponent();
        }

        private int getBbMa()
        {
            if (rad_bbSMA.Checked)
                return 1;

            if (rad_bbEMA.Checked)
                return 2;

            if (rad_bbHMA.Checked)
                return 3;

            return 0;
        }
        
        private void tbSMA_Scroll(object sender, EventArgs e)
        {
            lblSMAValue.Text = tbSMA.Value.ToString();
            
            if (OnChange != null)
                OnChange(this, new SpecialSettingChangeArgs(tbSMA.Value, tbEMA.Value, tbHMA.Value, getBbMa()));
        }

        private void tbEMA_Scroll(object sender, EventArgs e)
        {
            lblEMAValue.Text = tbEMA.Value.ToString();

            if (OnChange != null)
                OnChange(this, new SpecialSettingChangeArgs(tbSMA.Value, tbEMA.Value, tbHMA.Value, getBbMa()));
        }

        private void tbHMA_Scroll(object sender, EventArgs e)
        {
            lblHMAValue.Text = tbHMA.Value.ToString();

            if (OnChange != null)
                OnChange(this, new SpecialSettingChangeArgs(tbSMA.Value, tbEMA.Value, tbHMA.Value, getBbMa()));
        }

        private void FormMovingAverages_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (OnClosingWindow != null)
                OnClosingWindow(this, new EventArgs());
        }

        private void FormMovingAverages_Load(object sender, EventArgs e)
        {

        }

        private void rad_Click(object sender, EventArgs e)
        {
            rad_bbDefault.Checked = false;
            rad_bbSMA.Checked = false;
            rad_bbEMA.Checked = false;
            rad_bbHMA.Checked = false;
            ((RadioButton)sender).Checked = true;

            if (OnChange != null)
                OnChange(this, new SpecialSettingChangeArgs(tbSMA.Value, tbEMA.Value, tbHMA.Value, getBbMa()));
        }
    }

    public class SpecialSettingChangeArgs : EventArgs
    {
        int m_nSMAInterval;
        int m_nEMAInterval;
        int m_nHMAInterval;
        int m_nBbMA = 0;

        public SpecialSettingChangeArgs(int nSMA, int nEMA, int nHMA, int nBbMa)
        {
            m_nSMAInterval = nSMA;
            m_nEMAInterval = nEMA;
            m_nHMAInterval = nHMA;
            m_nBbMA = nBbMa;
        }

        public int SMAInterval
        {
            get { return m_nSMAInterval; }
        }

        public int EMAInterval
        {
            get { return m_nEMAInterval; }
        }

        public int HMAInterval
        {
            get { return m_nHMAInterval; }
        }

        public int BBMa
        {
            get { return m_nBbMA; }
        }
    }
}
