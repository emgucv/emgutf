using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Emgu.TF.Lite;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Forms;

namespace Emgu.TF.XamarinForms
{
    public class ModelCheckerPage : ButtonTextImagePage
    {
        private Xamarin.Forms.Editor _editor;

        public ModelCheckerPage()
            : base()
        {
            var button = this.TopButton;
            button.Text = "Check TF Lite Model";
            button.Clicked += OnButtonClicked;

            DisplayImage.IsVisible = false;
            
            //MessageLabel.MaxLines = -1;
            //MessageLabel.BackgroundColor = Color.Aqua;
            
            
            _editor = new Editor();
            //_editor.BackgroundColor = Color.BlueViolet;
            
            MainLayout.Children.Add(_editor);
            
            SetMessage("Please select a .tflite file to see the model parameters.");
        }

        
        void SetEditorMessage(String message, int heightRequest = 600)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () =>
                {
                    this._editor.Text = message;
                    this._editor.WidthRequest = this.Width;
                    this._editor.HeightRequest = heightRequest;
                    this._editor.Focus();
                }
            );
        }
        

        private static String IntArrayToString(int[] values)
        {
            return String.Format("[{0}]", String.Join(",", values));
        }

        private static String GetModelInfo(String fileName)
        {
            StringBuilder modelResult = new StringBuilder();
            try
            {
                modelResult.Append(String.Format("File Name:{0}{1}", fileName, Environment.NewLine));
                //string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
                using (FlatBufferModel fbm = new FlatBufferModel(fileName))
                {
                    if (!fbm.CheckModelIdentifier())
                        throw new Exception("Model identifier check failed");

                    using (Interpreter interpreter = new Interpreter(fbm))
                    {
                        Status allocateTensorStatus = interpreter.AllocateTensors();
                        if (allocateTensorStatus == Status.Error)
                            throw new Exception("Failed to allocate tensor");
                        int[] input = interpreter.InputIndices;
                        for (int i = 0; i < input.Length; i++)
                        {
                            Tensor inputTensor = interpreter.GetTensor(input[i]);

                            modelResult.Append(String.Format("Input {0} ({1}): {2}{3}{4}", i, inputTensor.Name,
                                inputTensor.Type, IntArrayToString( inputTensor.Dims ), Environment.NewLine));
                        }

                        int[] output = interpreter.OutputIndices;
                        for (int i = 0; i < output.Length; i++)
                        {
                            Tensor outputTensor = interpreter.GetTensor(output[i]);

                            modelResult.Append(String.Format("Output {0} ({1}): {2}{3}{4}", i, outputTensor.Name,
                                outputTensor.Type, IntArrayToString( outputTensor.Dims ), Environment.NewLine));
                        }

                    }
                }

                return modelResult.ToString();
            }
            catch (Exception ex)
            {
                modelResult.Append(String.Format("Exception processing file {0}: {1}{2} ", fileName, ex.ToString(),
                    Environment.NewLine));
                return modelResult.ToString();
            }
        }

        private async void OnButtonClicked(Object sender, EventArgs args)
        {

            FileData fileData = await CrossFilePicker.Current.PickFile();
            if (fileData == null)
                return; // user canceled file picking

            //string fileName = fileData.FileName;
            string fileName = fileData.FilePath;

            SetEditorMessage(GetModelInfo(fileName), 600);

        }
    }
}
