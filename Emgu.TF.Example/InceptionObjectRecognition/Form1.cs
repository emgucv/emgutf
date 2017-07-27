using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

            Inception inceptionGraph = new Inception();
            Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);
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

            //MultiboxGraph graph = new MultiboxGraph();
            //Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);
            //MultiboxGraph.Result result = graph.Detect(imageTensor);

            //Bitmap bmp = new Bitmap(fileName);
            //MultiboxGraph.DrawResults(bmp, result, 0.1f);
            //resultPictureBox.Image = bmp;
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
