// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;

namespace XRTK.Lumin.Profiles
{
    public class LuminMotionControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetControllerDefinitions()
        {
            // new MixedRealityControllerMapping("Lumin Motion Controller Left", typeof(LuminController), Handedness.Left),
            // new MixedRealityControllerMapping("Lumin Motion Controller Right", typeof(LuminController), Handedness.Right),
            throw new System.NotImplementedException();
        }
    }
}