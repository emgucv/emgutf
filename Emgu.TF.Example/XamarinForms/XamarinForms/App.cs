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
            Emgu.TF.TfInvoke.CheckLibraryLoaded();

            TabbedPage tabbedPage = new TabbedPage();
            tabbedPage.Title = "Emgu TF Demos";
            tabbedPage.Children.Add(new AboutPage());

            tabbedPage.Children.Add(new MultiboxDetectionPage());
            tabbedPage.Children.Add(new InceptionPage(InceptionPage.Model.Default));
            tabbedPage.Children.Add(new InceptionPage(InceptionPage.Model.Flower));
            tabbedPage.Children.Add(new ResnetPage());

            if (TfInvoke.OpHasKernel("QuantizeV2"))
            {
                tabbedPage.Children.Add(new StylizePage());
            }
            MainPage = tabbedPage;
            /*
            List<View> buttons = new List<View>();

            Button multiboxDetectionButton = new Button();
            multiboxDetectionButton.Text = "People Detection";
            multiboxDetectionButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new MultiboxDetectionPage());
            };
            buttons.Add(multiboxDetectionButton);

            Button inceptionButton = new Button();
            inceptionButton.Text = "Object recognition (Inception)";
            inceptionButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new InceptionPage(InceptionPage.Model.Default));
            };
            buttons.Add(inceptionButton);

            Button flowerButton = new Button();
            flowerButton.Text = "Flower Recognition";
            flowerButton.Clicked += (sender, args) =>
            {
                MainPage.Navigation.PushAsync(new InceptionPage(InceptionPage.Model.Flower));
            };
            buttons.Add(flowerButton);

            //Only include stylize demo if QuantizeV2 is available.
            if (TfInvoke.OpHasKernel("QuantizeV2"))
            {
                Button stylizeButton = new Button();
                stylizeButton.Text = "Stylize";
                stylizeButton.Clicked += (sender, args) => { MainPage.Navigation.PushAsync(new StylizePage()); };
                buttons.Add(stylizeButton);
            }

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
            */
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
