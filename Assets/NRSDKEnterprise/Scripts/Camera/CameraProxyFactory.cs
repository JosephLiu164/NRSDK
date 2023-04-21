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
    /// <summary> A camera proxy factory extension. </summary>
    public class CameraProxyFactoryExtension
    {
        /// <summary> Creates gray camera proxy. </summary>
        /// <param name="eye"> The eye.</param>
        /// <returns> The new gray camera proxy. </returns>
        public static NativeCameraProxy CreateGrayCameraProxy(NativeGrayEye eye)
        {
            NativeCameraProxy controller = CameraProxyFactory.GetInstance(NRGrayCamera.ID);
            if (controller == null)
            {
                controller = new NRGrayCamera();
                CameraProxyFactory.RegistCameraProxy(NRGrayCamera.ID, controller);
            }

            return controller;
        }
    }
}
