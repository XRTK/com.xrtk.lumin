// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Extensions;
using XRTK.Interfaces.Providers.Controllers;
using XRTK.Providers.Controllers;
using XRTK.Services;

namespace XRTK.Lumin.Providers.Controllers
{
    [System.Runtime.InteropServices.Guid("6AC05D69-0E7A-4CFE-91E3-05BC0564458D")]
    public class LuminController : BaseController
    {
        /// <inheritdoc />
        public LuminController() { }

        /// <inheritdoc />
        public LuminController(IMixedRealityControllerDataProvider controllerDataProvider, TrackingState trackingState, Handedness controllerHandedness, MixedRealityControllerMappingProfile controllerMappingProfile)
            : base(controllerDataProvider, trackingState, controllerHandedness, controllerMappingProfile)
        {
        }

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping("Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new MixedRealityInteractionMapping("Trigger Position", AxisType.SingleAxis, DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Trigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch),
            new MixedRealityInteractionMapping("Trigger Press (Select)", AxisType.Digital, DeviceInputType.Select),
            new MixedRealityInteractionMapping("Bumper Press", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Home Press", AxisType.Digital, DeviceInputType.Menu),
            new MixedRealityInteractionMapping("Touchpad Position", AxisType.DualAxis, DeviceInputType.Touchpad),
            new MixedRealityInteractionMapping("Touchpad Press", AxisType.SingleAxis, DeviceInputType.TouchpadPress),
            new MixedRealityInteractionMapping("Touchpad Touch", AxisType.SingleAxis, DeviceInputType.TouchpadTouch),
        };

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        private MixedRealityPose currentPointerPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose lastControllerPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose currentControllerPose = MixedRealityPose.ZeroIdentity;
        private Vector2 dualAxisPosition;

        public void UpdateController(InputDevice inputDevice)
        {
            if (!Enabled) { return; }

            base.UpdateController();

            UpdateControllerData(inputDevice);

            if (Interactions == null)
            {
                Debug.LogError($"No interaction configuration for {GetType().Name} {ControllerHandedness}");
                Enabled = false;
            }

            for (int i = 0; i < Interactions?.Length; i++)
            {
                var interactionMapping = Interactions[i];

                switch (interactionMapping.InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        UpdatePoseData(interactionMapping);
                        break;
                    case DeviceInputType.Menu:
                        if (inputDevice.TryGetFeatureValue(CommonUsages.menuButton, out var isHomeDown))
                        {
                            interactionMapping.BoolData = isHomeDown;
                        }

                        break;
                    case DeviceInputType.ButtonPress:
                        if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out var deviceIsBumperDown))
                        {
                            interactionMapping.BoolData = deviceIsBumperDown;
                        }

                        break;
                    case DeviceInputType.Select:
                    case DeviceInputType.Trigger:
                    case DeviceInputType.TriggerTouch:
                    case DeviceInputType.TriggerPress:
                    case DeviceInputType.TouchpadTouch:
                    case DeviceInputType.TouchpadPress:
                        UpdateSingleAxisData(interactionMapping, inputDevice);
                        break;
                    case DeviceInputType.Touchpad:
                        UpdateDualAxisData(interactionMapping, inputDevice);
                        break;
                    default:
                        Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                        break;
                }

                interactionMapping.RaiseInputAction(InputSource, ControllerHandedness);
            }
        }

        private void UpdateControllerData(InputDevice inputDevice)
        {
            var lastState = TrackingState;

            lastControllerPose = currentControllerPose;

            // The source is either a hand or a controller that supports pointing.
            // We can now check for position and rotation.
            if (inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out var isTracked))
            {
                IsPositionAvailable = isTracked;
            }

            inputDevice.TryGetFeatureValue(MagicLeapControllerUsages.ControllerDOF, out var deviceDof);

            if (IsPositionAvailable && deviceDof >= 1)
            {
                if (inputDevice.TryGetFeatureValue(MagicLeapControllerUsages.ControllerCalibrationAccuracy, out var calibrationAccuracy))
                {
                    IsPositionApproximate = calibrationAccuracy <= 2;
                }
            }
            else
            {
                IsPositionApproximate = false;
            }

            IsRotationAvailable = deviceDof == 2;

            // Devices are considered tracked if we receive position OR rotation data from the sensors.
            TrackingState = (IsPositionAvailable || IsRotationAvailable) ? TrackingState.Tracked : TrackingState.NotTracked;

            if (inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var devicePosition))
            {
                currentControllerPose.Position = devicePosition;
            }

            if (inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out var deviceRotation))
            {
                currentControllerPose.Rotation = deviceRotation;
            }

            // Raise input system events if it is enabled.
            if (lastState != TrackingState)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceTrackingStateChanged(InputSource, this, TrackingState);
            }

            if (TrackingState == TrackingState.Tracked && lastControllerPose != currentControllerPose)
            {
                if (IsPositionAvailable && IsRotationAvailable)
                {
                    MixedRealityToolkit.InputSystem?.RaiseSourcePoseChanged(InputSource, this, currentControllerPose);
                }
                else if (IsPositionAvailable && !IsRotationAvailable)
                {
                    MixedRealityToolkit.InputSystem?.RaiseSourcePositionChanged(InputSource, this, currentControllerPose.Position);
                }
                else if (!IsPositionAvailable && IsRotationAvailable)
                {
                    MixedRealityToolkit.InputSystem?.RaiseSourceRotationChanged(InputSource, this, currentControllerPose.Rotation);
                }
            }
        }

        private void UpdateSingleAxisData(MixedRealityInteractionMapping interactionMapping, InputDevice inputDevice)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SingleAxis || interactionMapping.AxisType == AxisType.Digital);

            inputDevice.TryGetFeatureValue(CommonUsages.trigger, out var deviceTriggerValue);
            inputDevice.TryGetFeatureValue(MagicLeapControllerUsages.ControllerTouch1Force, out var force);

            interactionMapping.FloatData = interactionMapping.Description.Contains("Trigger")
                ? deviceTriggerValue
                : force;

            switch (interactionMapping.InputType)
            {
                case DeviceInputType.Select:
                case DeviceInputType.Trigger:
                case DeviceInputType.TriggerPress:
                case DeviceInputType.TouchpadPress:
                    interactionMapping.BoolData = interactionMapping.FloatData.Approximately(1f, 0.001f) ||
                                                  interactionMapping.FloatData.Approximately(-1f, 0.001f);
                    break;
                case DeviceInputType.TriggerTouch:
                case DeviceInputType.TouchpadTouch:
                case DeviceInputType.TriggerNearTouch:
                    interactionMapping.BoolData = !interactionMapping.FloatData.Equals(0f);
                    break;
                default:
                    Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                    return;
            }
        }

        private void UpdateDualAxisData(MixedRealityInteractionMapping interactionMapping, InputDevice inputDevice)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.DualAxis);

            inputDevice.TryGetFeatureValue(MagicLeapControllerUsages.ControllerTouch1Force, out var force);
            inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out var deviceTouch1PosAndForce);

            if (force > 0f)
            {
                dualAxisPosition.x = deviceTouch1PosAndForce.x;
                dualAxisPosition.y = deviceTouch1PosAndForce.y;
            }
            else
            {
                dualAxisPosition.x = 0f;
                dualAxisPosition.y = 0f;
            }

            // Update the interaction data source
            interactionMapping.Vector2Data = dualAxisPosition;
        }

        private void UpdatePoseData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);

            if (interactionMapping.InputType == DeviceInputType.SpatialPointer)
            {
                // Pointer and controller pose is the same for this device.
                currentPointerPose = currentControllerPose;
            }
            else
            {
                Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                return;
            }

            // Update the interaction data source
            interactionMapping.PoseData = currentPointerPose;
        }
    }
}
