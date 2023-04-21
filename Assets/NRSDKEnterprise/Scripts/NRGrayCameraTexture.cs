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
    using UnityEngine;

    /// <summary> Create a gray camera texture. </summary>
    public class NRGrayCameraTexture : CameraModelView
    {
        /// <summary> The on update. </summary>
        public Action<CameraTextureFrame> OnUpdate;
        /// <summary> The current frame. </summary>
        public CameraTextureFrame CurrentFrame;
        /// <summary> The texture. </summary>
        private Texture2D m_Texture;
        /// <summary> The eye. </summary>
        private NativeGrayEye m_Eye;
        /// <summary> Information describing the raw. </summary>
        private byte[] m_RawData;
        public byte[] RawData
        {
            get { return m_RawData; }
        }

        /// <summary> Constructor. </summary>
        /// <param name="eye"> The eye.</param>
        public NRGrayCameraTexture(NativeGrayEye eye)
        {
            // Must Create GrayCamera Proxy at here.
            this.CreateGrayCameraProxy(eye);

            this.m_Eye = eye;
            this.m_Texture = CreateTexture();
            this.CurrentFrame.texture = m_Texture;
        }

        /// <summary> Gets the texture. </summary>
        /// <returns> The texture. </returns>
        public Texture2D GetTexture()
        {
            if (m_Texture == null)
            {
                m_Texture = CreateTexture();
            }
            return m_Texture;
        }

        /// <summary> Creates the texture. </summary>
        /// <returns> The new texture. </returns>
        private Texture2D CreateTexture()
        {
            return new Texture2D(Width, Height, TextureFormat.R8, false);
        }

        /// <summary> Load raw texture data. </summary>
        /// <param name="frame"> .</param>
        protected override void OnRawDataUpdate(FrameRawData frame)
        {
            base.OnRawDataUpdate(frame);

            int halfsize = frame.data.Length / 2;
            int index = (int)m_Eye == (int)NativeDevice.LEFT_GRAYSCALE_CAMERA ? 0 : halfsize;
            if (m_RawData == null || m_RawData.Length != halfsize)
            {
                m_RawData = new byte[halfsize];
            }
            Array.Copy(frame.data, index, m_RawData, 0, halfsize);

            if (m_Texture == null)
            {
                GetTexture();
            }
            m_Texture.LoadRawTextureData(m_RawData);
            m_Texture.Apply();

            CurrentFrame.timeStamp = frame.timeStamp;
            CurrentFrame.gain = frame.gain;
            CurrentFrame.exposureTime = frame.exposureTime;
            CurrentFrame.texture = m_Texture;

            OnUpdate?.Invoke(CurrentFrame);
        }

        /// <summary> On texture stopped. </summary>
        protected override void OnStopped()
        {
            GameObject.Destroy(m_Texture);
            m_Texture = null;
            m_RawData = null;
        }
    }
}
