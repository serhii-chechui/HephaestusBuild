using System;
using System.IO;
using System.Linq;
using UnityEditor;
// using UnityEditor.AddressableAssets.Settings;
// using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
#endif

namespace Editor
{
	public class BuildScript
	{
		#region Parameters
		
		private const string BuildsDirectory = "build";
		
		private const string DefinitionsProduction = "";
		private const string DefinitionsDevelopment = "DEVELOPMENT_BUILD";

		private const string KeystoreName = "/Users/ghost/keystores/merge_odyssey.keystore";
		private const string KeystorePassword = "xGh05ty3d";
		private const string KeyAliasName = "mergeodyssey";
		private const string KeyAliasPassword = "xGh05ty3d";
		
		private static BuildPlayerOptions _buildOptions;

		#endregion
		
		#region iOS
		
		#if UNITY_EDITOR_OSX
		
		[MenuItem("Tools/Build/iOS/Build iOS – Production")]
		public static void PerformIOSBuildProduction()
		{
			_buildOptions = new BuildPlayerOptions {
				options = BuildOptions.ShowBuiltPlayer,
				scenes = FindEnabledEditorScenes(),
				target = BuildTarget.iOS,
				targetGroup = BuildTargetGroup.iOS,
				locationPathName = Path.Combine(BuildsDirectory, BuildTarget.iOS.ToString())
			};

			GenericBuild(_buildOptions);
		}
		
		[MenuItem("Tools/Build/iOS/Build iOS – Development")]
		public static void PerformIOSBuildDevelopment()
		{
			_buildOptions = new BuildPlayerOptions {
				options = BuildOptions.ShowBuiltPlayer,
				scenes = FindEnabledEditorScenes(),
				target = BuildTarget.iOS,
				targetGroup = BuildTargetGroup.iOS,
				locationPathName = Path.Combine(BuildsDirectory, BuildTarget.iOS.ToString())
			};
			
			GenericBuild(_buildOptions, true);
		}
		
		#endif
		
		#endregion

		#region Android AAB

		[MenuItem("Tools/Build/Android/Android AAB - Production")]
		public static void PerformAndroidProductionBuildAAB()
		{
			AssembleAndroidBuild(false, true);
		}
		
		[MenuItem("Tools/Build/Android/Android APK - Production")]
		public static void PerformAndroidProductionBuildAPK()
		{
			AssembleAndroidBuild(false,false);
		}

		[MenuItem("Tools/Build/Android/Android AAB - Development")]
		public static void PerformAndroidDevelopmentBuildAAB()
		{
			AssembleAndroidBuild(true, true);
		}
		
		[MenuItem("Tools/Build/Android/Android APK - Development")]
		public static void PerformAndroidDevelopmentBuildAPK()
		{
			AssembleAndroidBuild(true, false);
		}
		
		public static void AssembleAndroidBuild(bool development, bool buildAppBundle)
		{
			EditorUserBuildSettings.androidBuildType = development ? AndroidBuildType.Development : AndroidBuildType.Release;
			EditorUserBuildSettings.development = development;
			EditorUserBuildSettings.buildAppBundle = buildAppBundle;
			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
			PlayerSettings.Android.buildApkPerCpuArchitecture = true;

			var fileExtension = buildAppBundle ? ".aab" : ".apk";
			
			var aabPath = Path.Combine(BuildsDirectory, "Android", $"{BuildTarget.Android.ToString()}{fileExtension}");
			var apkPath = Path.Combine(BuildsDirectory, "Android", $"{BuildTarget.Android.ToString()}{fileExtension}");

			var buildLocationPathName = buildAppBundle ? aabPath : apkPath;

			_buildOptions = new BuildPlayerOptions {
				options = BuildOptions.ShowBuiltPlayer,
				scenes = FindEnabledEditorScenes(),
				target = BuildTarget.Android,
				targetGroup = BuildTargetGroup.Android,
				locationPathName = buildLocationPathName
			};

			GenericBuild(_buildOptions, development);
		}

		#endregion

		#region Addressables

		// [MenuItem("Tools/Build/Build Addressables")]
		// private static void BuildAddressables()
		// {
		// 	Debug.Log("[BuildScript] Start build addressable");
		// 	AddressableAssetSettings.CleanPlayerContent();
		// 	Debug.Log("[BuildScript] Clean is done");
		// 	BuildCache.PurgeCache(false);
		// 	Debug.Log("[BuildScript] PurgeCache is done");
		// 	AddressableAssetSettings.BuildPlayerContent();
		// 	Debug.Log("[BuildScript] addressables have been built");
		// }

		#endregion

		private static string[] FindEnabledEditorScenes()
		{
			return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
		}

		private static void GenericBuild(BuildPlayerOptions buildOptions, bool development = false)
		{
			Debug.Log($"================== Start build {buildOptions.target} ==================");
			
			EditorUserBuildSettings.SwitchActiveBuildTarget(buildOptions.targetGroup, buildOptions.target);

			if (buildOptions.target == BuildTarget.Android) {
				PlayerSettings.Android.keystoreName = KeystoreName;
				PlayerSettings.Android.keystorePass = KeystorePassword;
				PlayerSettings.Android.keyaliasName = KeyAliasName;
				PlayerSettings.Android.keyaliasPass = KeyAliasPassword;
			}

			PlayerSettings.SetScriptingDefineSymbolsForGroup(buildOptions.targetGroup, !development ? DefinitionsProduction : DefinitionsDevelopment);
			
			#if DEVELOPMENT_BUILD
				Debug.unityLogger.logEnabled = true;
			#else
				Debug.unityLogger.logEnabled = false;
			#endif
			
			//BuildAddressables();
			
			var buildReport  = BuildPipeline.BuildPlayer(buildOptions);
			var buildSummary = buildReport.summary;

			switch (buildSummary.result) {
				case BuildResult.Succeeded:
					Debug.Log($"Build Succeeded, size: {buildSummary.totalSize} bytes.");
					break;
				case BuildResult.Failed:
					throw new Exception($"BuildPlayer failure: {buildReport}");
				case BuildResult.Unknown:
					break;
				case BuildResult.Cancelled:
					Debug.LogWarning("Build was canceled.");
					break;
			}
		}
	}
}