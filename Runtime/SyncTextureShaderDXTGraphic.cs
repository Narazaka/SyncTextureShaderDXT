using net.narazaka.vrchat.sync_texture;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture_shaderdxt
{
    [RequireComponent(typeof(Graphic))]
    public class SyncTextureShaderDXTGraphic : SyncTextureShaderDXTRendererBase
    {
        protected override void SetMaterial(Material material)
        {
            GetComponent<Graphic>().material = material;
        }

#if UNITY_EDITOR
        public override Material GetSerializedMaterial() => GetComponent<Graphic>().material;
#endif
    }
}
