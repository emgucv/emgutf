using System;
using System.Runtime.InteropServices;

using AppKit;
using Foundation;
using CoreGraphics;

using Emgu.TF;
using Emgu.TF.Models;
using System.Threading.Tasks;

namespace Example.OSX
{
    public partial class ViewController : NSViewController
    {
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

        private static String ByteToSizeStr(long byteCount)
        {
            if (byteCount < 1024)
            {
                return String.Format("{0} B", byteCount);
            }
            else if (byteCount < 1024 * 1024)
            {
                return String.Format("{0} KB", byteCount / 1024);
            }
            else
            {
                return String.Format("{0} MB", byteCount / (1024 * 1024));
            }
        }



        void OnDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
		{
            String msg;
            if (e.TotalBytesToReceive > 0)
                msg = String.Format("{0} of {1} downloaded ({2}%)", ByteToSizeStr(e.BytesReceived), ByteToSizeStr(e.TotalBytesToReceive), e.ProgressPercentage);
            else
                msg = String.Format("{0} downloaded", ByteToSizeStr(e.BytesReceived));
            SetMessage(msg);
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

        [Action("inceptionClicked:")]
        async void inceptionClicked(NSObject sender)
        {
            
			SetMessage("Please wait while we download Inception model from internet...");
			SetImage(null);

			if (_inceptionGraph == null)
			{
				_inceptionGraph = new Inception();
				_inceptionGraph.OnDownloadProgressChanged += OnDownloadProgressChanged;
				//_inceptionGraph.OnDownloadCompleted += inceptionGraph_OnDownloadCompleted;
			}
			await _inceptionGraph.Init();

            String fileName = "space_shuttle.jpg";

            //Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(fileName, 224, 224, 128.0f, 1.0f / 128.0f);
            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(fileName, 224, 224, 128.0f, 1.0f);
            var recognitionResults = _inceptionGraph.Recognize(imageTensor);
            String resStr = String.Empty;
            if (recognitionResults != null && recognitionResults.Length > 0)
            {
                resStr = String.Format("Object is {0} with {1}% probability.", recognitionResults[0].Label, recognitionResults[0].Probability * 100);
            }
            SetMessage(resStr);
            SetImage(new NSImage(fileName));
        }

        [Action("multiboxClicked:")]
        async void multiboxClicked(NSObject sender)
        {
			SetMessage("Please wait while we download Multibox model from internet...");
            SetImage(null);

			if (_multiboxGraph == null)
			{
				_multiboxGraph = new MultiboxGraph();
				_multiboxGraph.OnDownloadProgressChanged += OnDownloadProgressChanged;
				//_multiboxGraph.OnDownloadCompleted += multiboxGraph_OnDownloadCompleted;
			}
			await _multiboxGraph.Init();
            SetMessage("");

            String fileName = "surfers.jpg";

            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(fileName, 224, 224, 128.0f, 1.0f / 128.0f);
            MultiboxGraph.Result[] detectResult = _multiboxGraph.Detect(imageTensor);
            Emgu.Models.Annotation[] annotations = MultiboxGraph.FilterResults(detectResult, 0.1f);

            NSImage img = new NSImage(fileName);
            Emgu.Models.NativeImageIO.DrawAnnotations(img, annotations);

            SetImage(img);
            
        }

        [Action("stylizeClicked:")]
        async void stylizeClicked(NSObject sender)
        {
			SetMessage("Please wait while we download Stylize model from internet...");
            SetImage(null);

			if (_stylizeGraph == null)
			{
				_stylizeGraph = new StylizeGraph();
				_stylizeGraph.OnDownloadProgressChanged += OnDownloadProgressChanged;
				//_stylizeGraph.OnDownloadCompleted += stylizeGraph_OnDownloadCompleted;
			}
			await _stylizeGraph.Init();
            SetMessage("");

            byte[] jpeg = _stylizeGraph.StylizeToJpeg("surfers.jpg", 0);
            using (System.IO.Stream s = new System.IO.MemoryStream(jpeg))
            {
                NSImage newImg = NSImage.FromStream(s);
                SetImage(newImg);
            }
        }
    }
}
