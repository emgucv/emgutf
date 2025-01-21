﻿namespace Emgu.TF.Maui.Demo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Emgu.CV.Platform.Maui.MauiInvoke.Init();
            Emgu.TF.Platform.Maui.MauiInvoke.Init();

        }
		
		protected override Window CreateWindow(IActivationState? activationState)
		{
			return new Window(new AppShell());
		}
    }
}