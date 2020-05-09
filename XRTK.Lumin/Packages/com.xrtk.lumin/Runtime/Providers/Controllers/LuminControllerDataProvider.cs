// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;
using XRTK.Attributes;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Platforms;
using XRTK.Definitions.Utilities;
using XRTK.Extensions;
using XRTK.Interfaces.InputSystem;
using XRTK.Lumin.Profiles;
using XRTK.Providers.Controllers;
using XRTK.Services;

namespace XRTK.Lumin.Providers.Controllers
{
    [RuntimePlatform(typeof(LuminPlatform))]
    [Guid("851006A2-0762-49AA-80A5-A01C9A8DBB58")]
    public class LuminControllerDataProvider : BaseControllerDataProvider
    {
        /// <inheritdoc />
        public LuminControllerDataProvider(string name, uint priority, LuminControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

        private readonly Dictionary<InputDevice, LuminController> activeControllers = new Dictionary<InputDevice, LuminController>();
        private readonly List<XRInputSubsystemDescriptor> descriptors = new List<XRInputSubsystemDescriptor>();
        private readonly List<InputDevice> devices = new List<InputDevice>();

        private XRInputSubsystem inputSubsystem;

        /// <inheritdoc />
        public override void Enable()
        {
            if (!Application.isPlaying) { return; }

            if (inputSubsystem == null)
            {
                descriptors.Clear();
                SubsystemManager.GetSubsystemDescriptors(descriptors);

                if (descriptors.Count > 0)
                {
                    var descriptorToUse = descriptors[0];

                    if (descriptors.Count > 1)
                    {
                        Debug.LogWarning($"Found {descriptors.Count} {nameof(XRInputSubsystemDescriptor)}s. Using \"{descriptorToUse.id}\"");
                    }

                    inputSubsystem = descriptorToUse.Create();
                }

                if (inputSubsystem == null)
                {
                    throw new Exception($"Failed to start {nameof(XRInputSubsystem)}!");
                }
            }

            if (inputSubsystem != null &&
                !inputSubsystem.running)
            {
                inputSubsystem.Start();
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (!Application.isPlaying) { return; }
            if (inputSubsystem == null || !inputSubsystem.running) { return; }

            if (inputSubsystem.TryGetInputDevices(devices))
            {
                // Remove any controllers if they're no longer in the list of devices
                foreach (var activeController in activeControllers)
                {
                    if (!devices.Contains(activeController.Key))
                    {
                        RemoveController(activeController.Key);
                    }
                }

                // Add any controllers if needed, then update the controllers
                foreach (var inputDevice in devices)
                {
                    if (!activeControllers.TryGetValue(inputDevice, out var controller))
                    {
                        controller = GetController(inputDevice);
                    }

                    controller?.UpdateController(inputDevice);
                }
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (!Application.isPlaying) { return; }

            foreach (var activeController in activeControllers)
            {
                RemoveController(activeController.Key, false);
            }

            activeControllers.Clear();

            if (inputSubsystem != null &&
                inputSubsystem.running)
            {
                inputSubsystem.Stop();
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            if (!Application.isPlaying) { return; }

            if (inputSubsystem != null &&
                inputSubsystem.running)
            {
                inputSubsystem.Stop();
            }

            inputSubsystem?.Destroy();
        }

        private LuminController GetController(InputDevice inputDevice, bool addController = true)
        {
            //If a device is already registered with the ID provided, just return it.
            if (activeControllers.ContainsKey(inputDevice))
            {
                var controller = activeControllers[inputDevice];
                Debug.Assert(controller != null);
                return controller;
            }

            if (!addController ||
                !inputDevice.characteristics.HasFlags(InputDeviceCharacteristics.Controller))
            {
                return null;
            }

            LuminController detectedController;

            try
            {
                detectedController = new LuminController(this, TrackingState.NotTracked, Handedness.Both, GetControllerMappingProfile(typeof(LuminController), Handedness.Both));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(LuminController)}!\n{e}");
                return null;
            }

            activeControllers.Add(inputDevice, detectedController);
            AddController(detectedController);

            MixedRealityToolkit.InputSystem?.RaiseSourceDetected(detectedController.InputSource, detectedController);
            return detectedController;
        }

        private void RemoveController(InputDevice inputDevice, bool removeFromRegistry = true)
        {
            var controller = GetController(inputDevice, false);

            if (controller != null)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);
            }

            if (removeFromRegistry)
            {
                RemoveController(controller);
                activeControllers.Remove(inputDevice);
            }
        }
    }
}