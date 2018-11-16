using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LightweightPipeline;

namespace UnityEditor.Experimental.Rendering.LightweightPipeline
{
    public static class MaterialUpdater
    {
        [MenuItem("Edit/Render Pipeline/Update project wide Lightweight Render Pipeline Materials")]
        public static void UpdateProjectMaterials()
        {
            List<MaterialUpgrader> upgraders = new List<MaterialUpgrader>();
            GetUpgraders(ref upgraders);

            MaterialUpgrader.UpgradeProjectFolder(upgraders, "Upgrade to LightweightRP Materials", MaterialUpgrader.UpgradeFlags.LogMessageWhenNoUpgraderFound);
        }
        
        [MenuItem("Edit/Render Pipeline/Update selected Lightweight Render Pipeline Materials")]
        public static void UpdateSelectedMaterials()
        {
            List<MaterialUpgrader> upgraders = new List<MaterialUpgrader>();
            GetUpgraders(ref upgraders);

            MaterialUpgrader.UpgradeSelection(upgraders, "Upgrade to LightweightRP Materials", MaterialUpgrader.UpgradeFlags.LogMessageWhenNoUpgraderFound);
        }

        private static void GetUpgraders(ref List<MaterialUpgrader> upgraders)
        {
            // Lit updater
            upgraders.Add(new LitUpdater("Lightweight Render Pipeline/Lit"));
            // Simple Lit updater
            upgraders.Add(new SimpleLitUpdater("Lightweight Render Pipeline/Simple Lit"));
            // Unlit updater
            upgraders.Add(new UnlitUpdater("Lightweight Render Pipeline/Unlit"));
            // Particle updaters
            upgraders.Add(new ParticleUpgrader("Lightweight Render Pipeline/Particles/lit"));
            upgraders.Add(new ParticleUpgrader("Lightweight Render Pipeline/Particles/Unlit"));
        }
    }

    internal class LitUpdater : MaterialUpgrader
    {
        public static void UpdateStandardMaterialKeywords(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
            
            if(material.GetTexture("_MetallicGlossMap"))
                material.SetFloat("_Smoothness", material.GetFloat("_GlossMapScale"));
            else
                material.SetFloat("_Smoothness", material.GetFloat("_Glossiness"));
        }

        public static void UpdateStandardSpecularMaterialKeywords(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
            
            if(material.GetTexture("_SpecGlossMap"))
                material.SetFloat("_Smoothness", material.GetFloat("_GlossMapScale"));
            else
                material.SetFloat("_Smoothness", material.GetFloat("_Glossiness"));
        }

        public LitUpdater(string oldShaderName)
        {
            if (oldShaderName == null)
                throw new ArgumentNullException("oldShaderName");

            string standardShaderPath = ShaderUtils.GetShaderPath(ShaderPathID.Lit);

            if (oldShaderName.Contains("Specular"))
            {
                RenameShader(oldShaderName, standardShaderPath, UpdateStandardSpecularMaterialKeywords);
            }
            else
            {
                RenameShader(oldShaderName, standardShaderPath, UpdateStandardMaterialKeywords);
            }

            RenameTexture("_MainTex", "_BaseMap");
            RenameColor("_Color", "_BaseColor");
            RenameFloat("_GlossyReflections", "_EnvironmentReflections");
        }
    }
    
    internal class UnlitUpdater : MaterialUpgrader
    {
        static Shader unlitShader = Shader.Find(ShaderUtils.GetShaderPath(ShaderPathID.Unlit));
        
        public static void UpgradeToUnlit(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            if (material.GetFloat("_SampleGI") == 0)
            {
                material.shader = unlitShader;
            }
        }

        public UnlitUpdater(string oldShaderName)
        {
            if (oldShaderName == null)
                throw new ArgumentNullException("oldShaderName");

            if (oldShaderName.Contains("Specular"))
            {
                RenameShader(oldShaderName, ShaderUtils.GetShaderPath(ShaderPathID.BakedLit), UpgradeToUnlit);
            }

            RenameTexture("_MainTex", "_BaseMap");
            RenameColor("_Color", "_BaseColor");
        }
    }

    internal class SimpleLitUpdater : MaterialUpgrader
    {
        public SimpleLitUpdater(string oldShaderName)
        {
            if (oldShaderName == null)
                throw new ArgumentNullException("oldShaderName");
            
            RenameTexture("_MainTex", "_BaseMap");
            RenameColor("_Color", "_BaseColor");
            RenameFloat("_Shininess", "_Smoothness");
            RenameFloat("_GlossinessSource", "_SmoothnessSource");
            RenameFloat("_SpecSource", "_SpecularHighlights");
        }
    }
}