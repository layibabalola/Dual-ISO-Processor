namespace Dual_ISO_Processor
{
    partial class DualISOProcessor
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
            this.btnSetImageFolder = new System.Windows.Forms.Button();
            this.txtImageFolderPath = new System.Windows.Forms.TextBox();
            this.txtCr2hdrPath = new System.Windows.Forms.TextBox();
            this.btnSetcr2hdrPath = new System.Windows.Forms.Button();
            this.btnProcessImages = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
            this.dlgFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.lblElapsed = new System.Windows.Forms.Label();
            this.lblElapsedValue = new System.Windows.Forms.Label();
            this.lblDurationPerImageValue = new System.Windows.Forms.Label();
            this.lblDurationPerImg = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblImagesProcessedVal = new System.Windows.Forms.Label();
            this.lblImagesProcessed = new System.Windows.Forms.Label();
            this.lblThreadsVal = new System.Windows.Forms.Label();
            this.lblThreads = new System.Windows.Forms.Label();
            this.lblTimeRemainingValue = new System.Windows.Forms.Label();
            this.lblTimeRemaining = new System.Windows.Forms.Label();
            this.lblFolderTimeVal = new System.Windows.Forms.Label();
            this.lblFolderTime = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.lblErrorsVal = new System.Windows.Forms.Label();
            this.lblErrors = new System.Windows.Forms.Label();
            this.btnBusiness = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSetImageFolder
            // 
            this.btnSetImageFolder.Location = new System.Drawing.Point(9, 10);
            this.btnSetImageFolder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSetImageFolder.Name = "btnSetImageFolder";
            this.btnSetImageFolder.Size = new System.Drawing.Size(114, 30);
            this.btnSetImageFolder.TabIndex = 0;
            this.btnSetImageFolder.Text = "Set Image Folder";
            this.btnSetImageFolder.UseVisualStyleBackColor = true;
            this.btnSetImageFolder.Click += new System.EventHandler(this.btnSetImageFolder_Click);
            // 
            // txtImageFolderPath
            // 
            this.txtImageFolderPath.Location = new System.Drawing.Point(128, 15);
            this.txtImageFolderPath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtImageFolderPath.Name = "txtImageFolderPath";
            this.txtImageFolderPath.Size = new System.Drawing.Size(387, 20);
            this.txtImageFolderPath.TabIndex = 1;
            // 
            // txtCr2hdrPath
            // 
            this.txtCr2hdrPath.Location = new System.Drawing.Point(128, 76);
            this.txtCr2hdrPath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtCr2hdrPath.Name = "txtCr2hdrPath";
            this.txtCr2hdrPath.Size = new System.Drawing.Size(387, 20);
            this.txtCr2hdrPath.TabIndex = 3;
            // 
            // btnSetcr2hdrPath
            // 
            this.btnSetcr2hdrPath.Location = new System.Drawing.Point(9, 71);
            this.btnSetcr2hdrPath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSetcr2hdrPath.Name = "btnSetcr2hdrPath";
            this.btnSetcr2hdrPath.Size = new System.Drawing.Size(114, 30);
            this.btnSetcr2hdrPath.TabIndex = 2;
            this.btnSetcr2hdrPath.Text = "Set cr2hdr Path";
            this.btnSetcr2hdrPath.UseVisualStyleBackColor = true;
            this.btnSetcr2hdrPath.Click += new System.EventHandler(this.btnSetcr2hdrPath_Click);
            // 
            // btnProcessImages
            // 
            this.btnProcessImages.Location = new System.Drawing.Point(9, 173);
            this.btnProcessImages.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnProcessImages.Name = "btnProcessImages";
            this.btnProcessImages.Size = new System.Drawing.Size(248, 24);
            this.btnProcessImages.TabIndex = 6;
            this.btnProcessImages.Text = "Process Images";
            this.btnProcessImages.UseVisualStyleBackColor = true;
            this.btnProcessImages.Click += new System.EventHandler(this.btnProcessImages_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(10, 226);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(504, 19);
            this.progressBar1.TabIndex = 7;
            // 
            // dlgOpen
            // 
            this.dlgOpen.FileName = "dlgOpen";
            // 
            // lblElapsed
            // 
            this.lblElapsed.AutoSize = true;
            this.lblElapsed.Location = new System.Drawing.Point(50, 130);
            this.lblElapsed.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblElapsed.Name = "lblElapsed";
            this.lblElapsed.Size = new System.Drawing.Size(74, 13);
            this.lblElapsed.TabIndex = 8;
            this.lblElapsed.Text = "Elapsed Time:";
            // 
            // lblElapsedValue
            // 
            this.lblElapsedValue.AutoSize = true;
            this.lblElapsedValue.Location = new System.Drawing.Point(126, 130);
            this.lblElapsedValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblElapsedValue.Name = "lblElapsedValue";
            this.lblElapsedValue.Size = new System.Drawing.Size(49, 13);
            this.lblElapsedValue.TabIndex = 9;
            this.lblElapsedValue.Text = "00:00:00";
            // 
            // lblDurationPerImageValue
            // 
            this.lblDurationPerImageValue.AutoSize = true;
            this.lblDurationPerImageValue.Location = new System.Drawing.Point(425, 130);
            this.lblDurationPerImageValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDurationPerImageValue.Name = "lblDurationPerImageValue";
            this.lblDurationPerImageValue.Size = new System.Drawing.Size(70, 13);
            this.lblDurationPerImageValue.TabIndex = 11;
            this.lblDurationPerImageValue.Text = "00:00:00.000";
            // 
            // lblDurationPerImg
            // 
            this.lblDurationPerImg.AutoSize = true;
            this.lblDurationPerImg.Location = new System.Drawing.Point(320, 130);
            this.lblDurationPerImg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDurationPerImg.Name = "lblDurationPerImg";
            this.lblDurationPerImg.Size = new System.Drawing.Size(101, 13);
            this.lblDurationPerImg.TabIndex = 10;
            this.lblDurationPerImg.Text = "Duration Per Image:";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(266, 173);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(248, 24);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblImagesProcessedVal
            // 
            this.lblImagesProcessedVal.AutoSize = true;
            this.lblImagesProcessedVal.Location = new System.Drawing.Point(425, 152);
            this.lblImagesProcessedVal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblImagesProcessedVal.Name = "lblImagesProcessedVal";
            this.lblImagesProcessedVal.Size = new System.Drawing.Size(24, 13);
            this.lblImagesProcessedVal.TabIndex = 14;
            this.lblImagesProcessedVal.Text = "0/0";
            // 
            // lblImagesProcessed
            // 
            this.lblImagesProcessed.AutoSize = true;
            this.lblImagesProcessed.Location = new System.Drawing.Point(324, 152);
            this.lblImagesProcessed.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblImagesProcessed.Name = "lblImagesProcessed";
            this.lblImagesProcessed.Size = new System.Drawing.Size(97, 13);
            this.lblImagesProcessed.TabIndex = 13;
            this.lblImagesProcessed.Text = "Images Processed:";
            // 
            // lblThreadsVal
            // 
            this.lblThreadsVal.AutoSize = true;
            this.lblThreadsVal.Location = new System.Drawing.Point(127, 152);
            this.lblThreadsVal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblThreadsVal.Name = "lblThreadsVal";
            this.lblThreadsVal.Size = new System.Drawing.Size(24, 13);
            this.lblThreadsVal.TabIndex = 16;
            this.lblThreadsVal.Text = "0/0";
            // 
            // lblThreads
            // 
            this.lblThreads.AutoSize = true;
            this.lblThreads.Location = new System.Drawing.Point(42, 152);
            this.lblThreads.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblThreads.Name = "lblThreads";
            this.lblThreads.Size = new System.Drawing.Size(83, 13);
            this.lblThreads.TabIndex = 15;
            this.lblThreads.Text = "Threads In Use:";
            // 
            // lblTimeRemainingValue
            // 
            this.lblTimeRemainingValue.AutoSize = true;
            this.lblTimeRemainingValue.Location = new System.Drawing.Point(96, 210);
            this.lblTimeRemainingValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTimeRemainingValue.Name = "lblTimeRemainingValue";
            this.lblTimeRemainingValue.Size = new System.Drawing.Size(70, 13);
            this.lblTimeRemainingValue.TabIndex = 18;
            this.lblTimeRemainingValue.Text = "00:00:00.000";
            // 
            // lblTimeRemaining
            // 
            this.lblTimeRemaining.AutoSize = true;
            this.lblTimeRemaining.Location = new System.Drawing.Point(9, 210);
            this.lblTimeRemaining.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTimeRemaining.Name = "lblTimeRemaining";
            this.lblTimeRemaining.Size = new System.Drawing.Size(86, 13);
            this.lblTimeRemaining.TabIndex = 17;
            this.lblTimeRemaining.Text = "Time Remaining:";
            // 
            // lblFolderTimeVal
            // 
            this.lblFolderTimeVal.AutoSize = true;
            this.lblFolderTimeVal.Location = new System.Drawing.Point(103, 42);
            this.lblFolderTimeVal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFolderTimeVal.Name = "lblFolderTimeVal";
            this.lblFolderTimeVal.Size = new System.Drawing.Size(70, 13);
            this.lblFolderTimeVal.TabIndex = 20;
            this.lblFolderTimeVal.Text = "00:00:00.000";
            // 
            // lblFolderTime
            // 
            this.lblFolderTime.AutoSize = true;
            this.lblFolderTime.Location = new System.Drawing.Point(9, 42);
            this.lblFolderTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFolderTime.Name = "lblFolderTime";
            this.lblFolderTime.Size = new System.Drawing.Size(92, 13);
            this.lblFolderTime.TabIndex = 19;
            this.lblFolderTime.Text = "Folder Remaining:";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(128, 106);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.ReadOnly = true;
            this.numericUpDown1.Size = new System.Drawing.Size(39, 20);
            this.numericUpDown1.TabIndex = 21;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 110);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Threads Allowed:";
            // 
            // lblErrorsVal
            // 
            this.lblErrorsVal.AutoSize = true;
            this.lblErrorsVal.Location = new System.Drawing.Point(425, 107);
            this.lblErrorsVal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblErrorsVal.Name = "lblErrorsVal";
            this.lblErrorsVal.Size = new System.Drawing.Size(13, 13);
            this.lblErrorsVal.TabIndex = 24;
            this.lblErrorsVal.Text = "0";
            // 
            // lblErrors
            // 
            this.lblErrors.AutoSize = true;
            this.lblErrors.Location = new System.Drawing.Point(382, 107);
            this.lblErrors.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblErrors.Name = "lblErrors";
            this.lblErrors.Size = new System.Drawing.Size(37, 13);
            this.lblErrors.TabIndex = 23;
            this.lblErrors.Text = "Errors:";
            // 
            // btnBusiness
            // 
            this.btnBusiness.Enabled = false;
            this.btnBusiness.Location = new System.Drawing.Point(370, 109);
            this.btnBusiness.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnBusiness.Name = "btnBusiness";
            this.btnBusiness.Size = new System.Drawing.Size(8, 11);
            this.btnBusiness.TabIndex = 25;
            this.btnBusiness.UseVisualStyleBackColor = true;
            this.btnBusiness.Visible = false;
            this.btnBusiness.Click += new System.EventHandler(this.btnBusiness_Click);
            // 
            // DualISOProcessor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 258);
            this.Controls.Add(this.btnBusiness);
            this.Controls.Add(this.lblErrorsVal);
            this.Controls.Add(this.lblErrors);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.lblFolderTimeVal);
            this.Controls.Add(this.lblFolderTime);
            this.Controls.Add(this.lblTimeRemainingValue);
            this.Controls.Add(this.lblTimeRemaining);
            this.Controls.Add(this.lblThreadsVal);
            this.Controls.Add(this.lblThreads);
            this.Controls.Add(this.lblImagesProcessedVal);
            this.Controls.Add(this.lblImagesProcessed);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblDurationPerImageValue);
            this.Controls.Add(this.lblDurationPerImg);
            this.Controls.Add(this.lblElapsedValue);
            this.Controls.Add(this.lblElapsed);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnProcessImages);
            this.Controls.Add(this.txtCr2hdrPath);
            this.Controls.Add(this.btnSetcr2hdrPath);
            this.Controls.Add(this.txtImageFolderPath);
            this.Controls.Add(this.btnSetImageFolder);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "DualISOProcessor";
            this.Text = "Dual ISO Processor";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSetImageFolder;
        private System.Windows.Forms.TextBox txtImageFolderPath;
        private System.Windows.Forms.TextBox txtCr2hdrPath;
        private System.Windows.Forms.Button btnSetcr2hdrPath;
        private System.Windows.Forms.Button btnProcessImages;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.OpenFileDialog dlgOpen;
        private System.Windows.Forms.FolderBrowserDialog dlgFolderBrowser;
        private System.Windows.Forms.Label lblElapsed;
        private System.Windows.Forms.Label lblElapsedValue;
        private System.Windows.Forms.Label lblDurationPerImageValue;
        private System.Windows.Forms.Label lblDurationPerImg;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblImagesProcessedVal;
        private System.Windows.Forms.Label lblImagesProcessed;
        private System.Windows.Forms.Label lblThreadsVal;
        private System.Windows.Forms.Label lblThreads;
        private System.Windows.Forms.Label lblTimeRemainingValue;
        private System.Windows.Forms.Label lblTimeRemaining;
        private System.Windows.Forms.Label lblFolderTimeVal;
        private System.Windows.Forms.Label lblFolderTime;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblErrorsVal;
        private System.Windows.Forms.Label lblErrors;
        private System.Windows.Forms.Button btnBusiness;
    }
}

