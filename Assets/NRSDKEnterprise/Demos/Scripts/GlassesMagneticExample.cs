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
    public class GlassesMagneticExample : MonoBehaviour
    {
        public Text m_Lable;
        private NRGlassesMagneticProvider m_NRGlassesMagneticProvider;
        Pose m_MagneticPose;

        void Start()
        {
            m_NRGlassesMagneticProvider = new NRGlassesMagneticProvider();
            m_NRGlassesMagneticProvider.Start();

            StartCoroutine(DevicePosTrasformSample());
        }

        void Update()
        {
            var frame = m_NRGlassesMagneticProvider.GetCurrentFrame();
            m_Lable.text = string.Format("magnetic:{0} timestamp:{1}", frame.magnetic, frame.timestamp);
        }

        IEnumerator DevicePosTrasformSample()
        {
            while (NRFrame.SessionStatus != SessionState.Running)
                yield return null;
            
            m_MagneticPose = NRFrameExtension.MagneticPoseFromHead;
            NRDebugger.Info("[magnetic] Pose : {0}", m_MagneticPose.ToString("F8"));
        }
    }
}