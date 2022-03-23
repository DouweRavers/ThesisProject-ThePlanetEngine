using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    public class HeightmapTexture : ScriptableObject
    {
        public static Texture2D GetTextureHeightValue(Texture2D baseTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightmapTexture", ShaderType.HEIGHTMAP);
            renderer.AddTexture("base_texture", baseTexture);
            renderer.AddValue("seed", data.Seed);
            renderer.AddValue("continent_scale", data.ContinentScale);
            renderer.OutputTextureProperties("height_texture_out", baseTexture.width, baseTexture.height, RenderTextureFormat.RFloat);
            return renderer.GetOutputTexture(TextureFormat.RFloat);
        }

        public static Texture2D GetTextureColored(Texture2D baseTexture, Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightmapColorTexture", ShaderType.HEIGHTMAP);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddTexture("base_texture", baseTexture);
            renderer.AddTexture("gradient_texture", data.OceanGradient.GetTexture(baseTexture.width, baseTexture.height));
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.OutputTextureProperties("color_texture_out", baseTexture.width, baseTexture.height);
            return renderer.GetOutputTexture();
        }
    }
}