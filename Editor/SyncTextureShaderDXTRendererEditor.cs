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
    [CustomEditor(typeof(SyncTextureShaderDXTRenderer))]
    public class SyncTextureShaderDXTRendererEditor : Editor
    {
        SerializedProperty SyncTextureManager;
        SerializedProperty Original;
        SerializedProperty Received;
        SerializedProperty AlwaysReceivedMaterial;

        bool Foldout;

        void OnEnable()
        {
            SyncTextureManager = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRenderer.SyncTextureManager));
            Original = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRenderer.Original));
            Received = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRenderer.Received));
            AlwaysReceivedMaterial = serializedObject.FindProperty(nameof(SyncTextureShaderDXTRenderer.AlwaysReceivedMaterial));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            serializedObject.Update();

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
