/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using NRKernal.Enterprise;
using System;
using System.Collections.Generic;

namespace NRKernal.Enterprise.NRExamples
{
    public class GlassesErrorEventExample : MonoBehaviour
    {
        void Start()
        {
            NRDevice.Subsystem.RegistErrorAndEventCallback(GlassesHardwareError, GlassesHardwareEvent);
            NRDevice.Subsystem.StartErrorAndEventReport();
        }
        
        public void GlassesHardwareError(UInt32 timeOffset, UInt32 categoryId, UInt32 eventId, UInt32 param1, UInt32 param2, string description)
        {
            Debug.LogErrorFormat("GlassesHardwareError: timeOffset={0}, categoryId={1}, eventId={2}, param1={3}, param2={4}, desc={5}", timeOffset, categoryId, eventId, param1, param2, description);
        }

        public void GlassesHardwareEvent(UInt32 timeOffset, UInt32 categoryId, UInt32 eventId, UInt32 param1, UInt32 param2, string description)
        {
            Debug.LogErrorFormat("GlassesHardwareEvent: timeOffset={0}, categoryId={1}, eventId={2}, param1={3}, param2={4}, desc={5}", timeOffset, categoryId, eventId, param1, param2, description);
        }
    }
}