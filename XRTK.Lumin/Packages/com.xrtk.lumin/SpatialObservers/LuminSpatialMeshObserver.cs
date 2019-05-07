// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Providers.SpatialObservers;

namespace XRTK.Lumin.SpatialObservers
{
    /// <summary>
    /// Lumin Spatial Mesh Observer
    /// </summary>
    public class LuminSpatialMeshObserver : BaseMixedRealitySpatialMeshObserver
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <param name="profile"></param>
        public LuminSpatialMeshObserver(string name, uint priority, BaseMixedRealitySpatialMeshObserverProfile profile) : base(name, priority, profile)
        {
        }
    }
}
