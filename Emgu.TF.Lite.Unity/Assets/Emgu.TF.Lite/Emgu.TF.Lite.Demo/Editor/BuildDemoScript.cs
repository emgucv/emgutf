#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;

public class BuildDemoScript {

	static void PerformOSXBuild () {
		EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
		string[] scenes = {"Assets/Emgu.TF/Emgu.TF.Demo/InceptionScene.unity"};
		BuildPipeline.BuildPlayer (scenes, "Builds/OSX/osx.app", BuildTarget.StandaloneOSX, BuildOptions.None);
	}


	static void PerformWin64Build () {
		EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
		string[] scenes = {"Assets/Emgu.TF/Emgu.TF.Demo/InceptionScene.unity"};
		BuildPipeline.BuildPlayer (scenes, "Builds/Win64/win64.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
	}

	static void PerformAndroidBuild () {
		EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTargetGroup.Android, BuildTarget.Android);
		string[] scenes = {"Assets/Emgu.TF/Emgu.TF.Demo/InceptionScene.unity"};
		BuildPipeline.BuildPlayer (scenes, "Builds/Android", BuildTarget.Android, BuildOptions.None);
	}

} 
#endif
