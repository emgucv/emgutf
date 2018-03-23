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
            graph.Init(
                onDownloadProgressChanged: OnDownloadProgressChangedEventHandler,
                onDownloadFileCompleted:
                (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                {
                    Detect("surfers.jpg");
                }
                );
        }

        public void OnDownloadProgressChangedEventHandler(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            String msg = String.Format("Downloading models, please wait... {0} of {1} bytes ({2}%) downloaded.", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
            SetLabelText(msg);
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

        public void Detect(String fileName)
        {

            Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);

            Stopwatch watch = Stopwatch.StartNew();           
            MultiboxGraph.Result result = graph.Detect(imageTensor);
            watch.Stop();

            Bitmap bmp = new Bitmap(fileName);
            MultiboxGraph.DrawResults(bmp, result, 0.1f);


            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    resultPictureBox.Image = bmp;
                    messageLabel.Text = String.Format("Detection completed in {0} milliseconds", watch.ElapsedMilliseconds);
                }));
            }
            else
            {
                resultPictureBox.Image = bmp;
                messageLabel.Text = String.Format("Detection completed in {0} milliseconds", watch.ElapsedMilliseconds);
            }
            
        }
    }
}
