using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Emgu.Models;
using System.Threading.Tasks;

namespace Emgu.TF.Models
{
    public class Resnet : Emgu.TF.Util.UnmanagedObject
    {
        private FileDownloadManager _downloadManager;
        private Status _status = null;
        private SessionOptions _sessionOptions = null;
        private Session _session = null;
        private String[] _labels = null;
        private String _inputName = null;
        private String _outputName = null;
        private String _savedModelDir = null;

        public Graph Graph
        {
            get
            {
                if (_session == null)
                    return null;
                return _session.Graph;
            }
        }

        public Buffer MetaGraphDefBuffer
        {
            get
            {
                if (_session == null)
                    return null;
                return _session.MetaGraphDefBuffer;
            }
        }

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
        public Resnet(Status status = null, SessionOptions sessionOptions = null)
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

        public event System.Net.DownloadProgressChangedEventHandler OnDownloadProgressChanged;

        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                String[] modelFiles = null, 
                String downloadUrl = null,
                String inputName = null,
                String outputName = null,
                String localModelFolder = "Resnet")
        {
            if (_session == null)
            {
                _inputName = inputName == null ? "serving_default_input_1" : inputName;
                _outputName = outputName == null ? "StatefulPartitionedCall" : outputName;
                
                _downloadManager.Clear();
                String url = downloadUrl == null
                    ? "https://github.com/emgucv/models/raw/master/resnet/"
                    : downloadUrl;
                String[] fileNames = modelFiles == null
                    ? new string[] { "resnet_50_classification_1.zip", "ImageNetLabels.txt" }
                    : modelFiles;
                for (int i = 0; i < fileNames.Length; i++)
                    _downloadManager.AddFile(url + fileNames[i], localModelFolder);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
                yield return _downloadManager.Download();
#else
                await _downloadManager.Download();

                System.IO.FileInfo localZipFile = new System.IO.FileInfo( _downloadManager.Files[0].LocalFile );

                _savedModelDir = System.IO.Path.Combine(localZipFile.DirectoryName, "SavedModel");
                if (!System.IO.Directory.Exists(_savedModelDir))
                {
                    System.IO.Directory.CreateDirectory(_savedModelDir);

                    System.IO.Compression.ZipFile.ExtractToDirectory(
                        localZipFile.FullName,
                        _savedModelDir);
                }

                CreateSession();
#endif
            }
        }

        private void CreateSession()
        {
            if (_session != null)
                _session.Dispose();

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            UnityEngine.Debug.Log("Importing model");
#endif

            _session = new Session(
                _savedModelDir,
                new string[] { "serve" },
                _sessionOptions
            );

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            UnityEngine.Debug.Log("Model imported");
#endif
            
            _labels = File.ReadAllLines(_downloadManager.Files[1].LocalFile);
        }

        /// <summary>
        /// Pass the image tensor to the graph and return the probability that the object in image belongs to each of the object class.
        /// </summary>
        /// <param name="image">The image to be classified</param>
        /// <returns>The object classes, sorted by probability from high to low</returns>
        public RecognitionResult[] Recognize(Tensor image)
        {
            Operation input = _session.Graph[_inputName];
            if (input == null)
                throw new Exception(String.Format("Could not find input operation '{0}' in the graph", _inputName));

            Operation output = _session.Graph[_outputName];
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
                results[i] = new RecognitionResult(_labels[ (i+1) % _labels.Length ], probabilities[i]);
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

            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }
        }
    }
}
