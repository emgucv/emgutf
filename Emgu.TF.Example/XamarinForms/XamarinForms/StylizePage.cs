//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using System.Threading.Tasks;
#if __ANDROID__
using Android.Graphics;
#elif __IOS__
using UIKit;
using CoreGraphics;
#elif __UNIFIED__
using AppKit;
using CoreGraphics;
#endif
using Emgu.TF;
using Emgu.Models;
using Emgu.TF.Models;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;
using Tensorflow;

namespace Emgu.TF.XamarinForms
{
    public class StylizePage
#if __ANDROID__
        : AndroidCameraPage
#else
        : ButtonTextImagePage
#endif
    {
        private StylizeGraph _stylizeGraph;
        
        private async Task Init(DownloadProgressChangedEventHandler onProgressChanged)
        {
            if (_stylizeGraph == null)
            {
                SessionOptions so = new SessionOptions();
                if (TfInvoke.IsGoogleCudaEnabled)
                {
                    Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
                    config.GpuOptions = new Tensorflow.GPUOptions();
                    config.GpuOptions.AllowGrowth = true;
                    so.SetConfig(config.ToProtobuf());
                }
                _stylizeGraph = new StylizeGraph(null, so);
                _stylizeGraph.OnDownloadProgressChanged += onProgressChanged;

                await _stylizeGraph.Init();

            }
        }


        public StylizePage()
            : base()
        {
            Title = "Stylize";
            this.TopButton.Text = "Stylize";

            this.TopButton.Clicked += async (sender, args) =>
            {
                try
                {
                    this.TopButton.IsEnabled = false;
                    SetMessage("Please wait...");
                    SetImage();

                    SetMessage("Please wait while we download the model from internet.");
                    await Init(this.onDownloadProgressChanged);

                    String[] images = await LoadImages(new string[] { "surfers.jpg" });
                    if (images == null)
                    {
                        return;
                    }

#if __ANDROID__
                    Stopwatch watch = Stopwatch.StartNew();
                    Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(images[0], 224, 224, 128.0f, 1.0f / 128.0f);
                    Tensor stylizedImage = _stylizeGraph.Stylize(imageTensor, 0);
                    watch.Stop();
                    byte[] rawPixel = Emgu.TF.Models.ImageIO.TensorToPixel(stylizedImage, 255.0f, 0.0f, 4);
                    int[] dim = stylizedImage.Dim;
                    Bitmap bmp = NativeImageIO.PixelToBitmap(rawPixel, dim[2], dim[1], 4);
                    SetImage(bmp);
#else
                    Stopwatch watch = Stopwatch.StartNew();
                    byte[] jpeg = _stylizeGraph.StylizeToJpeg(images[0], 1);
                    watch.Stop();
                    SetImage(jpeg);
#endif
#if __MACOS__
                    NSImage img = new NSImage(images[0]);
                    var displayImage = this.DisplayImage;
                    displayImage.WidthRequest = img.Size.Width;
                    displayImage.HeightRequest = img.Size.Height;
#endif
                    SetMessage(String.Format("Stylized in {0} milliseconds.", watch.ElapsedMilliseconds));

                }
                catch (Exception excpt)
                {
                    String msg = excpt.Message.Replace(System.Environment.NewLine, " ");
                    SetMessage(msg);
                }
                finally
                {
                    this.TopButton.IsEnabled = true;
                }

            };
 
        }

    }
}
