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

        private GUIContent keyPointContent;
        private GUIContent poseFilterContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            keyPointFilterLevel = serializedObject.FindProperty(nameof(keyPointFilterLevel));
            poseFilterLevel = serializedObject.FindProperty(nameof(poseFilterLevel));

            keyPointContent = new GUIContent(keyPointFilterLevel.displayName, keyPointFilterLevel.tooltip);
            poseFilterContent = new GUIContent(poseFilterLevel.displayName, poseFilterLevel.tooltip);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            keyPointFilterLevel.isExpanded = EditorGUILayoutExtensions.FoldoutWithBoldLabel(keyPointFilterLevel.isExpanded, handTrackingFoldoutHeader);

            if (keyPointFilterLevel.isExpanded)
            {
                EditorGUI.indentLevel++;
                keyPointFilterLevel.intValue = (int)(LuminApi.KeyPointFilterLevel)EditorGUILayout.EnumPopup(keyPointContent, (LuminApi.KeyPointFilterLevel)keyPointFilterLevel.intValue);
                poseFilterLevel.intValue = (int)(LuminApi.PoseFilterLevel)EditorGUILayout.EnumPopup(poseFilterContent, (LuminApi.PoseFilterLevel)poseFilterLevel.intValue);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}