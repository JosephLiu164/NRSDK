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
    using System;
    using UnityEngine;
    using System.Runtime.InteropServices;

    // The enum of channel type.
    public enum NRChannelType
    {
        NR_SENSOR_TYPE_RGB_CAMERA = 1,
    }

    /// <summary> HMD Gray Eye offset Native API . </summary>
    public static class NativeHMDExtension
    {
        /// <summary> Get if the specified channel is enable(for example, RGBCamera is disable while PowerSavingMode is open). </summary>
        /// <param name="nativehmd"> The nativehmd to act on.</param>
        /// <param name="channelType"> The channel type to act on.</param>
        /// <returns> Is the specified channel enable or not. </returns>
        public static bool NRHMDIsChannelEnabled(this NativeHMD nativehmd, NRChannelType channelType)
        {
            bool enable = true;
            NativeResult result = NativeApi.NRHMDIsChannelEnabled(nativehmd.HmdHandle, channelType, ref enable);
            return result == NativeResult.Success ? enable : true;
        }

        /// <summary> Get if RGBCamera is enable(It's disable while PowerSavingMode is open). </summary>
        /// <param name="nativehmd"> The nativehmd to act on.</param>
        /// <param name="ipd"> IPD of the two eyes in meters.</param>
        /// <returns> True if it succeeds, false if it fails.</returns>
        public static bool NRHMDUpdateIPD(this NativeHMD nativehmd, float ipd)
        {
            NativeResult result = NativeApi.NRHMDUpdateIPD(nativehmd.HmdHandle, ipd);
            return result == NativeResult.Success;
        }

        /// <summary> Get the offline bias data for the accelerometer. </summary>
        /// <param name="nativehmd"> The nativehmd to act on. </param>
        /// <param name="bias"> The output bias data for the accelerometer. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public static bool GetIMUAccelerometerBias(this NativeHMD nativehmd, ref Vector3 bias)
        {
            NativeVector3f nativeBias = new NativeVector3f();
            NativeResult result = NativeApi.NRHMDGetIMUAccelerometerBias(nativehmd.HmdHandle, ref nativeBias);
            bias = nativeBias.ToUnityVector3();
            return result == NativeResult.Success;
        }

        /// <summary> Get the offline bias data for the gyroscope. </summary>
        /// <param name="nativehmd"> The nativehmd to act on. </param>
        /// <param name="bias"> The output bias data for the gyroscope. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public static bool GetIMUGyroscopeBias(this NativeHMD nativehmd, ref Vector3 bias)
        {
            NativeVector3f nativeBias = new NativeVector3f();
            NativeResult result = NativeApi.NRHMDGetIMUGyroscopeBias(nativehmd.HmdHandle, ref nativeBias);
            bias = nativeBias.ToUnityVector3();
            return result == NativeResult.Success;
        }

        /// <summary> A native api. </summary>
        private struct NativeApi
        {
            /// <summary> Get if custom channel is enabled. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <param name="channel_type"> The type of sensor channel.</param>
            /// <param name="out_is_enabled"> [in,out] The result that if the specified channel is enable or not.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDIsChannelEnabled(UInt64 hmd_handle, NRChannelType channel_type,  ref bool out_is_enabled);

            /// <summary> Get the offline bias data for the accelerometer. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <param name="out_bias"> The output bias data for the accelerometer.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetIMUAccelerometerBias(UInt64 hmd_handle, ref NativeVector3f out_bias);

            /// <summary> Get the offline bias data for the gyroscope. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <param name="out_bias"> The output bias data for the gyroscope.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetIMUGyroscopeBias(UInt64 hmd_handle, ref NativeVector3f out_bias);

            /// <summary> Set specific IPD values and update related parameters. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <param name="eye_ipd"> IPD of two eyes in meters.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDUpdateIPD(UInt64 hmd_handle, float eye_ipd);
        }
    }
}
