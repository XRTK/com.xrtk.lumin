// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Interfaces.SpatialAwarenessSystem;
using XRTK.Lumin.Profiles;
using XRTK.Providers.SpatialObservers;

#if PLATFORM_LUMIN
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.MagicLeap;
using XRTK.Definitions.SpatialAwarenessSystem;
using XRTK.Services;

#endif // PLATFORM_LUMIN

namespace XRTK.Lumin.SpatialObservers
{
    /// <summary>
    /// Lumin Spatial Mesh Observer
    /// </summary>
    public class LuminSpatialMeshObserver : BaseMixedRealitySpatialMeshObserver
    {
        /// <inheritdoc />
        public LuminSpatialMeshObserver(string name, uint priority, LuminSpatialMeshObserverProfile profile, IMixedRealitySpatialAwarenessSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

#if PLATFORM_LUMIN

        private readonly List<XRMeshSubsystemDescriptor> descriptors = new List<XRMeshSubsystemDescriptor>();

        private readonly List<MeshInfo> meshInfos = new List<MeshInfo>();

        private XRMeshSubsystem meshSubsystem;

        private float lastUpdated = 0;

        #region IMixedRealityService implementation

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (!Application.isPlaying || Application.isEditor || meshSubsystem != null) { return; }

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
            if (!IsRunning ||
                meshSubsystem == null ||
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

            ConfigureObserverVolume();

            if (meshSubsystem.TryGetMeshInfos(meshInfos))
            {
                for (int i = 0; i < meshInfos.Count; i++)
                {
                    MeshInfo_Update(meshInfos[i]);
                }
            }

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

            base.StartObserving();

            meshSubsystem?.Start();

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

            meshSubsystem?.Stop();

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

            flags |= LuminApi.MeshingFlags.Planarize;
            flags |= LuminApi.MeshingFlags.RemoveMeshSkirt;

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
                spatialMeshObject.GameObject.name = $"SpatialMesh_{meshInfo.MeshId}";

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
                        Debug.LogWarning($"No output for {result.MeshId} | {result.Status}");
                        RaiseMeshRemoved(spatialMeshObject);
                        return;
                    }

                    if (!SpatialMeshObjects.TryGetValue(result.MeshId.GetHashCode(), out var meshObject))
                    {
                        Debug.LogWarning($"Failed to find a spatial mesh object for {result.MeshId}!");
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
