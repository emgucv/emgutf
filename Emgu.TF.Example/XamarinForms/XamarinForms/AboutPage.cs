using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tensorflow;
using Xamarin.Forms;


namespace Emgu.TF.XamarinForms
{
    public class AboutPage : ContentPage
    {
        /// <summary>
        /// Create and run a simple graph that add two numbers and returns the default session devices used.
        /// </summary>
        /// <returns></returns>
        private static Session.Device[] GetSessionDevices()
        {
            SessionOptions so = new SessionOptions();
            if (TfInvoke.IsGoogleCudaEnabled)
            {
                Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
                config.GpuOptions = new Tensorflow.GPUOptions();
                config.GpuOptions.AllowGrowth = true;
                so.SetConfig(config.ToProtobuf());
            }
            int a = 1;
            int b = 1;
            //Creating tensor from value a
            Tensor tensorA = new Tensor(a);
            //Creating tensor from value b
            Tensor tensorB = new Tensor(b);
            //Create a new graph
            Graph graph = new Graph();
            //Place holder in the graph for tensorA
            Operation opA = graph.Placeholder(DataType.Int32, null, "valA");
            //Place holder in the graph for tensorB
            Operation opB = graph.Placeholder(DataType.Int32, null, "valB");
            //Adding the two tensor
            Operation sumOp = graph.Add(opA, opB, "sum");

            //Create a new session
            using (Session session = new Session(graph, so))
            {
                //Execute the session and get the sum
                Tensor[] results = session.Run(new Output[] {opA, opB}, new Tensor[] {tensorA, tensorB},
                    new Output[] {sumOp});

                Session.Device[] devices = session.ListDevices(null);
                return devices;
            }
        }

        public AboutPage()
        {
            Session.Device[] devices = GetSessionDevices();
            StringBuilder sb = new StringBuilder();
            foreach (Session.Device d in devices)
            {
                sb.Append(String.Format("<H4 style=\"color: blue;\">{1}: {0}</H4>", d.Name, d.Type));
            }
            String tensorflowVer = TfInvoke.Version;
            Title = "About Emgu TF";
            Content =

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
                    <H3> Default Session Devices: </H3> {4}
                    <H3> Tensorflow  <a href=https://github.com/tensorflow/tensorflow/blob/master/LICENSE > License</a> </H3>
                    <H3><a href=http://www.emgu.com/wiki/index.php/Emgu_TF >Visit our website</a> <br/><br/><H3>
                    <H3><a href=mailto:support@emgu.com>Email Support</a> <br/><br/><H3>"
                            + @"
                    </body>
                    </html>",
                            tensorflowVer,
                            System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                            System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                            System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture,
                            sb.ToString())
                        }
            };
        }
    }
}
