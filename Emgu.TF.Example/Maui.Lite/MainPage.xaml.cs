namespace Maui.Demo.Lite;

public partial class MainPage : ContentPage
{

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

        Button multiboxDetectionButton = new Button();
        multiboxDetectionButton.Text = "Coco SSD Mobilenet";

        Button mobilenetButton = new Button();
        mobilenetButton.Text = "Mobilenet Object recognition";
        mobilenetButton.Clicked += (sender, args) =>
        {
            this.Navigation.PushAsync(new MobilenetPage());
        };

        /*
        Button smartReplyButton = new Button();
        smartReplyButton.Text = "Smart Reply";
        */

        Button inceptionButton = new Button();
        inceptionButton.Text = "Inception Flower recognition";
        inceptionButton.Clicked += (sender, args) =>
        {
            this.Navigation.PushAsync(new InceptionPage());
        };

        Button modelCheckerButton = new Button();
        modelCheckerButton.Text = "TF Lite model checker";

        modelCheckerButton.Clicked += (sender, args) =>
        {
            this.Navigation.PushAsync(new ModelCheckerPage());
        };


        List<View> buttonList = new List<View>()
        {
            multiboxDetectionButton,
            mobilenetButton,
            inceptionButton,
            modelCheckerButton
        };
        

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


}

