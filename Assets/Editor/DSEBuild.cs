using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

public class DSEBuild {
	private enum PATH_STYLES {
		ABSOLUTE,
		RELATIVE
	}

	private static string targetPath = "../DSEBuild/";
	private static string[] scenes = new string[]{"Assets/Examples/InteractiveConsole.unity"};

	[@MenuItem("DSE/Build IOS/Absolute Path")]
	static void IOSAbsolute() {
		CustomBuild(PATH_STYLES.ABSOLUTE);
	}

	[@MenuItem("DSE/Build IOS/Relative Path")]
	static void IOSRelative() {
		CustomBuild(PATH_STYLES.RELATIVE);
	}

	static void CustomBuild(PATH_STYLES pathStyle) {
		string finalPath = targetPath;
		if(pathStyle == PATH_STYLES.ABSOLUTE) {
			finalPath = Path.GetFullPath(finalPath);
		}

		Debug.Log("Saving build to " + finalPath);
		BuildPipeline.BuildPlayer(
			scenes,
			finalPath,
			BuildTarget.iPhone,
			BuildOptions.None
		);
	}
}
