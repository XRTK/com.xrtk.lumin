// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using XRTK.Definitions.Utilities;
using XRTK.Extensions;
using Debug = UnityEngine.Debug;

namespace XRTK.Lumin.Editor
{
    public static class LabDriver
    {
        [Serializable]
        public class LabDriverQuery
        {
            public Result[] results;
            public long version;
            public string root;
            public bool success;

            [Serializable]
            public class Result
            {
                public long token;
                public string state;
                public string moduleName;
                public DateTimeOffset startTime;
                public DateTimeOffset endTime;
                public string commandName;
                public string[] commandArgs;
                public string[] output;
                public string[] error;
            }
        }

        /// <summary>
        /// Queries for the Magic Leap Lab shim paths for remote support libraries.
        /// </summary>
        /// <param name="luminSdkPath">The path to the MLSDK that you want the remote support libraries for.</param>
        /// <returns>The directories for the shim paths to the remote support library</returns>
        public static async Task<List<string>> GetLuminRemoteSupportLibrariesAsync(string luminSdkPath)
        {
            var result = new List<string>();
            ProcessResult processResult;

            try
            {
                processResult = await new Process().RunAsync($"labdriver com.magicleap.zi:get-shim-search-paths -sdk=\"{luminSdkPath}\"", false);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return result;
            }

            if (processResult.ExitCode == 0)
            {
                foreach (var response in processResult.Output)
                {
                    var queryResponse = JsonUtility.FromJson<LabDriverQuery>(response);

                    if (queryResponse != null)
                    {
                        if (queryResponse.success)
                        {
                            foreach (var responseResult in queryResponse.results)
                            {
                                result.AddRange(responseResult?.output ?? Array.Empty<string>());
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to deserialize query response!");
                    }
                }
            }
            else
            {
                foreach (var error in processResult.Errors)
                {
                    Debug.LogError(error);
                }
            }

            return result;
        }
    }
}