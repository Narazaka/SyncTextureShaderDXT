using net.narazaka.vrchat.sync_texture;
using System.Collections.Generic;
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
        [SerializeField]
        public string[] TexturePropertyNames;
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
            GetComponent<Renderer>().sharedMaterial = !AlwaysReceivedMaterial && IsRendered ? Original : Received;
        }
    }
}
