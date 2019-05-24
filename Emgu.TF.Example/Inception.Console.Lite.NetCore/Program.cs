//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Emgu.TF.Lite.Models;
using System.Threading;

namespace Inception.Console.Lite.Netstandard
{
    class Program
    {
        private static Emgu.TF.Lite.Models.Inception inception;
        private static String fileName = "tulips.jpg";
        static void Main(string[] args)
        {
            new Thread(() => { Run(); }).Start();

            System.Console.ReadKey();
        }

        private static void Run()
        {
            inception = new Emgu.TF.Lite.Models.Inception();
            inception.OnDownloadProgressChanged += onDownloadProgressChanged;
            inception.OnDownloadCompleted += onDownloadCompleted;

            System.Console.WriteLine("Initializing inception");
            inception.Init();
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
            var result = inception.Recognize(fileName);
            watch.Stop();
            String resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", result[0].Label, result[0].Probability * 100, watch.ElapsedMilliseconds);

            System.Console.WriteLine(resStr);
            System.Console.WriteLine("Press any key to continue:");

        }
    }
}