// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers.Hands;
using XRTK.Providers.Controllers.Hands;

namespace XRTK.Lumin.Controllers
{
    public class LuminHandControllerDataProvider : BaseHandControllerDataProvider
    {
        public LuminHandControllerDataProvider(string name, uint priority, HandControllerDataProviderProfile profile)
            : base(name, priority, profile) { }
    }
}