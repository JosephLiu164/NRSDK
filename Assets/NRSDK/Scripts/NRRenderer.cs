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
    using AOT;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using UnityEngine;


    /// <summary>
    /// NRNativeRender operate rendering-related things, provides the feature of optimized rendering
    /// and low latency. </summary>
    public class NRRenderer : MonoBehaviour
    {
        /// <summary> Renders the event delegate described by eventID. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        private delegate void RenderEventDelegate(int eventID);
        /// <summary> Handle of the render thread. </summary>
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        /// <summary> The render thread handle pointer. </summary>
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);

        private const int STARTNATIVERENDEREVENT = 0x0002;
        private const int RESUMENATIVERENDEREVENT = 0x0003;
        private const int PAUSENATIVERENDEREVENT = 0x0004;
        private const int STOPNATIVERENDEREVENT = 0x0005;

        public enum Eyes
        {
            /// <summary> Left Display. </summary>
            Left = 0,
            /// <summary> Right Display. </summary>
            Right = 1,
            Count = 2
        }

        public Camera leftCamera;
        public Camera rightCamera;
        /// <summary> Gets or sets the native renderring. </summary>
        /// <value> The m native renderring. </value>
        private static NativeRenderring m_NativeRenderring;
        internal static NativeRenderring NativeRenderring
        {
            get
            {
                if (NRSessionManager.Instance.NativeAPI != null)
                {
                    m_NativeRenderring = NRSessionManager.Instance.NativeAPI.NativeRenderring;
                }

                return m_NativeRenderring;
            }
            set
            {
                m_NativeRenderring = value;
            }
        }
        
        /// <summary> Values that represent renderer states. </summary>
        public enum RendererState
        {
            UnInitialized,
            Initialized,
            Running,
            Paused,
            Destroyed
        }

        /// <summary> The current state. </summary>
        private RendererState m_CurrentState = RendererState.UnInitialized;
        /// <summary> Gets the current state. </summary>
        /// <value> The current state. </value>
        public RendererState CurrentState
        {
            get
            {
                return m_CurrentState;
            }
            set
            {
                m_CurrentState = value;
            }
        }

        /// <summary> Gets a value indicating whether this object is linear color space. </summary>
        /// <value> True if this object is linear color space, false if not. </value>
        public static bool isLinearColorSpace
        {
            get
            {
                return QualitySettings.activeColorSpace == ColorSpace.Linear;
            }
        }

        public void Create()
        {
            NRDebugger.Info("[NRRender] Create");
#if !UNITY_EDITOR
            NativeRenderring.Create();
#endif
            NRDebugger.Info("[NRRender] Created");
        }

        private void Start() {
            
        }

        /// <summary> Start the render pipleline. </summary>
        /// <param name="leftcamera">  Left Eye.</param>
        /// <param name="rightcamera"> Right Eye.</param>
        ///
        /// ### <param name="poseprovider"> provide the pose of camera every frame.</param>
        public void Start(Camera leftcamera, Camera rightcamera)
        {
            NRDebugger.Info("[NRRender] Start");
            if (m_CurrentState != RendererState.UnInitialized)
            {
                return;
            }

            leftCamera = leftcamera;
            rightCamera = rightcamera;

#if !UNITY_EDITOR
            leftCamera.depthTextureMode = DepthTextureMode.None;
            rightCamera.depthTextureMode = DepthTextureMode.None;
            leftCamera.rect = new Rect(0, 0, 1, 1);
            rightCamera.rect = new Rect(0, 0, 1, 1);
            leftCamera.enabled = false;
            rightCamera.enabled = false;
            m_CurrentState = RendererState.Initialized;

            GL.IssuePluginEvent(RenderThreadHandlePtr, STARTNATIVERENDEREVENT);
#endif
        }

        /// <summary> Pause render. </summary>
        public void Pause()
        {
            NRDebugger.Info("[NRRender] Pause");
            if (m_CurrentState != RendererState.Running)
            {
                return;
            }
            GL.IssuePluginEvent(RenderThreadHandlePtr, PAUSENATIVERENDEREVENT);
        }

        /// <summary> Resume render. </summary>
        public void Resume()
        {
            Invoke("DelayResume", 0.3f);
        }

        /// <summary> Delay resume. </summary>
        private void DelayResume()
        {
            NRDebugger.Info("[NRRender] Resume");
            if (m_CurrentState != RendererState.Paused)
            {
                return;
            }
            GL.IssuePluginEvent(RenderThreadHandlePtr, RESUMENATIVERENDEREVENT);
        }

#if !UNITY_EDITOR
        void Update()
        {
            if (m_CurrentState == RendererState.Running)
            {
                leftCamera.enabled = true;
                rightCamera.enabled = true;
            }
        }
#endif

        /// <param name="distance"> The distance from plane to center camera.</param>
        [Obsolete("Use NRFrame.SetFocusDistance instead", false)]
        public void SetFocusDistance(float distance)
        {
            NRFrame.SetFocusDistance(distance);
        }

        /// <summary> Executes the 'on render thread' operation. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            NRDebugger.Info("[NRRender] RunOnRenderThread : eventID={0}", eventID);

            if (eventID == STARTNATIVERENDEREVENT)
            {
                NativeRenderring?.Start();
                NRSessionManager.Instance.NRRenderer.CurrentState = RendererState.Running;
            }
            else if (eventID == RESUMENATIVERENDEREVENT)
            {
                NativeRenderring?.Resume();
                NRSessionManager.Instance.NRRenderer.CurrentState = RendererState.Running;
            }
            else if (eventID == PAUSENATIVERENDEREVENT)
            {
                NRSessionManager.Instance.NRRenderer.CurrentState = RendererState.Paused;
                NativeRenderring?.Pause();
            }
        }

        public void Destroy()
        {
            if (m_CurrentState == RendererState.Destroyed || m_CurrentState == RendererState.UnInitialized)
            {
                return;
            }

            // NRDebugger.Info("[NRRender] Destroy, issue event");
            // GL.IssuePluginEvent(RenderThreadHandlePtr, STOPNATIVERENDEREVENT);

            NRDebugger.Info("[NRRender] Destroy");
            m_CurrentState = RendererState.Destroyed;
            NativeRenderring?.Stop();
            NativeRenderring?.Destroy();
            NativeRenderring = null;
            NRDebugger.Info("[NRRender] Destroyed");
        }

        private void OnDestroy()
        {
            this.Destroy();
        }
    }
}
