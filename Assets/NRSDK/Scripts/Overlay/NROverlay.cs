using System.Linq;
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
    using UnityEngine;
    using UnityEngine.Assertions;

    [ExecuteInEditMode]
    public class NROverlay : OverlayBase
    {
        /// <summary>
        /// If true, the layer will be created as an external surface. externalSurfaceObject contains the Surface object. It's effective only on Android.
        /// </summary>
        [Tooltip("If true, the layer will be created as an external surface. externalSurfaceObject contains the Surface object. It's effective only on Android.")]
        public bool isExternalSurface = false;

        /// <summary>
        /// The width which will be used to create the external surface. It's effective only on Android.
        /// </summary>
        [Tooltip("The width which will be used to create the external surface. It's effective only on Android.")]
        public int externalSurfaceWidth = 0;

        /// <summary>
        /// The height which will be used to create the external surface. It's effective only on Android.
        /// </summary>
        [Tooltip("The height which will be used to create the external surface. It's effective only on Android.")]
        public int externalSurfaceHeight = 0;

        /// <summary>
        /// If true, the texture's content is copied to the compositor each frame.
        /// </summary>
        [Tooltip("If true, the texture's content is copied to the compositor each frame.")]
        public bool isDynamic = false;

        /// <summary>
        /// If true, the layer would be used to present protected content. The flag is effective only on Android.
        /// </summary>
        [Tooltip("If true, the layer would be used to present protected content. The flag is effective only on Android.")]
        public bool isProtectedContent = false;

        /// <summary>
        /// The Texture to show in the layer.
        /// </summary>
        [Tooltip("The Texture to show in the layer.")]
        public Texture texture;

        /// <summary>
        /// Which display this overlay should render to.
        /// </summary>
        [Tooltip("Which display this overlay should render to.")]
        public LayerSide layerSide = LayerSide.Both;

        /// <summary>
        /// Whether render this overlay in screen space. It meens a 0-dof layer for 2d layer, and meens no warping for 3d layer.
        /// </summary>
        [Tooltip("Whether render this overlay in screen space.")]
        public bool isScreenSpace = false;

        /// <summary>
        /// Whether this overlay is 3D rendering layer.
        /// </summary>
        [Tooltip("Whether this overlay is 3D rendering layer.")]
        public bool is3DLayer = false;

        /// <summary>
        /// Preview the overlay in the editor using a mesh renderer.
        /// </summary>
        [Tooltip("Preview the overlay in the editor using a mesh renderer.")]
        public bool previewInEditor;

        private bool userTextureMask = false;

        protected IntPtr m_SurfaceId = IntPtr.Zero;
        public IntPtr SurfaceId
        {
            get { return isExternalSurface ? m_SurfaceId : IntPtr.Zero; }
            set { m_SurfaceId = value; }
        }
        public delegate void ExternalSurfaceObjectCreated();
        public delegate void BufferedSurfaceObjectChangeded(RenderTexture rt);
        public event ExternalSurfaceObjectCreated externalSurfaceObjectCreated;
        /// <summary> Only for RenderTexture imageType.</summary>
        public event BufferedSurfaceObjectChangeded onBufferChanged;

        public Texture MainTexture
        {
            get
            {
                return texture;
            }
            set
            {
                if (texture != value)
                {
                    SetDirty(true);
                    userTextureMask = true;
                    texture = value;
                }
            }
        }

        /// <summary> Determines the on-screen appearance of a layer. </summary>
        public enum OverlayShape
        {
            Quad,
            //Cylinder,
            //Cubemap,
            //OffcenterCubemap,
            //Equirect,
        }

        private NROverlayMeshGenerator m_MeshGenerator;
        public OverlayShape overlayShape { get; set; } = OverlayShape.Quad;
        private Matrix4x4 m_OriginPose;

        public Rect[] sourceUVRect = new Rect[2] {
                new Rect(0, 0, 1, 1),
                new Rect(0, 0, 1, 1)
        };

        new void Start()
        {
            base.Start();
            ApplyMeshAndMat();
        }

        private void ApplyMeshAndMat()
        {
            m_MeshGenerator = gameObject.GetComponentInChildren<NROverlayMeshGenerator>();
            if (m_MeshGenerator == null)
            {
                var go = new GameObject("NROverlayMeshGenerator");
                // go.hideFlags = HideFlags.HideInHierarchy;
                go.transform.SetParent(this.transform, false);
                m_MeshGenerator = go.AddComponent<NROverlayMeshGenerator>();
            }
            m_MeshGenerator.SetOverlay(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

            m_BufferSpec = new BufferSpec();
            if (isExternalSurface)
            {
                m_BufferSpec.size = new NativeResolution(externalSurfaceWidth, externalSurfaceHeight);
            }
            else if (texture != null)
            {
                m_BufferSpec.size = new NativeResolution(texture.width, texture.height);
            }

            m_BufferSpec.colorFormat = NRTextureFormat.NR_TEXTURE_FORMAT_COLOR_RGBA8;
            m_BufferSpec.depthFormat = NRTextureFormat.NR_TEXTURE_FORMAT_DEPTH_24;
            m_BufferSpec.samples = 1;
            int surfaceFlag = 0;
            if (isExternalSurface)
            {
                if (is3DLayer)
                {
                    surfaceFlag |= (int)NRExternalSurfaceFlags.NR_EXTERNAL_SURFACE_FLAG_SYNCHRONOUS;
                    surfaceFlag |= (int)NRExternalSurfaceFlags.NR_EXTERNAL_SURFACE_FLAG_USE_TIMESTAMPS;
                }
            }
            m_BufferSpec.surfaceFlag = surfaceFlag;

            UInt64 createFlag = 0;
            if (isProtectedContent)
                createFlag |= (UInt64)NRSwapchainCreateFlags.NR_SWAPCHAIN_CREATE_FLAGS_PROTECT_TEXTURE;
            if (!isDynamic)
                createFlag |= (UInt64)NRSwapchainCreateFlags.NR_SWAPCHAIN_CREATE_FLAGS_STATIC_TEXTURE;

            m_BufferSpec.createFlag = createFlag;

            m_OriginPose = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        }

        internal override void CreateOverlayTextures()
        {
            NRDebugger.Info("[NROverlay] CreateOverlayTextures:  isDynamic={0}, bufferCnt={1}", isDynamic, m_BufferSpec.bufferCount);
            ReleaseOverlayTextures();

            IntPtr texturePtr = IntPtr.Zero;

            if (isExternalSurface)
            {
                externalSurfaceObjectCreated?.Invoke();
            }
            else if (isDynamic)
            {
                if (texture == null)
                {
                    NRDebugger.Warning("Current texture is empty!!!");
                    return;
                }
                for (int i = 0; i < m_BufferSpec.bufferCount; ++i)
                {
                    RenderTexture rt = UnityExtendedUtility.CreateRenderTexture(
                        texture.width, texture.height, 24, RenderTextureFormat.ARGB32
                        );
                    texturePtr = rt.GetNativeTexturePtr();
                    Textures.Add(texturePtr, rt);
                }
            }
            else if (texture)
            {
                texturePtr = texture.GetNativeTexturePtr();
                Textures.Add(texturePtr, texture);
            }

            if (Textures.Count > 0)
                NRSessionManager.Instance.NRSwapChainMan.NativeSwapchain.SetSwapChainBuffers(m_SwapChainHandler, Textures.Keys.ToArray());
        }

        internal override void ReleaseOverlayTextures()
        {
            if (Textures.Count == 0)
            {
                return;
            }
            if (isDynamic)
            {
                foreach (var item in Textures)
                {
                    RenderTexture rt = item.Value as RenderTexture;
                    if (rt != null)
                    {
                        rt.Release();
                    }
                    else if (item.Value != null)
                    {
                        GameObject.Destroy(item.Value);
                    }
                }
            }
            Textures.Clear();
        }

        internal override void CreateViewport()
        {
            base.CreateViewport();

            if (layerSide != LayerSide.Both)
            {
                var targetDisplay = layerSide == LayerSide.Left ? NativeDevice.LEFT_DISPLAY : NativeDevice.RIGHT_DISPLAY;

                m_ViewPorts = new ViewPort[1];
                m_ViewPorts[0].viewportType = is3DLayer ? NRViewportType.NR_VIEWPORT_PROJECTION : NRViewportType.NR_VIEWPORT_QUAD;
                m_ViewPorts[0].sourceUV = new NativeRectf(sourceUVRect[0]);
                m_ViewPorts[0].targetDisplay = targetDisplay;
                m_ViewPorts[0].swapchainHandler = m_SwapChainHandler;
                m_ViewPorts[0].is3DLayer = is3DLayer;
                m_ViewPorts[0].isExternalSurface = isExternalSurface;
                m_ViewPorts[0].textureArraySlice = -1;
                m_ViewPorts[0].spaceType = isScreenSpace ? NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW : NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
                m_ViewPorts[0].nativePose = CalculatePose(targetDisplay);
                m_ViewPorts[0].quadSize = new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
                if (is3DLayer)
                {
                    NRFrame.GetEyeFov(targetDisplay, ref m_ViewPorts[0].fov);
                }
                NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[0]);
            }
            else
            {
                m_ViewPorts = new ViewPort[2];
                m_ViewPorts[0].viewportType = is3DLayer ? NRViewportType.NR_VIEWPORT_PROJECTION : NRViewportType.NR_VIEWPORT_QUAD;
                m_ViewPorts[0].sourceUV = new NativeRectf(sourceUVRect[0]);
                m_ViewPorts[0].targetDisplay = NativeDevice.LEFT_DISPLAY;
                m_ViewPorts[0].swapchainHandler = m_SwapChainHandler;
                m_ViewPorts[0].is3DLayer = is3DLayer;
                m_ViewPorts[0].isExternalSurface = isExternalSurface;
                m_ViewPorts[0].index = -1;
                m_ViewPorts[0].textureArraySlice = -1;
                m_ViewPorts[0].spaceType = isScreenSpace ? NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW : NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
                m_ViewPorts[0].nativePose = CalculatePose(NativeDevice.LEFT_DISPLAY);
                m_ViewPorts[0].quadSize = new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
                if (is3DLayer)
                {
                    NRFrame.GetEyeFov(NativeDevice.LEFT_DISPLAY, ref m_ViewPorts[0].fov);
                }
                NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[0]);

                m_ViewPorts[1].viewportType = is3DLayer ? NRViewportType.NR_VIEWPORT_PROJECTION : NRViewportType.NR_VIEWPORT_QUAD;
                m_ViewPorts[1].sourceUV = new NativeRectf(sourceUVRect[1]);
                m_ViewPorts[1].targetDisplay = NativeDevice.RIGHT_DISPLAY;
                m_ViewPorts[1].swapchainHandler = m_SwapChainHandler;
                m_ViewPorts[1].is3DLayer = is3DLayer;
                m_ViewPorts[1].isExternalSurface = isExternalSurface;
                m_ViewPorts[1].index = -1;
                m_ViewPorts[1].textureArraySlice = -1;
                m_ViewPorts[1].spaceType = isScreenSpace ? NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW : NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
                m_ViewPorts[1].nativePose = CalculatePose(NativeDevice.RIGHT_DISPLAY);
                m_ViewPorts[1].quadSize = new Vector2(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
                if (is3DLayer)
                {
                    NRFrame.GetEyeFov(NativeDevice.RIGHT_DISPLAY, ref m_ViewPorts[1].fov);
                }
                NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[1]);
            }

            if (is3DLayer)
            {
                ClearMask();
            }
        }

        /// <summary>
        /// Update transform of viewPort.
        /// </summary>
        internal override void PopulateViewPort()
        {
            base.PopulateViewPort();

            if (m_ViewPorts == null)
            {
                NRDebugger.Warning("Can not update view port for this layer:{0}", gameObject.name);
                return;
            }

            if (layerSide != LayerSide.Both)
            {
                var targetDisplay = layerSide == LayerSide.Left ? NativeDevice.LEFT_DISPLAY : NativeDevice.RIGHT_DISPLAY;
                m_ViewPorts[0].nativePose = CalculatePose(targetDisplay);

                NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[0]);
            }
            else
            {
                m_ViewPorts[0].nativePose = CalculatePose(NativeDevice.LEFT_DISPLAY);
                m_ViewPorts[1].nativePose = CalculatePose(NativeDevice.RIGHT_DISPLAY);

                NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[0]);
                NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[1]);
            }
        }

        public void Apply()
        {
            SetDirty(true);
        }

        internal override void PopulateBuffers(IntPtr bufferHandler)
        {
            if (isDynamic)
            {
                Texture targetTexture;
                if (!Textures.TryGetValue(bufferHandler, out targetTexture))
                {
                    NRDebugger.Error("Can not find the texture:" + bufferHandler);
                    return;
                }
                onBufferChanged?.Invoke((RenderTexture)targetTexture);
            }
        }

        public void UpdateTransform(Matrix4x4 originMatrix, bool isScreenSpace)
        {
            m_OriginPose = originMatrix;
            this.isScreenSpace = isScreenSpace;
        }

        // calculate model matrix for 2d layer
        private NativeTransform CalculatePose(NativeDevice targetDisplay)
        {
            Assert.IsTrue(targetDisplay == NativeDevice.LEFT_DISPLAY || targetDisplay == NativeDevice.RIGHT_DISPLAY);

            if (is3DLayer)
            {
                return new NativeTransform();
            }
            else
            {
                // head-locked 2d layer
                if (isScreenSpace)
                {
                    return ConversionUtility.UnityMatrixToApiPose(m_OriginPose);
                }
                else // world-locked 2d layer
                {
                    Matrix4x4 modelMatrix = transform.localToWorldMatrix;

                    // take care of world offset as part of model matrix.
                    Matrix4x4 worldOffseMatrix = NRFrame.GetWorldMatrixFromUnityToNative();
                    if (!worldOffseMatrix.isIdentity)
                        modelMatrix = worldOffseMatrix.inverse * modelMatrix;
                        
                    return ConversionUtility.UnityMatrixToApiPose(modelMatrix);
                }
            }
        }

        public void UpdateExternalSurface(int transformCount, Pose[] poses, NativeDevice[] targetEyes, Int64 timestamp, int frameIndex)
        {
            var transforms = new NativeTransform[transformCount];
            for (int i = 0; i < transformCount; i++)
            {
                var mat = ConversionUtility.GetTMatrix(poses[i]);
                transforms[i] = ConversionUtility.UnityMatrixToApiPose(mat);
            }
            NRSessionManager.Instance.NRSwapChainMan.UpdateExternalSurface(m_SwapChainHandler, transformCount, transforms, targetEyes, timestamp, frameIndex);
        }

        private void ClearMask()
        {
            if (m_MeshGenerator != null)
            {
                GameObject.DestroyImmediate(m_MeshGenerator.gameObject);
                m_MeshGenerator = null;
            }
        }
        internal override void Destroy()
        {
            base.Destroy();
            if (userTextureMask)
            {
                texture = null;
                userTextureMask = false;
            }
        }

        new void OnDestroy()
        {
            base.OnDestroy();
            ClearMask();
        }
    }
}