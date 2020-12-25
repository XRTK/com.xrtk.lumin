// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;
using XRTK.Editor.Extensions;
using XRTK.Editor.Profiles.InputSystem.Controllers;
using XRTK.Lumin.Profiles;

namespace XRTK.Lumin.Editor
{
    [CustomEditor(typeof(LuminHandControllerDataProviderProfile))]
    public class LuminHandControllerDataProviderProfileInspector : BaseMixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty poseFilterLevel;
        private SerializedProperty keyPointFilterLevel;

        private static readonly GUIContent handTrackingFoldoutHeader = new GUIContent("Lumin Hand Tracking Settings");

        private bool showLuminHandTrackingSettings = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            keyPointFilterLevel = serializedObject.FindProperty(nameof(keyPointFilterLevel));
            poseFilterLevel = serializedObject.FindProperty(nameof(poseFilterLevel));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            showLuminHandTrackingSettings = EditorGUILayoutExtensions.FoldoutWithBoldLabel(showLuminHandTrackingSettings, handTrackingFoldoutHeader);

            if (showLuminHandTrackingSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(keyPointFilterLevel);
                EditorGUILayout.PropertyField(poseFilterLevel);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}