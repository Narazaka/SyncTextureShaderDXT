using UnityEngine;
using UnityEditor;
using net.narazaka.vrchat.sync_texture;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using VRC.Udon;
using UdonSharpEditor;
using net.narazaka.vrchat.sync_texture.color_encoder;

namespace net.narazaka.vrchat.sync_texture_shaderdxt.editor
{
    [CustomEditor(typeof(SyncTextureShaderDXTCamera))]
    public class SyncTextureShaderDXTCameraEditor : UnityEditor.Editor
    {
        SerializedProperty SyncTextureShaderDXTRenderers;
        SerializedProperty SyncTexture;
        SerializedProperty Width;
        SerializedProperty Height;
        SerializedProperty SourceTexures;
        SerializedProperty ReceivedTexture;

        bool Foldout;

        void OnEnable()
        {
            SyncTextureShaderDXTRenderers = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.SyncTextureShaderDXTRenderers));
            SyncTexture = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.SyncTexture));
            Width = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.Width));
            Height = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.Height));
            SourceTexures = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.SourceTexures));
            ReceivedTexture = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.ReceivedTexture));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.Update();

            EditorGUILayout.PropertyField(Width);
            EditorGUILayout.PropertyField(Height);

            if (GUILayout.Button("Prepare Objects"))
            {
                PrepareObjects();
            }

            EditorGUILayout.PropertyField(SyncTextureShaderDXTRenderers, true);
            EditorGUILayout.PropertyField(SyncTexture);

            Foldout = EditorGUILayout.Foldout(Foldout, "internal");

            using (new EditorGUI.IndentLevelScope())
            {
                if (Foldout)
                {
                    EditorGUILayout.PropertyField(SourceTexures, true);
                    EditorGUILayout.PropertyField(ReceivedTexture);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void PrepareObjects()
        {
            var syncTextureManager = Object.FindObjectsByType<SyncTextureManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
            if (syncTextureManager == null)
            {
                var go = new GameObject("SyncTextureManager");
                syncTextureManager = go.AddComponent<SyncTextureManager>();
                Undo.RegisterCreatedObjectUndo(go, "create SyncTextureManager");
            }
            var colorEncoder = Object.FindObjectsByType<ColorEncoderRG88>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault();
            if (colorEncoder == null)
            {
                var go = new GameObject("ColorEncoderRG88");
                colorEncoder = go.AddComponent<ColorEncoderRG88>();
                Undo.RegisterCreatedObjectUndo(go, "create ColorEncoderRG88");
            }

            var syncRenderers = Object.FindObjectsByType<SyncTextureShaderDXTRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var syncCameras = Object.FindObjectsByType<SyncTextureShaderDXTCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var syncRenderersDict = syncRenderers.GroupBy(sr => SyncTextureShaderDXTRendererMaterialInfo.Get(sr).Textures.First()).ToDictionary(g => g.Key, g => g.ToArray());
            
            var syncTextureShaderDXTRoot = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(go => go.name == "SyncTextureShaderDXTRoot");
            if (syncTextureShaderDXTRoot == null)
            {
                syncTextureShaderDXTRoot = new GameObject("SyncTextureShaderDXTRoot");
                Undo.RegisterCreatedObjectUndo(syncTextureShaderDXTRoot, "create SyncTextureShaderDXTRoot");
            }
            var syncTextures = syncTextureShaderDXTRoot.GetComponentsInChildren<SyncTexture2D8>();
            var unusedSyncTextures = new HashSet<SyncTexture>(syncTextures);

            foreach (var syncCamera in syncCameras)
            {
                var targetTexture = syncCamera.GetComponent<Camera>().targetTexture;
                if (syncRenderersDict.TryGetValue(targetTexture, out var targetRenderers))
                {
                    Undo.RecordObject(syncCamera, "set SyncTextureShaderDXTRenderers");
                    syncCamera.SyncTextureShaderDXTRenderers = targetRenderers;
                }

                var udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(syncCamera);
                var syncTexture = syncTextures.FirstOrDefault(st => st.CallbackListeners.Any(l => l == udonBehaviour));
                if (syncTexture == null)
                {
                    var go = new GameObject(syncCamera.GetInstanceID().ToString());
                    go.transform.SetParent(syncTextureShaderDXTRoot.transform);
                    syncTexture = go.AddComponent<SyncTexture2D8>();
                    syncTexture.CallbackListeners = new UdonBehaviour[] { udonBehaviour };
                    syncTexture.GetPixelsBulkCount = 0;
                    syncTexture.ColorEncoder = colorEncoder;
                    syncTexture.PrepareCallbackAsync = true;
                    Undo.RegisterCreatedObjectUndo(go, "create SyncTexture2D8 for camera");
                }
                else
                {
                    unusedSyncTextures.Remove(syncTexture);
                }
                if (syncCamera.SyncTexture != syncTexture)
                {
                    Undo.RecordObject(syncTexture, "set SyncCamera SyncTexture");
                    syncCamera.SyncTexture = syncTexture;
                }
            }

            foreach (var syncTexture in unusedSyncTextures)
            {
                Undo.DestroyObjectImmediate(syncTexture.gameObject);
            }
        }
    }
}
