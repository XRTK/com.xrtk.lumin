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
            EditorApplication.delayCall += CheckPackage;
        }

        [MenuItem("Mixed Reality Toolkit/Packages/Install Lumin Package Assets...", true)]
        private static bool ImportLuminPackageAssetsValidation()
        {
            return !Directory.Exists($"{DefaultPath}\\Profiles");
        }

        [MenuItem("Mixed Reality Toolkit/Packages/Install Lumin Package Assets...")]
        private static void ImportLuminPackageAssets()
        {
            EditorPreferences.Set($"{nameof(LuminPackageInstaller)}.Profiles", false);
            EditorApplication.delayCall += CheckPackage;
        }

        private static void CheckPackage()
        {
            if (!EditorPreferences.Get($"{nameof(LuminPackageInstaller)}.Profiles", false))
            {
                EditorPreferences.Set($"{nameof(LuminPackageInstaller)}.Profiles", PackageInstaller.TryInstallAssets(HiddenPath, $"{DefaultPath}\\Profiles"));
            }
        }
    }
}
