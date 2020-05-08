// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;
using XRTK.Editor.Extensions;
using XRTK.Editor.Profiles.InputSystem.Controllers;
using XRTK.Lumin.Profiles;

#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif // PLATFORM_LUMIN

namespace XRTK.Lumin.Editor
{
    [CustomEditor(typeof(LuminHandControllerDataProviderProfile))]
    public class LuminHandControllerDataProviderProfileInspector : BaseMixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty poseFilterLevel;
        private SerializedProperty keyPointFilterLevel;

        private static readonly GUIContent handTrackingFoldoutHeader = new GUIContent("Lumin Hand Tracking Settings");

#if PLATFORM_LUMIN
        private GUIContent keyPointContent;
        private GUIContent poseFilterContent;
#endif // PLATFORM_LUMIN

        private bool showLuminHandTrackingSettings = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            keyPointFilterLevel = serializedObject.FindProperty(nameof(keyPointFilterLevel));
            poseFilterLevel = serializedObject.FindProperty(nameof(poseFilterLevel));

#if PLATFORM_LUMIN
            keyPointContent = new GUIContent(keyPointFilterLevel.displayName, keyPointFilterLevel.tooltip);
            poseFilterContent = new GUIContent(poseFilterLevel.displayName, poseFilterLevel.tooltip);
#endif // PLATFORM_LUMIN
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            showLuminHandTrackingSettings = EditorGUILayoutExtensions.FoldoutWithBoldLabel(showLuminHandTrackingSettings, handTrackingFoldoutHeader, true);
            if (showLuminHandTrackingSettings)
            {
                EditorGUI.indentLevel++;
#if PLATFORM_LUMIN
                keyPointFilterLevel.intValue = (int)(MLHandTracking.KeyPointFilterLevel)EditorGUILayout.EnumPopup(keyPointContent, (MLHandTracking.KeyPointFilterLevel)keyPointFilterLevel.intValue);
                poseFilterLevel.intValue = (int)(MLHandTracking.PoseFilterLevel)EditorGUILayout.EnumPopup(poseFilterContent, (MLHandTracking.PoseFilterLevel)poseFilterLevel.intValue);
#else
                EditorGUILayout.PropertyField(keyPointFilterLevel);
                EditorGUILayout.PropertyField(poseFilterLevel);
#endif // PLATFORM_LUMIN
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}