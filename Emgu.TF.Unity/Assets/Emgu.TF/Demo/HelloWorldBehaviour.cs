//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;
using Emgu.TF;
using Emgu.TF.Models;
using UnityEngine.UI;

public class HelloWorldBehaviour : MonoBehaviour {

    public Text DisplayText;

    // Use this for initialization
    void Start () {
        String version = Emgu.TF.TfInvoke.Version;
        int a = 1;
        int b = 2;
        int sum = Add(a, b);
        DisplayText.text = String.Format("Tensorflow Version: {0}{1}Execution Test: {2}+{3}={4}", version, Environment.NewLine, a, b, sum);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public static int Add(int a, int b, SessionOptions sessionOptions = null)
    {
        //Creating tensor from value a
        Tensor tensorA = new Tensor(a);
        //Creating tensor from value b
        Tensor tensorB = new Tensor(b);
        //Create a new graph
        Graph graph = new Graph();
        //Place holder in the graph for tensorA
        Operation opA = graph.Placeholder(DataType.Int32, null, "valA");
        //Place holder in the graph for tensorB
        Operation opB = graph.Placeholder(DataType.Int32, null, "valB");
        //Adding the two tensor
        Operation sumOp = graph.Add(opA, opB, "sum");

        //Create a new session
        Session session = new Session(graph, sessionOptions);
        
        //Execute the session and get the sum
        Tensor[] results = session.Run(new Output[] { opA, opB }, new Tensor[] { tensorA, tensorB }, new Output[] { sumOp });

        //retuning the value
        return results[0].Flat<int>()[0];
    }
}
