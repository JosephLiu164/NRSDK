/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NRKernal.Enterprise.NRExamples
{
    public class PupilDistanceAdjustment : MonoBehaviour
    {
        [SerializeField]
        private Text m_Value;
        [SerializeField]
        private Slider m_DistanceAdjSlider;
        const int k_DefaultIPD = 64;

        IEnumerator Start()
        {
            while (NRFrame.SessionStatus != SessionState.Running)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(0.2f);

            m_DistanceAdjSlider.maxValue = 80;
            m_DistanceAdjSlider.minValue = 50;
            m_DistanceAdjSlider.wholeNumbers = true;
            m_DistanceAdjSlider.onValueChanged.AddListener(OnValueChange);
            m_DistanceAdjSlider.value = k_DefaultIPD;
        }

        void OnValueChange(float val)
        {
            NRDevice.Subsystem.UpdateIPD(val * 0.001f);
            m_Value.text = string.Format("{0}mm", (int)val);
        }
    }
}
