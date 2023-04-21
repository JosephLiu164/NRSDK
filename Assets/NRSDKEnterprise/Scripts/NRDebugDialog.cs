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
    using UnityEngine;
    using System.Collections;

    public class NRDebugDialog : MonoBehaviour
    {
        public class DebugInfoProxy : AndroidJavaProxy
        {
            public DebugInfoProxy() : base("ai.nreal.virtualcontroller.debug.IDebugInfoProvider")
            {
            }

            public string getVersion()
            {
                return NativeConstants.VersionCode;
            }

            public string getExtraInfo()
            {
                return NRDevice.Subsystem.GetGlassesStatus();
            }

            public void open()
            {
                NRDebugger.logLevel = LogLevel.All;
                NRDevice.Subsystem.SetLoggerTrigger();
            }
        }

        void Start()
        {
            StartCoroutine(BindDebugDialog());
        }

        private IEnumerator BindDebugDialog()
        {
            var virtualDisplay = GameObject.FindObjectOfType<NRVirtualDisplayer>();
            while (virtualDisplay == null || !virtualDisplay.Subsystem.running)
            {
                yield return new WaitForEndOfFrame();
                if (virtualDisplay == null)
                {
                    virtualDisplay = GameObject.FindObjectOfType<NRVirtualDisplayer>();
                }
            }

            NRDefaultPhoneScreenProvider.RegistDebugInfoProxy(new DebugInfoProxy());
        }
    }
}
