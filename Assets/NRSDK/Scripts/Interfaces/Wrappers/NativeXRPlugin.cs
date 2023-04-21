/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

#if USING_XR_MANAGEMENT && USING_XR_SDK_NREAL
#define USING_XR_SDK
#endif

namespace NRKernal
{
    using System;
    using System.Runtime.InteropServices;
    using AOT;

    internal delegate void OnGfxThreadStartCallback(UInt64 renderingHandle);
    internal delegate void OnGfxThreadSubmitCallback(UInt64 frameHandle, IntPtr idTexture);
    internal delegate void OnGfxThreadPopulateFrameCallback();
    internal delegate void OnDisplaySubSystemStartCallback(bool start);

    /// <summary> A controller for handling native glasses. </summary>
    internal partial class NativeXRPlugin
    {
#if USING_XR_SDK
        internal static void RegistEventCallback(OnGfxThreadStartCallback onStartCallback, OnGfxThreadPopulateFrameCallback onPopulateFrameCallback, OnGfxThreadSubmitCallback onSubmitCallback)
        {
            NativeApi.RegistEventCallback(onStartCallback, onPopulateFrameCallback, onSubmitCallback);
        }

        internal static void RegistDisplaySubSystemEventCallback(OnDisplaySubSystemStartCallback onStartCallback)
        {
            NativeApi.RegistDisplaySubSystemEventCallback(onStartCallback);
        }

        internal static void SetLogLevel(int logLevel)
        {
            NativeApi.SetLogLevel(logLevel);
        }

        internal static void PopulateFrameHandle(UInt64 frameHandle)
        {
            NativeApi.PopulateFrameHandle(frameHandle);
        }

        internal static IntPtr[] CreateDisplayTextures(int texNum, int texWidth, int texHeight, int texArrayLength)
        {
            if (!NativeApi.CreateDisplayTextures((UInt32)texNum, (UInt32)texWidth, (UInt32)texHeight, (UInt32)texArrayLength))
            {
                NRDebugger.Error("CreateDisplayTextures failed");
                return null;
            }
            IntPtr[] rst = new IntPtr[texNum];
            for (UInt32 i = 0; i < texNum; i++)
            {
                rst[i] = NativeApi.AcquireDisplayTexture(i);
            }
            return rst;
        }

        internal static bool PopulateDisplayTexture(IntPtr idTexture)
        {
            return NativeApi.PopulateDisplayTexture((IntPtr)idTexture);
        }

        internal static LostTrackingReason GetLostTrackingReason()
        {
            return NativeApi.GetLostTrackingReason();
        }

        internal static UInt64 GetFramePresentTime()
        {
            return NativeApi.GetFramePresentTime();
        }

        internal static UInt64 GetTrackingHandle()
        {
            return NativeApi.GetTrackingHandle();
        }

        internal static UInt64 GetHeadTrackingHandle()
        {
            return NativeApi.GetHeadTrackingHandle();
        }

        internal static UInt64 GetHMDHandle()
        {
            return NativeApi.GetHMDHandle();
        }

        internal static UInt64 GetMetricsHandle()
        {
            return NativeApi.GetMetricsHandle();
        }

        internal static UInt64 GetDisplayHandle()
        {
            return NativeApi.GetDisplayHandle();
        }

        internal static void SetTargetFrameRate(int targetFrameRate)
        {
            NativeApi.SetTargetFrameRate(targetFrameRate);
        }
        
        internal static int GetTargetFrameRate()
        {
            return NativeApi.GetTargetFrameRate();
        }
        

        private partial struct NativeApi
        {
            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static void RegistEventCallback(OnGfxThreadStartCallback onStartCallback, OnGfxThreadPopulateFrameCallback onPopulateFrameCallback, OnGfxThreadSubmitCallback onSubmitCallback);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static void RegistDisplaySubSystemEventCallback(OnDisplaySubSystemStartCallback onStartCallback);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern void SetLogLevel(int logLevel);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static void PopulateFrameHandle(UInt64 frameHandle);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static bool CreateDisplayTextures(UInt32 texNum, UInt32 texWidth, UInt32 texHeight, UInt32 texArrayLength);
            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static IntPtr AcquireDisplayTexture(UInt32 texIdx);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern bool PopulateDisplayTexture(IntPtr idTexture);



            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern LostTrackingReason GetLostTrackingReason();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetFramePresentTime();


            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetTrackingHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetHeadTrackingHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetHMDHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetMetricsHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetDisplayHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern void SetTargetFrameRate(int targetFrameRate);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern int GetTargetFrameRate();
        }
#endif
    }
}
