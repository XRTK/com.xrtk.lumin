// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;

[assembly: InternalsVisibleTo("XRTK.Lumin.Editor")]
namespace XRTK.Lumin
{
    /// <summary>
    /// Shamelessly lifted from the UnityEngine.XR.MagicLeap packages, but Unity had to make their class internal. Boo.
    /// </summary>
    internal static class LuminApi
    {
        private const string UNITY_MAGIC_LEAP_DLL = "UnityMagicLeap";

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_MeshingUpdateSettings(ref MeshingSettings newSettings);

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_MeshingSetDensity(float density);

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_MeshingSetBounds(Vector3 center, Quaternion rotation, Vector3 extents);

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_MeshingSetBatchSize(int batchSize);

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern IntPtr UnityMagicLeap_MeshingAcquireConfidence(MeshId meshId, out int count);

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_MeshingReleaseConfidence(MeshId meshId);

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_GesturesUpdateConfiguration(ref GestureConfiguration gestureConfiguration);

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_GesturesCreate();

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_GesturesUpdate();

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_GesturesStart();

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_GesturesStop();

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_GesturesDestroy();

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool UnityMagicLeap_GesturesIsHandGesturesEnabled();

        [DllImport(UNITY_MAGIC_LEAP_DLL)]
        public static extern void UnityMagicLeap_GesturesSetHandGesturesEnabled([MarshalAs(UnmanagedType.I1)]bool value);

        [Flags]
        public enum MeshingFlags
        {
            None = 0,
            PointCloud = 1,
            ComputeNormals = 2,
            ComputeConfidence = 4,
            Planarize = 8,
            RemoveMeshSkirt = 16, // 0x00000010
            IndexOrderCCW = 32, // 0x00000020
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MeshingSettings
        {
            public MeshingFlags flags;
            public float fillHoleLength;
            public float disconnectedComponentArea;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GestureConfiguration
        {
            /// <summary>
            /// Array length excludes [NoHand], since we do not allow it to be disabled.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)HandKeyPose.NoHand)]
            public byte[] KeyposeConfig;

            /// <summary>
            /// Determines if the hand tracking pipeline is currently enabled.
            /// </summary>
            [MarshalAs(UnmanagedType.I1)]
            public bool HandTrackingPipelineEnabled;

            /// <summary>
            /// The fidelity to track key points with.
            /// </summary>
            public KeyPointFilterLevel KeyPointsFilterLevel;

            /// <summary>
            /// The fidelity to track key poses with.
            /// </summary>
            public PoseFilterLevel PoseFilterLevel;
        }

        /// <summary>
        /// Configured level for key points filtering of key points and hand centers.
        /// </summary>
        public enum KeyPointFilterLevel
        {
            /// <summary>
            /// Default value, no filtering is done, the points are raw.
            /// </summary>
            Raw,

            /// <summary>
            /// Some smoothing at the cost of latency.
            /// </summary>
            Smoothed,

            /// <summary>
            /// Predictive smoothing, at higher cost of latency.
            /// </summary>
            ExtraSmoothed
        }

        /// <summary>
        /// Configured level of filtering for static poses.
        /// </summary>
        public enum PoseFilterLevel
        {
            /// <summary>
            /// Default value, no filtering, the poses are raw.
            /// </summary>
            Raw,

            /// <summary>
            /// Some robustness to flicker at some cost of latency.
            /// </summary>
            Robust,

            /// <summary>
            /// More robust to flicker at higher latency cost.
            /// </summary>
            ExtraRobust
        }

        /// <summary>
        /// Static key pose types which are available when both hands are separated.
        /// </summary>
        public enum HandKeyPose
        {
            /// <summary>
            /// Index finger.
            /// </summary>
            Finger,

            /// <summary>A
            /// A closed fist.
            /// </summary>
            Fist,

            /// <summary>
            /// A pinch.
            /// </summary>
            Pinch,

            /// <summary>
            /// A closed fist with the thumb pointed up.
            /// </summary>
            Thumb,

            /// <summary>
            /// An L shape
            /// </summary>
            L,

            /// <summary>
            /// An open hand.
            /// </summary>
            OpenHand = 5,

            /// <summary>
            /// A pinch with all fingers, except the index finger and the thumb, extended out.
            /// </summary>
            Ok,

            /// <summary>
            /// A rounded 'C' alphabet shape.
            /// </summary>
            C,

            /// <summary>
            /// No pose was recognized.
            /// </summary>
            NoPose,

            /// <summary>
            /// No hand was detected. Should be the last pose.
            /// </summary>
            NoHand
        }
    }
}