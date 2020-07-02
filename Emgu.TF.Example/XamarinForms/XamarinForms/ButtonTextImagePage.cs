using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Forms;


namespace Emgu.TF.XamarinForms
{
    public partial class ButtonTextImagePage : ContentPage
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
            mainLayout.BackgroundColor = new Color(0, 1, 0);
            */

            _mainLayout.Children.Add(TopButton);
            _mainLayout.Children.Add(MessageLabel);
            _mainLayout.Children.Add(DisplayImage);
            
            Content = _mainLayout;
        }

        public bool HasCameraOption { get; set; }

        /// <summary>
        /// Allow user to pick the images.
        /// </summary>
        /// <param name="imageNames">The default image names</param>
        /// <param name="labels">The labels use for the pick image dialog, corresponding to each image</param>
        /// <returns>null if user canceled. Otherwise the list of images.</returns>
        public virtual async Task<String[]> LoadImages(String[] imageNames, String[] labels = null)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                //use default images
                return imageNames;
            }

            if (!(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX)
            || System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)))
                await Plugin.Media.CrossMedia.Current.Initialize(); //Implementation only available for iOS, Android

            String[] mats = new String[imageNames.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                String pickImgString = "Use Image from";
                if (labels != null && labels.Length > i)
                    pickImgString = labels[i];
                bool haveCameraOption;
                bool havePickImgOption;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    //CrossMedia is not implemented on Windows.
                    haveCameraOption = false;
                    havePickImgOption = true; //We will provide our implementation of pick image option
                }
                else
                {
                    haveCameraOption =
                        (Plugin.Media.CrossMedia.Current.IsCameraAvailable && Plugin.Media.CrossMedia.Current.IsTakePhotoSupported);
                    havePickImgOption =
                        Plugin.Media.CrossMedia.Current.IsPickVideoSupported;
                }

                String action;
                List<String> options = new List<string>();
                options.Add("Default");
                if (havePickImgOption)
                    options.Add("Photo Library");
                if (haveCameraOption)
                    options.Add("Photo from Camera");

#if __IOS__ || __ANDROID__
                if (this.HasCameraOption && haveCameraOption)
                    options.Add("Camera");
#endif
                if (options.Count == 1)
                {
                    action = "Default";
                }
                else
                {
                    action = await DisplayActionSheet(pickImgString, "Cancel", null, options.ToArray());
                    if (action == null) //user clicked outside of action sheet
                        return null;
                }
                /*
                if (haveCameraOption & havePickImgOption)
                {
                    action = await DisplayActionSheet(pickImgString, "Cancel", null, "Default", "Photo Library",
                        "Camera");
                }
                else if (havePickImgOption)
                {
                    action = await DisplayActionSheet(pickImgString, "Cancel", null, "Default", "Photo Library");
                }
                else
                {
                    action = "Default";
                }*/


                if (action.Equals("Default"))
                {
#if __ANDROID__
                    FileInfo fi = Emgu.TF.Util.AndroidFileAsset.WritePermanentFileAsset(Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity, imageNames[i], "tmp",
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
                        FileData fileData = await CrossFilePicker.Current.PickFile(new string[] { "Image | *.jpg;*.jpeg;*.png;*.bmp;*.gif | All Files | *" });
                        if (fileData == null)
                            return null;
                        mats[i] = fileData.FilePath;

                    }
                    else
                    {
                        var photoResult = await Plugin.Media.CrossMedia.Current.PickPhotoAsync();
                        if (photoResult == null) //canceled
                            return null;
                        mats[i] = photoResult.Path;
                    }
                }
                else if (action.Equals("Photo from Camera"))
                {
                    var mediaOptions = new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        Directory = "Emgu",
                        Name = $"{DateTime.UtcNow}.jpg"
                    };
                    var takePhotoResult = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(mediaOptions);
                    if (takePhotoResult == null) //canceled
                        return null;
                    mats[i] = takePhotoResult.Path;
                } else if (action.Equals("Camera"))
                {
                    mats[i] = "Camera";
                }

                //Handle user cancel
                if (action == null)
                    return null;
            }
            //InvokeOnImagesLoaded(mats);
            if (mats == null) //canceled
                return null;

            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == null)
                    return null; //canceled

            return mats;
        }

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

                   //For iOS
                   //Xamarin Form's Image class do not seems to re-render after Source is change
                   //forcing focus seems to force a re-rendering
                   this.DisplayImage.Focus();
               });

        }

        public void SetMessage(String message)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () =>
                {
                    this.MessageLabel.Text = message;
                    this.MessageLabel.WidthRequest = this.Width;
                    this.MessageLabel.HeightRequest = 60;

                    this.MessageLabel.LineBreakMode = LineBreakMode.WordWrap;
                    this.MessageLabel.Focus();
                }
            );
        }

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

        protected void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            String msg;
            if (e.TotalBytesToReceive > 0)
                msg = String.Format("{0} of {1} downloaded ({2}%)", ByteToSizeStr(e.BytesReceived), ByteToSizeStr(e.TotalBytesToReceive), e.ProgressPercentage);
            else
                msg = String.Format("{0} downloaded", ByteToSizeStr(e.BytesReceived));
            SetMessage(msg);
        }
    }
}
