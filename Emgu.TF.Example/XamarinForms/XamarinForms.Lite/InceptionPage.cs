//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using Emgu.TF.Lite;
using Emgu.TF.Lite.Models;
#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Preferences;
#elif __UNIFIED__ && !__IOS__
using AppKit;
using CoreGraphics;
#elif __IOS__
using UIKit;
using CoreGraphics;
#endif

namespace Emgu.TF.XamarinForms
{
    public class InceptionPage : ButtonTextImagePage
    {
        private Inception inceptionGraph;

        private string[] _image = null;

        public InceptionPage()
           : base()
        {
            
            var button = this.GetButton();
            button.Text = "Perform Image Recognition";
            button.Clicked += OnButtonClicked;

            OnImagesLoaded += async (sender, image) =>
            {
                SetMessage("Please wait...");
                SetImage();
                _image = image;
                Task<Tuple<string, string, long>> t = new Task<Tuple<string, string, long>>(
                    () =>
                    {
                        try
                        {
                            SetMessage("Please wait while we download the Inception Model from internet.");
                            inceptionGraph = new Inception();
                            inceptionGraph.Init(onDownloadProgressChanged, onDownloadCompleted);
                            SetMessage("Please wait...");

                            
                            /*
                            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile(image[0], 224, 224, 128.0f, 1.0f / 128.0f);
                            Stopwatch watch = Stopwatch.StartNew();
                            float[] probability = inceptionGraph.Recognize(imageTensor);
                            watch.Stop();

                            String resStr = String.Empty;
                            if (probability != null)
                            {
                                String[] labels = inceptionGraph.Labels;
                                float maxVal = 0;
                                int maxIdx = 0;
                                for (int i = 0; i < probability.Length; i++)
                                {
                                    if (probability[i] > maxVal)
                                    {
                                        maxVal = probability[i];
                                        maxIdx = i;
                                    }
                                }
                                resStr = String.Format("Object is {0} with {1}% probability.", labels[maxIdx], maxVal * 100);
                            }*/
                            //return new Tuple<string, string, long>(image[0], resStr, 0);
                            return new Tuple<string, string, long>(String.Empty, String.Empty, 0);
                            //SetImage(t.Result.Item1);
                            //GetLabel().Text = String.Format("Detected {0} in {1} milliseconds.", t.Result.Item2, t.Result.Item3);
                        }
                        catch (Exception e)
                        {
                            String msg = e.Message.Replace(System.Environment.NewLine, " ");
                            SetMessage(msg);
                            return new Tuple<string, string, long>(null, msg, 0);
                        }
                    });
                t.Start();

#if !__IOS__
                var result = await t;
                SetImage(t.Result.Item1);
                GetLabel().Text = t.Result.Item2;
#endif

            };
        }

        private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            SetMessage(String.Format("{0} of {1} downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            String localFileName = DownloadableModels.GetLocalFileName(inceptionGraph._modelFiles[0]);
            System.Diagnostics.Debug.Assert(File.Exists(localFileName), "File doesn't exist");
            FileInfo file = new FileInfo(localFileName);

            using (FlatBufferModel model = new FlatBufferModel(localFileName))
            using (BuildinOpResolver resolver = new BuildinOpResolver())
            using (Interpreter interpreter = new Interpreter(model, resolver))
            //using (InterpreterBuilder interpreterBuilder = new InterpreterBuilder(model, resolver))
            {
                
                //Status buildStatus = interpreterBuilder.Build(interpreter);

                bool check = model.CheckModelIdentifier();

                int[] input = interpreter.GetInput();
                int[] output = interpreter.GetOutput();
                String inputName = interpreter.GetInputName(input[0]);
                String outputName = interpreter.GetOutputName(output[0]);

                Status allocateTensorStatus = interpreter.AllocateTensors();

                //IntPtr inputPtr = interpreter.GetInputTensorPtr(0);
                Tensor inputTensor = interpreter.GetTensor(input[0]);
                Emgu.TF.Lite.Models.ImageIO.ReadImageFileToTensor(_image[0], inputTensor.Data, 224, 224, 128.0f, 1.0f / 128.0f);

                interpreter.Invoke();
                Tensor outputTensor = interpreter.GetTensor(output[0]);

            }
        }

        private void OnButtonClicked(Object sender, EventArgs args)
        {
            LoadImages(new string[] { "grace_hopper.jpg" });
        }

    }
}
