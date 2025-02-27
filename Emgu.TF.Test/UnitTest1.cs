﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
#if VS_TEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#else
using NUnit.Framework;
#endif
using Emgu.TF;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.Models;
using Emgu.TF.Models;
using Tensorflow;
using Google.Protobuf;
using Google.Protobuf.Collections;

namespace Emgu.TF.Test
{
    [TestFixture]
    public class UnitTest1
    {
        [TestAttribute]
        public void TestGetVersion()
        {
            String version = Emgu.TF.TfInvoke.Version;
            Trace.WriteLine(String.Format("Tensorflow version: {0}", version));
        }

        /*
        [TestMethod]
        public void TestLoadLargeGraph()
        {
            Inception inceptionGraph = new Inception(null, new string[] { "optimized_graph.pb", "output_labels.txt" }, "https://github.com/emgucv/models/raw/master/inception_flower_retrain/", "Mul", "final_result");
        }*/

        [TestAttribute]
        public void TestTString()
        {
            byte[] data = File.ReadAllBytes("grace_hopper.jpg");
            TString s = new TString(data);
            byte[] data2 = s.Data;
            EmguAssert.AreEqual(data.Length, data2.Length);
            for (int i = 0; i < data.Length; i++)
            {
                EmguAssert.AreEqual(data[i], data2[i]);
            }
        }

        [TestAttribute]
        public async Task TestInceptionBatch()
        {
            //using (Tensor imageTensor = ImageIO.ReadTensorFromImageFile<float>("grace_hopper.jpg", 224, 224, 128.0f, 1.0f))
            using (Tensor imageTensor = ImageIO.ReadTensorFromImageFiles<float>(
                new String[] { "grace_hopper.jpg", "grace_hopper.jpg" }, 
                224, 
                224, 
                128.0f, 
                1.0f))
            using (Inception inceptionGraph = new Inception())
            {
                await inceptionGraph.Init();

                Inception.RecognitionResult[][] results = inceptionGraph.Recognize(imageTensor);

                Trace.WriteLine(String.Format("Object is {0} with {1}% probability", results[0][0].Label, results[0][0].Probability * 100));

            }
        }

        [TestAttribute]
        public async Task TestInception()
        {
            using (Tensor imageTensor = ImageIO.ReadTensorFromImageFile<float>("grace_hopper.jpg", 224, 224, 128.0f, 1.0f))
            using (Inception inceptionGraph = new Inception())
            {
                await inceptionGraph.Init();

                HashSet<string> opNames = new HashSet<string>();
                HashSet<string> couldBeInputs = new HashSet<string>();
                HashSet<string> couldBeOutputs = new HashSet<string>();
                foreach (Operation op in inceptionGraph.Graph)
                {

                    String name = op.Name;
                    opNames.Add(name);

                    if (op.NumInputs == 0 && op.OpType.Equals("Placeholder"))
                    {
                        couldBeInputs.Add(op.Name);
                        AttrMetadata dtypeMeta = op.GetAttrMetadata("dtype");
                        AttrMetadata shapeMeta = op.GetAttrMetadata("shape");
                        DataType type = op.GetAttrType("dtype");
                        Int64[] shape = op.GetAttrShape("shape");
                        Buffer valueBuffer = op.GetAttrValueProto("shape");
                        Buffer shapeBuffer = op.GetAttrTensorShapeProto("shape");
                        Tensorflow.TensorShapeProto shapeProto =
                            Tensorflow.TensorShapeProto.Parser.ParseFrom(shapeBuffer.Data);
                    }

                    if (op.OpType.Equals("Const"))
                    {
                        AttrMetadata dtypeMeta = op.GetAttrMetadata("dtype");
                        AttrMetadata valueMeta = op.GetAttrMetadata("value");
                        using (Tensor valueTensor = op.GetAttrTensor("value"))
                        {
                            var dim = valueTensor.Dim;
                        }
                    }

                    if (op.OpType.Equals("Conv2D"))
                    {
                        AttrMetadata stridesMeta = op.GetAttrMetadata("strides");
                        AttrMetadata paddingMeta = op.GetAttrMetadata("padding");
                        AttrMetadata boolMeta = op.GetAttrMetadata("use_cudnn_on_gpu");
                        Int64[] strides = op.GetAttrIntList("strides");
                        bool useCudnn = op.GetAttrBool("use_cudnn_on_gpu");
                        String padding = op.GetAttrString("padding");
                    }

                    foreach (Output output in op.Outputs)
                    {
                        int[] shape = inceptionGraph.Graph.GetTensorShape(output);
                        if (output.NumConsumers == 0)
                        {
                            couldBeOutputs.Add(name);
                        }
                    }

                    Buffer buffer = inceptionGraph.Graph.GetOpDef(op.OpType);
                    Tensorflow.OpDef opDef = Tensorflow.OpDef.Parser.ParseFrom(buffer.Data);
                }

                using (Buffer versionDef = inceptionGraph.Graph.Versions())
                {
                    int l = versionDef.Length;
                }

                Inception.RecognitionResult[][] results = inceptionGraph.Recognize(imageTensor);

                Trace.WriteLine(String.Format("Object is {0} with {1}% probability", results[0][0].Label, results[0][0].Probability * 100));

            }
        }

        [TestAttribute]
        public async Task TestResnetBatch()
        {
            using (Tensor imageTensor = ImageIO.ReadTensorFromImageFiles<float>(
                new string[] {"surfers.jpg", "surfers.jpg"}, 
                224, 
                224, 
                0, 
                1.0f / 255.0f))
            using (Resnet resnet = new Resnet())
            {
                await resnet.Init();

                Resnet.RecognitionResult[][] results = resnet.Recognize(imageTensor);

            }
        }

        [TestAttribute]
        public async Task TestResnet()
        {
            using (Tensor imageTensor = ImageIO.ReadTensorFromImageFile<float>("surfers.jpg", 224, 224, 0, 1.0f/255.0f))
            using (Resnet resnet = new Resnet())
            {
                await resnet.Init();

                MetaGraphDef metaGraphDef = MetaGraphDef.Parser.ParseFrom(resnet.MetaGraphDefBuffer.Data);
                var signatureDef = metaGraphDef.SignatureDef["serving_default"];
                var inputNode = signatureDef.Inputs;
                var outputNode = signatureDef.Outputs;

                HashSet<string> opNames = new HashSet<string>();
                HashSet<string> couldBeInputs = new HashSet<string>();
                HashSet<string> couldBeOutputs = new HashSet<string>();
                foreach (Operation op in resnet.Graph)
                {

                    String name = op.Name;
                    opNames.Add(name);

                    if (op.NumInputs == 0 && op.OpType.Equals("Placeholder"))
                    {
                        couldBeInputs.Add(op.Name);
                        AttrMetadata dtypeMeta = op.GetAttrMetadata("dtype");
                        AttrMetadata shapeMeta = op.GetAttrMetadata("shape");
                        DataType type = op.GetAttrType("dtype");
                        Int64[] shape = op.GetAttrShape("shape");
                        Buffer valueBuffer = op.GetAttrValueProto("shape");
                        Buffer shapeBuffer = op.GetAttrTensorShapeProto("shape");
                        Tensorflow.TensorShapeProto shapeProto =
                            Tensorflow.TensorShapeProto.Parser.ParseFrom(shapeBuffer.Data);
                    }

                    if (op.OpType.Equals("Const"))
                    {
                        AttrMetadata dtypeMeta = op.GetAttrMetadata("dtype");
                        AttrMetadata valueMeta = op.GetAttrMetadata("value");
                        using (Tensor valueTensor = op.GetAttrTensor("value"))
                        {
                            var dim = valueTensor.Dim;
                        }
                    }

                    if (op.OpType.Equals("Conv2D"))
                    {
                        AttrMetadata stridesMeta = op.GetAttrMetadata("strides");
                        AttrMetadata paddingMeta = op.GetAttrMetadata("padding");
                        AttrMetadata boolMeta = op.GetAttrMetadata("use_cudnn_on_gpu");
                        Int64[] strides = op.GetAttrIntList("strides");
                        bool useCudnn = op.GetAttrBool("use_cudnn_on_gpu");
                        String padding = op.GetAttrString("padding");
                    }

                    foreach (Output output in op.Outputs)
                    {
                        int[] shape = resnet.Graph.GetTensorShape(output);
                        if (output.NumConsumers == 0)
                        {
                            couldBeOutputs.Add(name);
                        }
                    }

                    Buffer buffer = resnet.Graph.GetOpDef(op.OpType);
                    Tensorflow.OpDef opDef = Tensorflow.OpDef.Parser.ParseFrom(buffer.Data);
                }

                using (Buffer versionDef = resnet.Graph.Versions())
                {
                    int l = versionDef.Length;
                }

                Resnet.RecognitionResult[][] results = resnet.Recognize(imageTensor);

            }
        }

        [TestAttribute]
        public async Task TestMaskRCNNBatch()
        {
            using (Tensor imageTensor = ImageIO.ReadTensorFromImageFiles<byte>(
                new string[] { "surfers.jpg", "surfers.jpg" },
                -1,
                -1,
                0,
                1.0f))
            using (MaskRcnnInceptionV2Coco model = new MaskRcnnInceptionV2Coco())
            {
                await model.Init();

                MaskRcnnInceptionV2Coco.RecognitionResult[][] results = model.Recognize(imageTensor);

            }
        }

        [TestAttribute]
        public async Task TestMaskRCNN()
        {
            using (Tensor imageTensor = ImageIO.ReadTensorFromImageFile<byte>(
                "surfers.jpg",
                -1,
                -1,
                0,
                1.0f))
            using (MaskRcnnInceptionV2Coco model = new MaskRcnnInceptionV2Coco())
            {
                await model.Init();

                MaskRcnnInceptionV2Coco.RecognitionResult[][] results = model.Recognize(imageTensor);

            }
        }


        [TestAttribute]
        public async Task TestMultiboxPeopleDetect()
        {
            Tensor imageResults = ImageIO.ReadTensorFromImageFile<float>("surfers.jpg", 224, 224, 128.0f, 1.0f / 128.0f);

            MultiboxGraph multiboxGraph = new MultiboxGraph();
            //bool processCompleted = false;

            await multiboxGraph.Init();

            MultiboxGraph.Result[] result = multiboxGraph.Detect(imageResults);

        }


        [TestAttribute]
        public async Task TestStylize()
        {
            StylizeGraph stylizeGraph = new StylizeGraph();
            await stylizeGraph.Init();
            Tensor image = ImageIO.ReadTensorFromImageFile<float>("surfers.jpg");
            Tensor stylizedImage = stylizeGraph.Stylize(image, 0);
        }

        [TestAttribute]
        public void TestTensorCreate()
        {
            Tensor t = new Tensor(DataType.Float, new[] { 1, 3, 224, 224 });
        }

        [TestAttribute]
        public void TestEncodeJpeg()
        {
            Tensor image = ImageIO.ReadTensorFromImageFile<float>("surfers.jpg", 299, 299, 0, 1.0f / 255.0f, true, false);
            byte[] jpegRaw = ImageIO.TensorToJpeg(image, 255.0f, 0.0f);
            File.WriteAllBytes("surefers_out.jpg", jpegRaw);
        }

        [TestAttribute]
        public void TestCUDAEnabled()
        {
            bool cuda = TfInvoke.IsGoogleCudaEnabled;
            Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
            config.LogDevicePlacement = true;
            byte[] pbuff;
            using (MemoryStream ms = new MemoryStream())
            {
                config.WriteTo(ms);
                pbuff = ms.ToArray();
            }
            SessionOptions options = new SessionOptions();
            options.SetConfig(pbuff);
            Add(3, 4, options);
        }

        [TestAttribute]
        public void TestChooseDevice()
        {
            SessionOptions so = new SessionOptions();
            Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
            //config.DeviceCount.Add("GPU", 1);
            //config.DeviceCount.Add("CPU", 1);

            config.GpuOptions = new GPUOptions();
            config.GpuOptions.VisibleDeviceList = "0";
            //config.GpuOptions.VisibleDeviceList = "0, 1";
            //var devicesList = config.GpuOptions.VisibleDeviceList;

            //config.LogDevicePlacement = true;

            if (TfInvoke.IsGoogleCudaEnabled)
                so.SetConfig(config.ToProtobuf());

            int sum = Add(1, 2, so);

        }

        public static int Add(int a, int b, SessionOptions sessionOptions = null)
        {
            //Create a Tensor from value "a"
            Tensor tensorA = new Tensor(a);
            //Create a Tensor from value "b"
            Tensor tensorB = new Tensor(b);

            //create a new Graph
            Graph graph = new Graph();
            //place holder for tensorA
            Operation opA = graph.Placeholder(DataType.Int32, null, "valA");
            //place holder for tensorB
            Operation opB = graph.Placeholder(DataType.Int32, null, "valB");
            //The actual operation
            Operation sumOp = graph.Add(opA, opB, "sum");

            using (Buffer versionDef = graph.Versions())
            using (Buffer graphDef = new Buffer())
            {
                Tensorflow.VersionDef vdef = Tensorflow.VersionDef.Parser.ParseFrom(versionDef.Data);

                graph.ToGraphDef(graphDef);

                Tensorflow.GraphDef gdef = Tensorflow.GraphDef.Parser.ParseFrom(graphDef.Data);

                using (MemoryStream ms = new MemoryStream())
                using (Google.Protobuf.CodedOutputStream stream = new CodedOutputStream(ms))
                {
                    gdef.WriteTo(stream);
                    stream.Flush();
                    byte[] serializedGraphDef2 = ms.ToArray();

                    byte[] serializedGraphDef1 = graphDef.Data;

                    bool equal = true;
                    for (int i = 0; i < serializedGraphDef1.Length; i++)
                    {
                        if (serializedGraphDef1[i] != serializedGraphDef2[i])
                            equal = false;
                    }

                }

                foreach (Operation op in graph)
                {
                    String device = op.Device;
                }
            }
            Session session = new Session(graph, sessionOptions);
            Session.Device[] devices = session.ListDevices();
            Tensor[] results = session.Run(new Output[] { opA, opB }, new Tensor[] { tensorA, tensorB }, new Output[] { sumOp });
            return results[0].Flat<int>()[0];
        }

        [TestAttribute]
        public void TestAddition()
        {
            int a = 10;
            int b = 20;
            int sum = Add(a, b);
        }

        [TestAttribute]
        public void TestHelloWorld()
        {
            String h = "Hello, tensorflow!";

            Tensor hello = Tensor.FromString(System.Text.Encoding.Default.GetBytes(h));
            Graph g = new Graph();
            Operation helloOp = g.Const(hello, DataType.String);
            Session session = new Session(g);
            Tensor[] results = session.Run(new Output[] { }, new Tensor[] { }, new Output[] { helloOp });
            byte[] data = results[0].DecodeString();
            String output = System.Text.Encoding.Default.GetString(data);
            EmguAssert.IsTrue(output.Equals(h));
        }

        [TestAttribute]
        public void TestNativeImageIO()
        {
            int inputHeight = 299;
            int inputWidth = 299;
            String file = "surfers.jpg";
            Tensor t0 = new Tensor(DataType.Uint8, new int[] { 1, (int)inputHeight, (int)inputWidth, 3 });
            NativeImageIO.ReadImageFileToTensor<float>(file, t0.DataPointer, inputHeight, inputWidth, 0.0f,
                1.0f / 255.0f);
            Tensor t1 = ImageIO.ReadTensorFromImageFile<float>(file);
            float maxDiff = 0.0f;
            var ta0 = t0.GetData(false) as float[];
            var ta1 = t1.GetData(false) as float[];
            for (int i = 0; i < ta0.Length; i++)
            {
                float diff = Math.Abs(ta0[i] - ta1[i]);
                if (diff < maxDiff)
                    maxDiff = diff;
            }

        }

        [TestAttribute]
        public void TestIsOperationSupported()
        {
            bool castSupported = TfInvoke.OpHasKernel("Cast");
            EmguAssert.IsTrue(castSupported);
            bool notSupported = TfInvoke.OpHasKernel("NotASupportedOperation");
            EmguAssert.IsTrue(!notSupported);
            bool quantizeV2HasKernel = TfInvoke.OpHasKernel("QuantizeV2");
            bool quantizeV2IsRegistered = TfInvoke.OpIsRegistered("QuantizeV2");

        }

        /*
        [Test]
        public async void TestMultiSession()
        {
            Tensor imageTensor = ImageIO.ReadTensorFromImageFile<float>("grace_hopper.jpg", 224, 224, 128.0f, 1.0f);
            int sessionCount = 10;

            Inception[] graphs = new Inception[sessionCount];
            for (int i = 0; i < sessionCount; i++)
            {
                graphs[i] = new Inception();
                await graphs[i].Init();
            }
            for (int i = 0; i < sessionCount; i++)
            {
                Inception.RecognitionResult[] results = graphs[i].Recognize(imageTensor);
            }
        }*/

        [TestAttribute]
        public void TestServer1()
        {
            Tensorflow.ServerDef def = new ServerDef();
            ClusterDef clusterDef = new ClusterDef();
            JobDef jd = new JobDef();
            clusterDef.Job.Add(jd);

            def.Cluster = clusterDef;
            byte[] pbuff;
            using (MemoryStream ms = new MemoryStream())
            {
                def.WriteTo(ms);
                pbuff = ms.ToArray();
            }

            /*
            using (Emgu.TF.Server s = new Emgu.TF.Server(pbuff))
            {
                String target = s.Target;
            }*/

        }

        /*
        [TestMethod]
        public void TestServer2()
        {
            ServerDef serverDef = new ServerDef();
            ClusterDef cd = new ClusterDef();
            JobDef jd0 = new JobDef();
            
            jd0.Name = "worker";
            jd0.Tasks[0] = "127.0.0.1:5555";
            cd.Job.Add(jd0);

            serverDef.Cluster = cd;
            serverDef.JobName = "worker";
            serverDef.TaskIndex = 0;
            
            Emgu.TF.Server server = new Server(serverDef.ToByteArray());
            using (Status s1 = new Status())
                server.Start(s1);
            using (Status s2 = new Status())
                server.Stop(s2);
        }*/
    }
    
}
