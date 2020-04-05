// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        /// <summary>
        /// Creates a new instance of the data provider.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Hand controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public LuminHandControllerDataProvider(string name, uint priority, LuminHandControllerDataProviderProfile profile)
            : base(name, priority, profile)
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

                bool status = MLHands.KeyPoseManager.EnableKeyPoses(keyPoses, true, true);

                if (!status)
                {
                    Debug.LogError("Error: Failed enabling tracked key poses.");
                    return;
                }

                MLHands.KeyPoseManager.SetKeyPointsFilterLevel(keyPointFilterLevel);
                MLHands.KeyPoseManager.SetPoseFilterLevel(poseFilterLevel);

                if (!MLHands.KeyPoseManager.LastConfigurationApplied())
                {
                    Debug.LogError("Error: Failed updating key pose configuration!");
                }
            }

            LuminHandDataConverter.HandMeshingEnabled = HandMeshingEnabled;
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (MLHands.IsStarted)
            {
                if (MLHands.Left.IsVisible)
                {
                    var controller = GetOrAddController(Handedness.Left);
                    controller?.UpdateController(leftHandConverter.GetHandData());
                }
                else
                {
                    RemoveController(Handedness.Left);
                }

                if (MLHands.Right.IsVisible)
                {
                    var controller = GetOrAddController(Handedness.Right);
                    controller?.UpdateController(rightHandConverter.GetHandData());
                }
                else
                {
                    RemoveController(Handedness.Right);
                }
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (MLHands.IsStarted)
            {
                MLHands.Stop();
                MLHands.KeyPoseManager.DisableAllKeyPoses();
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
            var detectedController = new MixedRealityHandController(TrackingState.Tracked, handedness, inputSource);

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