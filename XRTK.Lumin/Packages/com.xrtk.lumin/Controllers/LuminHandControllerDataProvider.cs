// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Interfaces.InputSystem;
using XRTK.Lumin.Profiles;
using XRTK.Providers.Controllers.Hands;

#if PLATFORM_LUMIN

using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Lumin.Utilities;
using XRTK.Services;

#endif // PLATFORM_LUMIN

namespace XRTK.Lumin.Controllers
{
    public class LuminHandControllerDataProvider : BaseHandControllerDataProvider
    {
        /// <inheritdoc />
        public LuminHandControllerDataProvider(string name, uint priority, LuminHandControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
#if PLATFORM_LUMIN
            keyPointFilterLevel = (MLKeyPointFilterLevel)profile.KeyPointFilterLevel;
            poseFilterLevel = (MLPoseFilterLevel)profile.PoseFilterLevel;
        }

        private readonly MLPoseFilterLevel poseFilterLevel;
        private readonly MLKeyPointFilterLevel keyPointFilterLevel;
        private readonly LuminHandDataConverter leftHandConverter = new LuminHandDataConverter(Handedness.Left);
        private readonly LuminHandDataConverter rightHandConverter = new LuminHandDataConverter(Handedness.Right);
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

            var controllerType = typeof(MixedRealityHandController);
            var pointers = RequestPointers(controllerType, handedness, true);
            var inputSource = MixedRealityToolkit.InputSystem.RequestNewGenericInputSource($"{handedness} Hand Controller", pointers);
            var detectedController = new MixedRealityHandController(this, TrackingState.Tracked, handedness, inputSource);

            if (!detectedController.SetupConfiguration(controllerType))
            {
                // Controller failed to be setup correctly.
                // Return null so we don't raise the source detected.
                return null;
            }

            for (int i = 0; i < detectedController.InputSource?.Pointers?.Length; i++)
            {
                detectedController.InputSource.Pointers[i].Controller = detectedController;
            }

            detectedController.TryRenderControllerModel(controllerType);

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