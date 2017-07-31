//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.TF;
using Emgu.TF.Models;

namespace InceptionObjectRecognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            TfInvoke.CheckLibraryLoaded();
            messageLabel.Text = String.Empty;

            Recognize("space_shuttle.jpg");

        }

        public void Recognize(String fileName)
        {
            fileNameTextBox.Text = fileName;
            pictureBox.ImageLocation = fileName;

            //Use the following code for the full inception model
            Inception inceptionGraph = new Inception();
            Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);

            //Uncomment the following code to use a retrained model to recognize followers, downloaded from the internet
            //Inception inceptionGraph = new Inception(null, new string[] {"optimized_graph.pb", "output_labels.txt"}, "https://github.com/emgucv/models/raw/master/inception_flower_retrain/", "Mul", "final_result");
            //Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 299, 299, 128.0f, 1.0f / 128.0f);

            //Uncomment the following code to use a retrained model to recognize followers, if you deployed the models with the application
            //For ".pb" and ".txt" bundled with the application, set the url to null
            //Inception inceptionGraph = new Inception(null, new string[] {"optimized_graph.pb", "output_labels.txt"}, null, "Mul", "final_result");
            //Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 299, 299, 128.0f, 1.0f / 128.0f);

            float[] probability = inceptionGraph.Recognize(imageTensor);
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
                resStr = String.Format("Object is {0} with {1}% probability.", labels[maxIdx], maxVal * 100);
            }
            messageLabel.Text = resStr;

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
