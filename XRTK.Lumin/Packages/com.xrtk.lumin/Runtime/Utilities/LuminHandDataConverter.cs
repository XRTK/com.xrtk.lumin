// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;

#if PLATFORM_LUMIN

using System;
using UnityEngine;

#endif // PLATFORM_LUMIN

namespace XRTK.Lumin.Utilities
{
    /// <summary>
    /// Converts oculus hand data to <see cref="HandData"/>.
    /// </summary>
    public sealed class LuminHandDataConverter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handedness">Handedness of the hand this converter is created for.</param>
        public LuminHandDataConverter(Handedness handedness)
        {
            this.handedness = handedness;
        }

        private readonly Handedness handedness;

#if PLATFORM_LUMIN

        /// <summary>
        /// Gets or sets whether hand mesh data should be read and converted.
        /// </summary>
        public static bool HandMeshingEnabled { get; set; }

        /// <summary>
        /// Gets updated hand data for the current frame.
        /// </summary>
        /// <returns>Platform agnostics hand data.</returns>
        public HandData GetHandData()
        {
            var hand = ToMagicLeapHand(handedness);
            var updatedHandData = new HandData
            {
                IsTracked = hand.IsVisible,
                TimeStamp = DateTimeOffset.UtcNow.Ticks
            };

            if (updatedHandData.IsTracked)
            {
                UpdateHandJoints(hand, updatedHandData.Joints);
                UpdateHandMesh(hand, updatedHandData.Mesh);
            }

            PostProcess(updatedHandData);
            return updatedHandData;
        }

        /// <summary>
        /// Gets the magic leap hand reference for the given handedness.
        /// </summary>
        /// <param name="handedness">Handedness to convert.</param>
        /// <returns>Magic Leap hand reference.</returns>
        private static MLHand ToMagicLeapHand(Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.Left:
                    return MLHands.Left;
                case Handedness.Right:
                    return MLHands.Right;
                default:
                    return null;
            }
        }

        private static void UpdateHandJoints(MLHand hand, MixedRealityPose[] jointPoses)
        {
            var ring = hand.Ring;
            var pinky = hand.Pinky;
            var index = hand.Index;
            var thumb = hand.Thumb;
            var wrist = hand.Wrist;
            var middle = hand.Middle;

            for (int i = 0; i < jointPoses.Length; i++)
            {
                var trackedHandJoint = (TrackedHandJoint)i;

                switch (trackedHandJoint)
                {
                    // Wrist and Palm
                    case TrackedHandJoint.Wrist:
                        jointPoses[i] = ComputeJointPose(wrist.Center);
                        break;
                    case TrackedHandJoint.Palm:
                        jointPoses[i] = EstimatePalmPose(wrist.Center, middle.MCP);
                        break;
                    // Finger: Thumb
                    case TrackedHandJoint.ThumbMetacarpalJoint:
                        jointPoses[i] = ComputeJointPose(thumb.MCP);
                        break;
                    case TrackedHandJoint.ThumbProximalJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.ThumbDistalJoint:
                        jointPoses[i] = ComputeJointPose(thumb.IP);
                        break;
                    case TrackedHandJoint.ThumbTip:
                        jointPoses[i] = ComputeJointPose(thumb.Tip);
                        break;
                    // Finger: Index
                    case TrackedHandJoint.IndexMetacarpal:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.IndexKnuckle:
                        jointPoses[i] = ComputeJointPose(index.MCP);
                        break;
                    case TrackedHandJoint.IndexMiddleJoint:
                        jointPoses[i] = ComputeJointPose(index.PIP);
                        break;
                    case TrackedHandJoint.IndexDistalJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.IndexTip:
                        jointPoses[i] = ComputeJointPose(index.Tip);
                        break;
                    // Finger: Middle
                    case TrackedHandJoint.MiddleMetacarpal:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.MiddleKnuckle:
                        jointPoses[i] = ComputeJointPose(middle.MCP);
                        break;
                    case TrackedHandJoint.MiddleMiddleJoint:
                        jointPoses[i] = ComputeJointPose(middle.PIP);
                        break;
                    case TrackedHandJoint.MiddleDistalJoint:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.MiddleTip:
                        jointPoses[i] = ComputeJointPose(middle.Tip);
                        break;
                    // Finger: Ring
                    case TrackedHandJoint.RingMetacarpal:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.RingKnuckle:
                        jointPoses[i] = ComputeJointPose(ring.MCP);
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
                        jointPoses[i] = ComputeJointPose(ring.Tip);
                        break;
                    // Finger: Pinky
                    case TrackedHandJoint.PinkyMetacarpal:
                        // TODO: Estimate?
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.PinkyKnuckle:
                        jointPoses[i] = ComputeJointPose(pinky.MCP);
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
                        jointPoses[i] = ComputeJointPose(pinky.Tip);
                        break;
                }
            }
        }

        private void UpdateHandMesh(MLHand hand, HandMeshData handMeshData)
        {
            // TODO: Get hand mesh data and convert.
        }

        private static MixedRealityPose EstimatePalmPose(MLKeyPoint wrist, MLKeyPoint middleDistal)
        {
            var wristRootPose = ComputeJointPose(wrist);
            var middleDistalPose = ComputeJointPose(middleDistal);
            var palmPosition = Vector3.Lerp(wristRootPose.Position, middleDistalPose.Position, .5f);
            var palmRotation = wristRootPose.Rotation;

            return new MixedRealityPose(palmPosition, palmRotation);
        }

        private static MixedRealityPose ComputeJointPose(MLKeyPoint keyPoint)
        {
            var pose = MixedRealityPose.ZeroIdentity;

            if (keyPoint.IsValid)
            {
                pose.Position = keyPoint.Position;

                // Joint rotation tracking is not supported on Lumin,
                // so we gotta live with Quaternion.identity here.
            }

            return pose;
        }

#endif // PLATFORM_LUMIN
    }
}