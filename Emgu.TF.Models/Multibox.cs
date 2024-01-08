//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using Emgu.Models;
using System.Net;
using System.ComponentModel;
using System.Globalization;
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
using UnityEngine;
#else
using System.Drawing;
using System.Threading.Tasks;

#endif

namespace Emgu.TF.Models
{
    /// <summary>
    /// Multibox graph
    /// </summary>
    public class MultiboxGraph : Emgu.TF.Util.UnmanagedObject
    {
        private FileDownloadManager _downloadManager;
        private Graph _graph = null;
        private SessionOptions _sessionOptions = null;
        private Session _session = null;
        private Status _status = null;
        private float[] _boxPriors = null;

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
        /// Create a new multibox graph  
        /// </summary>
        /// <param name="status">The status object that can be used to keep track of error or exceptions</param>
        /// <param name="sessionOptions">The options for running the tensorflow session.</param>
        public MultiboxGraph(Status status = null, SessionOptions sessionOptions = null)
        {
            _status = status;
            _sessionOptions = sessionOptions;
            _downloadManager = new FileDownloadManager();

            _downloadManager.OnDownloadProgressChanged += onDownloadProgressChanged;
        }

        /// <summary>
        /// Callback when the model download progress is changed.
        /// </summary>
        public event FileDownloadManager.DownloadProgressChangedEventHandler OnDownloadProgressChanged;

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
                String localModelFolder = "Multibox")
        {
            DownloadableFile[] downloadableFiles;
            if (modelFiles == null)
            {
                downloadableFiles = new DownloadableFile[2];
            }
            else
            {
                String url = downloadUrl ?? "https://github.com/emgucv/models/raw/master/mobile_multibox_v1a/";
                String[] fileNames = modelFiles ?? new string[] { "multibox_model.pb", "multibox_location_priors.txt" };
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
        /// Initiate the graph by checking if the model file exist locally, if not download the graph from internet.
        /// </summary>
        /// <param name="modelFile">The tensorflow graph.</param>
        /// <param name="labelFile">the object class labels.</param>
        /// <param name="localModelFolder">The local folder to store the model</param>
        public
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                DownloadableFile modelFile = null,
                DownloadableFile labelFile = null, 
                String localModelFolder = "Multibox")
        {
            if (_graph == null)
            {
                //String defaultLocalSubfolder = "Multibox";
                if (modelFile == null)
                {
                    modelFile = new DownloadableFile(
                        "https://github.com/emgucv/models/raw/master/mobile_multibox_v1a/multibox_model.pb",
                        localModelFolder,
                        "D1466DF5497E722E4A49E3839F667F07C579DD4C049258018E5F8EE9E01943A7"
                    );
                }

                if (labelFile == null)
                {
                    labelFile = new DownloadableFile(
                        "https://github.com/emgucv/models/raw/master/mobile_multibox_v1a/multibox_location_priors.txt",
                        localModelFolder,
                        "8742979FBAAAAB73CDDE4FAB55126AD78C6D9F84F310D8D51566BDF3F48F1E65"
                    );
                }

                _downloadManager.Clear();
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

        private void onDownloadProgressChanged(long? totalBytesToReceive, long bytesReceived, double? progressPercentage)
        {
            OnDownloadProgressChanged?.Invoke(totalBytesToReceive, bytesReceived, progressPercentage);
        }

        /// <summary>
        /// Return true if the graph has been imported
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
            _graph?.Dispose();
            _graph = new Graph();
            String localFileName = _downloadManager.Files[0].LocalFile;

            byte[] model = File.ReadAllBytes(localFileName);

            if (model.Length == 0)
                throw new FileNotFoundException(String.Format("Unable to load file {0}", localFileName));
            Buffer modelBuffer = Buffer.FromString(model);

            using (ImportGraphDefOptions options = new ImportGraphDefOptions())
                _graph.ImportGraphDef(modelBuffer, options, _status);

            #region Sorted the detected region by confident
            Tensor shapeIdxTensor = new Tensor(1);
            Operation shapeIdx = _graph.Const(shapeIdxTensor, DataType.Int32, opName: "shape_idx");
            Operation outputScores = _graph["output_scores/Reshape"];
            Operation shape = _graph.Shape(outputScores, null, "output_scores_shape");
            Operation kValue = _graph.Gather(shape, shapeIdx, true, "get_output_scores_k");
            Operation topK = _graph.TopKV2(outputScores, kValue, opName: "top_k_output_scores");
            #endregion

            _session?.Dispose();

            _session = new Session(_graph, _sessionOptions);

            _boxPriors = ReadBoxPriors(_downloadManager.Files[1].LocalFile);
        }

        /// <summary>
        /// Detect objects from the image.
        /// </summary>
        /// <param name="image">The image tensor.</param>
        /// <returns>The detection result</returns>
        public Result[] Detect(Tensor image)
        {
            if (_graph == null)
            {
                throw new NullReferenceException("The multibox graph has not been initialized. Please call the Init function first.");
            }
            Tensor[] finalTensor = _session.Run(new Output[] { _graph["ResizeBilinear"] }, new Tensor[] { image },
                new Output[]
                {
                    _graph["output_scores/Reshape"], 
                    _graph["output_locations/Reshape"], 
                    new Output(_graph["top_k_output_scores"],0),
                    new Output(_graph["top_k_output_scores"],1)
                });

            float[] encodedScores = finalTensor[2].Flat<float>();
            float[] encodedLocations = finalTensor[1].Flat<float>();

            int[] indices = finalTensor[3].Flat<int>();
            float[] scores = DecodeScoresEncoding(encodedScores);
            Result[] results = new Result[indices.Length];
            float[][] locations = MultiboxGraph.DecodeLocationsEncoding(encodedLocations, _boxPriors);
            for (int i = 0; i < indices.Length; i++)
            {
                results[i] = new Result();
                results[i].Scores = scores[i];
                results[i].DecodedLocations = locations[indices[i]];
            }

            return results;

        }

        /// <summary>
        /// A detection result;
        /// </summary>
        public class Result
        {
            /// <summary>
            /// The score for the detection
            /// </summary>
            public float Scores;

            /// <summary>
            /// The location for the detection
            /// </summary>
            public float[] DecodedLocations;
        }

        /// <summary>
        /// Read the box priors
        /// </summary>
        /// <param name="fileName">The name of the box priors file</param>
        /// <returns>The floating point box priors value</returns>
        public static float[] ReadBoxPriors(String fileName)
        {
            List<float> priors = new List<float>();
            foreach (String line in File.ReadAllLines(fileName))
            {
                String[] tokens = line.Split(',');
                foreach (var token in tokens)
                {
                    float result = 0;
                    //if (float.TryParse(token.Trim(), out result))
                    if (float.TryParse(token.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                        priors.Add(result);
                }
            }
            return priors.ToArray();
        }

        /// <summary>
        /// Decode the location encoding
        /// </summary>
        /// <param name="locationEncoding">The location encoding</param>
        /// <param name="boxPriors">The box priors</param>
        /// <returns>The list of locations, each location is 4 floating value.</returns>
        public static float[][] DecodeLocationsEncoding(float[] locationEncoding, float[] boxPriors)
        {
            int numLocations = locationEncoding.Length / 4;

            float[][] locations = new float[numLocations][];
            bool nonZero = false;
            for (int i = 0; i < numLocations; ++i)
            {
                locations[i] = new float[4];
                for (int j = 0; j < 4; ++j)
                {
                    float currEncoding = locationEncoding[4 * i + j];
                    nonZero = nonZero || currEncoding != 0.0f;

                    float mean = boxPriors[i * 8 + j * 2];
                    float stdDev = boxPriors[i * 8 + j * 2 + 1];
                    float currentLocation = currEncoding * stdDev + mean;
                    currentLocation = Math.Max(currentLocation, 0.0f);
                    currentLocation = Math.Min(currentLocation, 1.0f);
                    locations[i][j] = currentLocation;
                }
            }

            if (!nonZero)
            {
                throw new Exception("No non-zero encodings; check log for inference errors.");
            }
            return locations;
        }

        /// <summary>
        /// Decode the scores
        /// </summary>
        /// <param name="scoresEncoding">The scores encoding</param>
        /// <returns>The scores</returns>
        public static float[] DecodeScoresEncoding(float[] scoresEncoding)
        {
            float[] scores = new float[scoresEncoding.Length];
            for (int i = 0; i < scoresEncoding.Length; ++i)
            {
                scores[i] = 1 / ((float)(1 + Math.Exp(-scoresEncoding[i])));
            }
            return scores;
        }

        /// <summary>
        /// Convert and filter the multibox results to annotations
        /// </summary>
        /// <param name="results">Multibox detection result</param>
        /// <param name="scoreThreshold">The score threshold</param>
        /// <returns>The Annotation to be drawn.</returns>
        public static Annotation[] FilterResults(MultiboxGraph.Result[] results, float scoreThreshold)
        {
            List<Annotation> goodResults = new List<Annotation>();
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].Scores > scoreThreshold)
                {
                    Annotation r = new Annotation();
                    r.Rectangle = results[i].DecodedLocations;
                    r.Label = String.Empty;
                    //r.Label = String.Format("{0:0.00}%", results[i].Scores * 100);
                    goodResults.Add(r);
                }
            }
            return goodResults.ToArray();
        }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
        public static Rect[] ScaleLocation(float[] location, int imageWidth, int imageHeight, bool flipUpSideDown = false)
        {
            Rect[] scaledLocation = new Rect[location.Length / 4];
            for (int i = 0; i < scaledLocation.Length; i++)
            {
                float left = location[i * 4] * imageWidth;
                float top = location[i * 4 + 1] * imageHeight;
                float right = location[i * 4 + 2] * imageWidth;
                float bottom = location[i * 4 + 3] * imageHeight;

                scaledLocation[i] = new Rect(left, top, right - left, bottom - top);
				if (flipUpSideDown)
				{
					Rect rFlipped = scaledLocation[i];
					rFlipped.y = imageHeight - scaledLocation[i].y;
					rFlipped.height = -scaledLocation[i].height;
					scaledLocation[i] = rFlipped;
				}
            }
            return scaledLocation;
        }

        public static void DrawResults(Texture2D image, MultiboxGraph.Result[] results, float scoreThreshold, bool flipUpSideDown = false)
        {
            Annotation[] annotations = FilterResults(results, scoreThreshold);
            
            Color color = new Color(1.0f, 0, 0);//Set color to red
            for (int i = 0; i < annotations.Length; i++)
            {
                Rect[] rects = ScaleLocation(annotations[i].Rectangle, image.width, image.height, flipUpSideDown);
                
                foreach (Rect r in rects)
                {
                    NativeImageIO.DrawRect(image, r, color);
                }
            }
            image.Apply();
            //GUI.color = Color.white;//Reset color to white
            /*
            Android.Graphics.Paint p = new Android.Graphics.Paint();
            p.SetStyle(Paint.Style.Stroke);
            p.AntiAlias = true;
            p.Color = Android.Graphics.Color.Red;
            Canvas c = new Canvas(bmp);


            for (int i = 0; i < result.Scores.Length; i++)
            {
                if (result.Scores[i] > scoreThreshold)
                {
                    Rectangle rect = locations[result.Indices[i]];
                    Android.Graphics.Rect r = new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
                    c.DrawRect(r, p);
                }
            }*/
        }

#endif

        /// <summary>
        /// Release the memory associated with the Multibox
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
