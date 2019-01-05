//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.TF;
using Emgu.TF.Models;
using System.Diagnostics;

namespace InceptionObjectRecognition
{
    public partial class MainForm : Form
    {
        private Inception inceptionGraph;

        public MainForm()
        {
            InitializeComponent();

            TfInvoke.CheckLibraryLoaded();
            messageLabel.Text = String.Empty;

            DisableUI();

            
            inceptionGraph = new Inception();
            inceptionGraph.OnDownloadProgressChanged += OnDownloadProgressChangedEventHandler;
            inceptionGraph.OnDownloadCompleted += onDownloadCompleted;

            //Use the following code for the full inception model
            inceptionGraph.Init();

            //Uncomment the following code to use a retrained model to recognize followers, downloaded from the internet
            //inceptionGraph.Init(new string[] {"optimized_graph.pb", "output_labels.txt"}, "https://github.com/emgucv/models/raw/master/inception_flower_retrain/", "Mul", "final_result");
        }

        public void OnDownloadProgressChangedEventHandler(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            String msg = String.Format("Downloading models, please wait... {0} of {1} bytes ({2}%) downloaded.", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
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

        public void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            EnableUI();
            Recognize("space_shuttle.jpg");
        }

        public void DisableUI()
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    openFileButton.Enabled = false;
                }));
            }
            else
            {
                openFileButton.Enabled = false;
            }
        }

        public void EnableUI()
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker) EnableUI);
            }
            else
            {
                openFileButton.Enabled = true;
            }

        }

        private bool _coldSession = true;

        public void Recognize(String fileName)
        {            
            Tensor imageTensor = ImageIO.ReadTensorFromImageFile<float>(fileName, 224, 224, 128.0f, 1.0f / 128.0f);

            float[] probability;
            if (_coldSession)
            {
                //First run of the recognition graph, here we will compile the graph and initialize the session
                //This is expected to take much longer time than consecutive runs.
                probability = inceptionGraph.Recognize(imageTensor);
                _coldSession = false;
            }


            //Here we are trying to time the execution of the graph after it is loaded
            //If we are not interest in the performance, we can skip the 3 lines that follows
            Stopwatch sw = Stopwatch.StartNew();
            probability = inceptionGraph.Recognize(imageTensor);
            sw.Stop();

            String resStr = String.Empty;
            if (probability != null)
            {
                String[] labels = inceptionGraph.Labels;
                float maxVal = 0;
                int maxIdx = 0;
                for (int i = 0; i < probability.Length; i++)
                {
                    if (probability[i] > maxVal)
                    {
                        maxVal = probability[i];
                        maxIdx = i;
                    }
                }
                resStr = String.Format("Object is {0} with {1}% probability. Recognition done in {2} in {3} milliseconds.", labels[maxIdx], maxVal * 100, TfInvoke.IsGoogleCudaEnabled ? "GPU" : "CPU", sw.ElapsedMilliseconds);
            }

            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    fileNameTextBox.Text = fileName;
                    pictureBox.ImageLocation = fileName;
                    messageLabel.Text = resStr;
                }));
            }
            else
            {
                fileNameTextBox.Text = fileName;
                pictureBox.ImageLocation = fileName;
                messageLabel.Text = resStr;
            }

        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofd.FileName;
                Recognize(fileName);
            }
        }
    }
}
