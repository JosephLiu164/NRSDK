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
    using System.Runtime.InteropServices;

    /// <summary> IMU data Native API . </summary>
    public class NativeIMUData
    {
        /// <summary> Callback, called when the imu data. </summary>
        /// <param name="imu_handle">      Handle of the imu.</param>
        /// <param name="imu_data_handle"> Handle of the imu data.</param>
        /// <param name="user_data">       Information describing the user.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void IMUDataCallback(UInt64 imu_handle, UInt64 imu_data_handle, UInt64 user_data);
        /// <summary> Handle of the imu. </summary>
        private UInt64 m_IMUHandle;

        /// <summary> Creates a new bool. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Create()
        {
            NativeResult result = NativeApi.NRImuCreate(ref m_IMUHandle);
            NativeErrorListener.Check(result, this, "Create");
            return result == NativeResult.Success;
        }

        /// <summary> Starts a capture. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool StartCapture()
        {
            var result = NativeApi.NRImuStartCapture(m_IMUHandle);
            return result == NativeResult.Success;
        }

        /// <summary> Stops a capture. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool StopCapture()
        {
            var result = NativeApi.NRImuStopCapture(m_IMUHandle);
            return result == NativeResult.Success;
        }

        /// <summary> Callback, called when the regist. </summary>
        /// <param name="callback"> The callback.</param>
        /// <param name="userdata"> (Optional) The userdata.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool RegistCallback(IMUDataCallback callback, UInt64 userdata = 0)
        {
            var result = NativeApi.NRImuSetCaptureCallback(m_IMUHandle, callback, userdata);
            return result == NativeResult.Success;
        }

        /// <summary> Gets a frame. </summary>
        /// <param name="imu_data_handle"> Handle of the imu data.</param>
        /// <param name="frame">           [in,out] The frame.</param>
        /// <returns> The frame. </returns>
        public IMUDataFrame GetFrame(UInt64 imu_data_handle, ref IMUDataFrame frame)
        {
            NativeApi.NRImuDataGetHMDTimeNanos(m_IMUHandle, imu_data_handle, ref frame.timeStamp);
            NativeApi.NRImuDataGetGyroscope(m_IMUHandle, imu_data_handle, ref frame.gyroscope);
            NativeApi.NRImuDataGetAccelerometer(m_IMUHandle, imu_data_handle, ref frame.accelerometer);
            NativeApi.NRImuDataDestroy(m_IMUHandle, imu_data_handle);
            return frame;
        }

        /// <summary> Destroys this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Destroy()
        {
            NativeResult result = NativeApi.NRImuDestroy(m_IMUHandle);
            NativeErrorListener.Check(result, this, "Destroy");
            return result == NativeResult.Success;
        }

        /// <summary> A native api. </summary>
        private struct NativeApi
        {
            /// <summary> Nr imu create. </summary>
            /// <param name="out_imu_handle"> [in,out] Handle of the out imu.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuCreate(ref UInt64 out_imu_handle);

            /// <summary> Nr imu data get hmd time nanos. </summary>
            /// <param name="imu_handle">         Handle of the imu.</param>
            /// <param name="imu_data_handle">    Handle of the imu data.</param>
            /// <param name="out_hmd_time_nanos"> [in,out] The out hmd time nanos.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuDataGetHMDTimeNanos(UInt64 imu_handle,
                UInt64 imu_data_handle, ref UInt64 out_hmd_time_nanos);

            /// <summary> Nr imu data get gyroscope. </summary>
            /// <param name="imu_handle">      Handle of the imu.</param>
            /// <param name="imu_data_handle"> Handle of the imu data.</param>
            /// <param name="out_gyroscope">   [in,out] The out gyroscope.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuDataGetGyroscope(UInt64 imu_handle,
                UInt64 imu_data_handle, ref NativeVector3f out_gyroscope);

            /// <summary> Nr imu data get accelerometer. </summary>
            /// <param name="imu_handle">        Handle of the imu.</param>
            /// <param name="imu_data_handle">   Handle of the imu data.</param>
            /// <param name="out_accelerometer"> [in,out] The out accelerometer.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuDataGetAccelerometer(UInt64 imu_handle,
                UInt64 imu_data_handle, ref NativeVector3f out_accelerometer);

            /// <summary> Callback, called when the nr imu set capture. </summary>
            /// <param name="imu_handle">    Handle of the imu.</param>
            /// <param name="data_callback"> The data callback.</param>
            /// <param name="user_data">     Information describing the user.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuSetCaptureCallback(UInt64 imu_handle,
                IMUDataCallback data_callback, UInt64 user_data);

            /// <summary> Nr imu data destroy. </summary>
            /// <param name="imu_handle">      Handle of the imu.</param>
            /// <param name="imu_data_handle"> Handle of the imu data.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuDataDestroy(UInt64 imu_handle, UInt64 imu_data_handle);

            /// <summary> Nr imu start capture. </summary>
            /// <param name="imu_handle"> Handle of the imu.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuStartCapture(UInt64 imu_handle);

            /// <summary> Nr imu stop capture. </summary>
            /// <param name="imu_handle"> Handle of the imu.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuStopCapture(UInt64 imu_handle);

            /// <summary> Nr imu destroy. </summary>
            /// <param name="imu_handle"> Handle of the imu.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuDestroy(UInt64 imu_handle);
        };
    }
}
