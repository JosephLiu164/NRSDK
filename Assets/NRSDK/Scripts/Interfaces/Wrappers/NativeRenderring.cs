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
    using UnityEngine;
    using System.Runtime.InteropServices;
    using System.Diagnostics;

    /// <summary>
    /// HMD Eye offset Native API .
    /// </summary>
    internal class NativeRenderring
    {
        private UInt64 m_RenderingHandle = 0;
        public UInt64 RenderingHandle
        {
            get
            {
                return m_RenderingHandle;
            }
        }

        public NativeRenderring()
        {
        }

        ~NativeRenderring()
        {
        }

        public bool Create()
        {
            var result = NativeApi.NRRenderingCreate(ref m_RenderingHandle);
            NativeErrorListener.Check(result, this, "Create", true);

#if !UNITY_STANDALONE_WIN
            NativeColorSpace colorspace = QualitySettings.activeColorSpace == ColorSpace.Gamma ?
               NativeColorSpace.COLOR_SPACE_GAMMA : NativeColorSpace.COLOR_SPACE_LINEAR;
            NativeApi.NRRenderingInitSetTextureColorSpace(m_RenderingHandle, colorspace);
#endif
            return result == NativeResult.Success;
        }

        public bool Start()
        {
            if (m_RenderingHandle == 0)
            {
                return false;
            }

            var result = NativeApi.NRRenderingStart(m_RenderingHandle);
            NativeErrorListener.Check(result, this, "Start", true);
            return result == NativeResult.Success;
        }

        public bool Pause()
        {
            if (m_RenderingHandle == 0)
            {
                return false;
            }

            var result = NativeApi.NRRenderingPause(m_RenderingHandle);
            NativeErrorListener.Check(result, this, "Pause", true);
            return result == NativeResult.Success;
        }

        public bool Resume()
        {
            if (m_RenderingHandle == 0)
            {
                return false;
            }

            var result = NativeApi.NRRenderingResume(m_RenderingHandle);
            NativeErrorListener.Check(result, this, "Resume", true);
            return result == NativeResult.Success;
        }

        public void GetFramePresentTime(ref UInt64 present_time)
        {
            if (m_RenderingHandle == 0)
            {
                return;
            }

            NativeApi.NRRenderingGetFramePresentTime(m_RenderingHandle, ref present_time);
        }

        public bool Stop()
        {
            if (m_RenderingHandle == 0)
            {
                return false;
            }

            NativeResult result = NativeApi.NRRenderingStop(m_RenderingHandle);
            NativeErrorListener.Check(result, this, "Stop", true);
            return result == NativeResult.Success;

        }

        public bool Destroy()
        {
            if (m_RenderingHandle == 0)
            {
                return false;
            }

            NativeResult result = NativeApi.NRRenderingDestroy(m_RenderingHandle);
            NativeErrorListener.Check(result, this, "Destroy", true);
            return result == NativeResult.Success;
        }

        private partial struct NativeApi
        {
            #region NRRender
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingCreate(ref UInt64 out_rendering_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingDestroy(UInt64 rendering_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingInitSetTextureColorSpace(UInt64 rendering_handle, NativeColorSpace color_space);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingStart(UInt64 rendering_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingStop(UInt64 rendering_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingPause(UInt64 rendering_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingResume(UInt64 rendering_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingGetFramePresentTime(UInt64 rendering_handle, ref UInt64 frame_present_time);
            #endregion
        };
    }
}