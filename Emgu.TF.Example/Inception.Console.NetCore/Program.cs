//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Emgu.TF;
using Emgu.TF.Models;
using System.Threading.Tasks;
using Tensorflow;

namespace Inception.Console.Netstandard
{
    class Program
    {
        private static Emgu.TF.Models.Inception _inceptionGraph;
        private static FileInfo _inputFileInfo;

        static async Task Main(string[] args)
        {
            System.Console.WriteLine("Starting...");
#if DEBUG
            ConsoleTraceListener consoleTraceListener = new ConsoleTraceListener();
            Trace.Listeners.Add(consoleTraceListener);
#endif
            String fileName = Path.Join(AssemblyDirectory, "tulips.jpg");
            if (args.Length > 0)
                fileName = args[0];

            _inputFileInfo = new FileInfo(fileName);
            if (!_inputFileInfo.Exists)
            {
                System.Console.WriteLine(String.Format("File '{0}' does not exist. Please provide a valid file name as input parameter.", _inputFileInfo.FullName));
                return;
            }
            Trace.WriteLine(String.Format("Working on file {0}", _inputFileInfo.FullName));
 
            await Run();
            //System.Console.WriteLine("Press any key to continue:");
            //System.Console.ReadKey();
        }

        private static async Task Run()
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
            //_inceptionGraph.OnDownloadCompleted += onDownloadCompleted;

            //use a retrained model to recognize followers
            await _inceptionGraph.Init(
                new string[] { "optimized_graph.pb", "output_labels.txt" },
                "https://github.com/emgucv/models/raw/master/inception_flower_retrain/",
                "Placeholder",
                "final_result");

            Stopwatch watch = Stopwatch.StartNew();
            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(_inputFileInfo.FullName, 299, 299, 0.0f, 1.0f / 255.0f, false, false);
            var results = _inceptionGraph.Recognize(imageTensor);
            watch.Stop();
            String resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", results[0].Label, results[0].Probability * 100, watch.ElapsedMilliseconds);

            System.Console.WriteLine(resStr);
            
        }

        private static void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive <= 0)
                System.Console.WriteLine(String.Format("{0} bytes downloaded", e.BytesReceived, e.ProgressPercentage));
            else
                System.Console.WriteLine(String.Format("{0} of {1} bytes downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        /// <summary>
        /// Get the directory from the assembly
        /// </summary>
        /// <remarks>https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in</remarks>
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
