/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary> 6-dof Trackable Image Tracking's Native API . </summary>
    internal partial class NativeAPI
    {
        private static UInt64 m_ApiHandler;

#if UNITY_ANDROID
        /// <summary> Create nrapi on android platform. </summary>
        internal static void Create(IntPtr unityActivity)
        {
            if (m_ApiHandler != 0)
                return;

            UInt64 apiHandler = 0;
            NativeApi.NRAPICreate(unityActivity, ref apiHandler);
            m_ApiHandler = apiHandler;
        }
#else
        /// <summary> Create nrapi on none-android platform. </summary>
        internal static void Create()
        {
            if (m_ApiHandler != 0)
                return;

            UInt64 apiHandler = 0;
            NativeApi.NRAPICreate(ref apiHandler);
            m_ApiHandler = apiHandler;
        }
#endif


        /// <summary> Gets the version. </summary>
        /// <returns> The version. </returns>
        internal static string GetVersion()
        {
            NRVersion version = new NRVersion();
            NativeApi.NRGetVersion(m_ApiHandler, ref version);
            return version.ToString();
        }

        /// <summary> Destroy nrapi. </summary>
        internal static void Destroy()
        {
            NativeApi.NRAPIDestroy(m_ApiHandler);   
            m_ApiHandler = 0;
        }

        private partial struct NativeApi
        {
#if UNITY_ANDROID
            /// <summary> Nr create. </summary>
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRAPICreate(IntPtr android_activity, ref UInt64 out_api_handle);
#else
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRAPICreate(ref UInt64 out_api_handle);
#endif

            /// <summary> Nr get version. </summary>
            /// <param name="out_version"> [in,out] The out version.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRGetVersion(UInt64 api_handle, ref NRVersion out_version);

            /// <summary> Nr destroy. </summary>
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRAPIDestroy(UInt64 api_handle);
        };
    }
}
