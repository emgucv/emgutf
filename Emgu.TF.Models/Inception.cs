//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
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


        private void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (OnDownloadProgressChanged != null)
                OnDownloadProgressChanged(sender, e);
        }

        /// <summary>
        /// Callback when graph download progress is changed.
        /// </summary>
        public event System.Net.DownloadProgressChangedEventHandler OnDownloadProgressChanged;

        /// <summary>
        /// Initiate the graph by checking if the graph file exit on disk, if not download the graph from internet.
        /// </summary>
        /// <param name="modelFiles">An array where the first file is the tensorflow graph and the second file are the object class labels. </param>
        /// <param name="downloadUrl">The url where the file can be downloaded</param>
        /// <param name="inputName">The input operation name. Default to "input" if not specified.</param>
        /// <param name="outputName">The output operation name. Default to "output" if not specified.</param>
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(String[] modelFiles = null, 
                String downloadUrl = null, 
                String inputName = null, 
                String outputName = null,
                String localModelFolder = "Inception" 
                )
        {
            _inputName = inputName == null ? "input" : inputName;
            _outputName = outputName == null ? "output" : outputName;

            _downloadManager.Clear();
            String url = downloadUrl == null ? "https://github.com/emgucv/models/raw/master/inception/" : downloadUrl;
            String[] fileNames = modelFiles == null ? new string[] { "tensorflow_inception_graph.pb", "imagenet_comp_graph_label_strings.txt" } : modelFiles;
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
        /// Get the graph from this inception model
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
        /// Pass the image tensor to the graph and return the probability that the object in image belongs to each of the object class.
        /// </summary>
        /// <param name="image">The image to be classified</param>
        /// <returns>The object classes, sorted by probability from high to low</returns>
        public RecognitionResult[] Recognize(Tensor image)
        {
            Operation input = _graph[_inputName];
            if (input == null)
                throw new Exception(String.Format("Could not find input operation '{0}' in the graph", _inputName));

            Operation output = _graph[_outputName];
            if (output == null)
                throw new Exception(String.Format("Could not find output operation '{0}' in the graph", _outputName));

            Tensor[] finalTensor = _session.Run(new Output[] { input }, new Tensor[] { image },
                new Output[] { output });
            float[] probability = finalTensor[0].GetData(false) as float[];
            //return probability;
            return SortResults(probability);
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
