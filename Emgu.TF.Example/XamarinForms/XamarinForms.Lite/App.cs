//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
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
            multiboxDetectionButton.Text = "People Detection";
            Button inceptionButton = new Button();
            inceptionButton.Text = "Object recognition";
            Button stylizeButton = new Button();
            stylizeButton.Text = "Stylize";

            // The root page of your application
            ContentPage page =
               new ContentPage
               {
                   Content = new StackLayout
                   {
                       VerticalOptions = LayoutOptions.Start,
                       Children =
                     {
                       multiboxDetectionButton,
                       inceptionButton,
                       stylizeButton
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
                   //page.DisplayAlert("Emgu TF Examples", "App version: ...", "Ok");
               }
            );
            page.ToolbarItems.Add(aboutItem);

            multiboxDetectionButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new MultiboxDetectionPage());
            };

            inceptionButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new InceptionPage());
            };

            stylizeButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new StylizePage());
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
