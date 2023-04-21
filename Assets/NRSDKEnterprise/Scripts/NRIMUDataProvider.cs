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
    using AOT;

    /// <summary> A nrimu data provider. </summary>
    public class NRIMUDataProvider
    {
        /// <summary> Imu data event. </summary>
        /// <param name="frame"> The frame.</param>
        public delegate void IMUDataEvent(IMUDataFrame frame);
        /// <summary>
        /// Note: Event are called with a frequency of 1000. Do not put any heavy operations to this
        /// event. </summary>
        private static event IMUDataEvent OnUpdate;

#if !UNITY_EDITOR
        /// <summary> The native imu API. </summary>
        private static NativeIMUData m_NativeIMUApi;
#endif
        /// <summary> The current frame. </summary>
        private static IMUDataFrame m_CurrentFrame;
        private static object m_Lock = new object();
        private IMUDataEvent onDataUpdated;

        /// <summary> Gets current frame. </summary>
        /// <returns> The current frame. </returns>
        public IMUDataFrame GetCurrentFrame()
        {
            lock (m_Lock)
            {
                return m_CurrentFrame;
            }
        }

        /// <summary>
        /// IMU data provider.
        /// </summary>
        /// <param name="onDataReceived">
        /// Callback to receive imu data.
        /// Note: 
        /// The call frequency is very fast, and it is in the child thread, 
        /// so don't put the operation that is too heavy or needs to be executed in the unity main thread here
        /// </param>
        public NRIMUDataProvider(IMUDataEvent onDataReceived = null)
        {
#if !UNITY_EDITOR
            m_NativeIMUApi = new NativeIMUData();
            m_NativeIMUApi.Create();
            m_NativeIMUApi.RegistCallback(Capture);
#endif
            if (onDataReceived != null)
            {
                onDataUpdated = onDataReceived;
                OnUpdate += onDataUpdated;
            }
        }

        /// <summary> Captures. </summary>
        /// <param name="imu_handle">      Handle of the imu.</param>
        /// <param name="imu_data_handle"> Handle of the imu data.</param>
        /// <param name="user_data">       Information describing the user.</param>
        [MonoPInvokeCallback(typeof(NativeIMUData.IMUDataCallback))]
        private static void Capture(ulong imu_handle, ulong imu_data_handle, ulong user_data)
        {
            lock (m_Lock)
            {
#if !UNITY_EDITOR
                m_NativeIMUApi.GetFrame(imu_data_handle, ref m_CurrentFrame);
                OnUpdate?.Invoke(m_CurrentFrame);
#endif
            }
        }

        /// <summary> Starts this object. </summary>
        public void Start()
        {
#if !UNITY_EDITOR
            m_NativeIMUApi.StartCapture();
#endif
        }

        /// <summary> Stops this object. </summary>
        public void Stop()
        {
#if !UNITY_EDITOR
            m_NativeIMUApi.StopCapture();
#endif
            OnUpdate -= onDataUpdated;
        }

        /// <summary> Releases this object. </summary>
        public void Release()
        {
#if !UNITY_EDITOR
            m_NativeIMUApi.Destroy();
            m_NativeIMUApi = null;
#endif
            onDataUpdated = null;
            OnUpdate -= onDataUpdated;
        }
    }
}
