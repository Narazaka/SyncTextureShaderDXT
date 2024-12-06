using UnityEngine;
using UnityEditor;
using net.narazaka.vrchat.sync_texture;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using VRC.Udon;
using UdonSharpEditor;
using net.narazaka.vrchat.sync_texture.color_encoder;
using net.narazaka.vrchat.sync_texture.editor;

namespace net.narazaka.vrchat.sync_texture_shaderdxt.editor
{
    [CustomEditor(typeof(SyncTextureShaderDXTCamera))]
    public class SyncTextureShaderDXTCameraEditor : UnityEditor.Editor
    {
        SerializedProperty SyncTextureShaderDXTRenderers;
        SerializedProperty SyncTextureManager;
        SerializedProperty SyncTexture;
        SerializedProperty Width;
        SerializedProperty Height;
        SerializedProperty AltSubjectTexture;
        SerializedProperty EnableSyncWhenOnEnable;
        SerializedProperty SyncWhenOnPostRender;
        SerializedProperty MarkRenderedWhenOnEnable;
        SerializedProperty InitializationMode;
        SerializedProperty InitializationSource;
        SerializedProperty InitializationColor;
        SerializedProperty InitializationTexture;
        SerializedProperty InitializationMaterial;
        SerializedProperty SourceTexures;
        SerializedProperty ReceivedTexture;

        bool AltFoldout;
        bool FoldoutRT;
        bool FoldoutInternal;

        void OnEnable()
        {
            SyncTextureShaderDXTRenderers = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.SyncTextureShaderDXTRenderers));
            SyncTextureManager = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.SyncTextureManager));
            SyncTexture = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.SyncTexture));
            Width = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.Width));
            Height = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.Height));
            AltSubjectTexture = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.AltSubjectTexture));
#pragma warning disable CS0618 // obsolete
            EnableSyncWhenOnEnable = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.EnableSyncWhenOnEnable));
#pragma warning restore CS0618
            SyncWhenOnPostRender = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.SyncWhenOnPostRender));
            MarkRenderedWhenOnEnable = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.MarkRenderedWhenOnEnable));
            InitializationMode = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.InitializationMode));
            InitializationSource = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.InitializationSource));
            InitializationColor = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.InitializationColor));
            InitializationTexture = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.InitializationTexture));
            InitializationMaterial = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.InitializationMaterial));
            SourceTexures = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.SourceTexures));
            ReceivedTexture = serializedObject.FindProperty(nameof(SyncTextureShaderDXTCamera.ReceivedTexture));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            EditorGUILayout.HelpBox("This component has no sync features so you can use any Synchronization Method.\n[None is recommended if other components are None]", MessageType.Info);

            serializedObject.Update();

            EditorGUILayout.PropertyField(Width);
            EditorGUILayout.PropertyField(Height);
            EditorGUILayout.PropertyField(EnableSyncWhenOnEnable, new GUIContent("Sync When On Disable"));
            EditorGUILayout.PropertyField(SyncWhenOnPostRender);
            if (!EnableSyncWhenOnEnable.boolValue && !SyncWhenOnPostRender.boolValue)
            {
                EditorGUILayout.HelpBox($"{nameof(SyncTextureShaderDXTCamera.SyncWhenOnDisable)} and {nameof(SyncTextureShaderDXTCamera.SyncWhenOnPostRender)} is false, you need to call {nameof(SyncTextureShaderDXTCamera.Rendered)}() manually.", MessageType.Info);
            }
            EditorGUILayout.PropertyField(MarkRenderedWhenOnEnable);
            AltFoldout = EditorGUILayout.Foldout(AltFoldout, "Alternative texture (optional)");
            if (AltFoldout)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(AltSubjectTexture);
                    EditorGUILayout.HelpBox("If AltSubjectTexture is set, Camera.renderTexture is ignored.", MessageType.Info);
                }
            }
            var syncCamera = target as SyncTextureShaderDXTCamera;
            if (syncCamera.SubjectTexture == null)
            {
                EditorGUILayout.HelpBox("No texture exists!\nPlease set the Target Texture of the camera or set Alt Subject Texture", MessageType.Error);
            }

            if (GUILayout.Button("Prepare Objects"))
            {
                PrepareObjects();
            }

            EditorGUILayout.PropertyField(SyncTextureShaderDXTRenderers, true);
            EditorGUILayout.PropertyField(SyncTextureManager);
            EditorGUILayout.PropertyField(SyncTexture);

            FoldoutRT = EditorGUILayout.Foldout(FoldoutRT, "Custom Render Texture Options");

            using (new EditorGUI.IndentLevelScope())
            {
                if (FoldoutRT)
                {
                    EditorGUILayout.PropertyField(InitializationMode);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(InitializationSource, new GUIContent("Source"));
                        using (new EditorGUI.IndentLevelScope())
                        {
                            if (InitializationSource.enumValueIndex == (int)CustomRenderTextureInitializationSource.TextureAndColor)
                            {
                                EditorGUILayout.PropertyField(InitializationColor, new GUIContent("Color"));
                                EditorGUILayout.PropertyField(InitializationTexture, new GUIContent("Texture"));
                            }
                            else
                            {
                                EditorGUILayout.PropertyField(InitializationMaterial, new GUIContent("Material"));
                            }
                        }
                    }
                }
            }

            FoldoutInternal = EditorGUILayout.Foldout(FoldoutInternal, "internal");

            using (new EditorGUI.IndentLevelScope())
            {
                if (FoldoutInternal)
                {
                    EditorGUILayout.PropertyField(SourceTexures, true);
                    EditorGUILayout.PropertyField(ReceivedTexture);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void PrepareObjects()
        {
            var syncCameras = Object.FindObjectsByType<SyncTextureShaderDXTCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var noSubjectSyncCameras = syncCameras.Where(sc => sc.SubjectTexture == null).ToArray();
            if (noSubjectSyncCameras.Length > 0)
            {
                EditorUtility.DisplayDialog("Error", $"Invalid {nameof(SyncTextureShaderDXTCamera)} exists! Please set the Target Texture of the camera or set Alt Subject Texture.\n[{string.Join(", ", noSubjectSyncCameras.Select(syncCamera => syncCamera.name))}]", "OK");
                EditorGUIUtility.PingObject(noSubjectSyncCameras[0]);
                return;
            }
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
                var targetTexture = syncCamera.SubjectTexture;
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
                    // syncTexture.SyncEnabled = false;
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
                if (syncCamera.SyncTextureManager != syncTextureManager)
                {
                    Undo.RecordObject(syncTexture, "set SyncCamera SyncTextureManager");
                    syncCamera.SyncTextureManager = syncTextureManager;
                }

                // set data list
                var serializedObject = new SerializedObject(syncTexture);
                serializedObject.Update();
                serializedObject.FindProperty("TextureWidth").intValue = syncCamera.Width / 2;
                serializedObject.FindProperty("TextureHeight").intValue = syncCamera.Height / 2;
                SyncTextureEditor.SetDataList(serializedObject.FindProperty("DataList"), SyncTextureEditor.GetSyncTextureStat(serializedObject).ChunkCount);
                serializedObject.ApplyModifiedProperties();
            }

            foreach (var syncTexture in unusedSyncTextures)
            {
                Undo.DestroyObjectImmediate(syncTexture.gameObject);
            }
        }
    }
}
