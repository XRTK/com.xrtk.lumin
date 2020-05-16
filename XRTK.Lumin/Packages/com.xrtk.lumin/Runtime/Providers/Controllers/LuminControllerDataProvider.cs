// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Platforms;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Lumin.Native;
using XRTK.Lumin.Profiles;
using XRTK.Lumin.Providers.CameraSystem;
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

        private readonly IntPtr statePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MlInput.MLInputControllerState)) * 2);
        private readonly MlInput.MLInputConfiguration inputConfiguration = MlInput.MLInputConfiguration.Default;
        private readonly MlController.MLControllerConfiguration controllerConfiguration = MlController.MLControllerConfiguration.Default;

        /// <summary>
        /// Dictionary to capture all active controllers detected
        /// </summary>
        private readonly Dictionary<byte, LuminController> activeControllers = new Dictionary<byte, LuminController>();

        private MlApi.MLHandle inputHandle;
        private MlApi.MLHandle controllerHandle;
        private MlInput.MLInputControllerCallbacksEx controllerCallbacksEx;
        private MlController.MLControllerSystemState controllerSystemState;
        private MlInput.MLInputControllerState[] controllerStates = MlInput.MLInputControllerState.Default;
        private MlTypes.MLTransform tempControllerTransform;

        /// <inheritdoc />
        public override void Enable()
        {
            if (!Application.isPlaying) { return; }

            if (!inputHandle.IsValid)
            {
                if (MlInput.MLInputCreate(inputConfiguration, ref inputHandle).IsOk)
                {
                    controllerCallbacksEx.on_connect += (id, data) =>
                    {
                        Debug.Log($"controller {id} connected!");
                        GetController(id);
                    };

                    controllerCallbacksEx.on_disconnect += (id, data) =>
                    {
                        Debug.Log($"controller {id} disconnected!");
                        RemoveController(id);
                    };

                    controllerCallbacksEx.on_button_down += (id, button, data) => Debug.Log($"controller {id}:{button}.down");
                    controllerCallbacksEx.on_button_up += (id, button, data) => Debug.Log($"controller {id}:{button}.up");

                    if (!MlInput.MLInputSetControllerCallbacksEx(inputHandle, controllerCallbacksEx, IntPtr.Zero).IsOk)
                    {
                        Debug.LogError("Failed to set controller callbacks!");
                    }

                    if (MlInput.MLInputGetControllerState(inputHandle, statePointer).IsOk)
                    {
                        MlInput.MLInputControllerState.GetControllerStates(statePointer, ref controllerStates);
                    }
                }
                else
                {
                    Debug.LogError("Failed to create input tracker!");
                }
            }

            if (!controllerHandle.IsValid)
            {
                if (!MlController.MLControllerCreateEx(controllerConfiguration, ref controllerHandle).IsOk)
                {
                    Debug.LogError("Failed to create controller tracker!");
                }
                else
                {
                    MlController.MLControllerGetState(controllerHandle, ref controllerSystemState);
                }
            }

            LuminCameraDataProvider.OnSnapshotCaptured += LuminCameraDataProvider_OnSnapshotCaptured;
        }

        private void LuminCameraDataProvider_OnSnapshotCaptured(MlSnapshot.MLSnapshot snapshot)
        {
            foreach (var controllerState in controllerSystemState.controller_state)
            {
                foreach (var controllerStream in controllerState.stream)
                {
                    if (controllerStream.is_active)
                    {
                        if (MlSnapshot.MLSnapshotGetTransform(snapshot, controllerStream.coord_frame_controller, ref tempControllerTransform).IsOk)
                        {
                            // Debug.Log($"{nameof(controllerState.controller_id)}.{controllerState.controller_id}:{tempControllerTransform}");
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            if (!Application.isPlaying) { return; }
            if (!controllerHandle.IsValid) { return; }

            if (MlInput.MLInputGetControllerState(inputHandle, statePointer).IsOk)
            {
                MlInput.MLInputControllerState.GetControllerStates(statePointer, ref controllerStates);
            }

            if (MlController.MLControllerGetState(controllerHandle, ref controllerSystemState).IsOk)
            {
                // Debug.Log(controllerSystemState);
            }

            foreach (var controller in activeControllers)
            {
                controller.Value?.UpdateController();
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (!Application.isPlaying) { return; }

            LuminCameraDataProvider.OnSnapshotCaptured -= LuminCameraDataProvider_OnSnapshotCaptured
                ;
            if (controllerHandle.IsValid)
            {
                controllerCallbacksEx.on_connect = null;
                controllerCallbacksEx.on_disconnect = null;
                controllerCallbacksEx.on_button_down = null;
                controllerCallbacksEx.on_button_up = null;

                if (!MlInput.MLInputSetControllerCallbacksEx(inputHandle, controllerCallbacksEx, IntPtr.Zero).IsOk)
                {
                    Debug.LogError("Failed to clear controller callbacks!");
                }

                if (!MlController.MLControllerDestroy(controllerHandle).IsOk)
                {
                    Debug.LogError($"Failed to destroy {nameof(MlController)} tracker!");
                }
            }

            if (inputHandle.IsValid)
            {
                if (!MlInput.MLInputDestroy(inputHandle).IsOk)
                {
                    Debug.LogError($"Failed to destroy the input tracker!");
                }
            }

            foreach (var activeController in activeControllers)
            {
                RemoveController(activeController.Key, false);
            }

            activeControllers.Clear();
        }

        /// <inheritdoc />
        protected override void OnDispose(bool finalizing)
        {
            if (finalizing)
            {
                Marshal.FreeHGlobal(statePointer);
            }

            base.OnDispose(finalizing);
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
            if (controllerStates[controllerId].type == MlInput.MLInputControllerType.None) { return null; }

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
            }

            if (removeFromRegistry)
            {
                RemoveController(controller);
                activeControllers.Remove(controllerId);
            }
        }
    }
}