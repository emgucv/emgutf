//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using Emgu.TF;
using Emgu.Models;
using Emgu.TF.Models;
using Tensorflow;

namespace Emgu.TF.XamarinForms
{
    public class InceptionPage : ModelButtonTextImagePage
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

        private bool _coldSession = true;

        public override String GetButtonName(ButtonMode mode)
        {
            switch (mode)
            {
                case ButtonMode.WaitingModelDownload:
                    return "Download Model";
                default:
                    return "Recognize object";
            }
        }

        public InceptionPage(Model model)
            : base()
        {
            Title = model == Model.Flower ? "Flower Recognition" : "Object recognition (Inception)";
            _model = model;

            if (_inceptionGraph == null)
            {
                SessionOptions so = new SessionOptions();
                if (TfInvoke.IsGoogleCudaEnabled)
                {
                    Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
                    config.GpuOptions = new Tensorflow.GPUOptions();
                    config.GpuOptions.AllowGrowth = true;
                    so.SetConfig(config.ToProtobuf());
                }
                _inceptionGraph = new Inception(null, so);
                _inceptionGraph.OnDownloadProgressChanged += onDownloadProgressChanged;
                _inceptionGraph.OnDownloadCompleted += onDownloadCompleted;
                _inceptionGraph.OnDownloadCompleted += (sender, e) =>
                {
                    OnButtonClicked(sender, e);
                };
            }
            OnImagesLoaded += (sender, image) =>
            {
#if !DEBUG
                try
#endif
                {
                    SetMessage("Please wait...");
                    SetImage();

                    Tensor imageTensor;
                    if (_model == Model.Flower)
                    {
                        imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(image[0], 299, 299, 0.0f, 1.0f / 255.0f, false, false);
                    }
                    else
                    {
                        imageTensor =
                            Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(image[0], 224, 224, 128.0f, 1.0f);
                    }

                    Inception.RecognitionResult result;
                    if (_coldSession)
                    {
                        //First run of the recognition graph, here we will compile the graph and initialize the session
                        //This is expected to take much longer time than consecutive runs.
                        result = _inceptionGraph.Recognize(imageTensor)[0];
                        _coldSession = false;
                    }

                    //Here we are trying to time the execution of the graph after it is loaded
                    //If we are not interest in the performance, we can skip the following 3 lines
                    Stopwatch sw = Stopwatch.StartNew();
                    result = _inceptionGraph.Recognize(imageTensor)[0];
                    sw.Stop();

                    String msg = String.Format("Object is {0} with {1}% probability. Recognized in {2} milliseconds.", result.Label, result.Probability * 100, sw.ElapsedMilliseconds);
                    SetMessage(msg);

                    var jpeg = Emgu.Models.NativeImageIO.ImageFileToJpeg(image[0]);
                    SetImage(jpeg.Raw, jpeg.Width, jpeg.Height);
                }
#if  !DEBUG
                catch (Exception excpt)
                {
                    String msg = excpt.Message.Replace(System.Environment.NewLine, " ");
                    SetMessage(msg);
                }
#endif
            };

        }

        public override void OnButtonClicked(Object sender, EventArgs args)
        {
            base.OnButtonClicked(sender, args);

            if (_buttonMode == ButtonMode.WaitingModelDownload)
            {
                if (_model == Model.Flower)
                {
                    //use a retrained model to recognize followers
                    _inceptionGraph.Init(
                        new string[] { "optimized_graph.pb", "output_labels.txt" },
                        "https://github.com/emgucv/models/raw/master/inception_flower_retrain/",
                        "Placeholder",
                        "final_result");
                }
                else
                {
                    //The inception model
                    _inceptionGraph.Init();
                }

            }
            else
            {
                if (_model == Model.Flower)
                {
                    LoadImages(new string[] { "tulips.jpg" });
                }
                else
                {
                    LoadImages(new string[] { "space_shuttle.jpg" });
                }

            }
        }
    }
}
