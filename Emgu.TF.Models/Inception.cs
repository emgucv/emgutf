//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
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

namespace Emgu.TF.Models
{
    public class Inception 
    {
        private FileDownloadManager _downloadManager;
        private Graph _graph = null;
        private Status _status = null;
        private String _inputName = null;
        private String _outputName = null;

        public Inception(Status status = null)
        {
            _status = status;
            _downloadManager = new FileDownloadManager();

            _downloadManager.OnDownloadProgressChanged += onDownloadProgressChanged;
            _downloadManager.OnDownloadCompleted += onDownloadCompleted;
        }

        private void onDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ImportGraph();
            if (OnDownloadCompleted != null)
            {
                OnDownloadCompleted(sender, e);
            }
        }

        private void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (OnDownloadProgressChanged != null)
                OnDownloadProgressChanged(sender, e);
        }

        public event System.Net.DownloadProgressChangedEventHandler OnDownloadProgressChanged;
        public event System.ComponentModel.AsyncCompletedEventHandler OnDownloadCompleted;


        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            void
#endif
            Init(String[] modelFiles = null, String downloadUrl = null, String inputName = null, String outputName = null)
        {
            _inputName = inputName == null ? "input" : inputName;
            _outputName = outputName == null ? "output" : outputName;

            _downloadManager.Clear();
            String url = downloadUrl == null ? "https://github.com/emgucv/models/raw/master/inception/" : downloadUrl;
            String[] fileNames = modelFiles == null ? new string[] { "tensorflow_inception_graph.pb", "imagenet_comp_graph_label_strings.txt" } : modelFiles;
            for (int i = 0; i < fileNames.Length; i++)
                _downloadManager.AddFile(url + fileNames[i]);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            yield return _downloadManager.Download();
#else
            _downloadManager.Download();
#endif
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
            _graph = new Graph();
            String localFileName = _downloadManager.Files[0].LocalFile;
            byte[] model = File.ReadAllBytes(localFileName);

            Buffer modelBuffer = Buffer.FromString(model);

            using (ImportGraphDefOptions options = new ImportGraphDefOptions())
                _graph.ImportGraphDef(modelBuffer, options, _status);
        }

        public String[] Labels
        {
            get
            {
                return File.ReadAllLines(_downloadManager.Files[1].LocalFile);
            }
        }

        public float[] Recognize(Tensor image)
        {
            Session inceptionSession = new Session(_graph);
            Tensor[] finalTensor = inceptionSession.Run(new Output[] { _graph[_inputName] }, new Tensor[] { image },
                new Output[] { _graph[_outputName] });
            float[] probability = finalTensor[0].GetData(false) as float[];
            return probability;
        }

        public RecognitionResult MostLikely(Tensor image)
        {
            float[] probability = Recognize(image);

            RecognitionResult result = new RecognitionResult();
            result.Label = String.Empty;

            if (probability != null)
            {
                String[] labels = Labels;
                float maxVal = 0;
                int maxIdx = 0;
                for (int i = 0; i < probability.Length; i++)
                {
                    if (probability[i] > maxVal)
                    {
                        maxVal = probability[i];
                        maxIdx = i;
                    }
                }
                result.Label = labels[maxIdx];
                result.Probability = maxVal;
            }
            return result;
        }

        public class RecognitionResult
        {
            public String Label;
            public double Probability;
        }
    }
}
