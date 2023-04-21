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
    using System;
    using UnityEngine;

    /// <summary> A glasses magnetic data provider. </summary>
    public class NRGlassesMagneticProvider
    {
        public struct MagneticFrame
        {
            public Vector3 magnetic;
            public UInt64 timestamp;
        }

        /// <summary> Magnetic data event. </summary>
        /// <param name="frame"> The frame.</param>
        public delegate void MagneticDataEvent(MagneticFrame frame);
        /// <summary>
        /// Note: Event are called with a frequency of 1000. Do not put any heavy operations to this
        /// event. </summary>
        private static event MagneticDataEvent OnDataUpdated;

#if !UNITY_EDITOR
        /// <summary> The native magnetic API. </summary>
        private static NativeGlassesMagnetic m_NativeGlassesMagneticApi;
#endif
        /// <summary> The current frame. </summary>
        private static MagneticFrame m_CurrentFrame;
        private static object m_Lock = new object();

        /// <summary> Gets current frame. </summary>
        /// <returns> The current frame. </returns>
        public MagneticFrame GetCurrentFrame()
        {
            return m_CurrentFrame;
        }

        /// <summary>
        /// Magnetic data provider.
        /// </summary>
        /// <param name="onDataReceived">
        /// Callback to receive magnetic data.
        /// Note: 
        /// The call frequency is very fast, and it is in the child thread, 
        /// so don't put the operation that is too heavy or needs to be executed in the unity main thread here
        /// </param>
        public NRGlassesMagneticProvider(MagneticDataEvent onDataReceived = null)
        {
#if !UNITY_EDITOR
            m_NativeGlassesMagneticApi = new NativeGlassesMagnetic();
            m_NativeGlassesMagneticApi.Create(Capture);
#endif
            OnDataUpdated += onDataReceived;
        }

        /// <summary> Captures. </summary>
        /// <param name="glasses_magnetic_handle">      Handle of the glasses magnetic.</param>
        /// <param name="user_data">       Information describing the user.</param>
        [MonoPInvokeCallback(typeof(NRGlassesMagneticDataCallback))]
        private static void Capture(UInt64 glasses_magnetic_handle, UInt64 glasses_magnetic_data_handle, UInt64 user_data)
        {
            lock (m_Lock)
            {
#if !UNITY_EDITOR
                m_NativeGlassesMagneticApi.GetMagneticData(glasses_magnetic_data_handle, ref m_CurrentFrame.magnetic, ref m_CurrentFrame.timestamp);
                OnDataUpdated?.Invoke(m_CurrentFrame);
#endif
            }
            NRDebugger.Info("[NRGlassesMagneticProvider] Capture magnetic:{0} timestamp:{1}", m_CurrentFrame.magnetic, m_CurrentFrame.timestamp);
        }

        /// <summary> Starts this object. </summary>
        public void Start()
        {
#if !UNITY_EDITOR
            m_NativeGlassesMagneticApi.StartCapture();
#endif
        }

        /// <summary> Stops this object. </summary>
        public void Stop()
        {
#if !UNITY_EDITOR
            m_NativeGlassesMagneticApi.StopCapture();
#endif
        }

        /// <summary> Releases this object. </summary>
        public void Release()
        {
#if !UNITY_EDITOR
            m_NativeGlassesMagneticApi.Destroy();
            m_NativeGlassesMagneticApi = null;
#endif
            OnDataUpdated = null;
        }
    }
}
