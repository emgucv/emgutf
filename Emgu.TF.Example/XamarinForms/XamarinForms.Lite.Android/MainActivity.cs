//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Android;

namespace XamarinForms.Droid
{
    [Activity(Label = "Emgu TF Lite", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState); // add this line to your code, it may also be called: bundle

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            CheckAppPermissions();

            Emgu.TF.Lite.TfLiteInvokeAndroid.Init();

            LoadApplication(new Emgu.TF.XamarinForms.App());
        }

        private void CheckAppPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return;
            }
            else
            {
                if (PackageManager.CheckPermission(Manifest.Permission.ReadMediaImages, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.ReadMediaAudio, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.ReadMediaVideo, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.Camera, PackageName) != Permission.Granted)
                {
                    var permissions = new string[]
                    {
                        Manifest.Permission.ReadMediaImages,
                        Manifest.Permission.ReadMediaAudio,
                        Manifest.Permission.ReadMediaVideo,
                        Manifest.Permission.WriteExternalStorage, 
                        Manifest.Permission.Camera
                    };
                    RequestPermissions(permissions, 1);
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

