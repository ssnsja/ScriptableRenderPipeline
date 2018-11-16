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

            MaterialUpgrader.UpgradeProjectFolder(upgraders, "Update to LightweightRP Materials", MaterialUpgrader.UpgradeFlags.LogMessageWhenNoUpgraderFound);
        }
        
        [MenuItem("Edit/Render Pipeline/Update selected Lightweight Render Pipeline Materials")]
        public static void UpdateSelectedMaterials()
        {
            List<MaterialUpgrader> upgraders = new List<MaterialUpgrader>();
            GetUpgraders(ref upgraders);

            MaterialUpgrader.UpgradeSelection(upgraders, "Update LightweightRP Materials", MaterialUpgrader.UpgradeFlags.LogMessageWhenNoUpgraderFound);
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
            
            material.SetColor("_BaseColor", material.GetColor("_Color"));

            SetQueue(material);
        }

        public static void UpdateStandardSpecularMaterialKeywords(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
            
            if(material.GetTexture("_SpecGlossMap"))
                material.SetFloat("_Smoothness", material.GetFloat("_GlossMapScale"));
            else
                material.SetFloat("_Smoothness", material.GetFloat("_Glossiness"));
            
            material.SetColor("_BaseColor", material.GetColor("_Color"));

            SetQueue(material);
        }

        public static void SetQueue(Material material)
        {
            if (material.GetFloat("_Surface") == 0)
            {
                if (material.GetFloat("_AlphaClip") == 0)
                    material.renderQueue = 2000;
                else
                    material.renderQueue = 2450;
            }
            else
            {
                material.renderQueue = 3000;
            }
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
        static Shader bakedLit = Shader.Find(ShaderUtils.GetShaderPath(ShaderPathID.BakedLit));
        
        public static void UpgradeToUnlit(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            if (material.GetFloat("_SampleGI") != 0)
            {
                material.shader = bakedLit;
                material.EnableKeyword("_NORMALMAP");
            }
        }

        public UnlitUpdater(string oldShaderName)
        {
            if (oldShaderName == null)
                throw new ArgumentNullException("oldShaderName");

            RenameShader(oldShaderName, ShaderUtils.GetShaderPath(ShaderPathID.Unlit), UpgradeToUnlit);

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
            
            RenameShader(oldShaderName, ShaderUtils.GetShaderPath(ShaderPathID.SimpleLit), UpgradeToSimpleLit);
            
            RenameTexture("_MainTex", "_BaseMap");
            RenameFloat("_SpecSource", "_SpecularHighlights");
        }
        
        public static void UpgradeToSimpleLit(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            material.SetFloat("_SmoothnessSource" ,1 - material.GetFloat("_GlossinessSource"));
            if (material.GetTexture("_SpecGlossMap") == null)
            {
                var col = material.GetColor("_SpecColor");
                
                col.a = material.GetFloat("_Shininess");
                material.SetColor("_SpecColor", col);
                if (material.GetFloat("_Surface") == 0)
                {
                    var colBase = material.GetColor("_Color");
                    colBase.a = col.a;
                    material.SetColor("_BaseColor", colBase);
                }
            }
        }
    }
}