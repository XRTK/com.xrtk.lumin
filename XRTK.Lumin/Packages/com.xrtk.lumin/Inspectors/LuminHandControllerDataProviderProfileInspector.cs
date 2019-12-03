// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;
using XRTK.Inspectors.Profiles;
using XRTK.Inspectors.Utilities;
using XRTK.Lumin.Profiles;

namespace XRTK.Lumin.Inspectors
{
    [CustomEditor(typeof(LuminHandControllerDataProviderProfile))]
    public class LuminHandControllerDataProviderProfileInspector : BaseMixedRealityProfileInspector
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

#if PLATFORM_LUMIN
            serializedObject.Update();

            EditorGUILayout.BeginVertical("Label");
            EditorGUILayout.PropertyField(handTrackingEnabled);
            EditorGUILayout.PropertyField(keyPointFilterLevel);
            EditorGUILayout.PropertyField(poseFilterLevel);

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
#else
            EditorGUILayout.HelpBox("Please switch to Lumin build target to edit this profile.", MessageType.Info);
#endif
        }
    }
}