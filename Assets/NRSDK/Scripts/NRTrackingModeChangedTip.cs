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
    using UnityEngine.UI;
    using System.Collections;
    using System.IO;

    public class NRTrackingModeChangedTip : MonoBehaviour
    {
        [SerializeField]
        private Camera m_LeftCamera;
        [SerializeField]
        private Camera m_RightCamera;
        [SerializeField]
        private Transform m_Plane;
        [SerializeField]
        private float m_Distance = 25;

        [SerializeField]
        private Text m_Lable;
        [SerializeField]
        private Transform m_LoadingUI;
        public RenderTexture LeftRT { get; private set; }
        public RenderTexture RightRT { get; private set; }


        [SerializeField]
        private NROverlay m_LeftOverlay;
        [SerializeField]
        private NROverlay m_RightOverlay;

        private static NativeResolution resolution = new NativeResolution(1920, 1080);

        public static NRTrackingModeChangedTip Create()
        {
            NRDebugger.Info("[NRTrackingModeChangedTip] Create");
            NRTrackingModeChangedTip lostTrackingTip;
            var config = NRSessionManager.Instance.NRSessionBehaviour?.SessionConfig;
            if (config == null || config.TrackingModeChangeTipPrefab == null)
            {
                lostTrackingTip = GameObject.Instantiate(Resources.Load<NRTrackingModeChangedTip>("NRTrackingModeChangedTip"));
            }
            else
            {
                lostTrackingTip = GameObject.Instantiate(config.TrackingModeChangeTipPrefab);
            }
#if !UNITY_EDITOR
            resolution = NRFrame.GetDeviceResolution(NativeDevice.LEFT_DISPLAY);
#endif
            lostTrackingTip.LeftRT = UnityExtendedUtility.CreateRenderTexture(resolution.width, resolution.height, 24, RenderTextureFormat.ARGB32);
            lostTrackingTip.m_LeftCamera.targetTexture = lostTrackingTip.LeftRT;
            lostTrackingTip.m_LeftCamera.clearFlags = CameraClearFlags.Color;
            lostTrackingTip.m_LeftCamera.backgroundColor = new Color(0, 0, 0, 1);

            lostTrackingTip.m_LeftOverlay.MainTexture = lostTrackingTip.LeftRT;
            lostTrackingTip.m_LeftOverlay.compositionDepth = int.MaxValue;
            lostTrackingTip.m_LeftOverlay.layerSide = LayerSide.Left;

            lostTrackingTip.RightRT = UnityExtendedUtility.CreateRenderTexture(resolution.width, resolution.height, 24, RenderTextureFormat.ARGB32);
            lostTrackingTip.m_RightCamera.targetTexture = lostTrackingTip.RightRT;
            lostTrackingTip.m_RightCamera.clearFlags = CameraClearFlags.Color;
            lostTrackingTip.m_RightCamera.backgroundColor = new Color(0, 0, 0, 1);

            lostTrackingTip.m_RightOverlay.MainTexture = lostTrackingTip.RightRT;
            lostTrackingTip.m_RightOverlay.compositionDepth = int.MaxValue;
            lostTrackingTip.m_RightOverlay.layerSide = LayerSide.Right;
            
            NRDebugger.Info("[NRTrackingModeChangedTip] Created");
            return lostTrackingTip;
        }

        public void SetMessage(string msg)
        {
            m_Lable.text = msg;
        }

        void Update()
        {
            m_LoadingUI.Rotate(-Vector3.forward, 2f, Space.Self);
        }

        void LateUpdate()
        {
            var leftCameraPosition = m_LeftCamera.transform.localPosition;
            var rightCameraPosition = m_RightCamera.transform.localPosition;
            var leftCameraRotation = m_LeftCamera.transform.localRotation;
            var rightCameraRotation = m_RightCamera.transform.localRotation;
            var leftCameraForward = m_LeftCamera.transform.forward;
            var rightCameraForward = m_RightCamera.transform.forward;

            var centerPos = (leftCameraPosition + rightCameraPosition) * 0.5f;
            var forward = (leftCameraForward + rightCameraForward) * 0.5f;
            var centerRotation = Quaternion.Lerp(leftCameraRotation, rightCameraRotation, 0.5f);

            m_Plane.localPosition = centerPos + forward * m_Distance;
            m_Plane.localRotation = centerRotation;
        }

        void Start()
        {
            m_LeftCamera.aspect = (float)resolution.width / resolution.height;
            m_RightCamera.aspect = (float)resolution.width / resolution.height;
        }

        void OnEnable()
        {
            m_LeftCamera.enabled = true;
            m_RightCamera.enabled = true;
        }

        void OnDisable()
        {
            m_LeftCamera.enabled = false;
            m_RightCamera.enabled = false;
        }

        void OnDestroy()
        {
            if (LeftRT != null)
            {
                LeftRT.Release();
                Destroy(LeftRT);
                LeftRT = null;
            }
            if (RightRT != null)
            {
                RightRT.Release();
                Destroy(RightRT);
                RightRT = null;
            }
        }
    }
}
