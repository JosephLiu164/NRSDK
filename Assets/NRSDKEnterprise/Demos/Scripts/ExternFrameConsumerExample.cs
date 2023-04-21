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
    public class ExternFrameConsumerExample : NativeCameraProxy.IExternFrameConsumer
    {
        public void UpdateFrame(FrameRawData frame)
        {
            NRDebugger.Info("ExternFrameConsumerDemo UpdateFrame: " + frame.timeStamp);
        }
    }
}
