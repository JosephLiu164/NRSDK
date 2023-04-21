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

namespace NRKernal.Enterprise.NRExamples
{
    /// <summary>
    /// A example to use glasses intensity interface.
    /// </summary>
    public class GlassesLightIntensityExample : MonoBehaviour
    {
        public Text m_IntensityLable;

        void OnEnable()
        {
            NRDevice.Subsystem.RegisGlassesIntensityCallback();
            NRDevice.Subsystem.AddLightIntensityEventListener(OnGlassesIntensityChanged);

            // Open intensity listen state.
            NRDevice.Subsystem.SetLightIntensityState(1);
        }

        void OnDisable()
        {
            NRDevice.Subsystem.RemoveLightIntensityEventListener(OnGlassesIntensityChanged);
        }

        private void OnGlassesIntensityChanged(int value)
        {
            // Don't do things that can only be done on the main thread.
            Debug.LogError("OnGlassesIntensityChanged:" + value);
            MainThreadDispather.QueueOnMainThread(() => m_IntensityLable.text = value.ToString());
        }
    }
}
