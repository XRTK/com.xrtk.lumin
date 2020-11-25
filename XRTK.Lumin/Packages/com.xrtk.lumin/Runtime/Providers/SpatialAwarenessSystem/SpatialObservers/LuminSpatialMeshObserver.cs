// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Definitions.SpatialAwarenessSystem;
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
    [System.Runtime.InteropServices.Guid("A1E7BFED-F290-43E3-84B6-01C740CC9614")]
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
            if (MixedRealityToolkit.CameraSystem != null)
            {
                ObserverOrigin = MixedRealityToolkit.CameraSystem.MainCameraRig.CameraTransform.localPosition;
                ObserverOrientation = MixedRealityToolkit.CameraSystem.MainCameraRig.CameraTransform.localRotation;
            }
            else
            {
                var cameraTransform = CameraCache.Main.transform;
                ObserverOrigin = cameraTransform.position;
                ObserverOrientation = cameraTransform.rotation;
            }

            extents.extents = (MlTypes.MLVec3f)ObservationExtents;
            extents.center = (MlTypes.MLVec3f)ObserverOrigin;
            extents.rotation = (MlTypes.MLQuaternionf)ObserverOrientation;
        }

        private void RequestMeshInfo()
        {
            if (!MlMeshing2.MLMeshingRequestMeshInfo(meshingClientHandle, in extents, out MlApi.MLHandle meshHandle).IsOk)
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

                    meshInfoResult = MlMeshing2.MLMeshingFreeResource(meshingClientHandle, in requestHandle);

                    if (!meshInfoResult.IsOk)
                    {
                        Debug.LogError($"Failed to release mesh info results resource! {meshInfoResult}");
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
                var spatialMeshObject = await RequestSpatialMeshObject(meshInfo.id.GetHashCode());
                spatialMeshObject.GameObject.name = $"SpatialMesh_{meshInfo.id}";

                MeshGenerationResult result;
                try
                {
                    result = await GenerateMeshAsync(meshInfo.id, spatialMeshObject.Mesh, spatialMeshObject.Collider, MeshRecalculateNormals);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    RaiseMeshRemoved(spatialMeshObject);
                    return;
                }

                if (result.Status == MeshGenerationStatus.GenerationAlreadyInProgress)
                {
                    return;
                }

                if (result.Status != MeshGenerationStatus.Success)
                {
                    Debug.LogWarning($"No output for {result.Id} | {result.Status}");
                    RaiseMeshRemoved(spatialMeshObject);
                    return;
                }

                if (!SpatialMeshObjects.TryGetValue(result.Id.GetHashCode(), out var meshObject))
                {
                    Debug.LogWarning($"Failed to find a spatial mesh object for {result.Id}!");
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

                // Recalculate the mesh normals if requested.
                if (MeshRecalculateNormals)
                {
                    if (meshObject.Filter.sharedMesh != null)
                    {
                        meshObject.Filter.sharedMesh.RecalculateNormals();
                    }
                    else
                    {
                        meshObject.Filter.mesh.RecalculateNormals();
                    }
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
            else if (SpatialMeshObjects.TryGetValue(meshInfo.id.GetHashCode(), out var meshObject))
            {
                RaiseMeshRemoved(meshObject);
            }
        }

        #region Mesh Generation

        /// <summary>
        ///   <para>The status of a XRMeshSubsystem.GenerateMeshAsync.</para>
        /// </summary>
        private enum MeshGenerationStatus
        {
            /// <summary>
            ///   <para>The mesh generation was successful.</para>
            /// </summary>
            Success,
            /// <summary>
            ///   <para>The XRMeshSubsystem was already generating the requested mesh.</para>
            /// </summary>
            GenerationAlreadyInProgress,
            /// <summary>
            ///   <para>The mesh generation was canceled.</para>
            /// </summary>
            Canceled,
            /// <summary>
            ///   <para>The mesh generation failed for unknown reasons.</para>
            /// </summary>
            UnknownError,
        }

        private struct MeshGenerationResult
        {
            public MeshGenerationResult(MlTypes.MLCoordinateFrameUID id, MeshGenerationStatus status)
            {
                Id = id;
                Status = status;
            }

            public MlTypes.MLCoordinateFrameUID Id { get; }

            public MeshGenerationStatus Status { get; }
        }

        private static async Task<MeshGenerationResult> GenerateMeshAsync(MlTypes.MLCoordinateFrameUID id, Mesh mesh, MeshCollider collider, bool meshRecalculateNormals)
        {
            var result = new MeshGenerationResult(id, MeshGenerationStatus.GenerationAlreadyInProgress);

            // TODO populate mesh data

            await Awaiters.UnityMainThread;
            return result;
        }

        #endregion Mesh Generation
    }
}
