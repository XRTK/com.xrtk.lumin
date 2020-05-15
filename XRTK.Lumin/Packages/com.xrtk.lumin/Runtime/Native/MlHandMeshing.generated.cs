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

    internal static class MlHandMeshing
    {
        /// <summary>
        /// Stores a hand mesh's vertices and indices
        /// @apilevel 6
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLHandMeshBlock
        {
            /// <summary>
            /// The number of indices in index buffer
            /// </summary>
            public ushort index_count;

            /// <summary>
            /// The number of vertices in vertex buffer
            /// </summary>
            public uint vertex_count;

            /// <summary>
            /// Pointer to the vertex buffer
            /// </summary>
            public IntPtr vertex;

            /// <summary>
            /// Pointer to index buffer In the index buffer each value
            /// is the index of a vertex in the vertex buffer Three indices
            /// define one triangle For example, the first triangle will have
            /// the vertices: vertex[index[0]], vertex[index[1]], vertex[index[2]]
            /// Index order is clockwise
            /// </summary>
            /// <remarks>
            /// Pointer to the index buffer
            /// </remarks>
            public IntPtr index;
        }

        /// <summary>
        /// Stores MLHandMeshBlock data
        /// @apilevel 6
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MLHandMesh
        {
            /// <summary>
            /// Version of this structure
            /// </summary>
            public uint version;

            /// <summary>
            /// Number of meshes available in data
            /// </summary>
            public uint data_count;

            /// <summary>
            /// The mesh data
            /// </summary>
            public IntPtr data;
        }

        /// <summary>
        /// Create the hand meshing client
        /// MLResult_InvalidParam HandMeshing Client was not created due to an invalid parameter
        /// MLResult_Ok HandMeshing Client was created successfully
        /// MLResult_PrivilegeDenied HandMeshing Client was not created due to insufficient privilege
        /// MLResult_UnspecifiedFailure HandMeshing Client was not created due to an unknown error
        /// </summary>
        /// <param name="out_client_handle">The handle to the created client</param>
        /// <remarks>
        /// Note that this will be the only function in the HandMeshing API that will return MLResult_PrivilegeDenied
        /// Trying to call the other functions with an invalid MLHandle will result in MLResult_InvalidParam
        /// @apilevel 6
        /// @priv HandMesh
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLHandMeshingCreateClient(ref MlApi.MLHandle out_client_handle);

        /// <summary>
        /// Destroy the hand meshing client
        /// @apilevel 6
        /// MLResult_InvalidParam HandMeshing Client was not destroyed due to an invalid parameter
        /// MLResult_Ok HandMeshing Client was destroyed successfully
        /// </summary>
        /// <param name="inout_client_handle">The client to destroy</param>
        /// <remarks>
        /// @priv HandMesh
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLHandMeshingDestroyClient(ref MlApi.MLHandle inout_client_handle);

        /// <summary>
        /// Request the hand mesh
        /// @apilevel 6
        /// MLResult_InvalidParam Mesh was not requested due to an invalid parameter
        /// MLResult_Ok Mesh was requested successfully
        /// </summary>
        /// <param name="client_handle">The handle to the created client</param>
        /// <param name="out_request_handle">The handle for the current request Needs to be passed to query the result of the request</param>
        /// <remarks>
        /// @priv HandMesh
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLHandMeshingRequestMesh(MlApi.MLHandle client_handle, ref MlApi.MLHandle out_request_handle);

        /// <summary>
        /// Get the Result of a previous hand mesh request
        /// @apilevel 6
        /// MLResult_InvalidParam Mesh was not updated due to an invalid parameter
        /// MLResult_Ok Mesh was populated successfully
        /// MLResult_Pending Mesh pending update
        /// </summary>
        /// <param name="client_handle">The handle to the created client</param>
        /// <param name="request_handle">The handle received from a previous MLHandMeshingRequestMesh call</param>
        /// <param name="out_mesh">The final result which will be populated only if the result is successful</param>
        /// <remarks>
        /// @priv HandMesh
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLHandMeshingGetResult(MlApi.MLHandle client_handle, MlApi.MLHandle request_handle, ref MlHandMeshing.MLHandMesh out_mesh);

        /// <summary>
        /// Free resources created by the hand meshing APIS Needs to be called whenever MLHandMeshingGetResult,
        /// returns a success
        /// @apilevel 6
        /// MLResult_InvalidParam Resources were not freed due to an invalid parameter
        /// MLResult_Ok Resources were freed successfully
        /// </summary>
        /// <param name="client_handle">The handle to the created client</param>
        /// <param name="out_request_handle">The handle received from a previous MLHandMeshingRequestMesh call</param>
        /// <remarks>
        /// @priv HandMesh
        /// </remarks>
        [DllImport("ml_perception_client", CallingConvention = CallingConvention.Cdecl)]
        public static extern MlApi.MLResult MLHandMeshingFreeResource(MlApi.MLHandle client_handle, ref MlApi.MLHandle out_request_handle);
    }
}
