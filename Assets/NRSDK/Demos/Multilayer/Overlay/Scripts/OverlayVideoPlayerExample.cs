using System;
using UnityEngine;

namespace NRKernal.NRExamples
{
    public class OverlayVideoPlayerExample : MonoBehaviour
    {
        [SerializeField]
        private NROverlay videoOverlayPrefab;
        private AndroidVideoPlayer m_VideoPlayer;

        private NROverlay m_Overlay;
        public bool useDRMSource = false;

        public string videoUri_drm = "https://storage.googleapis.com/wvmedia/cenc/h264/tears/tears.mpd";
        public string videoUri_normal = "https://storage.googleapis.com/exoplayer-test-media-1/mp4/android-screens-10s.mp4";

        void Start()
        {
            if (useDRMSource)
            {
                LoadDrmItem();
            }
            else
            {
                LoadNormalItem();
            }
        }

        private void InitVideoSurface()
        {
            NRDebugger.Info("[OverlayVideoPlayerExample] OnLayerCreated: surfaceId={0}.", m_Overlay.SurfaceId);
            var surfaceJo = m_Overlay.SurfaceId;
            if (surfaceJo == IntPtr.Zero)
            {
                NRDebugger.Error("[OverlayVideoPlayerExample] InitVideoSurface faild...");
                return;
            }

            if (m_VideoPlayer == null)
            {
                m_VideoPlayer = new AndroidVideoPlayer();
                // Playing video from internet needs the permission of "android.permission.INTERNET",
                // Add it to your "AndroidManifest.xml" file in "Assets/Plugin".
                string url = useDRMSource ? videoUri_drm : videoUri_normal;
                m_VideoPlayer.InitWithMediaPlayer(surfaceJo, url, useDRMSource);
            }
        }

        public void LoadDrmItem() => LoadVideoItem(true);

        public void LoadNormalItem() => LoadVideoItem(false);

        private void LoadVideoItem(bool usedrm)
        {
            if (m_Overlay != null)
            {
                m_VideoPlayer?.Release();
                m_VideoPlayer = null;
                GameObject.Destroy(m_Overlay.gameObject);
                NRDebugger.Info("[OverlayVideoPlayerExample] OnLayerDestroied.");
                m_Overlay.Destroy();
            }

            m_Overlay = GameObject.Instantiate(videoOverlayPrefab.gameObject, transform).GetComponent<NROverlay>();
            m_Overlay.gameObject.name = "Overlay-" + (usedrm ? "drm" : "normal");
            m_Overlay.isProtectedContent = usedrm;
            m_Overlay.externalSurfaceObjectCreated += InitVideoSurface;
            m_Overlay.gameObject.SetActive(true);
            useDRMSource = usedrm;
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                m_VideoPlayer?.Pause();
            }
            else
            {
                m_VideoPlayer?.Play();
            }
        }

        void OnDestroy()
        {
            if (m_VideoPlayer != null)
            {
                m_VideoPlayer?.Pause();
                m_VideoPlayer?.Release();
                m_VideoPlayer = null;
                GameObject.Destroy(m_Overlay.gameObject);
            }
        }
    }
}
