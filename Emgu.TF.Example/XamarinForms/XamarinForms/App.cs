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
            Emgu.TF.TfInvoke.CheckLibraryLoaded();

            List<View> buttons = new List<View>();

            Button multiboxDetectionButton = new Button();
            multiboxDetectionButton.Text = "People Detection";
            multiboxDetectionButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new MultiboxDetectionPage());
            };
            buttons.Add(multiboxDetectionButton);

            Button inceptionButton = new Button();
            inceptionButton.Text = "Object recognition";
            inceptionButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new InceptionPage());
            };
            buttons.Add(inceptionButton);

#if __ANDROID__ || __UNIFIED__
            //Only add stylize for Android, iOS and Mac.
            //Quantization is not available on Windows
            Button stylizeButton = new Button();
            stylizeButton.Text = "Stylize";
            stylizeButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new StylizePage());
            };
            buttons.Add(stylizeButton);
#endif
            StackLayout layout = new StackLayout
            {
                VerticalOptions = LayoutOptions.Start,
                Children = { }
            };
            foreach(View v in buttons)
            {
                layout.Children.Add(v);
            }

            // The root page of your application
            ContentPage page =
               new ContentPage
               {
                   Content = layout
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
