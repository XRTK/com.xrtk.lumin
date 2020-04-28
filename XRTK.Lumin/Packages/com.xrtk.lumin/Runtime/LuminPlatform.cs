// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Definitions.Platforms
{
    /// <summary>
    /// Used by the XRTK to signal that the feature is available on the Lumin platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("39EBB366-21A7-4D08-9412-036B32976D0C")]
    public class LuminPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_LUMIN
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public override bool IsBuildTargetAvailable
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Lumin;
#else
                return false;
#endif
            }
        }
    }
}