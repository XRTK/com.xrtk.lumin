// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if PLATFORM_LUMIN

using UnityEngine.XR.MagicLeap;
using XRTK.Definitions.Utilities;

namespace XRTK.Lumin.Extensions
{
    /// <summary>
    /// Shamelessly lifted from the UnityEngine.XR.MagicLeap packages, but Unity had to make their class internal. Boo.
    /// </summary>
    public static class HandednessExtensions
    {
        /// <summary>
        /// Gets the magic leap hand reference for the given handedness.
        /// </summary>
        /// <param name="handedness">Handedness to convert.</param>
        /// <returns>Magic Leap hand reference.</returns>
        public static MLHand ToMagicLeapHand(this Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.Left:
                    return MLHands.Left;
                case Handedness.Right:
                    return MLHands.Right;
                default:
                    return null;
            }
        }
    }
}

#endif // PLATFORM_LUMIN