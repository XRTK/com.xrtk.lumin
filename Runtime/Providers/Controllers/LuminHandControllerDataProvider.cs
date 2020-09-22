// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Platforms;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Lumin.Native;
using XRTK.Lumin.Profiles;
using XRTK.Providers.Controllers.Hands;
using XRTK.Services;

namespace XRTK.Lumin.Providers.Controllers
{
    [RuntimePlatform(typeof(LuminPlatform))]
    [System.Runtime.InteropServices.Guid("9511D571-2E24-41EF-AA7E-DF4432617DE1")]
    public class LuminHandControllerDataProvider : BaseHandControllerDataProvider
    {
        /// <inheritdoc />
        public LuminHandControllerDataProvider(string name, uint priority, LuminHandControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
            keyPointFilterLevel = profile.KeyPointFilterLevel;
            poseFilterLevel = profile.PoseFilterLevel;
        }

        private readonly MLPoseFilterLevel poseFilterLevel;
        private readonly MLKeyPointFilterLevel keyPointFilterLevel;
        private readonly MixedRealityPose[] tempLeftKeyPoses = new MixedRealityPose[24];
        private readonly MixedRealityPose[] tempRightKeyPoses = new MixedRealityPose[24];
        private readonly Dictionary<Handedness, MixedRealityHandController> activeControllers = new Dictionary<Handedness, MixedRealityHandController>();

        private MlApi.MLHandle handTrackingHandle;
        private MlHandTracking.MLHandTrackingConfiguration configuration = new MlHandTracking.MLHandTrackingConfiguration();
        private MlHandTracking.MLHandTrackingDataEx handTrackingDataEx = new MlHandTracking.MLHandTrackingDataEx();
        private MlHandTracking.MLHandTrackingStaticData staticHandTrackingData;
        private MlTypes.MLTransform tempTransform = new MlTypes.MLTransform(new MlTypes.MLVec3f(), MlTypes.MLQuaternionf.Identity());
        private readonly MixedRealityPose poseIdentity = new MixedRealityPose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1f));

        /// <inheritdoc />
        public override void Initialize()
        {
            if (!Application.isPlaying) { return; }

            if (!handTrackingHandle.IsValid)
            {
                if (!MlHandTracking.MLHandTrackingCreate(ref handTrackingHandle).IsOk)
                {
                    Debug.LogError($"Failed to start {nameof(MlHandTracking)}!");
                    return;
                }

                configuration.keypose_enable_finger = false;
                configuration.keypose_enable_fist = false;
                configuration.keypose_enable_pinch = false;
                configuration.keypose_enable_thumb = false;
                configuration.keypose_enable_l = false;
                configuration.keypose_enable_open_hand = false;
                configuration.keypose_enable_ok = false;
                configuration.keypose_enable_c = false;
                configuration.keypose_enable_no_pose = true;
                configuration.keypose_enable_no_hand = true;
                configuration.handtracking_pipeline_enabled = true;
                configuration.key_points_filter_level = keyPointFilterLevel;
                configuration.pose_filter_level = poseFilterLevel;

                if (MlHandTracking.MLHandTrackingSetConfiguration(handTrackingHandle, ref configuration).IsOk)
                {
                    if (!MlHandTracking.MLHandTrackingGetConfiguration(handTrackingHandle, ref configuration).IsOk)
                    {
                        Debug.LogError($"Failed to get {nameof(MlHandTracking.MLHandTrackingConfiguration)}:{configuration}!");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to set {nameof(MlHandTracking.MLHandTrackingConfiguration)}:{configuration}!");
                }
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (!Application.isPlaying) { return; }

            if (handTrackingHandle.IsValid)
            {
                var leftHandData = new HandData();
                var rightHandData = new HandData();

                if (MlHandTracking.MLHandTrackingGetDataEx(handTrackingHandle, ref handTrackingDataEx).IsOk)
                {
                    leftHandData.TrackingState = handTrackingDataEx.left_hand_state.keypose < MlHandTracking.MLHandTrackingKeyPose.NoHand && !handTrackingDataEx.left_hand_state.is_holding_control ? TrackingState.Tracked : TrackingState.NotTracked;
                    rightHandData.TrackingState = handTrackingDataEx.right_hand_state.keypose < MlHandTracking.MLHandTrackingKeyPose.NoHand && !handTrackingDataEx.right_hand_state.is_holding_control ? TrackingState.Tracked : TrackingState.NotTracked;
                }
                else
                {
                    Debug.LogError($"{nameof(MlHandTracking.MLHandTrackingGetDataEx)} Failed!");
                }

                if (leftHandData.TrackingState == TrackingState.Tracked || rightHandData.TrackingState == TrackingState.Tracked)
                {
                    if (MlHandTracking.MLHandTrackingGetStaticData(handTrackingHandle, ref staticHandTrackingData).IsOk)
                    {
                        var now = DateTimeOffset.UtcNow.Ticks;

                        if (!MlPerception.MLPerceptionGetSnapshot(out var snapshot).IsOk)
                        {
                            Debug.LogError($"{nameof(MlPerception.MLPerceptionGetSnapshot)} Failed!");
                            return;
                        }

                        if (leftHandData.TrackingState == TrackingState.Tracked)
                        {
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Hand_Center] = GetPoseData(ref staticHandTrackingData.left.hand_center, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Wrist_Center] = GetPoseData(ref staticHandTrackingData.left.wrist.center, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Wrist_Radial] = GetPoseData(ref staticHandTrackingData.left.wrist.radial, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Wrist_Ulnar] = GetPoseData(ref staticHandTrackingData.left.wrist.ulnar, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_Tip] = GetPoseData(ref staticHandTrackingData.left.index.tip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_DIP] = GetPoseData(ref staticHandTrackingData.left.index.dip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_PIP] = GetPoseData(ref staticHandTrackingData.left.index.pip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_MCP] = GetPoseData(ref staticHandTrackingData.left.index.mcp, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_Tip] = GetPoseData(ref staticHandTrackingData.left.middle.tip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_DIP] = GetPoseData(ref staticHandTrackingData.left.middle.dip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_PIP] = GetPoseData(ref staticHandTrackingData.left.middle.pip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_MCP] = GetPoseData(ref staticHandTrackingData.left.middle.mcp, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_Tip] = GetPoseData(ref staticHandTrackingData.left.ring.tip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_DIP] = GetPoseData(ref staticHandTrackingData.left.ring.dip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_PIP] = GetPoseData(ref staticHandTrackingData.left.ring.pip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_MCP] = GetPoseData(ref staticHandTrackingData.left.ring.mcp, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_Tip] = GetPoseData(ref staticHandTrackingData.left.pinky.tip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_DIP] = GetPoseData(ref staticHandTrackingData.left.pinky.dip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_PIP] = GetPoseData(ref staticHandTrackingData.left.pinky.pip, ref snapshot);
                            tempLeftKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_MCP] = GetPoseData(ref staticHandTrackingData.left.pinky.mcp, ref snapshot);

                            leftHandData = SyncHandPoseData(leftHandData, Handedness.Left);
                            leftHandData.UpdatedAt = now;
                        }

                        if (rightHandData.TrackingState == TrackingState.Tracked)
                        {
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Hand_Center] = GetPoseData(ref staticHandTrackingData.right.hand_center, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Wrist_Center] = GetPoseData(ref staticHandTrackingData.right.wrist.center, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Wrist_Radial] = GetPoseData(ref staticHandTrackingData.right.wrist.radial, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Wrist_Ulnar] = GetPoseData(ref staticHandTrackingData.right.wrist.ulnar, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_Tip] = GetPoseData(ref staticHandTrackingData.right.index.tip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_DIP] = GetPoseData(ref staticHandTrackingData.right.index.dip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_PIP] = GetPoseData(ref staticHandTrackingData.right.index.pip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_MCP] = GetPoseData(ref staticHandTrackingData.right.index.mcp, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_Tip] = GetPoseData(ref staticHandTrackingData.right.middle.tip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_DIP] = GetPoseData(ref staticHandTrackingData.right.middle.dip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_PIP] = GetPoseData(ref staticHandTrackingData.right.middle.pip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_MCP] = GetPoseData(ref staticHandTrackingData.right.middle.mcp, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_Tip] = GetPoseData(ref staticHandTrackingData.right.ring.tip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_DIP] = GetPoseData(ref staticHandTrackingData.right.ring.dip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_PIP] = GetPoseData(ref staticHandTrackingData.right.ring.pip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_MCP] = GetPoseData(ref staticHandTrackingData.right.ring.mcp, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_Tip] = GetPoseData(ref staticHandTrackingData.right.pinky.tip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_DIP] = GetPoseData(ref staticHandTrackingData.right.pinky.dip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_PIP] = GetPoseData(ref staticHandTrackingData.right.pinky.pip, ref snapshot);
                            tempRightKeyPoses[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_MCP] = GetPoseData(ref staticHandTrackingData.right.pinky.mcp, ref snapshot);

                            rightHandData = SyncHandPoseData(rightHandData, Handedness.Right);
                            rightHandData.UpdatedAt = now;
                        }

                        if (!MlPerception.MLPerceptionReleaseSnapshot(snapshot).IsOk)
                        {
                            Debug.LogError($"{nameof(MlPerception.MLPerceptionReleaseSnapshot)} Failed!");
                        }
                    }
                    else
                    {
                        Debug.LogError($"{nameof(MlHandTracking.MLHandTrackingGetStaticData)} Failed!");
                    }
                }

                GetOrAddController(Handedness.Left).UpdateController(leftHandData);
                GetOrAddController(Handedness.Right).UpdateController(rightHandData);
            }
        }

        private MixedRealityPose GetPoseData(ref MlHandTracking.MLKeyPointState keyPoint, ref MlSnapshot.MLSnapshot snapshot)
        {
            var pose = poseIdentity;

            if (keyPoint.is_valid)
            {
                if (MlSnapshot.MLSnapshotGetTransform(snapshot, keyPoint.frame_id, ref tempTransform).IsOk)
                {
                    pose.Position = (Vector3)tempTransform.position;
                }
                else
                {
                    pose = poseIdentity;
                    Debug.LogError($"{nameof(MlSnapshot.MLSnapshotGetTransform)} Failed!");
                }
            }
            else
            {
                pose = poseIdentity;
            }

            return pose;
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            if (!Application.isPlaying) { return; }

            if (handTrackingHandle.IsValid)
            {
                if (!MlHandTracking.MLHandTrackingDestroy(handTrackingHandle).IsOk)
                {
                    Debug.LogError($"Failed to destroy {nameof(MlHandTracking)}!");
                }
            }

            foreach (var activeController in activeControllers)
            {
                RemoveController(activeController.Key, false);
            }

            activeControllers.Clear();
        }

        private MixedRealityHandController GetOrAddController(Handedness handedness)
        {
            // If a device is already registered with the handedness, just return it.
            if (TryGetController(handedness, out var existingController))
            {
                return existingController;
            }

            MixedRealityHandController detectedController;

            try
            {
                detectedController = new MixedRealityHandController(this, TrackingState.Tracked, handedness, GetControllerMappingProfile(typeof(MixedRealityHandController), handedness));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(MixedRealityHandController)}!\n{e}");
                return null;
            }

            detectedController.TryRenderControllerModel();

            activeControllers.Add(handedness, detectedController);
            AddController(detectedController);
            MixedRealityToolkit.InputSystem?.RaiseSourceDetected(detectedController.InputSource, detectedController);

            return detectedController;
        }

        private void RemoveController(Handedness handedness, bool removeFromRegistry = true)
        {
            if (TryGetController(handedness, out var controller))
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);

                if (removeFromRegistry)
                {
                    RemoveController(controller);
                    activeControllers.Remove(handedness);
                }
            }
        }

        private bool TryGetController(Handedness handedness, out MixedRealityHandController controller)
        {
            if (activeControllers.ContainsKey(handedness))
            {
                var existingController = activeControllers[handedness];
                Debug.Assert(existingController != null, $"Hand Controller {handedness} has been destroyed but remains in the active controller registry.");
                controller = existingController;
                return true;
            }

            controller = null;
            return false;
        }

        private HandData SyncHandPoseData(HandData handData, Handedness handedness)
        {
            var jointData = handedness == Handedness.Left ? tempLeftKeyPoses : tempRightKeyPoses;

            for (int i = 0; i < HandData.JointCount; i++)
            {
                var trackedHandJoint = (TrackedHandJoint)i;

                switch (trackedHandJoint)
                {
                    // Wrist and Palm
                    case TrackedHandJoint.Wrist:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Wrist_Center];
                        break;
                    case TrackedHandJoint.Palm:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Hand_Center];
                        break;
                    // Finger: Thumb
                    case TrackedHandJoint.ThumbMetacarpal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Thumb_MCP];
                        break;
                    case TrackedHandJoint.ThumbProximal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Thumb_CMC];
                        break;
                    case TrackedHandJoint.ThumbDistal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Thumb_IP];
                        break;
                    case TrackedHandJoint.ThumbTip:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Thumb_Tip];
                        break;
                    // Finger: Index
                    case TrackedHandJoint.IndexMetacarpal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_MCP];
                        break;
                    case TrackedHandJoint.IndexProximal:
                        handData.Joints[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.IndexIntermediate:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_PIP];
                        break;
                    case TrackedHandJoint.IndexDistal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_DIP];
                        break;
                    case TrackedHandJoint.IndexTip:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Index_Tip];
                        break;
                    // Finger: Middle
                    case TrackedHandJoint.MiddleMetacarpal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_MCP];
                        break;
                    case TrackedHandJoint.MiddleProximal:
                        // TODO: Estimate?
                        handData.Joints[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.MiddleIntermediate:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_PIP];
                        break;
                    case TrackedHandJoint.MiddleDistal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_DIP];
                        break;
                    case TrackedHandJoint.MiddleTip:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_Tip];
                        break;
                    // Finger: Ring
                    case TrackedHandJoint.RingMetacarpal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_MCP];
                        break;
                    case TrackedHandJoint.RingProximal:
                        // TODO: Estimate?
                        handData.Joints[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.RingIntermediate:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_PIP];
                        break;
                    case TrackedHandJoint.RingDistal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Ring_DIP];
                        break;
                    case TrackedHandJoint.RingTip:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Middle_Tip];
                        break;
                    // Finger: Pinky
                    case TrackedHandJoint.LittleMetacarpal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_MCP];
                        break;
                    case TrackedHandJoint.LittleProximal:
                        // TODO: Estimate?
                        handData.Joints[i] = MixedRealityPose.ZeroIdentity;
                        break;
                    case TrackedHandJoint.LittleIntermediate:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_PIP];
                        break;
                    case TrackedHandJoint.LittleDistal:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_DIP];
                        break;
                    case TrackedHandJoint.LittleTip:
                        handData.Joints[i] = jointData[(int)MlHandTracking.MLHandTrackingKeyPoint.Pinky_Tip];
                        break;
                }
            }

            return handData;
        }
    }
}