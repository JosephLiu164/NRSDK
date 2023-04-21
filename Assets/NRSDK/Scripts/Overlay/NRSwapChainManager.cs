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
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.Assertions;
    using AOT;

    public enum LayerTextureType
    {
        RenderTexture,
        StandardTexture,
        EglTexture
    }

    public struct BufferSpec
    {
        public NativeResolution size;
        public NRTextureFormat colorFormat;
        public NRTextureFormat depthFormat;
        public int surfaceFlag;
        public UInt64 createFlag;
        public int samples;
        // Number of render buffers
        public int bufferCount;

        public void Copy(BufferSpec bufferspec)
        {
            this.size = bufferspec.size;
            this.colorFormat = bufferspec.colorFormat;
            this.depthFormat = bufferspec.depthFormat;
            this.samples = bufferspec.samples;
            this.bufferCount = bufferspec.bufferCount;
            this.surfaceFlag = bufferspec.surfaceFlag;
        }

        public override string ToString()
        {
            return string.Format("[size:{0} bufferCount:{1}, surfaceFlag:{2}, createFlag:{3}]"
                , size.ToString(), bufferCount, surfaceFlag, createFlag);
        }
    }

    public enum LayerSide
    {
        Left = 0,
        Right = 1,
        Both = 2,
        [HideInInspector]
        Count = Both
    };

    internal struct ViewPort
    {
        internal bool is3DLayer;
        internal bool isExternalSurface;
        internal int index;
        internal UInt64 nativeHandler;
        internal UInt64 swapchainHandler;
        internal NativeRectf sourceUV;
        internal NativeDevice targetDisplay;
        internal NRViewportType viewportType;
        internal NRReferenceSpaceType spaceType;
        // internal Matrix4x4 transform;
        internal NativeTransform nativePose;
        internal Vector2 quadSize;
        internal NativeFov4f fov;
        internal int textureArraySlice;

        public override string ToString()
        {
            return string.Format("[index:{0} nativeHandler:{1} swapchainHandler:{2} targetDisplay:{3} viewportType:{4} spaceType:{5} is3DLayer:{6} isExternalSurface:{7} textureArraySlice:{8}]", 
                index, nativeHandler, swapchainHandler, targetDisplay, viewportType, spaceType, is3DLayer, isExternalSurface, textureArraySlice);
        }
    };

    public class NRSwapChainManager : MonoBehaviour
    {
        private static List<OverlayBase> Overlays = new List<OverlayBase>();
        private UInt64 m_RenderHandle;
        internal NativeSwapchain NativeSwapchain { get; set; }
        private UInt64 m_FrameHandle = 0;
        public UInt64 FrameHandle
        {
            get { return m_FrameHandle; }
        }
        private UInt64 m_CachFrameHandle = 0;
        private Dictionary<UInt64, IntPtr> m_LayerWorkingBufferDict;
        private Queue<TaskInfo> m_TaskQueue;

        /// <summary>
        /// Max overlay count is MaxOverlayCount-2, the two overlay are left display and right display.
        /// </summary>
        private const int MaxOverlayCount = 7;

        private struct TaskInfo
        {
            public Action<OverlayBase> callback;
            public OverlayBase obj;
        }

        internal Action onBeforeSubmitFrameInMainThread;

        private bool IsMultiThread
        {
            get
            {
                if (NRSessionManager.Instance.NRSessionBehaviour != null)
                {
                    return NRSessionManager.Instance.NRSessionBehaviour.SessionConfig.UseMultiThread;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsInitialized { get; private set; }

        private AndroidJavaObject m_ProtectedCodec;
        protected AndroidJavaObject ProtectedCodec
        {
            get
            {
                if (m_ProtectedCodec == null)
                {
                    m_ProtectedCodec = new AndroidJavaObject("ai.nreal.protect.session.ProtectSession");
                }

                return m_ProtectedCodec;
            }
        }

#if USING_XR_SDK
        private NRDisplayOverlay m_DisplayOverlay;
        internal NRDisplayOverlay DisplayOverlay
        {
            get { return m_DisplayOverlay; }
        }
#else
        private NRDisplayOverlay m_LeftDisplayOverlay;
        private NRDisplayOverlay m_RightDisplayOverlay;
#endif


        /// <summary> Renders the event delegate described by eventID. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        private delegate void RenderEventDelegate(int eventID);
        /// <summary> Handle of the render thread. </summary>
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        /// <summary> The render thread handle pointer. </summary>
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);
        
        private const int k_InitSwapChainEvent = 0x0001;
        private const int k_SubmitFrameEvent = 0x0002;

        private const float m_DefaultFocusDistance = 1.4f;

        private Vector3 m_FocusPoint = new Vector3(0, 0, m_DefaultFocusDistance);
        private Vector3 m_FocusPointGL = new Vector3(0, 0, -m_DefaultFocusDistance);
        public Vector3 FocusPoint
        {
            get { return m_FocusPoint; }
        }

        private Vector3 m_FocusPlaneNorm = -Vector3.forward;
        private Vector3 m_FocusPlaneNormGL = Vector3.forward;
        public Vector3 FocusPlaneNorm
        {
            get { return m_FocusPlaneNorm; }
        }

        private NRFrameFlags m_FrameChangedType = NRFrameFlags.NR_FRAME_CHANGED_NONE;

        void Awake()
        {
            NRDebugger.Info("[SwapChain] Awake");
            m_LayerWorkingBufferDict = new Dictionary<UInt64, IntPtr>();
            m_TaskQueue = new Queue<TaskInfo>();

#if !UNITY_EDITOR 
#if USING_XR_SDK
            NativeXRPlugin.RegistEventCallback(OnGfxThreadStart, OnGfxThreadPopulateFrame, OnGfxThreadSubmit);
#else
            StartCoroutine(RenderCoroutine());
#endif
#endif
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                InitDisplayOverlay();
            }
            
        }

        // void OnSessionStateChanged(SessionState state)
        void InitDisplayOverlay()
        {
#if USING_XR_SDK
            NRDebugger.Info("[SwapChain] InitDisplayOverlay USING_XR_SDK");
            var centerCamera = NRSessionManager.Instance.NRHMDPoseTracker.centerCamera;
            var cOverlay = centerCamera.gameObject.GetComponent<NRDisplayOverlay>();
            if (cOverlay == null)
            {
                cOverlay = centerCamera.gameObject.AddComponent<NRDisplayOverlay>();
            }
            m_DisplayOverlay = cOverlay;
#else   
            NRDebugger.Info("[SwapChain] InitDisplayOverlay No USING_XR_SDK.");
            var leftCamera = NRSessionManager.Instance.NRHMDPoseTracker.leftCamera;
            var lOverlay = leftCamera.gameObject.GetComponent<NRDisplayOverlay>();
            if (lOverlay == null)
            {
                lOverlay = leftCamera.gameObject.AddComponent<NRDisplayOverlay>();
                lOverlay.targetDisplay = NativeDevice.LEFT_DISPLAY;
            }
            m_LeftDisplayOverlay = lOverlay;

            var rightCamera = NRSessionManager.Instance.NRHMDPoseTracker.rightCamera;
            var rOverlay = rightCamera.gameObject.GetComponent<NRDisplayOverlay>();
            if (rOverlay == null)
            {
                rOverlay = rightCamera.gameObject.AddComponent<NRDisplayOverlay>();
                rOverlay.targetDisplay = NativeDevice.RIGHT_DISPLAY;
            }
            m_RightDisplayOverlay = rOverlay;
#endif
        }

        [MonoPInvokeCallback(typeof(OnGfxThreadStartCallback))]
        private static void OnGfxThreadStart(UInt64 renderingHandle)
        {
            NRSessionManager.Instance.NRSwapChainMan.GfxThread_Initialize(renderingHandle);
        }

        [MonoPInvokeCallback(typeof(OnGfxThreadPopulateFrameCallback))]
        private static void OnGfxThreadPopulateFrame()
        {
            NRSessionManager.Instance.NRSwapChainMan.PopulateFrame();
        }

        [MonoPInvokeCallback(typeof(OnGfxThreadSubmitCallback))]
        private static void OnGfxThreadSubmit(UInt64 frameHandle, IntPtr idTexture)
        {
            NRSessionManager.Instance.NRSwapChainMan.GfxThread_SubmitFrame(frameHandle, idTexture);
        }

        internal void GfxThread_Initialize(UInt64 renderHandler)
        {
            if (IsInitialized)
            {
                return;
            }

            NRDebugger.Info("[SwapChain] Initialize: renderHandler={0}", renderHandler);
            m_RenderHandle = renderHandler;
            NativeSwapchain = new NativeSwapchain(renderHandler);
            IsInitialized = true;
        }

        void Update()
        {
            if (!IsReady())
            {
                return;
            }

            if (m_TaskQueue.Count != 0)
            {
                while (m_TaskQueue.Count != 0)
                {
                    var task = m_TaskQueue.Dequeue();
                    task.callback.Invoke(task.obj);
                }
            }
        }

        internal void PopulateFrame()
        {
            if (!IsReady())
            {
                return;
            }

#if USING_XR_SDK
            // wait for display layer has created texture
            if (!m_DisplayOverlay.isReady)
            {
                return;
            }
#endif

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Debug("[SwapChain] PopulateFrame");

            // NRDebugger.Info("[SwapChain] Update: frameCnt={0}", Time.frameCount);
            if (Overlays.Count != 0)
            {
#if !UNITY_EDITOR
                PopulateOverlaysRenderBuffers();
                PopulateBufferViewports();
                PopulateFrameInfo();
#endif
            }
        }

        #region common functions
        private bool IsReady()
        {
#if UNITY_EDITOR
            return IsInitialized;
#else
    #if USING_XR_SDK
            return IsInitialized && NRFrame.SessionStatus == SessionState.Running;
    #else
            return IsInitialized && NRFrame.SessionStatus == SessionState.Running && NRSessionManager.Instance.NRRenderer.CurrentState == NRRenderer.RendererState.Running;
    #endif
#endif
        }

        internal void Add(OverlayBase overlay)
        {
            if (Overlays.Contains(overlay))
            {
                NRDebugger.Warning("[SwapChain] Overlay has been existed: " + overlay.SwapChainHandler);
                return;
            }

            if (Overlays.Count == MaxOverlayCount)
            {
                throw new NotSupportedException("The current count of overlays exceeds the maximum!");
            }

            if (IsReady())
            {
                AddLayer(overlay);
            }
            else
            {
                m_TaskQueue.Enqueue(new TaskInfo()
                {
                    callback = AddLayer,
                    obj = overlay
                });
            }
        }

        internal void Remove(OverlayBase overlay)
        {
            if (!Overlays.Contains(overlay))
            {
                return;
            }

            if (IsReady())
            {
                RemoveLayer(overlay);
            }
            else
            {
                m_TaskQueue.Enqueue(new TaskInfo()
                {
                    callback = RemoveLayer,
                    obj = overlay
                });
            }
        }

        private void AddLayer(OverlayBase overlay)
        {
            Overlays.Add(overlay);
            Overlays.Sort();

            BufferSpec bufferSpec = overlay.BufferSpec;

            bool isExternalSurface = false;
            var nrOverlay = overlay as NROverlay;
            if (nrOverlay != null)
                isExternalSurface = nrOverlay.isExternalSurface;
            
#if !UNITY_EDITOR
            bool useTextureArray = false;

#if USING_XR_SDK
            useTextureArray = overlay is NRDisplayOverlay;
#endif

            overlay.NativeSpecHandler = NativeSwapchain.CreateBufferSpec(overlay.BufferSpec, useTextureArray);

            if (isExternalSurface)
            {
                IntPtr androidSurface = IntPtr.Zero;
                nrOverlay.SwapChainHandler = NativeSwapchain.CreateSwapchainAndroidSurface(overlay.NativeSpecHandler, ref androidSurface);
                nrOverlay.SurfaceId = androidSurface;
                NRDebugger.Info("[SwapChain] AddLayer externalSurface name:{0} SurfaceId:{1}", overlay.gameObject.name, androidSurface);
            }
            else
            {
                overlay.SwapChainHandler = NativeSwapchain.CreateSwapchain(overlay.NativeSpecHandler);
            }
            
            bufferSpec.bufferCount = NativeSwapchain.GetRecommandBufferCount(overlay.SwapChainHandler);
#else
            bufferSpec.bufferCount = 0;
#endif

            overlay.BufferSpec = bufferSpec;
            
            NRDebugger.Info("[SwapChain] AddLayer name:{0} bufferSpec:{1} isExternalSurface:{2}", overlay.gameObject.name, bufferSpec.ToString(), isExternalSurface);

            overlay.CreateOverlayTextures();
            overlay.CreateViewport();
            UpdateProtectContentSetting();
        }

        private void RemoveLayer(OverlayBase overlay)
        {
            NRDebugger.Info("[SwapChain] RemoveLayer: name={0}", overlay.gameObject.name);
            overlay.ReleaseOverlayTextures();

#if !UNITY_EDITOR
            NativeSwapchain.DestroyBufferSpec(overlay.NativeSpecHandler);
            var viewports = overlay.ViewPorts;
            for (int i = 0; i < viewports.Length; i++)
            {
                if (viewports[i].nativeHandler != 0)
                {
                    NativeSwapchain.DestroyBufferViewPort(viewports[i].nativeHandler);
                }
            }
            NativeSwapchain.DestroySwapChain(overlay.SwapChainHandler);
#endif

            Overlays.Remove(overlay);
            UpdateOverlayViewPortIndex();
            UpdateProtectContentSetting();
        }

        private bool useProtectContent = false;
        private void UpdateProtectContentSetting()
        {
            bool flag = false;
            for (int i = 0; i < Overlays.Count; i++)
            {
                var overlay = Overlays[i];
                if (overlay is NROverlay && ((NROverlay)overlay).isProtectedContent)
                {
                    flag = true;
                    break;
                }
            }

            if (flag != useProtectContent)
            {
                NRDebugger.Info("[SwapChain] Protect content setting changed.");
                try
                {
                    AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    var unityActivity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (flag)
                    {
                        NRDebugger.Info("[SwapChain] Use protect content.");
                        ProtectedCodec.Call("start", unityActivity);
                    }
                    else
                    {
                        NRDebugger.Info("[SwapChain] Use un-protect content.");
                        ProtectedCodec.Call("stop", unityActivity);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

                useProtectContent = flag;
            }
        }

        private void PrintOverlayInfo()
        {
            System.Text.StringBuilder st = new System.Text.StringBuilder();
            for (int j = 0; j < Overlays.Count; j++)
            {
                var overlay = Overlays[j];
                if (overlay.IsActive)
                {
                    st.Append(string.Format("[Overlay] {0}\n", overlay.ToString()));
                }
            }

            NRDebugger.Info(st.ToString());
        }

        private void UpdateOverlayViewPortIndex()
        {
            int index = 0;
            for (int j = 0; j < Overlays.Count; j++)
            {
                var overlay = Overlays[j];
                if (overlay.IsActive)
                {
                    var viewports = overlay.ViewPorts;
                    Assert.IsTrue(viewports != null && viewports.Length >= 1);
                    int count = viewports.Length;
                    for (int i = 0; i < count; i++)
                    {
                        viewports[i].index = index;
                        index++;
                    }
                }
            }
            PrintOverlayInfo();
        }

        private int GetViewportsCount()
        {
            int index = 0;
            for (int j = 0; j < Overlays.Count; j++)
            {
                var overlay = Overlays[j];
                if (overlay.IsActive)
                {
                    var viewports = overlay.ViewPorts;
                    Assert.IsTrue(viewports != null && viewports.Length >= 1);
                    int count = viewports.Length;
                    for (int i = 0; i < count; i++)
                    {
                        index++;
                    }
                }
            }
            return index;
        }
        #endregion

        #region viewport
        private void PopulateBufferViewports()
        {
            for (int i = 0; i < Overlays.Count; ++i)
            {
                if (Overlays[i].IsActive)
                {
                    Overlays[i].PopulateViewPort();
                }
            }
        }

        internal void CreateBufferViewport(ref ViewPort viewport)
        {
#if !UNITY_EDITOR
            viewport.nativeHandler = NativeSwapchain.CreateBufferViewport();
#endif

            if (viewport.is3DLayer)
            {
                NativeSwapchain.Populate3DBufferViewportData(viewport.nativeHandler, viewport.swapchainHandler,
                    ref viewport.sourceUV, viewport.targetDisplay,  viewport.isExternalSurface,
                    viewport.viewportType, viewport.spaceType, ref viewport.nativePose, viewport.fov, viewport.textureArraySlice);
            }
            else
            {
                NativeSwapchain.PopulateBufferViewportData(viewport.nativeHandler, viewport.swapchainHandler, 
                    ref viewport.sourceUV, viewport.targetDisplay, 
                    viewport.viewportType, viewport.spaceType, ref viewport.nativePose, viewport.quadSize);
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[SwapChain] CreateBufferViewport:{0}", viewport.ToString());
            UpdateOverlayViewPortIndex();
        }


        /// <summary>
        /// Sync viewport setting with native.
        /// </summary>
        internal void PopulateViewportData(ref ViewPort viewport)
        {
            if (viewport.index == -1)
            {
                NRDebugger.Error("[SwapChain] PopulateViewportData index error:" + viewport.ToString());
                return;
            }

            // NRDebugger.Info("[SwapChain] PopulateViewportData:{0}", viewport.ToString());

            var targetDisplay = viewport.targetDisplay;
            if (viewport.is3DLayer)
            {
                NativeSwapchain.Populate3DBufferViewportData(viewport.nativeHandler, viewport.swapchainHandler,
                    ref viewport.sourceUV, targetDisplay, viewport.isExternalSurface,
                    viewport.viewportType, viewport.spaceType, ref viewport.nativePose, viewport.fov, viewport.textureArraySlice);

                // todo: by_yhj
                // NativeSwapchain.SetBufferViewportFocusPlane(viewport.nativeHandler, viewport.spaceType, m_FocusPointGL, m_FocusPlaneNormGL);
            }
            else
            {
                NativeSwapchain.PopulateBufferViewportData(viewport.nativeHandler, viewport.swapchainHandler, 
                    ref viewport.sourceUV, targetDisplay, 
                    viewport.viewportType, viewport.spaceType, ref viewport.nativePose, viewport.quadSize);
            }
            NativeSwapchain.SetBufferViewPort(m_FrameHandle, viewport.index, viewport.nativeHandler);
            // NRDebugger.Info("[SwapChain] PopulateViewportData:{0}", viewport.ToString());
        }

        internal void DestroyBufferViewPort(UInt64 viewportHandle)
        {
            NativeSwapchain.DestroyBufferViewPort(viewportHandle);
            UpdateOverlayViewPortIndex();
        }
        #endregion

        #region submit frame

        private List<UInt64> mDynLayers = new List<UInt64>();
        private UInt64[] mDynLayerHandlerArr = null;
        private IntPtr[] mDynLayerWorkingBufferArr = null;
        private List<UInt64> GetDynamicLayerHandlers()
        {
            mDynLayers.Clear();
            for (int i = 0; i < Overlays.Count; i++)
            {
                if (Overlays[i] is NRDisplayOverlay)
                {
                    mDynLayers.Add(Overlays[i].SwapChainHandler);
                }
                else if (Overlays[i] is NROverlay && ((NROverlay)Overlays[i]).isDynamic)
                {
                    mDynLayers.Add(Overlays[i].SwapChainHandler);
                }
            }
            return mDynLayers;
        }

        /// <summary>
        /// Populate overlays's render buffers.
        /// </summary>
        private void PopulateOverlaysRenderBuffers()
        {
            var swapchainHandlers = GetDynamicLayerHandlers();
            Assert.IsTrue((swapchainHandlers != null && swapchainHandlers.Count != 0), "SwapChain handler of dynamic layer is empty!");

            if (mDynLayerHandlerArr == null || mDynLayerHandlerArr.Length != swapchainHandlers.Count)
            {
                mDynLayerHandlerArr = swapchainHandlers.ToArray();
                mDynLayerWorkingBufferArr = new IntPtr[swapchainHandlers.Count];
            }
            else
            {
                for (int i = 0; i < swapchainHandlers.Count; i++)
                {
                    mDynLayerHandlerArr[i] = swapchainHandlers[i];
                    mDynLayerWorkingBufferArr[i] = IntPtr.Zero;
                }
            }

            NativeSwapchain.AcquireFrame(ref m_FrameHandle, mDynLayerHandlerArr, mDynLayerWorkingBufferArr);
            // NRDebugger.Info("[SwapChain] AcquireFrame: {0}", m_FrameHandle);

#if USING_XR_SDK
            NativeXRPlugin.PopulateFrameHandle(m_FrameHandle);
#endif
            for (int i = 0; i < mDynLayerWorkingBufferArr.Length; i++)
            {
                m_LayerWorkingBufferDict[swapchainHandlers[i]] = mDynLayerWorkingBufferArr[i];
                // NRDebugger.Info("[SwapChain] PopulateOverlaysRenderBuffers: swapChain={0}, workingBuffer={1}", swapchainHandlers[i], mDynLayerWorkingBufferArr[i]);
            }

            for (int i = 0; i < Overlays.Count; ++i)
            {
#if !USING_XR_SDK
                if (Overlays[i] is NRDisplayOverlay)
                    continue;
#endif
                Overlays[i].PopulateBuffers(GetWorkingBufferHandler(Overlays[i].SwapChainHandler));
            }
        }

        /// <summary>
        /// Populate frame info.
        /// </summary>
        private void PopulateFrameInfo()
        {
            if (m_FrameHandle == 0)
                return;

            NativeSwapchain.PopulateFrameInfo(m_FrameHandle, NRFrame.CurrentPoseTimeStamp);
        }

        internal IntPtr GetWorkingBufferHandler(UInt64 swapchainHandler)
        {
            IntPtr bufferid;
            if (!m_LayerWorkingBufferDict.TryGetValue(swapchainHandler, out bufferid))
            {
                return IntPtr.Zero;
            }

            return bufferid;
        }

        internal void GfxThread_SubmitFrame(UInt64 frameHandle, IntPtr idTexture)
        {
            if (!IsReady() || frameHandle == 0)
            {
                NRDebugger.Warning("[SwapChain] Can not submit frame!");
                return;
            }
    
            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Debug("[SwapChain] GfxThread_SubmitFrame IsInitialized:{0} _FrameHandle:{1} frameHandle:{2} curPresentTime:{3}", 
                    IsInitialized, m_FrameHandle, frameHandle, NRFrame.CurrentPoseTimeStamp);

            if (frameHandle != m_FrameHandle)
                NRDebugger.Error("[SwapChain] SubmitFrame frame handle not match");
            
#if USING_XR_SDK
            if (idTexture != IntPtr.Zero && m_DisplayOverlay.WorkingTexture != idTexture)
            {
                NRDebugger.Info("[SwapChain] SubmitFrame texture id not match: WorkingTexture:{0} idTexture:{1}", m_DisplayOverlay.WorkingTexture, idTexture);
            }
#endif

            NativeSwapchain.SubmitFrame(frameHandle);
        }

        internal void UpdateExternalSurface(UInt64 swapchainHandler, int transformCount, NativeTransform[] transforms, NativeDevice[] targetEyes, 
            Int64 timestamp, int frameIndex)
        {
            NativeSwapchain.UpdateExternalSurface(swapchainHandler, transformCount, transforms, targetEyes, timestamp, frameIndex);
        }
        #endregion


        /// <summary> Set a plane in camera space that acts as the focal plane of the Scene for this frame. </summary>
        /// <param name="point"> The position of the focal point in the Scene, relative to the Camera.</param>
        /// <param name="normal"> Surface normal of the plane being viewed at the focal point, relative to the Camera.</param>
        public void SetFocusPlane(Vector3 point, Vector3 normal)
        {
            m_FocusPoint = point;
            m_FocusPointGL = new Vector3(point.x, point.y, -point.z);

            m_FocusPlaneNorm = normal;
            m_FocusPlaneNormGL = new Vector3(normal.x, normal.y, -normal.z);

            m_FrameChangedType = NRFrameFlags.NR_FRAME_CHANGED_FOCUS_PLANE;
        }
        
        /// <summary> The renders coroutine. </summary>
        /// <returns> An IEnumerator. </returns>
        private IEnumerator RenderCoroutine()
        {
            WaitForEndOfFrame delay = new WaitForEndOfFrame();
            yield return delay;

            while (true)
            {
                yield return delay;

                if (!IsInitialized && NRFrame.SessionStatus == SessionState.Running && NRFrame.NRRenderer.CurrentState == NRRenderer.RendererState.Running)
                {
                    // NRDebugger.Info("[SwapChain] RenderCoroutine: k_InitSwapChainEvent");
                    GL.IssuePluginEvent(RenderThreadHandlePtr, k_InitSwapChainEvent);
                    continue;
                }

                if (!IsReady())
                    continue;

                onBeforeSubmitFrameInMainThread?.Invoke();
                if (NRDebugger.logLevel <= LogLevel.Debug)
                    NRDebugger.Debug("[SwapChain] RenderCoroutine: frameHandle={0} => {1}", m_CachFrameHandle, m_FrameHandle);

                m_CachFrameHandle = m_FrameHandle;
                GL.IssuePluginEvent(RenderThreadHandlePtr, k_SubmitFrameEvent);
            }
        }

        /// <summary> Executes the 'on render thread' operation. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            if (k_SubmitFrameEvent != eventID)
                UnityEngine.Debug.LogFormat("[SwapChain] RunOnRenderThread : eventID={0}", eventID);

            if (eventID == k_InitSwapChainEvent)
            {
                OnGfxThreadStart(NRRenderer.NativeRenderring.RenderingHandle);
            }
            else if (eventID == k_SubmitFrameEvent)
            {
                var swapChainMan = NRSessionManager.Instance.NRSwapChainMan;
                OnGfxThreadSubmit(swapChainMan.m_CachFrameHandle, IntPtr.Zero);
            }
        }

        internal void NRDebugSaveToPNG()
        {
            NRDebugger.Info("NRDebugSaveToPNG.");
#if USING_XR_SDK
            NRDebugger.Warning("NRSDK don't support to save png on XR_SDK mode currently.");
            return;
#else
            if (m_LeftDisplayOverlay != null)
            {
                m_LeftDisplayOverlay.NRDebugSaveToPNG();
            }
            if (m_RightDisplayOverlay != null)
            {
                m_RightDisplayOverlay.NRDebugSaveToPNG();
            }
#endif
        }
    }
}