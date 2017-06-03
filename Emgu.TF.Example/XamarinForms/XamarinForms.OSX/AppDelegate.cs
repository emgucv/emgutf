using System;
using System.Globalization;
using AppKit;
using CoreGraphics;
using Foundation;
//using Xamarin.Forms.Controls;
//using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.MacOS;


namespace XamarinForms.OSX
{
    [Register("AppDelegate")]
    public class AppDelegate : Xamarin.Forms.Platform.MacOS.FormsApplicationDelegate
    {
		NSWindow _window;
		public AppDelegate()
		{
			ObjCRuntime.Runtime.MarshalManagedException += (sender, args) =>
			{
				Console.WriteLine(args.Exception.ToString());
			};


			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

			var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
			//var rect = NSWindow.FrameRectFor(NSScreen.MainScreen.Frame, style);
			_window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			_window.Title = "Emgu TF Xamarin Forms for Mac";
			//_window.TitleVisibility = NSWindowTitleVisibility.Hidden;
		}

		public override NSWindow MainWindow
		{
			get { return _window; }
		}


		public override void DidFinishLaunching(NSNotification notification)
		{
			// Insert code here to initialize your application
			Xamarin.Forms.Forms.Init();

            Emgu.TF.TfInvoke.CheckLibraryLoaded();

			var app = new Emgu.TF.XamarinForms.App();
			LoadApplication(app);

			base.DidFinishLaunching(notification);
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}
    }
}
