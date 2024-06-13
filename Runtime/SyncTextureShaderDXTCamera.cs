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
        public SyncTexture SyncTexture;
        [SerializeField]
        public int Width = 512;
        [SerializeField]
        public int Height = 256;
        [SerializeField]
        public CustomRenderTexture[] SourceTexures;
        [SerializeField]
        public CustomRenderTexture ReceivedTexture;

        void OnEnable()
        {
            foreach (var renderer in SyncTextureShaderDXTRenderers)
            {
                renderer.Rendered();
            }
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
        }

        // editor
        // “¯‚¶render texture‚ÌSyncTextureShaderDXTRenderer‚ð‚¢‚ê‚é
    }
}
