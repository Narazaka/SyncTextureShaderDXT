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
    [CustomEditor(typeof(SyncTextureShaderDXTRendererBase), true)]
    public class SyncTextureShaderDXTRendererEditor : Editor
    {
        SerializedProperty SyncTextureManager;
        SerializedProperty Original;
        SerializedProperty Received;
        SerializedProperty TexturePropertyNames;
        SerializedProperty AlwaysReceivedMaterial;

        bool Foldout;

        void OnEnable()
        {
            SyncTextureManager = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRendererBase.SyncTextureManager));
            Original = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRendererBase.Original));
            Received = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRendererBase.Received));
            TexturePropertyNames = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRendererBase.TexturePropertyNames));
            AlwaysReceivedMaterial = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRendererBase.AlwaysReceivedMaterial));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            EditorGUILayout.HelpBox("This component has no sync features so you can use any Synchronization Method.\n[None is recommended if other components are None]", MessageType.Info);

            serializedObject.Update();

            EditorGUILayout.PropertyField(TexturePropertyNames, new GUIContent(TexturePropertyNames.displayName + " (optional)"), true);
            EditorGUILayout.HelpBox("To set a texture other than MainTexture, specify TexturePropertyNames.", MessageType.Info);
            EditorGUILayout.PropertyField(AlwaysReceivedMaterial, new GUIContent(AlwaysReceivedMaterial.displayName + " (for debug)"));

            Foldout = EditorGUILayout.Foldout(Foldout, "internal");

            using (new EditorGUI.IndentLevelScope())
            {
                if (Foldout)
                {
                    EditorGUILayout.PropertyField(SyncTextureManager);
                    EditorGUILayout.PropertyField(Original);
                    EditorGUILayout.PropertyField(Received);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
