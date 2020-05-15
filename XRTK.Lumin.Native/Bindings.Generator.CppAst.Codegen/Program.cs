// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CppAst;
using CppAst.CodeGen.Common;
using CppAst.CodeGen.CSharp;
using Zio.FileSystems;

namespace Bindings.Generator.CppAst.CodeGen
{
    internal static class Program
    {
        private const string ML_SDK_VERSION = "v0.24.1";
        private const string OUTPUT_DIRECTORY = "..\\..\\..\\..\\..\\XRTK.Lumin\\Packages\\com.xrtk.lumin\\Runtime\\Native";

        public static string MlSdkPath => Environment.ExpandEnvironmentVariables("%mlsdk%");

        public static string BaseSdkPath => $"{MlSdkPath}\\{ML_SDK_VERSION}\\";

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
            //Directory.GetFiles($"{BaseSdkPath}include", "*.h", SearchOption.TopDirectoryOnly).ToList();
            var files = new List<string>
            {
                $"{BaseSdkPath}include\\ml_camera.h",
                $"{BaseSdkPath}include\\ml_camera_metadata.h",
                $"{BaseSdkPath}include\\ml_lifecycle.h",
                $"{BaseSdkPath}include\\ml_graphics.h",
                $"{BaseSdkPath}include\\ml_graphics_utils.h",
                $"{BaseSdkPath}include\\ml_input.h",
                $"{BaseSdkPath}include\\ml_logging.h",
                $"{BaseSdkPath}include\\ml_movement.h",
                $"{BaseSdkPath}include\\ml_controller.h",
                $"{BaseSdkPath}include\\ml_aruco_tracking.h",
                $"{BaseSdkPath}include\\ml_cv_camera.h",
                $"{BaseSdkPath}include\\ml_data_array.h",
                $"{BaseSdkPath}include\\ml_eye_tracking.h",
                $"{BaseSdkPath}include\\ml_found_object.h",
                $"{BaseSdkPath}include\\ml_hand_meshing.h",
                $"{BaseSdkPath}include\\ml_hand_tracking.h",
                $"{BaseSdkPath}include\\ml_head_tracking.h",
                $"{BaseSdkPath}include\\ml_image_tracking.h",
                $"{BaseSdkPath}include\\ml_lighting_tracking.h",
                $"{BaseSdkPath}include\\ml_meshing2.h",
                $"{BaseSdkPath}include\\ml_perception.h",
                $"{BaseSdkPath}include\\ml_persistent_coordinate_frames.h",
                $"{BaseSdkPath}include\\ml_planes.h",
                $"{BaseSdkPath}include\\ml_raycast.h",
                $"{BaseSdkPath}include\\ml_snapshot.h",
            };

            var csOptions = new CSharpConverterOptions
            {
                GenerateAsInternal = true,
                GenerateEnumItemAsFields = true,
                DispatchOutputPerInclude = true,
                DefaultDllImportNameAndArguments = "ERROR",
                MappingRules =
                {
                    e => e.Map("*").DllImportLibrary("\"ml_camera\"", "ml_camera.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_camera_metadata\"", "ml_camera_metadata.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_lifecycle\"", "ml_lifecycle.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_lifecycle\"", "ml_fileinfo.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_graphics\"", "ml_graphics.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_graphics_utils\"", "ml_graphics_utils.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_input\"", "ml_input.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_ext_logging\"", "ml_logging.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_movement\"", "ml_movement.h"),

                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_controller.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_aruco_tracking.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_cv_camera.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_data_array.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_eye_tracking.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_found_object.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_hand_meshing.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_hand_tracking.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_head_tracking.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_image_tracking.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_lighting_tracking.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_meshing2.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_perception.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_persistent_coordinate_frames.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_planes.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_raycast.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_perception_client\"", "ml_snapshot.h"),

                    e => e.Map("*").DllImportLibrary("\"ml_audio\"", "ml_audio.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_app_connect\"", "ml_app_connect.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_avatar\"", "ml_avatar.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_bluetooth_adapter\"", "ml_bluetooth_adapter.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_bluetooth_gatt\"", "ml_bluetooth_gatt.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_bluetooth_le\"", "ml_bluetooth_le.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_bluetooth\"", "ml_bluetooth.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_connections\"", "ml_connections.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_contacts\"", "ml_contacts.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_dca\"", "ml_dca.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_dispatch\"", "ml_dispatch.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_identity\"", "ml_identity.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_identity\"", "ml_token_identity.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_locale\"", "ml_locale.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_location\"", "ml_location.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_media_ccparser\"", "ml_media_cea608_caption.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_media_ccparser\"", "ml_media_cea708_caption.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediacodec\"", "ml_mediacodec.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediacodeclist\"", "ml_mediacodeclist.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediacrypto\"", "ml_media_crypto.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_media_data_source\"", "ml_media_data_source.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediadrm\"", "ml_media_drm.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediaerror\"", "ml_media_error.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediaextractor\"", "ml_media_extractor.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediaformat\"", "ml_media_format.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediaplayer\"", "ml_media_player.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediastream_source\"", "ml_mediastream_source.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_mediacodec\"", "ml_media_surface_texture.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_musicservice_provider\"", "ml_musicservice_provider.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_musicservice\"", "ml_music_service.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_networking\"", "ml_networking.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_platform\"", "ml_platform.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_privileges\"", "ml_privilege_functions.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_purchase\"", "ml_purchase.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_remote\"", "ml_remote.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_screens\"", "ml_screens.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_secure_storage\"", "ml_secure_storage.h"),
                    e => e.Map("*").DllImportLibrary("\"ml_sharedfile\"", "ml_sharedfile.h"),
                }
            };

            var csCompilation = CSharpConverter.Convert(files, csOptions);

            if (csCompilation.HasErrors)
            {
                foreach (var message in csCompilation.Diagnostics.Messages)
                {
                    Console.Error.WriteLine(message);
                }

                Console.Error.WriteLine("Unexpected parsing errors");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
                return 1;
            }

            var fileSystem = new PhysicalFileSystem();
            csCompilation.DumpTo(
                new CodeWriter(
                    new CodeWriterOptions(
                        new SubFileSystem(fileSystem, fileSystem.ConvertPathFromInternal(OUTPUT_DIRECTORY)))));

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            return 0;
        }
    }
}
