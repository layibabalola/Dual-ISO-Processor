using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;

namespace Dual_ISO_Processor
{
    public partial class DualISOProcessor : Form
    {
        static int maxThreads = Environment.ProcessorCount;
        int allowedThreads = Environment.ProcessorCount;
        int threadCount = 0;
        int errorCount = 0;
        long maxDirQueue = 0;
        long maxFileQueue = 0;
        long maxSubfolderFileQueue = 0;
        long fileQueueBeforeEmpty = 0;
        long imagesProcessed = 0;
        double percentComplete = 0;
        bool formClosed = false;
        bool preparing = false;
        bool setControlsBusy = false;
        bool workCompleted = false;
        bool isResting = true;
        static ConcurrentQueue<DirectoryInfo> dirQueue = new ConcurrentQueue<DirectoryInfo>();
        static ConcurrentQueue<FileInfo> fileQueue = new ConcurrentQueue<FileInfo>();
        static ConcurrentQueue<FileInfo> subFolderFileQueue = new ConcurrentQueue<FileInfo>();
        BackgroundWorker[] bwBusiness;
        Stopwatch swCanceling = new System.Diagnostics.Stopwatch();
        Stopwatch swPreparing = new System.Diagnostics.Stopwatch();
        Stopwatch cwStopwatch = new Stopwatch();
        BackgroundWorker bwProgressBar = new BackgroundWorker();
        BackgroundWorker bwPreparing = new BackgroundWorker();
        BackgroundWorker bwCumulativeStopWatch = new BackgroundWorker();
        BackgroundWorker bwCancel = new BackgroundWorker();
        BackgroundWorker bwThreads = new BackgroundWorker();

        string elapsedTime = "00:00:00";
        string timeRemaining = "00:00:00";
        string folderTimeRemaining = "00:00:00";
        string durationPerImage = "00:00:00.000";
        string process = "Preparing";
        string initialDirectory = string.Empty;
        string currentDirectory = string.Empty;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(IntPtr classname, string title);

        public DualISOProcessor()
        {
            InitializeComponent();

            this.CenterToScreen();

            numericUpDown1.Maximum = maxThreads;
            numericUpDown1.Value = allowedThreads;

            bwBusiness = new BackgroundWorker[maxThreads];

            if (!string.IsNullOrEmpty(Dual_ISO_Processor.Properties.Settings.Default.ImageFolder))
            {
                txtImageFolderPath.Text = Dual_ISO_Processor.Properties.Settings.Default.ImageFolder;
            }
            if (!string.IsNullOrEmpty(Dual_ISO_Processor.Properties.Settings.Default.cr2hdrPath))
            {
                txtCr2hdrPath.Text = Dual_ISO_Processor.Properties.Settings.Default.cr2hdrPath;
            }

            bwCumulativeStopWatch.DoWork += BwCumulativeStopWatch_DoWork;
            bwCumulativeStopWatch.WorkerSupportsCancellation = true;

            bwProgressBar.DoWork += BwProgressBar_DoWork;
            bwProgressBar.WorkerSupportsCancellation = true;

            bwPreparing.DoWork += BwPreparing_DoWork;
            bwPreparing.WorkerSupportsCancellation = true;

            bwCancel.DoWork += BwCancel_DoWork;
            bwCancel.WorkerSupportsCancellation = true;

            bwThreads.DoWork += BwThreads_DoWork;
            bwThreads.ProgressChanged += BwThreads_ProgressChanged;
            bwThreads.WorkerReportsProgress = true;
            bwThreads.WorkerSupportsCancellation = true;

            //Instantiate theoretical maximim number of business workers
            for (int x = 0; x < maxThreads; x++)
            {
                bwBusiness[x] = new BackgroundWorker();
                bwBusiness[x].DoWork += BwBusiness_DoWork;
                bwBusiness[x].WorkerSupportsCancellation = true;
            }

            this.FormClosing += DualISOProcessor_FormClosing;
            this.FormClosed += DualISOProcessor_FormClosed;

            foreach (Control c in this.Controls)
            {
                c.EnabledChanged += Control_EnabledChanged;
            }

            bwThreads.RunWorkerAsync();
        }
        void FindAndMoveMsgBox(string title, int x, int y)
        {
            //Moves a messagebox to the desired position
            Thread thr = new Thread(() => // create a new thread
            {
                IntPtr msgBox = IntPtr.Zero;
                // while there's no MessageBox, FindWindow returns IntPtr.Zero
                while ((msgBox = FindWindow(IntPtr.Zero, title)) == IntPtr.Zero) ;
                // after the while loop, msgBox is the handle of your MessageBox
                Rectangle r = new Rectangle();

                ManagedWinapi.Windows.SystemWindow msgBoxWindow = new ManagedWinapi.Windows.SystemWindow(msgBox);
                msgBoxWindow.Location = new Point(x, y);
            });
            thr.Start(); // starts the thread
        }

        private void Control_EnabledChanged(object sender, EventArgs e)
        {
            Control c = (Control)sender;

            if (c.GetType() == typeof(Button))
            {
                Button b = ((Button)sender);
                if (b.Enabled)
                {
                    b.ForeColor = Button.DefaultForeColor;
                    b.BackColor = Button.DefaultBackColor;
                }
                else
                {
                    b.BackColor = Color.AliceBlue;
                }
            }
        }

        private void DualISOProcessor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dual_ISO_Processor.Properties.Settings.Default.ImageFolder = txtImageFolderPath.Text;
            Dual_ISO_Processor.Properties.Settings.Default.cr2hdrPath = txtCr2hdrPath.Text;
            Dual_ISO_Processor.Properties.Settings.Default.Save();

            formClosed = true;
        }

        private void DualISOProcessor_FormClosed(object sender, FormClosedEventArgs e)
        {
            formClosed = true;
        }

        private void SetControlValues(ProgressChangedEventArgs e)
        {
            try
            {
                setControlsBusy = true;
                progressBar1.Value = e.ProgressPercentage;

                if (e.UserState.GetType() != typeof(List<Control>))
                {
                    Control c = (Control)e.UserState;

                    if (c.GetType() == typeof(Button))
                    {
                        Button btnSource = ((Button)c);
                        if (((Button)(Controls.Find(btnSource.Name, true).FirstOrDefault())) != null)
                        {
                            Button btnDest = ((Button)(Controls.Find(btnSource.Name, true).FirstOrDefault()));
                            btnDest.Text = btnSource.Text;
                            btnDest.Enabled = btnSource.Enabled;
                            btnDest.ForeColor = btnSource.ForeColor;
                        }
                    }
                    else if (c.GetType() == typeof(Label))
                    {
                        Label lblSource = ((Label)c);
                        if (((Label)(Controls.Find(lblSource.Name, true).FirstOrDefault())) != null)
                        {
                            Label lblDest = ((Label)(Controls.Find(lblSource.Name, true).FirstOrDefault()));
                            lblDest.Text = lblSource.Text;
                            lblDest.Enabled = lblSource.Enabled;
                            lblDest.ForeColor = lblSource.ForeColor;
                        }
                    }
                    else if (c.GetType() == typeof(TextBox))
                    {
                        TextBox txtSource = ((TextBox)c);
                        if (((TextBox)(Controls.Find(txtSource.Name, true).FirstOrDefault())) != null)
                        {
                            TextBox txtDest = ((TextBox)(Controls.Find(txtSource.Name, true).FirstOrDefault()));
                            txtDest.Text = txtSource.Text;
                            txtDest.Enabled = txtSource.Enabled;
                            txtDest.ForeColor = txtSource.ForeColor;

                            if (!isResting)
                            {
                                txtDest.SelectionStart = txtSource.SelectionStart;
                                txtDest.SelectionLength = txtSource.SelectionLength;
                            }
                        }
                    }
                }
                else if (e.UserState.GetType() == typeof(List<Control>))
                {
                    List<Control> controlList = ((List<Control>)(e.UserState));
                    if (controlList.Count() > 0)
                    {
                        foreach (Control c in controlList)
                        {
                            if (c.GetType() == typeof(Button))
                            {
                                Button btnSource = ((Button)c);
                                if (((Button)(Controls.Find(btnSource.Name, true).FirstOrDefault())) != null)
                                {
                                    Button btnDest = ((Button)(Controls.Find(btnSource.Name, true).FirstOrDefault()));
                                    btnDest.Text = btnSource.Text;
                                    btnDest.Enabled = btnSource.Enabled;
                                    btnDest.Enabled = btnSource.Enabled;
                                    btnDest.ForeColor = btnSource.ForeColor;
                                }
                            }
                            else if (c.GetType() == typeof(Label))
                            {
                                Label lblSource = ((Label)c);
                                if (((Label)(Controls.Find(lblSource.Name, true).FirstOrDefault())) != null)
                                {
                                    Label lblDest = ((Label)(Controls.Find(lblSource.Name, true).FirstOrDefault()));
                                    lblDest.Text = lblSource.Text;
                                    lblDest.Enabled = lblSource.Enabled;
                                    lblDest.ForeColor = lblSource.ForeColor;
                                }
                            }
                            else if (c.GetType() == typeof(TextBox))
                            {
                                TextBox txtSource = ((TextBox)c);
                                if (((TextBox)(Controls.Find(txtSource.Name, true).FirstOrDefault())) != null)
                                {
                                    TextBox txtDest = ((TextBox)(Controls.Find(txtSource.Name, true).FirstOrDefault()));
                                    txtDest.Text = txtSource.Text;
                                    txtDest.Enabled = txtSource.Enabled;
                                    txtDest.ForeColor = txtSource.ForeColor;

                                    if (!isResting)
                                    {
                                        txtDest.SelectionStart = txtSource.SelectionStart;
                                        txtDest.SelectionLength = txtSource.SelectionLength;
                                    }
                                }
                            }
                        }
                    }

                    setControlsBusy = false;
                }
            }
            catch (Exception ex)
            {
                if (Directory.Exists(txtCr2hdrPath.Text))
                {
                    File.WriteAllText(txtCr2hdrPath.Text + "\\ExceptionLog." + Guid.NewGuid().ToString() + ".log", ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }

        private void BwCancel_DoWork(object sender, DoWorkEventArgs e)
        {
            percentComplete = 1;
            swCanceling.Start();

            while (!((BackgroundWorker)(sender)).CancellationPending || threadCount > 0)
            {
                percentComplete = threadCount / allowedThreads;
            }

            if (bwCumulativeStopWatch.IsBusy)
            {
                bwCumulativeStopWatch.CancelAsync();
            }

            //Set all globals to original state
            dirQueue = new ConcurrentQueue<DirectoryInfo>();
            fileQueue = new ConcurrentQueue<FileInfo>();
            maxDirQueue = 0;
            maxFileQueue = 0;
            maxSubfolderFileQueue = 0;
            currentDirectory = initialDirectory;
        }

        private void BwThreads_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SetControlValues(e);
        }

        private void BwThreads_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                bool businessBusy = false;

                if (preparing)
                {
                    List<Control> controlList = new List<Control>();
                    isResting = false;

                    Button btnP = new Button();
                    btnP.Name = btnProcessImages.Name;
                    btnP.Text = process;
                    btnP.Enabled = false;

                    controlList.Add(btnP);

                    Button btnC = new Button();
                    btnC.Name = btnCancel.Name;
                    btnC.Text = btnCancel.Text;
                    btnC.Enabled = btnCancel.Enabled;

                    controlList.Add(btnC);

                    Label lblImgProc = new Label();
                    lblImgProc.Name = lblImagesProcessedVal.Name;
                    lblImgProc.Text = lblImagesProcessedVal.Text;
                    lblImgProc.Enabled = lblImagesProcessedVal.Enabled;

                    controlList.Add(lblImgProc);

                    Label lblElapVal = new Label();
                    lblElapVal.Name = lblElapsedValue.Name;
                    lblElapVal.Text = lblElapsedValue.Text;
                    lblElapVal.Enabled = lblElapsedValue.Enabled;

                    controlList.Add(lblElapVal);

                    Label lblDurationPerImg = new Label();
                    lblDurationPerImg.Name = lblDurationPerImageValue.Name;
                    lblDurationPerImg.Text = lblDurationPerImageValue.Text;
                    lblDurationPerImg.Enabled = lblDurationPerImageValue.Enabled;

                    controlList.Add(lblDurationPerImg);

                    Label lblTRemainVal = new Label();
                    lblTRemainVal.Name = lblTimeRemainingValue.Name;
                    lblTRemainVal.Text = lblTimeRemainingValue.Text;
                    lblTRemainVal.Enabled = lblTimeRemainingValue.Enabled;

                    controlList.Add(lblTRemainVal);

                    Label lblFolderTVal = new Label();
                    lblFolderTVal.Name = lblFolderTimeVal.Name;
                    lblFolderTVal.Text = lblFolderTimeVal.Text;
                    lblFolderTVal.Enabled = lblFolderTimeVal.Enabled;

                    controlList.Add(lblFolderTVal);

                    ProgressBar pBar = new ProgressBar();

                    if (!btnProcessImages.Text.Contains("Preparing"))
                    {
                        btnP.Text = "Preparing";
                    }
                    else
                    {
                        if (maxDirQueue > 0)
                        {
                            pBar.Value = (int)(100 - 100 * (double)(((double)dirQueue.Count() / (double)maxDirQueue)));
                            if (pBar.Value != progressBar1.Value)
                            {
                                btnP.Text = process += ".";
                            }
                        }
                        else
                        {
                            pBar.Value = 5;
                        }
                    }

                    lblImgProc.Text = "0/0";
                    btnP.Enabled = false;
                    btnC.Enabled = true;

                    Button btnSetPath = new Button();
                    btnSetPath.Name = btnSetcr2hdrPath.Name;
                    btnSetPath.Text = btnSetcr2hdrPath.Text.Replace("Set ", "");
                    btnSetPath.Enabled = false;

                    controlList.Add(btnSetPath);

                    Button btnSetImage = new Button();
                    btnSetImage.Name = btnSetImageFolder.Name;
                    btnSetImage.Text = btnSetImageFolder.Text.Replace("Set ", "");
                    btnSetImage.Enabled = false;

                    controlList.Add(btnSetImage);

                    TextBox txt = new TextBox();
                    txt.Name = txtCr2hdrPath.Name;
                    txt.Text = txtCr2hdrPath.Text;
                    txt.Enabled = false;

                    controlList.Add(txt);

                    TextBox txt3 = new TextBox();
                    txt3.Name = txtImageFolderPath.Name;
                    txt3.Text = txtImageFolderPath.Text;
                    txt3.Enabled = false;

                    controlList.Add(txt3);

                    ((BackgroundWorker)(sender)).ReportProgress(pBar.Value, controlList);
                }
                else
                {
                    //Set Buttons enabled or disabled
                    try
                    {
                        if (bwCancel.IsBusy)
                        {
                            //Cancelling
                            List<Control> controlList = new List<Control>();
                            isResting = false;

                            Label lblT = new Label();
                            lblT.Name = lblThreadsVal.Name;
                            lblT.Text = threadCount.ToString() + "/" + maxThreads.ToString();

                            controlList.Add(lblT);

                            Label lblErr = new Label();
                            lblErr.Name = lblErrorsVal.Name;
                            lblErr.Text = errorCount.ToString();
                            if (errorCount > 0)
                            {
                                lblErr.ForeColor = Color.Red;
                            }

                            controlList.Add(lblErr);

                            Label lblImgProc = new Label();
                            lblImgProc.Name = lblImagesProcessedVal.Name;
                            lblImgProc.Text = imagesProcessed.ToString();
                            lblImgProc.Enabled = lblImagesProcessedVal.Enabled;

                            controlList.Add(lblImgProc);

                            Label lblElapVal = new Label();
                            lblElapVal.Name = lblElapsedValue.Name;
                            lblElapVal.Text = elapsedTime;
                            lblElapVal.Enabled = lblElapsedValue.Enabled;

                            controlList.Add(lblElapVal);

                            Label lblDurationPerImg = new Label();
                            lblDurationPerImg.Name = lblDurationPerImageValue.Name;
                            lblDurationPerImg.Text = durationPerImage;
                            lblDurationPerImg.Enabled = lblDurationPerImageValue.Enabled;

                            controlList.Add(lblDurationPerImg);

                            Label lblTRemainVal = new Label();
                            lblTRemainVal.Name = lblTimeRemainingValue.Name;
                            lblTRemainVal.Text = timeRemaining;
                            lblTRemainVal.Enabled = lblTimeRemainingValue.Enabled;

                            controlList.Add(lblTRemainVal);

                            Label lblFolderTVal = new Label();
                            lblFolderTVal.Name = lblFolderTimeVal.Name;
                            lblFolderTVal.Text = folderTimeRemaining;
                            lblFolderTVal.Enabled = lblFolderTimeVal.Enabled;

                            controlList.Add(lblFolderTVal);

                            Button btnP = new Button();
                            btnP.Name = btnProcessImages.Name;
                            btnP.Text = btnProcessImages.Text;
                            btnP.Enabled = false;

                            controlList.Add(btnP);

                            ProgressBar pBar = new ProgressBar();
                            pBar.Name = progressBar1.Name;
                            pBar.Enabled = progressBar1.Enabled;
                            pBar.Value = progressBar1.Value;

                            Button btnCncl = new Button();
                            btnCncl.Name = btnCancel.Name;
                            btnCncl.Enabled = false;

                            if (!btnCancel.Text.Contains("Canceling"))
                            {
                                pBar.Value = 99;
                                btnCncl.Text = "Canceling";
                            }
                            else
                            {
                                string processText = "Process Images";
                                double valueSlices = 100 / maxThreads;
                                int processSubStrSlices = (int)Math.Round((double)processText.Length / (double)maxThreads);
                                pBar.Value = (int)(threadCount * valueSlices);
                                btnP.Text = processText.Substring(
                                    (processSubStrSlices * threadCount) > processText.Length ? processText.Length - 1 : (processSubStrSlices * threadCount),
                                    processText.Length - (processSubStrSlices * threadCount) < 0 ? 0 : processText.Length - (processSubStrSlices * threadCount));

                                btnCncl.Text = "Canceling";
                                for (int x = 0; x < (maxThreads - threadCount); x++)
                                {
                                    btnCncl.Text += ".";
                                }
                            }

                            controlList.Add(btnCncl);
                            controlList.Add(pBar);

                            Button btnSetPath = new Button();
                            btnSetPath.Name = btnSetcr2hdrPath.Name;
                            btnSetPath.Text = btnSetcr2hdrPath.Text.Replace("Set ", "");
                            btnSetPath.Enabled = false;

                            controlList.Add(btnSetPath);

                            Button btnSetImage = new Button();
                            btnSetImage.Name = btnSetImageFolder.Name;
                            if (maxSubfolderFileQueue > 0)
                            {
                                btnSetImage.Text = (1 - ((double)subFolderFileQueue.Count() / (double)maxSubfolderFileQueue)).ToString("##0.###%");
                            }
                            else
                            {
                                btnSetImage.Text = "0%";
                            }
                            btnSetImage.Enabled = false;

                            controlList.Add(btnSetImage);

                            TextBox txt = new TextBox();
                            txt.Name = txtCr2hdrPath.Name;
                            txt.Text = txtCr2hdrPath.Text;
                            txt.Enabled = false;

                            controlList.Add(txt);

                            TextBox txt3 = new TextBox();
                            txt3.Name = txtImageFolderPath.Name;
                            txt3.Text = txtImageFolderPath.Text;
                            if (initialDirectory != string.Empty)
                            {
                                txt3.Text = initialDirectory;
                            }
                            txt3.Enabled = false;

                            controlList.Add(txt3);

                            ((BackgroundWorker)(sender)).ReportProgress(pBar.Value, controlList);
                        }
                        //Doing work. Set relevant UI disabled
                        else if ((bwCumulativeStopWatch.IsBusy || bwPreparing.IsBusy || bwProgressBar.IsBusy || businessBusy || setControlsBusy) &&
                            !bwCancel.IsBusy)
                        {
                            List<Control> controlList = new List<Control>();
                            isResting = false;

                            Label lblT = new Label();
                            lblT.Name = lblThreadsVal.Name;
                            lblT.Text = threadCount.ToString() + "/" + maxThreads.ToString();

                            controlList.Add(lblT);

                            Label lblErr = new Label();
                            lblErr.Name = lblErrorsVal.Name;
                            lblErr.Text = errorCount.ToString();
                            if (errorCount > 0)
                            {
                                lblErr.ForeColor = Color.Red;
                            }

                            controlList.Add(lblErr);

                            Label lblImgProc = new Label();
                            lblImgProc.Name = lblImagesProcessedVal.Name;
                            lblImgProc.Text = imagesProcessed.ToString() + "/" + maxFileQueue;
                            lblImgProc.Enabled = lblImagesProcessedVal.Enabled;

                            controlList.Add(lblImgProc);

                            Label lblElapVal = new Label();
                            lblElapVal.Name = lblElapsedValue.Name;
                            lblElapVal.Text = elapsedTime;
                            lblElapVal.Enabled = lblElapsedValue.Enabled;

                            controlList.Add(lblElapVal);

                            Label lblDurationPerImg = new Label();
                            lblDurationPerImg.Name = lblDurationPerImageValue.Name;
                            lblDurationPerImg.Text = durationPerImage;
                            lblDurationPerImg.Enabled = lblDurationPerImageValue.Enabled;

                            controlList.Add(lblDurationPerImg);

                            Label lblTRemainVal = new Label();
                            lblTRemainVal.Name = lblTimeRemainingValue.Name;
                            lblTRemainVal.Text = timeRemaining;
                            lblTRemainVal.Enabled = lblTimeRemainingValue.Enabled;

                            controlList.Add(lblTRemainVal);

                            Label lblFolderTVal = new Label();
                            lblFolderTVal.Name = lblFolderTimeVal.Name;
                            lblFolderTVal.Text = folderTimeRemaining;
                            lblFolderTVal.Enabled = lblFolderTimeVal.Enabled;

                            controlList.Add(lblFolderTVal);

                            Button btnP = new Button();
                            btnP.Name = btnProcessImages.Name;
                            btnP.Text = percentComplete.ToString("##0.###%");
                            btnP.Enabled = false;

                            controlList.Add(btnP);

                            Button btnCncl = new Button();
                            btnCncl.Name = btnCancel.Name;
                            btnCncl.Text = btnCancel.Text;
                            btnCncl.Enabled = true;

                            controlList.Add(btnCncl);

                            Button btnSetPath = new Button();
                            btnSetPath.Name = btnSetcr2hdrPath.Name;
                            btnSetPath.Text = btnSetcr2hdrPath.Text.Replace("Set ", "");
                            btnSetPath.Enabled = false;

                            controlList.Add(btnSetPath);

                            Button btnSetImage = new Button();
                            btnSetImage.Name = btnSetImageFolder.Name;
                            if (maxSubfolderFileQueue > 0)
                            {
                                btnSetImage.Text = (1 - ((double)subFolderFileQueue.Count() / (double)maxSubfolderFileQueue)).ToString("##0.###%");
                            }
                            else
                            {
                                btnSetImage.Text = "0%";
                            }
                            btnSetImage.Enabled = false;

                            controlList.Add(btnSetImage);

                            TextBox txt = new TextBox();
                            txt.Name = txtCr2hdrPath.Name;
                            txt.Text = txtCr2hdrPath.Text;
                            txt.Enabled = false;

                            controlList.Add(txt);

                            TextBox txt3 = new TextBox();
                            txt3.Name = txtImageFolderPath.Name;
                            txt3.Text = currentDirectory;
                            txt3.SelectionStart = txt3.Text.Length - 1;
                            txt3.SelectionLength = 0;
                            txt3.Enabled = false;

                            controlList.Add(txt3);

                            ((BackgroundWorker)(sender)).ReportProgress(Convert.ToInt32(percentComplete * 100), controlList);
                        }
                        else
                        {
                            //No longer doing work. Set everything back to resting state
                            List<Control> controlList = new List<Control>();
                            isResting = true;

                            Label lblT = new Label();
                            lblT.Name = lblThreadsVal.Name;
                            lblT.Text = threadCount.ToString() + "/" + maxThreads.ToString();

                            controlList.Add(lblT);

                            Label lblErr = new Label();
                            lblErr.Name = lblErrorsVal.Name;
                            lblErr.Text = lblErrorsVal.Text;
                            lblErr.ForeColor = lblErrorsVal.ForeColor;

                            controlList.Add(lblErr);

                            Label lblImgProc = new Label();
                            lblImgProc.Name = lblImagesProcessedVal.Name;
                            lblImgProc.Text = lblImagesProcessedVal.Text;
                            lblImgProc.Enabled = lblImagesProcessedVal.Enabled;

                            controlList.Add(lblImgProc);

                            Label lblElapVal = new Label();
                            lblElapVal.Name = lblElapsedValue.Name;
                            lblElapVal.Text = lblElapsedValue.Text;
                            lblElapVal.Enabled = lblElapsedValue.Enabled;

                            controlList.Add(lblElapVal);

                            Label lblDurationPerImg = new Label();
                            lblDurationPerImg.Name = lblDurationPerImageValue.Name;
                            lblDurationPerImg.Text = lblDurationPerImageValue.Text;
                            lblDurationPerImg.Enabled = lblDurationPerImageValue.Enabled;

                            controlList.Add(lblDurationPerImg);

                            Label lblTRemainVal = new Label();
                            lblTRemainVal.Name = lblTimeRemainingValue.Name;
                            lblTRemainVal.Text = lblTimeRemainingValue.Text;
                            lblTRemainVal.Enabled = lblTimeRemainingValue.Enabled;

                            controlList.Add(lblTRemainVal);

                            Button btnP = new Button();
                            btnP.Name = btnProcessImages.Name;
                            btnP.Text = "Process Images";
                            btnP.Enabled = true;

                            controlList.Add(btnP);

                            Button btnC = new Button();
                            btnC.Name = btnCancel.Name;
                            btnC.Text = "Cancel";
                            btnC.Enabled = false;

                            controlList.Add(btnC);

                            Button btn = new Button();
                            btn.Name = btnSetcr2hdrPath.Name;
                            btn.Text = "Set cr2hdr Path";
                            btn.Enabled = true;

                            controlList.Add(btn);

                            Button btn3 = new Button();
                            btn3.Name = btnSetImageFolder.Name;
                            btn3.Text = "Set Image Folder";
                            btn3.Enabled = true;

                            controlList.Add(btn3);

                            TextBox txt = new TextBox();
                            txt.Name = txtCr2hdrPath.Name;
                            txt.Text = txtCr2hdrPath.Text;
                            txt.Enabled = true;

                            controlList.Add(txt);

                            TextBox txt3 = new TextBox();
                            txt3.Name = txtImageFolderPath.Name;
                            txt3.Text = txtImageFolderPath.Text;
                            txt3.Enabled = true;

                            controlList.Add(txt3);

                            ((BackgroundWorker)(sender)).ReportProgress(0, controlList);

                            elapsedTime = "00:00:00";
                            durationPerImage = "00:00:00.000";

                            if (workCompleted)
                            {
                                workCompleted = false;
                                string msg = "Work Completed. All files in image folder have been processed.";

                                if (errorCount > 0)
                                {
                                    msg += "\r\n\r\nThere were errors.";
                                }

                                //Now that the work is completed, the user can try again and if successful, cleanup can happen.
                                errorCount = 0;

                                FindAndMoveMsgBox("Work Status", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                                MessageBox.Show(this, msg, "Work Status");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Directory.Exists(txtCr2hdrPath.Text))
                        {
                            File.WriteAllText(txtCr2hdrPath.Text + "\\ExceptionLog." + Guid.NewGuid().ToString() + ".log", ex.Message + "\r\n" + ex.StackTrace);
                        }
                    }
                }
                Thread.Sleep(250);
            } while (!formClosed);
        }


        private void BwCumulativeStopWatch_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                elapsedTime = cwStopwatch.Elapsed.Milliseconds > 0 ? cwStopwatch.Elapsed.ToString(@"hh\:mm\:ss\.FFF", null) :
                    cwStopwatch.Elapsed.ToString(@"hh\:mm\:ss", null);

                long fileQueueCount = fileQueue.Count();
                long milliDurationPerImage = 0;

                if (imagesProcessed > 0)
                {
                    milliDurationPerImage = cwStopwatch.ElapsedMilliseconds / imagesProcessed;
                }

                if (fileQueueCount > 0)
                {
                    fileQueueBeforeEmpty = fileQueueCount;
                }

                TimeSpan durationTimeSpan = TimeSpan.FromMilliseconds(milliDurationPerImage);
                durationPerImage = durationTimeSpan.Milliseconds > 0 ? durationTimeSpan.ToString(@"hh\:mm\:ss\.FFF", null) :
                    durationTimeSpan.ToString(@"hh\:mm\:ss", null);

                TimeSpan timeRemainingTimeSpan = TimeSpan.FromMilliseconds(milliDurationPerImage * (maxFileQueue - imagesProcessed));
                timeRemaining = string.Format("{0:D2} Days, {1:D2} Hours, {2:D2} Minutes, {3:D2} Seconds, {4:D2} Milliseconds", timeRemainingTimeSpan.Days,
                    timeRemainingTimeSpan.Hours, timeRemainingTimeSpan.Minutes, timeRemainingTimeSpan.Seconds, timeRemainingTimeSpan.Milliseconds);

                TimeSpan folderTimeRemainingTimeSpan = TimeSpan.FromMilliseconds(milliDurationPerImage * (maxSubfolderFileQueue -
                    (maxSubfolderFileQueue - subFolderFileQueue.Count())));
                folderTimeRemaining = string.Format("{0:D2} Days, {1:D2} Hours, {2:D2} Minutes, {3:D2} Seconds, {4:D2} Milliseconds",
                    folderTimeRemainingTimeSpan.Days, folderTimeRemainingTimeSpan.Hours, folderTimeRemainingTimeSpan.Minutes,
                    folderTimeRemainingTimeSpan.Seconds, folderTimeRemainingTimeSpan.Milliseconds);
            } while (!((BackgroundWorker)(sender)).CancellationPending && !formClosed);

            cwStopwatch.Stop();

            e.Cancel = true;
        }

        private void BwPreparing_DoWork(object sender, DoWorkEventArgs e)
        {
            preparing = true;
            process = "Preparing";
            swPreparing.Start();
            DirectoryInfo di = new DirectoryInfo(txtImageFolderPath.Text);
            Stopwatch diListStopwatch = new Stopwatch();
            Stopwatch fileListStopwatch = new Stopwatch();

            //Prepare and Setup File Queues for Processing
            if (di.Exists)
            {
                try
                {
                    diListStopwatch.Start();
                    IEnumerable<DirectoryInfo> diList = di.EnumerateDirectories("*", SearchOption.AllDirectories).Where(
                        d => d.EnumerateFiles("*.dng", SearchOption.TopDirectoryOnly).Union(d.EnumerateFiles("*.cr2",
                        SearchOption.TopDirectoryOnly)).Count() > 0 && !d.Name.Contains("Dual ISO DNG") && !d.Name.Contains("Dual ISO CR2") &&
                        !d.Name.Contains("Dual ISO Original CR2") && !d.Name.Contains("logs"));

                    dirQueue = new ConcurrentQueue<DirectoryInfo>(diList);
                    maxDirQueue = dirQueue.Count;

                    diListStopwatch.Stop();
                    //How long did it take to get a directory listing and build dir queue?
                    string diListDuration = diListStopwatch.Elapsed.ToString();

                    fileListStopwatch.Start();
                    List<FileInfo> fileList = new List<FileInfo>();
                    fileList.AddRange(di.EnumerateFiles("*.dng", SearchOption.TopDirectoryOnly).Union(di.EnumerateFiles("*.cr2",
                        SearchOption.TopDirectoryOnly)).ToList());
                    foreach (DirectoryInfo dInfo in diList)
                    {
                        string path = dInfo.FullName;
                        fileList.AddRange(dInfo.EnumerateFiles("*.dng", SearchOption.TopDirectoryOnly).Union(dInfo.EnumerateFiles("*.cr2",
                            SearchOption.TopDirectoryOnly)).ToList());
                        DirectoryInfo dInfoDequeue;
                        dirQueue.TryDequeue(out dInfoDequeue);
                    }
                    fileQueue = new ConcurrentQueue<FileInfo>(fileList);

                    maxFileQueue = fileQueue.Count;

                    if (maxFileQueue > 0)
                    {
                        FileInfo subfolderFileInfo;
                        fileQueue.TryPeek(out subfolderFileInfo);

                        if (subfolderFileInfo != null)
                        {
                            subFolderFileQueue = new ConcurrentQueue<FileInfo>(
                                subfolderFileInfo.Directory.EnumerateFiles("*.dng", SearchOption.TopDirectoryOnly).Union(
                                    subfolderFileInfo.Directory.EnumerateFiles("*.cr2", SearchOption.TopDirectoryOnly)).ToList()
                                );

                            maxSubfolderFileQueue = subFolderFileQueue.Count();
                        }
                    }

                    fileListStopwatch.Stop();
                    //How long did it take to get all file listings and build file queue?
                    string fileListDuration = fileListStopwatch.Elapsed.ToString();

                    btnBusiness_Click(this, new EventArgs());
                }
                catch (Exception ex)
                {
                    if (di != null)
                    {
                        File.WriteAllText(di.FullName + "\\ExceptionLog." + Guid.NewGuid().ToString() + ".log", ex.Message + "\r\n" + ex.StackTrace);
                    }
                }
            }
            else
            {
                FindAndMoveMsgBox("Path Error", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Image Folder Path Invalid", "Path Error");
            }

            e.Cancel = true;
        }

        private void BwBusiness_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch directoryStopwatch = new Stopwatch();
            threadCount++;

            do
            {
                bool errorFlag = false;

                if (!directoryStopwatch.IsRunning)
                {
                    directoryStopwatch.Start();
                }

                FileInfo fiPeek;
                fileQueue.TryPeek(out fiPeek);

                try
                {
                    Stopwatch swFile = new Stopwatch();
                    swFile.Start();
                    FileInfo fi;
                    while (!fileQueue.TryDequeue(out fi) && fileQueue.Count() != 0)
                    {

                    }

                    if (fi != null && File.Exists(fi.FullName))
                    {
                        bool cr2Flag = false;
                        string filePath = fi.FullName;
                        string directoryPath = fi.DirectoryName;
                        currentDirectory = directoryPath;

                        //Create Subfolders for Processed DNGs and Logs
                        if (fi.Name.ToLower().Contains(".dng"))
                        {
                            fi.Directory.CreateSubdirectory("Dual ISO DNG");
                        }
                        if (fi.Name.ToLower().Contains(".cr2"))
                        {
                            cr2Flag = true;
                            fi.Directory.CreateSubdirectory("Dual ISO CR2");
                            fi.Directory.CreateSubdirectory("Dual ISO Original CR2");
                        }
                        fi.Directory.CreateSubdirectory("logs");

                        //Handle condition where application exited before files moved
                        if (File.Exists(fi.FullName.ToLower().Replace(".dng", ".cr2")))
                        {
                            cr2Flag = true;
                        }

                        //Get initial image path pre-move
                        string initialImagePath = fi.FullName;
                        //Get initial log path pre-move
                        string initialLogPath = initialImagePath + ".log";

                        //Get log directory path
                        string logDirectoryPath = fi.Directory.EnumerateDirectories().Where(
                            d => d.Name.Contains("logs")).FirstOrDefault().FullName;

                        //Get destination log path
                        string destLogPath = fi.Directory.EnumerateDirectories().Where(
                            d => d.Name.Contains("logs")).FirstOrDefault().FullName + "\\" + fi.Name + ".log";

                        //Get destination image path
                        string destImagePath = string.Empty;
                        if (fi.Name.ToLower().Contains(".dng"))
                        {
                            if (!cr2Flag)
                            {
                                destImagePath = fi.Directory.EnumerateDirectories().Where(
                                    d => d.Name.Contains("Dual ISO DNG")).FirstOrDefault().FullName + "\\" + fi.Name;
                            }
                            else
                            {
                                destImagePath = fi.Directory.EnumerateDirectories().Where(
                                    d => d.Name.Contains("Dual ISO CR2")).FirstOrDefault().FullName + "\\" + fi.Name;
                            }
                        }
                        else if (fi.Name.ToLower().Contains(".cr2"))
                        {
                            destImagePath = fi.Directory.EnumerateDirectories().Where(
                                d => d.Name.Contains("Dual ISO Original CR2")).FirstOrDefault().FullName + "\\" + fi.Name;
                        }

                        string initialDNGPath = string.Empty;
                        string destDNGPath = string.Empty;

                        if (cr2Flag)
                        {
                            initialDNGPath = initialImagePath.ToLower().Replace(".cr2", ".dng").ToUpper();
                            destDNGPath = destImagePath.Replace("Dual ISO Original CR2", "Dual ISO CR2").ToLower().Replace(".cr2", ".dng").ToUpper();
                        }

                        string output = string.Empty;
                        string errorOutput = string.Empty;

                        if (!formClosed)
                        {
                            // Use ProcessStartInfo class.
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.CreateNoWindow = true;
                            startInfo.FileName = "\"" + txtCr2hdrPath.Text + "\"";
                            startInfo.WorkingDirectory = txtCr2hdrPath.Text.Length > 0 ? txtCr2hdrPath.Text.Substring(0, txtCr2hdrPath.Text.IndexOf(".exe")).Substring(0, txtCr2hdrPath.Text.LastIndexOf("\\")) : startInfo.WorkingDirectory;
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            startInfo.Arguments = "\"" + filePath + "\"";
                            startInfo.RedirectStandardOutput = true;
                            startInfo.RedirectStandardError = true;
                            startInfo.UseShellExecute = false;
                            Process exeProcess = Process.Start(startInfo);

                            // Start the process with the info we specified.
                            // Call WaitForExit and then the using-statement will close.
                            using (exeProcess)
                            {
                                output = exeProcess.StandardOutput.ReadToEnd();
                                errorOutput = exeProcess.StandardError.ReadToEnd();
                                exeProcess.WaitForExit();
                                output += "\r\nProcess Errored?: ";
                                if (exeProcess.ExitCode == 0)
                                {
                                    output += "No";
                                }
                                else
                                {
                                    output += "Yes";
                                    errorCount++;
                                    errorFlag = true;
                                }
                                output += "\r\n" + errorOutput;
                                output += "\r\n\r\n" + exeProcess.TotalProcessorTime.ToString() + " process duration.";

                                string processDuration = exeProcess.TotalProcessorTime.ToString();
                            }

                            //Move Image if Processing was Successful
                            if (!errorFlag)
                            {
                                if (File.Exists(initialImagePath))
                                {
                                    if (!File.Exists(destImagePath))
                                    {
                                        fi.MoveTo(destImagePath);
                                    }
                                    else
                                    {
                                        if (destImagePath.ToLower().Contains(".dng"))
                                        {
                                            fi.MoveTo(destImagePath.ToLower().Replace(".dng", "." + Guid.NewGuid().ToString() + ".dng"));
                                        }
                                        else if (destImagePath.ToLower().Contains(".cr2"))
                                        {
                                            fi.MoveTo(destImagePath.ToLower().Replace(".cr2", "." + Guid.NewGuid().ToString() + ".cr2"));
                                        }
                                    }

                                    imagesProcessed++;
                                    FileInfo subfolderFileInfoDequeued;
                                    subFolderFileQueue.TryDequeue(out subfolderFileInfoDequeued);
                                }
                            }

                            swFile.Stop();
                            output += "\r\n" + swFile.Elapsed.ToString() + " file activity duration.";

                            if (errorFlag)
                            {
                                File.WriteAllText(filePath + ".ProcessErrored.log", output);
                            }
                            else
                            {
                                File.WriteAllText(filePath + ".log", output);
                            }

                            //Move Log and perform cleanup if Processing was Successful
                            if (!errorFlag)
                            {
                                if (File.Exists(initialLogPath))
                                {
                                    if (!File.Exists(destLogPath))
                                    {
                                        File.Move(initialLogPath, destLogPath);
                                    }
                                    else
                                    {
                                        File.Move(initialLogPath, destLogPath.Replace(".log", "." + Guid.NewGuid().ToString() + ".log"));
                                    }
                                }

                                if (cr2Flag)
                                {
                                    //Move resultant DNG if CR2 was processed
                                    if (File.Exists(initialDNGPath))
                                    {
                                        if (!File.Exists(destDNGPath))
                                        {
                                            File.Move(initialDNGPath, destDNGPath);
                                        }
                                        else
                                        {
                                            File.Move(initialDNGPath, (destDNGPath.ToLower().Replace(".dng", "." + Guid.NewGuid().ToString() + ".dng")));
                                        }
                                    }
                                    else
                                    //Resultant DNG leftover while CR2 still in folder
                                    {
                                        DirectoryInfo dirInfo = new DirectoryInfo(currentDirectory);
                                        dirInfo.CreateSubdirectory("Dual ISO Original CR2");

                                        string leftoverInitialCR2Path = initialImagePath.ToLower().Replace(".dng", ".cr2").ToUpper();
                                        string leftoverDestCR2Path = destImagePath.Replace(
                                            "Dual ISO CR2", "Dual ISO Original CR2").ToLower().Replace(".dng", ".cr2").ToUpper();

                                        if (File.Exists(leftoverInitialCR2Path))
                                        {
                                            if (!File.Exists(leftoverDestCR2Path))
                                            {
                                                File.Move(leftoverInitialCR2Path, leftoverDestCR2Path);
                                            }
                                            else
                                            {
                                                File.Move(leftoverInitialCR2Path, leftoverDestCR2Path.ToLower().Replace(".cr2", "." +
                                                    Guid.NewGuid().ToString() + ".cr2"));
                                            }
                                        }
                                    }
                                }

                                string tempDurationPerImage = durationPerImage.ToString();

                                //Anything else left in the queue?
                                if (fileQueue.Count() > 0)
                                {
                                    FileInfo fiPeekNext;
                                    while (!fileQueue.TryPeek(out fiPeekNext) && fileQueue.Count() != 0)
                                    {

                                    }

                                    //Switching to a new folder? Clean up logs after making sure there are no images left in folder.
                                    if (
                                        (fiPeekNext.DirectoryName != directoryPath && Directory.EnumerateFiles(
                                            directoryPath, "*.dng", SearchOption.TopDirectoryOnly).Union(Directory.EnumerateFiles(
                                            directoryPath, "*.cr2", SearchOption.TopDirectoryOnly)).Count() == 0) || fiPeekNext == null)
                                    {
                                        directoryStopwatch.Stop();

                                        subFolderFileQueue = new ConcurrentQueue<FileInfo>(
                                            fiPeekNext.Directory.EnumerateFiles("*.dng", SearchOption.TopDirectoryOnly).Union(
                                                fiPeekNext.Directory.EnumerateFiles("*.cr2", SearchOption.TopDirectoryOnly)).ToList()
                                            );

                                        maxSubfolderFileQueue = subFolderFileQueue.Count();

                                        string directoryDuration = directoryStopwatch.Elapsed.ToString() + " Directory Duration.";
                                        directoryDuration += "\r\n" + tempDurationPerImage + " Duration Per Image.";
                                        if (!File.Exists(directoryPath + "\\directoryDuration.log"))
                                        {
                                            File.WriteAllText(directoryPath + "\\directoryDuration.log", directoryDuration);
                                        }

                                        directoryStopwatch.Reset();

                                        IEnumerable<FileInfo> logs = new DirectoryInfo(directoryPath).EnumerateFiles("*.log", SearchOption.TopDirectoryOnly);
                                        if (logs.Count() > 0)
                                        {
                                            foreach (FileInfo log in logs)
                                            {
                                                if (Directory.Exists(log.DirectoryName + "\\logs"))
                                                {
                                                    log.MoveTo(log.DirectoryName + "\\logs\\" + log.Name);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //No more files in queue. Clean up logs for the final time after making sure there are no image files remaining
                                    if (Directory.EnumerateFiles(directoryPath, "*.dng", SearchOption.TopDirectoryOnly).Union(
                                        Directory.EnumerateFiles(directoryPath, "*.cr2", SearchOption.TopDirectoryOnly)).Count() == 0)
                                    {
                                        directoryStopwatch.Stop();

                                        string directoryDuration = directoryStopwatch.Elapsed.ToString() + " Directory Duration.";
                                        directoryDuration += "\r\n" + tempDurationPerImage + " Duration Per Image.";
                                        if (!File.Exists(directoryPath + "\\directoryDuration.log"))
                                        {
                                            File.WriteAllText(directoryPath + "\\directoryDuration.log", directoryDuration);
                                        }

                                        directoryStopwatch.Reset();

                                        IEnumerable<FileInfo> logs = new DirectoryInfo(directoryPath).EnumerateFiles("*.log", SearchOption.TopDirectoryOnly);
                                        if (logs.Count() > 0)
                                        {
                                            foreach (FileInfo log in logs)
                                            {
                                                if (Directory.Exists(log.DirectoryName + "\\logs"))
                                                {
                                                    if (File.Exists(log.FullName))
                                                    {
                                                        log.MoveTo(log.DirectoryName + "\\logs\\" + log.Name);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (fiPeek != null)
                    {
                        File.WriteAllText(fiPeek.DirectoryName + "\\ExceptionLog." + fiPeek.Name + "." +
                            Guid.NewGuid().ToString() + ".log", ex.Message + "\r\n" + ex.StackTrace);
                    }
                }
            } while (fileQueue.Count() > 0 && !((BackgroundWorker)(sender)).CancellationPending && !formClosed);

            //Queue empty and last worker? Time to clean up logs and signal work completed.
            if (fileQueue.Count() == 0 && threadCount == 1)
            {
                workCompleted = true;

                if (!bwCancel.IsBusy)
                {
                    bwCancel.RunWorkerAsync();
                    Cancel();

                    //Clean up logs

                    DirectoryInfo di = new DirectoryInfo(txtImageFolderPath.Text);

                    if (di.Exists)
                    {
                        try
                        {
                            IEnumerable<DirectoryInfo> diList = di.EnumerateDirectories("*", SearchOption.AllDirectories).Where(
                                d => d.EnumerateFiles("*.log", SearchOption.TopDirectoryOnly).Count() > 0 && !d.Name.Contains("Dual ISO DNG") &&
                                !d.Name.Contains("Dual ISO CR2") && !d.Name.Contains("Dual ISO Original CR2") && !d.Name.Contains("logs"));

                            List<FileInfo> logList = new List<FileInfo>();
                            //We don't want to clean up the process errored logs, 
                            //since we want to keep those next to the files that need to be reprocessed.
                            logList.AddRange(di.EnumerateFiles("*.log", SearchOption.TopDirectoryOnly).Where(
                                lgFile => !lgFile.Name.Contains("ProcessErrored")).ToList());
                            foreach (DirectoryInfo dInfo in diList)
                            {
                                string path = dInfo.FullName;
                                logList.AddRange(dInfo.EnumerateFiles("*.log", SearchOption.TopDirectoryOnly).Where(
                                lgFile => !lgFile.Name.Contains("ProcessErrored")).ToList());
                            }

                            foreach (FileInfo log in logList)
                            {
                                if (Directory.Exists(log.DirectoryName + "\\logs"))
                                {
                                    log.MoveTo(log.DirectoryName + "\\logs\\" + log.Name);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            File.WriteAllText(di.FullName + "\\ExceptionLog.WorkComplete." + Guid.NewGuid().ToString() + ".log", ex.Message + "\r\n" + ex.StackTrace);
                        }
                    }
                }
            }

            threadCount--;
        }

        private void BwProgressBar_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                if (maxFileQueue > 0)
                {
                    double newPercentComplete = ((double)imagesProcessed / (double)maxFileQueue);
                    if (newPercentComplete != percentComplete)
                    {
                        percentComplete = newPercentComplete;
                    }
                }
            } while (!((BackgroundWorker)(sender)).CancellationPending && !formClosed);

            percentComplete = 0;
            e.Cancel = true;
        }

        private void btnSetImageFolder_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtImageFolderPath.Text))
            {
                dlgFolderBrowser.SelectedPath = this.txtImageFolderPath.Text;
            }
            else
            {
                dlgFolderBrowser.SelectedPath = Application.StartupPath;
            }

            if (DialogResult.OK == dlgFolderBrowser.ShowDialog())
            {
                txtImageFolderPath.Text = dlgFolderBrowser.SelectedPath;
                Dual_ISO_Processor.Properties.Settings.Default.ImageFolder = txtImageFolderPath.Text;
                Dual_ISO_Processor.Properties.Settings.Default.Save();
            }
        }

        private void btnSetcr2hdrPath_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtCr2hdrPath.Text))
            {
                dlgOpen.InitialDirectory = this.txtCr2hdrPath.Text;
            }
            else
            {
                dlgOpen.InitialDirectory = Application.StartupPath;
            }
            dlgOpen.Filter = "EXE File|*.exe";
            dlgOpen.Title = "cr2hdr Path";
            dlgOpen.RestoreDirectory = true;

            if (DialogResult.OK == dlgOpen.ShowDialog())
            {
                txtCr2hdrPath.Text = dlgOpen.FileName;
                Dual_ISO_Processor.Properties.Settings.Default.cr2hdrPath = txtCr2hdrPath.Text;
                Dual_ISO_Processor.Properties.Settings.Default.Save();
            }
        }

        private void btnProcessImages_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtCr2hdrPath.Text))
            {
                FindAndMoveMsgBox("Path Error", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Invalid cr2hdr path.", "Path Error");
            }
            else if (!Directory.Exists(txtImageFolderPath.Text))
            {
                FindAndMoveMsgBox("Path Error", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Invalid image folder path.", "Path Error");
            }
            else
            {
                bool pathHit = false;
                bool exifHit = false;

                List<string> pathDirectories = new List<string>();

                if (System.Environment.GetEnvironmentVariables()["Path"] != null)
                {
                    pathDirectories.AddRange(System.Environment.GetEnvironmentVariables()["Path"].ToString().Split(';').ToList<string>());
                }

                if (System.Environment.GetEnvironmentVariables()["PATH"] != null)
                {
                    pathDirectories.AddRange(System.Environment.GetEnvironmentVariables()["PATH"].ToString().Split(';').ToList<string>());
                }

                if (System.Environment.GetEnvironmentVariables()["path"] != null)
                {
                    pathDirectories.AddRange(System.Environment.GetEnvironmentVariables()["path"].ToString().Split(';').ToList<string>());
                }

                //Check working folder for dependencies
                if (File.Exists(txtCr2hdrPath.Text.Substring(0, txtCr2hdrPath.Text.IndexOf(".exe")).Substring(0, txtCr2hdrPath.Text.LastIndexOf("\\"))
                    + "\\dcraw.exe"))
                {
                    pathHit = true;
                }
                if (File.Exists(txtCr2hdrPath.Text.Substring(0, txtCr2hdrPath.Text.IndexOf(".exe")).Substring(0, txtCr2hdrPath.Text.LastIndexOf("\\"))
                    + "\\exiftool.exe"))
                {
                    exifHit = true;
                }

                if (pathHit && exifHit)
                {

                }
                else
                {
                    //Check folders in PATH environment variable for dependencies
                    foreach (string path in pathDirectories)
                    {
                        if (File.Exists(path + "\\dcraw.exe"))
                        {
                            pathHit = true;
                        }
                        if (File.Exists(path + "\\exiftool.exe"))
                        {
                            exifHit = true;
                        }
                        if (pathHit && exifHit)
                        {
                            break;
                        }
                    }
                }

                if (!pathHit)
                {
                    FindAndMoveMsgBox("dcraw", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    MessageBox.Show(this, "dcraw.exe not found. \r\n\r\nPlease install and copy to the cr2hdr path or the Windows directory.", "dcraw");
                }
                else
                {
                    bool beginProcessing = true;

                    if (!exifHit)
                    {
                        FindAndMoveMsgBox("ExifTool Missing. Continue?", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                        if (MessageBox.Show(this, "exiftool.exe not found. \r\nThis is optional. \r\n\r\nIf wanted, please install and copy to the cr2hdr path or " +
                            "the Windows directory. \r\nContinue?", "ExifTool Missing. Continue?", MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            beginProcessing = false;
                        }
                    }

                    if (beginProcessing)
                    {
                        btnProcessImages.Enabled = false;

                        if (!bwPreparing.IsBusy && !bwProgressBar.IsBusy)
                        {
                            //Reset all counters
                            imagesProcessed = 0;
                            elapsedTime = string.Empty;
                            durationPerImage = string.Empty;
                            timeRemaining = string.Empty;

                            //Reset Stopwatch
                            cwStopwatch.Stop();
                            cwStopwatch.Reset();
                            cwStopwatch.Start();

                            //Start workers
                            bwPreparing.RunWorkerAsync();
                            bwProgressBar.RunWorkerAsync();

                            initialDirectory = txtImageFolderPath.Text;
                        }
                    }
                }
            }
        }

        private void Cancel(bool prepareCanceled = false)
        {
            preparing = false;

            foreach (BackgroundWorker bw in bwBusiness)
            {
                if (bw != null && bw.IsBusy)
                {
                    bw.CancelAsync();
                }
            }

            if (bwPreparing.IsBusy && prepareCanceled != true)
            {
                bwPreparing.CancelAsync();
            }

            if (bwProgressBar.IsBusy)
            {
                bwProgressBar.CancelAsync();
            }

            bwCancel.CancelAsync();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = false;

            if (bwPreparing.IsBusy)
            {
                bwPreparing.CancelAsync();
            }

            if (!bwCancel.IsBusy)
            {
                bwCancel.RunWorkerAsync();
                Cancel();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (allowedThreads != numericUpDown1.Value)
            {
                //Make sure correct number of threads are in use
                if (bwBusiness != null && bwBusiness.Count() > 0)
                {
                    allowedThreads = (int)numericUpDown1.Value;

                    //Is at least one worker busy (without the Cancel worker being busy)? 
                    //If so, preparing is complete and we can cancel and start business workers as we please.
                    if (bwBusiness[0] != null && bwBusiness[0].IsBusy && !bwCancel.IsBusy)
                    {
                        //Kill all workers greater than the allowed ones
                        for (int x = maxThreads; x > allowedThreads; x--)
                        {
                            if (bwBusiness[x - 1] != null)
                            {
                                if (bwBusiness[x - 1].IsBusy)
                                {
                                    bwBusiness[x - 1].CancelAsync();
                                }
                            }
                        }

                        //Activate all workers less or equal to the allowed ones
                        for (int x = 1; x <= allowedThreads; x++)
                        {
                            if (bwBusiness[x - 1] != null)
                            {
                                if (!bwBusiness[x - 1].IsBusy)
                                {
                                    bwBusiness[x - 1].RunWorkerAsync();
                                }
                            }
                        }
                    }
                    else
                    {
                        numericUpDown1.Value = allowedThreads;
                    }
                }
            }
        }

        private void btnBusiness_Click(object sender, EventArgs e)
        {
            bool quitPreparing = false;
            preparing = false;

            process = "Processing";
            swPreparing.Stop();
            swPreparing.Reset();

            //Begin cumulative stopwatch worker
            bwCumulativeStopWatch.RunWorkerAsync();

            int workerPosition = 0;
            foreach (BackgroundWorker bw in bwBusiness)
            {
                //Wait for business worker to stop being busy
                if (bw != null && bw.IsBusy && !quitPreparing)
                {
                    Stopwatch swBusyWorker = new Stopwatch();
                    swBusyWorker.Start();

                    while (bw != null && bw.IsBusy)
                    {
                        //Keep checking if worker is busy for 5 seconds
                        if (swBusyWorker.ElapsedMilliseconds > 5000)
                        {
                            //If worker is still busy after 5 seconds, toggle quit preparing flag
                            quitPreparing = true;
                            break;
                        }
                    }
                }
                //If quit preparing flag is true cancel Processing, otherwise run business worker
                if (quitPreparing || bwPreparing.CancellationPending)
                {
                    if (!bwCancel.IsBusy)
                    {
                        bwCancel.RunWorkerAsync();
                    }

                    Cancel(quitPreparing);
                    break;
                }
                else
                {
                    if ((workerPosition + 1) <= allowedThreads)
                    {
                        bw.RunWorkerAsync();
                    }
                }

                workerPosition++;
            }
        }
    }
}