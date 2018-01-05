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
using Google.Protobuf;

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
        public void TestLoadLargeGraph()
        {
            Inception inceptionGraph = new Inception(null, new string[] { "optimized_graph.pb", "output_labels.txt" }, "https://github.com/emgucv/models/raw/master/inception_flower_retrain/", "Mul", "final_result");
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

        [TestMethod]
        public void TestCUDAEnabled()
        {
            bool cuda = TfInvoke.IsGoogleCudaEnabled;
            Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
            config.LogDevicePlacement = true;
            byte[] pbuff;
            using (MemoryStream ms = new MemoryStream())
            {
                config.WriteTo(ms);
                pbuff = ms.ToArray();
            }
            SessionOptions options = new SessionOptions();
            options.SetConfig(pbuff);
            Add(3, 4, options);
        }

        [TestMethod]
        public void TestChooseDevice()
        {
            SessionOptions so = new SessionOptions();
            Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
            config.LogDevicePlacement = true;
            using (MemoryStream ms = new MemoryStream())
            using (Google.Protobuf.CodedOutputStream stream = new CodedOutputStream(ms))
            {
               config.WriteTo(stream);
                byte[] bytes = ms.ToArray();
                so.SetConfig(bytes);
            }
            int sum = Add(1, 2, so);
            
        }

        public static int Add(int a, int b, SessionOptions sessionOptions = null)
        {
            Tensor tensorA = new Tensor(a);
            Tensor tensorB = new Tensor(b);
            Graph graph = new Graph();
            Operation opA = graph.Placeholder(DataType.Int32, null, "valA");
            Operation opB = graph.Placeholder(DataType.Int32, null, "valB");
            Operation sumOp = graph.Add(opA, opB, "sum");

            Session session = new Session(graph, sessionOptions);
            Tensor[] results = session.Run(new Output[] { opA, opB }, new Tensor[] { tensorA, tensorB }, new Output[] { sumOp });
            return results[0].Flat<int>()[0];
        }

        [TestMethod]
        public void TestAddition()
        {
            int a = 10;
            int b = 20;
            int sum = Add(a, b);
        }

        [TestMethod]
        public void TestHelloWorld()
        {
            String h = "Hello, tensorflow!";

            Tensor hello = Tensor.FromString(System.Text.Encoding.Default.GetBytes(h));
            Graph g = new Graph();
            Operation helloOp = g.Const(hello, DataType.String);
            Session session = new Session(g);
            Tensor[] results = session.Run(new Output[] {}, new Tensor[] {}, new Output[] {helloOp});
            byte[] data = results[0].DecodeString();
            String output = System.Text.Encoding.Default.GetString(data);

        }
    }
}
