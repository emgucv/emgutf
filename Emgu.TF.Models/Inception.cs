﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Emgu.TF;
using Emgu.Models;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Emgu.TF.Models
{
    /// <summary>
    /// Inception image classification 
    /// </summary>
    public class Inception : Emgu.TF.Util.UnmanagedObject
    {
        private FileDownloadManager _downloadManager;
        private Graph _graph = null;
        private SessionOptions _sessionOptions = null;
        private Session _session = null;
        private Status _status = null;
        private String _inputName = null;
        private String _outputName = null;
        private String[] _labels = null;

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
        /// Create a new inception object 
        /// </summary>
        /// <param name="status">The status object that can be used to keep track of error or exceptions</param>
        /// <param name="sessionOptions">The options for running the tensorflow session.</param>
        public Inception(Status status = null, SessionOptions sessionOptions = null)
        {
            _status = status;
            _sessionOptions = sessionOptions;
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
        /// Initiate the graph by checking if the model file exist locally, if not download the graph from internet.
        /// </summary>
        /// <param name="modelFile">The tensorflow graph.</param>
        /// <param name="labelFile">the object class labels.</param>
        /// <param name="inputName">The input operation name. Default to "input" if not specified.</param>
        /// <param name="outputName">The output operation name. Default to "output" if not specified.</param>
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(DownloadableFile modelFile = null,
                DownloadableFile labelFile = null,
                String inputName = null,
                String outputName = null
            )
        {
            if (_graph == null)
            {
                _inputName = inputName == null ? "input" : inputName;
                _outputName = outputName == null ? "output" : outputName;

                _downloadManager.Clear();

                String defaultLocalSubfolder = "Inception";
                if (modelFile == null)
                {
                    modelFile = new DownloadableFile(
                        "https://github.com/emgucv/models/raw/master/inception/tensorflow_inception_graph.pb",
                        defaultLocalSubfolder,
                        "A39B08B826C9D5A5532FF424C03A3A11A202967544E389ACA4B06C2BD8AEF63F"
                    );
                }

                if (labelFile == null)
                {
                    labelFile = new DownloadableFile(
                        "https://github.com/emgucv/models/raw/master/inception/imagenet_comp_graph_label_strings.txt",
                        defaultLocalSubfolder,
                        "DA2A31ECFE9F212AE8DD07379B11A74CB2D7A110EBA12C5FC8C862A65B8E6606"
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
                    System.Diagnostics.Trace.WriteLine("Failed to download files");

            }
        }


        /// <summary>
        /// Initiate the graph by checking if the model file exist locally, if not download the graph from internet.
        /// </summary>
        /// <param name="modelFiles">An array where the first file is the tensorflow graph and the second file is the object class labels. </param>
        /// <param name="downloadUrl">The url where the file can be downloaded</param>
        /// <param name="inputName">The input operation name. Default to "input" if not specified.</param>
        /// <param name="outputName">The output operation name. Default to "output" if not specified.</param>
        /// <param name="localModelFolder">The local folder to store the model</param>
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(String[] modelFiles, 
                String downloadUrl, 
                String inputName, 
                String outputName,
                String localModelFolder = "Inception" 
                )
        {
            DownloadableFile[] downloadableFiles;
            if (modelFiles == null)
            {
                downloadableFiles = new DownloadableFile[2];
            }
            else
            {
                String url = downloadUrl == null ? "https://github.com/emgucv/models/raw/master/inception/" : downloadUrl;
                String[] fileNames = modelFiles == null ? new string[] { "tensorflow_inception_graph.pb", "imagenet_comp_graph_label_strings.txt" } : modelFiles;
                downloadableFiles = new DownloadableFile[fileNames.Length];
                for (int i = 0; i < fileNames.Length; i++)
                    downloadableFiles[i] = new DownloadableFile(url + fileNames[i], localModelFolder);
            }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            return Init(downloadableFiles[0], downloadableFiles[1], inputName, outputName);
#else
            await Init(downloadableFiles[0], downloadableFiles[1], inputName, outputName);
#endif

        }

        /// <summary>
        /// Return true if the graph has been imported.
        /// </summary>
        public bool Imported
        {
            get
            {
                return _graph != null;
            }
        }

        private void ImportGraph()
        {
            if (_graph != null)
                _graph.Dispose();

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            UnityEngine.Debug.Log("Reading model definition");
#endif
            _graph = new Graph();
            String localFileName = _downloadManager.Files[0].LocalFile;
            byte[] model = File.ReadAllBytes(localFileName);
            if (model.Length == 0)
                throw new FileNotFoundException(String.Format("Unable to load file {0}", localFileName));
            Buffer modelBuffer = Buffer.FromString(model);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            UnityEngine.Debug.Log("Importing model");
#endif
            using (ImportGraphDefOptions options = new ImportGraphDefOptions())
                _graph.ImportGraphDef(modelBuffer, options, _status);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            UnityEngine.Debug.Log("Model imported");
#endif
            _session = new Session(_graph, _sessionOptions, _status);
            _labels = File.ReadAllLines(_downloadManager.Files[1].LocalFile);
        }

        /// <summary>
        /// Get the TF graph from this inception model
        /// </summary>
        public Graph Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Get the labels of the object classes
        /// </summary>
        public String[] Labels
        {
            get { return _labels; }
        }

        /// <summary>
        /// Get the Tensorflow session
        /// </summary>
        public Session Session
        {
            get { return _session; }
        }

        /// <summary>
        /// Pass the image tensor to the graph and return the probability that the object in image belongs to each of the object class.
        /// </summary>
        /// <param name="imageTensor">The tensor that contains the images to be classified</param>
        /// <returns>The object classes, sorted by probability from high to low</returns>
        public RecognitionResult[][] Recognize(Tensor imageTensor)
        {
            Operation input = _graph[_inputName];
            if (input == null)
                throw new Exception(String.Format("Could not find input operation '{0}' in the graph", _inputName));

            Operation output = _graph[_outputName];
            if (output == null)
                throw new Exception(String.Format("Could not find output operation '{0}' in the graph", _outputName));

            Tensor[] finalTensor = _session.Run(new Output[] { input }, new Tensor[] { imageTensor },
                new Output[] { output });
            float[,] probability = finalTensor[0].GetData(true) as float[,];
            int imageCount = probability.GetLength(0);
            int probLength = probability.GetLength(1);
            RecognitionResult[][] results = new RecognitionResult[imageCount][];
            for (int i = 0; i < imageCount; i++)
            {
                float[] p = new float[probLength];
                for (int j = 0; j < p.Length; j++)
                    p[j] = probability[i, j];
                results[i] = SortResults(p);
            }
            return results;
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

            if (_labels.Length != probabilities.Length)
                Trace.TraceWarning("Length of labels does not equals to the length of probabilities");

            RecognitionResult[] results = new RecognitionResult[Math.Min(_labels.Length, probabilities.Length)];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new RecognitionResult(_labels[i], probabilities[i]);
            }
            Array.Sort<RecognitionResult>(results, new Comparison<RecognitionResult>((a, b) => -a.Probability.CompareTo(b.Probability)));
            return results;
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
        /// Release the memory associated with this inception graph
        /// </summary>
        protected override void DisposeObject()
        {
            if (_graph != null)
            {
                _graph.Dispose();
                _graph = null;
            }

            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }
        }
    }
}
