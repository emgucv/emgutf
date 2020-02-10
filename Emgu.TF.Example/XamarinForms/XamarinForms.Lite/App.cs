//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
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
            Emgu.TF.Lite.TfLiteInvoke.CheckLibraryLoaded();

            Button multiboxDetectionButton = new Button();
            multiboxDetectionButton.Text = "Coco SSD Mobilenet";
            
            Button mobilenetButton = new Button();
            mobilenetButton.Text = "Object recognition";
            /*
            Button smartReplyButton = new Button();
            smartReplyButton.Text = "Smart Reply";
            */

            Button inceptionButton = new Button();
            inceptionButton.Text = "Flower recognition";

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

#if NETFX_CORE
		    String aboutIcon = "questionmark.png";
#else
            String aboutIcon = null;
#endif

            MainPage =
             new NavigationPage(
                page
             );

            ToolbarItem aboutItem = new ToolbarItem("About", aboutIcon,
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
