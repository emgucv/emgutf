using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            StackLayout mainLayout = new StackLayout();
            mainLayout.VerticalOptions = LayoutOptions.FillAndExpand;
            mainLayout.HorizontalOptions = LayoutOptions.FillAndExpand;
            mainLayout.Orientation = StackOrientation.Vertical;
            mainLayout.Spacing = 15;
            mainLayout.Padding = new Thickness(10, 10, 10, 10);

            DisplayImage.HorizontalOptions = LayoutOptions.Center;
            
            /*
            DisplayImage.BackgroundColor = new Color(1, 0, 0);
            MessageLabel.BackgroundColor = new Color(0, 0, 1);
            mainLayout.BackgroundColor = new Color(0, 1, 0);
            */

            mainLayout.Children.Add(TopButton);
            mainLayout.Children.Add(MessageLabel);
            mainLayout.Children.Add(DisplayImage);
            
            Content = mainLayout;
        }

        public virtual async void LoadImages(String[] imageNames, String[] labels = null)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                //use default images
                InvokeOnImagesLoaded(imageNames);
                return;
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
                }


                if (action.Equals("Default"))
                {
#if __ANDROID__
                    FileInfo fi = Emgu.TF.Util.AndroidFileAsset.WritePermanantFileAsset(Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity, imageNames[i], "tmp",
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
                        // our implementation of pick image for Windows
                        /*
                        using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
                        {
                            dialog.Multiselect = false;
                            dialog.Title = "Select an Image File";
                            dialog.Filter = "Image | *.jpg;*.jpeg;*.png;*.bmp;*.gif | All Files | *";
                            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                mats[i] = dialog.FileName;
                            }
                            else
                            {
                                return; 
                            }
                        }*/
                        System.Reflection.Assembly windowsFormsAssembly = Emgu.TF.Util.Toolbox.FindAssembly("System.Windows.Forms.dll");
                        if (windowsFormsAssembly != null)
                        {
                            //Running on Windows
                            Type openFileDialogType = windowsFormsAssembly.GetType("System.Windows.Forms.OpenFileDialog");
                            Type dialogResultType = windowsFormsAssembly.GetType("System.Windows.Forms.DialogResult");
                            if (openFileDialogType != null && dialogResultType != null)
                            {
                                object dialog = Activator.CreateInstance(openFileDialogType);
                                openFileDialogType.InvokeMember(
                                    "Multiselect",
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                                    Type.DefaultBinder,
                                    dialog,
                                    new object[] { (object) false });
                                openFileDialogType.InvokeMember(
                                    "Title", 
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                                    Type.DefaultBinder, 
                                    dialog, 
                                    new object[] { (object) "Select an Image File"});
                                openFileDialogType.InvokeMember(
                                    "Filter",
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                                    Type.DefaultBinder,
                                    dialog,
                                    new object[] { (object)"Image | *.jpg;*.jpeg;*.png;*.bmp;*.gif | All Files | *" });
                                object dialogResult = openFileDialogType.InvokeMember(
                                    "ShowDialog",
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
                                    Type.DefaultBinder,
                                    dialog,
                                    null);
                                String dialogResultStr = Enum.GetName(dialogResultType, dialogResult);
                                if (dialogResultStr.Equals("OK"))
                                {
                                    //mats[i] = dialog.FileName;
                                    String fileName = openFileDialogType.InvokeMember(
                                        "FileName",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                                        Type.DefaultBinder,
                                        dialog,
                                        null) as String;
                                    mats[i] = fileName;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }

                    }
                    else
                    {
                        var photoResult = await Plugin.Media.CrossMedia.Current.PickPhotoAsync();
                        if (photoResult == null) //canceled
                            return;
                        mats[i] = photoResult.Path;
                    }
                }
                else if (action.Equals("Camera"))
                {
                    var mediaOptions = new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        Directory = "Emgu",
                        Name = $"{DateTime.UtcNow}.jpg"
                    };
                    var takePhotoResult = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(mediaOptions);
                    if (takePhotoResult == null) //canceled
                        return;
                    mats[i] = takePhotoResult.Path;
                }

                //Handle user cancel
                if (action == null)
                    return;
            }
            InvokeOnImagesLoaded(mats);
        }

        public void InvokeOnImagesLoaded(string[] imageFiles)
        {
            if (imageFiles == null) //cancelled
                return;

            for (int i = 0; i < imageFiles.Length; i++)
                if (imageFiles[i] == null)
                    return; //cancelled

            if (OnImagesLoaded != null)
                OnImagesLoaded(this, imageFiles);
        }

        public event EventHandler<string[]> OnImagesLoaded;

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
    }
}
