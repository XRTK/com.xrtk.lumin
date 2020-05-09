// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;

namespace XRTK.Lumin.Utilities
{
    /// <summary>
    /// Converts Lumin hand data to <see cref="HandData"/>.
    /// </summary>
    public sealed class LuminHandDataConverter
    {
        private enum Joint
        {
            Mcp = 2,
            Pip = 3,
            Tip = 4
        }

        private static List<Bone> indexBones = new List<Bone>();
        private static List<Bone> middleBones = new List<Bone>();
        private static List<Bone> ringBones = new List<Bone>();
        private static List<Bone> pinkyBones = new List<Bone>();
        private static List<Bone> thumbBones = new List<Bone>();

        /// <summary>
        /// Gets or sets whether hand mesh data should be read and converted.
        /// </summary>
        public static bool HandMeshingEnabled { get; set; }

        /// <summary>
        /// Gets updated hand data for the current frame.
        /// </summary>
        /// <returns>Platform agnostics hand data.</returns>
        public HandData GetHandData(InputDevice inputDevice)
        {
            if (!inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var wristCenter))
            {
                Debug.LogWarning($"Failed to get {nameof(CommonUsages.devicePosition)}");
            }

            if (!inputDevice.TryGetFeatureValue(MagicLeapHandUsages.NormalizedCenter, out var deviceNormalizedCenter))
            {
                Debug.LogWarning($"Failed to get {nameof(MagicLeapHandUsages.NormalizedCenter)}");
            }

            if (!inputDevice.TryGetFeatureValue(MagicLeapHandUsages.Confidence, out var deviceHandConfidence))
            {
                Debug.LogWarning($"Failed to get {nameof(MagicLeapHandUsages.Confidence)}");
            }

            var updatedHandData = new HandData
            {
                IsTracked = deviceHandConfidence > 0.8f,
                TimeStamp = DateTimeOffset.UtcNow.Ticks
            };

            if (updatedHandData.IsTracked)
            {
                UpdateHandJoints(inputDevice, updatedHandData.Joints);
                //UpdateHandMesh(inputDevice, updatedHandData.Mesh);
            }

            return updatedHandData;
        }

        private static void UpdateHandJoints(InputDevice inputDevice, MixedRealityPose[] jointPoses)
        {
            if (!inputDevice.TryGetFeatureValue(MagicLeapHandUsages.WristCenter, out var deviceWristCenter))
            {
                Debug.LogWarning($"Failed to get {nameof(MagicLeapHandUsages.WristCenter)}");
            }

            if (!inputDevice.TryGetFeatureValue(MagicLeapHandUsages.WristUlnar, out var deviceWristUlnar))
            {
                Debug.LogWarning($"Failed to get {nameof(MagicLeapHandUsages.WristUlnar)}");
            }

            if (!inputDevice.TryGetFeatureValue(MagicLeapHandUsages.WristRadial, out var deviceWristRadial))
            {
                Debug.LogWarning($"Failed to get {nameof(MagicLeapHandUsages.WristRadial)}");
            }

            if (!inputDevice.TryGetFeatureValue(CommonUsages.handData, out var deviceHand))
            {
                Debug.LogWarning($"Failed to get {nameof(CommonUsages.handData)}");
            }

            if (!deviceHand.TryGetFingerBones(HandFinger.Index, indexBones))
            {
                Debug.LogWarning($"Failed to get {nameof(indexBones)}");
            }

            if (!deviceHand.TryGetFingerBones(HandFinger.Middle, middleBones))
            {
                Debug.LogWarning($"Failed to get {nameof(middleBones)}");
            }

            if (!deviceHand.TryGetFingerBones(HandFinger.Ring, ringBones))
            {
                Debug.LogWarning($"Failed to get {nameof(ringBones)}");
            }

            if (!deviceHand.TryGetFingerBones(HandFinger.Pinky, pinkyBones))
            {
                Debug.LogWarning($"Failed to get {nameof(pinkyBones)}");
            }

            if (!deviceHand.TryGetFingerBones(HandFinger.Thumb, thumbBones))
            {
                Debug.LogWarning($"Failed to get {nameof(thumbBones)}");
            }

            for (int i = 0; i < jointPoses.Length; i++)
            {
                var trackedHandJoint = (TrackedHandJoint)i;

                switch (trackedHandJoint)
                {
                    // Wrist and Palm
                    case TrackedHandJoint.Wrist:
                        jointPoses[i].Position = deviceWristCenter;
                        break;
                    case TrackedHandJoint.Palm:
                        jointPoses[i].Position = Vector3.Lerp(deviceWristCenter, GetPose(ref middleBones, Joint.Mcp).Position, 0.5f);
                        break;
                    // Finger: Thumb
                    case TrackedHandJoint.ThumbMetacarpalJoint:
                        jointPoses[i] = GetPose(ref thumbBones, Joint.Mcp);
                        break;
                    case TrackedHandJoint.ThumbProximalJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.ThumbDistalJoint:
                        jointPoses[i] = GetPose(ref thumbBones, Joint.Pip);
                        break;
                    case TrackedHandJoint.ThumbTip:
                        jointPoses[i] = GetPose(ref thumbBones, Joint.Tip);
                        break;
                    // Finger: Index
                    case TrackedHandJoint.IndexMetacarpal:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.IndexKnuckle:
                        jointPoses[i] = GetPose(ref indexBones, Joint.Mcp);
                        break;
                    case TrackedHandJoint.IndexMiddleJoint:
                        jointPoses[i] = GetPose(ref indexBones, Joint.Pip);
                        break;
                    case TrackedHandJoint.IndexDistalJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.IndexTip:
                        jointPoses[i] = GetPose(ref indexBones, Joint.Tip);
                        break;
                    // Finger: Middle
                    case TrackedHandJoint.MiddleMetacarpal:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.MiddleKnuckle:
                        jointPoses[i] = GetPose(ref middleBones, Joint.Mcp);
                        break;
                    case TrackedHandJoint.MiddleMiddleJoint:
                        jointPoses[i] = GetPose(ref middleBones, Joint.Pip);
                        break;
                    case TrackedHandJoint.MiddleDistalJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.MiddleTip:
                        jointPoses[i] = GetPose(ref middleBones, Joint.Tip);
                        break;
                    // Finger: Ring
                    case TrackedHandJoint.RingMetacarpal:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.RingKnuckle:
                        jointPoses[i] = GetPose(ref ringBones, Joint.Mcp);
                        break;
                    case TrackedHandJoint.RingMiddleJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.RingDistalJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.RingTip:
                        jointPoses[i] = GetPose(ref ringBones, Joint.Tip);
                        break;
                    // Finger: Pinky
                    case TrackedHandJoint.PinkyMetacarpal:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.PinkyKnuckle:
                        jointPoses[i] = GetPose(ref pinkyBones, Joint.Mcp);
                        break;
                    case TrackedHandJoint.PinkyMiddleJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.PinkyDistalJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.PinkyTip:
                        jointPoses[i] = GetPose(ref pinkyBones, Joint.Tip);
                        break;
                }
            }
        }

        private static MixedRealityPose GetPose(ref List<Bone> bones, Joint joint)
        {
            bones[(int)joint].TryGetPosition(out var position);
            bones[(int)joint].TryGetRotation(out var rotation);
            return new MixedRealityPose(position, rotation);
        }

        //private void UpdateHandMesh(MLHandTracking.Hand hand, HandMeshData handMeshData)
        //{
        //    // TODO: Get hand mesh data and convert.
        //}
    }
}
