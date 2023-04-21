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
using System.Collections.Generic;

namespace NRKernal.Enterprise.NRExamples
{
    public class GlassesSleepTimeExample : MonoBehaviour
    {
        [SerializeField]
        Dropdown mSleepTimeDropdown;

        private int[] m_SleepTimeData = new int[]
        {
            30, 60, 5 * 60, 20 * 60, 60 * 60, -1
        };


        void Start()
        {
            List<string> options = new List<string>();
            foreach (var t in m_SleepTimeData)
            {
                options.Add(ConvertToFormatTime(t));
            }
            mSleepTimeDropdown.AddOptions(options);

            int sleepTime = GetSleepTime();
            int selectedIndex = GetSelectedIndexByTime(sleepTime);
            mSleepTimeDropdown.value = selectedIndex;
            mSleepTimeDropdown.onValueChanged.AddListener(OnDropDownValueChanged);
        }

        void OnDropDownValueChanged(int index)
        {
            if (index < 0 || index >= m_SleepTimeData.Length)
            {
                return;
            }

            int secs = m_SleepTimeData[index];
            SetSleepTime(secs);
        }

        int GetSelectedIndexByTime(int timeSecs)
        {
            if (timeSecs < 0)
            {
                return m_SleepTimeData.Length - 1;
            }

            int index = 0;
            for (int i = 0; i < m_SleepTimeData.Length; i++)
            {
                if (timeSecs >= m_SleepTimeData[i] && m_SleepTimeData[i] > 0)
                {
                    index = i;
                }
                else
                    break;
            }

            return index;
        }

        string ConvertToFormatTime(int secs)
        {
            if (secs < 0)
            {
                return "Never";
            }
            else if (secs < 60)
            {
                return string.Format("{0} s", secs);
            }
            else if (secs < 60 * 60)
            {
                return string.Format("{0} min", secs / 60);
            }
            else
            {
                return string.Format("{0} hour", secs / 60 / 60);
            }
        }

        private int GetSleepTime()
        {
            int powerMode = NRDevice.Subsystem.GetPowerMode();
            if (powerMode == 0)
            {
                return -1;
            }
            return NRDevice.Subsystem.GetSleepTime();
        }

        public static void SetSleepTime(int tSecs)
        {
            int powerMode = tSecs < 0 ? 0 : 1;
            NRDevice.Subsystem.SetPowerMode(powerMode);

            if (powerMode == 1)
            {
                NRDevice.Subsystem.SetSleepTime(tSecs);
            }
        }
    }
}