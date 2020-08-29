// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace XRTK.Lumin
{
    /// <summary>
    /// Configured level for keypoints filtering of keypoints and hand centers
    /// </summary>
    public enum MLKeyPointFilterLevel : int
    {
        /// <summary>
        /// Default value, no filtering is done, the points are raw
        /// </summary>
        MLKeypointFilterLevel_0,

        /// <summary>
        /// Some smoothing at the cost of latency
        /// </summary>
        MLKeypointFilterLevel_1,

        /// <summary>
        /// Predictive smoothing, at higher cost of latency
        /// </summary>
        MLKeypointFilterLevel_2,

        /// <summary>
        /// Ensure enum is represented as 32 bits
        /// </summary>
        MLKeypointFilterLevel_Ensure32Bits = unchecked((int)0x7FFFFFFF),
    }
}