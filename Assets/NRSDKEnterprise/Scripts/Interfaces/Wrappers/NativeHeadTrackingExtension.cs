/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          

* NRSDK is distributed in the hope that it will be usefull                                                              

* https://www.nreal.ai/       
* 
*****************************************************************************/

namespace NRKernal.Enterprise
{
    using System;
    using UnityEngine;
    using System.Runtime.InteropServices;

    /// <summary> Only Internal Test Not Release !!!!! </summary>
    public static class NativeHeadTrackingExtension
    {
        /// <summary> A NativeHeadTracking extension method that gets eye pose. </summary>
        /// <param name="headTracking">    The headTracking to act on.</param>
        /// <param name="outLeftEyePose">  [in,out] The out left eye pose.</param>
        /// <param name="outRightEyePose"> [in,out] The out right eye pose.</param>
        /// <returns> The eye pose. </returns>
        public static NativeResult GetEyePose(this NativeHeadTracking headTracking, ref Pose outLeftEyePose, ref Pose outRightEyePose)
        {
            NativeMat4f lefteyepos = new NativeMat4f(Matrix4x4.identity);
            NativeMat4f righteyepos = new NativeMat4f(Matrix4x4.identity);
            NativeResult result = NativeApi.NRHeadTrackingGetEyePose(headTracking.TrackingHandle, headTracking.HeadTrackingHandle, ref lefteyepos, ref righteyepos);

            if (result == NativeResult.Success)
            {
                ConversionUtility.ApiPoseToUnityPose(lefteyepos, out outLeftEyePose);
                ConversionUtility.ApiPoseToUnityPose(righteyepos, out outRightEyePose);
            }
            NRDebugger.Info("[NativeHeadTracking] GetEyePose :" + result);
            return result;
        }

        /// <summary> A NativeHeadTracking extension method that gets projection matrix. </summary>
        /// <param name="headTracking">             The headTracking to act on.</param>
        /// <param name="outLeftProjectionMatrix">  [in,out] The out left projection matrix.</param>
        /// <param name="outRightProjectionMatrix"> [in,out] The out right projection matrix.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public static bool GetProjectionMatrix(this NativeHeadTracking headTracking, ref Matrix4x4 outLeftProjectionMatrix, ref Matrix4x4 outRightProjectionMatrix)
        {
            NativeMat4f projectmatrix = new NativeMat4f(Matrix4x4.identity);
            NativeResult result_left = NativeApi.NRInternalGetProjectionMatrix((int)NativeDevice.LEFT_DISPLAY, ref projectmatrix);
            outLeftProjectionMatrix = projectmatrix.ToUnityMat4f();
            NativeResult result_right = NativeApi.NRInternalGetProjectionMatrix((int)NativeDevice.RIGHT_DISPLAY, ref projectmatrix);
            outRightProjectionMatrix = projectmatrix.ToUnityMat4f();
            return (result_left == NativeResult.Success && result_right == NativeResult.Success);
        }

        public static bool GetHeadPoseExtended(this NativeHeadTracking headTracking, UInt64 timestamp, ref Pose pose,
                ref Vector3 linearVelocity, ref Vector3 angularVelocity, ref Vector3 accBias, ref Vector3 gyroBias)
        {
            if (headTracking.TrackingHandle == 0)
            {
                NRDebugger.Error("[NativeTrack] GetHeadPoseExtended: trackingHandle is zero");
                pose = Pose.identity;
                linearVelocity = Vector3.zero;
                angularVelocity = Vector3.zero;
                accBias = Vector3.zero;
                gyroBias = Vector3.zero;
                return false;
            }
            UInt64 headPoseHandle = 0;
            var acquireTrackingPoseResult = NativeHeadTracking.NativeApi.NRHeadTrackingAcquireTrackingPose(headTracking.TrackingHandle, headTracking.HeadTrackingHandle, timestamp, ref headPoseHandle);
            if (acquireTrackingPoseResult != NativeResult.Success)
            {
                NRDebugger.Info("[NativeTrack] GetHeadPoseExtended: {0}", acquireTrackingPoseResult);
                return false;
            }

            NativeMat4f headpos_native = new NativeMat4f(Matrix4x4.identity);
            NativeVector3f linear_velocity = new NativeVector3f();
            NativeVector3f angular_velocity = new NativeVector3f();
            NativeVector3f acc_bias = new NativeVector3f();
            NativeVector3f gyro_bias = new NativeVector3f();
            var getPoseResult = NativeApi.NRTrackingPoseGetPoseExtended(headTracking.TrackingHandle, headPoseHandle,
                ref headpos_native, ref linear_velocity, ref angular_velocity, ref acc_bias, ref gyro_bias);
            NativeErrorListener.Check(getPoseResult, headTracking, "GetFramePresentHeadPose");
            ConversionUtility.ApiPoseToUnityPose(headpos_native, out pose);
            linearVelocity = linear_velocity.ToUnityVector3();
            angularVelocity = angular_velocity.ToUnityVector3();
            accBias = acc_bias.ToUnityVector3();
            gyroBias = gyro_bias.ToUnityVector3();
            NativeHeadTracking.NativeApi.NRTrackingPoseDestroy(headTracking.TrackingHandle, headPoseHandle);
            return true;
        }

        /// <summary> A native api. </summary>
        private struct NativeApi
        {
            /// <summary> Nr head tracking get eye pose. </summary>
            /// <param name="session_handle">       Handle of the session.</param>
            /// <param name="head_tracking_handle"> Handle of the head tracking.</param>
            /// <param name="left_eye">             [in,out] The left eye.</param>
            /// <param name="right_eye">            [in,out] The right eye.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingGetEyePose(UInt64 session_handle, UInt64 head_tracking_handle, ref NativeMat4f left_eye, ref NativeMat4f right_eye);

            /// <summary> Nr internal get projection matrix. </summary>
            /// <param name="index"> Zero-based index of the.</param>
            /// <param name="eye">   [in,out] The eye.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRInternalGetProjectionMatrix(int index, ref NativeMat4f eye);

            /// <summary> Nr tracking pose get pose extended. </summary>
            /// <param name="tracking_handle">      Handle of the tracking.</param>
            /// <param name="tracking_pose_handle"> Handle of the tracking pose.</param>
            /// <param name="out_pose">             [in,out] The out pose.</param>
            /// <param name="out_linear_velocity">  [in,out] The out linear velocity.</param>
            /// <param name="out_angular_velocity"> [in,out] The out angular velocity.</param>
            /// <param name="out_acc_bias">         [in,out] The out acc bias.</param>
            /// <param name="out_gyro_bias">        [in,out] The out gyro bias.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackingPoseGetPoseExtended(UInt64 tracking_handle,
                                                            UInt64 tracking_pose_handle,
                                                            ref NativeMat4f out_pose,
                                                            ref NativeVector3f out_linear_velocity,
                                                            ref NativeVector3f out_angular_velocity,
                                                            ref NativeVector3f out_acc_bias,
                                                            ref NativeVector3f out_gyro_bias);
        };
    }
}
