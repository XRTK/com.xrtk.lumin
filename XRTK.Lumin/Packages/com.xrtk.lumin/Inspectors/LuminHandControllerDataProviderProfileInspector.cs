// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using UnityEngine;
using XRTK.Inspectors.Profiles.InputSystem.Controllers;
using XRTK.Lumin.Profiles;

using UnityEngine.XR.MagicLeap;

namespace XRTK.Lumin.Inspectors
{
    [CustomEditor(typeof(LuminHandControllerDataProviderProfile))]
    public class LuminHandControllerDataProviderProfileInspector : BaseMixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty poseFilterLevel;
        private SerializedProperty keyPointFilterLevel;

        private GUIContent keyPointContent;
        private GUIContent poseFilterContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            keyPointFilterLevel = serializedObject.FindProperty(nameof(keyPointFilterLevel));
            keyPointContent = new GUIContent(keyPointFilterLevel.displayName, keyPointFilterLevel.tooltip);
            poseFilterLevel = serializedObject.FindProperty(nameof(poseFilterLevel));
            poseFilterContent = new GUIContent(poseFilterLevel.displayName, poseFilterLevel.tooltip);
        }

        public override void OnInspectorGUI()
        {
            RenderHeader();

            EditorGUILayout.LabelField("Lumin Hand Controller Data Provider Settings", EditorStyles.boldLabel);

            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Lumin Hand Settings");
            keyPointFilterLevel.intValue = (int)(MLKeyPointFilterLevel)EditorGUILayout.EnumPopup(keyPointContent, (MLKeyPointFilterLevel)keyPointFilterLevel.intValue);
            poseFilterLevel.intValue = (int)(MLPoseFilterLevel)EditorGUILayout.EnumPopup(poseFilterContent, (MLPoseFilterLevel)poseFilterLevel.intValue);

            serializedObject.ApplyModifiedProperties();
        }
    }
}