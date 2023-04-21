/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

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
using NRKernal.Enterprise;
using System.Collections;

namespace NRKernal.Enterprise.NRExamples
{
    /// <summary> A controller for handling gray camera captures. </summary>
    [HelpURL("https://developer.nreal.ai/develop/unity/rgb-camera")]
    public class GrayCameraCaptureController : MonoBehaviour
    {
        /// <summary> The eye. </summary>
        public NativeGrayEye Eye;
        /// <summary> The capture image. </summary>
        public RawImage CaptureImage;
        /// <summary> Number of frames. </summary>
        public Text FrameCount;
        /// <summary> Gets or sets the gray camera texture. </summary>
        /// <value> The gray camera texture. </value>
        private NRGrayCameraTexture GrayCamTexture { get; set; }

        /// <summary> Starts this object. </summary>
        private void Start()
        {
            GrayCamTexture = new NRGrayCameraTexture(Eye);
            CaptureImage.texture = GrayCamTexture.GetTexture();
            GrayCamTexture.Play();

            StartCoroutine(DevicePosTrasformSample());
        }
        
        /// <summary> Sample of tranform between device pose. </summary>
        IEnumerator DevicePosTrasformSample()
        {
            while (NRFrame.SessionStatus != SessionState.Running)
            {
                yield return null;
            }
            yield return new WaitForSeconds(2.0f);

            // left gray camera In unity space & api space & openCV space
            {
                Pose lEyePos = NRFrameExtension.GrayEyePoseFromHead.LEyePose;
                NativeMat4f apiPos = NativeMat4f.identity;
                ConversionUtility.UnityPoseToApiPose(lEyePos, out apiPos);
                Matrix4x4 mat = ConversionUtility.GetTMatrix(lEyePos.position, lEyePos.rotation);
                Matrix4x4 matCV = ConversionUtility.UnityPoseToCVMatrix(lEyePos);
                Debug.LogWarningFormat("[NRCoord] device={0}: \npos={1}, rot={2}, mat=\n{3}\n==>apiPose=\n{4}\nmatCV=\n{5}", "LeftGrayCamera", lEyePos.position.ToString(),
                    lEyePos.rotation.ToString(), mat.ToString(),
                    apiPos.ToString(), matCV.ToString());
            }

            // right gray camera In unity space & api space & openCV space
            {
                Pose rEyePos = NRFrameExtension.GrayEyePoseFromHead.REyePose;
                NativeMat4f apiPos = NativeMat4f.identity;
                ConversionUtility.UnityPoseToApiPose(rEyePos, out apiPos);
                Matrix4x4 mat = ConversionUtility.GetTMatrix(rEyePos.position, rEyePos.rotation);
                Matrix4x4 matCV = ConversionUtility.UnityPoseToCVMatrix(rEyePos);
                Debug.LogWarningFormat("[NRCoord] device={0}: \npos={1}, rot={2}, mat=\n{3}\n==>apiPose=\n{4}\nmatCV=\n{5}", "RightGrayCamera", rEyePos.position.ToString(),
                    rEyePos.rotation.ToString(), mat.ToString(),
                    apiPos.ToString(), matCV.ToString());
            }

            // instrinsic&distortion info
            {
                NativeMat3f lEyeInstrMat = NRFrame.GetDeviceIntrinsicMatrix(NativeDevice.LEFT_GRAYSCALE_CAMERA);
                NativeMat3f rEyeInstrMat = NRFrame.GetDeviceIntrinsicMatrix(NativeDevice.RIGHT_GRAYSCALE_CAMERA);
                NRDistortionParams lEyeDistParam = NRFrame.GetDeviceDistortion(NativeDevice.LEFT_GRAYSCALE_CAMERA);
                NRDistortionParams rEyeDistParam = NRFrame.GetDeviceDistortion(NativeDevice.RIGHT_GRAYSCALE_CAMERA);
                Debug.LogWarningFormat("[NRCoord] device info :\n lEyeInstrMat=\n{0}\n rEyeInstrMat=\n{1}\n lEyeDistParam=\n{2}\n rEyeDistParam=\n{3}", 
                    lEyeInstrMat.ToString(), rEyeInstrMat.ToString(),
                    lEyeDistParam.ToString(), rEyeDistParam.ToString());
            }

            //  right gray camera from left gray camera In unity space & api space & openCV space
            {
                Pose lCamPos = NRFrameExtension.GrayEyePoseFromHead.LEyePose;
                Pose rCamPos = NRFrameExtension.GrayEyePoseFromHead.REyePose;
                Matrix4x4 Head_T_Lcam = Matrix4x4.TRS(lCamPos.position, lCamPos.rotation, Vector3.one);
                Matrix4x4 Head_T_Rcam = Matrix4x4.TRS(rCamPos.position, rCamPos.rotation, Vector3.one);
                Matrix4x4 Lcam_T_Rcam = Head_T_Lcam.inverse * Head_T_Rcam;
                Matrix4x4 matCV = ConversionUtility.UnityMatrixToCVMatrix(Lcam_T_Rcam);
                Debug.LogWarningFormat("[NRCoord] rCamFromLCam :\n Head_T_Lcam=\n{0}\n Head_T_Rcam=\n{1}\n Lcam_T_Rcam=\n{2}\n matCV=\n{3}", 
                    Head_T_Lcam.ToString(), Head_T_Rcam.ToString(),
                    Lcam_T_Rcam.ToString(), matCV.ToString());
            }
        }

        /// <summary> Updates this object. </summary>
        void Update()
        {
            if (GrayCamTexture != null)
            {
                FrameCount.text = GrayCamTexture.FrameCount.ToString();
            }
        }

        /// <summary> Plays this object. </summary>
        public void Play()
        {
            GrayCamTexture.Play();

            // The origin texture will be destroyed after call "Stop",
            // Rebind the texture.
            CaptureImage.texture = GrayCamTexture.GetTexture();
        }

        /// <summary> Pauses this object. </summary>
        public void Pause()
        {
            GrayCamTexture?.Pause();
        }

        /// <summary> Stops this object. </summary>
        public void Stop()
        {
            GrayCamTexture?.Stop();
        }

        /// <summary> Executes the 'destroy' action. </summary>
        void OnDestroy()
        {
            GrayCamTexture?.Stop();
        }
    }
}