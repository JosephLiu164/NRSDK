/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace NRKernal.Enterprise.NRExamples
{
    /// <summary> A controller for handling imu data. </summary>
    public class ImuDataController : MonoBehaviour
    {
        /// <summary> The nrimu provider. </summary>
        private NRIMUDataProvider m_NRIMUProvider;
        /// <summary> The FPS. </summary>
        public Text mFPS;
        /// <summary> The time stamp. </summary>
        public Text mTimeStamp;
        /// <summary> The accelerometer. </summary>
        public Text mAccelerometer;
        /// <summary> The gyroscope. </summary>
        public Text mGyroscope;
        /// <summary> Number of currents. </summary>
        private int currentCount;
        /// <summary> Number of last frames. </summary>
        private int lastFrameCount;
        /// <summary> The time last. </summary>
        private float time_last = 0;

        /// <summary> Starts this object. </summary>
        void Start()
        {
            m_NRIMUProvider = new NRIMUDataProvider(OnUpdate);
            m_NRIMUProvider.Start();
        }

        /// <summary> Executes the 'update' action. </summary>
        /// <param name="frame"> The frame.</param>
        private void OnUpdate(IMUDataFrame frame)
        {
            currentCount++;
        }

        /// <summary> Updates this object. </summary>
        void Update()
        {
            if (m_NRIMUProvider == null)
            {
                return;
            }

            if (time_last > 1f)
            {
                var frame = m_NRIMUProvider.GetCurrentFrame();
                mTimeStamp.text = frame.timeStamp.ToString();
                mAccelerometer.text = frame.accelerometer.ToString();
                mGyroscope.text = frame.gyroscope.ToString();
                mFPS.text = ((currentCount - lastFrameCount) * 1f / Time.deltaTime).ToString();
                time_last = 0f;
            }

            time_last += Time.deltaTime;
            lastFrameCount = currentCount;
        }
    }
}
