// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Definitions.SpatialAwarenessSystem;
using XRTK.Interfaces.CameraSystem;
using XRTK.Interfaces.SpatialAwarenessSystem;
using XRTK.Lumin.Native;
using XRTK.Lumin.Profiles;
using XRTK.Providers.SpatialObservers;
using XRTK.Services;
using XRTK.Utilities;
using XRTK.Utilities.Async;

namespace XRTK.Lumin.Providers.SpatialAwareness.SpatialObservers
{
    [RuntimePlatform(typeof(LuminPlatform))]
    [Guid("A1E7BFED-F290-43E3-84B6-01C740CC9614")]
    public class LuminSpatialMeshObserver : BaseMixedRealitySpatialMeshObserver
    {
        /// <inheritdoc />
        public LuminSpatialMeshObserver(string name, uint priority, LuminSpatialMeshObserverProfile profile, IMixedRealitySpatialAwarenessSystem parentService)
            : base(name, priority, profile, parentService)
        {
            meshingSettings = MlMeshing2.MLMeshingSettings.Default;

            if (MeshRecalculateNormals)
            {
                meshingSettings.flags |= MlMeshing2.MeshingFlags.ComputeNormals;
            }
        }

        private float lastUpdated = 0;
        private MlApi.MLHandle meshingClientHandle;
        private MlMeshing2.MLMeshingExtents extents;
        private MlMeshing2.MLMeshingSettings meshingSettings;
        private readonly List<MlApi.MLHandle> meshInfoRequests = new List<MlApi.MLHandle>();

        #region IMixedRealityService implementation

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (!Application.isPlaying || Application.isEditor) { return; }

            if (!meshingClientHandle.IsValid)
            {
                if (!MlMeshing2.MLMeshingInitSettings(ref meshingSettings).IsOk)
                {
                    Debug.LogError("Failed to initialize meshing settings!");
                }

                if (!MlMeshing2.MLMeshingCreateClient(ref meshingClientHandle, meshingSettings).IsOk)
                {
                    Debug.LogError("failed to create meshing client!");
                }
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            // Only update the observer if it is running.
            if (!IsRunning ||
                !Application.isPlaying ||
                !meshingClientHandle.IsValid)
            {
                return;
            }

            // Update the observer location if it is not stationary
            if (!IsStationaryObserver)
            {
                UpdateObserverLocation();
            }

            // The application can update the observer volume at any time, make sure we are using the latest.
            UpdateObserverVolume();

            // and If enough time has passed since the previous observer update
            if (Time.time - lastUpdated >= UpdateInterval)
            {
                RequestMeshInfo();
                lastUpdated = Time.time;
            }

            ProcessPendingMeshInfoRequests();
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            if (Application.isPlaying &&
                meshingClientHandle.IsValid &&
                !MlMeshing2.MLMeshingDestroyClient(ref meshingClientHandle).IsOk)
            {
                Debug.LogError("Failed to destroy meshing client!");
            }
        }

        #endregion IMixedRealityService implementation

        #region IMixedRealitySpatialMeshObserver implementation

        /// <inheritdoc/>
        public override void StartObserving()
        {
            if (IsRunning)
            {
                return;
            }

            base.StartObserving();

            // We want the first update immediately.
            lastUpdated = 0;
        }

        /// <inheritdoc />
        public override void StopObserving()
        {
            if (!IsRunning)
            {
                return;
            }

            base.StopObserving();
        }

        #endregion IMixedRealitySpatialMeshObserver implementation

        private void UpdateObserverLocation()
        {
            if (MixedRealityToolkit.TryGetSystem<IMixedRealityCameraSystem>(out var cameraSystem))
            {
                ObserverOrigin = cameraSystem.MainCameraRig.CameraTransform.localPosition;
                ObserverOrientation = cameraSystem.MainCameraRig.CameraTransform.localRotation;
            }
            else
            {
                var cameraTransform = CameraCache.Main.transform;
                ObserverOrigin = cameraTransform.position;
                ObserverOrientation = cameraTransform.rotation;
            }

            extents.rotation = ObserverOrientation;
        }

        private void UpdateObserverVolume()
        {
            extents.extents = ObservationExtents;
            extents.center = ObserverOrigin;
        }

        private void RequestMeshInfo()
        {
            if (!MlMeshing2.MLMeshingRequestMeshInfo(meshingClientHandle, in extents, out var meshHandle).IsOk)
            {
                Debug.LogError("Failed to request mesh info!");
                return;
            }

            meshInfoRequests.Add(meshHandle);
        }

        private void ProcessPendingMeshInfoRequests()
        {
            for (var i = 0; i < meshInfoRequests.Count; i++)
            {
                var requestHandle = meshInfoRequests[i];
                var meshInfoResult = MlMeshing2.MLMeshingGetMeshInfoResult(meshingClientHandle, requestHandle, out var meshInfo);

                if (meshInfoResult != MlApi.MLResult.Code.Pending)
                {
                    if (meshInfoResult != MlApi.MLResult.Code.Ok)
                    {
                        meshInfoRequests.RemoveAt(i);
                        Debug.LogError($"Failed to get mesh info result! {meshInfoResult}");
                        continue;
                    }

                    for (var j = 0; j < meshInfo.data_count; j++)
                    {
                        MeshInfo_Update(meshInfo.Data[j]);
                    }

                    meshInfoRequests.RemoveAt(i);

                    if (!MlMeshing2.MLMeshingFreeResource(meshingClientHandle, in requestHandle).IsOk)
                    {
                        Debug.LogError("Failed to release mesh info result resource!");
                    }
                }
            }
        }

        private async void MeshInfo_Update(MlMeshing2.MLMeshingBlockInfo meshInfo)
        {
            if (meshInfo.state == MlMeshing2.MLMeshingMeshState.Unchanged) { return; }

            // If we're adding or updating a mesh
            if (meshInfo.state != MlMeshing2.MLMeshingMeshState.Deleted)
            {
                var spatialMeshObject = await RequestSpatialMeshObject(meshInfo.id.ToGuid());
                spatialMeshObject.GameObject.name = $"SpatialMesh_{meshInfo.id}";

                MeshGenerationResult meshResult;

                try
                {
                    meshResult = await GenerateMeshAsync(meshInfo, spatialMeshObject);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    RaiseMeshRemoved(spatialMeshObject);
                    return;
                }

                if (meshResult.Status == MlMeshing2.MLMeshingResult.Pending)
                {
                    return;
                }

                if (meshResult.Status != MlMeshing2.MLMeshingResult.Success)
                {
                    Debug.LogWarning($"No output for {meshResult.Id} | {meshResult.Status}");
                    RaiseMeshRemoved(spatialMeshObject);
                    return;
                }

                if (!SpatialMeshObjects.TryGetValue(meshResult.Id.ToGuid(), out var meshObject))
                {
                    Debug.LogWarning($"Failed to find a spatial mesh object for {meshResult.Id}!");
                    // Likely it was removed before data could be cooked.
                    return;
                }

                // Apply the appropriate material to the mesh.
                var displayOption = MeshDisplayOption;

                if (displayOption != SpatialMeshDisplayOptions.None)
                {
                    meshObject.Collider.enabled = true;
                    meshObject.Renderer.enabled = displayOption == SpatialMeshDisplayOptions.Visible ||
                                                  displayOption == SpatialMeshDisplayOptions.Occlusion;
                    meshObject.Renderer.sharedMaterial = displayOption == SpatialMeshDisplayOptions.Visible
                        ? MeshVisibleMaterial
                        : MeshOcclusionMaterial;
                }
                else
                {
                    meshObject.Renderer.enabled = false;
                    meshObject.Collider.enabled = false;
                }

                if (!meshObject.GameObject.activeInHierarchy)
                {
                    meshObject.GameObject.SetActive(true);
                }

                switch (meshInfo.state)
                {
                    case MlMeshing2.MLMeshingMeshState.New:
                        RaiseMeshAdded(meshObject);
                        break;
                    case MlMeshing2.MLMeshingMeshState.Updated:
                        RaiseMeshUpdated(meshObject);
                        break;
                }
            }
            else if (SpatialMeshObjects.TryGetValue(meshInfo.id.ToGuid(), out var meshObject))
            {
                RaiseMeshRemoved(meshObject);
            }
        }

        #region Mesh Generation

        private static readonly VertexAttributeDescriptor[] VertexLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position)
        };

        private static readonly VertexAttributeDescriptor[] NormalsLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal)
        };

        private readonly struct MeshGenerationResult
        {
            public MeshGenerationResult(MlTypes.MLCoordinateFrameUID id, MlMeshing2.MLMeshingResult status)
            {
                Id = id;
                Status = status;
            }

            public MlTypes.MLCoordinateFrameUID Id { get; }

            public MlMeshing2.MLMeshingResult Status { get; }
        }

        /// <summary>
        /// Helper struct used as layout when normals are requested.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct VertexData
        {
            /// <summary>
            /// Position data of vertex.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Normal data of vertex.
            /// </summary>
            public Vector3 Normal;
        }

        private async Task<MeshGenerationResult> GenerateMeshAsync(MlMeshing2.MLMeshingBlockInfo meshInfo, SpatialMeshObject spatialMeshObject)
        {
            int levelOfDetail = (int)MeshLevelOfDetail;

            if (levelOfDetail < 0)
            {
                Debug.LogWarning($"{MeshLevelOfDetail} is unsupported! Falling back to low level of detail.");
                levelOfDetail = 0;
            }

            var blockRequest = new MlMeshing2.MLMeshingBlockRequest
            {
                id = meshInfo.id,
                level = (MlMeshing2.MLMeshingLOD)levelOfDetail
            };

            var meshRequest = new MlMeshing2.MLMeshingMeshRequest
            {
                request_count = 1,
                data = blockRequest
            };

            if (!MlMeshing2.MLMeshingRequestMesh(meshingClientHandle, in meshRequest, out var outRequestHandle).IsOk)
            {
                Debug.LogError("Failed to request a new mesh!");
                return new MeshGenerationResult(meshInfo.id, MlMeshing2.MLMeshingResult.Failed);
            }

            var meshRequestResult = new MlApi.MLResult(MlApi.MLResult.Code.Pending);

            var outMeshResult = new MlMeshing2.MLMeshingMesh
            {
                result = MlMeshing2.MLMeshingResult.Pending
            };

            await Awaiters.BackgroundThread;

            while (meshRequestResult.Value == MlApi.MLResult.Code.Pending)
            {
                meshRequestResult = MlMeshing2.MLMeshingGetMeshResult(meshingClientHandle, outRequestHandle, out outMeshResult);
                await Task.Delay(25); // TODO make this delay configurable?
            }

            await Awaiters.UnityMainThread;

            if (!meshRequestResult.IsOk ||
                outMeshResult.result == MlMeshing2.MLMeshingResult.Failed ||
                !MlMeshing2.MLMeshingFreeResource(meshingClientHandle, outRequestHandle).IsOk)
            {
                return new MeshGenerationResult(meshInfo.id, MlMeshing2.MLMeshingResult.Failed);
            }

            if (outMeshResult.data_count != meshRequest.request_count)
            {
                Debug.LogError($"Mesh Block count mismatch! Expected {meshRequest.request_count} but got {outMeshResult.data_count} blocks.");
                return new MeshGenerationResult(meshInfo.id, MlMeshing2.MLMeshingResult.Failed);
            }

            if (meshInfo.id != outMeshResult.data.id)
            {
                Debug.LogError($"Mesh info id mismatch!\n->{meshInfo.id}\n<-{outMeshResult.data.id}");
                return new MeshGenerationResult(meshInfo.id, MlMeshing2.MLMeshingResult.Failed);
            }

            var mesh = spatialMeshObject.Mesh == null ? new Mesh() : spatialMeshObject.Mesh;

            mesh.name = $"Mesh_{meshInfo.id}";

            if (outMeshResult.data.vertex_count == 0 ||
                outMeshResult.data.vertex == null ||
                outMeshResult.data.index_count == 0 ||
                outMeshResult.data.index == null)
            {
                return new MeshGenerationResult(meshInfo.id, outMeshResult.result);
            }

            await Awaiters.BackgroundThread;

            if (MeshRecalculateNormals)
            {
                var normals = new NativeArray<VertexData>((int)outMeshResult.data.vertex_count, Allocator.None);

                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = new VertexData
                    {
                        Position = outMeshResult.data.vertex[i],
                        Normal = outMeshResult.data.normal[i]
                    };
                }

                mesh.SetVertexBufferParams((int)outMeshResult.data.vertex_count, NormalsLayout);
                mesh.SetVertexBufferData(normals, 0, 0, (int)outMeshResult.data.vertex_count);

                normals.Dispose();
            }
            else
            {
                var vertices = new NativeArray<Vector3>((int)outMeshResult.data.vertex_count, Allocator.None);

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = outMeshResult.data.vertex[i];
                }

                mesh.SetVertexBufferParams((int)outMeshResult.data.vertex_count, VertexLayout);
                mesh.SetVertexBufferData(vertices, 0, 0, (int)outMeshResult.data.vertex_count);

                vertices.Dispose();
            }

            var indices = new NativeArray<short>(outMeshResult.data.index_count, Allocator.None);

            for (int i = 0; i < outMeshResult.data.index_count; i++)
            {
                indices[i] = (short)outMeshResult.data.index[i];
            }

            mesh.SetIndexBufferParams(outMeshResult.data.index_count, IndexFormat.UInt16);
            mesh.SetIndexBufferData(indices, 0, 0, outMeshResult.data.index_count);

            indices.Dispose();

            mesh.SetSubMesh(0, new SubMeshDescriptor(0, outMeshResult.data.index_count));
            mesh.Optimize();
            mesh.RecalculateBounds();

            if (MeshRecalculateNormals)
            {
                mesh.RecalculateNormals();
            }

            spatialMeshObject.Mesh = mesh;

            await Awaiters.UnityMainThread;
            return new MeshGenerationResult(meshInfo.id, outMeshResult.result);
        }

        #endregion Mesh Generation
    }
}
