//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Emgu.TF.Lite.Models;
using System.Threading;

namespace Inception.Console.Lite.Netstandard
{
    class Program
    {
        private static Emgu.TF.Lite.Models.Inception inception;
        private static FileInfo _inputFileInfo;

        static async Task Main(string[] args)
        {
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

            inception = new Emgu.TF.Lite.Models.Inception();
            inception.OnDownloadProgressChanged += onDownloadProgressChanged;

            System.Console.WriteLine("Initializing inception");
            await inception.Init();

            Stopwatch watch = Stopwatch.StartNew();
            var result = inception.Recognize(_inputFileInfo.FullName);
            watch.Stop();
            String resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", result[0].Label, result[0].Probability * 100, watch.ElapsedMilliseconds);

            System.Console.WriteLine(resStr);
            System.Console.WriteLine("Press any key to continue:");

            System.Console.ReadKey();
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