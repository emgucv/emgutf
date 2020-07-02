//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using Emgu.TF;
using Emgu.Models;
using Emgu.TF.Models;
using Tensorflow;

namespace Emgu.TF.XamarinForms
{
    public class InceptionPage
#if __ANDROID__
        : AndroidCameraPage
#else
        : ButtonTextImagePage
#endif
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

        private async Task Init(DownloadProgressChangedEventHandler onProgressChanged)
        {
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
                _inceptionGraph.OnDownloadProgressChanged += onProgressChanged;

                if (_model == Model.Flower)
                {
                    //use a retrained model to recognize followers
                    await _inceptionGraph.Init(
                        new string[] { "optimized_graph.pb", "output_labels.txt" },
                        "https://github.com/emgucv/models/raw/master/inception_flower_retrain/",
                        "Placeholder",
                        "final_result");
                }
                else
                {
                    //The original inception model
                    await _inceptionGraph.Init();
                }
                //await _inceptionGraph.Init();
            }
        }

        private bool _coldSession = true;

        /*
        public override String GetButtonName(ButtonMode mode)
        {
            switch (mode)
            {
                case ButtonMode.WaitingModelDownload:
                    return "Download Model";
                default:
                    return "Recognize object";
            }
        }*/

        private String _defaultButtonText = "Recognize object";

#if __ANDROID__
        private String _StopCameraButtonText = "Stop Camera";
        private bool _isBusy = false;
#endif

        public InceptionPage(Model model)
            : base()
        {
#if __ANDROID__
            //HasCameraOption = true;
            HasCameraOption = false;
#endif

            Title = model == Model.Flower ? "Flower Recognition" : "Object recognition (Inception)";
            _model = model;

            this.TopButton.Text = _defaultButtonText;

            this.TopButton.Clicked += async (sender, e) =>
            {
#if !DEBUG
                try
#endif
                {
                    this.TopButton.IsEnabled = false;
                    SetImage();

                    SetMessage("Please wait while we download / initialize the model ...");
                    await Init(this.onDownloadProgressChanged);
                    SetMessage("Model Loaded.");
                    String[] images;
                    if (_model == Model.Flower)
                    {
                        images = await LoadImages(new string[] {"tulips.jpg"});
                    }
                    else
                    {
                        images = await LoadImages(new string[] {"space_shuttle.jpg"});
                    }

                    if (images == null)
                    {
                        //User canceled
                        this.TopButton.IsEnabled = true;
                        return;
                    }

                    if (images[0] == "Camera")
                    {
                        //TODO: handle camera stream
#if __ANDROID__
                        this.TopButton.Text = _StopCameraButtonText;
                        StartCapture(async delegate (Object sender, Android.Graphics.Bitmap m)
                        {
                            //Skip the frame if busy, 
                            //Otherwise too many frames arriving and will eventually saturated the memory.
                            if (!_isBusy)
                            {
                                _isBusy = true;
                                try
                                {
                                    Stopwatch watch = Stopwatch.StartNew();
                                    Inception.RecognitionResult result;
                                    //await Task.Run(() =>
                                    //{
                                        Tensor imageTensor;

                                        if (_model == Model.Flower)
                                        {
                                            imageTensor = new Tensor(DataType.Float, new int[] {1, 299, 299, 3} );
                                            Emgu.Models.NativeImageIO.ReadBitmapToTensor<float>(m, imageTensor.DataPointer, 299, 299, 0.0f,
                                                1.0f / 255.0f, false, false);

                                        }
                                        else
                                        {
                                            imageTensor = new Tensor(DataType.Float, new int[] { 1, 224, 224, 3 });
                                            Emgu.Models.NativeImageIO.ReadBitmapToTensor<float>(m, imageTensor.DataPointer, 224, 224, 128.0f, 
                                                    1.0f);
                                        }
                                        result = _inceptionGraph.Recognize(imageTensor)[0];
                                    //});
                                    watch.Stop();
                                    SetImage(m);
                                    String msg = String.Format("Object is {0} with {1}% probability. Recognized in {2} milliseconds.",
                                        result.Label, result.Probability * 100, watch.ElapsedMilliseconds);
                                    SetMessage(msg);
                                }
                                finally
                                {
                                    _isBusy = false;
                                    
                                }
                            }
                        });
#else
                        throw new NotImplementedException("Camera handling is not implemented");
#endif
                    }
                    else
                    {
                        Tensor imageTensor;
                        if (_model == Model.Flower)
                        {
                            imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(images[0], 299, 299, 0.0f,
                                1.0f / 255.0f, false, false);
                        }
                        else
                        {
                            imageTensor =
                                Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(images[0], 224, 224, 128.0f, 1.0f);
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

                        String msg = String.Format("Object is {0} with {1}% probability. Recognized in {2} milliseconds.",
                            result.Label, result.Probability * 100, sw.ElapsedMilliseconds);
                        SetMessage(msg);

#if __ANDROID__
                        var bmp = Emgu.Models.NativeImageIO.ImageFileToBitmap(images[0]);
                        SetImage(bmp);
#else
                        var jpeg = Emgu.Models.NativeImageIO.ImageFileToJpeg(images[0]);
                        SetImage(jpeg.Raw, jpeg.Width, jpeg.Height);
#endif
                        this.TopButton.IsEnabled = true;
                    }
                }
#if !DEBUG
                    catch (Exception excpt)
                    {
                        String msg = excpt.Message.Replace(System.Environment.NewLine, " ");
                        SetMessage(msg);
                    }
#endif
            };
        }

        /*
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
                    //The original inception model
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
        }*/
    }
}
