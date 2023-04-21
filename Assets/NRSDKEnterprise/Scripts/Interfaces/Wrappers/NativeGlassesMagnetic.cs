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
    using UnityEngine;

    /// <summary> The callback method type which will be called when an magnetic data is ready.</summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NRGlassesMagneticDataCallback(UInt64 glasses_magnetic_handle,
                                        UInt64 glasses_magnetic_data_handle, UInt64 user_data);

    /// <summary> Native GlassesMagnetic API.  </summary>
    internal class NativeGlassesMagnetic
    {
        private static UInt64 m_GlassesMagneticHandle;

        public bool Create(NRGlassesMagneticDataCallback callback)
        {
            var result = NativeApi.NRGlassesMagneticCreate(ref m_GlassesMagneticHandle);
            NativeErrorListener.Check(result, this, "Create");
            if (result != NativeResult.Success)
            {
                return false;
            }
            result = NativeApi.NRGlassesMagneticSetCaptureCallback(m_GlassesMagneticHandle, callback, 0);
            return result == NativeResult.Success;
        }

        public bool StartCapture()
        {
            var result = NativeApi.NRGlassesMagneticStartCapture(m_GlassesMagneticHandle);
            NativeErrorListener.Check(result, this, "StartCapture");
            return result == NativeResult.Success;
        }

        public bool StopCapture()
        {
            var result = NativeApi.NRGlassesMagneticStopCapture(m_GlassesMagneticHandle);
            NativeErrorListener.Check(result, this, "StopCapture");
            return result == NativeResult.Success;
        }

        public void GetMagneticData(UInt64 glasses_magnetic_data_handle, ref Vector3 magnetic, ref UInt64 timestamp)
        {
            NativeVector3f data = NativeVector3f.identity;
            NativeApi.NRGlassesMagneticDataGetData(m_GlassesMagneticHandle, glasses_magnetic_data_handle, ref data);
            NativeApi.NRGlassesMagneticDataGetHMDTimeNanos(m_GlassesMagneticHandle, glasses_magnetic_data_handle, ref timestamp);
            NativeApi.NRGlassesMagneticDataDestroy(m_GlassesMagneticHandle, glasses_magnetic_data_handle);
            magnetic.x = data.X;
            magnetic.y = data.Y;
            magnetic.z = data.Z;

            Debug.LogFormat("data:{0} timestamp:{1}", data.ToString(), timestamp);
        }

        public void Destroy()
        {
            var result = NativeApi.NRGlassesMagneticDestroy(m_GlassesMagneticHandle);
            NativeErrorListener.Check(result, this, "Release");
        }

        private partial struct NativeApi
        {
            /// @brief Destroy the magnetic data object.
            /// @param[in] glasses_magnetic_handle The handle of glasses magnetic object.
            /// @param[in] glasses_magnetic_data_handle  The handle of glasses magnetic data.
            /// @return The result of operation.
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesMagneticDataDestroy(UInt64 glasses_magnetic_handle,
                                                               UInt64 glasses_magnetic_data_handle);

            /// @brief Get the time of current magnetic data.
            /// @param[in] glasses_magnetic_handle The handle of glasses magnetic object.
            /// @param[in] glasses_magnetic_data_handle  The handle of glasses magnetic data.
            /// @param[out] out_hmd_time_nanos  The time of the data.
            /// @return The result of operation.
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesMagneticDataGetHMDTimeNanos(UInt64 glasses_magnetic_handle, UInt64 glasses_magnetic_data_handle,
                                     ref UInt64 out_hmd_time_nanos);

            /// @brief Get the magnetic values of the sensor.
            /// @param[in] glasses_magnetic_handle The handle of glasses magnetic object.
            /// @param[in] glasses_magnetic_data_handle  The handle of glasses magnetic data.
            /// @param[out] out_magnetic  The magnetic values, and they are in uT and 
            ///                             x point to up, y point to right, z point to forward.
            /// @return The result of operation.
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesMagneticDataGetData(
                UInt64 glasses_magnetic_handle, UInt64 glasses_magnetic_data_handle, ref NativeVector3f out_magnetic);

            /// @brief Create the glasses magnetic object.
            /// @param[out]	out_glasses_magnetic_handle The handle of glasses magnetic object.
            /// @return The result of operation.
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesMagneticCreate(ref UInt64 out_glasses_magnetic_handle);

            /// @brief Set the callback method .
            /// @param[in]	glasses_magnetic_handle The handle of glasses magnetic object.
            /// @param[in]	data_callback The callback function.
            /// @param[in]	user_data The data which will be returned when callback is triggered.
            /// @return The result of operation.
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesMagneticSetCaptureCallback(UInt64 glasses_magnetic_handle, NRGlassesMagneticDataCallback data_callback, UInt64 user_data);

            /// @brief Start magnetic data capture.
            /// @param[in]	glasses_magnetic_handle The handle of glasses magnetic object.
            /// @return The result of operation.
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesMagneticStartCapture(UInt64 glasses_magnetic_handle);

            /// @brief Stop magnetic data capture.
            /// @param[in]	glasses_magnetic_handle The handle of glasses magnetic object.
            /// @return The result of operation.
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesMagneticStopCapture(UInt64 glasses_magnetic_handle);

            /// @brief Release memory used by the glasses magnetic object.
            /// @param[in]	glasses_magnetic_handle The handle of glasses magnetic object.
            /// @return The result of operation.
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesMagneticDestroy(UInt64 glasses_magnetic_handle);
        }
    }
}
