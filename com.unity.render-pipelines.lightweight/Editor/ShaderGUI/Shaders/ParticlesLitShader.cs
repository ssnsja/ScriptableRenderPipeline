using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Rendering.LWRP.ShaderGUI
{
    internal class ParticlesLitShader : BaseShaderGUI
    {
        // Properties
        private LitGUI.LitProperties litProperties;
        private ParticleGUI.ParticleProperties particleProps;
        
        // List of renderers using this material in the scene, used for validating vertex streams
        List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();
        
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new LitGUI.LitProperties(properties);
            particleProps = new ParticleGUI.ParticleProperties(properties);
        }
        
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
            
            material.shaderKeywords = null; // Clear all keywords

            SetupMaterialBlendMode(material);
            //SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Blend"));
            ParticleGUI.SetupMaterialWithColorMode(material);
            ParticleGUI.SetMaterialKeywords(material); // Set particle specific keywords
            LitGUI.SetMaterialKeywords(material, litProperties); // Set lit specific 
        }
        
        public override void DrawSurfaceOptions(Material material)
        {
            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                base.DrawSurfaceOptions(material);
                DoPopup(ParticleGUI.Styles.colorMode, particleProps.colorMode, Enum.GetNames(typeof(ParticleGUI.ColorMode)));
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material)obj);
            }
        }
        
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            LitGUI.Inputs(litProperties, materialEditor);
            DrawEmissionProperties(material, true);
        }
        
        public override void DrawAdvancedOptions(Material material)
        {
            EditorGUI.BeginChangeCheck();
            {
                materialEditor.ShaderProperty(particleProps.flipbookMode, ParticleGUI.Styles.flipbookMode);
                ParticleGUI.FadingOptions(material, materialEditor, particleProps);
                ParticleGUI.DoVertexStreamsArea(material, m_RenderersUsingThisMaterial);
            }
            base.DrawAdvancedOptions(material);
        }

        private static void NullThing(Rect rect){}

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            if (m_FirstTimeApply)
            {
                CacheRenderersUsingThisMaterial(materialEditor.target as Material);
                m_FirstTimeApply = false;
            }
            
            base.OnGUI(materialEditor, props);
        }

        void CacheRenderersUsingThisMaterial(Material material)
        {
            m_RenderersUsingThisMaterial.Clear();

            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer.sharedMaterial == material)
                    m_RenderersUsingThisMaterial.Add(renderer);
            }
        }
    }
} // namespace UnityEditor
