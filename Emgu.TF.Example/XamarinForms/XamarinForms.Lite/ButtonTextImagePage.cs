using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Forms;
#if __ANDROID__
using Plugin.CurrentActivity;
#endif

#if __MACOS__
using AppKit;
using CoreGraphics;
using Xamarin.Forms.Platform.MacOS;
#elif __IOS__
using UIKit;
using CoreGraphics;
using Xamarin.Forms.Platform.iOS;
using Plugin.Media;
#else
using Plugin.Media;
#endif


namespace Emgu.TF.XamarinForms
{
    public partial class ButtonTextImagePage
#if __MACOS__ || __IOS__
        : Emgu.Util.AvCaptureSessionPage
#else
        : ContentPage
#endif
    {
        private Button _topButton = new Button();
        public Button TopButton
        {
            get { return _topButton; }
        }

        private Label _messageLabel = new Label();
        public Label MessageLabel
        {
            get { return _messageLabel; }
        }

        private Image _displayImage = new Image();

        public Image DisplayImage
        {
            get { return _displayImage; }
        }

        private StackLayout _mainLayout;

        public StackLayout MainLayout
        {
            get { return _mainLayout; }
        }

#if __MACOS__

        public NSImageView NSImageView { get; set; }
#elif __IOS__

        public UIImageView UIImageView { get; set; }
#endif

        public ButtonTextImagePage()
        {
            
            TopButton.Text = "Click me";
            TopButton.IsEnabled = true;
            TopButton.HorizontalOptions = LayoutOptions.Center;

            MessageLabel.Text = "";
            MessageLabel.HorizontalOptions = LayoutOptions.Center;
            MessageLabel.VerticalOptions = LayoutOptions.Center;
            MessageLabel.VerticalTextAlignment = TextAlignment.Center;
            MessageLabel.HorizontalTextAlignment = TextAlignment.Center;

            _mainLayout = new StackLayout();
            _mainLayout.VerticalOptions = LayoutOptions.FillAndExpand;
            _mainLayout.HorizontalOptions = LayoutOptions.FillAndExpand;
            _mainLayout.Orientation = StackOrientation.Vertical;
            _mainLayout.Spacing = 15;
            _mainLayout.Padding = new Thickness(10, 10, 10, 10);


            DisplayImage.HorizontalOptions = LayoutOptions.Center;

            /*
            DisplayImage.BackgroundColor = new Color(1, 0, 0);
            MessageLabel.BackgroundColor = new Color(0, 0, 1);
            _mainLayout.BackgroundColor = new Color(0, 1, 0);
            */

            _mainLayout.Children.Add(TopButton);
            _mainLayout.Children.Add(MessageLabel);
            _mainLayout.Children.Add(DisplayImage);
            //_mainLayout.BackgroundColor= Color.Bisque;

#if __MACOS__
            NSImageView = new NSImageView();
            NSImageView.ImageScaling = NSImageScale.None;
            _mainLayout.Children.Add(NSImageView.ToView());
#elif __IOS__
            UIImageView = new UIImageView();
            UIImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            _mainLayout.Children.Add(UIImageView.ToView());
#endif
            Content = new ScrollView()
            {
                Content = _mainLayout
            };
        }

        public virtual async Task<String[]> LoadImages(String[] imageNames, String[] labels = null)
        {
            /*
#if (__MACOS__) //Xamarin Mac
            String[] mats = new String[imageNames.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                String pickImgString = "Use Image from";

                String action;
                if (AllowAvCaptureSession)
                    action = await DisplayActionSheet(pickImgString, "Cancel", null, "Default", "Photo Library",  "Camera Stream");
                else
                    action = await DisplayActionSheet(pickImgString, "Cancel", null, "Default", "Photo Library");
                if (action.Equals("Default"))
                {
                    mats[i] = imageNames[i];
                }
                else if (action.Equals("Photo Library"))
                {
                    NSOpenPanel dlg = NSOpenPanel.OpenPanel;
                    dlg.CanChooseFiles = true;
                    dlg.CanChooseDirectories = false;
                    dlg.AllowsMultipleSelection = false;
                    dlg.AllowedFileTypes = new string[] { "jpg", "jpeg", "png", "bmp" };
                    if (dlg.RunModal() == 1)
                    {
                        mats[i] = dlg.Url.Path;
                    }
                    else
                    {
                        //canceled
                        return null;
                    }
                }
                else if (action.Equals("Camera Stream"))
                {
                    mats[i] = action;
                } else if (action.Equals("Cancel"))
                {
                    //canceled
                    return null;
                }
            }
            return mats;
#else*/

#if !__MACOS__
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                await CrossMedia.Current.Initialize();
#endif

            String[] mats = new String[imageNames.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                String pickImgString = "Use Image from";
                if (labels != null && labels.Length > i)
                    pickImgString = labels[i];
                bool haveCameraOption = false;
                bool havePickImgOption = false;
                bool haveLiveCameraOption = false;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    //CrossMedia is not implemented on Windows.
                    haveCameraOption = false;
                    havePickImgOption = true; //We will CrossFilePicker.Current.PickFile to pick image
                } else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                    .OSPlatform.OSX))
                {
#if __MACOS__
                    haveLiveCameraOption = AllowAvCaptureSession;
#endif
                    havePickImgOption = true; //We will CrossFilePicker.Current.PickFile to pick image
                }
                else
                {
#if !__MACOS__
                    haveCameraOption =
                        (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported);
                    havePickImgOption =
                        CrossMedia.Current.IsPickVideoSupported;
#endif
                }
#if __IOS__
                haveLiveCameraOption = AllowAvCaptureSession;
#endif
                List<String> options = new List<string>();
                options.Add("Default");
                if (havePickImgOption)
                    options.Add("Photo Library");
                if (haveCameraOption)
                    options.Add("Photo from Camera");
                if (haveLiveCameraOption)
                    options.Add("Camera Stream");

                String action = await DisplayActionSheet(pickImgString, "Cancel", null, options.ToArray());

                if (action.Equals("Default"))
                {
#if __ANDROID__
                    FileInfo fi = Emgu.TF.Util.AndroidFileAsset.WritePermanentFileAsset(CrossCurrentActivity.Current.Activity, imageNames[i], "tmp",
                        Emgu.TF.Util.AndroidFileAsset.OverwriteMethod.AlwaysOverwrite);

                    mats[i] = fi.FullName;
#else
                    mats[i] = imageNames[i];
            
#endif
                }
                else if (action.Equals("Photo Library"))
                {
                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    {
                        FileData fileData = await CrossFilePicker.Current.PickFile(new string[] {"Image | *.jpg;*.jpeg;*.png;*.bmp;*.gif | All Files | *"});
                        if (fileData == null)
                            return null;
                        mats[i] = fileData.FilePath;
                    }
                    else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime
                        .InteropServices
                        .OSPlatform.OSX))
                    {
                        FileData fileData = await CrossFilePicker.Current.PickFile(new string[] { "jpg", "jpeg", "png", "bmp" });
                        if (fileData == null)
                            return null;
                        mats[i] = fileData.FilePath;
                    }
                    else
                    {
#if !__MACOS__
                        var photoResult = await CrossMedia.Current.PickPhotoAsync();
                        if (photoResult == null) //canceled
                            return null;
                        mats[i] = photoResult.Path;
#endif
                    }
                }
                else if (action.Equals("Photo from Camera"))
                {
#if !__MACOS__
                    var mediaOptions = new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        Directory = "Emgu",
                        Name = $"{DateTime.UtcNow}.jpg"
                    };
                    var takePhotoResult = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
                    if (takePhotoResult == null) //canceled
                        return null;
                    mats[i] = takePhotoResult.Path;
#endif
                } else if (action.Equals("Camera Stream"))
                {
                    mats[i] = action;
                }
                else if (action.Equals("Cancel"))
                {
                    //canceled
                    return null;
                }

                //Handle user cancel
                if (action == null)
                    return null;
            }
            //InvokeOnImagesLoaded(mats);
            return mats;

        }

        public void SetImage(String fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception(String.Format("File '{0}' do not exist.", fileName));
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
               () =>
               {
                   var imageSource = new FileImageSource();
                   imageSource.File = fileName;
                   this.DisplayImage.Source = imageSource;
#if __MACOS__
                   NSImage image = new NSImage(fileName);
                   this.DisplayImage.WidthRequest = image.Size.Width;
                   this.DisplayImage.HeightRequest = image.Size.Height;
                   NSImageView.Hidden = true;
                   DisplayImage.IsVisible = true;
#elif __IOS__
                   UIImageView.Hidden = true;
                   DisplayImage.IsVisible = true;
#endif
                   this.DisplayImage.Focus();
               });
        }

#if __MACOS__
        public void SetImage(NSImage image)
        {

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
               () =>
               {
                   if (NSImageView.Frame.Size != image.Size)
                       NSImageView.Frame = new CGRect(CGPoint.Empty, image.Size);
                   NSImage oldImage = NSImageView.Image;
                   NSImageView.Image = image;
                   if (oldImage != null)
                       oldImage.Dispose();
                   NSImageView.Hidden = false;
                   DisplayImage.IsVisible = false;
               });
        }
#elif __IOS__
        public void SetImage(UIImage image)
        {

            Xamarin.Forms.Device.BeginInvokeOnMainThread(
               () =>
               {
                   if (UIImageView.Frame.Size != image.Size)
                       UIImageView.Frame = new CGRect(CGPoint.Empty, image.Size);
                   //SetMessage(String.Format("{0} image", _counter));
                   UIImage oldImage = UIImageView.Image;
                   UIImageView.Image = image;
                   if (oldImage != null)
                       oldImage.Dispose();
                   UIImageView.Hidden = false;
                   DisplayImage.IsVisible = false;
               });
        }
#endif
        public void SetImage(byte[] image = null, int widthRequest = -1, int heightRequest = -1)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
               () =>
               {
                   if (image == null)
                   {
                       this.DisplayImage.Source = null;
                       return;
                   }

                   this.DisplayImage.Source = ImageSource.FromStream(() => new MemoryStream(image));

                   if (widthRequest > 0)
                       this.DisplayImage.WidthRequest = widthRequest;
                   if (heightRequest > 0)
                       this.DisplayImage.HeightRequest = heightRequest;
#if __MACOS__
                   NSImageView.Hidden = true;
                   DisplayImage.IsVisible = true;
#elif __IOS__
                   UIImageView.Hidden = true;
                   DisplayImage.IsVisible = true;
                   //Xamarin Form's Image class do not seems to re-render after Source is change
                   //forcing focus seems to force a re-rendering
                   this.DisplayImage.Focus();
#endif
               });

        }

        /*
        public Xamarin.Forms.Label GetLabel()
        {
            return this.MessageLabel;
        }*/

        public
#if __MACOS__ || __IOS__
            override 
#endif
            void SetMessage(String message, int heightRequest = 60)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () =>
                {
                    this.MessageLabel.Text = message;
                    this.MessageLabel.WidthRequest = this.Width;
                    this.MessageLabel.HeightRequest = heightRequest;

                    this.MessageLabel.LineBreakMode = LineBreakMode.WordWrap;
                    this.MessageLabel.Focus();
                }
            );
        }
    }
}
