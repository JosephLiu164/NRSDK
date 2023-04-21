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

namespace NRKernal.Experimental.NRExamples
{
    public class FocusDistPanel : MonoBehaviour
    {
        [SerializeField]
        private Text m_TxtFocusDist;
        [SerializeField]
        private Text m_TxtFocusPlaneNorm;
        [SerializeField]
        private Slider m_DistanceAdjSlider;
        [SerializeField]
        private Toggle m_AutoFucusToggle;
        [SerializeField]
        private Toggle m_AutoFocusPlaneNormToggle;
        public FocusManager m_FocusMan;


        [SerializeField]
        private Text m_TxtTargetFPS;
        [SerializeField]
        private Button m_BtnTargetFPS;
        private int m_TargetFPS = 30;

        [SerializeField]
        GameObject m_FocusAnchorGO;

        void Start()
        {
            if (m_FocusMan == null)
            {
                m_FocusMan = GameObject.FindObjectOfType<FocusManager>();
            }

            float default_distance = 1.4f;
            m_TxtFocusDist.text = default_distance.ToString("F2");

            m_DistanceAdjSlider.maxValue = 100.0f;
            m_DistanceAdjSlider.minValue = 1.0f;
            m_DistanceAdjSlider.value = default_distance;
            m_DistanceAdjSlider.onValueChanged.AddListener(OnSlideValueChange);

            m_AutoFucusToggle.isOn = m_FocusMan != null;
            m_AutoFucusToggle.onValueChanged.AddListener(OnToggleChanged);

            m_AutoFocusPlaneNormToggle.isOn = m_FocusMan != null ? m_FocusMan.adjustFocusPlaneNorm : false;
            m_AutoFocusPlaneNormToggle.onValueChanged.AddListener(OnToggleFocusPlaneNormChanged);

            /// targetFPS
            m_TargetFPS = 30;
            NRFrame.targetFrameRate = m_TargetFPS;

            m_TxtTargetFPS.text = m_TargetFPS.ToString();
            m_BtnTargetFPS.onClick.AddListener(OnBtnTargetFPS);
        }

        void OnToggleChanged(bool isOn)
        {
            if (m_FocusMan != null)
            {
                m_FocusMan.enabled = isOn;
            }
        }

        void OnToggleFocusPlaneNormChanged(bool isOn)
        {
            if (m_FocusMan != null)
            {
                m_FocusMan.adjustFocusPlaneNorm = isOn;
            }
        }

        void OnSlideValueChange(float val)
        {
            NRFrame.SetFocusDistance(val);
            m_TxtFocusDist.text = val.ToString("F2");
        }

        void OnBtnTargetFPS()
        {
            if (m_TargetFPS == 60)
                m_TargetFPS = 10;
            else if (m_TargetFPS == 10)
                m_TargetFPS = 15;
            else if (m_TargetFPS == 15)
                m_TargetFPS = 30;
            else if (m_TargetFPS == 30)
                m_TargetFPS = 60;
            else
                m_TargetFPS = 30;

            NRFrame.targetFrameRate = m_TargetFPS;
            m_TxtTargetFPS.text = m_TargetFPS.ToString();
        }

        private void Update()
        {
            var focusPoint = NRSessionManager.Instance.NRSwapChainMan.FocusPoint;
            float focusDist = focusPoint.magnitude;
            m_TxtFocusDist.text = focusPoint.ToString("F2");
            var focusPlaneNorm = NRSessionManager.Instance.NRSwapChainMan.FocusPlaneNorm;
            m_TxtFocusPlaneNorm.text = focusPlaneNorm.ToString("F2");

            if (m_FocusAnchorGO != null)
            {
                m_FocusAnchorGO.transform.localPosition = focusPoint;
            }
        }
    }
}
