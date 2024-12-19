using net.narazaka.vrchat.sync_texture;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture_shaderdxt
{
    public abstract class SyncTextureShaderDXTRendererBase : UdonSharpBehaviour
    {
        [SerializeField]
        public SyncTextureManager SyncTextureManager;
        [SerializeField]
        public Material Original;
        [SerializeField]
        public Material Received;
        [SerializeField]
        public string[] TexturePropertyNames;
        [SerializeField, Tooltip("for debug")]
        public bool AlwaysReceivedMaterial;

        bool IsRendered;

        void Start()
        {
            TrySetMaterial();
        }

        void OnEnable()
        {
            TrySetMaterial();
        }

        public void Rendered()
        {
            IsRendered = true;
            TrySetMaterial();
        }

        void TrySetMaterial()
        {
            if (!Networking.IsOwner(SyncTextureManager.gameObject))
            {
                SetMaterial(!AlwaysReceivedMaterial && IsRendered ? Original : Received);
            }
        }

        protected abstract void SetMaterial(Material material);
#if UNITY_EDITOR
        public abstract Material GetSerializedMaterial();
#endif
    }
}
