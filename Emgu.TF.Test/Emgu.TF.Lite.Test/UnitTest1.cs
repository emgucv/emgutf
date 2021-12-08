using System;
#if VS_TEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#else
using NUnit.Framework;
#endif
using Emgu.TF.Lite;
using Emgu.TF.Lite.Models;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;

namespace Emgu.TF.Lite.Test
{
    [TestFixture]
    public class UnitTest1
    {
        [TestAttribute]
        public void TestGetVersion()
        {
            String version = TfLiteInvoke.Version;
            String emgucvBuildInfo = Emgu.CV.CvInvoke.BuildInformation;
        }

        [TestAttribute]
        public async Task TestMobilenet()
        {
            using (Mobilenet mobilenet = new Mobilenet())
            {
                await mobilenet.Init();
                var result = mobilenet.Recognize("grace_hopper.jpg")[0];
            }
        }
    }
}
