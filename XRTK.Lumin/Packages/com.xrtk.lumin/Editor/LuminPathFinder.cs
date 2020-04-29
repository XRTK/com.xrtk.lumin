// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Utilities.Editor;

namespace XRTK.Lumin.Editor
{
    /// <summary>
    /// Dummy scriptable object used to find the relative path of the com.xrtk.lumin.
    /// </summary>
    /// <inheritdoc cref="IPathFinder" />
    public class LuminPathFinder : ScriptableObject, IPathFinder
    {
        /// <inheritdoc />
        public string Location => $"/Editor/{nameof(LuminPathFinder)}.cs";
    }
}
