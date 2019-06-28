using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;


namespace Emgu.TF.XamarinForms
{
    public class AboutPage : ContentPage
    {
        public AboutPage()
        {
            String tensorflowVer = TfInvoke.Version;
            Title = "About Emgu TF";
            Content =
            //new StackLayout
            //{
                //Children = {
                    new WebView()
                    {
                        WidthRequest =  400,
                        HeightRequest = 1000,
                        Source =  new HtmlWebViewSource()
                        {
                            Html = String.Format(
                            @"<html>
                    <body>
                    <H1> Emgu TF Demos </H1>
                    <H3> Tensorflow version: {0} </H3>
                    <H3> OS: {1} </H3>
                    <H3> Framework: {2} </H3>
                    <H3> Processor: {3} </H3>
                    <H3> Tensorflow  <a href=https://github.com/tensorflow/tensorflow/blob/master/LICENSE > License</a> </H3>
                    <H3><a href=http://www.emgu.com/wiki/index.php/Emgu_TF >Visit our website</a> <br/><br/><H3>
                    <H3><a href=mailto:support@emgu.com>Email Support</a> <br/><br/><H3>"
                            + @"
                    </body>
                    </html>",
                            tensorflowVer,
                            System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                            System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                            System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture)
                        }
                    //}
                //}
            };
        }
    }
}
