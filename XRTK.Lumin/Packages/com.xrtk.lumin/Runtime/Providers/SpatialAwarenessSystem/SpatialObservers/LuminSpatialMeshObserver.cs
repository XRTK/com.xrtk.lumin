// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.SpatialAwarenessSystem;
using XRTK.Lumin.Native;
using XRTK.Lumin.Profiles;
using XRTK.Providers.SpatialObservers;
using XRTK.Services;
using XRTK.Utilities;

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
        private MlApi.MLHandle meshingHandle;
        private MlMeshing2.MLMeshingSettings meshingSettings;

        #region IMixedRealityService implementation

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (!Application.isPlaying || Application.isEditor) { return; }

            if (!meshingHandle.IsValid)
            {
                if (!MlMeshing2.MLMeshingInitSettings(ref meshingSettings).IsOk)
                {
                    Debug.LogError("Failed to initialize meshing settings!");
                }

                if (!MlMeshing2.MLMeshingCreateClient(ref meshingHandle, meshingSettings).IsOk)
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
                !Application.isPlaying)
            {
                return;
            }

            // and If enough time has passed since the previous observer update
            if (!(Time.time - lastUpdated >= UpdateInterval)) { return; }

            // Update the observer location if it is not stationary
            if (!IsStationaryObserver)
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
            }

            //if (meshSubsystem.TryGetMeshInfos(meshInfos))
            //{
            //    for (int i = 0; i < meshInfos.Count; i++)
            //    {
            //        MeshInfo_Update(meshInfos[i]);
            //    }
            //}

            lastUpdated = Time.time;
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            if (!Application.isPlaying) { return; }

            if (meshingHandle.IsValid)
            {
                if (!MlMeshing2.MLMeshingDestroyClient(ref meshingHandle).IsOk)
                {
                    Debug.LogError("Failed to destroy meshing client!");
                }
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

        //private async void MeshInfo_Update(MeshInfo meshInfo)
        //{
        //    if (meshInfo.ChangeState == MeshChangeState.Unchanged) { return; }

        //    // If we're adding or updating a mesh
        //    if (meshInfo.ChangeState != MeshChangeState.Removed)
        //    {
        //        var spatialMeshObject = await RequestSpatialMeshObject(meshInfo.MeshId.GetHashCode());
        //        spatialMeshObject.GameObject.name = $"SpatialMesh_{meshInfo.MeshId}";

        //        var meshAttributes = MeshRecalculateNormals ? MeshVertexAttributes.Normals : MeshVertexAttributes.None;

        //        try
        //        {
        //            OnMeshGenerated(new MeshGenerationResult());
        //            //meshSubsystem.GenerateMeshAsync(meshInfo.MeshId, spatialMeshObject.Mesh, spatialMeshObject.Collider, meshAttributes, OnMeshGenerated);
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError($"{e.Message}\n{e.StackTrace}");
        //        }

        //        void OnMeshGenerated(MeshGenerationResult result)
        //        {
        //            if (result.Status == MeshGenerationStatus.GenerationAlreadyInProgress)
        //            {
        //                return;
        //            }

        //            if (result.Status != MeshGenerationStatus.Success)
        //            {
        //                Debug.LogWarning($"No output for {result.MeshId} | {result.Status}");
        //                RaiseMeshRemoved(spatialMeshObject);
        //                return;
        //            }

        //            if (!SpatialMeshObjects.TryGetValue(result.MeshId.GetHashCode(), out var meshObject))
        //            {
        //                Debug.LogWarning($"Failed to find a spatial mesh object for {result.MeshId}!");
        //                // Likely it was removed before data could be cooked.
        //                return;
        //            }

        //            // Apply the appropriate material to the mesh.
        //            var displayOption = MeshDisplayOption;

        //            if (displayOption != SpatialMeshDisplayOptions.None)
        //            {
        //                meshObject.Collider.enabled = true;
        //                meshObject.Renderer.enabled = displayOption == SpatialMeshDisplayOptions.Visible ||
        //                                              displayOption == SpatialMeshDisplayOptions.Occlusion;
        //                meshObject.Renderer.sharedMaterial = displayOption == SpatialMeshDisplayOptions.Visible
        //                    ? MeshVisibleMaterial
        //                    : MeshOcclusionMaterial;
        //            }
        //            else
        //            {
        //                meshObject.Renderer.enabled = false;
        //                meshObject.Collider.enabled = false;
        //            }

        //            // Recalculate the mesh normals if requested.
        //            if (MeshRecalculateNormals)
        //            {
        //                if (meshObject.Filter.sharedMesh != null)
        //                {
        //                    meshObject.Filter.sharedMesh.RecalculateNormals();
        //                }
        //                else
        //                {
        //                    meshObject.Filter.mesh.RecalculateNormals();
        //                }
        //            }

        //            if (!meshObject.GameObject.activeInHierarchy)
        //            {
        //                meshObject.GameObject.SetActive(true);
        //            }

        //            switch (meshInfo.ChangeState)
        //            {
        //                case MeshChangeState.Added:
        //                    RaiseMeshAdded(meshObject);
        //                    break;
        //                case MeshChangeState.Updated:
        //                    RaiseMeshUpdated(meshObject);
        //                    break;
        //            }
        //        }
        //    }
        //    else if (SpatialMeshObjects.TryGetValue(meshInfo.MeshId.GetHashCode(), out var meshObject))
        //    {
        //        RaiseMeshRemoved(meshObject);
        //    }
        //}
    }
}
