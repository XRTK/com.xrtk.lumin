// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Lumin
{
    /// <summary>
    /// Configured level of filtering for static poses
    /// </summary>
    public enum MLPoseFilterLevel : int
    {
        /// <summary>
        /// Default value, No filtering, the poses are raw
        /// </summary>
        MLPoseFilterLevel_0,

        /// <summary>
        /// Some robustness to flicker at some cost of latency
        /// </summary>
        MLPoseFilterLevel_1,

        /// <summary>
        /// More robust to flicker at higher latency cost
        /// </summary>
        MLPoseFilterLevel_2,

        /// <summary>
        /// Ensure enum is represented as 32 bits
        /// </summary>
        MLPoseFilterLevel_Ensure32Bits = unchecked((int)0x7FFFFFFF),
    }
}