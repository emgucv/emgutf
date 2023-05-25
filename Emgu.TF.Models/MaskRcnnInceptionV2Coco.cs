//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
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
        //private Graph _graph = null;
        private SessionOptions _sessionOptions = null;
        private Session _session = null;
        private Status _status = null;
        private String[] _labels = null;
        private String _savedModelDir = null;


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
            if (_session == null)
            {
                _downloadManager.Clear();

                String defaultLocalSubfolder = "MaskRcnn";
                if (modelFile == null)
                {
                    modelFile = new DownloadableFile(
                        "https://emgu-public.s3.amazonaws.com/mask_rcnn_inception_v2_coco_saved_model/mask_rcnn_inception_v2_coco_saved_model.zip",
                        defaultLocalSubfolder,
                        "4F043142473125E3758BCF9042AFF6F14C0C04A5D2273F20D1831A337176DAAC"
                    );
                }

                if (labelFile == null)
                {
                    labelFile = new DownloadableFile(
                        "https://emgu-public.s3.amazonaws.com/mask_rcnn_inception_v2_coco_saved_model/coco-labels-paper.txt",
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
                {
                    System.IO.FileInfo localZipFile = new System.IO.FileInfo(_downloadManager.Files[0].LocalFile);

                    _savedModelDir = System.IO.Path.Combine(localZipFile.DirectoryName, "SavedModel");
                    if (!System.IO.Directory.Exists(_savedModelDir))
                    {
                        System.IO.Directory.CreateDirectory(_savedModelDir);

                        System.IO.Compression.ZipFile.ExtractToDirectory(
                            localZipFile.FullName,
                            _savedModelDir);
                    }
                    
                    CreateSession();
                }
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
                String url = downloadUrl == null ? "https://emgu-public.s3.amazonaws.com/mask_rcnn_inception_v2_coco_saved_model/" : downloadUrl;
                String[] fileNames = modelFiles == null ? new string[] { "mask_rcnn_inception_v2_coco_saved_model.zip", "coco-labels-paper.txt" } : modelFiles;
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
                return _session != null;
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
            //_session = new Session(_graph, _sessionOptions, _status);
            _labels = File.ReadAllLines(_downloadManager.Files[1].LocalFile);
        }

        /// <summary>
        /// Get the TF graph from this mask rcnn inception model
        /// </summary>
        public Graph Graph
        {
            get
            {
                if (_session == null)
                    return null;
                return _session.Graph;
            }
        }

        /// <summary>
        /// Get the labels of the Coco dataset.
        /// </summary>
        public String[] Labels
        {
            get { return _labels; }
        }

        /// <summary>
        /// Recognized the objects from the tensor.
        /// </summary>
        /// <param name="image">The image tensor</param>
        /// <returns>The recognition result.</returns>
        public RecognitionResult[][] Recognize(Tensor image)
        {
            // Use this from command line to find out the input tensor name:
            // saved_model_cli show --dir SavedModel --tag_set serve --signature_def serving_default
            //   inputs['input_tensor'] tensor_info:
            //   dtype: DT_UINT8
            //   shape: (1, -1, -1, 3)
            //   name: serving_default_input_tensor: 0
            Output input = _session.Graph["serving_default_input_tensor"]; 

            Output[] outputs = new Output[]
            {
                //_session.Graph["detection_boxes"], 
                new Output(_session.Graph["StatefulPartitionedCall"], 4),

                //_session.Graph["detection_scores"], 
                new Output(_session.Graph["StatefulPartitionedCall"], 8),

                //_session.Graph["detection_classes"],
                new Output(_session.Graph["StatefulPartitionedCall"], 5),

                //_session.Graph["num_detections"],
                new Output(_session.Graph["StatefulPartitionedCall"], 12),

                //_session.Graph["detection_masks"]
                new Output(_session.Graph["StatefulPartitionedCall"], 6),
            };

            Tensor[] finalTensor = _session.Run(new Output[] { input }, new Tensor[] { image }, outputs);
            
            float[,,] detectionBoxes = finalTensor[0].JaggedData as float[,,];
            float[,] detectionScores = finalTensor[1].JaggedData as float[,];
            float[,] detectionClasses = finalTensor[2].JaggedData as float[,];
            float[,,,] detectionMask = finalTensor[4].JaggedData as float[,,,];

            int imageCount = detectionScores.GetLength(0);
            RecognitionResult[][] allResults = new RecognitionResult[imageCount][];
            for (int idx = 0; idx < imageCount; idx++)
            {

                //int numberOfClasses = detectionScores.GetLength(1);
                List<RecognitionResult> results = new List<RecognitionResult>();
                int numDetections = (int) (finalTensor[3].Data as float[])[0];
                for (int i = 0; i < numDetections; i++)
                {
                    RecognitionResult r = new RecognitionResult();
                    r.Class = (int) detectionClasses[0, i];
                    r.Label = Labels[r.Class - 1];
                    r.Probability = detectionScores[0, i];
                    r.Region = new float[]
                    {
                        detectionBoxes[0, i, 0], detectionBoxes[0, i, 1], detectionBoxes[0, i, 2],
                        detectionBoxes[0, i, 3]
                    };
                    results.Add(r);

                    float[,] m = new float[detectionMask.GetLength(2), detectionMask.GetLength(3)];
                    for (int j = 0; j < m.GetLength(0); j++)
                    for (int k = 0; k < m.GetLength(1); k++)
                    {
                        m[j, k] = detectionMask[0, i, j, k];
                    }

                    r.Mask = m;
                }

                allResults[idx] = results.ToArray();
            }

            return allResults;
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
            /*
            if (_graph != null)
            {
                _graph.Dispose();
                _graph = null;
            }*/

            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }
        }
    }
}
