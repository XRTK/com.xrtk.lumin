// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;
using XRTK.Definitions.Utilities;
using XRTK.Lumin.Providers.InputSystem.Controllers;

namespace XRTK.Lumin.Profiles
{
    public class LuminControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(LuminController), Handedness.Left),
                new ControllerDefinition(typeof(LuminController), Handedness.Right)
            };
        }
    }
}