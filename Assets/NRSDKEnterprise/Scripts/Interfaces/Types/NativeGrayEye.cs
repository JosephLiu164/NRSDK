/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal.Enterprise
{
    using UnityEngine;

    /// <summary> Values that represent native gray eyes. </summary>
    public enum NativeGrayEye
    {
        /// The left grayscale camera.
        LEFT = NativeDevice.LEFT_GRAYSCALE_CAMERA,
        /// The right grayscale camera.
        RIGHT = NativeDevice.RIGHT_GRAYSCALE_CAMERA,
    }

    /// <summary> A gray eye pose data. </summary>
    public struct GrayEyePoseData
    {
        /// <summary> Left eye pose. </summary>
        public Pose LEyePose;

        /// <summary> Right eye pose. </summary>
        public Pose REyePose;
    }
}