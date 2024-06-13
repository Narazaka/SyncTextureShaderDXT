using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.narazaka.vrchat.sync_texture_shaderdxt.editor
{

    public class SyncTextureShaderDXTRendererMaterialInfo : System.IEquatable<SyncTextureShaderDXTRendererMaterialInfo>
    {
        public static SyncTextureShaderDXTRendererMaterialInfo Get(SyncTextureShaderDXTRenderer syncTextureShaderDXTRenderer) =>
            new SyncTextureShaderDXTRendererMaterialInfo(syncTextureShaderDXTRenderer.GetComponent<Renderer>().sharedMaterial, syncTextureShaderDXTRenderer.TexturePropertyNames);
        public readonly Material Material;
        public readonly string[] TexturePropertyNames;

        public SyncTextureShaderDXTRendererMaterialInfo(Material material, string[] texturePropertyNames)
        {
            Material = material;
            TexturePropertyNames = texturePropertyNames;
        }

        public override bool Equals(object obj) => Equals(obj as SyncTextureShaderDXTRendererMaterialInfo);

        public bool Equals(SyncTextureShaderDXTRendererMaterialInfo other) =>
            other != null &&
            Material == other.Material &&
            TexturePropertyNames.OrderBy(a => a).SequenceEqual(other.TexturePropertyNames.OrderBy(a => a));

        public override int GetHashCode() => System.HashCode.Combine(Material, TexturePropertyNames.OrderBy(a => a).ToArray());

        public static bool operator ==(SyncTextureShaderDXTRendererMaterialInfo left, SyncTextureShaderDXTRendererMaterialInfo right) => EqualityComparer<SyncTextureShaderDXTRendererMaterialInfo>.Default.Equals(left, right);
        public static bool operator !=(SyncTextureShaderDXTRendererMaterialInfo left, SyncTextureShaderDXTRendererMaterialInfo right) => !(left == right);

        public IEnumerable<Texture> Textures
        {
            get
            {
                if (TexturePropertyNames == null || TexturePropertyNames.Length == 0)
                {
                    yield return Material.mainTexture;
                }
                else
                {
                    foreach (var texturePropertyName in TexturePropertyNames)
                    {
                        yield return Material.GetTexture(texturePropertyName);
                    }
                }
            }
        }

        public void SetTextures(Material material, Texture texture)
        {
            if (TexturePropertyNames == null || TexturePropertyNames.Length == 0)
            {
                material.mainTexture = texture;
            }
            else
            {
                foreach (var texturePropertyName in TexturePropertyNames)
                {
                    material.SetTexture(texturePropertyName, texture);
                }
            }
        }
    }
}
