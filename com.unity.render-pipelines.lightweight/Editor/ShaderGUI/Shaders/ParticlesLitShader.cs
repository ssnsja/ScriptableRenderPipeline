using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEditor.Experimental.Rendering.LightweightPipeline;

namespace UnityEditor.Experimental.Rendering.LightweightPipeline.ShaderGUI
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

/*        public override void DrawAdditionalFoldouts(Material material)
        {
            var vertexStreams = EditorGUILayout.BeginFoldoutHeaderGroup(GetHeaderState(3), ParticleGUI.Styles.VertexStreams, EditorStyles.foldoutHeader, NullThing, ParticleGUI.Styles.vertexStreamIcon);
            if (vertexStreams)
            {
                ParticleGUI.DoVertexStreamsArea(material, m_RenderersUsingThisMaterial);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            StoreHeaderState(vertexStreams, 3);
        }*/

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

/*        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.Label(Styles.blendingOptionsText, EditorStyles.boldLabel);

                BlendModePopup();
                ColorModePopup();

                EditorGUILayout.Space();
                GUILayout.Label(Styles.mainOptionsText, EditorStyles.boldLabel);

                FlipbookModePopup();
                TwoSidedPopup(material);
                FadingPopup(material);

                if (distortionEnabled != null)
                {
                    EditorGUI.BeginChangeCheck();
                    var enabled = distortionEnabled.floatValue;
                    enabled = EditorGUILayout.Toggle(Styles.distortionEnabled, enabled != 0.0f) ? 1.0f : 0.0f;
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MaterialEditor.RegisterPropertyChangeUndo("Distortion Enabled");
                        distortionEnabled.floatValue = enabled;
                    }
                    
                    if (enabled != 0.0f)
                    {
                        int indentation = 2;
                        m_MaterialEditor.ShaderProperty(distortionStrength, Styles.distortionStrength, indentation);
                        m_MaterialEditor.ShaderProperty(distortionBlend, Styles.distortionBlend, indentation);
                    }
                }

                EditorGUILayout.Space();
                GUILayout.Label(Styles.mapsOptionsText, EditorStyles.boldLabel);

                DoAlbedoArea(material);
                DoSpecularMetallicArea(material);
                DoNormalMapArea(material);
                DoEmissionArea(material);

                if (!flipbookMode.hasMixedValue && (FlipbookMode)flipbookMode.floatValue != FlipbookMode.Blended)
                {
                    EditorGUI.BeginChangeCheck();
                    m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
                    if (EditorGUI.EndChangeCheck())
                        emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                    MaterialChanged((Material)obj);
            }

            EditorGUILayout.Space();

            GUILayout.Label(Styles.requiredVertexStreamsText, EditorStyles.boldLabel);
            DoVertexStreamsArea(material);
        }*/

        /*public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            if (material == null)
                throw new ArgumentNullException("oldShader");

            if (newShader == null)
                throw new ArgumentNullException("newShader");

            // Sync the lighting flag for the unlit shader
/*            if (newShader.name.Contains("Unlit"))
                material.SetFloat("_LightingEnabled", 0.0f);
            else
                material.SetFloat("_LightingEnabled", 1.0f);#1#

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Blend"));
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Blend", (float)blendMode);

            MaterialChanged(material);
        }*/

/*        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(StylesParticle.renderingMode, (int)mode, StylesParticle.blendNames);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }*/

/*        void ColorModePopup()
        {
            if (colorMode != null)
            {
                EditorGUI.showMixedValue = colorMode.hasMixedValue;
                var mode = (ColorMode)colorMode.floatValue;

                EditorGUI.BeginChangeCheck();
                mode = (ColorMode)EditorGUILayout.Popup(StylesParticle.colorMode, (int)mode, StylesParticle.colorNames);
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo("Color Mode");
                    colorMode.floatValue = (float)mode;
                }

                EditorGUI.showMixedValue = false;
            }
        }*/


/*        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertyWithHDRColor(Styles.albedoText, albedoMap, albedoColor, true);
            if (((BlendMode)material.GetFloat("_Blend") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText, MaterialEditor.kMiniTextureFieldLabelIndentLevel);
            }
        }*/

        /*public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Additive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Subtractive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Modulate:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.EnableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }*/

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
