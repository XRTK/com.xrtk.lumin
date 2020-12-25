// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using CppSharp;

namespace Bindings.Generator.CppSharp
{
    internal static class Program
    {
        private const string ML_SDK_VERSION = "v0.24.1";
        private const string OUTPUT_DIRECTORY = "..\\..\\..\\..\\..\\XRTK.Lumin\\Packages\\com.xrtk.lumin\\Runtime\\Native";

        public static string MlSdkPath => Environment.ExpandEnvironmentVariables("%mlsdk%");

        public static string BasePath => $"{MlSdkPath}\\{ML_SDK_VERSION}\\";

        private static int Main(string[] _)
        {
            if (string.IsNullOrWhiteSpace(MlSdkPath))
            {
                Console.WriteLine("No mlsdk environment variable is defined. Make sure you have downloaded the latest magic leap sdk from The Lab, and define this path to the sdk version you wish to use. ex: \"C:\\Users\\your-account\\MagicLeap\\mlsdk\"");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
                return 1;
            }

            Console.WriteLine($"Found mlsdk at path: {MlSdkPath}");

            ConsoleDriver.Run(new LuminLibrary());

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            return 0;
        }
    }
}
