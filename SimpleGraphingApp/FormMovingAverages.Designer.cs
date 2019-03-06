namespace SimpleGraphingApp
{
    partial class FormMovingAverages
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblSMA = new System.Windows.Forms.Label();
            this.tbSMA = new System.Windows.Forms.TrackBar();
            this.lblSMAValue = new System.Windows.Forms.Label();
            this.lblEMA = new System.Windows.Forms.Label();
            this.lblEMAValue = new System.Windows.Forms.Label();
            this.tbEMA = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.tbSMA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbEMA)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSMA
            // 
            this.lblSMA.AutoSize = true;
            this.lblSMA.Location = new System.Drawing.Point(15, 27);
            this.lblSMA.Name = "lblSMA";
            this.lblSMA.Size = new System.Drawing.Size(71, 13);
            this.lblSMA.TabIndex = 0;
            this.lblSMA.Text = "SMA Interval:";
            // 
            // tbSMA
            // 
            this.tbSMA.Location = new System.Drawing.Point(92, 12);
            this.tbSMA.Maximum = 100;
            this.tbSMA.Minimum = 2;
            this.tbSMA.Name = "tbSMA";
            this.tbSMA.Size = new System.Drawing.Size(231, 45);
            this.tbSMA.TabIndex = 1;
            this.tbSMA.Value = 10;
            this.tbSMA.Scroll += new System.EventHandler(this.tbSMA_Scroll);
            // 
            // lblSMAValue
            // 
            this.lblSMAValue.AutoSize = true;
            this.lblSMAValue.Location = new System.Drawing.Point(329, 27);
            this.lblSMAValue.Name = "lblSMAValue";
            this.lblSMAValue.Size = new System.Drawing.Size(19, 13);
            this.lblSMAValue.TabIndex = 0;
            this.lblSMAValue.Text = "10";
            // 
            // lblEMA
            // 
            this.lblEMA.AutoSize = true;
            this.lblEMA.Location = new System.Drawing.Point(15, 78);
            this.lblEMA.Name = "lblEMA";
            this.lblEMA.Size = new System.Drawing.Size(71, 13);
            this.lblEMA.TabIndex = 0;
            this.lblEMA.Text = "EMA Interval:";
            // 
            // lblEMAValue
            // 
            this.lblEMAValue.AutoSize = true;
            this.lblEMAValue.Location = new System.Drawing.Point(329, 78);
            this.lblEMAValue.Name = "lblEMAValue";
            this.lblEMAValue.Size = new System.Drawing.Size(19, 13);
            this.lblEMAValue.TabIndex = 0;
            this.lblEMAValue.Text = "10";
            // 
            // tbEMA
            // 
            this.tbEMA.Location = new System.Drawing.Point(92, 63);
            this.tbEMA.Maximum = 100;
            this.tbEMA.Minimum = 2;
            this.tbEMA.Name = "tbEMA";
            this.tbEMA.Size = new System.Drawing.Size(231, 45);
            this.tbEMA.TabIndex = 1;
            this.tbEMA.Value = 10;
            this.tbEMA.Scroll += new System.EventHandler(this.tbEMA_Scroll);
            // 
            // FormMovingAverages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 114);
            this.Controls.Add(this.tbEMA);
            this.Controls.Add(this.lblEMAValue);
            this.Controls.Add(this.tbSMA);
            this.Controls.Add(this.lblEMA);
            this.Controls.Add(this.lblSMAValue);
            this.Controls.Add(this.lblSMA);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormMovingAverages";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Moving Averages";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMovingAverages_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.tbSMA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbEMA)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSMA;
        private System.Windows.Forms.TrackBar tbSMA;
        private System.Windows.Forms.Label lblSMAValue;
        private System.Windows.Forms.Label lblEMA;
        private System.Windows.Forms.Label lblEMAValue;
        private System.Windows.Forms.TrackBar tbEMA;
    }
}