using net.narazaka.vrchat.sync_texture;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture_shaderdxt
{
    [RequireComponent(typeof(Camera))]
    public class SyncTextureShaderDXTCamera : UdonSharpBehaviour
    {
        [SerializeField]
        public SyncTextureShaderDXTRenderer[] SyncTextureShaderDXTRenderers;
        [SerializeField]
        public SyncTexture2D SyncTexture;
        [SerializeField]
        public int Width = 512;
        [SerializeField]
        public int Height = 256;
        [SerializeField]
        public Texture AltSubjectTexture;
        [SerializeField]
        public bool EnableSyncWhenOnEnable = true;
        [SerializeField]
        public CustomRenderTexture[] SourceTexures;
        [SerializeField]
        public CustomRenderTexture ReceivedTexture;

        void OnEnable()
        {
            if (EnableSyncWhenOnEnable)
            {
                EnableSync();
            }
        }

        public void EnableSync()
        {
            foreach (var renderer in SyncTextureShaderDXTRenderers)
            {
                renderer.Rendered();
            }
            SyncTexture.SyncEnabled = true;
            SyncTexture.Source = SourceTexures[SourceTexures.Length - 1];
        }

        public void OnPrepare()
        {
            foreach (var source in SourceTexures)
            {
                source.Update();
            }
            SyncTexture.SendCustomEventDelayedFrames(nameof(SyncTexture.OnPrepared), 2);
        }

        public void OnReceive()
        {
            ReceivedTexture.Update();
            SyncTexture.SyncEnabled = true;
        }

#if UNITY_EDITOR
        public Texture SubjectTexture => AltSubjectTexture == null ? GetComponent<Camera>().targetTexture : AltSubjectTexture;
#endif
    }
}
