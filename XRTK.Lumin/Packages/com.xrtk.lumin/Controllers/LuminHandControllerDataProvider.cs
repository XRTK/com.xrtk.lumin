// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Interfaces.Providers.Controllers;
using XRTK.Services;

namespace XRTK.Lumin.Controllers
{
    public class LuminHandControllerDataProvider : BaseDataProvider, IMixedRealityPlatformHandControllerDataProvider
    {
        public LuminHandControllerDataProvider(string name, uint priority)
            : base(name, priority)
        {
        }

        public IMixedRealityController[] GetActiveControllers()
        {
            throw new System.NotImplementedException();
        }
    }
}