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
using System.Net;
using System.Threading.Tasks;

namespace Emgu.TF.Models
{
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

        public event System.Net.DownloadProgressChangedEventHandler OnDownloadProgressChanged;
        
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(String[] modelFiles = null, String downloadUrl = null, String localModelFolder = "MaskRcnn")
        {
            if (_graph == null)
            {
                _downloadManager.Clear();
                String url = downloadUrl == null
                    ? "https://github.com/emgucv/models/raw/master/mask_rcnn_inception_v2_coco_2018_01_28/"
                    : downloadUrl;
                String[] fileNames = modelFiles == null
                    ? new string[] {"frozen_inference_graph.pb", "coco-labels-paper.txt"}
                    : modelFiles;
                for (int i = 0; i < fileNames.Length; i++)
                    _downloadManager.AddFile(url + fileNames[i], localModelFolder);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
                yield return _downloadManager.Download();
#else
                await _downloadManager.Download();
                ImportGraph();
#endif
            }
        }

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
            float[,,] detectinBoxes = finalTensor[0].JaggedData as float[,,];
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
                r.Region = new float[] { detectinBoxes[0, i, 0], detectinBoxes[0, i, 1], detectinBoxes[0, i, 2], detectinBoxes[0, i, 3] };
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

        public class RecognitionResult
        {
            public int Class;
            public String Label;
            public double Probability;
            public float[] Region;
            public float[,] Mask;
        }
        
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
