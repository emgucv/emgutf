//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Emgu.TF.XamarinForms
{
    public class App : Application
    {
        public App()
        {
            Emgu.TF.Lite.TfLiteInvoke.Init();

            Button multiboxDetectionButton = new Button();
            multiboxDetectionButton.Text = "Coco SSD Mobilenet";
            
            Button mobilenetButton = new Button();
            mobilenetButton.Text = "Mobilenet Object recognition";
            /*
            Button smartReplyButton = new Button();
            smartReplyButton.Text = "Smart Reply";
            */

            Button inceptionButton = new Button();
            inceptionButton.Text = "Inception Flower recognition";

            Button modelCheckerButton = new Button();
            modelCheckerButton.Text = "TF Lite model checker";

            /*
#if __IOS__ || __MACOS__
            Button cameraViewButton = new Button();
            cameraViewButton.Text = "Camera View";
            cameraViewButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new CameraViewPage());
            };
#endif*/

            // The root page of your application
            ContentPage page =
               new ContentPage
               {
                   Content = new StackLayout
                   {
                       VerticalOptions = LayoutOptions.Center,
                       Children =
                     {
                           multiboxDetectionButton,
                           //smartReplyButton,
                           mobilenetButton,
                           inceptionButton, 
                           modelCheckerButton
                     }
                   }
               };

            NavigationPage navigationPage = new NavigationPage(page);

            //Fix for UWP navigation text
            if (Device.RuntimePlatform == Device.WPF)
                navigationPage.BarTextColor = Color.Green;

            MainPage = navigationPage;

            ToolbarItem aboutItem = new ToolbarItem("About", null,
               () =>
               {
                   MainPage.Navigation.PushAsync(new AboutPage());
               }
            );
            page.ToolbarItems.Add(aboutItem);

            
            multiboxDetectionButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new CocoSsdMobilenetPage());
            };

            mobilenetButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new MobilenetPage());
            };

            /*
            smartReplyButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new SmartReplyPage());
            };*/
            inceptionButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new InceptionPage());
            };

            modelCheckerButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new ModelCheckerPage());
            };
        }

        public Page CurrentPage
        {
            get
            {
                NavigationPage np = MainPage as NavigationPage;
                return np.CurrentPage;
            }
        }


        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
