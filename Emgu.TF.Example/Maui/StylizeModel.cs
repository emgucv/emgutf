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
using Emgu.CV.Models;

namespace Emgu.TF.Maui.Demo
{
    public class StylizeModel : Emgu.Util.DisposableObject, Emgu.CV.Models.IProcessAndRenderModel
    {

        private StylizeGraph _stylizeGraph;

        private Tensor _inputTensor;

        //private System.Drawing.Size _inputSize;

        /// <summary>
        /// Return true if the model is initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _stylizeGraph != null;
            }
        }

        public StylizeModel()
        {
        }

        public RenderType RenderMethod
        {
            get { return Emgu.CV.Models.RenderType.Overwrite; }
        }
        public void Clear()
        {
            if (_stylizeGraph != null)
            {
                _stylizeGraph.Dispose();
                _stylizeGraph = null;
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
            if (_stylizeGraph == null)
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
                _stylizeGraph = new StylizeGraph(null, so);
                if (onDownloadProgressChanged != null)
                {
                    _stylizeGraph.OnDownloadProgressChanged += (receive, received, percentage) =>
                    {
                        onDownloadProgressChanged(receive, received, percentage);
                    };
                }

                await _stylizeGraph.Init();

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

                resizedMat.ConvertTo(tensorMat, DepthType.Cv32F, 1.0f / 256.0f, -0.5f);
            }


            //Here we are trying to time the execution of the graph after it is loaded
            //If we are not interest in the performance, we can skip the following 3 lines
            Stopwatch sw = Stopwatch.StartNew();
            var stylizedImage = _stylizeGraph.Stylize(_inputTensor, 1);
            sw.Stop();
            var jpegBytes = Emgu.TF.Models.ImageIO.TensorToJpeg(stylizedImage, 255.0f);
            using (Mat tmp = new Mat())
            using (InputArray iaImage = imageIn.GetInputArray())
            {
                CvInvoke.Imdecode(jpegBytes, ImreadModes.AnyColor, tmp);
                CvInvoke.Resize(tmp, imageOut, iaImage.GetSize());
                //tmp.CopyTo(imageOut);
            }

            String msg = String.Format("Stylized in {0} milliseconds.", sw.ElapsedMilliseconds);
            return msg;

        }


        protected override void DisposeObject()
        {
            Clear();
        }
    }


}
