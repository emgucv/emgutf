//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.TF;
using Emgu.TF.Models;
using System.Diagnostics;

namespace MultiboxPeopleDetection
{
    public partial class MainForm : Form
    {
        private MultiboxGraph graph;
        public MainForm()
        {
            InitializeComponent();
            TfInvoke.CheckLibraryLoaded();
            SetLabelText(String.Empty);

            graph = new MultiboxGraph();
            graph.OnDownloadProgressChanged += OnDownloadProgressChangedEventHandler;
            graph.OnDownloadCompleted += onDownloadCompleted;

            graph.Init();
        }

        public void OnDownloadProgressChangedEventHandler(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            String msg = String.Format("Downloading models, please wait... {0} of {1} bytes ({2}%) downloaded.", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
            SetLabelText(msg);
        }

        public void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Detect("surfers.jpg");
        }

        public void SetLabelText(String msg)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    messageLabel.Text = msg;

                }));
            }
            else
            {
                messageLabel.Text = msg;

            }
        }

        private bool _coldSession = true;

        public void Detect(String fileName)
        {
            Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);

            MultiboxGraph.Result[] result;
            if (_coldSession)
            {
                //First run of the detection, here we will compile the graph and initialize the session
                //This is expected to take much longer time than consecutive runs.
                result = graph.Detect(imageTensor);
                _coldSession = false;
            }

            //Here we are trying to time the execution of the graph after it is loaded
            //If we are not interest in the performance, we can skip the 3 lines that follows
            Stopwatch watch = Stopwatch.StartNew();           
            result = graph.Detect(imageTensor);
            watch.Stop();

            Bitmap bmp = new Bitmap(fileName);
            MultiboxGraph.DrawResults(bmp, result, 0.1f);

            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    RenderResult(bmp, watch.ElapsedMilliseconds);
                }));
            }
            else
            {
                RenderResult(bmp, watch.ElapsedMilliseconds);
            }
            
        }

        public void RenderResult(Bitmap bmp, long elaspseMilliseonds)
        {
            resultPictureBox.Image = bmp;

            messageLabel.Text = String.Format("Detection completed with {0} in {1} milliseconds", TfInvoke.IsGoogleCudaEnabled ? "GPU" : "CPU", elaspseMilliseonds);
        }
    }
}
