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
    [CustomEditor(typeof(LuminControllerDataProviderProfile))]
    public class LuminControllerDataProviderProfileInspector : BaseMixedRealityProfileInspector
    {
        private SerializedProperty handTrackingEnabled;
        private SerializedProperty keyPointFilterLevel;
        private SerializedProperty poseFilterLevel;

        protected override void OnEnable()
        {
            base.OnEnable();

            handTrackingEnabled = serializedObject.FindProperty("handTrackingEnabled");
            keyPointFilterLevel = serializedObject.FindProperty("keyPointFilterLevel");
            poseFilterLevel = serializedObject.FindProperty("poseFilterLevel");
        }

        public override void OnInspectorGUI()
        {
            MixedRealityInspectorUtility.RenderMixedRealityToolkitLogo();

            if (thisProfile.ParentProfile != null &&
                GUILayout.Button("Back to Configuration Profile"))
            {
                Selection.activeObject = thisProfile.ParentProfile;
            }

            EditorGUILayout.Space();
            thisProfile.CheckProfileLock();

            if (MixedRealityInspectorUtility.CheckProfilePlatform(SupportedPlatforms.Lumin | SupportedPlatforms.Editor))
            {
                serializedObject.Update();

                EditorGUILayout.BeginVertical("Label");
                EditorGUILayout.PropertyField(handTrackingEnabled);
                EditorGUILayout.PropertyField(keyPointFilterLevel);
                EditorGUILayout.PropertyField(poseFilterLevel);

                EditorGUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}