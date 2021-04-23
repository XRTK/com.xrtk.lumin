// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers.Hands;

namespace XRTK.Lumin.Profiles
{
    public class LuminHandControllerDataProviderProfile : BaseHandControllerDataProviderProfile
    {
        [SerializeField]
        [Tooltip("Configured level for keypoints filtering of keypoints and hand centers.")]
        private MLKeyPointFilterLevel keyPointFilterLevel = 0;

        /// <summary>
        /// Configured level for keypoints filtering of keypoints and hand centers.
        /// </summary>
        public MLKeyPointFilterLevel KeyPointFilterLevel => keyPointFilterLevel;

        [SerializeField]
        [Tooltip("Configured level of filtering for static poses.")]
        private MLPoseFilterLevel poseFilterLevel = 0;

        /// <summary>
        /// Configured level of filtering for static poses.
        /// </summary>
        public MLPoseFilterLevel PoseFilterLevel => poseFilterLevel;
    }
}