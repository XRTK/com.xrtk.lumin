// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Providers.Controllers.Hands;
using XRTK.Lumin.Profiles;
using UnityEngine;
using System;
using System.Linq;

#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace XRTK.Lumin.Controllers
{
    /// <summary>
    /// Hand controller data provier for the Lumin platform.
    /// </summary>
    public class LuminHandControllerDataProvider : BaseHandControllerDataProvider
    {
        private readonly LuminHandControllerDataProviderProfile profile;
        private MLHandKeyPose[] keyPoses;

        /// <summary>
        /// Creates a new instance of the data provider.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Hand controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public LuminHandControllerDataProvider(string name, uint priority, LuminHandControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
            this.profile = profile;
        }

#if PLATFORM_LUMIN

        public override void Initialize()
        {
            base.Initialize();
            keyPoses = Enum.GetValues(typeof(MLHandKeyPose)).Cast<MLHandKeyPose>().ToArray();
        }

        public override void Enable()
        {
            if (!MLHands.IsStarted)
            {
                var result = MLHands.Start();
                if (!result.IsOk)
                {
                    Debug.LogError($"Error: Failed starting MLHands: {result}");
                    return;
                }

                bool status = MLHands.KeyPoseManager.EnableKeyPoses(keyPoses, true, true);
                if (!status)
                {
                    Debug.LogError("Error: Failed enabling tracked key poses.");
                    return;
                }

                MLHands.KeyPoseManager.SetKeyPointsFilterLevel(profile.KeyPointFilterLevel);
                MLHands.KeyPoseManager.SetPoseFilterLevel(profile.PoseFilterLevel);
            }
        }

        public override void Disable()
        {
            if (MLHands.IsStarted)
            {
                MLHands.Stop();
                MLHands.KeyPoseManager.EnableKeyPoses(keyPoses, false, true);
            }

            base.Disable();
        }

#endif
    }
}