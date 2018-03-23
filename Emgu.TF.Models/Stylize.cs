
//#if !__UNIFIED__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Emgu.TF.Models
{
    public class StylizeGraph : DownloadableModels
    {
        public StylizeGraph(
            Status status = null, 
            String[] modelFiles = null, 
            String downloadUrl = null,
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
            : base(
        modelFiles ?? new string[] { "stylize_quantized.pb" },
        downloadUrl ?? "https://github.com/emgucv/models/raw/master/stylize_v1/")
        {
            Download(1, onDownloadProgressChanged);
#if __ANDROID__
            byte[] model = File.ReadAllBytes(System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads, _modelFiles[0]));
#else
            byte[] model = File.ReadAllBytes(_modelFiles[0]);
#endif

            Buffer modelBuffer = Buffer.FromString(model);

            using (ImportGraphDefOptions options = new ImportGraphDefOptions())
                ImportGraphDef(modelBuffer, options, status);
        }

        private const int NumStyles = 26;

        private static Tensor GetStyleTensor(int style, int numStyles)
        {
            float[] styleValues = new float[numStyles];
            for (int i = 0; i < numStyles; i++)
            {
                if (i == style)
                    styleValues[i] = 1.0f;
            }
            Tensor styleTensor = new Tensor(styleValues);
            return styleTensor;
        }

        public Tensor Stylize(Tensor imageValue, int style)
        {
            Session stylizeSession = new Session(this);
            Tensor styleTensor = GetStyleTensor(style, NumStyles);
            
            Tensor[] finalTensor = stylizeSession.Run(
                new Output[] { this["input"], this["style_num"] }, new Tensor[] { imageValue, styleTensor },
                new Output[] { this[@"transformer/expand/conv3/conv/Sigmoid"] });

            return finalTensor[0];
        }
    }
}
//#endif
