﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
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
using System.Runtime.InteropServices;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
using UnityEngine;
#endif

namespace Emgu.TF.Lite.Models
{
    /// <summary>
    /// The mobile net model for object class labeling 
    /// </summary>
    public class Mobilenet : Emgu.TF.Util.UnmanagedObject
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
        /// Create a Mobilenet for object labeling.
        /// </summary>
        public Mobilenet()
        {
            _downloadManager = new FileDownloadManager();
            _downloadManager.OnDownloadProgressChanged += onDownloadProgressChanged;
        }

        private void onDownloadProgressChanged(long? totalBytesToReceive, long bytesReceived, double? progressPercentage)
        {
            if (OnDownloadProgressChanged != null)
                OnDownloadProgressChanged(totalBytesToReceive, bytesReceived, progressPercentage);
        }

        /// <summary>
        /// Callback when model download progress is changed.
        /// </summary>
        public event FileDownloadManager.DownloadProgressChangedEventHandler OnDownloadProgressChanged;


        /// <summary>
        /// Initialize the graph by downloading the model from the internet
        /// </summary>
        /// <param name="modelFile">The tflite flatbuffer model files</param>
        /// <param name="labelFile">Text file that contains the labels</param>
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                DownloadableFile modelFile = null,
                DownloadableFile labelFile = null)
        {

            _downloadManager.Clear();

            String defaultLocalSubfolder = "Mobilenet";
            if (modelFile == null)
            {
                modelFile = new DownloadableFile(
                    "https://github.com/emgucv/models/raw/master/mobilenet_v1_1.0_224_float_2017_11_08/mobilenet_v1_1.0_224.tflite",
                    defaultLocalSubfolder,
                    "FDACE547B17907FA22821F4898F2AF49DCE7787FE688AD7C5D8D0220F3781C65"
                );
            }

            if (labelFile == null)
            {
                labelFile = new DownloadableFile(
                    "https://github.com/emgucv/models/raw/master/mobilenet_v1_1.0_224_float_2017_11_08/labels.txt",
                    defaultLocalSubfolder,
                    "536FEACC519DE3D418DE26B2EFFB4D75694A8C4C0063E36499A46FA8061E2DA9"
                );
            }

            _downloadManager.AddFile(modelFile);
            _downloadManager.AddFile(labelFile);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            yield return _downloadManager.Download();
#else
            await _downloadManager.Download();
#endif
            if (_downloadManager.AllFilesDownloaded)
                ImportGraph();
            else
            {
                System.Diagnostics.Trace.WriteLine("Failed to download all files");
            }
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
                _model = new FlatBufferModel(modelFileName);
                if (!File.Exists(modelFileName))
                    throw new Exception(String.Format("File {0} doesn't exist", modelFileName));
                if (!_model.CheckModelIdentifier())
                    throw new Exception("Model identifier check failed");
            }

            if (_interpreter == null)
            {
                _interpreter = new Interpreter(_model);
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
        /// Get the input tensor for this graph
        /// </summary>
        public Tensor InputTensor
        {
            get
            {
                return _inputTensor;
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
        public RecognitionResult[] Recognize(Texture texture, bool flipUpsideDown=true, bool swapBR = false)
        {
            NativeImageIO.ReadTensorFromTexture<float>(
                texture, 
                _inputTensor.DataPointer, 
                224, 224, 128.0f, 1.0f / 128.0f,
                flipUpsideDown,
                swapBR);

            return Invoke();

        }
#else
        /// <summary>
        /// Load the file, ran it through the mobile net graph and return the recognition results
        /// </summary>
        /// <param name="imageFile">The image to be loaded</param>
        /// <returns>The recognition result sorted by probability</returns>
        public RecognitionResult[] Recognize(String imageFile)
        {
            NativeImageIO.ReadImageFileToTensor<float>(imageFile, _inputTensor.DataPointer, 224, 224, 128.0f, 1.0f / 128.0f);

            return Invoke();
        }

#endif

        /// <summary>
        /// Run data in the input tensor through the mobile net graph and return the recognition results
        /// </summary>
        /// <returns>The recognition result sorted by probability</returns>
        public RecognitionResult[] Invoke()
        {
            _interpreter.Invoke();

            float[] probability = _outputTensor.Data as float[];
            if (probability == null)
                return null;
            return SortResults(probability);
        }

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
