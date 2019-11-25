// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Utilities;

#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace XRTK.Lumin.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Data Providers/Hands/Lumin Hand Controller Data Provider Profile", fileName = "LuminHandControllerDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Input)]
    public class LuminHandControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
#if PLATFORM_LUMIN
        [Header("Hand Tracking Level Configuration")]

        [SerializeField]
        [Tooltip("Configured level for keypoints filtering of keypoints and hand centers.")]
        private MLKeyPointFilterLevel keyPointFilterLevel = MLKeyPointFilterLevel.ExtraSmoothed;

        /// <summary>
        /// Configured level for keypoints filtering of keypoints and hand centers.
        /// </summary>
        public MLKeyPointFilterLevel KeyPointFilterLevel => keyPointFilterLevel;

        [SerializeField]
        [Tooltip("Configured level of filtering for static poses.")]
        private MLPoseFilterLevel poseFilterLevel = MLPoseFilterLevel.ExtraRobust;

        /// <summary>
        /// Configured level of filtering for static poses.
        /// </summary>
        public MLPoseFilterLevel PoseFilterLevel => poseFilterLevel;
#endif
    }
}