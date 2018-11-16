using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.LWRP.ShaderGUI
{
    public static class LitGUI
    {
        public enum WorkflowMode
        {
            Specular = 0,
            Metallic
        }

        public enum SmoothnessMapChannel
        {
            SpecularMetallicAlpha,
            AlbedoAlpha,
        }

        public static class Styles
        {
            public static GUIContent workflowModeText = new GUIContent("Workflow Mode", "SelectWorkflow");
            public static readonly string[] workflowNames = Enum.GetNames(typeof(WorkflowMode));

            public static GUIContent specularMapText =
                new GUIContent("Specular Map", "Specular (RGB) and Smoothness (A)");

            public static GUIContent metallicMapText =
                new GUIContent("Metallic Map", "Metallic (R) and Smoothness (A)");

            public static GUIContent smoothnessText = new GUIContent("Smoothness", "Smoothness value");
            public static GUIContent smoothnessScaleText = new GUIContent("Smoothness", "Smoothness scale factor");

            public static GUIContent smoothnessMapChannelText =
                new GUIContent("Source", "Smoothness texture and channel");

            public static GUIContent highlightsText = new GUIContent("Specular Highlights", "Specular Highlights");

            public static GUIContent reflectionsText =
                new GUIContent("Environment Reflections", "Glossy Reflections");

            public static GUIContent occlusionText = new GUIContent("Occlusion Map", "Occlusion (G)");

            public static readonly string[] metallicSmoothnessChannelNames = {"Metallic Alpha", "Albedo Alpha"};
            public static readonly string[] specularSmoothnessChannelNames = {"Specular Alpha", "Albedo Alpha"};
        }

        public struct LitProperties
        {
            // Surface Option Props
            public MaterialProperty workflowMode;

            // Surface Input Props
            public MaterialProperty metallic;
            public MaterialProperty specColor;
            public MaterialProperty metallicGlossMap;
            public MaterialProperty specGlossMap;
            public MaterialProperty smoothness;
            public MaterialProperty smoothnessScale;
            public MaterialProperty smoothnessMapChannel;
            public MaterialProperty bumpMapProp;
            public MaterialProperty bumpScaleProp;
            public MaterialProperty occlusionStrength;
            public MaterialProperty occlusionMap;

            // Advanced Props
            public MaterialProperty highlights;
            public MaterialProperty reflections;

            public LitProperties(MaterialProperty[] properties)
            {
                // Surface Option Props
                workflowMode = BaseShaderGUI.FindProperty("_WorkflowMode", properties, false);
                // Surface Input Props
                metallic = BaseShaderGUI.FindProperty("_Metallic", properties);
                specColor = BaseShaderGUI.FindProperty("_SpecColor", properties, false);
                metallicGlossMap = BaseShaderGUI.FindProperty("_MetallicGlossMap", properties);
                specGlossMap = BaseShaderGUI.FindProperty("_SpecGlossMap", properties, false);
                smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties);
                smoothnessScale = BaseShaderGUI.FindProperty("_GlossMapScale", properties, false);
                smoothnessMapChannel = BaseShaderGUI.FindProperty("_SmoothnessTextureChannel", properties, false);
                bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
                bumpScaleProp = BaseShaderGUI.FindProperty("_BumpScale", properties, false);
                occlusionStrength = BaseShaderGUI.FindProperty("_OcclusionStrength", properties, false);
                occlusionMap = BaseShaderGUI.FindProperty("_OcclusionMap", properties, false);
                // Advanced Props
                highlights = BaseShaderGUI.FindProperty("_SpecularHighlights", properties, false);
                reflections = BaseShaderGUI.FindProperty("_EnvironmentReflections", properties, false);
            }
        }

        public static void Inputs(LitProperties properties, MaterialEditor materialEditor)
        {
            DoMetallicSpecularArea(properties, materialEditor);
            BaseShaderGUI.DoNormalArea(materialEditor, properties.bumpMapProp, properties.bumpScaleProp);

            if (properties.occlusionMap != null)
            {
                materialEditor.TexturePropertySingleLine(Styles.occlusionText, properties.occlusionMap,
                    properties.occlusionMap.textureValue != null ? properties.occlusionStrength : null);
            }
        }

        public static void DoMetallicSpecularArea(LitProperties properties, MaterialEditor materialEditor)
        {
            string[] metallicSpecSmoothnessChannelName;
            bool hasGlossMap = false;
            if (properties.workflowMode == null ||
                (WorkflowMode) properties.workflowMode.floatValue == WorkflowMode.Metallic)
            {
                hasGlossMap = properties.metallicGlossMap.textureValue != null;
                metallicSpecSmoothnessChannelName = Styles.metallicSmoothnessChannelNames;
                materialEditor.TexturePropertySingleLine(Styles.metallicMapText, properties.metallicGlossMap,
                    hasGlossMap ? null : properties.metallic);
            }
            else
            {
                hasGlossMap = properties.specGlossMap.textureValue != null;
                metallicSpecSmoothnessChannelName = Styles.specularSmoothnessChannelNames;
                materialEditor.TexturePropertySingleLine(Styles.specularMapText, properties.specGlossMap,
                    hasGlossMap ? null : properties.specColor);
            }

            bool showSmoothnessScale = hasGlossMap && properties.smoothnessScale != null;
            if (properties.smoothnessMapChannel != null)
            {
                int smoothnessChannel = (int) properties.smoothnessMapChannel.floatValue;
                if (smoothnessChannel == (int) SmoothnessMapChannel.AlbedoAlpha)
                    showSmoothnessScale = true;
            }

            int indentation = 2; // align with labels of texture properties
            materialEditor.ShaderProperty(showSmoothnessScale ? properties.smoothnessScale : properties.smoothness,
                showSmoothnessScale ? Styles.smoothnessScaleText : Styles.smoothnessText, indentation);

            int prevIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 3;
            if (properties.smoothnessMapChannel != null)
                BaseShaderGUI.DoPopup(Styles.smoothnessMapChannelText, properties.smoothnessMapChannel,
                    metallicSpecSmoothnessChannelName, materialEditor);
            EditorGUI.indentLevel = prevIndentLevel;
        }

        public static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            int ch = (int) material.GetFloat("_SmoothnessTextureChannel");
            if (ch == (int) SmoothnessMapChannel.AlbedoAlpha)
                return SmoothnessMapChannel.AlbedoAlpha;

            return SmoothnessMapChannel.SpecularMetallicAlpha;
        }

        public static void SetMaterialKeywords(Material material, LitProperties properties)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)


            bool hasGlossMap = false;
            bool isSpecularWorkFlow = false;
            if (material.HasProperty("_WorkflowMode"))
            {
                isSpecularWorkFlow = (WorkflowMode) material.GetFloat("_WorkflowMode") == WorkflowMode.Specular;
                if (isSpecularWorkFlow)
                    hasGlossMap = material.GetTexture("_SpecGlossMap") != null;
                else
                    hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;
            }
            else
            {
                hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;
            }

            CoreUtils.SetKeyword(material, "_SPECULAR_SETUP", isSpecularWorkFlow);

            CoreUtils.SetKeyword(material, "_METALLICSPECGLOSSMAP", hasGlossMap);

            CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));

            if (material.HasProperty("_SpecularHighlights"))
                CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF",
                    material.GetFloat("_SpecularHighlights") == 0.0f);
            if (material.HasProperty("_EnvironmentReflections"))
                CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF",
                    material.GetFloat("_EnvironmentReflections") == 0.0f);
            if (material.HasProperty("_OcclusionMap"))
                CoreUtils.SetKeyword(material, "_OCCLUSIONMAP", material.GetTexture("_OcclusionMap"));

            if (material.HasProperty("_ReceiveShadows"))
                CoreUtils.SetKeyword(material, "_RECEIVE_SHADOWS_OFF", material.GetFloat("_ReceiveShadows") == 0.0f);

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled =
                (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            if (material.HasProperty("_SmoothnessTextureChannel"))
            {
                CoreUtils.SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
                    GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha);
            }
        }
    }
}
