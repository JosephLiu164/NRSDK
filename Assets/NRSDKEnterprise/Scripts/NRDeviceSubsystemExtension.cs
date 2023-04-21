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
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary> Glasses hardware errors. </summary>
    public delegate void GlassesHardwareError(UInt32 timeOffset, UInt32 categoryId, UInt32 eventId,
                                                       UInt32 param1, UInt32 param2,
                                                       string description);

    /// <summary> Glasses hardware events. </summary>
    public delegate void GlassesHardwareEvent(UInt32 timeOffset, UInt32 categoryId, UInt32 eventId,
                                                       UInt32 param1, UInt32 param2,
                                                       string description);

    /// <summary> Callback, called when the nr glasses control temperature. </summary>
    /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
    /// <param name="temperature">            The temperature.</param>
    /// <param name="user_data">              Information describing the user.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NRGlassesControlTemperatureCallback(UInt64 glasses_control_handle, int temperature, UInt64 user_data);

    /// <summary>
    /// The callback method type which will be called when an light intensity data is ready.
    /// </summary>
    /// <param name="glasses_control_handle"></param>
    /// <param name="value"></param>
    /// <param name="user_data"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NRGlassesControlLightIntensityCallback(UInt64 glasses_control_handle, int value, IntPtr user_data);

    /// <summary>
    /// The callback method type which will be called when received a glasses hardware error. </summary>
    /// <param name="glasses_control_handle"> The handle of GlassesControl.</param>
    /// <param name="time_offset"> lowest 2bytes are effective with the byte order of the host device</param>
    /// <param name="event_id"> event_id highest 2bytes are events level flag, lowest 2bytes are events code.</param>
    /// <param name="param1"> lowest 2bytes are effective with the byte order of the host device.</param>
    /// <param name="param2"> lowest 2bytes are effective with the byte order of the host device.</param>
    /// <param name="description"> description
    /// <param name="description_length"> description_length
    /// <param name="user_data"> The custom user data.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void NRGlassesControlGlassesHardwareErrorCallback(UInt64 glasses_control_handle,
                                                       UInt32 category_id, UInt32 event_id,
                                                       UInt32 time_offset, UInt32 param1, UInt32 param2,
                                                       string description,
                                                       UInt32 description_length,
                                                       UInt64 user_data);

    /// <summary>
    /// The callback method type which will be called when received a glasses hardware error. </summary>
    /// <param name="glasses_control_handle"> The handle of GlassesControl.</param>
    /// <param name="time_offset"> lowest 2bytes are effective with the byte order of the host device.</param>
    /// <param name="event_id"> highest 2bytes are events level flag, lowest 2bytes are events code.</param>
    /// <param name="param1"> lowest 2bytes are effective with the byte order of the host device.</param>
    /// <param name="param2"> lowest 2bytes are effective with the byte order of the host device.</param>
    /// <param name="description"> description..</param>
    /// <param name="description_length"> description_length.</param>
    /// <param name="user_data"> The custom user data.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void NRGlassesControlGlassesHardwareEventCallback(UInt64 glasses_control_handle,
                                                       UInt32 category_id, UInt32 event_id,
                                                       UInt32 time_offset, UInt32 param1, UInt32 param2,
                                                       string description,
                                                       UInt32 description_length,
                                                       UInt64 user_data);


    public delegate void LightIntensityChangedEvent(int value);
    public static class NRDeviceSubsystemExtension
    {
        /// <summary>
        /// Event queue for all listeners interested in LightIntensityChangedCallback events. </summary>
        private static event LightIntensityChangedEvent OnLightIntensityChanged;

        /// <summary> The brightness minimum. </summary>
        [Obsolete("Use NRDeviceSubsystem.BRIGHTNESS_MIN instead", false)]
        public const int BRIGHTNESS_MIN = 0;
        /// <summary> The brightness maximum. </summary>
        [Obsolete("Use NRDeviceSubsystem.BRIGHTNESS_MAX instead", false)]
        public const int BRIGHTNESS_MAX = 7;

        #region Light intensity

        public static GlassesHardwareError OnGlassesHardwareError;
        public static GlassesHardwareEvent OnGlassesHardwareEvent;

        [MonoPInvokeCallback(typeof(NRGlassesControlLightIntensityCallback))]
        private static void LightIntensityChangedCallbackInternal(UInt64 glasses_control_handle, int value, IntPtr user_data)
        {
            OnLightIntensityChanged?.Invoke(value);
        }

        /// <summary>
        /// Adds an event listener to 'callback'. </summary>
        /// <param name="callback"> The LightIntensityChangedEvent callback.</param>
        public static void AddLightIntensityEventListener(this NRDeviceSubsystem subsystem, LightIntensityChangedEvent callback)
        {
            OnLightIntensityChanged += callback;
        }

        /// <summary> Removes the event listener. </summary>
        /// <param name="callback"> The LightIntensityChangedEvent callback.</param>
        public static void RemoveLightIntensityEventListener(this NRDeviceSubsystem subsystem, LightIntensityChangedEvent callback)
        {
            OnLightIntensityChanged -= callback;
        }

        /// <summary>
        /// A NRDeviceSubsystem extension method that regis glasses controller extra callbacks. </summary>
        public static void RegisGlassesIntensityCallback(this NRDeviceSubsystem subsystem)
        {
            if (!subsystem.IsAvailable)
            {
                NRDebugger.Warning("[NRDevice] Can not regist event when glasses disconnect...");
                return;
            }
#if !UNITY_EDITOR
            NativeApi.NRGlassesControlSetLightIntensityCallback(subsystem.NativeGlassesHandler, LightIntensityChangedCallbackInternal, IntPtr.Zero);
#endif
        }


        /// <summary>
        /// Sets light intensity state.
        /// </summary>
        /// <param name="state">1:open, 0:close</param>
        /// <returns>The result</returns>
        public static bool SetLightIntensityState(this NRDeviceSubsystem subsystem, int state)
        {
            if (!subsystem.IsAvailable)
            {
                return false;
            }
#if !UNITY_EDITOR
            var result = NativeApi.NRGlassesControlSetLightIntensityState(subsystem.NativeGlassesHandler, state);
            return result == NativeResult.Success;
#else
            return true;
#endif
        }

        /// <summary>
        /// Gets light intensity state.
        /// </summary>
        /// <returns>1:open, 0:close</returns>
        public static int GetLightIntensityState(this NRDeviceSubsystem subsystem)
        {
            if (!subsystem.IsAvailable)
            {
                return 0;
            }
#if !UNITY_EDITOR
            int state = 0;
            NativeApi.NRGlassesControlGetLightIntensityState(subsystem.NativeGlassesHandler, ref state);
            return state;
#else
            return 0;
#endif
        }

        #endregion

        #region Glasses Info
        private const int MaxMessageSize = 1024;
        /// <summary> The points. </summary>
        private static byte[] m_GlassesStatusInfo;
        private static GCHandle m_TmpHandle;

        /// <summary> A NativeGlassesController extension method that gets a duty. </summary>
        /// <param name="glassescontroller"> The glassescontroller to act on.</param>
        /// <returns> The duty. </returns>
        public static int GetDuty(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return -1;
            }

#if !UNITY_EDITOR
            int duty = -1;
            var result = NativeApi.NRGlassesControlGetDuty(device.NativeGlassesHandler, ref duty);
            return result == NativeResult.Success ? duty : -1;
#else
            return 0;
#endif
        }

        /// <summary> A NativeGlassesController extension method that sets a duty. </summary>
        /// <param name="glassescontroller"> The glassescontroller to act on.</param>
        /// <param name="duty">              The duty.</param>
        public static void SetDuty(this NRDeviceSubsystem device, int duty)
        {
            if (!device.IsAvailable)
            {
                return;
            }
#if !UNITY_EDITOR
            NativeApi.NRGlassesControlSetDuty(device.NativeGlassesHandler, duty);
#endif
        }

        /// <summary> A NativeGlassesController extension method that gets a temprature. </summary>
        /// <param name="glassescontroller"> The glassescontroller to act on.</param>
        /// <param name="temperatureType">   Type of the temperature.</param>
        /// <returns> The temprature. </returns>
        public static int GetTemprature(this NRDeviceSubsystem device, NativeGlassesTemperaturePosition temperatureType)
        {
            if (!device.IsAvailable)
            {
                return 0;
            }

#if !UNITY_EDITOR
            int temp = -1;
            var result = NativeApi.NRGlassesControlGetTemperatureData(device.NativeGlassesHandler, temperatureType, ref temp);
            return result == NativeResult.Success ? temp : -1;
#else
            return 0;
#endif
        }

        /// <summary>
        ///  A NRDeviceSubsystem extension method that get sleep time.
        /// </summary>
        /// <returns> Sleep time. </returns>
        public static int GetSleepTime(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return -1;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            int timeSec = -1;
            var result = NativeApi.NRGlassesControlGetSleepTime(device.NativeGlassesHandler, ref timeSec);
            return result == NativeResult.Success ? timeSec : -1;
#else
            return -1;
#endif
        }

        /// <summary>
        /// A NRDeviceSubsystem extension method that set sleep time.
        /// </summary>
        /// <param name="timeSec"> Sleep time. </param>
        public static void SetSleepTime(this NRDeviceSubsystem device, int timeSec)
        {
            if (!device.IsAvailable)
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            var result = NativeApi.NRGlassesControlSetSleepTime(device.NativeGlassesHandler, timeSec);
#endif
        }

        /// <summary>
        /// A NRDeviceSubsystem extension method that get power mode. If return 0, means your app will never be killed when you take off the glasses
        /// </summary>
        /// <returns> Power mode. </returns>
        public static int GetPowerMode(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return -1;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            int powerMode = -1;
            var result = NativeApi.NRGlassesControlGetPowerMode(device.NativeGlassesHandler, ref powerMode);
            return result == NativeResult.Success ? powerMode : -1;
#else
            return -1;
#endif
        }

        /// <summary>
        /// A NRDeviceSubsystem extension method that set power mode. If you set the param as 0, your app will never be killed when you take off the glasses
        /// </summary>
        /// <param name="mode"> Power mode </param>
        public static void SetPowerMode(this NRDeviceSubsystem device, int mode)
        {
            if (!device.IsAvailable)
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            var result = NativeApi.NRGlassesControlSetPowerMode(device.NativeGlassesHandler, mode);
#endif
        }

        /// <summary> A NativeGlassesController extension method that gets a version. </summary>
        /// <param name="glassescontroller"> The glassescontroller to act on.</param>
        /// <returns> The version. </returns>
        public static string GetVersion(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return "";
            }

#if !UNITY_EDITOR
            byte[] bytes = new byte[128];
            var result = NativeApi.NRGlassesControlGetVersion(device.NativeGlassesHandler, bytes, bytes.Length);
            return System.Text.Encoding.ASCII.GetString(bytes, 0, bytes.Length);
#else
            return "";
#endif
        }

        /// <summary>
        /// A NativeGlassesController extension method that gets glasses identifier. </summary>
        /// <param name="glassescontroller"> The glassescontroller to act on.</param>
        /// <returns> The glasses identifier. </returns>
        public static string GetGlassesID(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return "";
            }

#if !UNITY_EDITOR
            byte[] bytes = new byte[64];
            var result = NativeApi.NRGlassesControlGetGlassesID(device.NativeGlassesHandler, bytes, bytes.Length);
            return System.Text.Encoding.ASCII.GetString(bytes, 0, bytes.Length);
#else
            return "";
#endif
        }

        /// <summary>
        /// A NativeGlassesController extension method that gets glasses serial number. </summary>
        /// <param name="glassescontroller"> The glassescontroller to act on.</param>
        /// <returns> The glasses serial number. </returns>
        public static string GetGlassesSN(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return "";
            }

#if !UNITY_EDITOR
            byte[] bytes = new byte[64];
            var result = NativeApi.NRGlassesControlGetGlassesSN(device.NativeGlassesHandler, bytes, bytes.Length);
            return System.Text.Encoding.ASCII.GetString(bytes, 0, bytes.Length);
#else
            return "";
#endif
        }

        /// <summary> A NativeGlassesController extension method that gets a mode. </summary>
        /// <param name="glassescontroller"> The glassescontroller to act on.</param>
        /// <returns> The mode. </returns>
        public static NativeGlassesMode GetMode(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return NativeGlassesMode.ThreeD_1080;
            }

            NativeGlassesMode mode = NativeGlassesMode.TwoD_1080;
#if !UNITY_EDITOR
            var result = NativeApi.NRGlassesControlGet2D3DMode(device.NativeGlassesHandler, ref mode);
#endif
            return mode;
        }

        /// <summary> A NativeGlassesController extension method that sets a mode. </summary>
        /// <param name="glassescontroller"> The glassescontroller to act on.</param>
        /// <param name="mode">              The mode.</param>
        public static void SetMode(this NRDeviceSubsystem device, NativeGlassesMode mode)
        {
            if (!device.IsAvailable)
            {
                return;
            }

#if !UNITY_EDITOR
            NativeApi.NRGlassesControlSet2D3DMode(device.NativeGlassesHandler, mode);
#endif
        }

        public static NativeResult SetModeDirectly(this NRDeviceSubsystem device, NativeGlassesMode mode)
        {
            NativeResult result = NativeApi.NRGlassesControlSet2D3DMode(device.NativeGlassesHandler, mode);
            NRDebugger.Info("SetModeDirectly {0} result: {1}", mode.ToString(), result.ToString());
            return result;
        }

        public static string GetGlassesStatus(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return "";
            }

#if !UNITY_EDITOR
            if (m_GlassesStatusInfo == null)
            {
                m_GlassesStatusInfo = new byte[MaxMessageSize];
                m_TmpHandle = GCHandle.Alloc(m_GlassesStatusInfo, GCHandleType.Pinned);
            }
            var result = NativeApi.NRGlassesControlGet7211ICStatus(device.NativeGlassesHandler, 1, (m_TmpHandle.AddrOfPinnedObject()), MaxMessageSize);
            return System.Text.UTF8Encoding.UTF8.GetString(m_GlassesStatusInfo);
#else
            return "";
#endif
        }

        public static void SetLoggerTrigger(this NRDeviceSubsystem device)
        {
            if (!device.IsAvailable)
            {
                return;
            }

#if !UNITY_EDITOR
            NativeApi.NRGlassesControlSetLogTrigger(device.NativeGlassesHandler, 1);
#endif
        }
        #endregion

        public static bool IsRGBCameraEnable(this NRDeviceSubsystem device)
        {
#if !UNITY_EDITOR
            return device.NativeHMD.NRHMDIsChannelEnabled(NRChannelType.NR_SENSOR_TYPE_RGB_CAMERA);
#else
            return false;
#endif
        }

        public static bool UpdateIPD(this NRDeviceSubsystem device, float ipd)
        {
#if !UNITY_EDITOR
            return device.NativeHMD.NRHMDUpdateIPD(ipd);
#else
            return false;
#endif
        }

        public static void RegistErrorAndEventCallback(this NRDeviceSubsystem device, GlassesHardwareError glassesHardwareErrorCallback, GlassesHardwareEvent glassesHardwareEventCallback)
        {
#if !UNITY_EDITOR
            NativeResult result = NativeApi.NRGlassesControlSetGlassesHardwareErrorCallback(device.NativeGlassesHandler, OnGlassesHardwareErrorCB, 0);
            NativeErrorListener.Check(result, device, "SetGlassesHardwareErrorCallback");
            result = NativeApi.NRGlassesControlSetGlassesHardwareEventCallback(device.NativeGlassesHandler, OnGlassesHardwareEventCB, 0);
            NativeErrorListener.Check(result, device, "SetGlassesHardwareEventCallback");
#endif

            OnGlassesHardwareError = glassesHardwareErrorCallback;
            OnGlassesHardwareEvent = glassesHardwareEventCallback;
        }

        public static void StartErrorAndEventReport(this NRDeviceSubsystem device)
        {
#if !UNITY_EDITOR
            NativeResult result = NativeApi.NRGlassesControlStartErrorsAndEventsReport(device.NativeGlassesHandler);
            NativeErrorListener.Check(result, device, "StartErrorAndEventReport");
#endif
        }

        [MonoPInvokeCallback(typeof(NRGlassesControlGlassesHardwareEventCallback))]
        private static void OnGlassesHardwareErrorCB(UInt64 glasses_control_handle,
                                                       UInt32 category_id, UInt32 event_id,
                                                       UInt32 time_offset, UInt32 param1, UInt32 param2,
                                                       string description,
                                                       UInt32 description_length,
                                                       UInt64 user_data)
        {
            OnGlassesHardwareError?.Invoke(time_offset, category_id, event_id, param1, param2, description);
        }

        [MonoPInvokeCallback(typeof(NRGlassesControlGlassesHardwareErrorCallback))]
        private static void OnGlassesHardwareEventCB(UInt64 glasses_control_handle,
                                                       UInt32 category_id, UInt32 event_id,
                                                       UInt32 time_offset, UInt32 param1, UInt32 param2,
                                                       string description,
                                                       UInt32 description_length,
                                                       UInt64 user_data)
        {
            OnGlassesHardwareEvent?.Invoke(time_offset, category_id, event_id, param1, param2, description);
        }

        public static bool GetIMUAccelerometerBias(this NRDeviceSubsystem device, out Vector3 bias)
        {
            bias = new Vector3();
#if !UNITY_EDITOR
            return device.NativeHMD.GetIMUAccelerometerBias(ref bias);
#else
            return false;
#endif
        }

        public static bool GetIMUGyroscopeBias(this NRDeviceSubsystem device, out Vector3 bias)
        {
            bias = new Vector3();
#if !UNITY_EDITOR
            return device.NativeHMD.GetIMUGyroscopeBias(ref bias);
#else
            return false;
#endif
        }

        /// <summary> Set the display state of the glasses. </summary>
        /// <param name="state"> 0: Turn off screen, 1: Turn on screen. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public static bool SetDisplayState(this NRDeviceSubsystem device, int state)
        {
            NativeResult result = NativeApi.NRGlassesControlSetDisplayState(device.NativeGlassesHandler, state);
            return result == NativeResult.Success;
        }

        public static bool SetDisplayBypassPsensorFlag(this NRDeviceSubsystem device, int flag)
        {
            NativeResult result = NativeApi.NRGlassesControlSetDisplayBypassPsensorFlag(device.NativeGlassesHandler, flag);
            return result == NativeResult.Success;
        }

        public static bool GetDisplayBypassPsensorFlag(this NRDeviceSubsystem device, ref int flag)
        {
            NativeResult result = NativeApi.NRGlassesControlGetDisplayBypassPsensorFlag(device.NativeGlassesHandler, ref flag);
            return result == NativeResult.Success;
        }

        public static bool GetIfGlassesDisplayFine(this NRDeviceSubsystem device, ref int flag)
        {
            NativeResult result = NativeApi.NRGlassesControlGetIfGlassesDisplayFine(device.NativeGlassesHandler, ref flag);
            return result == NativeResult.Success;
        }

        private struct NativeApi
        {
            /// <summary>
            /// Sets the callback method .
            /// </summary>
            /// <param name="glasses_control_handle">glasses_control_handle The handle of light intensity object.</param>
            /// <param name="data_callback">data_callback The callback function.</param>
            /// <param name="user_data">user_data The data which will be returned when callback is triggered.</param>
            /// <returns>The result of operation.</returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetLightIntensityCallback(UInt64 glasses_control_handle,
                NRGlassesControlLightIntensityCallback data_callback, IntPtr user_data);

            /// <summary>
            /// Enable/Disable light intensity notify.
            /// </summary>
            /// <param name="glasses_control_handle">glasses_control_handle The handle of light intensity object.</param>
            /// <param name="state">state  0 for disable, 1 for enable.</param>
            /// <returns>The result of operation.</returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetLightIntensityState(UInt64 glasses_control_handle, int state);

            /// <summary>
            /// Stops light intensity notify.
            /// </summary>
            /// <param name="glasses_control_handle">glasses_control_handle The handle of light intensity object.</param>
            /// <param name="out_state">out_state The state of light intensity function.</param>
            /// <returns>The result of operation.</returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetLightIntensityState(UInt64 glasses_control_handle, ref int out_state);

            /// <summary> Nr glasses control get duty. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="out_dute">               [in,out] The out dute.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetDuty(UInt64 glasses_control_handle, ref int out_dute);

            /// <summary> Nr glasses control set duty. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="dute">                   The dute.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetDuty(UInt64 glasses_control_handle, int dute);

            /// <summary> Nr glasses control get temperature data. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="position">               The position.</param>
            /// <param name="out_temperature">        [in,out] The out temperature.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetTemperatureData(UInt64 glasses_control_handle, NativeGlassesTemperaturePosition position, ref int out_temperature);

            /// <summary>
            /// Nr glasses control get sleep time. 
            /// </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control. </param>
            /// <param name="out_time_sec"> Sleep time.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetSleepTime(UInt64 glasses_control_handle, ref int out_time_sec);

            /// <summary>
            /// Nr glasses control set sleep time.
            /// </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control. </param>
            /// <param name="time_sec"> Sleep time. </param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetSleepTime(UInt64 glasses_control_handle, int time_sec);

            /// <summary>
            /// Nr glasses control get power mode.
            /// </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control. </param>
            /// <param name="out_mode"> Power mode </param>
            /// <returns>  A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetPowerMode(UInt64 glasses_control_handle, ref int out_mode);

            /// <summary>
            /// Nr glasses control set power mode.
            /// </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control. </param>
            /// <param name="mode"> Power mode </param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetPowerMode(UInt64 glasses_control_handle, int mode);

            /// <summary> Nr glasses control get version. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="out_version">            The out version.</param>
            /// <param name="len">                    The length.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetVersion(UInt64 glasses_control_handle, byte[] out_version, int len);

            /// <summary> Nr glasses control get glasses identifier. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="out_glasses_id">         Identifier for the out glasses.</param>
            /// <param name="len">                    The length.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetGlassesID(UInt64 glasses_control_handle,
               byte[] out_glasses_id, int len);

            /// <summary> Nr glasses control get glasses serial number. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="out_glasses_sn">         The out glasses serial number.</param>
            /// <param name="len">                    The length.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetGlassesSN(UInt64 glasses_control_handle,
                byte[] out_glasses_sn, int len);

            /// <summary> Nr glasses control get 3D mode. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="out_mode">               [in,out] The out mode.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGet2D3DMode(UInt64 glasses_control_handle, ref NativeGlassesMode out_mode);

            /// <summary> Nr glasses control set 3D mode. </summary>
            /// <param name="glasses_control_handle"> Handle of the glasses control.</param>
            /// <param name="mode">                   The mode.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSet2D3DMode(UInt64 glasses_control_handle, NativeGlassesMode mode);

            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlGet7211ICStatus(UInt64 glasses_control_handle, int flag, IntPtr out_status, int len);

            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGlassesControlSetLogTrigger(UInt64 glasses_control_handle, int state);

            /// <summary> Tell glasses' MCU to start to report Errors and Logs. </summary>
            /// <param name="glasses_control_handle"> The handle of GlassesControl.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlStartErrorsAndEventsReport(UInt64 glasses_control_handle);

            /// <summary> Set the callback method when glasses report a hardware errors. </summary>
            /// <param name="glasses_control_handle"> The handle of GlassesControl.</param>
            /// <param name="data_callback"> The callback method.</param>
            /// <param name="user_data"> The data which will be returned when the callback is triggered.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetGlassesHardwareErrorCallback(
                UInt64 glasses_control_handle, NRGlassesControlGlassesHardwareErrorCallback data_callback, UInt64 user_data);

            /// <summary> Set the callback method when glasses report a hardware event. </summary>
            /// <param name="glasses_control_handle"> The handle of GlassesControl.</param>
            /// <param name="data_callback"> The callback method.</param>
            /// <param name="user_data"> The data which will be returned when the callback is triggered.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetGlassesHardwareEventCallback(
                UInt64 glasses_control_handle, NRGlassesControlGlassesHardwareEventCallback data_callback, UInt64 user_data);

            /// <summary> Set the display state of the glasses. </summary>
            /// <param name="glasses_control_handle"> The handle of GlassesControl.</param>
            /// <param name="state"> 0: Turn off screen, 1: Turn on screen.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetDisplayState(UInt64 glasses_control_handle, int state);

            /// <summary> Set the display flag for bypassing psensor. </summary>
            /// <param name="glasses_control_handle"> The handle of GlassesControl. </param>
            /// <param name="flag"> 0: not bypassed 1: bypass </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlSetDisplayBypassPsensorFlag(UInt64 glasses_control_handle, int flag);

            /// <summary> Get the display flag for bypassing psensor. </summary>
            /// <param name="glasses_control_handle"> The handle of GlassesControl. </param>
            /// <param name="out_flag"> 0: not bypassed 1: bypass </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetDisplayBypassPsensorFlag(UInt64 glasses_control_handle, ref int out_flag);

            /// <summary> Get whether the glasses display is fine </summary>
            /// <param name="glasses_control_handle"> The handle of GlassesControl. </param>
            /// <param name="out_status"> 0: fine 1:not fine </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGlassesControlGetIfGlassesDisplayFine(UInt64 glasses_control_handle, ref int out_status);
        }
    }
}