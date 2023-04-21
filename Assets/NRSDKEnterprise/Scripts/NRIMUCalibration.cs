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
    public enum NRIMUCalibrationPhase
    {
        None = -1,
        PitchUp,
        PitchDown,
        Num,
    }

    public class NRIMUCalibration
    {
        NativeIMUCalibration m_NativeIMUCalibrationApi;

        /// <summary>
        /// IMU calibration.
        /// </summary>
        public NRIMUCalibration()
        {
#if !UNITY_EDITOR
            m_NativeIMUCalibrationApi = new NativeIMUCalibration(NRSessionManager.Instance.NativeAPI);
#endif
        }

        public bool Start(float delta_degress, float last_time)
        {
#if !UNITY_EDITOR
            return m_NativeIMUCalibrationApi.StartImuCalibration(delta_degress, last_time);
#else
            return true;
#endif
        }

        public bool StartPhase(float pitch_degree)
        {
#if !UNITY_EDITOR
            return m_NativeIMUCalibrationApi.StartPhaseImuCalibration(pitch_degree);
#else
            return true;
#endif
        }
    
        public int PhaseQueryRate()
        {
#if !UNITY_EDITOR
            return m_NativeIMUCalibrationApi.PhaseQueryRateImuCalibration();
#else
            return 50;
#endif
        }
    
        public float QueryPitch()
        {
#if !UNITY_EDITOR
            return m_NativeIMUCalibrationApi.QueryPitchImuCalibration();
#else
            return 50.0f;
#endif
        }

        public bool Stop(NRCalibrationResult result)
        {
#if !UNITY_EDITOR
            return m_NativeIMUCalibrationApi.StopImuCalibration(result);
#else
            return true;
#endif
        }
    }
}