// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using XRTK.Extensions;
using XRTK.Lumin.Native;
using XRTK.Utilities.Async;
using XRTK.Editor.Utilities;
using Debug = UnityEngine.Debug;

namespace XRTK.Lumin.Editor
{
    [InitializeOnLoad]
    public static class LuminRemote
    {
        private static readonly string LuminPackageRoot = PathFinderUtility.ResolvePath<IPathFinder>(typeof(LuminPathFinder));
        private static readonly string LuminRemoteSupportPath = $"{LuminPackageRoot}\\Runtime\\Plugins\\Editor\\x64";
        private static readonly string LuminRemoteSupportFullPath = Path.GetFullPath(LuminRemoteSupportPath);

        private static string LuminSDKRoot => EditorPrefs.GetString(nameof(LuminSDKRoot));

        private static bool isRemoteConfigured;

        static LuminRemote()
        {
            if (Application.isBatchMode) { return; }
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Lumin) { return; }

            if (string.IsNullOrWhiteSpace(LuminSDKRoot))
            {
                Debug.LogWarning("Failed to resolve Magic Leap Sdk Path! Be sure to set this path in the 'External Tools' section of the editor preferences.");
                return;
            }

            if (!Directory.Exists(LuminRemoteSupportFullPath) ||
                EditorPreferences.Get($"ReImport_{nameof(LuminRemote)}", false))
            {
                EditorPreferences.Set($"ReImport_{nameof(LuminRemote)}", false);

                try
                {
                    if (Directory.Exists(LuminRemoteSupportFullPath))
                    {
                        var files = Directory.GetFiles(LuminRemoteSupportFullPath, "*", SearchOption.AllDirectories);

                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }

                        File.Delete($"{LuminRemoteSupportFullPath}.meta");
                        Directory.Delete(LuminRemoteSupportFullPath);
                    }

                    InstallLuminRemoteLibraries();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                EditorApplication.playModeStateChanged += EditorApplication_OnPlayModeStateChanged;
                EditorApplication.delayCall += () => MLRemoteIsServerConfigured(ref isRemoteConfigured);
            }
        }

        private static async void InstallLuminRemoteLibraries()
        {
            Directory.CreateDirectory(LuminRemoteSupportFullPath);

            var supportPaths = await LabDriver.GetLuminRemoteSupportLibrariesAsync(LuminSDKRoot);

            await Awaiters.UnityMainThread;

            if (supportPaths == null ||
                supportPaths.Count == 0)
            {
                Debug.LogError("Failed to copy lumin remote support libraries!");
                Directory.Delete(LuminRemoteSupportFullPath);
                return;
            }

            foreach (var path in supportPaths)
            {
                if (path.Contains("zi"))
                {
                    var supportFiles = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);

                    foreach (var sourceFile in supportFiles)
                    {
                        var destination = sourceFile.ToBackSlashes().Replace(path.ToBackSlashes(), LuminRemoteSupportFullPath.ToBackSlashes()).ToBackSlashes();

                        try
                        {
                            File.Copy(sourceFile, destination);
                            File.WriteAllText($"{destination}.meta", $@"fileFormatVersion: 2
guid: {GUID.Generate()}
PluginImporter:
  externalObjects: {{}}
  serializedVersion: 2
  iconMap: {{}}
  executionOrder: {{}}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 1
  isExplicitlyReferenced: 0
  validateReferences: 1
  platformData:
  - first:
      Any: 
    second:
      enabled: 0
      settings: {{}}
  - first:
      Editor: Editor
    second:
      enabled: 1
      settings:
        CPU: x86_64
        DefaultValueInitialized: true
  userData: 
  assetBundleName: 
  assetBundleVariant: 
");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }

                    }
                }
            }

            EditorApplication.delayCall += () => AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        [MenuItem("Mixed Reality Toolkit/Tools/Magic Leap/Reimport Remote Support Libraries", true)]
        private static bool UpdateLuminRemoteSupportLibrariesValidation()
        {
            return Directory.Exists(LuminRemoteSupportFullPath);
        }

        [MenuItem("Mixed Reality Toolkit/Tools/Magic Leap/Reimport Remote Support Libraries", false)]
        private static void UpdateLuminRemoteSupportLibraries()
        {
            if (EditorUtility.DisplayDialog("Attention!",
                "To reimport the remote support libraries, we have to restart the editor, is this ok?",
                "Restart",
                "Cancel"))
            {
                EditorAssemblyReloadManager.LockReloadAssemblies = true;

                EditorApplication.delayCall += () =>
                {
                    EditorPreferences.Set($"ReImport_{nameof(LuminRemote)}", true);
                    EditorApplication.OpenProject(Directory.GetParent(Application.dataPath).FullName);
                };
            }
        }

        private static void EditorApplication_OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (!isRemoteConfigured)
                    {
                        EditorApplication.isPlaying = false;
                        EditorApplication.Beep();
                        Debug.LogError("Lumin Remote is not running! Ensure you've started Magic Leap zero iteration in The Lab.");
                    }

                    if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore)
                    {
                        EditorApplication.isPlaying = false;
                        EditorApplication.Beep();

                        if (EditorUtility.DisplayDialog("Open GL Core Required to run Zero Iteration",
                            "To use Magic Leap zero iteration mode in the editor, the editor must restart using OpenGL.Core",
                            "Restart", "Do Not Restart"))
                        {
                            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
                            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.OpenGLCore, GraphicsDeviceType.Direct3D11 });
                            AssetDatabase.SaveAssets();
                            EditorApplication.OpenProject(Path.GetDirectoryName(Application.dataPath));
                        }
                        else
                        {
                            Debug.LogError("To use Magic Leap zero iteration mode in the editor, the editor must restart using OpenGL.Core");
                        }
                    }
                    break;
            }
        }

        [DllImport("ml_remote", CallingConvention = CallingConvention.Cdecl)]
        private static extern MlApi.MLResult MLRemoteIsServerConfigured(ref bool isConfigured);
    }
}