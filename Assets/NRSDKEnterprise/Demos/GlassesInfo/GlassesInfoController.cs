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

namespace NRKernal.Enterprise.NRExamples
{
    /// <summary> A controller for handling glasses informations. </summary>
    public class GlassesInfoController : MonoBehaviour
    {
        /// <summary> The switch 2D control. </summary>
        public Button m_Switch2dBtn;
        /// <summary> The switch 3D control. </summary>
        public Button m_Switch3dBtn;
        /// <summary> The slider. </summary>
        public Slider m_Slider;
        /// <summary> Information describing the glasses. </summary>
        public Text m_GlassesInfo;

        /// <summary> The foreground head temporary. </summary>
        public Text ForeHeadTemp;
        /// <summary> The temple temporary. </summary>
        public Text TempleTemp;
        private int m_Brightness = -1;

        /// <summary> Use this for initialization. </summary>
        void Start()
        {
            m_Switch2dBtn.onClick.AddListener(new UnityAction(() =>
            {
                NRDevice.Subsystem.SetMode(NativeGlassesMode.TwoD_1080);
            }));

            m_Switch3dBtn.onClick.AddListener(new UnityAction(() =>
            {
                NRDevice.Subsystem.SetMode(NativeGlassesMode.ThreeD_1080);
            }));

            m_Slider.maxValue = NRDeviceSubsystem.BrightnessMax;
            m_Slider.onValueChanged.AddListener((value) =>
            {
                int bight_value = (int)value;
                if (m_Brightness != bight_value)
                {
                    m_Brightness = bight_value;
                    NRDevice.Subsystem.SetBrightness(bight_value);
                }
            });

            m_GlassesInfo.text = NRDevice.Subsystem.GetVersion();
            StartCoroutine(UpdateTemprature());
        }

        void OnEnable()
        {
            NRDevice.Subsystem.RegisGlassesControllerExtraCallbacks();
            NRDevice.Subsystem.AddEventListener(OnBrightnessValueCallback);
            NRDevice.Subsystem.AddEventListener(OnBrightnessKeyCallback);
        }

        void OnDisable()
        {
            NRDevice.Subsystem.RemoveEventListener(OnBrightnessValueCallback);
            NRDevice.Subsystem.RemoveEventListener(OnBrightnessKeyCallback);
        }

        /// <summary> Executes the 'brightness key callback' action. </summary>
        /// <param name="key"> The key.</param>
        private void OnBrightnessKeyCallback(NRBrightnessKEYEvent key)
        {
            NRDebugger.Info("key:" + key.ToString());
        }

        /// <summary> Executes the 'brightness value callback' action. </summary>
        /// <param name="brightness"> The brightness.</param>
        private void OnBrightnessValueCallback(int brightness)
        {
            NRDebugger.Info("bightness:" + brightness);
        }

        /// <summary> Updates the temprature. </summary>
        /// <returns> An IEnumerator. </returns>
        IEnumerator UpdateTemprature()
        {
            while (true)
            {
                ForeHeadTemp.text = NRDevice.Subsystem.GetTemprature(NativeGlassesTemperaturePosition.TEMPERATURE_POSITION_GLASSES_FOREHEAD).ToString();
                TempleTemp.text = NRDevice.Subsystem.GetTemprature(NativeGlassesTemperaturePosition.TEMPERATURE_POSITION_GLASSES_TEMPLE).ToString();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
