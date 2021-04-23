// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.CameraSystem;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.CameraSystem;
using XRTK.Lumin.Native;
using XRTK.Providers.CameraSystem;

namespace XRTK.Lumin.Providers.CameraSystem
{
    [RuntimePlatform(typeof(LuminPlatform))]
    [System.Runtime.InteropServices.Guid("49E17DAC-C786-4B1A-A66A-54DA654923D5")]
    public class LuminCameraDataProvider : BaseCameraDataProvider
    {
        /// <inheritdoc />
        public LuminCameraDataProvider(string name, uint priority, BaseMixedRealityCameraDataProviderProfile profile, IMixedRealityCameraSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

        #region IMixedRealityCameraDataProvider Implementation

        /// <inheritdoc />
        public override bool IsOpaque => false;

        private float headHeight;

        /// <inheritdoc />
        public override float HeadHeight
        {
            get => headHeight;
            set
            {
                if (value.Equals(headHeight))
                {
                    return;
                }

                headHeight = value;
            }
        }

        /// <inheritdoc />
        protected override void ResetRigTransforms()
        {
            CameraRig.PlayspaceTransform.position = new Vector3(0f, HeadHeight, 0f);
            CameraRig.PlayspaceTransform.rotation = Quaternion.identity;
            CameraRig.CameraTransform.localPosition = Vector3.zero;
            CameraRig.CameraTransform.localRotation = Quaternion.identity;
            CameraRig.BodyTransform.localPosition = Vector3.zero;
            CameraRig.BodyTransform.localRotation = Quaternion.identity;
        }

        #endregion IMixedRealityCameraDataProvider Implementation

        private MlApi.MLHandle headTrackerHandle;
        private MlHeadTracking.MLHeadTrackingStaticData staticHeadData = new MlHeadTracking.MLHeadTrackingStaticData();
        private ulong rawMapEvents;
        private MlHeadTracking.MLHeadTrackingState headTrackingState;
        private MlPerception.MLPerceptionSettings perceptionSettings;
        private MlHeadTracking.MLHeadTrackingMapEvent lastMapEvent;
        private MlTypes.MLTransform headTransform;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (!Application.isPlaying) { return; }

            if (!MlPerception.MLPerceptionStartup(ref perceptionSettings).IsOk)
            {
                Debug.LogError($"Failed to start {nameof(MlPerception)}!");
            }

            if (!MlPerception.MLPerceptionInitSettings(ref perceptionSettings).IsOk)
            {
                Debug.LogError($"Failed to set {nameof(MlPerception)} settings!");
            }

            if (headTrackerHandle.IsValid) { return; }

            if (!MlHeadTracking.MLHeadTrackingCreate(ref headTrackerHandle).IsOk)
            {
                Debug.LogError($"Failed to start {nameof(MlHeadTracking)}!");
            }

            if (!MlHeadTracking.MLHeadTrackingGetStaticData(headTrackerHandle, ref staticHeadData).IsOk)
            {
                Debug.LogError($"Failed to get {nameof(MlHeadTracking)} static data!");
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (!Application.isPlaying) { return; }

            if (!headTrackerHandle.IsValid) { return; }

            if (MlHeadTracking.MLHeadTrackingGetMapEvents(headTrackerHandle, ref rawMapEvents).IsOk)
            {
                if (rawMapEvents > 0)
                {
                    var mapEvents = (MlHeadTracking.MLHeadTrackingMapEvent)rawMapEvents;

                    if (lastMapEvent != mapEvents)
                    {
                        Debug.Log(mapEvents);
                        lastMapEvent = mapEvents;
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to get head tracking map events");
            }

            if (!MlHeadTracking.MLHeadTrackingGetState(headTrackerHandle, ref headTrackingState).IsOk)
            {
                Debug.LogError($"Failed to get head tracking state");
            }

            if (!MlPerception.MLPerceptionGetSnapshot(out var snapshot).IsOk)
            {
                Debug.LogError("Failed to get perception snapshot!");
            }

            if (MlSnapshot.MLSnapshotGetTransform(snapshot, staticHeadData.coord_frame_head, ref headTransform).IsOk)
            {
                CameraRig.CameraTransform.localPosition = headTransform.position;
                CameraRig.CameraTransform.localRotation = headTransform.rotation;
            }

            if (!MlPerception.MLPerceptionReleaseSnapshot(snapshot).IsOk)
            {
                Debug.LogError("Failed to release perception snapshot!");
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            if (!Application.isPlaying) { return; }

            if (headTrackerHandle.IsValid)
            {
                if (!MlHeadTracking.MLHeadTrackingDestroy(headTrackerHandle).IsOk)
                {
                    Debug.LogError($"Failed to destroy {nameof(MlHeadTracking)}!");
                }
            }

            if (!MlPerception.MLPerceptionShutdown().IsOk)
            {
                Debug.LogError($"Failed to shutdown {nameof(MlPerception)}!");
            }
        }
    }
}