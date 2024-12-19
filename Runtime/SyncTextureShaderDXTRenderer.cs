﻿using net.narazaka.vrchat.sync_texture;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace net.narazaka.vrchat.sync_texture_shaderdxt
{
    [RequireComponent(typeof(Renderer))]
    public class SyncTextureShaderDXTRenderer : SyncTextureShaderDXTRendererBase
    {
        protected override void SetMaterial(Material material)
        {
            GetComponent<Renderer>().sharedMaterial = material;
        }

#if UNITY_EDITOR
        public override Material GetSerializedMaterial() => GetComponent<Renderer>().sharedMaterial;
#endif
    }
}
