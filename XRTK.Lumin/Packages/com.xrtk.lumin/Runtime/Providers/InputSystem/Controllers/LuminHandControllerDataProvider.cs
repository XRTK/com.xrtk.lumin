// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.InputSystem;
using XRTK.Lumin.Profiles;
using XRTK.Providers.Controllers.Hands;

#if PLATFORM_LUMIN

using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Services;

#endif // PLATFORM_LUMIN

namespace XRTK.Lumin.Providers.InputSystem.Controllers
{
    [RuntimePlatform(typeof(LuminPlatform))]
    [System.Runtime.InteropServices.Guid("9511D571-2E24-41EF-AA7E-DF4432617DE1")]
    public class LuminHandControllerDataProvider : BaseHandControllerDataProvider
    {
        /// <inheritdoc />
        public LuminHandControllerDataProvider(string name, uint priority, LuminHandControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
#if PLATFORM_LUMIN
            keyPointFilterLevel = (MLKeyPointFilterLevel)profile.KeyPointFilterLevel;
            poseFilterLevel = (MLPoseFilterLevel)profile.PoseFilterLevel;

            leftHandConverter = new LuminHandDataConverter(Handedness.Left, TrackedPoses);
            rightHandConverter = new LuminHandDataConverter(Handedness.Right, TrackedPoses);
        }

        private readonly MLPoseFilterLevel poseFilterLevel;
        private readonly MLKeyPointFilterLevel keyPointFilterLevel;
        private readonly LuminHandDataConverter leftHandConverter;
        private readonly LuminHandDataConverter rightHandConverter;
        private readonly MLHandKeyPose[] keyPoses = Enum.GetValues(typeof(MLHandKeyPose)).Cast<MLHandKeyPose>().ToArray();
        private readonly Dictionary<Handedness, MixedRealityHandController> activeControllers = new Dictionary<Handedness, MixedRealityHandController>();

        private bool isEnabled = false;

        /// <inheritdoc />
        public override void Enable()
        {
            if (!MLHands.IsStarted)
            {
                var result = MLHands.Start();

                if (!result.IsOk)
                {
                    Debug.LogError($"Error: Failed starting MLHands: {result}");
                    return;
                }

                isEnabled = true;
            }

            if (!MLHands.KeyPoseManager.EnableKeyPoses(keyPoses, true, true))
            {
                Debug.LogError($"Error: Failed {nameof(MLHands.KeyPoseManager.EnableKeyPoses)}.");
            }

            if (!MLHands.KeyPoseManager.SetKeyPointsFilterLevel(keyPointFilterLevel))
            {
                Debug.LogError($"Error: Failed {nameof(MLHands.KeyPoseManager.SetKeyPointsFilterLevel)}.");
            }

            if (!MLHands.KeyPoseManager.SetPoseFilterLevel(poseFilterLevel))
            {
                Debug.LogError($"Error: Failed {nameof(MLHands.KeyPoseManager.SetPoseFilterLevel)}.");
            }

            LuminHandDataConverter.HandMeshingEnabled = HandMeshingEnabled;
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (isEnabled)
            {
                GetOrAddController(Handedness.Left).UpdateController(leftHandConverter.GetHandData());
                GetOrAddController(Handedness.Right).UpdateController(rightHandConverter.GetHandData());
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (isEnabled)
            {
                MLHands.KeyPoseManager.DisableAllKeyPoses();
                MLHands.Stop();
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
#endif // PLATFORM_LUMIN
        }
    }
}