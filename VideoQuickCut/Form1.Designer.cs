namespace VideoQuickCut
{
    partial class Form1
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
            this.lblInputVideo = new System.Windows.Forms.Label();
            this.lblReplacementImage = new System.Windows.Forms.Label();
            this.lblOutputVideo = new System.Windows.Forms.Label();
            this.txtInputVideo = new System.Windows.Forms.TextBox();
            this.txtReplacementImage = new System.Windows.Forms.TextBox();
            this.txtOutputVideo = new System.Windows.Forms.TextBox();
            this.btnProcess = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnBrowseVideo = new System.Windows.Forms.Button();
            this.btnBrowseImage = new System.Windows.Forms.Button();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.flowLayoutPanelIntervals = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddInterval = new System.Windows.Forms.Button();
            this.flowLayoutPanelIntervals.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblInputVideo
            // 
            this.lblInputVideo.AutoSize = true;
            this.lblInputVideo.Location = new System.Drawing.Point(25, 12);
            this.lblInputVideo.Name = "lblInputVideo";
            this.lblInputVideo.Size = new System.Drawing.Size(74, 16);
            this.lblInputVideo.TabIndex = 0;
            this.lblInputVideo.Text = "Input Video";
            // 
            // lblReplacementImage
            // 
            this.lblReplacementImage.AutoSize = true;
            this.lblReplacementImage.Location = new System.Drawing.Point(25, 56);
            this.lblReplacementImage.Name = "lblReplacementImage";
            this.lblReplacementImage.Size = new System.Drawing.Size(129, 16);
            this.lblReplacementImage.TabIndex = 1;
            this.lblReplacementImage.Text = "Replacement image";
            // 
            // lblOutputVideo
            // 
            this.lblOutputVideo.AutoSize = true;
            this.lblOutputVideo.Location = new System.Drawing.Point(25, 110);
            this.lblOutputVideo.Name = "lblOutputVideo";
            this.lblOutputVideo.Size = new System.Drawing.Size(84, 16);
            this.lblOutputVideo.TabIndex = 2;
            this.lblOutputVideo.Text = "Output Video";
            // 
            // txtInputVideo
            // 
            this.txtInputVideo.Location = new System.Drawing.Point(28, 31);
            this.txtInputVideo.Name = "txtInputVideo";
            this.txtInputVideo.Size = new System.Drawing.Size(273, 22);
            this.txtInputVideo.TabIndex = 5;
            // 
            // txtReplacementImage
            // 
            this.txtReplacementImage.Location = new System.Drawing.Point(28, 80);
            this.txtReplacementImage.Name = "txtReplacementImage";
            this.txtReplacementImage.Size = new System.Drawing.Size(273, 22);
            this.txtReplacementImage.TabIndex = 6;
            // 
            // txtOutputVideo
            // 
            this.txtOutputVideo.Location = new System.Drawing.Point(28, 129);
            this.txtOutputVideo.Name = "txtOutputVideo";
            this.txtOutputVideo.Size = new System.Drawing.Size(273, 22);
            this.txtOutputVideo.TabIndex = 7;
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(28, 285);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(378, 23);
            this.btnProcess.TabIndex = 13;
            this.btnProcess.Text = "Replace Segment";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnBrowseVideo
            // 
            this.btnBrowseVideo.Location = new System.Drawing.Point(331, 31);
            this.btnBrowseVideo.Name = "btnBrowseVideo";
            this.btnBrowseVideo.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseVideo.TabIndex = 15;
            this.btnBrowseVideo.Text = "Browse";
            this.btnBrowseVideo.UseVisualStyleBackColor = true;
            this.btnBrowseVideo.Click += new System.EventHandler(this.btnBrowseVideo_Click);
            // 
            // btnBrowseImage
            // 
            this.btnBrowseImage.Location = new System.Drawing.Point(328, 80);
            this.btnBrowseImage.Name = "btnBrowseImage";
            this.btnBrowseImage.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseImage.TabIndex = 16;
            this.btnBrowseImage.Text = "Browse";
            this.btnBrowseImage.UseVisualStyleBackColor = true;
            this.btnBrowseImage.Click += new System.EventHandler(this.btnBrowseImage_Click);
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Location = new System.Drawing.Point(328, 129);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseOutput.TabIndex = 17;
            this.btnBrowseOutput.Text = "Browse";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(28, 330);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(375, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 18;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(28, 371);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(375, 74);
            this.txtLog.TabIndex = 19;
            this.txtLog.WordWrap = false;
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // flowLayoutPanelIntervals
            // 
            this.flowLayoutPanelIntervals.AutoScroll = true;
            this.flowLayoutPanelIntervals.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelIntervals.Controls.Add(this.btnAddInterval);
            this.flowLayoutPanelIntervals.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelIntervals.Location = new System.Drawing.Point(28, 158);
            this.flowLayoutPanelIntervals.Name = "flowLayoutPanelIntervals";
            this.flowLayoutPanelIntervals.Size = new System.Drawing.Size(382, 121);
            this.flowLayoutPanelIntervals.TabIndex = 22;
            this.flowLayoutPanelIntervals.WrapContents = false;
            // 
            // btnAddInterval
            // 
            this.btnAddInterval.Location = new System.Drawing.Point(3, 3);
            this.btnAddInterval.Name = "btnAddInterval";
            this.btnAddInterval.Size = new System.Drawing.Size(148, 23);
            this.btnAddInterval.TabIndex = 22;
            this.btnAddInterval.Text = "+ Add Interval";
            this.btnAddInterval.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 457);
            this.Controls.Add(this.flowLayoutPanelIntervals);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.btnBrowseImage);
            this.Controls.Add(this.btnBrowseVideo);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.txtOutputVideo);
            this.Controls.Add(this.txtReplacementImage);
            this.Controls.Add(this.txtInputVideo);
            this.Controls.Add(this.lblOutputVideo);
            this.Controls.Add(this.lblReplacementImage);
            this.Controls.Add(this.lblInputVideo);
            this.Name = "Form1";
            this.Text = "Quick Sermon Video Editor";
            this.flowLayoutPanelIntervals.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInputVideo;
        private System.Windows.Forms.Label lblReplacementImage;
        private System.Windows.Forms.Label lblOutputVideo;
        private System.Windows.Forms.TextBox txtInputVideo;
        private System.Windows.Forms.TextBox txtReplacementImage;
        private System.Windows.Forms.TextBox txtOutputVideo;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnBrowseVideo;
        private System.Windows.Forms.Button btnBrowseImage;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelIntervals;
        private System.Windows.Forms.Button btnAddInterval;
    }
}

