// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;
using XRTK.Interfaces.Providers.Controllers;
using XRTK.Providers.Controllers;

namespace XRTK.Lumin.Controllers
{
    public class LuminHandControllerDataProvider : BaseControllerDataProvider, IMixedRealityPlatformHandControllerDataProvider
    {
        /// <inheritdoc />
        public event HandDataUpdate OnHandDataUpdate;

        public LuminHandControllerDataProvider(string name, uint priority, BaseMixedRealityControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
        }
    }
}