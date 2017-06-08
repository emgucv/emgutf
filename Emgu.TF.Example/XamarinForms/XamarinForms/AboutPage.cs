using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;


namespace Emgu.TF.XamarinForms
{
	public class AboutPage : ContentPage
	{
		public AboutPage ()
		{
		    String tensorFlowVer = TfInvoke.Version;

         Content = new StackLayout {
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
<H3> Emgu TF Examples </H3>
<H1> Tensorflow version: {0} </H1>
<a href=http://www.emgu.com/wiki/index.php/Emgu_TF >Visit our website</a> <br/><br/>
<a href=mailto:support@emgu.com>Email Support</a> <br/><br/>"
                     + @"
</body>
</html>", tensorFlowVer)
                  }
               }
				}
			};
		}
	}
}
