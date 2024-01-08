//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
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
    public class InceptionModel : Emgu.Util.DisposableObject, Emgu.CV.Models.IProcessAndRenderModel
    {
        /// <summary>
        /// The inception model to use.
        /// </summary>
        public enum Model
        {
            /// <summary>
            /// The default inception model
            /// </summary>
            Default,
            /// <summary>
            /// The flower re-train model
            /// </summary>
            Flower
        }

        private Model _model;

        private Inception _inceptionGraph;

        private Tensor _inputTensor;

        //private System.Drawing.Size _inputSize;

        /// <summary>
        /// Return true if the model is initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _inceptionGraph != null;
            }
        }

        public RenderType RenderMethod
        {
            get { return Emgu.CV.Models.RenderType.Update; }
        }

        public InceptionModel(Model model)
        {
            _model = model;


        }


        public void Clear()
        {
            if (_inceptionGraph != null)
            {
                _inceptionGraph.Dispose();
                _inceptionGraph = null;
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
            if (_inceptionGraph == null)
            {
                using (SessionOptions so = new SessionOptions())
                {
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

                    _inceptionGraph = new Inception(null, so);
                    if (onDownloadProgressChanged != null)
                    {
                        _inceptionGraph.OnDownloadProgressChanged += (receive, received, percentage) =>
                        {
                            onDownloadProgressChanged(receive, received, percentage);
                        };
                    }

                    if (_model == Model.Flower)
                    {
                        String localModelFolder = "InceptionFlower";
                        DownloadableFile modelFile = new DownloadableFile(
                            "https://github.com/emgucv/models/raw/master/inception_flower_retrain/optimized_graph.pb",
                            localModelFolder,
                            "DE83CAD3F87B5070E24EFEADB8B84F72C940B73974DC69B46D96CDFB913385C4"
                        );

                        DownloadableFile labelFile = new DownloadableFile(
                            "https://github.com/emgucv/models/raw/master/inception_flower_retrain/output_labels.txt",
                            localModelFolder,
                            "298454B11DBEE503F0303367F3714D449855071DF9ECAC16AB0A01A0A7377DB6"
                        );

                        //use a retrained model to recognize followers
                        await _inceptionGraph.Init(
                            modelFile,
                            labelFile,
                            "Placeholder",
                            "final_result");
                    }
                    else
                    {
                        //The original inception model
                        await _inceptionGraph.Init();
                    }
                }

                System.Drawing.Size inputSize = GetInputSize();

                _inputTensor = new Tensor(DataType.Float, new int[] { 1, inputSize.Height, inputSize.Width, 3 });
            }
        }

        private System.Drawing.Size GetInputSize()
        {
            if (_model == Model.Flower)
            {
                return new Size(299, 299);
            }
            else
            {
                return new Size(224, 224);
            }
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
                if (_model == Model.Flower)
                    resizedMat.ConvertTo(tensorMat, DepthType.Cv32F, 1.0f / 255.0f, 0.0f);
                else
                    resizedMat.ConvertTo(tensorMat, DepthType.Cv32F, 1.0f, -128.0f);
            }

            //Here we are trying to time the execution of the graph after it is loaded
            //If we are not interest in the performance, we can skip the following 3 lines
            Stopwatch sw = Stopwatch.StartNew();
            var result = _inceptionGraph.Recognize(_inputTensor)[0];
            sw.Stop();

            String msg = String.Format(
                "Object is {0} with {1}% probability. Recognized in {2} milliseconds.",
                result[0].Label,
                result[0].Probability * 100,
                sw.ElapsedMilliseconds);
            return msg;

        }


        protected override void DisposeObject()
        {
            Clear();
        }
    }


}
