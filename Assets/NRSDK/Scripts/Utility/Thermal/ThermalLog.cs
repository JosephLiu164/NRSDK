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
    using UnityEngine;

    public class ThermalLog : AndroidJavaProxy
    {
        public ThermalLog() : base("com.nreal.nrealapp.IThermalLog")
        {
            NRDebugger.Info("[ThermalLog]: new ThermalLog");
        }

        void OnThermalLog(int status)
        {
            ThermalMgr.CurStatus = status;
            NRDebugger.Info("[ThermalLog]: status={0}", status);
        }
    }
}