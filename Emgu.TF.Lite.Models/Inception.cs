//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Emgu.TF.Lite;
using Emgu.Models;
using System.IO;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
using UnityEngine;
#endif

namespace Emgu.TF.Lite.Models
{
    /// <summary>
    /// The inception model for object class labeling 
    /// </summary>
    public class Inception : Emgu.TF.Util.UnmanagedObject
    {
        private FileDownloadManager _downloadManager;
        
        private Interpreter _interpreter = null;
        private String[] _labels = null;
        private FlatBufferModel _model = null;
        private Tensor _inputTensor;
        private Tensor _outputTensor;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
        /// <summary>
        /// Get the download progress
        /// </summary>
        public double DownloadProgress
        {
            get
            {
                if (_downloadManager == null)
                    return 0;
                if (_downloadManager.CurrentWebClient == null)
                    return 1;
                return _downloadManager.CurrentWebClient.downloadProgress;
            }
        }

        /// <summary>
        /// Get the name of the file that is currently being downloaded.
        /// </summary>
        public String DownloadFileName
        {
            get
            {
                if (_downloadManager == null)
                    return null;
                if (_downloadManager.CurrentWebClient == null)
                    return null;
                return _downloadManager.CurrentWebClient.url;
            }
        }
#endif

        /// <summary>
        /// Create a Inception model for object labeling.
        /// </summary>
        public Inception()
        {
            _downloadManager = new FileDownloadManager();

            _downloadManager.OnDownloadProgressChanged += onDownloadProgressChanged;
        }

        private void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (OnDownloadProgressChanged != null)
                OnDownloadProgressChanged(sender, e);
        }

        /// <summary>
        /// Callback when the download progress has been changed.
        /// </summary>
        public event System.Net.DownloadProgressChangedEventHandler OnDownloadProgressChanged;

        /// <summary>
        /// Initialize the graph by downloading the model from the Internet
        /// </summary>
        /// <param name="modelFiles">The model file names as an array. First one is the ".tflite" file and the second one should be the label names.</param>
        /// <param name="downloadUrl">The url where the files can be downloaded from.</param>
        /// <param name="localModelFolder">The subfolder where the file will be stored.</param>
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                String[] modelFiles = null, 
                String downloadUrl = null, 
                String localModelFolder = "Inception")
        {

            _downloadManager.Clear();
            String url = downloadUrl == null ? "https://github.com/emgucv/models/raw/master/inception_flower_retrain/" : downloadUrl;
            String[] fileNames = modelFiles == null ? new string[] { "optimized_graph.tflite", "output_labels.txt" } : modelFiles;
            for (int i = 0; i < fileNames.Length; i++)
                _downloadManager.AddFile(url + fileNames[i], localModelFolder);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            yield return _downloadManager.Download();
#else
            await _downloadManager.Download();
            ImportGraph();
#endif
        }

        /// <summary>
        /// Return true if the graph has been imported
        /// </summary>
        public bool Imported
        {
            get
            {
                return _interpreter != null;
            }
        }

        private void ImportGraph()
        {


#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            UnityEngine.Debug.Log("Reading model definition");
#endif

            if (_labels == null)
            {
                String labelFileName = _downloadManager.Files[1].LocalFile;
                if (!File.Exists(labelFileName))
                    throw new Exception(String.Format("File {0} doesn't exist", labelFileName));
                _labels = File.ReadAllLines(labelFileName);
            }

            if (_model == null)
            {
                String modelFileName = _downloadManager.Files[0].LocalFile;
                if (!File.Exists(modelFileName))
                    throw new Exception(String.Format("File {0} doesn't exist", modelFileName));
                _model = new FlatBufferModel(modelFileName);
                if (!_model.CheckModelIdentifier())
                    throw new Exception("Model identifier check failed");
            }

            if (_interpreter == null)
            {
                _interpreter = new Interpreter(_model);
                /*
                using (NNAPIDelegate d = new NNAPIDelegate())
                {
                    if (d.IsSupported)
                    {
                        _interpreter.UseNNAPI(true);
                    }
                }*/
                Status allocateTensorStatus = _interpreter.AllocateTensors();
                if (allocateTensorStatus == Status.Error)
                    throw new Exception("Failed to allocate tensor");
            }

            if (_inputTensor == null)
            {

                int[] input = _interpreter.InputIndices;
                _inputTensor = _interpreter.GetTensor(input[0]);
                
            }

            if (_outputTensor == null)
            {
                int[] output = _interpreter.OutputIndices;
                _outputTensor = _interpreter.GetTensor(output[0]);   
            }
        }

        /// <summary>
        /// Get the interpreter for this graph
        /// </summary>
        public Interpreter Interpreter
        {
            get
            {
                return _interpreter;
            }
        }

        /// <summary>
        /// Get the labels
        /// </summary>
        public String[] Labels
        {
            get { return _labels; }
        }

        /// <summary>
        /// Sort the result from the most likely to the less likely
        /// </summary>
        /// <param name="probabilities">The probability for the classes, this should be the values of the output tensor</param>
        /// <returns>The recognition result, sorted by likelihood.</returns>
        public RecognitionResult[] SortResults(float[] probabilities)
        {
            if (probabilities == null)
                return null;

            RecognitionResult[] results = new RecognitionResult[probabilities.Length];
            for (int i = 0; i < probabilities.Length; i++)
            {
                results[i] = new RecognitionResult(_labels[i], probabilities[i]);
            }
            Array.Sort<RecognitionResult>(results, new Comparison<RecognitionResult>((a, b) => -a.Probability.CompareTo(b.Probability)));
            return results;
        }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
        public RecognitionResult[] Recognize(
            Texture2D texture2D, 
            int inputHeight = 299,
            int inputWidth = 299, 
            float inputMean = 0.0f,
            float scale = 1.0f/255.0f,
            bool flipUpsideDown=true, 
            bool swapBR = false)
        {
            NativeImageIO.ReadTensorFromTexture<float>(
                texture2D, 
                _inputTensor.DataPointer, 
                inputHeight, 
                inputWidth,
                inputMean, 
                scale,
                flipUpsideDown,
                swapBR);

            _interpreter.Invoke();

            float[] probability = _outputTensor.GetData(false) as float[];
            if (probability == null)
                return null;

            return SortResults(probability);

        }
#else
        /// <summary>
        /// Load the file, ran it through the mobile net graph and return the recognition results
        /// </summary>
        /// <param name="imageFile">The image to be loaded</param>
        /// <param name="width">The width of the input tensor</param>
        /// <param name="height">The height of the input tensor</param>
        /// <param name="mean">The mean to be subtracted when converting the image to input tensor</param>
        /// <param name="scale">The scale to be multiplied when converting the image to input tensor</param>
        /// <param name="flipUpsideDown">If true, the image will be flipped upside down when it is coverted to input tensor</param>
        /// <param name="swapBR">If true, the blue and red channel will be swapped when converting the image to input tensor </param>
        /// <returns>The recognition result sorted by probability</returns>
        public RecognitionResult[] Recognize(String imageFile, int width = 299, int height = 299, float mean = 0.0f, float scale = 1.0f/255.0f, bool flipUpsideDown = false, bool swapBR = true)
        {
            NativeImageIO.ReadImageFileToTensor<float>(imageFile, _inputTensor.DataPointer, height, width, mean, scale, flipUpsideDown, swapBR);

            _interpreter.Invoke();

            float[] probability = _outputTensor.Data as float[];
            if (probability == null)
                return null;
            return SortResults(probability);

        }

#endif
        /// <summary>
        /// The result of the class labeling
        /// </summary>
        public class RecognitionResult
        {
            /// <summary>
            /// Create a recognition result by providing the label and the probability
            /// </summary>
            /// <param name="label">The label</param>
            /// <param name="probability">The probability</param>
            public RecognitionResult(String label, double probability)
            {
                Label = label;
                Probability = probability;
            }

            /// <summary>
            /// The label
            /// </summary>
            public String Label;
            /// <summary>
            /// The probability
            /// </summary>
            public double Probability;
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this graph
        /// </summary>
        protected override void DisposeObject()
        {
            
            if (_interpreter != null)
            {
                _interpreter.Dispose();
                _interpreter = null;
            }

            if (_model != null)
            {
                _model.Dispose();
                _model = null;
            }
        }
    }
}
