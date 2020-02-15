// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using XRTK.Definitions.Utilities;
using XRTK.Inspectors.Profiles.InputSystem.Controllers.Hands;
using XRTK.Inspectors.Utilities;
using XRTK.Lumin.Profiles;

namespace XRTK.Lumin.Inspectors
{
    [CustomEditor(typeof(LuminHandControllerDataProviderProfile))]
    public class LuminHandControllerDataProviderProfileInspector : MixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty keyPointFilterLevel;
        private SerializedProperty poseFilterLevel;

        protected override void OnEnable()
        {
            base.OnEnable();

            keyPointFilterLevel = serializedObject.FindProperty("keyPointFilterLevel");
            poseFilterLevel = serializedObject.FindProperty("poseFilterLevel");
        }

        protected override void OnPlatformInspectorGUI()
        {
            SupportedPlatforms supportedPlatforms = SupportedPlatforms.Lumin | SupportedPlatforms.Editor;
            if (MixedRealityInspectorUtility.CheckProfilePlatform(supportedPlatforms,
                $"You can't edit platform specific hand configuration with the current build target. Please switch to {supportedPlatforms}."))
            {
                EditorGUILayout.PropertyField(keyPointFilterLevel);
                EditorGUILayout.PropertyField(poseFilterLevel);
            }
        }
    }
}