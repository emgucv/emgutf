using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emgu.TF.Lite;
using Emgu.TF.Lite.Models;
using System.Threading;

namespace Emgu.TF.Lite.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGetVersion()
        {
            String version = TfLiteInvoke.Version;
        }

        [TestMethod]
        public void TestMobilenet()
        {
            using (Mobilenet mobilenet = new Mobilenet())
            {
                bool processCompleted = false;
                mobilenet.OnDownloadCompleted += (sender, e) =>
                {
                    var result = mobilenet.Recognize("grace_hopper.jpg")[0];
                    
                    processCompleted = true;
                };

                mobilenet.Init();
                while (!processCompleted)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
