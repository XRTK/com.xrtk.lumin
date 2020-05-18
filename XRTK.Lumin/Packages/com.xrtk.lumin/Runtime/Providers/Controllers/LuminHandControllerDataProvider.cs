// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Platforms;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Lumin.Native;
using XRTK.Lumin.Profiles;
using XRTK.Lumin.Utilities;
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
            LuminHandDataConverter.HandMeshingEnabled = profile.HandMeshingEnabled;
        }

        private readonly MLPoseFilterLevel poseFilterLevel;
        private readonly MLKeyPointFilterLevel keyPointFilterLevel;
        private readonly LuminHandDataConverter leftHandConverter = new LuminHandDataConverter(Handedness.Left);
        private readonly LuminHandDataConverter rightHandConverter = new LuminHandDataConverter(Handedness.Right);
        private readonly Dictionary<Handedness, MixedRealityHandController> activeControllers = new Dictionary<Handedness, MixedRealityHandController>();

        private MlApi.MLHandle handTrackingHandle = MlApi.MLHandle.Default;
        private MlHandTracking.MLHandTrackingConfiguration configuration = new MlHandTracking.MLHandTrackingConfiguration();
        private MlHandTracking.MLHandTrackingDataEx handTrackingDataEx = new MlHandTracking.MLHandTrackingDataEx();
        private MlHandTracking.MLHandTrackingStaticData staticHandTrackingData;
        private MlHandTracking.MLHandTrackingData handTrackingData;

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

                if (MlHandTracking.MLHandTrackingGetConfiguration(handTrackingHandle, ref configuration).IsOk)
                {
                    configuration.key_points_filter_level = keyPointFilterLevel;
                    configuration.pose_filter_level = poseFilterLevel;
                    configuration.handtracking_pipeline_enabled = true;

                    if (!MlHandTracking.MLHandTrackingSetConfiguration(handTrackingHandle, ref configuration).IsOk)
                    {
                        Debug.LogError($"Failed to set {nameof(MlHandTracking.MLHandTrackingConfiguration)}:{configuration}!");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to get {nameof(MlHandTracking.MLHandTrackingConfiguration)}:{configuration}!");
                }

                if (!MlHandTracking.MLHandTrackingGetStaticData(handTrackingHandle, ref staticHandTrackingData).IsOk)
                {
                    Debug.LogError($"{nameof(MlHandTracking.MLHandTrackingGetStaticData)} Failed!");
                }

                //if (!MlHandTracking.MLHandTrackingDataExInit(ref handTrackingDataEx).IsOk)
                //{
                //    Debug.LogError($"{nameof(MlHandTracking.MLHandTrackingDataExInit)} Failed!");
                //}
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (!Application.isPlaying) { return; }

            if (handTrackingHandle.IsValid)
            {
                if (MlHandTracking.MLHandTrackingGetDataEx(handTrackingHandle, ref handTrackingDataEx).IsOk)
                {
                    Debug.Log(handTrackingDataEx);
                    // GetOrAddController(Handedness.Left).UpdateController(leftHandConverter.GetHandData());
                    // GetOrAddController(Handedness.Right).UpdateController(rightHandConverter.GetHandData());
                }
                else
                {
                    Debug.LogError($"{nameof(MlHandTracking.MLHandTrackingGetDataEx)} Failed!");
                }
            }
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
    }
}