// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.InputSystem;
using XRTK.Lumin.Native;
using XRTK.Lumin.Profiles;
using XRTK.Providers.Controllers;
using XRTK.Services;

namespace XRTK.Lumin.Providers.Controllers
{
    [RuntimePlatform(typeof(LuminPlatform))]
    [System.Runtime.InteropServices.Guid("851006A2-0762-49AA-80A5-A01C9A8DBB58")]
    public class LuminControllerDataProvider : BaseControllerDataProvider
    {
        /// <inheritdoc />
        public LuminControllerDataProvider(string name, uint priority, LuminControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

        /// <summary>
        /// Dictionary to capture all active controllers detected
        /// </summary>
        private readonly Dictionary<byte, LuminController> activeControllers = new Dictionary<byte, LuminController>();

        private readonly MlInput.MLInputConfiguration inputConfiguration = new MlInput.MLInputConfiguration();

        private MlApi.MLHandle inputHandle = new MlApi.MLHandle();
        private MlInput.MLInputControllerState[] controllerStates = new MlInput.MLInputControllerState[2];

        /// <inheritdoc />
        public override void Enable()
        {
            if (!Application.isPlaying) { return; }

            if (!inputHandle.IsValid)
            {
                if (!MlInput.MLInputCreate(inputConfiguration, ref inputHandle).IsOk)
                {
                    Debug.LogError($"Failed to create {nameof(MlInput)}!");
                    return;
                }

                if (MlInput.MLInputGetControllerState(inputHandle, controllerStates).IsOk)
                {
                    foreach (var controllerState in controllerStates)
                    {
                        Debug.Log(controllerState);
                    }
                }
            }

            //if (!MLInput.IsStarted)
            //{
            //    var config = new MLInputConfiguration();
            //    var result = MLInput.Start(config);

            //    if (!result.IsOk)
            //    {
            //        Debug.LogError($"Error: failed starting MLInput: {result}");
            //        return;
            //    }
            //}

            //for (byte i = 0; i < 3; i++)
            //{
            //    // Currently no way to know what controllers are already connected.
            //    // Just guessing there could be no more than 3: Two Spatial Controllers and Mobile App Controller.
            //    var controller = GetController(i);

            //    if (controller != null)
            //    {
            //        MixedRealityToolkit.InputSystem?.RaiseSourceDetected(controller.InputSource, controller);
            //    }
            //}

            //MLInput.OnControllerConnected += OnControllerConnected;
            //MLInput.OnControllerDisconnected += OnControllerDisconnected;
            //MLInput.OnControllerButtonDown += MlInputOnControllerButtonDown;
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (!Application.isPlaying) { return; }
            if (!inputHandle.IsValid) { return; }

            foreach (var controller in activeControllers)
            {
                controller.Value?.UpdateController();
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (!Application.isPlaying) { return; }

            if (inputHandle.IsValid)
            {
                if (!MlInput.MLInputDestroy(inputHandle).IsOk)
                {
                    Debug.LogError($"Failed to destroy {nameof(MlInput)}!");
                }
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

            //var mlController = MLInput.GetController(controllerId);

            //if (mlController == null) { return null; }

            //if (mlController.Type == MLInputControllerType.None) { return null; }

            //var handedness = Handedness.Any;

            //if (mlController.Type == MLInputControllerType.Control)
            //{
            //    switch (mlController.Hand)
            //    {
            //        case MLInput.Hand.Left:
            //            handedness = Handedness.Left;
            //            break;
            //        case MLInput.Hand.Right:
            //            handedness = Handedness.Right;
            //            break;
            //    }
            //}

            //LuminController detectedController;

            //try
            //{
            //    detectedController = new LuminController(this, TrackingState.NotTracked, handedness, GetControllerMappingProfile(typeof(LuminController), handedness));
            //}
            //catch (Exception e)
            //{
            //    Debug.LogError($"Failed to create {nameof(LuminController)}!\n{e}");
            //    return null;
            //}

            //detectedController.MlControllerReference = mlController;
            //activeControllers.Add(controllerId, detectedController);
            //AddController(detectedController);
            //return detectedController;

            return null;
        }

        private void RemoveController(byte controllerId, bool removeFromRegistry = true)
        {
            var controller = GetController(controllerId, false);

            if (controller != null)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);
            }

            if (removeFromRegistry)
            {
                RemoveController(controller);
                activeControllers.Remove(controllerId);
            }
        }
    }
}