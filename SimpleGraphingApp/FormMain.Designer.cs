namespace SimpleGraphingApp
{
    partial class FormMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.candleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.candleWithOverlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.candleFromExternalDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.enableActionStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.movingAveragesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.showPlotCollectionVisualizerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.simpleGraphingControl1 = new SimpleGraphing.SimpleGraphingControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnRun = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.btnStepPrev = new System.Windows.Forms.ToolStripButton();
            this.btnStepNext = new System.Windows.Forms.ToolStripButton();
            this.btnReDraw = new System.Windows.Forms.ToolStripButton();
            this.btnCrossHairs = new System.Windows.Forms.ToolStripButton();
            this.btnScaleToVisible = new System.Windows.Forms.ToolStripButton();
            this.timerUI = new System.Windows.Forms.Timer(this.components);
            this.timerData = new System.Windows.Forms.Timer(this.components);
            this.openFileDialogBin = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.testToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(762, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDataToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripSeparator1,
            this.enableActionStripMenuItem,
            this.movingAveragesToolStripMenuItem,
            this.toolStripSeparator2,
            this.showPlotCollectionVisualizerToolStripMenuItem,
            this.toolStripSeparator3});
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.testToolStripMenuItem.Text = "Test";
            // 
            // showDataToolStripMenuItem
            // 
            this.showDataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lineToolStripMenuItem,
            this.candleToolStripMenuItem,
            this.candleWithOverlayToolStripMenuItem,
            this.candleFromExternalDataToolStripMenuItem});
            this.showDataToolStripMenuItem.Name = "showDataToolStripMenuItem";
            this.showDataToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.showDataToolStripMenuItem.Text = "&Show Data";
            // 
            // lineToolStripMenuItem
            // 
            this.lineToolStripMenuItem.Name = "lineToolStripMenuItem";
            this.lineToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.lineToolStripMenuItem.Text = "Line";
            this.lineToolStripMenuItem.Click += new System.EventHandler(this.lineToolStripMenuItem_Click);
            // 
            // candleToolStripMenuItem
            // 
            this.candleToolStripMenuItem.Name = "candleToolStripMenuItem";
            this.candleToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.candleToolStripMenuItem.Text = "Candle";
            this.candleToolStripMenuItem.Click += new System.EventHandler(this.candleToolStripMenuItem_Click);
            // 
            // candleWithOverlayToolStripMenuItem
            // 
            this.candleWithOverlayToolStripMenuItem.Name = "candleWithOverlayToolStripMenuItem";
            this.candleWithOverlayToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.candleWithOverlayToolStripMenuItem.Text = "Candle with Overlay";
            this.candleWithOverlayToolStripMenuItem.Click += new System.EventHandler(this.candleToolStripMenuItem_Click);
            // 
            // candleFromExternalDataToolStripMenuItem
            // 
            this.candleFromExternalDataToolStripMenuItem.Name = "candleFromExternalDataToolStripMenuItem";
            this.candleFromExternalDataToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.candleFromExternalDataToolStripMenuItem.Text = "Candle from external data";
            this.candleFromExternalDataToolStripMenuItem.Click += new System.EventHandler(this.candleFromExternalDataToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(239, 6);
            // 
            // enableActionStripMenuItem
            // 
            this.enableActionStripMenuItem.Name = "enableActionStripMenuItem";
            this.enableActionStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.enableActionStripMenuItem.Text = "Enable Action";
            this.enableActionStripMenuItem.Click += new System.EventHandler(this.enableActionStripMenuItem_Click);
            // 
            // movingAveragesToolStripMenuItem
            // 
            this.movingAveragesToolStripMenuItem.Name = "movingAveragesToolStripMenuItem";
            this.movingAveragesToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.movingAveragesToolStripMenuItem.Text = "Moving Averages...";
            this.movingAveragesToolStripMenuItem.Click += new System.EventHandler(this.movingAveragesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(239, 6);
            // 
            // showPlotCollectionVisualizerToolStripMenuItem
            // 
            this.showPlotCollectionVisualizerToolStripMenuItem.Name = "showPlotCollectionVisualizerToolStripMenuItem";
            this.showPlotCollectionVisualizerToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.showPlotCollectionVisualizerToolStripMenuItem.Text = "Show PlotCollection Visualizer...";
            this.showPlotCollectionVisualizerToolStripMenuItem.Click += new System.EventHandler(this.testPlotCollectionVisualizerToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(239, 6);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.simpleGraphingControl1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(762, 907);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 24);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(762, 932);
            this.toolStripContainer1.TabIndex = 3;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // simpleGraphingControl1
            // 
            this.simpleGraphingControl1.Configuration = ((SimpleGraphing.Configuration)(resources.GetObject("simpleGraphingControl1.Configuration")));
            this.simpleGraphingControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleGraphingControl1.EnableCrossHairs = true;
            this.simpleGraphingControl1.Location = new System.Drawing.Point(0, 0);
            this.simpleGraphingControl1.Name = "simpleGraphingControl1";
            this.simpleGraphingControl1.ScrollPercent = 0D;
            this.simpleGraphingControl1.ShowScrollBar = true;
            this.simpleGraphingControl1.Size = new System.Drawing.Size(762, 907);
            this.simpleGraphingControl1.TabIndex = 0;
            this.simpleGraphingControl1.UserUpdateCrosshairs = false;
            this.simpleGraphingControl1.Load += new System.EventHandler(this.simpleGraphingControl1_Load);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRun,
            this.btnStop,
            this.btnStepPrev,
            this.btnStepNext,
            this.btnReDraw,
            this.btnCrossHairs,
            this.btnScaleToVisible});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(204, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Visible = false;
            // 
            // btnRun
            // 
            this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRun.Image = ((System.Drawing.Image)(resources.GetObject("btnRun.Image")));
            this.btnRun.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(23, 22);
            this.btnRun.Text = "Run";
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(23, 22);
            this.btnStop.Text = "Stop";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStepPrev
            // 
            this.btnStepPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStepPrev.Image = ((System.Drawing.Image)(resources.GetObject("btnStepPrev.Image")));
            this.btnStepPrev.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnStepPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStepPrev.Name = "btnStepPrev";
            this.btnStepPrev.Size = new System.Drawing.Size(23, 22);
            this.btnStepPrev.Text = "Step Prev";
            this.btnStepPrev.Click += new System.EventHandler(this.btnStepPrev_Click);
            // 
            // btnStepNext
            // 
            this.btnStepNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStepNext.Image = ((System.Drawing.Image)(resources.GetObject("btnStepNext.Image")));
            this.btnStepNext.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnStepNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStepNext.Name = "btnStepNext";
            this.btnStepNext.Size = new System.Drawing.Size(23, 22);
            this.btnStepNext.Text = "Step Next";
            this.btnStepNext.Click += new System.EventHandler(this.btnStepNext_Click);
            // 
            // btnReDraw
            // 
            this.btnReDraw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnReDraw.Image = ((System.Drawing.Image)(resources.GetObject("btnReDraw.Image")));
            this.btnReDraw.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnReDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReDraw.Name = "btnReDraw";
            this.btnReDraw.Size = new System.Drawing.Size(23, 22);
            this.btnReDraw.Text = "Re-draw";
            this.btnReDraw.Click += new System.EventHandler(this.btnReDraw_Click);
            // 
            // btnCrossHairs
            // 
            this.btnCrossHairs.Checked = true;
            this.btnCrossHairs.CheckOnClick = true;
            this.btnCrossHairs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnCrossHairs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCrossHairs.Image = ((System.Drawing.Image)(resources.GetObject("btnCrossHairs.Image")));
            this.btnCrossHairs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCrossHairs.Name = "btnCrossHairs";
            this.btnCrossHairs.Size = new System.Drawing.Size(23, 22);
            this.btnCrossHairs.Text = "Enable Crosshairs";
            this.btnCrossHairs.Click += new System.EventHandler(this.btnCrossHairs_Click);
            // 
            // btnScaleToVisible
            // 
            this.btnScaleToVisible.CheckOnClick = true;
            this.btnScaleToVisible.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnScaleToVisible.Image = ((System.Drawing.Image)(resources.GetObject("btnScaleToVisible.Image")));
            this.btnScaleToVisible.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnScaleToVisible.Name = "btnScaleToVisible";
            this.btnScaleToVisible.Size = new System.Drawing.Size(23, 22);
            this.btnScaleToVisible.Text = "Scale to Visible";
            this.btnScaleToVisible.ToolTipText = "Scale to Visible - only applies when first creating the data.";
            // 
            // timerUI
            // 
            this.timerUI.Enabled = true;
            this.timerUI.Interval = 250;
            this.timerUI.Tick += new System.EventHandler(this.timerUI_Tick);
            // 
            // timerData
            // 
            this.timerData.Interval = 1000;
            this.timerData.Tick += new System.EventHandler(this.timerData_Tick);
            // 
            // openFileDialogBin
            // 
            this.openFileDialogBin.DefaultExt = "bin";
            this.openFileDialogBin.Filter = "Binary Files (*.bin)|*.bin||";
            this.openFileDialogBin.Title = "Select the binary file containing the list of PlotCollectionSets.";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 956);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "SimpleGraph Testing Application";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem candleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem movingAveragesToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRun;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.Timer timerUI;
        private System.Windows.Forms.Timer timerData;
        private SimpleGraphing.SimpleGraphingControl simpleGraphingControl1;
        private System.Windows.Forms.ToolStripButton btnStepPrev;
        private System.Windows.Forms.ToolStripButton btnStepNext;
        private System.Windows.Forms.ToolStripButton btnReDraw;
        private System.Windows.Forms.ToolStripMenuItem enableActionStripMenuItem;
        private System.Windows.Forms.ToolStripButton btnCrossHairs;
        private System.Windows.Forms.ToolStripMenuItem candleWithOverlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem showPlotCollectionVisualizerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnScaleToVisible;
        private System.Windows.Forms.ToolStripMenuItem candleFromExternalDataToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialogBin;
    }
}