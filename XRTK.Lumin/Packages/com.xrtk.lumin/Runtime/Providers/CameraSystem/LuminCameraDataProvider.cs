// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.CameraSystem;
using XRTK.Interfaces.CameraSystem;
using XRTK.Providers.CameraSystem;

namespace XRTK.Lumin.Providers.CameraSystem
{
    public class LuminCameraDataProvider : BaseCameraDataProvider
    {

        /// <inheritdoc />
        public LuminCameraDataProvider(string name, uint priority, BaseMixedRealityCameraDataProviderProfile profile, IMixedRealityCameraSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

        /// <inheritdoc />
        public override bool IsOpaque => false;

        private float headHeight;

        /// <inheritdoc />
        public override float HeadHeight
        {
            get => headHeight;
            set
            {
                if (value.Equals(headHeight))
                {
                    return;
                }

                headHeight = value;
            }
        }

        /// <inheritdoc />
        protected override void ResetRigTransforms()
        {
            CameraRig.PlayspaceTransform.position = new Vector3(0f, HeadHeight, 0f);
            CameraRig.PlayspaceTransform.rotation = Quaternion.identity;
            CameraRig.CameraTransform.localPosition = Vector3.zero;
            CameraRig.CameraTransform.localRotation = Quaternion.identity;
            CameraRig.BodyTransform.localPosition = Vector3.zero;
            CameraRig.BodyTransform.localRotation = Quaternion.identity;
        }
    }
}