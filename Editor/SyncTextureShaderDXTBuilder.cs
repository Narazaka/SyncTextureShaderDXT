using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Linq;
using net.narazaka.vrchat.sync_texture;
using VRC.SDKBase.Network;
using UdonSharpEditor;

namespace net.narazaka.vrchat.sync_texture_shaderdxt
{
    public class SyncTextureShaderDXTBuilder : IProcessSceneWithReport
    {
        public int callbackOrder => -2048;
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var syncTextureManager = Object.FindObjectsByType<SyncTextureManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).First();
            var syncRenderers = Object.FindObjectsByType<SyncTextureShaderDXTRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var syncCameras = Object.FindObjectsByType<SyncTextureShaderDXTCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var syncRenderersDict = syncRenderers.GroupBy(sr => sr.GetComponent<Renderer>().sharedMaterial.mainTexture).ToDictionary(g => g.Key, g => g.ToArray());
            var syncTextures = Object.FindObjectsByType<SyncTexture2D8>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var syncCamera in syncCameras)
            {
                var targetTexture = syncCamera.GetComponent<Camera>().targetTexture;
                var baseName = syncCamera.GetInstanceID().ToString();

                var sizedMaterial = new Material(Shader.Find("SyncTextureShaderDXT/IdentityCRTShader"))
                {
                    name = $"{baseName}_sized",
                    mainTexture = targetTexture,
                };
                var sizedCRT = new CustomRenderTexture(syncCamera.Width, syncCamera.Height, RenderTextureFormat.RGB565, RenderTextureReadWrite.Linear)
                {
                    name = $"{baseName}_sized",
                    depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
                    material = sizedMaterial,
                    useMipMap = false,
                    initializationMode = CustomRenderTextureUpdateMode.OnDemand,
                    updateMode = CustomRenderTextureUpdateMode.OnDemand,
                };
                var compressMaterial = new Material(Shader.Find("ShaderDXT/DXT1CompressCustomRenderTexture"))
                {
                    name = $"{baseName}_compress",
                    mainTexture = sizedCRT,
                };
                var compressCRT = new CustomRenderTexture(syncCamera.Width / 2, syncCamera.Height / 2, RenderTextureFormat.RG16, RenderTextureReadWrite.Linear)
                {
                    name = $"{baseName}_compress",
                    depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
                    material = compressMaterial,
                    useMipMap = false,
                    initializationMode = CustomRenderTextureUpdateMode.OnDemand,
                    updateMode = CustomRenderTextureUpdateMode.OnDemand,
                };
                var receivedTexture = new Texture2D(syncCamera.Width / 2, syncCamera.Height / 2, TextureFormat.RGB24, false, true)
                {
                    name = $"{baseName}_received",
                };
                var decompressMaterial = new Material(Shader.Find("ShaderDXT/DXT1DecompressCustomRenderTexture"))
                {
                    name = $"{baseName}_decompress",
                    mainTexture = receivedTexture,
                };
                var decompressCRT = new CustomRenderTexture(syncCamera.Width, syncCamera.Height, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    name = $"{baseName}_decompress",
                    depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
                    material = decompressMaterial,
                    useMipMap = true,
                    autoGenerateMips = true,
                    initializationMode = CustomRenderTextureUpdateMode.OnDemand,
                    updateMode = CustomRenderTextureUpdateMode.OnDemand,
                };

                syncCamera.SourceTexures = new CustomRenderTexture[] { sizedCRT, compressCRT };
                syncCamera.ReceivedTexture = decompressCRT;

                var decompressResultMaterials = new List<Material>();
                if (syncRenderersDict.TryGetValue(targetTexture, out var targetRenderers))
                {
                    var originals = targetRenderers.Select(tr => tr.GetComponent<Renderer>().sharedMaterial).Distinct().ToArray();
                    var decompressResultMaterialMap = new Dictionary<Material, Material>();
                    foreach (var original in originals)
                    {
                        var decompressResultMaterial = new Material(original)
                        {
                            name = $"{baseName}_{original.GetInstanceID()}_decompress_result",
                            mainTexture = decompressCRT,
                        };
                        decompressResultMaterials.Add(decompressResultMaterial);
                        decompressResultMaterialMap[original] = decompressResultMaterial;
                    }
                    foreach (var targetRenderer in targetRenderers)
                    {
                        targetRenderer.SyncTextureManager = syncTextureManager;
                        targetRenderer.Original = targetRenderer.GetComponent<Renderer>().sharedMaterial;
                        targetRenderer.Received = decompressResultMaterialMap[targetRenderer.Original];
                    }
                }

                var udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(syncCamera);
                var syncTexture = syncTextures.FirstOrDefault(st => st.CallbackListeners.Any(l => l == udonBehaviour));
                syncTexture.Source = compressCRT;
                syncTexture.Target = receivedTexture;

                var basePath = UdonSharp.Updater.UdonSharpLocator.IntermediatePrefabPath;
                if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);
                var obj = ScriptableObject.CreateInstance<Generated>();
                AssetDatabase.CreateAsset(obj, System.IO.Path.Join(basePath, $"{baseName}.asset"));
                AddObjectToAsset(sizedMaterial, obj);
                AddObjectToAsset(sizedCRT, obj);
                AddObjectToAsset(compressMaterial, obj);
                AddObjectToAsset(compressCRT, obj);
                AddObjectToAsset(receivedTexture, obj);
                AddObjectToAsset(decompressMaterial, obj);
                AddObjectToAsset(decompressCRT, obj);
                foreach (var decompressResultMaterial in decompressResultMaterials)
                {
                    AddObjectToAsset(decompressResultMaterial, obj);
                }
                AssetDatabase.SaveAssets();
            }
        }

        void AddObjectToAsset(Object obj, Object asset)
        {
            obj.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(obj, asset);
        }
    }
}
