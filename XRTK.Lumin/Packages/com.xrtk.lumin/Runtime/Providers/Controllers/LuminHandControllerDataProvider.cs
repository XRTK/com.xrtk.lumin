// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using XRTK.Attributes;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Platforms;
using XRTK.Definitions.Utilities;
using XRTK.Extensions;
using XRTK.Interfaces.InputSystem;
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
            LuminHandDataConverter.HandMeshingEnabled = HandMeshingEnabled;

            gestureConfiguration = new LuminApi.GestureConfiguration
            {
                KeyposeConfig = new byte[(int)LuminApi.HandKeyPose.NoHand],
                HandTrackingPipelineEnabled = true,
                KeyPointsFilterLevel = (LuminApi.KeyPointFilterLevel)profile.KeyPointFilterLevel,
                PoseFilterLevel = (LuminApi.PoseFilterLevel)profile.PoseFilterLevel
            };
        }

        private readonly List<InputDevice> devices = new List<InputDevice>();
        private readonly LuminHandDataConverter handConverter = new LuminHandDataConverter();
        private readonly List<XRInputSubsystemDescriptor> descriptors = new List<XRInputSubsystemDescriptor>();
        private readonly Dictionary<InputDevice, MixedRealityHandController> activeControllers = new Dictionary<InputDevice, MixedRealityHandController>();

        private LuminApi.GestureConfiguration gestureConfiguration;
        private XRInputSubsystem inputSubsystem;

        /// <inheritdoc />
        public override void Initialize()
        {
            if (!Application.isPlaying) { return; }

#if PLATFORM_LUMIN
            try
            {
                LuminApi.UnityMagicLeap_GesturesCreate();
                LuminApi.UnityMagicLeap_GesturesUpdateConfiguration(ref gestureConfiguration);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
#endif // PLATFORM_LUMIN
        }

        /// <inheritdoc />
        public override void Enable()
        {
            if (!Application.isPlaying) { return; }

#if PLATFORM_LUMIN

            try
            {
                LuminApi.UnityMagicLeap_GesturesStart();

                if (LuminApi.UnityMagicLeap_GesturesIsHandGesturesEnabled() == false)
                {
                    LuminApi.UnityMagicLeap_GesturesSetHandGesturesEnabled(true);

                    if (LuminApi.UnityMagicLeap_GesturesIsHandGesturesEnabled() == false)
                    {
                        throw new Exception($"Failed to initialize the native hand tracker for {nameof(LuminHandControllerDataProvider)}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

#endif // PLATFORM_LUMIN

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

#if PLATFORM_LUMIN
            try
            {
                LuminApi.UnityMagicLeap_GesturesUpdate();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
#endif // PLATFORM_LUMIN

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

                if (!devices.Any(device => device.characteristics.HasFlags(InputDeviceCharacteristics.HandTracking)))
                {
                    Debug.LogWarning("No tracked hands found!");
                }

                // Add any controllers if needed, then update the controllers
                foreach (var inputDevice in devices)
                {
                    if (!activeControllers.TryGetValue(inputDevice, out var controller))
                    {
                        controller = GetOrAddController(inputDevice);
                    }

                    if (controller != null)
                    {
                        switch (controller.ControllerHandedness)
                        {
                            case Handedness.Left:
                                controller.UpdateController(handConverter.GetHandData(inputDevice));
                                break;
                            case Handedness.Right:
                                controller.UpdateController(handConverter.GetHandData(inputDevice));
                                break;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (!Application.isPlaying) { return; }

#if PLATFORM_LUMIN
            try
            {
                LuminApi.UnityMagicLeap_GesturesStop();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
#endif // PLATFORM_LUMIN

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

#if PLATFORM_LUMIN
            try
            {
                LuminApi.UnityMagicLeap_GesturesDestroy();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
#endif // PLATFORM_LUMIN

            if (inputSubsystem != null &&
                inputSubsystem.running)
            {
                inputSubsystem.Stop();
            }

            inputSubsystem?.Destroy();
        }

        private MixedRealityHandController GetOrAddController(InputDevice inputDevice, bool addController = true)
        {
            //If a device is already registered with the ID provided, just return it.
            if (activeControllers.ContainsKey(inputDevice))
            {
                var controller = activeControllers[inputDevice];
                Debug.Assert(controller != null);
                return controller;
            }

            if (!addController ||
                !inputDevice.characteristics.HasFlags(InputDeviceCharacteristics.HandTracking))
            {
                return null;
            }

            Handedness handedness;

            if (inputDevice.characteristics.HasFlags(InputDeviceCharacteristics.Left))
            {
                handedness = Handedness.Left;
            }
            else if (inputDevice.characteristics.HasFlags(InputDeviceCharacteristics.Right))
            {
                handedness = Handedness.Right;
            }
            else
            {
                Debug.LogError($"Failed to get handedness value from {inputDevice} with characteristics: {inputDevice.characteristics}");
                return null;
            }

            MixedRealityHandController detectedController;

            try
            {
                detectedController = new MixedRealityHandController(this, TrackingState.Tracked, handedness, GetControllerMappingProfile(typeof(MixedRealityHandController), handedness));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {handedness} {nameof(MixedRealityHandController)}!\n{e}");
                return null;
            }

            detectedController.TryRenderControllerModel();

            activeControllers.Add(inputDevice, detectedController);
            AddController(detectedController);
            MixedRealityToolkit.InputSystem?.RaiseSourceDetected(detectedController.InputSource, detectedController);

            return detectedController;
        }

        private void RemoveController(InputDevice inputDevice, bool removeFromRegistry = true)
        {
            var controller = GetOrAddController(inputDevice, false);

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