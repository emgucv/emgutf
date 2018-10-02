//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

#if __ANDROID__ || __IOS__
using Plugin.Media;
#if __ANDROID__
using Plugin.CurrentActivity;
#endif
#elif __MACOS__
using AppKit;
using CoreGraphics;
#else
using Plugin.Media;
#endif

namespace Emgu.TF.XamarinForms
{
    public partial class ButtonTextImagePage : ContentPage
    {
        public ButtonTextImagePage()
        {
            InitializeComponent();
        }


        public virtual async void LoadImages(String[] imageNames, String[] labels = null)
        {
#if __ANDROID__ || __IOS__			
			await CrossMedia.Current.Initialize();
#endif			

#if (__MACOS__) //Xamarin Mac
            //use default images
            InvokeOnImagesLoaded(imageNames);
#else
            
            String[] mats = new String[imageNames.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                String pickImgString = "Use Image from";
                if (labels != null && labels.Length > i)
                    pickImgString = labels[i];

                bool haveCameraOption;
                bool havePickImgOption;
                if (Emgu.TF.Util.Platform.OperationSystem == Emgu.TF.Util.TypeEnum.OS.Windows)
                {
                    //CrossMedia is not implemented on Windows.
                    haveCameraOption = false;
                    havePickImgOption = false;
                }
                else
                {
                    haveCameraOption =
                        (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported);
                    havePickImgOption =
                        CrossMedia.Current.IsPickVideoSupported;
                }

                String action;
                if (haveCameraOption & havePickImgOption)
                {
                    action = await DisplayActionSheet(pickImgString, "Cancel", null, "Default", "Photo Library",
                        "Camera");
                } else if (havePickImgOption)
                {
                    action = await DisplayActionSheet(pickImgString, "Cancel", null, "Default", "Photo Library");
                }
                else
                {
                    action = "Default";
                }
                

                if (action.Equals("Default"))
                {
#if __ANDROID__
                    FileInfo fi = Emgu.TF.Util.AndroidFileAsset.WritePermanantFileAsset(CrossCurrentActivity.Current.Activity, imageNames[i], "tmp",
                        Emgu.TF.Util.AndroidFileAsset.OverwriteMethod.AlwaysOverwrite);

                    mats[i] = fi.FullName;

#else
                    mats[i] = imageNames[i];
            
#endif

                }
                else if (action.Equals("Photo Library"))
                {
                    var photoResult = await CrossMedia.Current.PickPhotoAsync();
                    if (photoResult == null) //cancelled
                        return;
                    mats[i] = photoResult.Path;
                }
                else if (action.Equals("Camera"))
                {
                    var mediaOptions = new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        Directory = "Emgu",
                        Name = $"{DateTime.UtcNow}.jpg"
                    };
                    var takePhotoResult = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
                    if (takePhotoResult == null) //cancelled
                        return;
                    mats[i] = takePhotoResult.Path;
                }
            }
            InvokeOnImagesLoaded(mats);
#endif
        }

        public void InvokeOnImagesLoaded(string[] images)
        {
            if (OnImagesLoaded != null)
                OnImagesLoaded(this, images);
        }

        public event EventHandler<string[]> OnImagesLoaded;

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
#endif
                });
        }

        public void SetImage(byte[] image = null, int widthRequest = -1, int heightRequest=-1)
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

#if __IOS__
                    //Xamarin Form's Image class do not seems to re-render after Soure is change
                    //forcing focus seems to force a re-rendering
                    this.DisplayImage.Focus();
#endif
                });
        }

        public Label GetLabel()
        {
            //return null;
            return this.MessageLabel;
        }

        public void SetMessage(String message)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () =>
            {
                this.MessageLabel.Text = message;
            }
            );
        }

        public Button GetButton()
        {
            //return null;
            return this.TopButton;
        }
    }
}
