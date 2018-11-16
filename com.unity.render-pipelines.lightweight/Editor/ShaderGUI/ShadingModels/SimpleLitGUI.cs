using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.LWRP.ShaderGUI
{
    public static class SimpleLitGUI
    {
        public enum SpecularSource
        {
            SpecularTextureAndColor,
            NoSpecular
        }
        
        public enum SmoothnessMapChannel
        {
            SpecularAlpha,
            AlbedoAlpha,
        }

        public static class Styles
        {
            public static GUIContent specularMapText =
                new GUIContent("Specular Map", "Specular (RGB) and Smoothness (A)");

            public static GUIContent smoothnessScaleText = new GUIContent("Smoothness", "Smoothness scale factor");

            public static GUIContent smoothnessMapChannelText =
                new GUIContent("Source", "Smoothness texture and channel");
            
            public static GUIContent highlightsText = new GUIContent("Specular Highlights", "Specular Highlights");
        }

        public struct SimpleLitProperties
        {
            // Surface Input Props
            public MaterialProperty specHighlights;
            public MaterialProperty specColor;
            public MaterialProperty specGlossMap;
            public MaterialProperty smoothnessMapChannel;
            public MaterialProperty bumpMapProp;

            public SimpleLitProperties(MaterialProperty[] properties)
            {
                // Surface Input Props
                specColor = BaseShaderGUI.FindProperty("_SpecColor", properties);
                specGlossMap = BaseShaderGUI.FindProperty("_SpecGlossMap", properties, false);
                specHighlights = BaseShaderGUI.FindProperty("_SpecularHighlights", properties, false);
                smoothnessMapChannel = BaseShaderGUI.FindProperty("_SmoothnessSource", properties, false);
                bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
            }
        }

        public static void Inputs(SimpleLitProperties properties, MaterialEditor materialEditor)
        {
            DoSpecularArea(properties, materialEditor);
            BaseShaderGUI.DoNormalArea(materialEditor, properties.bumpMapProp);
        }
        
        public static void Advanced(SimpleLitProperties properties)
        {
            SpecularSource specularSource = (SpecularSource)properties.specHighlights.floatValue;
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.Toggle(Styles.highlightsText, specularSource == SpecularSource.SpecularTextureAndColor);
            if (EditorGUI.EndChangeCheck())
                properties.specHighlights.floatValue = enabled ? (float)SpecularSource.SpecularTextureAndColor : (float)SpecularSource.NoSpecular;
        }

        public static void DoSpecularArea(SimpleLitProperties properties, MaterialEditor materialEditor)
        {
            SpecularSource specSource = (SpecularSource)properties.specHighlights.floatValue;
            EditorGUI.BeginDisabledGroup(specSource == SpecularSource.NoSpecular);

            materialEditor.TexturePropertySingleLine(Styles.specularMapText, properties.specGlossMap,
                properties.specColor);

            EditorGUI.indentLevel += 2;
            int glossinessSource = (int)properties.smoothnessMapChannel.floatValue;
            EditorGUI.BeginChangeCheck();
            glossinessSource = EditorGUILayout.Popup(Styles.smoothnessMapChannelText, glossinessSource, Enum.GetNames(typeof(SmoothnessMapChannel)));
            if (EditorGUI.EndChangeCheck())
                properties.smoothnessMapChannel.floatValue = glossinessSource;

            EditorGUI.indentLevel -= 2;
            
            EditorGUI.EndDisabledGroup();
        }

        public static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            int ch = (int) material.GetFloat("_SmoothnessTextureChannel");
            if (ch == (int) SmoothnessMapChannel.AlbedoAlpha)
                return SmoothnessMapChannel.AlbedoAlpha;

            return SmoothnessMapChannel.SpecularAlpha;
        }

        public static void SetMaterialKeywords(Material material)
        {
            UpdateMaterialSpecularSource(material);
            CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            CoreUtils.SetKeyword(material, "_RECEIVE_SHADOWS_OFF", material.GetFloat("_ReceiveShadows") == 0.0f);
        }
        
        private static void UpdateMaterialSpecularSource(Material material)
        {
            SpecularSource specSource = (SpecularSource)material.GetFloat("_SpecularHighlights");
            if (specSource == SpecularSource.NoSpecular)
            {
                CoreUtils.SetKeyword(material, "_SPECGLOSSMAP", false);
                CoreUtils.SetKeyword(material, "_SPECULAR_COLOR", false);
                CoreUtils.SetKeyword(material, "_GLOSSINESS_FROM_BASE_ALPHA", false);
            }
            else
            {
                var smoothnessSource = (SmoothnessMapChannel)material.GetFloat("_SmoothnessSource");
                bool hasMap = material.GetTexture("_SpecGlossMap");
                CoreUtils.SetKeyword(material, "_SPECGLOSSMAP", hasMap);
                CoreUtils.SetKeyword(material, "_SPECULAR_COLOR", !hasMap);
                CoreUtils.SetKeyword(material, "_GLOSSINESS_FROM_BASE_ALPHA", smoothnessSource == SmoothnessMapChannel.AlbedoAlpha);
            }
        }
    }
}
