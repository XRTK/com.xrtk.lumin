//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;

namespace XRTK.Lumin.Runtime.Native
{
    using System.Runtime.InteropServices;

    internal static class MlLifecycle
    {
        /// <summary>
        /// Values for why focus was lost
        /// </summary>
        /// <remarks>
        /// The system will communicate to an application the reason for losing focus
        /// @apilevel 7
        /// </remarks>
        public enum MLLifecycleFocusLostReason : int
        {
            /// <summary>
            /// Value returned when focus is lost due to an unknown event
            /// </summary>
            MLLifecycleFocusLostReason_Invalid = unchecked((int)-1),

            /// <summary>
            /// Value returned when focus is lost due to a system dialog
            /// </summary>
            MLLifecycleFocusLostReason_System = unchecked((int)0),
        }

        /// <summary>
        /// Lifecycle callback functions
        /// </summary>
        /// <remarks>
        /// This structure must be initialized by calling MLLifecycleCallbacksExInit before use
        /// @apilevel 7
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLLifecycleCallbacksEx
        {
            public uint version;

            public MlLifecycle.MLLifecycleCallbacksEx.on_stop_delegate on_stop;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_stop_delegate(IntPtr context);

            public MlLifecycle.MLLifecycleCallbacksEx.on_pause_delegate on_pause;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_pause_delegate(IntPtr context);

            public MlLifecycle.MLLifecycleCallbacksEx.on_resume_delegate on_resume;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_resume_delegate(IntPtr context);

            public MlLifecycle.MLLifecycleCallbacksEx.on_unload_resources_delegate on_unload_resources;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_unload_resources_delegate(IntPtr context);

            public MlLifecycle.MLLifecycleCallbacksEx.on_new_initarg_delegate on_new_initarg;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_new_initarg_delegate(IntPtr context);

            public MlLifecycle.MLLifecycleCallbacksEx.on_device_active_delegate on_device_active;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_device_active_delegate(IntPtr context);

            public MlLifecycle.MLLifecycleCallbacksEx.on_device_reality_delegate on_device_reality;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_device_reality_delegate(IntPtr context);

            public MlLifecycle.MLLifecycleCallbacksEx.on_device_standby_delegate on_device_standby;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_device_standby_delegate(IntPtr context);

            public MlLifecycle.MLLifecycleCallbacksEx.on_focus_lost_delegate on_focus_lost;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_focus_lost_delegate(IntPtr context, MlLifecycle.MLLifecycleFocusLostReason reason);

            public MlLifecycle.MLLifecycleCallbacksEx.on_focus_gained_delegate on_focus_gained;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void on_focus_gained_delegate(IntPtr context);
        }

        /// <summary>
        /// This structure should be explicitly freed by calling MLLifecycleFreeSelfInfo
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLLifecycleSelfInfo
        {
            /// <summary>
            /// Path to the writable dir of the application This path is valid when the
            /// user is logged in and using the device, ie the device is unlocked This path
            /// is not available when device is locked
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string writable_dir_path;

            /// <summary>
            /// Path to the application package dir
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string package_dir_path;

            /// <summary>
            /// Package name of the application
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string package_name;

            /// <summary>
            /// Component name of the application
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string component_name;

            /// <summary>
            /// Path to the application tmp dir
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string tmp_dir_path;

            /// <summary>
            /// Visible name of the application
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string visible_name;

            /// <summary>
            /// Path to the writable dir of the application available when device is
            /// locked This path is valid when the user has logged in once and the device
            /// is locked An application that needs to write data when running in the
            /// background eg a music app should use this path  The same application can
            /// continue using this path when the device is unlocked afterwards Therefore
            /// this path is always available to an application
            /// </summary>
            [MarshalAs(UnmanagedType.LPStr)]
            public string writable_dir_path_locked_and_unlocked;
        }

        /// <summary>
        /// Opaque structure containing array of init args and other fields
        /// </summary>
        /// <remarks>
        /// An app can have multiple initial argument objects accumulated if it has been triggered multiple
        /// times and the app hasn't retrieved its InitArgs These objects are stored in
        /// MLLifecycleInitArgList which the app can retrieve
        /// Note: The platform does not keep a copy of the InitArgs once they have been retrieved by the app
        /// The accessible fields in this structure are:
        /// InitArg Array length - Can be obtained by calling MLLifecycleGetInitArgListLength
        /// InitArg Array        - Can be obtained by calling MLLifecycleGetInitArgByIndex
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct MLLifecycleInitArgList : IEquatable<MLLifecycleInitArgList>
        {
            private readonly IntPtr _handle;

            public MLLifecycleInitArgList(IntPtr handle) => _handle = handle;

            public IntPtr Handle => _handle;

            public bool Equals(MLLifecycleInitArgList other) => _handle.Equals(other._handle);

            public override bool Equals(object obj) => obj is MLLifecycleInitArgList other && Equals(other);

            public override int GetHashCode() => _handle.GetHashCode();

            public override string ToString() => "0x" + (IntPtr.Size == 8 ? _handle.ToString("X16") : _handle.ToString("X8"));

            public static bool operator ==(MLLifecycleInitArgList left, MLLifecycleInitArgList right) => left.Equals(right);

            public static bool operator !=(MLLifecycleInitArgList left, MLLifecycleInitArgList right) => !left.Equals(right);
        }

        /// <summary>
        /// Opaque structure containing array of file infos and other fields
        /// </summary>
        /// <remarks>
        /// The accessible fields in this structure are:
        /// URI                    - Can be obtained by calling MLLifecycleGetInitArgUri
        /// File Info Array length - Can be obtained by calling MLLifecycleGetFileInfoListLength
        /// File Info Array        - Can be obtained by calling MLLifecycleGetFileInfoByIndex
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct MLLifecycleInitArg : IEquatable<MLLifecycleInitArg>
        {
            private readonly IntPtr _handle;

            public MLLifecycleInitArg(IntPtr handle) => _handle = handle;

            public IntPtr Handle => _handle;

            public bool Equals(MLLifecycleInitArg other) => _handle.Equals(other._handle);

            public override bool Equals(object obj) => obj is MLLifecycleInitArg other && Equals(other);

            public override int GetHashCode() => _handle.GetHashCode();

            public override string ToString() => "0x" + (IntPtr.Size == 8 ? _handle.ToString("X16") : _handle.ToString("X8"));

            public static bool operator ==(MLLifecycleInitArg left, MLLifecycleInitArg right) => left.Equals(right);

            public static bool operator !=(MLLifecycleInitArg left, MLLifecycleInitArg right) => !left.Equals(right);
        }

        /// <summary>
        /// Retrieve the process specific information for the application
        /// MLResult_AllocFailed If memory allocation fails
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure If internal error occurs
        /// </summary>
        /// <param name="out_self_info">Pointer to the MLLifecycleSelfInfo structure pointer The
        /// user needs to explicitly free this structure by calling
        /// MLLifecycleFreeSelfInfo</param>
        /// <remarks>
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleGetSelfInfo(out IntPtr out_self_info);

        /// <summary>
        /// Free the MLLifecycleSelfInfo struct that is allocated by MLLifecycleGetSelfInfo
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// </summary>
        /// <param name="info">Pointer to MLLifecycleSelfInfo struct pointer</param>
        /// <remarks>
        /// The pointer to the MLLifecycleSelfInfo struct will point to NULL after this call
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleFreeSelfInfo(out IntPtr info);

        /// <summary>
        /// Retrieve the initial arguments or "init args" for the application
        /// The platform does not store the initial argument objects once the
        /// app has retrieved them Subsequent calls to this API can return an
        /// empty list if there are no new init args available
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// MLResult_UnspecifiedFailure If internal error occurs
        /// </summary>
        /// <param name="out_arg_list">Pointer to the MLLifecycleInitArgList structure pointer The
        /// user needs to explicitly free this structure by calling
        /// MLLifecycleFreeInitArgList function</param>
        /// <remarks>
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleGetInitArgList(out MlLifecycle.MLLifecycleInitArgList out_arg_list);

        /// <summary>
        /// Retrieve the length of the init arg array
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// </summary>
        /// <param name="init_arg_list">Pointer to MLLifecycleInitArgList struct</param>
        /// <param name="out_init_arg_list_length">Pointer to variable that will hold length of MLLifecycleInitArg array</param>
        /// <remarks>
        /// This function can return length of 0 which implies there are no init
        /// args available
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleGetInitArgListLength(MlLifecycle.MLLifecycleInitArgList init_arg_list, ref long out_init_arg_list_length);

        /// <summary>
        /// Retrieve the MLLifecycleInitArg structure from MLLifecycleInitArgList for given index
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// </summary>
        /// <param name="init_arg_list">Pointer to MLLifecycleInitArgList struct</param>
        /// <param name="index">of the MLLifecycleInitArg array</param>
        /// <param name="out_init_arg">Pointer to MLLifecycleInitArg structure pointer</param>
        /// <remarks>
        /// Note: The caller should not free the pointer returned
        /// The memory will be released in the call to MLLifecycleFreeInitArgList
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleGetInitArgByIndex(MlLifecycle.MLLifecycleInitArgList init_arg_list, long index, out MlLifecycle.MLLifecycleInitArg out_init_arg);

        /// <summary>
        /// Retrieve the URI of the given MLLifecycleInitArg
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// </summary>
        /// <param name="init_arg">Pointer to MLLifecycleInitArg struct which was obtained by
        /// calling MLLifecycleGetInitArg</param>
        /// <param name="out_uri">Address of pointer to URI of the given MLLifecycleInitArg pointer
        /// The caller should not free the pointer returned
        /// The memory will be released in the call to MLLifecycleFreeInitArgList</param>
        /// <remarks>
        /// This returned URI can be of the pattern 
        /// &lt;schema
        /// ://&gt; where schema can be
        /// http, ftp etc
        /// Note: This field is typically used to pass URIs that the app can handle
        /// However, it can be any string that the app developer wants
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleGetInitArgUri(MlLifecycle.MLLifecycleInitArg init_arg, out string out_uri);

        /// <summary>
        /// Retrieve length of the MLFileInfo array in the given MLLifecycleInitArg
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// </summary>
        /// <param name="init_arg">Pointer to MLLifecycleInitArg array</param>
        /// <param name="out_file_info_length">Pointer to variable that will hold the length of MLFileInfo array</param>
        /// <remarks>
        /// This function can return length of 0 which implies there is no file info available
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleGetFileInfoListLength(MlLifecycle.MLLifecycleInitArg init_arg, ref long out_file_info_length);

        /// <summary>
        /// Retrieve the MLFileInfo structure from MLLifecycleInitArg for given index
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// </summary>
        /// <param name="init_arg">Pointer to MLLifecycleInitArg struct</param>
        /// <param name="index">of the MLFileInfo array</param>
        /// <param name="out_file_info">Pointer to MLFileInfo structure pointerThe caller should not free the pointer
        /// returned The memory will be released in the call to
        /// MLLifecycleFreeInitArgList</param>
        /// <remarks>
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleGetFileInfoByIndex(MlLifecycle.MLLifecycleInitArg init_arg, long index, out MlFileinfo.MLFileInfo out_file_info);

        /// <summary>
        /// Free the MLLifecycleInitArgList, MLLifecycleInitArg and MLFileInfo structures
        /// that are allocated by MLLifecycleGetInitArgList
        /// MLResult_InvalidParam If input parameter is invalid
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// </summary>
        /// <param name="init_arg_list">Pointer to MLLifecycleInitArgList struct pointer</param>
        /// <remarks>
        /// The pointer to the MLLifecycleInitArgList struct will point to NULL after this call
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleFreeInitArgList(out MlLifecycle.MLLifecycleInitArgList init_arg_list);

        /// <summary>
        /// This function should be called by applications to indicate that they are
        /// done with their initialization sequences and ready for the user
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure The operation failed with an unspecified error
        /// MLResult_UnspecifiedFailure If internal error occurs
        /// </summary>
        /// <remarks>
        /// Initialization checklist:
        /// Create graphics client connection with MLGraphicsCreateClient
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleSetReadyIndication();

        /// <summary>
        /// Initialize the lifecycle service interface
        /// MLResult_AllocFailed If memory allocation fails
        /// MLResult_Ok On success
        /// MLResult_UnspecifiedFailure If internal error occurs
        /// </summary>
        /// <param name="callbacks">Pointer to an MLLifecycleCallbacksEx structure</param>
        /// <param name="context">Pointer to the application context that the application wants a
        /// reference to during callbacks This parameter is optional and the user can pass a
        /// NULL if not using it</param>
        /// <remarks>
        /// Applications MUST do this BEFORE intiliazing any other sub-system or requesting permissions as
        /// the application WILL be terminated if it does not register within the time-out period
        /// @apilevel 5
        /// @priv None
        /// </remarks>
        [DllImport("ml_lifecycle", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLLifecycleInitEx(in MlLifecycle.MLLifecycleCallbacksEx callbacks, IntPtr context);
    }
}
