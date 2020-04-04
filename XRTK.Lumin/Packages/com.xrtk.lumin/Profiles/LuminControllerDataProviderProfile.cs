// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Utilities;
using XRTK.Providers.Controllers.Hands;

namespace XRTK.Lumin.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Data Providers/Lumin Controller Data Provider Profile", fileName = "LuminControllerDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Input)]
    public class LuminControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        #region Global Hand Settings Overrides

        [Header("General Settings")]

        [SerializeField]
        [Tooltip("If set, hand mesh data will be read and available for visualization. Disable for optimized performance.")]
        private bool handMeshingEnabled = false;

        /// <summary>
        /// If set, hand mesh data will be read and available for visualization. Disable for optimized performance.
        /// </summary>
        public bool HandMeshingEnabled => handMeshingEnabled;

        [Header("Hand Physics")]

        [SerializeField]
        [Tooltip("If set, hands will be setup with colliders and a rigidbody to work with Unity's physics system.")]
        private bool handPhysicsEnabled = false;

        /// <summary>
        /// If set, hands will be setup with colliders and a rigidbody to work with Unity's physics system.
        /// </summary>
        public bool HandPhysicsEnabled => handPhysicsEnabled;

        [SerializeField]
        [Tooltip("If set, hand colliders will be setup as triggers.")]
        private bool useTriggers = false;

        /// <summary>
        /// If set, hand colliders will be setup as triggers.
        /// </summary>
        public bool UseTriggers => useTriggers;

        [SerializeField]
        [Tooltip("Set the bounds mode to use for calculating hand bounds.")]
        private HandBoundsMode boundsMode = HandBoundsMode.Hand;

        /// <summary>
        /// Set the bounds mode to use for calculating hand bounds.
        /// </summary>
        public HandBoundsMode BoundsMode => boundsMode;

        #endregion Global Hand Settings Overrides

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