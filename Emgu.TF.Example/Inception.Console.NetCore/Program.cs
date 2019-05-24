//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;
using Emgu.TF;
using Emgu.TF.Models;
using Tensorflow;

namespace Inception.Console.Netstandard
{
    class Program
    {
        private static Emgu.TF.Models.Inception _inceptionGraph;
        private static String fileName = "tulips.jpg";

        static void Main(string[] args)
        {
            new Thread(() => { Run(); }).Start();

            System.Console.ReadKey();
        }

        private static void Run()
        {
            SessionOptions so = new SessionOptions();
            if (TfInvoke.IsGoogleCudaEnabled)
            {
                Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
                config.GpuOptions = new Tensorflow.GPUOptions();
                config.GpuOptions.AllowGrowth = true;
                so.SetConfig(config.ToProtobuf());
            }
            _inceptionGraph = new Emgu.TF.Models.Inception(null, so);
            _inceptionGraph.OnDownloadProgressChanged += onDownloadProgressChanged;
            _inceptionGraph.OnDownloadCompleted += onDownloadCompleted;

            //use a retrained model to recognize followers
            _inceptionGraph.Init(
                new string[] { "optimized_graph.pb", "output_labels.txt" },
                "https://github.com/emgucv/models/raw/master/inception_flower_retrain/",
                "Placeholder",
                "final_result");
        }

        private static void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive <= 0)
                System.Console.WriteLine(String.Format("{0} bytes downloaded", e.BytesReceived, e.ProgressPercentage));
            else
                System.Console.WriteLine(String.Format("{0} of {1} bytes downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private static void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            Stopwatch watch = Stopwatch.StartNew();
            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(fileName, 299, 299, 0.0f, 1.0f / 255.0f, false, false);
            var results = _inceptionGraph.Recognize(imageTensor);
            watch.Stop();
            String resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", results[0].Label, results[0].Probability * 100, watch.ElapsedMilliseconds);


            System.Console.WriteLine(resStr);
            System.Console.WriteLine("Press any key to continue:");

        }

    }
}
