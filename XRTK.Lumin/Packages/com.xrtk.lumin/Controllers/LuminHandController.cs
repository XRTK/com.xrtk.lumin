// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Providers.Controllers.Hands;
using UnityEngine;

#if PLATFORM_LUMIN
using System;
using UnityEngine.XR.MagicLeap;
#endif

namespace XRTK.Lumin.Controllers
{
    /// <summary>
    /// The default hand controller implementation for the Lumin platform.
    /// </summary>
    public class LuminHandController : BaseHandController
    {
        public LuminHandController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

#if PLATFORM_LUMIN

        public override void UpdateController()
        {
            base.UpdateController();

            MLHand hand = GetHand(ControllerHandedness);
            HandData updatedHandData = new HandData
            {
                IsTracked = hand.IsVisible,
                TimeStamp = DateTimeOffset.UtcNow.Ticks
            };

            if (updatedHandData.IsTracked)
            {
                UpdateHandJoints(hand, updatedHandData.Joints);
                UpdateHandMesh(hand, updatedHandData.Mesh);
            }

            UpdateBase(updatedHandData);
        }

        private void UpdateHandJoints(MLHand hand, MixedRealityPose[] jointPoses)
        {
            MLFinger pinky = hand.Pinky;
            MLFinger ring = hand.Ring;
            MLFinger middle = hand.Middle;
            MLFinger index = hand.Index;
            MLThumb thumb = hand.Thumb;
            MLWrist wrist = hand.Wrist;

            for (int i = 0; i < jointPoses.Length; i++)
            {
                TrackedHandJoint trackedHandJoint = (TrackedHandJoint)i;
                switch (trackedHandJoint)
                {
                    // Wrist and Palm
                    case TrackedHandJoint.Wrist:
                        jointPoses[i] = ConvertKeyPointToJointPose(wrist.Center);
                        break;
                    case TrackedHandJoint.Palm:
                        jointPoses[i] = ComputePalmPose(wrist.Center, middle.DIP);
                        break;
                    // Finger: Thumb
                    case TrackedHandJoint.ThumbMetacarpalJoint:
                        jointPoses[i] = ConvertKeyPointToJointPose(thumb.MCP);
                        break;
                    case TrackedHandJoint.ThumbProximalJoint:
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.ThumbDistalJoint:
                        jointPoses[i] = ConvertKeyPointToJointPose(thumb.IP);
                        break;
                    case TrackedHandJoint.ThumbTip:
                        jointPoses[i] = ConvertKeyPointToJointPose(thumb.Tip);
                        break;
                    // Finger: Index
                    case TrackedHandJoint.IndexKnuckle:
                        jointPoses[i] = ConvertKeyPointToJointPose(index.MCP);
                        break;
                    case TrackedHandJoint.IndexMiddleJoint:
                        jointPoses[i] = ConvertKeyPointToJointPose(index.PIP);
                        break;
                    case TrackedHandJoint.IndexDistalJoint:
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.IndexTip:
                        jointPoses[i] = ConvertKeyPointToJointPose(index.Tip);
                        break;
                    // Finger: Middle
                    case TrackedHandJoint.MiddleKnuckle:
                        jointPoses[i] = ConvertKeyPointToJointPose(middle.MCP);
                        break;
                    case TrackedHandJoint.MiddleMiddleJoint:
                        jointPoses[i] = ConvertKeyPointToJointPose(middle.PIP);
                        break;
                    case TrackedHandJoint.MiddleDistalJoint:
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.MiddleTip:
                        jointPoses[i] = ConvertKeyPointToJointPose(middle.Tip);
                        break;
                    // Finger: Ring
                    case TrackedHandJoint.RingKnuckle:
                        jointPoses[i] = ConvertKeyPointToJointPose(ring.MCP);
                        break;
                    case TrackedHandJoint.RingMiddleJoint:
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.RingDistalJoint:
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.RingTip:
                        jointPoses[i] = ConvertKeyPointToJointPose(ring.Tip);
                        break;
                    // Finger: Pinky
                    case TrackedHandJoint.PinkyKnuckle:
                        jointPoses[i] = ConvertKeyPointToJointPose(pinky.MCP);
                        break;
                    case TrackedHandJoint.PinkyMiddleJoint:
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.PinkyDistalJoint:
                        jointPoses[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.PinkyTip:
                        jointPoses[i] = ConvertKeyPointToJointPose(pinky.Tip);
                        break;
                }
            }
        }

        private void UpdateHandMesh(MLHand hand, HandMeshData handMeshData)
        {
            // TODO: Get hand mesh data and convert.
        }

        private MixedRealityPose ComputePalmPose(MLKeyPoint wrist, MLKeyPoint middleDistal)
        {
            MixedRealityPose wristRootPose = ConvertKeyPointToJointPose(wrist);
            MixedRealityPose middleDistalPose = ConvertKeyPointToJointPose(middleDistal);
            Vector3 palmPosition = Vector3.Lerp(wristRootPose.Position, middleDistalPose.Position, .5f);
            Quaternion palmRotation = wristRootPose.Rotation;

            return new MixedRealityPose(palmPosition, palmRotation);
        }

        private MixedRealityPose ConvertKeyPointToJointPose(MLKeyPoint keyPoint)
        {
            MixedRealityPose pose = MixedRealityPose.ZeroIdentity;

            if (keyPoint.IsValid)
            {
                pose.Position = keyPoint.Position;

                // Joint rotation tracking is not supported on Lumin,
                // so we gotta live with Quaternion.identity here.
            }

            return pose;
        }

        private MLHand GetHand(Handedness handedness)
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

#endif
    }
}