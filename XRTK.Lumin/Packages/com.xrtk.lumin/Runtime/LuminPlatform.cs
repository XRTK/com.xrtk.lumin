// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using XRTK.Definitions.Platforms;

namespace XRTK.Lumin
{
    /// <summary>
    /// Used by the XRTK to signal that the feature is available on the Lumin platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("39EBB366-21A7-4D08-9412-036B32976D0C")]
    public class LuminPlatform : BasePlatform
    {
        private const string luminXRDisplaySubsystemDescriptorId = "MagicLeap-Display";
        private const string luminXRInputSubsystemDescriptorId = "MagicLeap-Input";

        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
                var displaySubsystems = new List<XRDisplaySubsystem>();
                SubsystemManager.GetSubsystems(displaySubsystems);
                var luminXRDisplaySubsystemDescriptorFound = false;

                for (var i = 0; i < displaySubsystems.Count; i++)
                {
                    var displaySubsystem = displaySubsystems[i];
                    if (displaySubsystem.SubsystemDescriptor.id.Equals(luminXRDisplaySubsystemDescriptorId) &&
                        displaySubsystem.running)
                    {
                        luminXRDisplaySubsystemDescriptorFound = true;
                    }
                }

                // The Lumin XR Display Subsystem is not available / running,
                // Lumin as a platform doesn't seem to be available.
                if (!luminXRDisplaySubsystemDescriptorFound)
                {
                    return false;
                }

                var inputSubsystems = new List<XRInputSubsystem>();
                SubsystemManager.GetSubsystems(inputSubsystems);
                var luminXRInputSubsystemDescriptorFound = false;

                for (var i = 0; i < inputSubsystems.Count; i++)
                {
                    var inputSubsystem = inputSubsystems[i];
                    if (inputSubsystem.SubsystemDescriptor.id.Equals(luminXRInputSubsystemDescriptorId) &&
                        inputSubsystem.running)
                    {
                        luminXRInputSubsystemDescriptorFound = true;
                    }
                }

                // The Lumin XR Input Subsystem is not available / running,
                // Lumin XR as a platform doesn't seem to be available.
                if (!luminXRInputSubsystemDescriptorFound)
                {
                    return false;
                }

                // Only if both, Display and Input Lumin XR Subsystems are available
                // and running, the platform is considered available.
                return true;
            }
        }

#if UNITY_EDITOR
        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.Lumin
        };
#endif // UNITY_EDITOR
    }
}