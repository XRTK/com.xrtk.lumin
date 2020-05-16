// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Extensions;
using XRTK.Interfaces.Providers.Controllers;
using XRTK.Lumin.Native;
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

        /// <summary>
        /// Updates the controller's interaction mappings and ready the current input values.
        /// </summary>
        /// <param name="inputState"></param>
        /// <param name="controllerState"></param>
        internal void UpdateController(MlInput.MLInputControllerState inputState, MlController.MLControllerState controllerState)
        {
            if (!Enabled) { return; }

            base.UpdateController();

            UpdateControllerData(inputState, controllerState);

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
                        UpdatePoseData(interactionMapping, inputState);
                        break;
                    case DeviceInputType.ButtonPress:
                        interactionMapping.BoolData = inputState.button_state[(int)MlInput.MLInputControllerButton.Bumper];
                        break;
                    case DeviceInputType.Menu:
                        interactionMapping.BoolData = inputState.button_state[(int)MlInput.MLInputControllerButton.HomeTap];
                        break;
                    case DeviceInputType.Select:
                    case DeviceInputType.Trigger:
                    case DeviceInputType.TriggerTouch:
                    case DeviceInputType.TriggerPress:
                    case DeviceInputType.TouchpadTouch:
                    case DeviceInputType.TouchpadPress:
                        UpdateSingleAxisData(interactionMapping, inputState);
                        break;
                    case DeviceInputType.Touchpad:
                        UpdateDualAxisData(interactionMapping, inputState);
                        break;
                    default:
                        Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                        break;
                }

                interactionMapping.RaiseInputAction(InputSource, ControllerHandedness);
            }
        }

        private void UpdateControllerData(MlInput.MLInputControllerState inputState, MlController.MLControllerState controllerState)
        {
            var lastState = TrackingState;

            lastControllerPose = currentControllerPose;
            var controllerStream = controllerState.stream[(int)MlController.MLControllerMode.Fused6Dof];

            if (inputState.type == MlInput.MLInputControllerType.Device)
            {
                // The source is either a hand or a controller that supports pointing.
                // We can now check for position and rotation.
                IsPositionAvailable = controllerStream.is_active;

                if (IsPositionAvailable)
                {
                    IsPositionApproximate = controllerState.accuracy <= MlController.MLControllerCalibAccuracy.Medium;
                }
                else
                {
                    IsPositionApproximate = false;
                }

                IsRotationAvailable = controllerState.stream[(int)MlController.MLControllerMode.Fused6Dof].is_active;

                // Devices are considered tracked if we receive position OR rotation data from the sensors.
                TrackingState = (IsPositionAvailable || IsRotationAvailable) ? TrackingState.Tracked : TrackingState.NotTracked;
            }
            else
            {
                // The input source does not support tracking.
                TrackingState = TrackingState.NotApplicable;
            }

            currentControllerPose.Position = (Vector3)controllerStream.Transform.position;
            currentControllerPose.Rotation = (Quaternion)controllerStream.Transform.rotation;

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

        private void UpdateSingleAxisData(MixedRealityInteractionMapping interactionMapping, MlInput.MLInputControllerState inputState)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SingleAxis || interactionMapping.AxisType == AxisType.Digital);

            interactionMapping.FloatData = interactionMapping.Description.Contains("Touchpad")
                ? inputState.touch_pos_and_force[0].z
                : inputState.trigger_normalized;

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

        private void UpdateDualAxisData(MixedRealityInteractionMapping interactionMapping, MlInput.MLInputControllerState inputState)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.DualAxis);

            if (inputState.touch_pos_and_force[0].z > 0f)
            {
                dualAxisPosition.x = inputState.touch_pos_and_force[0].x;
                dualAxisPosition.y = inputState.touch_pos_and_force[0].y;
            }
            else
            {
                dualAxisPosition.x = 0f;
                dualAxisPosition.y = 0f;
            }

            // Update the interaction data source
            interactionMapping.Vector2Data = dualAxisPosition;
        }

        private void UpdatePoseData(MixedRealityInteractionMapping interactionMapping, MlInput.MLInputControllerState inputState)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);

            if (interactionMapping.InputType == DeviceInputType.SpatialPointer)
            {
                currentPointerPose = currentControllerPose;
            }
            else
            {
                Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                return;
            }

            // Update the interaction data source
            interactionMapping.PoseData = currentControllerPose;
        }
    }
}
