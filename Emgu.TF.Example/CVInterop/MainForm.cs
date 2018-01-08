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
using Emgu.CV;


namespace CVInterop
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            TfInvoke.CheckLibraryLoaded();
            messageLabel.Text = String.Empty;

            Recognize("space_shuttle.jpg");

        }

        public static Tensor ReadTensorFromMatBgr(Mat image, int inputHeight = -1, int inputWidth = -1, float inputMean = 0.0f, float scale = 1.0f, Status status = null)
        {
            if (image.NumberOfChannels != 3)
            {
                throw new ArgumentException("Input must be 3 channel BGR image");
            }

            Emgu.CV.CvEnum.DepthType depth = image.Depth;
            if (!(depth == Emgu.CV.CvEnum.DepthType.Cv8U || depth == Emgu.CV.CvEnum.DepthType.Cv32F))
            {
                throw new ArgumentException("Input image must be 8U or 32F");
            }

            //resize
            int finalHeight = inputHeight == -1 ? image.Height : inputHeight;
            int finalWidth = inputWidth == -1 ? image.Width : inputWidth;
            Size finalSize = new Size(finalWidth, finalHeight);

            if (image.Size != finalSize)
            {
                using (Mat tmp = new Mat())
                {
                    CvInvoke.Resize(image, tmp, finalSize);
                    return ReadTensorFromMatBgrF(tmp, inputMean, scale);
                }
            }
            else
            {
                return ReadTensorFromMatBgrF(image, inputMean, scale);
            }
        }

        private static Tensor ReadTensorFromMatBgrF(Mat image, float inputMean, float scale)
        {
            Tensor t = new Tensor(DataType.Float, new int[] { 1, image.Height, image.Width, 3 });
            using (Mat matF = new Mat(image.Size, Emgu.CV.CvEnum.DepthType.Cv32F, 3, t.DataPointer, sizeof(float) * 3 * image.Width))
            {
                image.ConvertTo(matF, Emgu.CV.CvEnum.DepthType.Cv32F, scale, -inputMean * scale);
            }
            return t;
        }

        public void Recognize(String fileName)
        {
            fileNameTextBox.Text = fileName;
            pictureBox.ImageLocation = fileName;
            
            using (Mat m = CvInvoke.Imread(fileName))
            {
                //Use the following code for the full inception model
                Inception inceptionGraph = new Inception();
                Tensor imageTensor = ReadTensorFromMatBgr(m, 224, 224, 128.0f, 1.0f / 128.0f);

                //Uncomment the following code to use a retrained model to recognize followers, downloaded from the internet
                //Inception inceptionGraph = new Inception(null, new string[] {"optimized_graph.pb", "output_labels.txt"}, "https://github.com/emgucv/models/raw/master/inception_flower_retrain/", "Mul", "final_result");
                //Tensor imageTensor = ImageIO.ReadTensorFromMatBgr(fileName, 299, 299, 128.0f, 1.0f / 128.0f);

                //Uncomment the following code to use a retrained model to recognize followers, if you deployed the models with the application
                //For ".pb" and ".txt" bundled with the application, set the url to null
                //Inception inceptionGraph = new Inception(null, new string[] {"optimized_graph.pb", "output_labels.txt"}, null, "Mul", "final_result");
                //Tensor imageTensor = ImageIO.ReadTensorFromMatBgr(fileName, 299, 299, 128.0f, 1.0f / 128.0f);

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
