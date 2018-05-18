using System;
using System.Runtime.InteropServices;

using AppKit;
using Foundation;
using CoreGraphics;

using Emgu.TF;
using Emgu.TF.Models;

namespace Example.OSX
{
    public partial class ViewController : NSViewController
    {
		void inceptionGraph_OnDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			String fileName = "space_shuttle.jpg";

            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);
            //MultiboxGraph.Result detectResult = graph.Detect(imageTensor);
            float[] probability = _inceptionGraph.Recognize(imageTensor);
            String resStr = String.Empty;
            if (probability != null)
            {
                String[] labels = _inceptionGraph.Labels;
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
            }
            SetMessage(resStr);
            SetImage(new NSImage(fileName));
		}

		void multiboxGraph_OnDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
		{
			String fileName = "surfers.jpg";
            
            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);
			MultiboxGraph.Result detectResult = _multiboxGraph.Detect(imageTensor);
            NSImage image = new NSImage(fileName);
            MultiboxGraph.DrawResults(image, detectResult, 0.1f);
			SetImage(image);
		}

		void stylizeGraph_OnDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			Tensor image = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile("surfers.jpg");
            Tensor stylizedImage = _stylizeGraph.Stylize(image, 0);
            var dim = stylizedImage.Dim;
            System.Drawing.Size sz = new System.Drawing.Size(dim[2], dim[1]);

            byte[] rawPixels = Emgu.TF.Models.ImageIO.TensorToPixel(stylizedImage, 255, 4);

            CGColorSpace cspace = CGColorSpace.CreateDeviceRGB();
            CGBitmapContext context = new CGBitmapContext(
                rawPixels,
                sz.Width, sz.Height,
                8,
                sz.Width * 4,
                cspace,
                CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big);
            CGImage cgImage = context.ToImage();

            NSImage newImg = new NSImage(cgImage, new CGSize(cgImage.Width, cgImage.Height));
			SetImage( newImg );

            NSData dta = newImg.AsTiff();
            NSBitmapImageRep imgRep = new NSBitmapImageRep(dta);
            //var datra = imgRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.NSBitmapImageFileType.Jpeg);


		}


        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
            messageLabel.StringValue = String.Empty;
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

		private Inception _inceptionGraph = null;
		private MultiboxGraph _multiboxGraph = null;
		private StylizeGraph _stylizeGraph = null;

		void OnDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
		{
			String message = String.Format("Downloading {0} of {1} ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
			SetMessage(message);
		}


        public void SetMessage(String message)
		{
			InvokeOnMainThread(
				() =>
			{
				messageLabel.StringValue = message;
			});
		}

		public void SetImage(NSImage image)
		{
			InvokeOnMainThread(
				() =>
				{
					mainImageView.Image = image;
				});
		}

        partial void inceptionClicked(NSObject sender)
        {
            
			SetMessage("Please wait while we download Inception model from internet...");
			SetImage(null);

			if (_inceptionGraph == null)
			{
				_inceptionGraph = new Inception();
				_inceptionGraph.OnDownloadProgressChanged += OnDownloadProgressChanged;
				_inceptionGraph.OnDownloadCompleted += inceptionGraph_OnDownloadCompleted;
			}
			_inceptionGraph.Init();
            
        }

        partial void multiboxClicked(NSObject sender)
        {
			SetMessage("Please wait while we download Multibox model from internet...");
            SetImage(null);

			if (_multiboxGraph == null)
			{
				_multiboxGraph = new MultiboxGraph();
				_multiboxGraph.OnDownloadProgressChanged += OnDownloadProgressChanged;
				_multiboxGraph.OnDownloadProgressChanged += multiboxGraph_OnDownloadProgressChanged;
			}
			_multiboxGraph.Init();

        }

        partial void stylizeClicked(NSObject sender)
        {
			SetMessage("Please wait while we download Stylize model from internet...");
            SetImage(null);

			if (_stylizeGraph == null)
			{
				_stylizeGraph = new StylizeGraph();
				_stylizeGraph.OnDownloadProgressChanged += OnDownloadProgressChanged;
				_stylizeGraph.OnDownloadCompleted += stylizeGraph_OnDownloadCompleted;
			}
			_stylizeGraph.Init();
            
        }
    }
}
