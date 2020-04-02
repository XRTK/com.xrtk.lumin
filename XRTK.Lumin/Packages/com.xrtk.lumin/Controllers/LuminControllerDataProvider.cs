// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Lumin.Profiles;
using XRTK.Providers.Controllers;

#if PLATFORM_LUMIN

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Services;

#endif // PLATFORM_LUMIN

namespace XRTK.Lumin.Controllers
{
    public class LuminControllerDataProvider : BaseControllerDataProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <param name="profile"></param>
        public LuminControllerDataProvider(string name, uint priority, LuminControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
        }

#if PLATFORM_LUMIN

        /// <summary>
        /// Dictionary to capture all active controllers detected
        /// </summary>
        private readonly Dictionary<byte, LuminController> activeControllers = new Dictionary<byte, LuminController>();
        private readonly MLPoseFilterLevel poseFilterLevel;
        private readonly MLKeyPointFilterLevel keyPointFilterLevel;
        private readonly LuminHandDataConverter leftHandConverter = new LuminHandDataConverter(Handedness.Left);
        private readonly LuminHandDataConverter rightHandConverter = new LuminHandDataConverter(Handedness.Right);
        private readonly MLHandKeyPose[] keyPoses = Enum.GetValues(typeof(MLHandKeyPose)).Cast<MLHandKeyPose>().ToArray();

        /// <inheritdoc />
        public override void Enable()
        {
            if (!MLInput.IsStarted)
            {
                var config = new MLInputConfiguration();
                var result = MLInput.Start(config);

                if (!result.IsOk)
                {
                    Debug.LogError($"Error: failed starting MLInput: {result}");
                    return;
                }
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

                MLHands.KeyPoseManager.SetKeyPointsFilterLevel(keyPointFilterLevel);
                MLHands.KeyPoseManager.SetPoseFilterLevel(poseFilterLevel);
            }

            for (byte i = 0; i < 3; i++)
            {
                // Currently no way to know what controllers are already connected.
                // Just guessing there could be no more than 3: Two Spatial Controllers and Mobile App Controller.
                var controller = GetController(i);

                if (controller != null)
                {
                    MixedRealityToolkit.InputSystem?.RaiseSourceDetected(controller.InputSource, controller);
                }
            }

            MLInput.OnControllerConnected += OnControllerConnected;
            MLInput.OnControllerDisconnected += OnControllerDisconnected;
            MLInput.OnControllerButtonDown += MlInputOnControllerButtonDown;
            LuminHandDataConverter.HandMeshingEnabled = HandMeshingEnabled;
        }

        ///// <inheritdoc />
        //public override void Update()
        //{
        //    base.Update();

        //    if (MLHands.IsStarted)
        //    {
        //        if (MLHands.Left.IsVisible)
        //        {
        //            var controller = GetOrAddController(Handedness.Left);
        //            controller?.UpdateController(leftHandConverter.GetHandData());
        //        }
        //        else
        //        {
        //            RemoveController(Handedness.Left);
        //        }

        //        if (MLHands.Right.IsVisible)
        //        {
        //            var controller = GetOrAddController(Handedness.Right);
        //            controller?.UpdateController(rightHandConverter.GetHandData());
        //        }
        //        else
        //        {
        //            RemoveController(Handedness.Right);
        //        }
        //    }
        //}

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            foreach (var controller in activeControllers)
            {
                controller.Value?.UpdateController();
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            MLInput.OnControllerConnected -= OnControllerConnected;
            MLInput.OnControllerDisconnected -= OnControllerDisconnected;
            MLInput.OnControllerButtonDown -= MlInputOnControllerButtonDown;
            MLInput.Stop();

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

        private LuminController GetController(byte controllerId, bool addController = true)
        {
            //If a device is already registered with the ID provided, just return it.
            if (activeControllers.ContainsKey(controllerId))
            {
                var controller = activeControllers[controllerId];
                Debug.Assert(controller != null);
                return controller;
            }

            if (!addController) { return null; }

            var mlController = MLInput.GetController(controllerId);

            if (mlController == null) { return null; }

            if (mlController.Type == MLInputControllerType.None) { return null; }

            var controllingHand = Handedness.Any;

            if (mlController.Type == MLInputControllerType.Control)
            {
                switch (mlController.Hand)
                {
                    case MLInput.Hand.Left:
                        controllingHand = Handedness.Left;
                        break;
                    case MLInput.Hand.Right:
                        controllingHand = Handedness.Right;
                        break;
                }
            }

            var pointers = mlController.Type == MLInputControllerType.Control ? RequestPointers(typeof(LuminController), controllingHand) : null;
            var inputSource = MixedRealityToolkit.InputSystem?.RequestNewGenericInputSource($"Lumin Controller {controllingHand}", pointers);
            var detectedController = new LuminController(TrackingState.NotTracked, controllingHand, inputSource);

            if (!detectedController.SetupConfiguration(typeof(LuminController)))
            {
                // Controller failed to be setup correctly.
                // Return null so we don't raise the source detected.
                return null;
            }

            for (int i = 0; i < detectedController.InputSource?.Pointers?.Length; i++)
            {
                detectedController.InputSource.Pointers[i].Controller = detectedController;
            }

            detectedController.MlControllerReference = mlController;
            activeControllers.Add(controllerId, detectedController);
            AddController(detectedController);
            return detectedController;
        }

        private void RemoveController(byte controllerId, bool removeFromRegistry = true)
        {
            var controller = GetController(controllerId, false);

            if (controller != null)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);
                RemoveController(controller);
            }

            if (removeFromRegistry)
            {
                activeControllers.Remove(controllerId);
            }
        }

        #region Controller Events

        private void OnControllerConnected(byte controllerId)
        {
            var controller = GetController(controllerId);

            if (controller != null)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceDetected(controller.InputSource, controller);
                controller.UpdateController();
            }
        }

        private void OnControllerDisconnected(byte controllerId)
        {
            RemoveController(controllerId);
        }

        private void MlInputOnControllerButtonDown(byte controllerId, MLInputControllerButton button)
        {
            if (activeControllers.TryGetValue(controllerId, out var controller))
            {
                switch (button)
                {
                    case MLInputControllerButton.HomeTap:
                        controller.IsHomePressed = true;
                        break;
                }
            }
        }

        #endregion Controller Events

#endif // PLATFORM_LUMIN
    }
}

//private MixedRealityHandController GetOrAddController(Handedness handedness)
//{
//    // If a device is already registered with the handedness, just return it.
//    if (TryGetController(handedness, out var existingController))
//    {
//        return existingController;
//    }

//    var controllerType = typeof(MixedRealityHandController);
//    var pointers = RequestPointers(controllerType, handedness, true);
//    var inputSource = MixedRealityToolkit.InputSystem.RequestNewGenericInputSource($"{handedness} Hand Controller", pointers);
//    var detectedController = new MixedRealityHandController(TrackingState.Tracked, handedness, inputSource);

//    if (!detectedController.SetupConfiguration(controllerType))
//    {
//        // Controller failed to be setup correctly.
//        // Return null so we don't raise the source detected.
//        return null;
//    }

//    for (int i = 0; i < detectedController.InputSource?.Pointers?.Length; i++)
//    {
//        detectedController.InputSource.Pointers[i].Controller = detectedController;
//    }

//    detectedController.TryRenderControllerModel(controllerType);

//    activeControllers.Add(handedness, detectedController);
//    MixedRealityToolkit.InputSystem?.RaiseSourceDetected(detectedController.InputSource, detectedController);

//    return detectedController;
//}

//private void RemoveController(Handedness handedness, bool removeFromRegistry = true)
//{
//    if (TryGetController(handedness, out var controller))
//    {
//        MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);

//        if (removeFromRegistry)
//        {
//            activeControllers.Remove(handedness);
//        }
//    }
//}

//private bool TryGetController(Handedness handedness, out MixedRealityHandController controller)
//{
//    if (activeControllers.ContainsKey(handedness))
//    {
//        var existingController = activeControllers[handedness];
//        Debug.Assert(existingController != null, $"Hand Controller {handedness} has been destroyed but remains in the active controller registry.");
//        controller = existingController;
//        return true;
//    }

//    controller = null;
//    return false;
//}