using Emgu.CV.Platform.Maui.UI;

namespace Emgu.TF.Maui.Demo
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            String aboutIcon = null;


            ToolbarItem aboutItem = new ToolbarItem("About", aboutIcon,
                () =>
                {
                    this.Navigation.PushAsync(new AboutPage());
                }
            );
            this.ToolbarItems.Add(aboutItem);

            Button inceptionButton = new Button();
            inceptionButton.Text = "Inception";

            Button flowerButton = new Button();
            flowerButton.Text = "Flower Recognition";

            Button multiboxDetectionButton = new Button();
            multiboxDetectionButton.Text = "People Detection";


            List<View> buttonList = new List<View>()
            {
                inceptionButton,
                flowerButton,
                multiboxDetectionButton
            };

            inceptionButton.Clicked += (sender, e) =>
            {
                Emgu.CV.Platform.Maui.UI.ProcessAndRenderPage inceptionPage = new ProcessAndRenderPage(
                    new InceptionModel(InceptionModel.Model.Default),
                    "Object recognition (Inception)",
                    "space_shuttle.jpg",
                    "Recognize object"
                );
                this.Navigation.PushAsync(inceptionPage);
            };

            flowerButton.Clicked += (sender, e) =>
            {
                Emgu.CV.Platform.Maui.UI.ProcessAndRenderPage inceptionPage = new ProcessAndRenderPage(
                    new InceptionModel(InceptionModel.Model.Flower),
                    "Flower Recognition (Inception)",
                    "tulips.jpg",
                    "Recognize flower"
                );
                this.Navigation.PushAsync(inceptionPage);
            };

            multiboxDetectionButton.Clicked += (sender, args) =>
            {
                Emgu.CV.Platform.Maui.UI.ProcessAndRenderPage multiboxPage = new ProcessAndRenderPage(
                    new MultiboxDetectionModel(),
                    "People detection",
                    "surfers.jpg",
                    "Detect people"
                );
                this.Navigation.PushAsync(multiboxPage);
            };
       

            //Only include stylize demo if QuantizeV2 is available.
            if (TfInvoke.OpHasKernel("QuantizeV2"))
            {
                Button stylizeButton = new Button();
                stylizeButton.Text = "Stylize";

                stylizeButton.Clicked += (sender, args) =>
                {
                    Emgu.CV.Platform.Maui.UI.ProcessAndRenderPage stylizePage = new ProcessAndRenderPage(
                        new StylizeModel(),
                        "Stylize",
                        "surfers.jpg",
                        "Stylize"
                    );
                    this.Navigation.PushAsync(stylizePage);
                };
                buttonList.Add(stylizeButton);
            }

            if (Device.RuntimePlatform != Device.Android)
            {
                //LoadSavedModel is not available for Android
                Button resnetButton = new Button();
                resnetButton.Text = "Resnet Object Recognition";
                resnetButton.Clicked += (sender, args) =>
                {
                    Emgu.CV.Platform.Maui.UI.ProcessAndRenderPage resnetPage = new ProcessAndRenderPage(
                        new ResnetModel(),
                        "Resnet Object Recognition",
                        "space_shuttle.jpg",
                        ""
                    );
                    this.Navigation.PushAsync(resnetPage);
                };
                buttonList.Add(resnetButton);
            }


            StackLayout buttonsLayout = new StackLayout
            {
                VerticalOptions = LayoutOptions.Start,
            };

            foreach (View b in buttonList)
                buttonsLayout.Children.Add(b);

            this.Content = new ScrollView()
            {
                Content = buttonsLayout,
            };
        }

        /*

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }*/
    }
}