// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using XRTK.Inspectors.Profiles;
using XRTK.Lumin.Profiles;

namespace XRTK.Lumin.Inspectors
{
    [CustomEditor(typeof(LuminControllerDataProviderProfile))]
    public class LuminControllerDataProviderProfileInspector : BaseMixedRealityProfileInspector
    {
        private SerializedProperty keyPointFilterLevel;
        private SerializedProperty poseFilterLevel;

        // Global hand settings overrides
        private SerializedProperty handMeshingEnabled;
        private SerializedProperty handPhysicsEnabled;
        private SerializedProperty useTriggers;
        private SerializedProperty boundsMode;

        protected override void OnEnable()
        {
            base.OnEnable();

            handMeshingEnabled = serializedObject.FindProperty(nameof(handMeshingEnabled));
            handPhysicsEnabled = serializedObject.FindProperty(nameof(handPhysicsEnabled));
            useTriggers = serializedObject.FindProperty(nameof(useTriggers));
            boundsMode = serializedObject.FindProperty(nameof(boundsMode));

            keyPointFilterLevel = serializedObject.FindProperty(nameof(keyPointFilterLevel));
            poseFilterLevel = serializedObject.FindProperty(nameof(poseFilterLevel));
        }

        public override void OnInspectorGUI()
        {
            RenderHeader();

            serializedObject.Update();

            EditorGUILayout.PropertyField(handMeshingEnabled);
            EditorGUILayout.PropertyField(handPhysicsEnabled);
            EditorGUILayout.PropertyField(useTriggers);
            EditorGUILayout.PropertyField(boundsMode);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(keyPointFilterLevel);
            EditorGUILayout.PropertyField(poseFilterLevel);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}