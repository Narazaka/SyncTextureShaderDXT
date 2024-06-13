using net.narazaka.vrchat.sync_texture;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture_shaderdxt
{
    [RequireComponent(typeof(Renderer))]
    public class SyncTextureShaderDXTRenderer : UdonSharpBehaviour
    {
        [SerializeField]
        public SyncTextureManager SyncTextureManager;
        [SerializeField]
        public Material Original;
        [SerializeField]
        public Material Received;
        [SerializeField, Tooltip("for debug")]
        public bool AlwaysReceivedMaterial;

        bool IsRendered;

        void Start()
        {
            SetMaterial();
        }

        void OnEnable()
        {
            SetMaterial();
        }

        public void Rendered()
        {
            IsRendered = true;
            SetMaterial();
        }

        void SetMaterial()
        {
            if (!Networking.IsOwner(SyncTextureManager.gameObject))
            {
                GetComponent<Renderer>().sharedMaterial = !AlwaysReceivedMaterial && IsRendered ? Original : Received;
            }
        }

        // editor
        // 同じrender textureでまとめてcustom render textureつくる

    }
}
