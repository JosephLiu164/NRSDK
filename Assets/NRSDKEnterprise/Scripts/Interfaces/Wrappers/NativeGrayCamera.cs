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

    /// <summary> Session Native API. </summary>
    public class NativeGrayCamera : ICameraDataProvider
    {
        /// <summary> Handle of the native camera. </summary>
        private UInt64 m_NativeCameraHandle;

        /// <summary> Creates a new bool. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Create()
        {
            var result = NativeApi.NRGrayscaleCameraCreate(ref m_NativeCameraHandle);
            NativeErrorListener.Check(result, this, "Create");
            return result == NativeResult.Success;
        }

        /// <summary> Gets raw data. </summary>
        /// <param name="imageHandle"> Handle of the image.</param>
        /// <param name="camera">      The camera.</param>
        /// <param name="ptr">         [in,out] The pointer.</param>
        /// <param name="size">        [in,out] The size.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool GetRawData(UInt64 imageHandle, NativeDevice camera, ref IntPtr ptr, ref int size)
        {
            uint data_width = 0;
            uint data_height = 0;
            var result = NativeApi.NRGrayscaleCameraImageGetData(m_NativeCameraHandle, imageHandle, camera, ref ptr, ref data_width, ref data_height);
            size = (int)(data_width * data_height);
            return result == NativeResult.Success;
        }

        /// <summary> Gets a resolution. </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///                                                the required range.</exception>
        /// <param name="imageHandle"> Handle of the image.</param>
        /// <param name="camera">      The camera.</param>
        /// <returns> The resolution. </returns>
        public NativeResolution GetResolution(UInt64 imageHandle, NativeDevice camera)
        {
            if (!Enum.IsDefined(typeof(NativeGrayEye), camera))
            {
                throw new ArgumentOutOfRangeException("GetResolution of camera:" + camera + ", which should be type of NativeGrayEye");
            }
            return NRFrameExtension.GetGrayCameraResolution((NativeGrayEye)camera);
        }

        /// <summary> Gets hmd time nanos. </summary>
        /// <param name="imageHandle"> Handle of the image.</param>
        /// <param name="camera">      The camera.</param>
        /// <returns> The hmd time nanos. </returns>
        public UInt64 GetHMDTimeNanos(UInt64 imageHandle, NativeDevice camera)
        {
            UInt64 time = 0;
            NativeApi.NRGrayscaleCameraImageGetTime(m_NativeCameraHandle, imageHandle, camera, ref time);
            return time;
        }

        /// <summary> Get exposure time. </summary>
        /// <param name="imageHandle"> Handle of the image. </param>
        /// <param name="camera">      The camera. </param>
        /// <returns> Exposure time of the image. </returns>
        public UInt32 GetExposureTime(UInt64 imageHandle, NativeDevice camera)
        {
            UInt32 exposureTime = 0;
            NativeApi.NRGrayscaleCameraImageGetExposureTime(m_NativeCameraHandle, imageHandle, camera, ref exposureTime);
            return exposureTime;
        }

        /// <summary> Get Gain. </summary>
        /// <param name="imageHandle"> Handle of the image. </param>
        /// <param name="camera">      The camera. </param>
        /// <returns> Gain of the image. </returns>
        public UInt32 GetGain(UInt64 imageHandle, NativeDevice camera)
        {
            UInt32 gain = 0;
            NativeApi.NRGrayscaleCameraImageGetGain(m_NativeCameraHandle, imageHandle, camera, ref gain);
            return gain;
        }

        /// <summary> Callback, called when the set capture. </summary>
        /// <param name="callback"> The callback.</param>
        /// <param name="userdata"> (Optional) The userdata.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SetCaptureCallback(CameraImageCallback callback, UInt64 userdata = 0)
        {
            var result = NativeApi.NRGrayscaleCameraSetCaptureCallback(m_NativeCameraHandle, callback, userdata);
            NativeErrorListener.Check(result, this, "SetCaptureCallback");
            return result == NativeResult.Success;
        }

        /// <summary> Sets image format. </summary>
        /// <param name="format"> Describes the format to use.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SetImageFormat(CameraImageFormat format)
        {
            return true;
        }

        /// <summary> Starts a capture. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool StartCapture()
        {
            var result = NativeApi.NRGrayscaleCameraStartCapture(m_NativeCameraHandle);
            NativeErrorListener.Check(result, this, "StartCapture");
            return result == NativeResult.Success;
        }

        /// <summary> Stops a capture. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool StopCapture()
        {
            var result = NativeApi.NRGrayscaleCameraStopCapture(m_NativeCameraHandle);
            NativeErrorListener.Check(result, this, "StopCapture");
            return result == NativeResult.Success;
        }

        /// <summary> Destroys the image described by imageHandle. </summary>
        /// <param name="imageHandle"> Handle of the image.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool DestroyImage(UInt64 imageHandle)
        {
            var result = NativeApi.NRGrayscaleCameraImageDestroy(m_NativeCameraHandle, imageHandle);
            NativeErrorListener.Check(result, this, "DestroyImage");
            return result == NativeResult.Success;
        }

        /// <summary> Releases this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Release()
        {
            var result = NativeApi.NRGrayscaleCameraDestroy(m_NativeCameraHandle);
            NativeErrorListener.Check(result, this, "Release");
            return result == NativeResult.Success;
        }

        /// <summary> A native api. </summary>
        private struct NativeApi
        {
            /// <summary> Nr grayscale camera image get data. </summary>
            /// <param name="grayscale_camera_handle">       Handle of the grayscale camera.</param>
            /// <param name="grayscale_camera_image_handle"> Handle of the grayscale camera image.</param>
            /// <param name="camera">                        The camera.</param>
            /// <param name="out_grey_image">                [in,out] The out grey image.</param>
            /// <param name="out_width">                     [in,out] Width of the out.</param>
            /// <param name="out_height">                    [in,out] Height of the out.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraImageGetData(UInt64 grayscale_camera_handle,
                UInt64 grayscale_camera_image_handle, NativeDevice camera, ref IntPtr out_grey_image,
                ref UInt32 out_width, ref UInt32 out_height);

            /// <summary> Nr grayscale camera image get time. </summary>
            /// <param name="grayscale_camera_handle">       Handle of the grayscale camera.</param>
            /// <param name="grayscale_camera_image_handle"> Handle of the grayscale camera image.</param>
            /// <param name="camera">                        The camera.</param>
            /// <param name="out_nano_time">                 [in,out] The out nano time.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraImageGetTime(UInt64 grayscale_camera_handle,
                UInt64 grayscale_camera_image_handle, NativeDevice camera, ref UInt64 out_nano_time);

            /// <summary> Nr grayscale camera image get gain. </summary>
            /// <param name="grayscale_camera_handle">       Handle of the grayscale camera. </param>
            /// <param name="grayscale_camera_image_handle"> Handle of the grayscale camera image. </param>
            /// <param name="eycamerae">                     The camera. </param>
            /// <param name="out_gain">                      [in,out] The gain of the image. </param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraImageGetGain(UInt64 grayscale_camera_handle,
                UInt64 grayscale_camera_image_handle, NativeDevice camera, ref UInt32 out_gain);

            /// <summary> Nr grayscale camera image get exposure time. </summary>
            /// <param name="grayscale_camera_handle">       Handle of the grayscale camera. </param>
            /// <param name="grayscale_camera_image_handle"> Handle of the grayscale camera image. </param>
            /// <param name="camera">                        The camera. </param>
            /// <param name="out_exposure_time">             [in,out] The exposure time of the image. </param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraImageGetExposureTime(UInt64 grayscale_camera_handle,
                UInt64 grayscale_camera_image_handle, NativeDevice camera, ref UInt32 out_exposure_time);

            /// <summary> Nr grayscale camera create. </summary>
            /// <param name="out_grayscale_camera_handle"> [in,out] Handle of the out grayscale camera.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraCreate(ref UInt64 out_grayscale_camera_handle);

            /// <summary> Nr grayscale camera destroy. </summary>
            /// <param name="grayscale_camera_handle"> Handle of the grayscale camera.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraDestroy(UInt64 grayscale_camera_handle);

            /// <summary> Callback, called when the nr grayscale camera set capture. </summary>
            /// <param name="grayscale_camera_handle"> Handle of the grayscale camera.</param>
            /// <param name="image_callback">          The image callback.</param>
            /// <param name="userdata">                The userdata.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary, CallingConvention = CallingConvention.Cdecl)]
            public static extern NativeResult NRGrayscaleCameraSetCaptureCallback(
                UInt64 grayscale_camera_handle, CameraImageCallback image_callback, UInt64 userdata);

            /// <summary> Nr grayscale camera start capture. </summary>
            /// <param name="grayscale_camera_handle"> Handle of the grayscale camera.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraStartCapture(UInt64 grayscale_camera_handle);

            /// <summary> Nr grayscale camera stop capture. </summary>
            /// <param name="grayscale_camera_handle"> Handle of the grayscale camera.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraStopCapture(UInt64 grayscale_camera_handle);

            /// <summary> Nr grayscale camera image destroy. </summary>
            /// <param name="grayscale_camera_handle">       Handle of the grayscale camera.</param>
            /// <param name="grayscale_camera_image_handle"> Handle of the grayscale camera image.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRGrayscaleCameraImageDestroy(UInt64 grayscale_camera_handle,
                UInt64 grayscale_camera_image_handle);
        };
    }
}
