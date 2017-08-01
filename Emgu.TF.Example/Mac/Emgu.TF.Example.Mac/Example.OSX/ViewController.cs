using System;

using AppKit;
using Foundation;

using Emgu.TF;
using Emgu.TF.Models;

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

        partial void inceptionClicked(NSObject sender)
        {
            messageLabel.StringValue = "Inception Clicked";
            mainImageView.Image = null;

            Inception inceptionGraph = new Inception();

            String fileName = "space_shuttle.jpg";

			Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);
			//MultiboxGraph.Result detectResult = graph.Detect(imageTensor);
			float[] probability = inceptionGraph.Recognize(imageTensor);
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
			}
            messageLabel.StringValue = resStr;
            mainImageView.Image = new NSImage(fileName);
        }

        partial void multiboxClicked(NSObject sender)
        {
            messageLabel.StringValue = "Multibox Clicked";
            mainImageView.Image = null;

			MultiboxGraph graph = new MultiboxGraph();
            String fileName = "surfers.jpg";

            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);
			MultiboxGraph.Result detectResult = graph.Detect(imageTensor);
            NSImage image = new NSImage(fileName);
            MultiboxGraph.DrawResults(image, detectResult, 0.1f);
            mainImageView.Image = image;
        }

        partial void stylizeClicked(NSObject sender)
        {
            messageLabel.StringValue = "stylize Clicked";
            mainImageView.Image = null;

            StylizeGraph stylizeGraph = new StylizeGraph();

            Tensor image = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile("surfers.jpg");
            Tensor stylizedImage = stylizeGraph.Stylize(image, 0);

        }
    }
}
