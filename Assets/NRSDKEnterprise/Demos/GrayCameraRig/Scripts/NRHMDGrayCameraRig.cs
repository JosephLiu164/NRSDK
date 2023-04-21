/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal.Enterprise.NRExamples
{
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary> A nrhmd gray camera rig. </summary>
    public class NRHMDGrayCameraRig : MonoBehaviour
    {
        /// <summary> The left camera anchor. </summary>
        [SerializeField]
        private Transform m_LeftCameraAnchor;

        /// <summary> The right camera anchor. </summary>
        [SerializeField]
        private Transform m_RightCameraAnchor;

        /// <summary> True if inited. </summary>
        private bool m_Inited;

        /// <summary> Gets camera anchor. </summary>
        /// <param name="nativeGrayEye"> The native gray eye.</param>
        /// <returns> The camera anchor. </returns>
        public Transform GetCameraAnchor(NativeDevice nativeGrayEye)
        {
            Assert.IsTrue(nativeGrayEye == NativeDevice.LEFT_GRAYSCALE_CAMERA || nativeGrayEye == NativeDevice.RIGHT_GRAYSCALE_CAMERA);
            return nativeGrayEye == NativeDevice.RIGHT_GRAYSCALE_CAMERA ? m_RightCameraAnchor : m_LeftCameraAnchor;
        }

        /// <summary> Updates this object. </summary>
        private void Update()
        {
            if (m_Inited)
            {
                return;
            }
            if (NRFrame.SessionStatus == SessionState.Running)
            {
                Init();
            }
        }

        /// <summary> Initializes this object. </summary>
        private void Init()
        {
            SetGrayCameraAnchorsPose(NativeDevice.LEFT_GRAYSCALE_CAMERA);
            SetGrayCameraAnchorsPose(NativeDevice.RIGHT_GRAYSCALE_CAMERA);
            m_Inited = true;
        }

        /// <summary> Sets gray camera anchors pose. </summary>
        /// <param name="nativeGrayEye"> The native gray eye.</param>
        private void SetGrayCameraAnchorsPose(NativeDevice nativeGrayEye)
        {
            var pose = NRFrame.GetDevicePoseFromHead(nativeGrayEye);
            var cameraAnchor = GetCameraAnchor(nativeGrayEye);
            cameraAnchor.localRotation = pose.rotation;
            cameraAnchor.localPosition = pose.position;
        }
    }
}
