using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emgu.TF;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Emgu.TF.Models;
using Tensorflow;

namespace Emgu.TF.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGetVersion()
        {
            String version = Emgu.TF.TfInvoke.Version;
            Trace.WriteLine(String.Format("Tensorflow version: {0}", version));
        }

        [TestMethod]
        public void TestInception()
        {
            Tensor imageTensor = ImageIO.ReadTensorFromImageFile("grace_hopper.jpg", 224, 224, 128.0f, 1.0f);

            Inception inceptionGraph = new Inception();
            
            float[] probability = inceptionGraph.Recognize(imageTensor);
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
                Trace.WriteLine(String.Format("Object is {0} with {1}% probability", labels[maxIdx], maxVal * 100));
            }
        }

        [TestMethod]
        public void TestMultiboxPeopleDetect()
        {

            Tensor imageResults = ImageIO.ReadTensorFromImageFile("surfers.jpg", 224, 224, 128.0f, 1.0f / 128.0f);

            MultiboxGraph multiboxGraph = new MultiboxGraph();

            MultiboxGraph.Result result = multiboxGraph.Detect(imageResults);

            Bitmap bmp = new Bitmap("surfers.jpg");
            MultiboxGraph.DrawResults(bmp, result, 0.1f);
            bmp.Save("surfers_result.jpg");
        }

        
        [TestMethod]
        public void TestStylize()
        {
            StylizeGraph stylizeGraph = new StylizeGraph();
            Tensor image = ImageIO.ReadTensorFromImageFile("surfers.jpg");
            Tensor stylizedImage = stylizeGraph.Stylize(image, 0);
        }

        [TestMethod]
        public void TestTensorCreate()
        {
            Tensor t = new Tensor(DataType.Float, new []{1, 3, 224, 224});
        }

        [TestMethod]
        public void TestEncodeJpeg()
        {
            Tensor image = ImageIO.ReadTensorFromImageFile("surfers.jpg");
            byte[] jpegRaw = ImageIO.EncodeJpeg(image);
            File.WriteAllBytes("surefers_out.jpg", jpegRaw);
        }
    }
}
