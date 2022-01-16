// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Extensions;
using XRTK.Services;
using Debug = UnityEngine.Debug;

namespace XRTK.Editor.BuildPipeline
{
    [RuntimePlatform(typeof(LuminPlatform))]
    public class LuminBuildInfo : BuildInfo
    {
        /// <inheritdoc />
        public override BuildTarget BuildTarget => BuildTarget.Lumin;

        /// <inheritdoc />
        public override bool Install => true;

        /// <inheritdoc />
        public override string ExecutableFileExtension => ".mpk";

        /// <inheritdoc />
        public override void OnPreProcessBuild(BuildReport report)
        {
            if (!MixedRealityToolkit.ActivePlatforms.Contains(BuildPlatform) ||
                EditorUserBuildSettings.activeBuildTarget != BuildTarget)
            {
                return;
            }

            if (VersionCode.HasValue)
            {
                PlayerSettings.Lumin.versionCode = VersionCode.Value;
            }
            else
            {
                // Usually version codes are unique and not tied to the usual semver versions
                // see https://developer.android.com/studio/publish/versioning#appversioning
                // versionCode - A positive integer used as an internal version number.
                // This number is used only to determine whether one version is more recent than another,
                // with higher numbers indicating more recent versions. The Android system uses the
                // versionCode value to protect against downgrades by preventing users from installing
                // an APK with a lower versionCode than the version currently installed on their device.
                PlayerSettings.Lumin.versionCode++;
            }

            var mabuPath = $"{Directory.GetParent(Application.dataPath)}{Path.DirectorySeparatorChar}Library{Path.DirectorySeparatorChar}Mabu";

            if (IsCommandLine &&
                Directory.Exists(mabuPath))
            {
                Directory.Delete(mabuPath, true);
            }
        }

        /// <inheritdoc />
        public override void OnPostProcessBuild(BuildReport buildReport)
        {
            if (!MixedRealityToolkit.ActivePlatforms.Contains(BuildPlatform) ||
                EditorUserBuildSettings.activeBuildTarget != BuildTarget ||
                buildReport.summary.result == BuildResult.Failed)
            {
                return;
            }

            if (Install && !Application.isBatchMode)
            {
                InstallOnDevice();
            }
        }

        private async void InstallOnDevice()
        {
            var canInstall = false;

            try
            {
                var deviceResult = await new Process().RunAsync("mldb devices", false);

                if (deviceResult.ExitCode == 0)
                {
                    foreach (var deviceId in deviceResult.Output)
                    {
                        if (!string.IsNullOrWhiteSpace(deviceId))
                        {
                            Debug.Log(deviceId);

                            if (!deviceId.Contains("List"))
                            {
                                canInstall = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            if (!canInstall)
            {
                Debug.Log("No devices found, skipping installation");
                return;
            }

            try
            {
                await new Process().RunAsync($"mldb install -u \"{OutputDirectory}\"", true);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
