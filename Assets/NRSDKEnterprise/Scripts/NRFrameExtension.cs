/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using System;
using UnityEngine;

namespace NRKernal.Enterprise
{
    /// <summary> A nr frame extension. </summary>
    public class NRFrameExtension
    {
        /// <summary> Information describing the gray eye pose. </summary>
        private static GrayEyePoseData m_GrayEyePoseData;
        /// <summary> Get the offset position between gray camera and head. </summary>
        /// <value> The gray eye pose from head. </value>
        public static GrayEyePoseData GrayEyePoseFromHead
        {
            get
            {
#if !UNITY_EDITOR
                if (NRFrame.SessionStatus == SessionState.Running)
                {
                    m_GrayEyePoseData.LEyePose = NRFrame.GetDevicePoseFromHead(NativeDevice.LEFT_GRAYSCALE_CAMERA);
                    m_GrayEyePoseData.REyePose = NRFrame.GetDevicePoseFromHead(NativeDevice.RIGHT_GRAYSCALE_CAMERA);
                }
#else
                m_GrayEyePoseData.LEyePose = new UnityEngine.Pose(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
                m_GrayEyePoseData.REyePose = new UnityEngine.Pose(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
#endif
                return m_GrayEyePoseData;
            }
        }


        /// <summary> Information describing the magnetic pose. </summary>
        private static Pose m_MagneticPose;
        /// <summary> Get the offset position between magnetic and head. </summary>
        /// <value> The magnetic pose from head. </value>
        public static Pose MagneticPoseFromHead
        {
            get
            {
#if !UNITY_EDITOR
                if (NRFrame.SessionStatus == SessionState.Running)
                {
                    m_MagneticPose = NRFrame.GetDevicePoseFromHead(NativeDevice.MAGENTICE);
                }
#else
                m_MagneticPose = new UnityEngine.Pose(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
#endif
                return m_MagneticPose;
            }
        }

        /// <summary> Get the intrinsic matrix of gray camera. </summary>
        /// <param name="eye"> gray camera inde.</param>
        /// <returns> The gray camera intrinsic matrix. </returns>
        public static NativeMat3f GetGrayCameraIntrinsicMatrix(NativeGrayEye eye)
        {
            return NRFrame.GetDeviceIntrinsicMatrix((NativeDevice)((int)eye));
        }

        /// <summary> Get the Distortion of gray camera. </summary>
        /// <param name="eye"> gray camera inde.</param>
        /// <returns> The gray camera distortion. </returns>
        public static NRDistortionParams GetGrayCameraDistortion(NativeGrayEye eye)
        {
            return NRFrame.GetDeviceDistortion((NativeDevice)((int)eye));
        }

        /// <summary> Get the resolution of gray camera. </summary>
        /// <param name="eye"> gray camera index.</param>
        /// <returns> The gray camera resolution. </returns>
        public static NativeResolution GetGrayCameraResolution(NativeGrayEye eye)
        {
            return NRFrame.GetDeviceResolution((NativeDevice)((int)eye));
        }

        /// <summary> Gets head pose extended by timestamp. </summary>
        /// <param name="pose">            [in,out] The pose.</param>
        /// <param name="linearVelocity">  [in,out] The out linear velocity.</param>
        /// <param name="angularVelocity"> [in,out] The out angular velocity.</param>
        /// <param name="accBias">         [in,out] The out acc bias.</param>
        /// <param name="gyroBias">        [in,out] The out gyro bias.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public static bool GetHeadPoseExtended(ref Pose pose,
                ref Vector3 linearVelocity, ref Vector3 angularVelocity, ref Vector3 accBias, ref Vector3 gyroBias)
        {
            return NRSessionManager.Instance.NativeAPI.NativeHeadTracking.GetHeadPoseExtended(NRFrame.CurrentPoseTimeStamp, ref pose,
                ref linearVelocity, ref angularVelocity, ref accBias, ref gyroBias);
        }
    }
}
