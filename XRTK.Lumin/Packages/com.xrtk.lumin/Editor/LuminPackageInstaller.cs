// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using UnityEditor;
using XRTK.Editor;
using XRTK.Extensions;
using XRTK.Utilities.Editor;

namespace XRTK.Lumin.Editor
{
    [InitializeOnLoad]
    internal static class LuminPackageInstaller
    {
        private static readonly string DefaultPath = $"{MixedRealityPreferences.ProfileGenerationPath}Lumin";
        private static readonly string HiddenPath = Path.GetFullPath($"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(LuminPathFinder)).ToForwardSlashes()}\\{MixedRealityPreferences.HIDDEN_PROFILES_PATH}");

        static LuminPackageInstaller()
        {
            if (!EditorPreferences.Get($"{nameof(LuminPackageInstaller)}", false))
            {
                EditorPreferences.Set($"{nameof(LuminPackageInstaller)}", PackageInstaller.TryInstallProfiles(HiddenPath, DefaultPath));
            }
        }
    }
}
