using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.LWRP.ShaderGUI
{
    public static class BakedLitGUI
    {
        public static class Styles
        {
            public static GUIContent sampleGILabel = new GUIContent("Global Illumination",
                "If enabled Global Illumination will be sampled from Ambient lighting, Lightprobes or Lightmap.");

        }

        public struct BakedLitProperties
        {
            // Surface Input Props
            public MaterialProperty sampleGIProp;
            public MaterialProperty bumpMapProp;

            public BakedLitProperties(MaterialProperty[] properties)
            {
                // Surface Input Props
                sampleGIProp = BaseShaderGUI.FindProperty("_SampleGI", properties, false);
                bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
            }
        }

        public static void Inputs(BakedLitProperties properties, MaterialEditor materialEditor)
        {
            if (properties.sampleGIProp != null)
            {
                EditorGUI.BeginDisabledGroup(properties.sampleGIProp.floatValue < 0.5f);
                BaseShaderGUI.DoNormalArea(materialEditor, properties.bumpMapProp);
                EditorGUI.EndDisabledGroup();
            }
        }
        
        public static void Advanced(BakedLitProperties properties)
        {
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.Toggle(Styles.sampleGILabel, properties.sampleGIProp.floatValue > 0);
            if (EditorGUI.EndChangeCheck())
                properties.sampleGIProp.floatValue = enabled ? 1f : 0f;
        }

        public static void SetMaterialKeywords(Material material)
        {
            if (material.HasProperty("_SampleGI"))
            {
                bool sampleGI = material.GetFloat("_SampleGI") > 0.5f;
                bool normalMap = material.GetTexture("_BumpMap");

                CoreUtils.SetKeyword(material, "_SAMPLE_GI", sampleGI && !normalMap);
                CoreUtils.SetKeyword(material, "_NORMALMAP", sampleGI && normalMap);
            }
        }
    }
}
