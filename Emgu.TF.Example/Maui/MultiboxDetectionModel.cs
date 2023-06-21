//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Emgu.CV;
using Emgu.CV.Platform.Maui.UI;
using Emgu.TF;
using Emgu.Models;
using Emgu.TF.Models;
using Tensorflow;
using Size = System.Drawing.Size;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using Emgu.CV.Structure;
using static Emgu.CV.DISOpticalFlow;

namespace Emgu.TF.Maui.Demo
{
    public class MultiboxDetectionModel : Emgu.Util.DisposableObject, Emgu.CV.Models.IProcessAndRenderModel
    {
        private MultiboxGraph _multiboxGraph;

        private Tensor _inputTensor;

        //private System.Drawing.Size _inputSize;

        /// <summary>
        /// Return true if the model is initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _multiboxGraph != null;
            }
        }

        public MultiboxDetectionModel()
        {
        }


        public void Clear()
        {
            if (_multiboxGraph != null)
            {
                _multiboxGraph.Dispose();
                _multiboxGraph = null;
            }

            if (_inputTensor != null)
            {
                _inputTensor.Dispose();
                _inputTensor = null;
            }
        }

        /// <summary>
        /// Download and initialize the model
        /// </summary>
        /// <param name="onDownloadProgressChanged">Callback when download progress has been changed</param>
        /// <param name="initOptions">Initialization options</param>
        /// <returns>Async task</returns>
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        public IEnumerator Init(Emgu.Util.FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged, Object initOptions)
#else
        public async Task Init(Emgu.Util.FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged, Object initOptions)
#endif
        {
            if (_multiboxGraph == null)
            {
                SessionOptions so = new SessionOptions();
                Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
                if (TfInvoke.IsGoogleCudaEnabled)
                {
                    config.GpuOptions = new Tensorflow.GPUOptions();
                    config.GpuOptions.AllowGrowth = true;
                }
#if DEBUG
                config.LogDevicePlacement = true;
#endif
                so.SetConfig(config.ToProtobuf());
                _multiboxGraph = new MultiboxGraph(null, so);
                if (onDownloadProgressChanged != null)
                {
                    _multiboxGraph.OnDownloadProgressChanged += (receive, received, percentage) =>
                    {
                        onDownloadProgressChanged(receive, received, percentage);
                    };
                }
                await _multiboxGraph.Init();
                
            }
            
            System.Drawing.Size inputSize = GetInputSize();

            _inputTensor = new Tensor(DataType.Float, new int[] { 1, inputSize.Height, inputSize.Width, 3 });
        }


        private System.Drawing.Size GetInputSize()
        {

            return new Size(224, 224);

        }

        public string ProcessAndRender(IInputArray imageIn, IInputOutputArray imageOut)
        {
            System.Drawing.Size s = GetInputSize();
            using (Mat resizedMat = new Mat(s, DepthType.Cv8U, 3))
            using (Mat tensorMat = new Mat(
                       s,
                       DepthType.Cv32F,
                       3,
                       _inputTensor.DataPointer,
                       3 * s.Width * Marshal.SizeOf<float>()))
            {
                CvInvoke.Resize(imageIn, resizedMat, s);
                CvInvoke.CvtColor(resizedMat, resizedMat, ColorConversion.Bgr2Rgb);

                resizedMat.ConvertTo(tensorMat, DepthType.Cv32F, 1.0f / 128.0f, -0.5);
            }


            //Here we are trying to time the execution of the graph after it is loaded
            //If we are not interest in the performance, we can skip the following 3 lines
            Stopwatch sw = Stopwatch.StartNew();
            MultiboxGraph.Result[] detectResult = _multiboxGraph.Detect(_inputTensor);
            sw.Stop();
            Emgu.Models.Annotation[] annotations = MultiboxGraph.FilterResults(detectResult, 0.1f);

            
            using (InputArray iaImage = imageIn.GetInputArray())
            {
                var imgSize = iaImage.GetSize();
                iaImage.CopyTo(imageOut);

                foreach (Emgu.Models.Annotation annotation in annotations)
                {
                    float x0 = annotation.Rectangle[0] * imgSize.Width;
                    float y0 = annotation.Rectangle[1] * imgSize.Height;
                    float x1 = annotation.Rectangle[2] * imgSize.Width;
                    float y1 = annotation.Rectangle[3] * imgSize.Height;

                    Rectangle r = new Rectangle((int)x0, (int)y0, (int)(x1 - x0), (int)(y1 - y0));
                    CvInvoke.Rectangle(imageOut, r, new MCvScalar(0, 0, 255), 2);
                }
            }

            String msg = String.Format("Detected in {0} milliseconds.", sw.ElapsedMilliseconds);
            return msg;

        }


        protected override void DisposeObject()
        {
            Clear();
        }
    }


}
