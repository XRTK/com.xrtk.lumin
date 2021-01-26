//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XRTK.Lumin.Native
{
    using System.Runtime.InteropServices;

    internal static class MlPersistentCoordinateFrames
    {
        /// <summary>
        /// Enumeration specifying the type of a persistent coordinate frame (PCF)
        /// Type is not fixed PCF can vary in its type between multiple headpose sessions
        /// A new Headpose session is created when the device reboots or loses tracking
        /// </summary>
        /// <remarks>
        /// @apilevel 8
        /// </remarks>
        public enum MLPersistentCoordinateFrameType : int
        {
            /// <summary>
            /// PCF is available only in the current headpose session
            /// This is PCF type is only available on the local device It cannot be shared
            /// with other users and will not persist across device reboots
            /// A SingleUserSingleSession type PCF can later be promoted to a
            /// SingleUserMultiSession type PCF
            /// </summary>
            MLPersistentCoordinateFrameType_SingleUserSingleSession = unchecked((int)1),

            /// <summary>
            /// PCF is available across multiple headpose sessions
            /// This PCF type is only available on the local device It cannot be shared
            /// with other users but will persist across device reboots
            /// </summary>
            MLPersistentCoordinateFrameType_SingleUserMultiSession = unchecked((int)2),

            /// <summary>
            /// PCF is available across multiple users and headpose sessions
            /// This PCF type can be shared with other users in the same physical
            /// environment and will persist across device reboots This PCF type requires
            /// that the user should enable the Shared World feature in the Settings app
            /// </summary>
            MLPersistentCoordinateFrameType_MultiUserMultiSession = unchecked((int)4),

            /// <summary>
            /// Ensure enum is represented as 32 bits
            /// </summary>
            MLPersistentCoordinateFrameType_Ensure32Bits = unchecked((int)0x7FFFFFFF),
        }

        /// <summary>
        /// This represents the state of a Persistent Coordinate Frame
        /// </summary>
        /// <remarks>
        /// It is exposed as a spherical region That’s because the translation / rotation quality of
        /// objects attached to a Persistent Coordinate Frame varies greatly to how far away that object
        /// is to the Persistent Coordinate Frame itself (as well as the distance from the user to that
        /// Persistent Coordinate Frame)
        /// This structure must be initialized by calling MLPersistentCoordinateFramesFrameStateInit before use
        /// @apilevel 8
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLPersistentCoordinateFramesFrameState
        {
            public uint version;

            /// <summary>
            /// A confidence value (from [0, 1]) representing the confidence in the PCF
            /// error within the valid radius
            /// </summary>
            public float confidence;

            /// <summary>
            /// The radius (in meters) within which the confidence is valid
            /// </summary>
            public float valid_radius_m;

            /// <summary>
            /// The rotational error (in degrees)
            /// </summary>
            public float rotation_err_deg;

            /// <summary>
            /// The translation error (in meters)
            /// </summary>
            public float translation_err_m;

            /// <summary>
            /// PCF type
            /// </summary>
            public MlPersistentCoordinateFrames.MLPersistentCoordinateFrameType type;
        }

        /// <summary>
        /// This represents a collection of filters and modifiers used by
        /// MLPersistentCoordinateFrameQuery to curate the returned values
        /// </summary>
        /// <remarks>
        /// This structure must be initialized by calling MLPersistentCoordinateQueryFilterInit before use
        /// @apilevel 8
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLPersistentCoordinateFramesQueryFilter
        {
            public uint version;

            /// <summary>
            /// [X,Y,Z] center query point from where the nearest neighbours will be calculated
            /// </summary>
            public MlTypes.MLVec3f target_point;

            /// <summary>
            /// Expected types of the results
            /// This is a bitmask field to specify all expected types
            /// For example, use
            /// @code    types_mask = MLPersistentCoordinateFrameType_SingleUserMultiSession | MLPersistentCoordinateFrameType_MultiUserMultiSession;
            /// @endcode
            /// </summary>
            /// <remarks>
            /// to get PCFs of MLPersistentCoordinateFrameType_SingleUserMultiSession and MLPersistentCoordinateFrameType_MultiUserMultiSession types
            /// </remarks>
            public uint types_mask;

            /// <summary>
            /// Upper bound number of expected results
            /// The implementation may return less entries than requested when total number of
            /// available elements is less than max_results or if internal memory limits are
            /// reached
            /// </summary>
            public uint max_results;

            /// <summary>
            /// Return only entries within radius of the sphere from target_point
            /// Radius is provided in meters Set to zero for unbounded results
            /// Filtering by distance will incur a performance penalty
            /// </summary>
            public float radius_m;

            /// <summary>
            /// Indicate if the result set should be sorted by distance from target_point
            /// Sorting the PCFs by distance will incur a performance penalty
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            public bool sorted;
        }

        /// <summary>
        /// Creates a Persistent Coordinate Frame Tracker
        /// MLResult_InvalidParam out param is null
        /// MLResult_Ok Returned a valid MLHandle
        /// MLResult_PrivilegeDenied Privileges not met Check app manifest
        /// MLResult_UnspecifiedFailure Unspecified failure
        /// </summary>
        /// <param name="out_tracker_handle">Pointer to a MLHandle</param>
        /// <remarks>
        /// @priv PcfRead
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLPersistentCoordinateFrameTrackerCreate(ref MlApi.MLHandle out_tracker_handle);

        /// <summary>
        /// Returns the count of currently available Persistent Coordinate Frames
        /// MLResult_InvalidParam out param is null
        /// MLResult_Ok Returned a valid MLHandle
        /// MLResult_PrivilegeDenied Privileges not met Check app manifest
        /// MLResult_UnspecifiedFailure Unspecified failure
        /// MLPassableWorldResult_LowMapQuality Map quality too low for content persistence Continue building the map
        /// MLPassableWorldResult_UnableToLocalize Currently unable to localize into any map Continue building the map
        /// </summary>
        /// <param name="tracker_handle">Valid MLHandle to a Persistent Coordinate Frame Tracker</param>
        /// <param name="out_count">Number of currently available Persistent Coordinate Frames</param>
        /// <remarks>
        /// @priv PcfRead
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLPersistentCoordinateFrameGetCount(MlApi.MLHandle tracker_handle, ref uint out_count);

        /// <summary>
        /// Returns all the Persistent Coordinate Frames currently available
        /// @apilevel 4
        /// MLResult_AllocFailed Size allocated is not sufficient Use MLPersistentCoordinateFrameGetCount to get the count
        /// MLResult_InvalidParam out param is null
        /// MLResult_Ok Returned a valid MLHandle
        /// MLResult_PrivilegeDenied Privileges not met Check app manifest
        /// MLResult_UnspecifiedFailure Unspecified failure
        /// MLPassableWorldResult_LowMapQuality Map quality too low for content persistence Continue building the map
        /// MLPassableWorldResult_UnableToLocalize Currently unable to localize into any map Continue building the map
        /// </summary>
        /// <param name="tracker_handle">Valid MLHandle to a Persistent Coordinate Frame Tracker</param>
        /// <param name="count">The size of the out_cfuids array</param>
        /// <param name="out_cfuids">An array used for writing the Persistent Coordinate Frame's MLCoordinateFrameUID</param>
        /// <remarks>
        /// @priv PcfRead
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLPersistentCoordinateFrameGetAllEx(MlApi.MLHandle tracker_handle, uint count, ref MlTypes.MLCoordinateFrameUID out_cfuids);

        /// <summary>
        /// Returns the closest Persistent Coordinate Frame to the target point passed in
        /// MLResult_InvalidParam out param is null
        /// MLResult_Ok Returned a valid MLHandle
        /// MLResult_PrivilegeDenied Privileges not met Check app manifest
        /// MLResult_UnspecifiedFailure Unspecified failure
        /// MLPassableWorldResult_LowMapQuality Map quality too low for content persistence Continue building the map
        /// MLPassableWorldResult_UnableToLocalize Currently unable to localize into any map Continue building the map
        /// </summary>
        /// <param name="tracker_handle">Valid MLHandle to a Persistent Coordinate Frames Tracker</param>
        /// <param name="target">XYZ of the destination that the nearest Persistent Coordinate Frame is requested for</param>
        /// <param name="out_cfuid">Pointer to be used to write the MLCoordinateFrameUID for the nearest Persistent Coordinate Frame to the target destination</param>
        /// <remarks>
        /// @priv PcfRead
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLPersistentCoordinateFrameGetClosest(MlApi.MLHandle tracker_handle, in MlTypes.MLVec3f target, ref MlTypes.MLCoordinateFrameUID out_cfuid);

        /// <summary>
        /// Returns filtered set of Persistent Coordinate Frames based on the informed parameters
        /// @apilevel 8
        /// MLResult_InvalidParam Invalid parameters
        /// MLResult_Ok Operation completed successfully
        /// MLResult_PrivilegeDenied Privileges not met Check app manifest
        /// MLResult_UnspecifiedFailure Unspecified failure
        /// MLPassableWorldResult_LowMapQuality Map quality too low for content persistence Continue building the map
        /// MLPassableWorldResult_UnableToLocalize Currently unable to localize into any map Continue building the map
        /// MLPassableWorldResult_SharedWorldNotEnabled MLPersistentCoordinateFramesQueryFiltertypes_mask is only MLPersistentCoordinateFrameType_MultiUserMultiSession but user has not enabled shared world in settings
        /// </summary>
        /// <param name="tracker_handle">Valid MLHandle to a Persistent Coordinate Frame Tracker</param>
        /// <param name="query_filter">Parameters used to curate the returned values</param>
        /// <param name="out_cfuids">An array of MLPersistentCoordinateFramesQueryFiltermax_results size used for writing the PCF's MLCoordinateFrameUID</param>
        /// <param name="out_cfuids_count">Number of entries populated in out_cfuids Any number between 0 and MLPersistentCoordinateFramesQueryFiltermax_results</param>
        /// <remarks>
        /// @priv PcfRead
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLPersistentCoordinateFrameQuery(MlApi.MLHandle tracker_handle, in MlPersistentCoordinateFrames.MLPersistentCoordinateFramesQueryFilter query_filter, ref MlTypes.MLCoordinateFrameUID out_cfuids, ref uint out_cfuids_count);

        /// <summary>
        /// Destroys a Persistent Coordinate Frame Tracker
        /// MLResult_InvalidParam Tracker handle is not known
        /// MLResult_Ok Returned a valid MLHandle
        /// MLResult_UnspecifiedFailure Unspecified failure
        /// </summary>
        /// <param name="tracker_handle">PersistentCoordinateFrameTracker handle to be destroyed</param>
        /// <remarks>
        /// @priv None
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLPersistentCoordinateFrameTrackerDestroy(MlApi.MLHandle tracker_handle);

        /// <summary>
        /// Returns an ASCII string representation for each result code
        /// </summary>
        /// <param name="result_code">MLResult type to be converted to string</param>
        /// <returns>
        /// ASCII string containing readable version of the result code
        /// </returns>
        /// <remarks>
        /// This call returns strings for all of the global MLResult and
        /// MLPassableWorldResult codes
        /// @priv None
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string MLPersistentCoordinateFrameGetResultString(MlApi.MLResult result_code);

        /// <summary>
        /// Return the state of the Persistent Coordinate Frame passed in as parameter
        /// @apilevel 7
        /// MLResult_Ok Returned a valid MLHandle
        /// MLResult_InvalidParam Cfuid and/or out_state parameters are null
        /// MLResult_PrivilegeDenied Privileges not met Check app manifest
        /// MLResult_UnspecifiedFailure Unspecified failure
        /// MLPassableWorldResult_LowMapQuality Map quality too low for content persistence Continue building the map
        /// MLPassableWorldResult_NotFound Passed cfuid is not available
        /// MLPassableWorldResult_UnableToLocalize Currently unable to localize into any map Continue building the map
        /// </summary>
        /// <param name="tracker_handle">Valid MLHandle to a Persistent Coordinate Frame Tracker</param>
        /// <param name="cfuid">Valid MLCoordinateFrameUID of a Persistent Coordinate Frame</param>
        /// <param name="out_state">Pointer to be used for writing the Persistent Coordinate Frame's state</param>
        /// <remarks>
        /// @priv PcfRead
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLPersistentCoordinateFramesGetFrameState(MlApi.MLHandle tracker_handle, in MlTypes.MLCoordinateFrameUID cfuid, ref MlPersistentCoordinateFrames.MLPersistentCoordinateFramesFrameState out_state);
    }
}