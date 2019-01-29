//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


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
            String tensorflowVer = Emgu.TF.Lite.TfLiteInvoke.Version;

            Content = new StackLayout
            {
                Children = {
                    new WebView()
                    {
                        WidthRequest =  1000,
                        HeightRequest = 1000,
                        Source =  new HtmlWebViewSource()
                        {
                            Html = String.Format(
                            @"<html>
                    <body>
                    <H1> Emgu TF Lite Examples </H1>
                    <H3> Tensorflow Lite version: {0} </H3>
                    <H3> Tensorflow Lite <a href=https://github.com/tensorflow/tensorflow/blob/master/LICENSE > license</a> </H3>
                    <H3><a href=http://www.emgu.com/wiki/index.php/Emgu_TF >Visit our website</a> <br/><br/><H3>
                    <H3><a href=mailto:support@emgu.com>Email Support</a> <br/><br/><H3>"
                            + @"
                    </body>
                    </html>", tensorflowVer)
                        }
                    }
                }
            };
        }
    }
}
