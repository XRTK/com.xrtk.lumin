// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using XRTK.Lumin.Native;

namespace XRTK.Lumin.Editor
{
    public class LuminRemote
    {
        private static bool isRemoteConfigured;

        [InitializeOnLoadMethod]
        private static void InitLuminRemote()
        {
            EditorApplication.playModeStateChanged += EditorApplication_OnPlayModeStateChanged;
            EditorApplication.delayCall += () => MLRemoteIsServerConfigured(ref isRemoteConfigured);
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