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
        public MainForm()
        {
            InitializeComponent();
            TfInvoke.CheckLibraryLoaded();
            Detect("surfers.jpg");
        }

        public void Detect(String fileName)
        {

            MultiboxGraph graph = new MultiboxGraph();
            Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);

            Stopwatch watch = Stopwatch.StartNew();           
            MultiboxGraph.Result result = graph.Detect(imageTensor);
            watch.Stop();

            Bitmap bmp = new Bitmap(fileName);
            MultiboxGraph.DrawResults(bmp, result, 0.1f);
            resultPictureBox.Image = bmp;
            messageLabel.Text = String.Format("Detection completed in {0} milliseconds", watch.ElapsedMilliseconds);
        }
    }
}
