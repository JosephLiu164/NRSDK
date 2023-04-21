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
    using System;
    using System.Collections;
    using UnityEngine;

    public class NRTrackingModeChangedListener
    {
        public delegate void OnTrackStateChangedDel(bool trackChanging, RenderTexture leftRT, RenderTexture rightRT);
        public event OnTrackStateChangedDel OnTrackStateChanged;
        private NRTrackingModeChangedTip m_LostTrackingTip;
        private Coroutine m_EnableRenderCamera;
        private Coroutine m_DisableRenderCamera;
        private const float MinTimeLastLimited = 0.5f;
        private const float MaxTimeLastLimited = 6f;

        public NRTrackingModeChangedListener()
        {
            NRHMDPoseTracker.OnHMDLostTracking += OnHMDLostTracking;
            NRHMDPoseTracker.OnChangeTrackingMode += OnChangeTrackingMode;
        }

        private void OnHMDLostTracking()
        {
            NRDebugger.Info("[NRTrackingModeChangedListener] OnHMDLostTracking: {0}", NRFrame.LostTrackingReason);
            ShowTips(string.Empty);
        }

        private void OnChangeTrackingMode(NRHMDPoseTracker.TrackingType origin, NRHMDPoseTracker.TrackingType target)
        {
            NRDebugger.Info("[NRTrackingModeChangedListener] OnChangeTrackingMode: {0} => {1}", origin, target);
            ShowTips(NativeConstants.TRACKING_MODE_SWITCH_TIP);
        }

        private void ShowTips(string tips)
        {
            if (m_EnableRenderCamera != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_EnableRenderCamera);
                m_EnableRenderCamera = null;
            }
            if (m_DisableRenderCamera != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_DisableRenderCamera);
                m_DisableRenderCamera = null;
            }
            m_EnableRenderCamera = NRKernalUpdater.Instance.StartCoroutine(EnableTrackingInitializingRenderCamera(tips));
        }

        public IEnumerator EnableTrackingInitializingRenderCamera(string tips)
        {
            if (m_LostTrackingTip == null)
            {
                m_LostTrackingTip = NRTrackingModeChangedTip.Create();
            }
            m_LostTrackingTip.gameObject.SetActive(true);
            var reason = NRFrame.LostTrackingReason;
            m_LostTrackingTip.SetMessage(tips);

            float begin_time = Time.realtimeSinceStartup;
            var endofFrame = new WaitForEndOfFrame();
            yield return endofFrame;
            yield return endofFrame;
            yield return endofFrame;
            NRDebugger.Info("[NRTrackingModeChangedListener] Enter tracking initialize mode...");
            OnTrackStateChanged?.Invoke(true, m_LostTrackingTip.LeftRT, m_LostTrackingTip.RightRT);

            NRHMDPoseTracker postTracker = NRSessionManager.Instance.NRHMDPoseTracker;
            while ((NRFrame.LostTrackingReason != LostTrackingReason.NONE || postTracker.IsTrackModeChanging || (Time.realtimeSinceStartup - begin_time) < MinTimeLastLimited)
                && (Time.realtimeSinceStartup - begin_time) < MaxTimeLastLimited)
            {
                NRDebugger.Info("[NRTrackingModeChangedListener] Wait for tracking: modeChanging={0}, lostTrackReason={1}",
                    postTracker.IsTrackModeChanging, NRFrame.LostTrackingReason);
                yield return endofFrame;
            }

            if (m_DisableRenderCamera == null)
            {
                m_DisableRenderCamera = NRKernalUpdater.Instance.StartCoroutine(DisableTrackingInitializingRenderCamera());
            }
            m_EnableRenderCamera = null;
        }

        public IEnumerator DisableTrackingInitializingRenderCamera()
        {
            if (m_LostTrackingTip != null)
            {
                m_LostTrackingTip.gameObject.SetActive(false);
            }
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            OnTrackStateChanged?.Invoke(false, m_LostTrackingTip.LeftRT, m_LostTrackingTip.RightRT);
            NRDebugger.Info("[NRTrackingModeChangedListener] Exit tracking initialize mode...");
            m_DisableRenderCamera = null;
        }
    }
}
