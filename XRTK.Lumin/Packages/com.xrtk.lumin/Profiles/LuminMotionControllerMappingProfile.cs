// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Lumin.Controllers;
using XRTK.Definitions.Controllers;
using XRTK.Providers.Controllers;

namespace XRTK.Lumin.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Mappings/Lumin Controller Mapping Profile", fileName = "LuminControllerMappingProfile")]
    public class LuminMotionControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.Lumin;

        /// <inheritdoc />
        public override string TexturePath => $"{base.TexturePath}LuminController";

        protected override void Awake()
        {
            if (!HasSetupDefaults)
            {
                ControllerMappings = new[]
                {
                    new MixedRealityControllerMapping("Lumin Motion Controller Left", typeof(LuminController), Handedness.Left),
                    new MixedRealityControllerMapping("Lumin Motion Controller Right", typeof(LuminController), Handedness.Right),
                };
            }

            base.Awake();
        }
    }
}
