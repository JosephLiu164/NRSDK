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
    /// <summary> A camera model view extension. </summary>
    public static class CameraModelViewExtension
    {
        /// <summary> A CameraModelView extension method that creates gray camera proxy. </summary>
        /// <param name="modelview"> The modelview to act on.</param>
        /// <param name="eye">       The eye.</param>
        public static void CreateGrayCameraProxy(this CameraModelView modelview, NativeGrayEye eye)
        {
            if (modelview.NativeCameraProxy != null)
            {
                return;
            }

            modelview.NativeCameraProxy = CameraProxyFactoryExtension.CreateGrayCameraProxy(eye);
            modelview.NativeCameraProxy.Regist(modelview);
        }
    }
}
