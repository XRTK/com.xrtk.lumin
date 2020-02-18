// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;
using XRTK.Definitions.Utilities;
using XRTK.Inspectors.Profiles;
using XRTK.Inspectors.Utilities;
using XRTK.Lumin.Profiles;

namespace XRTK.Lumin.Inspectors
{
    [CustomEditor(typeof(LuminHandControllerDataProviderProfile))]
    public class LuminHandControllerDataProviderProfileInspector : BaseMixedRealityProfileInspector
    {
        private SerializedProperty keyPointFilterLevel;
        private SerializedProperty poseFilterLevel;

        protected override void OnEnable()
        {
            base.OnEnable();

            keyPointFilterLevel = serializedObject.FindProperty("keyPointFilterLevel");
            poseFilterLevel = serializedObject.FindProperty("poseFilterLevel");
        }

        public override void OnInspectorGUI()
        {
            MixedRealityInspectorUtility.RenderMixedRealityToolkitLogo();

            if (thisProfile.ParentProfile != null &&
                GUILayout.Button("Back To Configuration Profile"))
            {
                Selection.activeObject = thisProfile.ParentProfile;
            }

            thisProfile.CheckProfileLock();
            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.Space();
            SupportedPlatforms supportedPlatforms = SupportedPlatforms.Lumin | SupportedPlatforms.Editor;
            if (MixedRealityInspectorUtility.CheckProfilePlatform(supportedPlatforms,
                $"You can't edit platform specific hand configuration with the current build target. Please switch to {supportedPlatforms}."))
            {
                EditorGUILayout.PropertyField(keyPointFilterLevel);
                EditorGUILayout.PropertyField(poseFilterLevel);
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}