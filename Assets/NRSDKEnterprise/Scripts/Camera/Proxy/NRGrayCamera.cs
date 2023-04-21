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

    /// <summary> A nr gray camera. </summary>
    internal class NRGrayCamera : NativeCameraProxy
    {
        /// <summary> The identifier. </summary>
        public new static string ID = "NRGrayCamera";
        /// <summary> The texture pointer extra. </summary>
        private IntPtr m_TexturePtrExtra = IntPtr.Zero;

        /// <summary> Gets the resolution. </summary>
        /// <value> The resolution. </value>
        public override NativeResolution Resolution
        {
            get
            {
#if !UNITY_EDITOR
                NativeResolution resolution = NRFrame.GetDeviceResolution(NativeDevice.LEFT_GRAYSCALE_CAMERA);
#else   
                NativeResolution resolution = new NativeResolution(1280, 720);
#endif
                return resolution;
            }
        }

#if !UNITY_EDITOR
        public NRGrayCamera() : base(new NativeGrayCamera()) { }
#else
        /// <summary> Default constructor. </summary>
        public NRGrayCamera() : base(new EditorCameraDataProvider()) { }
#endif

        /// <summary> Initializes this object. </summary>
        public override void Initialize()
        {
            base.Initialize();
            RegistCaptureCallback(GrayCameraCapture);
        }

        /// <summary> Gray camera capture. </summary>
        /// <param name="camera_handle">       Handle of the camera.</param>
        /// <param name="camera_image_handle"> Handle of the camera image.</param>
        /// <param name="userdata">            The userdata.</param>
        [MonoPInvokeCallback(typeof(CameraImageCallback))]
        public static void GrayCameraCapture(UInt64 camera_handle, UInt64 camera_image_handle, UInt64 userdata)
        {
            var controller = CameraProxyFactory.GetInstance(NRGrayCamera.ID);
            if (controller == null)
            {
                NRDebugger.Error("[NRGrayCamera] get controller instance faild.");
                return;
            }
            controller.UpdateFrame(camera_handle, camera_image_handle, userdata);
        }

        /// <summary> Updates the frame. </summary>
        /// <exception cref="FormatException"> Thrown when the format of the ? is incorrect.</exception>
        /// <param name="camera_handle">       Handle of the camera.</param>
        /// <param name="camera_image_handle"> Handle of the camera image.</param>
        /// <param name="userdata">            The userdata.</param>
        public override void UpdateFrame(UInt64 camera_handle, UInt64 camera_image_handle, UInt64 userdata)
        {
            int leftcamera_rawdata_size = 0;
            int rightcamera_rawdata_size = 0;

            this.CameraDataProvider.GetRawData(camera_image_handle, NativeDevice.LEFT_GRAYSCALE_CAMERA, ref this.m_TexturePtr, ref leftcamera_rawdata_size);
            this.CameraDataProvider.GetRawData(camera_image_handle, NativeDevice.RIGHT_GRAYSCALE_CAMERA, ref this.m_TexturePtrExtra, ref rightcamera_rawdata_size);

            if (leftcamera_rawdata_size != rightcamera_rawdata_size)
            {
                throw new FormatException("[NRGrayCamera] left、right Camera raw data size not match.");
            }
            var timestamp = this.CameraDataProvider.GetHMDTimeNanos(camera_image_handle, NativeDevice.LEFT_GRAYSCALE_CAMERA);
            var gain = this.CameraDataProvider.GetGain(camera_image_handle, NativeDevice.LEFT_GRAYSCALE_CAMERA);
            var exposureTime = this.CameraDataProvider.GetExposureTime(camera_image_handle, NativeDevice.LEFT_GRAYSCALE_CAMERA);
            this.QueueFrameForGrayCamera(this.m_TexturePtr, this.m_TexturePtrExtra, leftcamera_rawdata_size * 2, timestamp, gain, exposureTime);

            this.CameraDataProvider.DestroyImage(camera_image_handle);
        }

        /// <summary> Queue frame for gray camera. </summary>
        /// <param name="textureptr1"> The first textureptr.</param>
        /// <param name="textureptr2"> The second textureptr.</param>
        /// <param name="size">        The size.</param>
        /// <param name="timestamp">   The timestamp.</param>
        /// <param name="gain">        The gain.</param>
        /// <param name="exposureTime">The exposure time.</param>
        private void QueueFrameForGrayCamera(IntPtr textureptr1, IntPtr textureptr2, int size, UInt64 timestamp, UInt32 gain, UInt32 exposureTime)
        {
            if (!m_IsPlaying)
            {
                NRDebugger.Error("camera was not stopped properly, it still sending data.");
                return;
            }

            FrameRawData frame = FramePool.Get<FrameRawData>();
            bool result = FrameRawDataExtension.MakeSafe(textureptr1, textureptr2, size, timestamp, gain, exposureTime, ref frame);
            if (result)
            {
                m_CameraFrames.Enqueue(frame);
                if (m_ExternFrameConsumer != null)
                {
                    m_ExternFrameConsumer.UpdateFrame(frame);
                }
            }
            else
            {
                FramePool.Put<FrameRawData>(frame);
            }
        }
    }
}
