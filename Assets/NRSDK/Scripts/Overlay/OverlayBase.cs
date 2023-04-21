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
    using System;
    using System.Collections.Generic;

    public class OverlayBase : MonoBehaviour, IComparable<OverlayBase>
    {
        public class IntPtrComparer : IEqualityComparer<IntPtr>
        {
            public bool Equals(IntPtr x, IntPtr y)
            {
                return x == y;
            }

            public int GetHashCode(IntPtr obj)
            {
                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// The compositionDepth defines the order of the NROverlays in composition. The overlay with smaller compositionDepth would be composited in the front of the overlay with larger compositionDepth.
        /// </summary>
        [Tooltip("The compositionDepth defines the order of the NROverlays in composition. The overlay with smaller compositionDepth would be composited in the front of the overlay with larger compositionDepth.")]
        public int compositionDepth = 0;
        [Tooltip("Whether active this overlay when script start")]
        public bool ActiveOnStart = true;
        private bool alreadyAddedToSwapChain = false;
        //use custom comparer to avoid boxing, default comparer will convert struct IntPtr to object.
        public Dictionary<IntPtr, Texture> Textures = new Dictionary<IntPtr, Texture>(new IntPtrComparer());
        protected BufferSpec m_BufferSpec;
        internal ViewPort[] m_ViewPorts;
        private bool m_IsDirty = false;
        private bool m_IsActive = true;

        internal ViewPort[] ViewPorts
        {
            get
            {
                return m_ViewPorts;
            }
        }

        public BufferSpec BufferSpec
        {
            get { return m_BufferSpec; }
            set
            {
                m_BufferSpec.Copy(value);
            }
        }

        protected UInt64 m_SwapChainHandler = 0;
        internal UInt64 SwapChainHandler
        {
            get { return m_SwapChainHandler; }
            set { m_SwapChainHandler = value; }
        }

        public bool IsActive
        {
            get { return m_IsActive; }
            private set { m_IsActive = value; }
        }

        internal UInt64 NativeSpecHandler { get; set; }

        public int CompareTo(OverlayBase that)
        {
            return this.compositionDepth.CompareTo(that.compositionDepth);
        }

        internal virtual Texture GetTexturePtr()
        {
            return null;
        }

        protected void SetDirty(bool value)
        {
            m_IsDirty = m_IsDirty || value;
        }

        void OnEnable()
        {
            IsActive = true;
        }

        void OnDisable()
        {
            IsActive = false;
        }

        protected void Start()
        {
            if (ActiveOnStart)
            {
                InitAndActive();
            }
        }

        internal void InitAndActive()
        {
            if (!alreadyAddedToSwapChain)
            {
                Initialize();
                NRSessionManager.Instance?.NRSwapChainMan?.Add(this);
                alreadyAddedToSwapChain = true;
            }
        }

        void Update()
        {
            if (m_IsDirty && m_ViewPorts != null)
            {
                DestroyViewPort();
                CreateViewport();
                m_IsDirty = false;
            }
        }

        internal virtual void Destroy()
        {
            if (alreadyAddedToSwapChain)
            {
                NRSessionManager.Instance?.NRSwapChainMan?.Remove(this);
                alreadyAddedToSwapChain = false;
            }
        }

        protected void OnDestroy()
        {
            this.Destroy();
        }

        protected virtual void Initialize() { }

        internal virtual void CreateOverlayTextures() { }

        internal virtual void ReleaseOverlayTextures() { }

        internal virtual void CreateViewport() { }

        internal virtual void PopulateViewPort() { }

        internal virtual void DestroyViewPort()
        {
            if (m_ViewPorts != null)
            {
                foreach (var viewport in m_ViewPorts)
                {
                    NRSessionManager.Instance.NRSwapChainMan.DestroyBufferViewPort(viewport.nativeHandler);
                }
                m_ViewPorts = null;
            }
        }

        /// <summary> Just for display overlay. </summary>
        internal virtual void PopulateBuffers(IntPtr bufferHandler) { }

        public override string ToString()
        {
            if (ViewPorts.Length == 1)
            {
                return string.Format("swapchainHandler:{0}, go:{1}, depth:{2}, viewIndex:{3}, BufferSpec:{4}, viewPort:{5}", m_SwapChainHandler, gameObject.name, compositionDepth, ViewPorts[0].index, m_BufferSpec.ToString(), ViewPorts[0].ToString());
            }
            else if (ViewPorts.Length == 2)
            {
                return string.Format("swapchainHandler:{0}, go:{1}, depth:{2}, viewIndex:{3}_{4}, BufferSpec:{5}, viewPort_0={6}, viewPort_1={7}", 
                    m_SwapChainHandler, gameObject.name, compositionDepth, ViewPorts[0].index, ViewPorts[1].index, m_BufferSpec.ToString(), ViewPorts[0].ToString(), ViewPorts[1].ToString());
            }
            return string.Empty;
        }
    }
}
