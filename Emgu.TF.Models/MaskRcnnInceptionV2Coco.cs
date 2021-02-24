//----------------------------------------------------------------------------
//  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Emgu.TF;
using Emgu.Models;
using System.IO;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Emgu.TF.Models
{
    /// <summary>
    /// Mask Rcnn Inception image classification 
    /// </summary>
    public class MaskRcnnInceptionV2Coco : Emgu.TF.Util.UnmanagedObject
    {
        private FileDownloadManager _downloadManager;
        private Graph _graph = null;
        private SessionOptions _sessionOptions = null;
        private Session _session = null;
        private Status _status = null;
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
        /// Create a new mask rcnn inception object 
        /// </summary>
        /// <param name="status">The status object that can be used to keep track of error or exceptions</param>
        /// <param name="sessionOptions">The options for running the tensorflow session.</param>
        public MaskRcnnInceptionV2Coco(Status status = null, SessionOptions sessionOptions = null)
        {
            _status = status;
            _sessionOptions = sessionOptions;
            _downloadManager = new FileDownloadManager();

            _downloadManager.OnDownloadProgressChanged += onDownloadProgressChanged;
            //_downloadManager.OnDownloadCompleted += onDownloadCompleted;
        }

        private void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (OnDownloadProgressChanged != null)
                OnDownloadProgressChanged(sender, e);
        }

        /// <summary>
        /// Callback when model download progress is changed.
        /// </summary>
        public event System.Net.DownloadProgressChangedEventHandler OnDownloadProgressChanged;

        /// <summary>
        /// Initiate the graph by checking if the model file exist locally, if not download the graph from internet.
        /// </summary>
        /// <param name="modelFile">The tensorflow graph.</param>
        /// <param name="labelFile">the object class labels.</param>
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(DownloadableFile modelFile = null,
                DownloadableFile labelFile = null
            )
        {
            if (_graph == null)
            {
                _downloadManager.Clear();

                String defaultLocalSubfolder = "MaskRcnn";
                if (modelFile == null)
                {
                    modelFile = new DownloadableFile(
                        "https://github.com/emgucv/models/raw/master/mask_rcnn_inception_v2_coco_2018_01_28/frozen_inference_graph.pb",
                        defaultLocalSubfolder,
                        "AC9B51CDE227B24D20030042E6C1E29AF75668F509E51AA84ED686787CCCC309"
                    );
                }

                if (labelFile == null)
                {
                    labelFile = new DownloadableFile(
                        "https://github.com/emgucv/models/raw/master/mask_rcnn_inception_v2_coco_2018_01_28/coco-labels-paper.txt",
                        defaultLocalSubfolder,
                        "8925173E1B0AABFAEFDA27DE2BB908233BB8FB6E7582323D72988E4BE15A5F0B"
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
                    System.Diagnostics.Trace.WriteLine("Failed to download files");
                }
            }
        }

        /// <summary>
        /// Initiate the graph by checking if the model file exist locally, if not download the graph from internet.
        /// </summary>
        /// <param name="modelFiles">An array where the first file is the tensorflow graph and the second file is the object class labels. </param>
        /// <param name="downloadUrl">The url where the file can be downloaded</param>
        /// <param name="localModelFolder">The local folder to store the model</param>
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                String[] modelFiles, 
                String downloadUrl, 
                String localModelFolder = "MaskRcnn")
        {

            DownloadableFile[] downloadableFiles;
            if (modelFiles == null)
            {
                downloadableFiles = new DownloadableFile[2];
            }
            else
            {
                String url = downloadUrl == null ? "https://github.com/emgucv/models/raw/master/mask_rcnn_inception_v2_coco_2018_01_28/" : downloadUrl;
                String[] fileNames = modelFiles == null ? new string[] { "frozen_inference_graph.pb", "coco-labels-paper.txt" } : modelFiles;
                downloadableFiles = new DownloadableFile[fileNames.Length];
                for (int i = 0; i < fileNames.Length; i++)
                    downloadableFiles[i] = new DownloadableFile(url + fileNames[i], localModelFolder);
            }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            return Init(downloadableFiles[0], downloadableFiles[1]);
#else
            await Init(downloadableFiles[0], downloadableFiles[1]);
#endif

        }
    

        /// <summary>
        /// Return true if the model has been imported
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
        /// Get the TF graph from this mask rcnn inception model
        /// </summary>
        public Graph Graph
        {
            get
            {
                return _graph;
            }
        }

        public String[] Labels
        {
            get { return _labels; }
        }

        public RecognitionResult[] Recognize(Tensor image)
        {
            Output input = _graph["image_tensor"];
            Output[] outputs = new Output[] { _graph["detection_boxes"], _graph["detection_scores"], _graph["detection_classes"], _graph["num_detections"], _graph["detection_masks"] };

            Tensor[] finalTensor = _session.Run(new Output[] { input }, new Tensor[] { image }, outputs);
            int numDetections = (int) (finalTensor[3].Data as float[])[0];
            float[,,] detectionBoxes = finalTensor[0].JaggedData as float[,,];
            float[,] detectionScores = finalTensor[1].JaggedData as float[,];
            float[,] detectionClasses = finalTensor[2].JaggedData as float[,];
            float[,,,] detectionMask = finalTensor[4].JaggedData as float[,,,]; 
            List<RecognitionResult> results = new List<RecognitionResult>();
            int numberOfClasses = detectionScores.GetLength(1);
            for (int i = 0; i < numDetections; i++)
            {
                RecognitionResult r = new RecognitionResult();
                r.Class = (int) detectionClasses[0,i];
                r.Label = Labels[r.Class - 1];
                r.Probability = detectionScores[0,i];
                r.Region = new float[] { detectionBoxes[0, i, 0], detectionBoxes[0, i, 1], detectionBoxes[0, i, 2], detectionBoxes[0, i, 3] };
                results.Add(r);

                float[,] m = new float[detectionMask.GetLength(2), detectionMask.GetLength(3)];
                for (int j = 0; j < m.GetLength(0); j++)
                    for (int k = 0; k < m.GetLength(1); k++)
                    {
                        m[j, k] = detectionMask[0, i, j, k];
                    }
                r.Mask = m;
            }
            return results.ToArray();
        }

        /// <summary>
        /// The recognition result 
        /// </summary>
        public class RecognitionResult
        {
            /// <summary>
            /// The class number
            /// </summary>
            public int Class;
            /// <summary>
            /// The label
            /// </summary>
            public String Label;
            /// <summary>
            /// The probability
            /// </summary>
            public double Probability;
            /// <summary>
            /// The region
            /// </summary>
            public float[] Region;
            /// <summary>
            /// The mask
            /// </summary>
            public float[,] Mask;
        }

        /// <summary>
        /// Release the memory associated with the Mask rcnn inception model
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
