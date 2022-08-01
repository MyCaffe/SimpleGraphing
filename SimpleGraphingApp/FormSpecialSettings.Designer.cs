namespace SimpleGraphingApp
{
    partial class FormSpecialSettings
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
            this.lblHMA = new System.Windows.Forms.Label();
            this.lblHMAValue = new System.Windows.Forms.Label();
            this.tbHMA = new System.Windows.Forms.TrackBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rad_bbSMA = new System.Windows.Forms.RadioButton();
            this.rad_bbDefault = new System.Windows.Forms.RadioButton();
            this.rad_bbEMA = new System.Windows.Forms.RadioButton();
            this.rad_bbHMA = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.tbSMA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbEMA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHMA)).BeginInit();
            this.groupBox1.SuspendLayout();
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
            // lblHMA
            // 
            this.lblHMA.AutoSize = true;
            this.lblHMA.Location = new System.Drawing.Point(15, 129);
            this.lblHMA.Name = "lblHMA";
            this.lblHMA.Size = new System.Drawing.Size(72, 13);
            this.lblHMA.TabIndex = 0;
            this.lblHMA.Text = "HMA Interval:";
            // 
            // lblHMAValue
            // 
            this.lblHMAValue.AutoSize = true;
            this.lblHMAValue.Location = new System.Drawing.Point(329, 129);
            this.lblHMAValue.Name = "lblHMAValue";
            this.lblHMAValue.Size = new System.Drawing.Size(19, 13);
            this.lblHMAValue.TabIndex = 0;
            this.lblHMAValue.Text = "10";
            // 
            // tbHMA
            // 
            this.tbHMA.Location = new System.Drawing.Point(92, 114);
            this.tbHMA.Maximum = 100;
            this.tbHMA.Minimum = 2;
            this.tbHMA.Name = "tbHMA";
            this.tbHMA.Size = new System.Drawing.Size(231, 45);
            this.tbHMA.TabIndex = 1;
            this.tbHMA.Value = 10;
            this.tbHMA.Scroll += new System.EventHandler(this.tbHMA_Scroll);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rad_bbHMA);
            this.groupBox1.Controls.Add(this.rad_bbEMA);
            this.groupBox1.Controls.Add(this.rad_bbSMA);
            this.groupBox1.Controls.Add(this.rad_bbDefault);
            this.groupBox1.Location = new System.Drawing.Point(12, 165);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(374, 54);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bolliner Band MA";
            // 
            // rad_bbSMA
            // 
            this.rad_bbSMA.AutoSize = true;
            this.rad_bbSMA.Location = new System.Drawing.Point(80, 23);
            this.rad_bbSMA.Name = "rad_bbSMA";
            this.rad_bbSMA.Size = new System.Drawing.Size(48, 17);
            this.rad_bbSMA.TabIndex = 1;
            this.rad_bbSMA.TabStop = true;
            this.rad_bbSMA.Text = "SMA";
            this.rad_bbSMA.UseVisualStyleBackColor = true;
            this.rad_bbSMA.Click += new System.EventHandler(this.rad_Click);
            // 
            // rad_bbDefault
            // 
            this.rad_bbDefault.AutoSize = true;
            this.rad_bbDefault.Checked = true;
            this.rad_bbDefault.Location = new System.Drawing.Point(12, 23);
            this.rad_bbDefault.Name = "rad_bbDefault";
            this.rad_bbDefault.Size = new System.Drawing.Size(59, 17);
            this.rad_bbDefault.TabIndex = 0;
            this.rad_bbDefault.TabStop = true;
            this.rad_bbDefault.Text = "Default";
            this.rad_bbDefault.UseVisualStyleBackColor = true;
            this.rad_bbDefault.Click += new System.EventHandler(this.rad_Click);
            // 
            // rad_bbEMA
            // 
            this.rad_bbEMA.AutoSize = true;
            this.rad_bbEMA.Location = new System.Drawing.Point(134, 23);
            this.rad_bbEMA.Name = "rad_bbEMA";
            this.rad_bbEMA.Size = new System.Drawing.Size(48, 17);
            this.rad_bbEMA.TabIndex = 2;
            this.rad_bbEMA.TabStop = true;
            this.rad_bbEMA.Text = "EMA";
            this.rad_bbEMA.UseVisualStyleBackColor = true;
            this.rad_bbEMA.Click += new System.EventHandler(this.rad_Click);
            // 
            // rad_bbHMA
            // 
            this.rad_bbHMA.AutoSize = true;
            this.rad_bbHMA.Location = new System.Drawing.Point(188, 23);
            this.rad_bbHMA.Name = "rad_bbHMA";
            this.rad_bbHMA.Size = new System.Drawing.Size(49, 17);
            this.rad_bbHMA.TabIndex = 3;
            this.rad_bbHMA.TabStop = true;
            this.rad_bbHMA.Text = "HMA";
            this.rad_bbHMA.UseVisualStyleBackColor = true;
            this.rad_bbHMA.Click += new System.EventHandler(this.rad_Click);
            // 
            // FormSpecialSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 231);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tbHMA);
            this.Controls.Add(this.lblHMAValue);
            this.Controls.Add(this.tbEMA);
            this.Controls.Add(this.lblEMAValue);
            this.Controls.Add(this.lblHMA);
            this.Controls.Add(this.tbSMA);
            this.Controls.Add(this.lblEMA);
            this.Controls.Add(this.lblSMAValue);
            this.Controls.Add(this.lblSMA);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSpecialSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Moving Averages";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMovingAverages_FormClosing);
            this.Load += new System.EventHandler(this.FormMovingAverages_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tbSMA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbEMA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbHMA)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.Label lblHMA;
        private System.Windows.Forms.Label lblHMAValue;
        private System.Windows.Forms.TrackBar tbHMA;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rad_bbDefault;
        private System.Windows.Forms.RadioButton rad_bbSMA;
        private System.Windows.Forms.RadioButton rad_bbHMA;
        private System.Windows.Forms.RadioButton rad_bbEMA;
    }
}