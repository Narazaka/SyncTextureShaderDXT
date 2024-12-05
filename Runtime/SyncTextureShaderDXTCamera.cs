using JetBrains.Annotations;
using net.narazaka.vrchat.sync_texture;
using System;
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
        public SyncTextureManager SyncTextureManager;
        [SerializeField]
        public SyncTexture2D SyncTexture;
        [SerializeField]
        public int Width = 512;
        [SerializeField]
        public int Height = 256;
        [SerializeField]
        public Texture AltSubjectTexture;
        [SerializeField, Obsolete("Use SyncWhenOnDisable")]
        public bool EnableSyncWhenOnEnable = true; // for compatibility
#pragma warning disable CS0618 // obsolete
        public bool SyncWhenOnDisable { get => EnableSyncWhenOnEnable; set => EnableSyncWhenOnEnable = value; }
#pragma warning restore CS0618
        [SerializeField]
        public bool SyncWhenOnPostRender;
        [SerializeField]
        public CustomRenderTextureUpdateMode InitializationMode = CustomRenderTextureUpdateMode.OnDemand;
        [SerializeField]
        public CustomRenderTextureInitializationSource InitializationSource = CustomRenderTextureInitializationSource.TextureAndColor;
        [SerializeField]
        public Color InitializationColor = Color.white;
        [SerializeField]
        public Texture InitializationTexture;
        [SerializeField]
        public Material InitializationMaterial;
        [SerializeField]
        public CustomRenderTexture[] SourceTexures;
        [SerializeField]
        public CustomRenderTexture ReceivedTexture;

        void OnDisable()
        {
            if (SyncWhenOnDisable)
            {
                Rendered();
            }
        }

        void OnPostRender()
        {
            if (SyncWhenOnPostRender)
            {
                Rendered();
            }
        }

        [PublicAPI]
        public void Rendered()
        {
            foreach (var renderer in SyncTextureShaderDXTRenderers)
            {
                renderer.Rendered();
            }
            SyncTexture.ReceiveEnabled = false;
            SyncTexture.Source = SourceTexures[SourceTexures.Length - 1];
            SyncTextureManager.RequestSyncTexture(SyncTexture);
        }

        public void OnPrepare()
        {
            foreach (var source in SourceTexures)
            {
                source.Update();
            }
            SyncTexture.SendCustomEventDelayedFrames(nameof(SyncTexture.OnPrepared), 2);
        }

        public void OnReceiveApplied()
        {
            ReceivedTexture.Update();
        }

#if UNITY_EDITOR
        public Texture SubjectTexture => AltSubjectTexture == null ? GetComponent<Camera>().targetTexture : AltSubjectTexture;
#endif
    }
}
