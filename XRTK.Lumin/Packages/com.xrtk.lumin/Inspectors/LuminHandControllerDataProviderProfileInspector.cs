// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using XRTK.Inspectors.Profiles.InputSystem.Controllers;
using XRTK.Lumin.Profiles;

namespace XRTK.Lumin.Inspectors
{
    [CustomEditor(typeof(LuminHandControllerDataProviderProfile))]
    public class LuminHandControllerDataProviderProfileInspector : BaseMixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty keyPointFilterLevel;
        private SerializedProperty poseFilterLevel;

        protected override void OnEnable()
        {
            base.OnEnable();

            keyPointFilterLevel = serializedObject.FindProperty(nameof(keyPointFilterLevel));
            poseFilterLevel = serializedObject.FindProperty(nameof(poseFilterLevel));
        }

        public override void OnInspectorGUI()
        {
            RenderHeader();

            EditorGUILayout.LabelField("Lumin Hand Controller Data Provider Settings", EditorStyles.boldLabel);

            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Lumin Hand Settings");
            EditorGUILayout.PropertyField(keyPointFilterLevel);
            EditorGUILayout.PropertyField(poseFilterLevel);

            serializedObject.ApplyModifiedProperties();
        }
    }
}