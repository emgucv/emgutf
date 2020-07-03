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
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
using UnityEngine;
#endif

#if __IOS__
using UIKit;
using CoreGraphics;
#elif __MACOS__
using AppKit;
#endif

namespace Emgu.TF.Lite.Models
{
    /// <summary>
    /// Coco ssd mobile net base model
    /// </summary>
    public class CocoSsdMobilenet : Emgu.TF.Util.UnmanagedObject
    {
        private FileDownloadManager _downloadManager;

        private Interpreter _interpreter = null;
        private String[] _labels = null;
        private FlatBufferModel _model = null;
        private Tensor _inputTensor;
        private Tensor[] _outputTensors;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
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
        /// Create a new coco ssd mobilenet model.
        /// </summary>
        public CocoSsdMobilenet()
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
        /// Event handler that triggers when download progress changed.
        /// </summary>
        public event System.Net.DownloadProgressChangedEventHandler OnDownloadProgressChanged;

        public virtual
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                String[] modelFiles, 
                String downloadUrl,
                String localModelFolder)
        {

            _downloadManager.Clear();
            String url = downloadUrl;
            String[] fileNames = modelFiles;
            for (int i = 0; i < fileNames.Length; i++)
                _downloadManager.AddFile(url + fileNames[i], localModelFolder);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            yield return _downloadManager.Download();
#else
            await _downloadManager.Download();
#endif
            ImportGraph();
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
                System.Diagnostics.Debug.Assert(File.Exists(labelFileName), String.Format("File {0} doesn't exist", labelFileName));

                if (!File.Exists(labelFileName))
                    throw new Exception("Label file does not exist!");
                _labels = File.ReadAllLines(labelFileName);
            }

            if (_model == null)
            {
                String modelFileName = _downloadManager.Files[0].LocalFile;
                System.Diagnostics.Debug.Assert(File.Exists(modelFileName), String.Format("File {0} doesn't exist", modelFileName));

                if (!File.Exists(modelFileName))
                {
                    throw new Exception("Model file does not exist!");
                }
                _model = new FlatBufferModel(modelFileName);
                if (!_model.CheckModelIdentifier())
                    throw new Exception("Model identifier check failed");
            }

            if (_interpreter == null)
            {
                _interpreter = new Interpreter(_model);

                bool isAndroid = false;
#if UNITY_ANDROID && (!UNITY_EDITOR)
                isAndroid = true;
#else
                System.Reflection.Assembly monoAndroidAssembly = Emgu.TF.Util.Toolbox.FindAssembly("Mono.Android.dll");
                if (monoAndroidAssembly != null)
                {
                    isAndroid = true;
                }
#endif
                if (isAndroid)
                {
                    //_interpreter.ModifyGraphWithDelegate(TfLiteInvoke.DefaultGpuDelegateV2);
                    //_interpreter.ModifyGraphWithDelegate(TfLiteInvoke.DefaultNnApiDelegate);
                    _interpreter.UseNNAPI(false);
                    _interpreter.SetNumThreads(4);
                }
                //_interpreter.Build(_model);

                Status allocateTensorStatus = _interpreter.AllocateTensors();
                if (allocateTensorStatus == Status.Error)
                    throw new Exception("Failed to allocate tensor");
            }

            if (_inputTensor == null)
            {
                _inputTensor = _interpreter.Inputs[0];
            }

            if (_outputTensors == null)
            {
                _outputTensors = _interpreter.Outputs;
            }
        }

        /// <summary>
        /// Get the interpreter
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


#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
        public RecognitionResult[] Recognize(Texture texture, bool flipUpsideDown = true, bool swapBR = false, float scoreThreshold = 0.0f)
        {
            int height = _inputTensor.Dims[1];
            int width = _inputTensor.Dims[2];
            NativeImageIO.ReadTensorFromTexture<byte>(
                texture,
                _inputTensor.DataPointer,
                height, width, 0.0f, 1.0f,
                flipUpsideDown,
                swapBR);

            _interpreter.Invoke();

            return ConvertResults(scoreThreshold);
        }
#else

#if __IOS__
        /// <summary>
        /// Perform Coco Ssd Mobilenet detection
        /// </summary>
        /// <param name="image">The image where we will ran the network through</param>
        /// <param name="scoreThreshold">If non-positive, will return all results. If positive, we will only return results with score larger than this value</param>
        /// <returns>The result of the detection.</returns>
        public RecognitionResult[] Recognize(UIImage image, float scoreThreshold = 0.0f)
        {
            int height = _inputTensor.Dims[1];
            int width = _inputTensor.Dims[2];

            NativeImageIO.ReadImageToTensor<byte>(image, _inputTensor.DataPointer, height, width, 0.0f, 1.0f);

            _interpreter.Invoke();

            return ConvertResults(scoreThreshold);
        }
#elif __MACOS__
        /// <summary>
        /// Perform Coco Ssd Mobilenet detection
        /// </summary>
        /// <param name="image">The image where we will ran the network through</param>
        /// <param name="scoreThreshold">If non-positive, will return all results. If positive, we will only return results with score larger than this value</param>
        /// <returns>The result of the detection.</returns>
        public RecognitionResult[] Recognize(NSImage image, float scoreThreshold = 0.0f)
        {
            int height = _inputTensor.Dims[1];
            int width = _inputTensor.Dims[2];

            NativeImageIO.ReadImageToTensor<byte>(image, _inputTensor.DataPointer, height, width, 0.0f, 1.0f);

            _interpreter.Invoke();

            return ConvertResults(scoreThreshold);
        }
#endif
        private void ReadImageFileToTensor(String imageFile)
        {
            int height = _inputTensor.Dims[1];
            int width = _inputTensor.Dims[2];

            NativeImageIO.ReadImageFileToTensor<byte>(imageFile, _inputTensor.DataPointer, height, width, 0.0f, 1.0f);
        }

        /// <summary>
        /// Perform Coco Ssd Mobilenet detection
        /// </summary>
        /// <param name="imageFile">The image file where we will ran the network through</param>
        /// <param name="scoreThreshold">If non-positive, will return all results. If positive, we will only return results with score larger than this value</param>
        /// <returns>The result of the detection.</returns>
        public RecognitionResult[] Recognize(String imageFile, float scoreThreshold = 0.0f)
        {
            ReadImageFileToTensor(imageFile);
            //Stopwatch w = Stopwatch.StartNew();
            _interpreter.Invoke();
            //w.Stop();
            return ConvertResults(scoreThreshold);
        }
#endif

        private RecognitionResult[] ConvertResults(float scoreThreshold)
        {
            float[,,] outputLocations = _interpreter.Outputs[0].JaggedData as float[,,];
            float[] classes = _interpreter.Outputs[1].Data as float[];
            float[] scores = _interpreter.Outputs[2].Data as float[];
            int numDetections = (int)Math.Round((_interpreter.Outputs[3].Data as float[])[0]);

            // SSD Mobilenet V1 Model assumes class 0 is background class
            // in label file and class labels start from 1 to number_of_classes+1,
            // while outputClasses correspond to class index from 0 to number_of_classes
            List<RecognitionResult> results = new List<RecognitionResult>();

            int labelOffset = 1;
            for (int i = 0; i < numDetections; i++)
            {
                //if (classes[i] == 0) //background class
                //    continue;

                if (scoreThreshold > 0.0f && scores[i] < scoreThreshold)
                    continue;

                RecognitionResult r = new RecognitionResult();
                r.Class = (int)Math.Round(classes[i]);
                r.Label = _labels[r.Class + labelOffset];
                r.Score = scores[i];
                float x0 = outputLocations[0, i, 1];
                float y0 = outputLocations[0, i, 0];
                float x1 = outputLocations[0, i, 3];
                float y1 = outputLocations[0, i, 2];
                r.Rectangle = new float[] { x0, y0, x1, y1 };

                results.Add(r);
            }

            return results.ToArray();
        }

        /// <summary>
        /// Coco SSD mobile net recognition result.
        /// </summary>
        public class RecognitionResult
        {
            /// <summary>
            /// Rectangles will be in the format of (x, y, width, height) in the original image coordinate
            /// </summary>
            public float[] Rectangle;
            /// <summary>
            /// The object label
            /// </summary>
            public String Label;
            /// <summary>
            /// The score of the matching
            /// </summary>
            public double Score;
            /// <summary>
            /// The class index
            /// </summary>
            public int Class;
        }

        /// <summary>
        /// Release unmanaged memory associated with this coco ssd mobilenet model.
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
