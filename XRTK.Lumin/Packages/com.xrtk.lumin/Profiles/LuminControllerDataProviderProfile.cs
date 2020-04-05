// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;

namespace XRTK.Lumin.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Data Providers/Lumin Controller Data Provider Profile", fileName = "LuminControllerDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Input)]
    public class LuminControllerDataProviderProfile : BaseHandControllerDataProviderProfile
    {
        [Header("Lumin Platform Settings")]

        [SerializeField]
        [Tooltip("Configured level for keypoints filtering of keypoints and hand centers.")]
        private int keyPointFilterLevel = 0;

        /// <summary>
        /// Configured level for keypoints filtering of keypoints and hand centers.
        /// </summary>
        public int KeyPointFilterLevel => keyPointFilterLevel;

        [SerializeField]
        [Tooltip("Configured level of filtering for static poses.")]
        private int poseFilterLevel = 0;

        /// <summary>
        /// Configured level of filtering for static poses.
        /// </summary>
        public int PoseFilterLevel => poseFilterLevel;
    }
}