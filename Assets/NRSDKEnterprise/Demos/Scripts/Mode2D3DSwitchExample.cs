/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using static NRKernal.NRDevice;

namespace NRKernal.Enterprise.NRExamples
{
    /// <summary>
    /// A example to change display mode 2d or 3d.
    /// </summary>
    public class Mode2D3DSwitchExample : MonoBehaviour
    {
        void OnEnable()
        {
            NRDevice.Subsystem.OnSessionEvent += OnEvent;
        }

        void OnDisable()
        {
            NRDevice.Subsystem.OnSessionEvent -= OnEvent;
        }

        private void OnEvent(SessionEventType sessionEvent)
        {
            NRDebugger.Info("[Mode2D3DSwitchExample] OnEvent:" + sessionEvent.ToString());
            if(sessionEvent == SessionEventType.Pause)
            {
                NRDevice.Subsystem.SetModeDirectly(NativeGlassesMode.TwoD_1080);
            }
            else if(sessionEvent == SessionEventType.Resume)
            {
                NRDevice.Subsystem.SetModeDirectly((NativeGlassesMode)4);
            }
        }
    }
}
