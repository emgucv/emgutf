using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Emgu.TF.Util;
using Xamarin.Forms;

#if __ANDROID__ || __IOS__
using Xamarin.Media;
#endif

namespace Emgu.TF.XamarinForms
{
    public partial class ButtonTextImagePage : ContentPage
    {

        public ButtonTextImagePage()
        {
            InitializeComponent();
        }

#if __ANDROID__ || __IOS__
        private MediaPicker _mediaPicker;
#endif

#if __ANDROID__
        public const int PickImageRequestCode = 1000;
#endif

        public virtual async void LoadImages(String[] imageNames, String[] labels = null)
        {
#if (__UNIFIED__ && !__IOS__) //NETFX or Xamarin Mac

            InvokeOnImagesLoaded(imageNames);
#else
            if (_mediaPicker == null)
            {
#if __ANDROID__
                _mediaPicker = new MediaPicker(Forms.Context);
#else
            _mediaPicker = new MediaPicker();
#endif
            }

            String[] mats = new String[imageNames.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                String pickImgString = "Use Image from";
                if (labels != null && labels.Length > i)
                    pickImgString = labels[i];
                var action = await (_mediaPicker.IsCameraAvailable ?
                   DisplayActionSheet(pickImgString, "Cancel", null, "Default", "Photo Library", "Camera")
                   : DisplayActionSheet(pickImgString, "Cancel", null, "Default", "Photo Library"));

                if (action.Equals("Default"))
                {
#if __ANDROID__

                    FileInfo fi = Emgu.TF.Util.AndroidFileAsset.WritePermanantFileAsset(Forms.Context, imageNames[i], "tmp",
                        Emgu.TF.Util.AndroidFileAsset.OverwriteMethod.AlwaysOverwrite);

                    mats[i] = fi.FullName;

#else
            mats[i] = imageNames[i];
            
#endif

                }
                else if (action.Equals("Photo Library"))
                {
#if __ANDROID__
                    Android.Content.Intent intent = _mediaPicker.GetPickPhotoUI();
                    Android.App.Activity activity = Forms.Context as Android.App.Activity;
                    activity.StartActivityForResult(intent, PickImageRequestCode);
                    //once the image was picked, the MainActivity.OnActivityResult function will handle the remaining work flow
                    Task t = new Task(() =>
                      { _waitHandle.WaitOne(); });
                    t.Start();
                    await t;
                    if (MatHandle == null) //Cancelled
                        return;
                    mats[i] = MatHandle;
#else
            var file = await _mediaPicker.PickPhotoAsync();
            mats[i] = file.Path;       
#endif
                }
                else if (action.Equals("Camera"))
                {
#if __ANDROID__
                    Android.Content.Intent intent = _mediaPicker.GetTakePhotoUI(new StoreCameraMediaOptions());
                    Android.App.Activity activity = Forms.Context as Android.App.Activity;
                    activity.StartActivityForResult(intent, PickImageRequestCode);
                    //once the image was picked, the MainActivity.OnActivityResult function will handle the remaining work flow
                    Task t = new Task(() =>
                       { _waitHandle.WaitOne(); });
                    t.Start();
                    await t;
                    if (MatHandle == null) //Cancelled
                        return;
                    mats[i] = MatHandle;
#else
            var file = await _mediaPicker.TakePhotoAsync(new StoreCameraMediaOptions());
                    mats[i] = file.Path;
#endif
                }
            }
            InvokeOnImagesLoaded(mats);
#endif
        }

#if __ANDROID__
        private readonly System.Threading.EventWaitHandle _waitHandle = new System.Threading.AutoResetEvent(false);
        public void Continute()
        {
            _waitHandle.Set();
        }
        public String MatHandle;
#endif

        public void InvokeOnImagesLoaded(string[] images)
        {
            if (OnImagesLoaded != null)
                OnImagesLoaded(this, images);
        }

        public event EventHandler<string[]> OnImagesLoaded;

        public void SetImage(String fileName)
        {
            var imageSource = new FileImageSource();
            imageSource.File = fileName;
            this.DisplayImage.Source = imageSource;
        }

        public void SetImage(byte[] image = null)
        {
            if (image == null)
            {
                this.DisplayImage.Source = null;
                return;
            }

            this.DisplayImage.Source = ImageSource.FromStream(() => new MemoryStream(image));

        }

        public Label GetLabel()
        {
            //return null;
            return this.MessageLabel;
        }

        public Button GetButton()
        {
            //return null;
            return this.TopButton;
        }
    }
}
