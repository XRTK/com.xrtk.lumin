//// Copyright (c) XRTK. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

//#if PLATFORM_LUMIN

//using System;
//using System.Runtime.InteropServices;
//using UnityEngine;
//using UnityEngine.Experimental.XR;
//using UnityEngine.XR.MagicLeap;

//namespace XRTK.Lumin
//{
//    /// <summary>
//    /// Shamelessly lifted from the UnityEngine.XR.MagicLeap packages, but Unity had to make their class internal. Boo.
//    /// </summary>
//    internal static class LuminApi
//    {
//        private const string UNITY_MAGIC_LEAP_DLL = "UnityMagicLeap";

//        [DllImport(UNITY_MAGIC_LEAP_DLL)]
//        public static extern void UnityMagicLeap_MeshingUpdateSettings(MeshingSettings newSettings);

//        [DllImport(UNITY_MAGIC_LEAP_DLL)]
//        public static extern void UnityMagicLeap_MeshingSetLod(MLSpatialMapper.LevelOfDetail lod);

//        [DllImport(UNITY_MAGIC_LEAP_DLL)]
//        public static extern void UnityMagicLeap_MeshingSetBounds(Vector3 center, Quaternion rotation, Vector3 extents);

//        [DllImport(UNITY_MAGIC_LEAP_DLL)]
//        public static extern void UnityMagicLeap_MeshingSetBatchSize(int batchSize);

//        [DllImport(UNITY_MAGIC_LEAP_DLL)]
//        public static extern IntPtr UnityMagicLeap_MeshingAcquireConfidence(TrackableId meshId, out int count);

//        [DllImport(UNITY_MAGIC_LEAP_DLL)]
//        public static extern void UnityMagicLeap_MeshingReleaseConfidence(TrackableId meshId);

//        [Flags]
//        public enum MeshingFlags
//        {
//            None = 0,
//            PointCloud = 1,
//            ComputeNormals = 2,
//            ComputeConfidence = 4,
//            Planarize = 8,
//            RemoveMeshSkirt = 16, // 0x00000010
//            IndexOrderCCW = 32, // 0x00000020
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        public struct MeshingSettings
//        {
//            public MeshingFlags flags;
//            public float fillHoleLength;
//            public float disconnectedComponentArea;
//        }
//    }
//}

//#endif // PLATFORM_LUMIN