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

    /// <summary> The calibration result. </summary>
    public enum NRCalibrationResult
    {
        /// <summary> The failed result of calibration. </summary>
        NR_CALIBRATION_RESULT_FAILED = -1,
        /// <summary> The successful result of calibration. </summary>
        NR_CALIBRATION_RESULT_SUCCESS = 0,
    }

    /// <summary> IMU Calibration Native API . </summary>
    public class NativeIMUCalibration
    {
        /// <summary> The native interface. </summary>
        private NativeInterface m_NativeInterface;
        /// <summary> Gets the handle of the tracking. </summary>
        /// <value> The tracking handle. </value>
        public UInt64 TrackingHandle
        {
            get
            {
                return m_NativeInterface.TrackingHandle;
            }
        }

        /// <summary> Constructor. </summary>
        /// <param name="nativeInterface"> The native interface.</param>
        public NativeIMUCalibration(NativeInterface nativeInterface)
        {
            m_NativeInterface = nativeInterface;
        }

        public bool StartImuCalibration(float delta_degress, float last_time)
        {
            NativeResult result = NativeApi.NRImuCalibrationStart(TrackingHandle, delta_degress, last_time);
            NativeErrorListener.Check(result, this, "StartImuCalibration");
            return result == NativeResult.Success;
        }

        public bool StartPhaseImuCalibration(float pitch_degree)
        {
            // NRDebugger.Info("[NativeIMUCalibration] StartPhaseImuCalibration: deltaDeg={0}", pitch_degree);
            NativeResult result = NativeApi.NRImuCalibrationPhaseStart(TrackingHandle, pitch_degree);
            NativeErrorListener.Check(result, this, "StartPhaseImuCalibration");
            return result == NativeResult.Success;
        }

        public int PhaseQueryRateImuCalibration()
        {
            int rate = 0;
            NativeResult result = NativeApi.NRImuCalibrationPhaseQueryRate(TrackingHandle, ref rate);
            NativeErrorListener.Check(result, this, "PhaseQueryRateImuCalibration");
            
            return rate;
        }

        public float QueryPitchImuCalibration()
        {
            float pitch_degree = 0;
            NativeResult result = NativeApi.NRImuCalibrationQueryPitch(TrackingHandle, ref pitch_degree);
            NativeErrorListener.Check(result, this, "QueryPitchImuCalibration");
            
            return pitch_degree;
        }

        public bool StopImuCalibration(NRCalibrationResult calibrat_result)
        {
            NativeResult result = NativeApi.NRImuCalibrationStop(TrackingHandle, (int)calibrat_result);
            NativeErrorListener.Check(result, this, "StopImuCalibration");
            return result == NativeResult.Success;
        }

        private struct NativeApi
        {
            /// <summary> NR IMU calibration starts. </summary>
            /// <param name="tracking_handle">  Handle of the tracking.</param>
            /// <param name="delta_degree">     Illegal delta degree.</param>
            /// <param name="last_time">        Last time of phase.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuCalibrationStart(UInt64 tracking_handle, float delta_degree, float last_time);

            /// <summary> NR IMU calibration phase starts. </summary>
            /// <param name="tracking_handle">  Handle of the tracking.</param>
            /// <param name="base_degree">      Target pitch degree of current phase.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuCalibrationPhaseStart(UInt64 tracking_handle, float base_degree);

            /// <summary> Query current rate of NR IMU calibration. </summary>
            /// <param name="tracking_handle">  Handle of the tracking.</param>
            /// <param name="out_rate">         Rate retrieved.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuCalibrationPhaseQueryRate(UInt64 tracking_handle, ref int out_rate);

            /// <summary> Query current pitch degree of NR . </summary>
            /// <param name="tracking_handle">  Handle of the tracking.</param>
            /// <param name="out_rate">         Pitch degree retrieved.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuCalibrationQueryPitch(UInt64 tracking_handle, ref float out_pitch_degree);

            /// <summary> NR IMU calibration stops. </summary>
            /// <param name="tracking_handle">  Handle of the tracking.</param>
            /// <param name="calibration_result">        Last time of phase.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRImuCalibrationStop(UInt64 tracking_handle, int calibration_result);
        }
    }
}
