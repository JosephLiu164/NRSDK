/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

#if USING_XR_MANAGEMENT && USING_XR_SDK_NREAL
#define USING_XR_SDK
#endif

namespace NRKernal
{
    using System;
    using System.Linq;
    using UnityEngine;
    using AOT;
#if USING_XR_SDK 
    using UnityEngine.XR;
    using System.Runtime.InteropServices;
#endif

    [RequireComponent(typeof(Camera))]
    public class NRDisplayOverlay : OverlayBase
    {
        /// <summary>
        /// Which display this overlay should render to.
        /// </summary>
        [Tooltip("Which display this overlay should render to.")]
        public NativeDevice targetDisplay;
        private Camera m_RenderCamera;
        private int m_EyeWidth;
        private int m_EyeHeight;

#if USING_XR_SDK
        /// <summary> Renders the event delegate described by eventID. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        private delegate void RenderEventDelegate(int eventID);
        /// <summary> Handle of the render thread. </summary>
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        /// <summary> The render thread handle pointer. </summary>
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);
        
        private const int k_CreateDisplayTextures = 0x0001;
        
        private IntPtr m_WorkingTexture = IntPtr.Zero;
        internal IntPtr WorkingTexture
        {
            get { return m_WorkingTexture; }
        }        
        internal bool isReady = false;
#endif

        protected override void Initialize()
        {
            base.Initialize();

            m_RenderCamera = gameObject.GetComponent<Camera>();

#if !UNITY_EDITOR
            var resolution = NRFrame.GetDeviceResolution(targetDisplay);
            m_EyeWidth = resolution.width;
            m_EyeHeight = resolution.height;
#else
            m_EyeWidth = 1920;
            m_EyeHeight = 1080;
#endif

            this.compositionDepth = int.MaxValue - 1;
            m_BufferSpec.size = new NativeResolution((int)m_EyeWidth, (int)m_EyeHeight);
            m_BufferSpec.colorFormat = NRTextureFormat.NR_TEXTURE_FORMAT_COLOR_RGBA8;
            m_BufferSpec.depthFormat = NRTextureFormat.NR_TEXTURE_FORMAT_DEPTH_24;
            m_BufferSpec.samples = 1;
            m_BufferSpec.surfaceFlag = (int)NRExternalSurfaceFlags.NONE;
            m_BufferSpec.createFlag = (UInt64)NRSwapchainCreateFlags.NR_SWAPCHAIN_CREATE_FLAGS_NONE;
        }

#if !USING_XR_SDK
        void OnEnable()
        {
            Camera.onPreRender += OnPreRenderCallback;
        }

        void OnDisable()
        {
            Camera.onPreRender -= OnPreRenderCallback;
        }
        
        void OnPreRenderCallback(Camera cam)
        {
            if (cam != m_RenderCamera)
                return;

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Debug("[NRDisplayOverlay] OnPreRenderCallback : {0}", cam.name);

            // populate frame once only
            if (targetDisplay == NativeDevice.LEFT_DISPLAY)
                NRSessionManager.Instance.NRSwapChainMan.PopulateFrame();

            var bufferHandler = NRSessionManager.Instance.NRSwapChainMan.GetWorkingBufferHandler(m_SwapChainHandler);
            
            // Camera's targetTexture need to be set on OnPreRenderCallback of same camera.
            if (bufferHandler != IntPtr.Zero)
                PopulateBuffers(bufferHandler);
        }
#endif

        internal override void PopulateBuffers(IntPtr bufferHandler)
        {
            if (!Textures.ContainsKey(bufferHandler))
            {
                NRDebugger.Error("[NRDisplayOverlay] Can not find the texture:" + bufferHandler);
                return;
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Debug("[NRDisplayOverlay] PopulateBuffers: workingTex={0}", bufferHandler);
#if USING_XR_SDK
            m_WorkingTexture = bufferHandler;
            // swap render texture in XR native
            NativeXRPlugin.PopulateDisplayTexture(bufferHandler);
#else
            Texture targetTexture;
            Textures.TryGetValue(bufferHandler, out targetTexture);
            RenderTexture renderTexture = targetTexture as RenderTexture;
            if (renderTexture == null)
            {
                NRDebugger.Error("[NRDisplayOverlay] The texture is null...");
                return;
            }

            m_RenderCamera.targetTexture = renderTexture;
#endif
        }

        private NativeTransform CalculatePose(NativeDevice targetDisplay)
        {
            var headPose = NRFrame.HeadPose;
            var eyePose = targetDisplay == NativeDevice.LEFT_DISPLAY ? NRFrame.EyePoseFromHead.LEyePose : NRFrame.EyePoseFromHead.REyePose;

            var mv = ConversionUtility.GetTMatrix(headPose) * ConversionUtility.GetTMatrix(eyePose);
            return ConversionUtility.UnityMatrixToApiPose(mv);
        }

        internal void NRDebugSaveToPNG()
        {
            UnityExtendedUtility.SaveTextureAsPNG(m_RenderCamera.targetTexture);
        }

        internal override void CreateViewport()
        {
#if USING_XR_SDK
            m_ViewPorts = new ViewPort[2];
            m_ViewPorts[0].viewportType = NRViewportType.NR_VIEWPORT_PROJECTION;
            m_ViewPorts[0].sourceUV = new NativeRectf(0, 0, 1, 1);
            m_ViewPorts[0].targetDisplay = NativeDevice.LEFT_DISPLAY;
            m_ViewPorts[0].swapchainHandler = m_SwapChainHandler;
            m_ViewPorts[0].is3DLayer = true;
            m_ViewPorts[0].textureArraySlice = 0;
            m_ViewPorts[0].spaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
            m_ViewPorts[0].nativePose = CalculatePose(NativeDevice.LEFT_DISPLAY);
            NRFrame.GetEyeFov(NativeDevice.LEFT_DISPLAY, ref m_ViewPorts[0].fov);
            NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[0]);

            m_ViewPorts[1].viewportType = NRViewportType.NR_VIEWPORT_PROJECTION;
            m_ViewPorts[1].sourceUV = new NativeRectf(0, 0, 1, 1);
            m_ViewPorts[1].targetDisplay = NativeDevice.RIGHT_DISPLAY;
            m_ViewPorts[1].swapchainHandler = m_SwapChainHandler;
            m_ViewPorts[1].is3DLayer = true;
            m_ViewPorts[1].textureArraySlice = 1;
            m_ViewPorts[1].spaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
            m_ViewPorts[1].nativePose = CalculatePose(NativeDevice.RIGHT_DISPLAY);
            NRFrame.GetEyeFov(NativeDevice.RIGHT_DISPLAY, ref m_ViewPorts[1].fov);
            NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[1]);
#else
            m_ViewPorts = new ViewPort[1];
            m_ViewPorts[0].viewportType = NRViewportType.NR_VIEWPORT_PROJECTION;
            m_ViewPorts[0].sourceUV = new NativeRectf(0, 0, 1, 1);
            m_ViewPorts[0].targetDisplay = targetDisplay;
            m_ViewPorts[0].swapchainHandler = m_SwapChainHandler;
            m_ViewPorts[0].is3DLayer = true;
            m_ViewPorts[0].textureArraySlice = -1;
            m_ViewPorts[0].spaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
            m_ViewPorts[0].nativePose = CalculatePose(targetDisplay);
            NRFrame.GetEyeFov(targetDisplay, ref m_ViewPorts[0].fov);
            NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[0]);
#endif
        }

        internal override void PopulateViewPort()
        {
            if (m_ViewPorts == null)
            {
                NRDebugger.Warning("Can not update view port for this layer:{0}", gameObject.name);
                return;
            }

#if USING_XR_SDK
            m_ViewPorts[0].nativePose = CalculatePose(NativeDevice.LEFT_DISPLAY);
            m_ViewPorts[1].nativePose = CalculatePose(NativeDevice.RIGHT_DISPLAY);
            NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[0]);
            NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[1]);
#else
            m_ViewPorts[0].nativePose = CalculatePose(targetDisplay);
            NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[0]);
#endif
        }

        internal override void CreateOverlayTextures()
        {
            NRDebugger.Info("[NRDisplayOverlay] CreateOverlayTextures.");
            ReleaseOverlayTextures();

#if USING_XR_SDK
            // For XR mode, Display textures need to be created in render thread.
            GL.IssuePluginEvent(RenderThreadHandlePtr, k_CreateDisplayTextures);
#else
            for (int i = 0; i < m_BufferSpec.bufferCount; i++)
            {
                RenderTexture texture = UnityExtendedUtility.CreateRenderTexture((int)m_EyeWidth, (int)m_EyeHeight, 24, RenderTextureFormat.ARGB32);
                IntPtr texturePtr = texture.GetNativeTexturePtr();
                Textures.Add(texturePtr, texture);
            }
            if (Textures.Count > 0)
                NRSessionManager.Instance.NRSwapChainMan.NativeSwapchain.SetSwapChainBuffers(m_SwapChainHandler, Textures.Keys.ToArray());
#endif
        }


#if USING_XR_SDK
        void GfxThread_CreateOverlayTextures()
        {
            NRDebugger.Info("[NRDisplayOverlay] GfxThread_CreateOverlayTextures.");

            // Create render texture from XR native
            // In XR plugin framework, left/right eye should use same texture array target.
            var texturePtrs = NativeXRPlugin.CreateDisplayTextures(m_BufferSpec.bufferCount, m_EyeWidth, m_EyeHeight, 2);
            for (int i = 0; i < m_BufferSpec.bufferCount; i++)
            {
                if (NRDebugger.logLevel <= LogLevel.Debug)
                    NRDebugger.Info("[NRDisplayOverlay] CreateOverlayTextures-[{0}]: {1}", i, texturePtrs[i]);
                Textures.Add(texturePtrs[i], null);
            }

            isReady = true;
            if (Textures.Count > 0)
                NRSessionManager.Instance.NRSwapChainMan.NativeSwapchain.SetSwapChainBuffers(m_SwapChainHandler, Textures.Keys.ToArray());
        }

        /// <summary> Executes the 'on render thread' operation. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            if (NRDebugger.logLevel <= LogLevel.Debug)
                UnityEngine.Debug.LogFormat("[NRDisplayOverlay] RunOnRenderThread : eventID={0}", eventID);
            if (eventID == k_CreateDisplayTextures)
            {
                NRSessionManager.Instance.NRSwapChainMan.DisplayOverlay?.GfxThread_CreateOverlayTextures();
            }
        }
#endif

        internal override void ReleaseOverlayTextures()
        {
            if (Textures.Count == 0)
            {
                return;
            }

#if !USING_XR_SDK
            NRDebugger.Info("[NRDisplayOverlay] ReleaseOverlayTextures. ");
            foreach (var item in Textures)
            {
                RenderTexture rt = item.Value as RenderTexture;
                if (rt != null) rt.Release();
            }
#endif

            Textures.Clear();
        }
    }
}
