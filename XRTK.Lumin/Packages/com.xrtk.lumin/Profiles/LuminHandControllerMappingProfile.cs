// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Lumin.Controllers;
using XRTK.Providers.Controllers;

namespace XRTK.Lumin.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Mappings/Lumin Hand Controller Mapping Profile", fileName = "LuminHandControllerMappingProfile")]
    public class LuminHandControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.Hand;

        /// <inheritdoc />
        public override string TexturePath => $"{base.TexturePath}LuminController";

        protected override void Awake()
        {
            if (!HasSetupDefaults)
            {
                ControllerMappings = new[]
                {
                    new MixedRealityControllerMapping("Lumin Hand Controller Left", typeof(LuminHandController), Handedness.Left),
                    new MixedRealityControllerMapping("Lumin Hand Controller Right", typeof(LuminHandController), Handedness.Right),
                };
            }

            base.Awake();
        }
    }
}