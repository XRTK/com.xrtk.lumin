// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Providers.Controllers.Hands;
using XRTK.Lumin.Profiles;
using UnityEngine;
using System;
using System.Linq;
using XRTK.Definitions.Utilities;

#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace XRTK.Lumin.Controllers
{
    /// <summary>
    /// Hand controller data provier for the Lumin platform.
    /// </summary>
    public class LuminHandControllerDataProvider : BaseHandControllerDataProvider
    {
        private readonly LuminHandControllerDataProviderProfile profile;
        private MLHandKeyPose[] keyPoses;

        /// <summary>
        /// Creates a new instance of the data provider.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Hand controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public LuminHandControllerDataProvider(string name, uint priority, LuminHandControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
            this.profile = profile;
        }

#if PLATFORM_LUMIN

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();
            keyPoses = Enum.GetValues(typeof(MLHandKeyPose)).Cast<MLHandKeyPose>().ToArray();
        }

        /// <inheritdoc />
        public override void Enable()
        {
            if (!profile.HandTrackingEnabled)
            {
                return;
            }

            if (!MLHands.IsStarted)
            {
                var result = MLHands.Start();
                if (!result.IsOk)
                {
                    Debug.LogError($"Error: Failed starting MLHands: {result}");
                    return;
                }

                bool status = MLHands.KeyPoseManager.EnableKeyPoses(keyPoses, true, true);
                if (!status)
                {
                    Debug.LogError("Error: Failed enabling tracked key poses.");
                    return;
                }

                MLHands.KeyPoseManager.SetKeyPointsFilterLevel(profile.KeyPointFilterLevel);
                MLHands.KeyPoseManager.SetPoseFilterLevel(profile.PoseFilterLevel);
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (MLHands.IsStarted)
            {
                MLHands.Stop();
                MLHands.KeyPoseManager.EnableKeyPoses(keyPoses, false, true);
            }

            base.Disable();
        }

        /// <inheritdoc />
        public override void Update()
        {
            if (profile.HandTrackingEnabled && MLHands.IsStarted)
            {
                UpdateHandController(Handedness.Left, MLHands.Left);
                UpdateHandController(Handedness.Right, MLHands.Right);
            }
        }

        private void UpdateHandController(Handedness handedness, MLHand hand)
        {
            if (hand.IsVisible)
            {
                // Hand is being tracked by the device, update controller using
                // current data, beginning with converting the magic leap hand data
                // to the XRTK generic hand data model.
                HandData updatedHandData = new HandData
                {
                    IsTracked = true,
                    TimeStamp = DateTimeOffset.UtcNow.Ticks
                };

                MLFinger pinky = hand.Pinky;
                MLFinger ring = hand.Ring;
                MLFinger middle = hand.Middle;
                MLFinger index = hand.Index;
                MLThumb thumb = hand.Thumb;
                MLWrist wrist = hand.Wrist;

                for (int i = 0; i < updatedHandData.Joints.Length; i++)
                {
                    MixedRealityPose jointPose = updatedHandData.Joints[i];
                    TrackedHandJoint trackedHandJoint = (TrackedHandJoint)i;
                    switch (trackedHandJoint)
                    {
                        case TrackedHandJoint.Wrist:
                            ConvertKeyPointToJointPose(wrist.Center, jointPose);
                            break;
                        case TrackedHandJoint.ThumbProximalJoint:
                            ConvertKeyPointToJointPose(thumb.MCP, jointPose);
                            break;
                        case TrackedHandJoint.ThumbDistalJoint:
                            ConvertKeyPointToJointPose(thumb.IP, jointPose);
                            break;
                        case TrackedHandJoint.ThumbTip:
                            ConvertKeyPointToJointPose(thumb.Tip, jointPose);
                            break;
                        case TrackedHandJoint.IndexKnuckle:
                            ConvertKeyPointToJointPose(index.MCP, jointPose);
                            break;
                        case TrackedHandJoint.IndexMiddleJoint:
                            ConvertKeyPointToJointPose(index.PIP, jointPose);
                            break;
                        case TrackedHandJoint.IndexTip:
                            ConvertKeyPointToJointPose(index.Tip, jointPose);
                            break;
                        case TrackedHandJoint.MiddleKnuckle:
                            ConvertKeyPointToJointPose(middle.MCP, jointPose);
                            break;
                        case TrackedHandJoint.MiddleMiddleJoint:
                            ConvertKeyPointToJointPose(middle.PIP, jointPose);
                            break;
                        case TrackedHandJoint.MiddleTip:
                            ConvertKeyPointToJointPose(middle.Tip, jointPose);
                            break;
                        case TrackedHandJoint.RingKnuckle:
                            ConvertKeyPointToJointPose(ring.MCP, jointPose);
                            break;
                        case TrackedHandJoint.RingTip:
                            ConvertKeyPointToJointPose(ring.Tip, jointPose);
                            break;
                        case TrackedHandJoint.PinkyKnuckle:
                            ConvertKeyPointToJointPose(pinky.MCP, jointPose);
                            break;
                        case TrackedHandJoint.PinkyTip:
                            ConvertKeyPointToJointPose(pinky.Tip, jointPose);
                            break;
                    }
                }

                // Update provider base implementation
                UpdateHandData(handedness, updatedHandData);
            }
            else
            {
                // Hand is currently not being tracked / lost
                UpdateHandData(handedness, new HandData
                {
                    IsTracked = false
                });
            }
        }

        private void ConvertKeyPointToJointPose(MLKeyPoint keyPoint, MixedRealityPose jointPose)
        {
            if (keyPoint.IsValid)
            {
                jointPose.Position = keyPoint.Position;
                // Joint rotation tracking is not supported on Lumin,
                // so we gotta live with Quaternion.identity here
                jointPose.Rotation = Quaternion.identity;
            }
        }

#endif
    }
}