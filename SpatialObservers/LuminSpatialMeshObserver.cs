// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Providers.SpatialObservers;

#if PLATFORM_LUMIN
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.MagicLeap;
using XRTK.Definitions.SpatialAwarenessSystem;
using XRTK.Utilities;
#endif // PLATFORM_LUMIN

namespace XRTK.Lumin.SpatialObservers
{
    /// <summary>
    /// Lumin Spatial Mesh Observer
    /// </summary>
    public class LuminSpatialMeshObserver : BaseMixedRealitySpatialMeshObserver
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <param name="profile"></param>
        public LuminSpatialMeshObserver(string name, uint priority, BaseMixedRealitySpatialMeshObserverProfile profile) : base(name, priority, profile)
        {
        }

#if PLATFORM_LUMIN

        private readonly List<XRMeshSubsystemDescriptor> descriptors = new List<XRMeshSubsystemDescriptor>();

        private readonly List<MeshInfo> meshInfos = new List<MeshInfo>();

        private XRMeshSubsystem meshSubsystem;

        private float lastUpdated = 0;

        #region IMixedRealityService implementation

        public override void Initialize()
        {
            if (!Application.isPlaying || meshSubsystem != null) { return; }

            descriptors.Clear();
            SubsystemManager.GetSubsystemDescriptors(descriptors);

            if (descriptors.Count > 0)
            {
                var descriptorToUse = descriptors[0];

                if (descriptors.Count > 1)
                {
                    var typeOfD = typeof(XRMeshSubsystemDescriptor);
                    Debug.LogWarning($"Found {descriptors.Count} {typeOfD.Name}s. Using \"{descriptorToUse.id}\"");
                }

                meshSubsystem = descriptorToUse.Create();
            }

            if (meshSubsystem == null)
            {
                throw new Exception("Failed to start Lumin Mesh Subsystem!");
            }

            LuminApi.UnityMagicLeap_MeshingSetBatchSize(16);
            var levelOfDetail = MLSpatialMapper.LevelOfDetail.Medium;

            if (MeshLevelOfDetail == SpatialAwarenessMeshLevelOfDetail.Fine)
            {
                levelOfDetail = MLSpatialMapper.LevelOfDetail.Maximum;
            }

            LuminApi.UnityMagicLeap_MeshingSetLod(levelOfDetail);
            var settings = GetMeshingSettings();
            LuminApi.UnityMagicLeap_MeshingUpdateSettings(settings);
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            // Only update the observer if it is running.
            if (!Application.isPlaying || !IsRunning) { return; }

            // and If enough time has passed since the previous observer update
            if (!(Time.time - lastUpdated >= UpdateInterval)) { return; }

            // Update the observer location if it is not stationary
            if (!IsStationaryObserver)
            {
                var cameraTransform = CameraCache.Main.transform;
                ObserverOrigin = cameraTransform.position;
                ObserverOrientation = cameraTransform.rotation;
            }

            ConfigureObserverVolume();

            if (meshSubsystem.TryGetMeshInfos(meshInfos))
            {
                for (int i = 0; i < meshInfos.Count; i++)
                {
                    MeshInfo_Update(meshInfos[i]);
                }
            }

            // observer.Update(SurfaceObserver_OnSurfaceChanged);
            lastUpdated = Time.time;
        }

        private void ConfigureObserverVolume()
        {
            LuminApi.UnityMagicLeap_MeshingSetBounds(ObserverOrigin, ObserverOrientation, ObservationExtents);
        }

        /// <inheritdoc />
        protected override void OnDispose(bool finalizing)
        {
            meshSubsystem?.Destroy();

            base.OnDispose(finalizing);
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

            meshSubsystem.Start();

            // We want the first update immediately.
            lastUpdated = 0;

            base.StartObserving();
        }

        public override void StopObserving()
        {
            if (!IsRunning)
            {
                return;

            }

            meshSubsystem.Stop();

            base.StopObserving();
        }

        #endregion IMixedRealitySpatialMeshObserver implementation

        private LuminApi.MeshingSettings GetMeshingSettings()
        {
            var flags = LuminApi.MeshingFlags.IndexOrderCCW;

            if (MeshRecalculateNormals)
            {
                flags |= LuminApi.MeshingFlags.ComputeNormals;
            }

            // TODO Extend the observer profile with these options. 

            //if (requestVertexConfidence)
            //    flags |= LuminApi.MeshingFlags.ComputeConfidence;
            //if (planarize)
            flags |= LuminApi.MeshingFlags.Planarize;
            //if (removeMeshSkirt)
            //    flags |= LuminApi.MeshingFlags.RemoveMeshSkirt;
            //if (meshType == MeshType.PointCloud)
            //    flags |= LuminApi.MeshingFlags.PointCloud;

            var settings = new LuminApi.MeshingSettings
            {
                flags = flags,
                fillHoleLength = 5f,
                disconnectedComponentArea = 0.25f,
            };

            return settings;
        }

        private async void MeshInfo_Update(MeshInfo meshInfo)
        {
            if (meshInfo.ChangeState == MeshChangeState.Unchanged) { return; }

            // If we're adding or updating a mesh
            if (meshInfo.ChangeState != MeshChangeState.Removed)
            {
                var spatialMeshObject = await RequestSpatialMeshObject(meshInfo.MeshId.GetHashCode());
                spatialMeshObject.GameObject.name = $"SpatialMesh_{meshInfo.MeshId.ToString()}";

                var meshAttributes = MeshRecalculateNormals ? MeshVertexAttributes.Normals : MeshVertexAttributes.None;

                try
                {
                    meshSubsystem.GenerateMeshAsync(meshInfo.MeshId, spatialMeshObject.Mesh, spatialMeshObject.Collider, meshAttributes, OnMeshGenerated);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }

                void OnMeshGenerated(MeshGenerationResult result)
                {
                    if (result.Status == MeshGenerationStatus.GenerationAlreadyInProgress)
                    {
                        return;
                    }

                    if (result.Status != MeshGenerationStatus.Success)
                    {
                        Debug.LogWarning($"No output for {result.MeshId.ToString()} | {result.Status}");
                        RaiseMeshRemoved(spatialMeshObject);
                        return;
                    }

                    if (!SpatialMeshObjects.TryGetValue(result.MeshId.GetHashCode(), out var meshObject))
                    {
                        Debug.LogWarning($"Failed to find a spatial mesh object for {result.MeshId.ToString()}!");
                        // Likely it was removed before data could be cooked.
                        return;
                    }

                    // Apply the appropriate material to the mesh.
                    var displayOption = MeshDisplayOption;

                    if (displayOption != SpatialMeshDisplayOptions.None)
                    {
                        meshObject.Renderer.enabled = true;
                        meshObject.Renderer.sharedMaterial = displayOption == SpatialMeshDisplayOptions.Visible
                            ? MeshVisibleMaterial
                            : MeshOcclusionMaterial;
                    }
                    else
                    {
                        meshObject.Renderer.enabled = false;
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

                    meshObject.GameObject.SetActive(true);

                    switch (meshInfo.ChangeState)
                    {
                        case MeshChangeState.Added:
                            RaiseMeshAdded(meshObject);
                            break;
                        case MeshChangeState.Updated:
                            RaiseMeshUpdated(meshObject);
                            break;
                    }
                }
            }
            else if (SpatialMeshObjects.TryGetValue(meshInfo.MeshId.GetHashCode(), out var meshObject))
            {
                RaiseMeshRemoved(meshObject);
            }
        }
#endif // PLATFORM_LUMIN
    }
}
