using UnityEngine;

namespace PlanetEngine
{
    public enum CubeSides { FRONT, BACK, LEFT, RIGHT, TOP, BOTTOM }

    public class ProceduralTexture : ScriptableObject
    {
        #region Base textures        
        public static Texture2D GetBaseTexture(int width, int height)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateBaseTexture", ShaderType.BASETEXTURE);
            renderer.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat);
            return renderer.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        public static Texture2D GetBaseTexture(int width, int height, CubeSides side)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateSideBaseTexture", ShaderType.BASETEXTURE);
            renderer.AddValue("side", (int)side);
            renderer.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat); 
            return renderer.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        public static Texture2D GetBaseTexture(Texture2D parentBaseTexture, Rect zone)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("RegenerateBaseTexture", ShaderType.BASETEXTURE);
            renderer.AddTexture("base_texture", parentBaseTexture);
            SetZone(zone, parentBaseTexture.width, parentBaseTexture.height);
            renderer.OutputTextureProperties("base_texture_out", parentBaseTexture.width, parentBaseTexture.height, RenderTextureFormat.ARGBFloat);
            return renderer.GetOutputTexture(TextureFormat.RGBAFloat);

            void SetZone(Rect zone, int width, int height)
            {
                renderer.AddValue("left_bottom_corner", new int[] {
                    Mathf.RoundToInt((width - 1) * zone.x),
                    Mathf.RoundToInt((height - 1) * zone.y)
                });

                renderer.AddValue("right_bottom_corner", new int[] {
                    Mathf.RoundToInt((width - 1) * (zone.x + zone.width)),
                    Mathf.RoundToInt((height - 1) * zone.y)
                });

                renderer.AddValue("left_top_corner", new int[] {
                    Mathf.RoundToInt((width - 1) * zone.x),
                    Mathf.RoundToInt((height - 1) * (zone.y + zone.height))
                });

                renderer.AddValue("right_top_corner", new int[] {
                    Mathf.RoundToInt((width - 1) * (zone.x  + zone.width)),
                    Mathf.RoundToInt((height - 1) * (zone.y + zone.height))
                });
            }
        }
        #endregion

        #region Height textures
        public static Texture2D GetHeightTexture(Texture2D baseTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightmapTexture", ShaderType.HEIGHTMAP);
            renderer.AddTexture("base_texture", baseTexture);
            renderer.AddValue("seed", data.Seed);
            renderer.AddValue("continent_scale", data.ContinentScale);
            renderer.OutputTextureProperties("height_texture_out", baseTexture.width, baseTexture.height, RenderTextureFormat.RFloat);
            return renderer.GetOutputTexture(TextureFormat.RFloat);
        }

        public static Texture2D GetHeightTextureColored(Texture2D baseTexture, Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightmapColorTexture", ShaderType.HEIGHTMAP);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddTexture("base_texture", baseTexture);
            renderer.AddTexture("gradient_texture", data.OceanGradient.GetTexture(baseTexture.width, baseTexture.height));
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.AddValue("solar_heat", data.SolarHeat);
            renderer.AddValue("height_cooling", data.HeightCooling);
            renderer.OutputTextureProperties("color_texture_out", baseTexture.width, baseTexture.height);
            return renderer.GetOutputTexture();
        }

        public static Texture2D GetHeightBaseTexture(Texture2D baseTexture, Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightBaseTexture", ShaderType.HEIGHTMAP);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddTexture("base_texture", baseTexture);
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.OutputTextureProperties("height_base_texture_out", baseTexture.width, baseTexture.height, RenderTextureFormat.ARGBFloat);
            return renderer.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        public static Texture2D GetNormalTexture(Texture2D heightTexture)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateNormalTexture", ShaderType.HEIGHTMAP);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.OutputTextureProperties("normal_texture_out", heightTexture.width, heightTexture.height);
            return renderer.GetOutputTexture();
        }
        #endregion

        #region Ocean textures
        public static Texture2D GetOceanReflectiveTexture(Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightmapReflectiveTexture", ShaderType.HEIGHTMAP);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddValue("reflectiveness", data.OceanReflectiveness);
            renderer.OutputTextureProperties("reflective_texture_out", heightTexture.width, heightTexture.height);
            return renderer.GetOutputTexture();
        }
        #endregion

        #region Climate textures
        public static Texture2D GetHeatTexture(Texture2D baseTexture, Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeatTexture", ShaderType.CLIMATE);
            renderer.AddTexture("base_texture", baseTexture);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddValue("solar_heat", data.SolarHeat);
            renderer.AddValue("height_cooling", data.HeightCooling);
            renderer.OutputTextureProperties("heat_texture_out", baseTexture.width, baseTexture.height, RenderTextureFormat.RFloat);
            return renderer.GetOutputTexture(TextureFormat.RFloat);
        }

        public static Texture2D GetHumidityTexture(Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHumidityTexture", ShaderType.CLIMATE);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddValue("humidity_factor", data.HumidityTransfer);
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.OutputTextureProperties("humidity_texture_out", heightTexture.width, heightTexture.height, RenderTextureFormat.RFloat);
            return renderer.GetOutputTexture(TextureFormat.RFloat);
        }

        public static Texture2D GetHeatHumidityTextureColored(Texture2D dataTexture, bool previewHeat)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateColorTexture", ShaderType.CLIMATE);
            renderer.AddTexture("data_texture", dataTexture);
            renderer.AddValue("preview_heat", previewHeat);
            renderer.OutputTextureProperties("color_texture_out", dataTexture.width, dataTexture.height);
            return renderer.GetOutputTexture();
        }
        #endregion

        #region Biome textures
        public static Texture2D GetBiomeTexture(Texture2D heightTexture, Texture2D heatTexture, Texture2D humidityTexture, PlanetData data) {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateBiomeTexture", ShaderType.BIOME);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddTexture("heat_texture", heatTexture);
            renderer.AddTexture("humidity_texture", humidityTexture);
            renderer.AddTexture("gradient_ocean_texture", data.OceanGradient.GetTexture(heightTexture.width, heightTexture.height));
            renderer.AddTexture("gradient_biome_texture", data.biomeGradient.GetTexture(heightTexture.width, heightTexture.height));
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.OutputTextureProperties("biome_texture_out", heightTexture.width, heightTexture.height);
            return renderer.GetOutputTexture();
        }
        #endregion
    }
}
