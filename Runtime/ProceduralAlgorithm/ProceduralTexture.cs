using UnityEngine;

namespace PlanetEngine
{
    public enum CubeSides { FRONT, BACK, LEFT, RIGHT, TOP, BOTTOM }

    public class ProceduralTexture : ScriptableObject
    {
        #region Data textures
        public static Texture2D GetBaseTexture(int width, int height)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateBaseTexture", ShaderType.BASE);
            renderer.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat);
            return renderer.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        public static Texture2D GetBaseTexture(int width, int height, CubeSides side)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateSideBaseTexture", ShaderType.BASE);
            renderer.AddValue("side", (int)side);
            renderer.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat);
            return renderer.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        public static Texture2D GetBaseTexture(Texture2D parentBaseTexture, Rect zone)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("RegenerateBaseTexture", ShaderType.BASE);
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

        public static Texture2D GetHeightTexture(Texture2D baseTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightmapTexture", ShaderType.DATA);
            renderer.AddTexture("base_texture", baseTexture);
            renderer.AddValue("seed", data.Seed);
            renderer.AddValue("continent_scale", data.ContinentScale);
            renderer.OutputTextureProperties("height_texture_out", baseTexture.width, baseTexture.height, RenderTextureFormat.RFloat);
            return renderer.GetOutputTexture(TextureFormat.RFloat);
        }

        public static Texture2D GetHeatTexture(Texture2D baseTexture, Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeatTexture", ShaderType.DATA);
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
            renderer.SetKernel("GenerateHumidityTexture", ShaderType.DATA);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddValue("humidity_factor", data.HumidityTransfer);
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.OutputTextureProperties("humidity_texture_out", heightTexture.width, heightTexture.height, RenderTextureFormat.RFloat);
            return renderer.GetOutputTexture(TextureFormat.RFloat);
        }
        #endregion

        #region Color textures
        public static Texture2D GetHeightTextureColored(Texture2D baseTexture, Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightmapColorTexture", ShaderType.COLOR);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddTexture("base_texture", baseTexture);
            renderer.AddTexture("gradient_ocean_texture", data.OceanGradient.GetTexture(baseTexture.width, baseTexture.height));
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.AddValue("solar_heat", data.SolarHeat);
            renderer.AddValue("height_cooling", data.HeightCooling);
            renderer.OutputTextureProperties("color_texture_out", baseTexture.width, baseTexture.height);
            return renderer.GetOutputTexture();
        }

        public static Texture2D GetHeatTextureColored(Texture2D dataTexture)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeatmapColorTexture", ShaderType.COLOR);
            renderer.AddTexture("heat_texture", dataTexture);
            renderer.OutputTextureProperties("color_texture_out", dataTexture.width, dataTexture.height);
            return renderer.GetOutputTexture();
        }

        public static Texture2D GetHumidityTextureColored(Texture2D dataTexture)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHumidityColorTexture", ShaderType.COLOR);
            renderer.AddTexture("humidity_texture", dataTexture);
            renderer.OutputTextureProperties("color_texture_out", dataTexture.width, dataTexture.height);
            return renderer.GetOutputTexture();
        }

        public static Texture2D GetBiomeTextureColored(Texture2D heightTexture, Texture2D heatTexture, Texture2D humidityTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateBiomeColorTexture", ShaderType.COLOR);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddTexture("heat_texture", heatTexture);
            renderer.AddTexture("humidity_texture", humidityTexture);
            renderer.AddTexture("gradient_ocean_texture", data.OceanGradient.GetTexture(heightTexture.width, heightTexture.height));
            renderer.AddTexture("gradient_biome_texture", data.biomeGradient.GetTexture(heightTexture.width, heightTexture.height));
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.OutputTextureProperties("color_texture_out", heightTexture.width, heightTexture.height);
            return renderer.GetOutputTexture();
        }

        public static Texture2D GetOceanTextureColored(Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateOceanColorTexture", ShaderType.COLOR);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddTexture("gradient_ocean_texture", data.OceanGradient.GetTexture(heightTexture.width, heightTexture.height));
            renderer.OutputTextureProperties("color_texture_out", heightTexture.width, heightTexture.height);
            return renderer.GetOutputTexture();
        }

        public static Texture2D GetGroundTextureColored(Texture2D heatTexture, Texture2D humidityTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateGroundColorTexture", ShaderType.COLOR);
            renderer.AddTexture("heat_texture", heatTexture);
            renderer.AddTexture("humidity_texture", humidityTexture);
            renderer.AddTexture("gradient_biome_texture", data.biomeGradient.GetTexture(heatTexture.width, heatTexture.height));
            renderer.OutputTextureProperties("color_texture_out", heatTexture.width, heatTexture.height);
            return renderer.GetOutputTexture();
        }
        #endregion

        #region Effect textures
        public static Texture2D GetOceanReflectiveTexture(Texture2D heightTexture, PlanetData data)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateHeightmapReflectiveTexture", ShaderType.EFFECT);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddValue("reflectiveness", data.OceanReflectiveness);
            renderer.OutputTextureProperties("reflective_texture_out", heightTexture.width, heightTexture.height);
            return renderer.GetOutputTexture();
        }

        public static Texture2D GetNormalTexture(Texture2D heightTexture, PlanetData data) {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateNormalTexture", ShaderType.EFFECT);
            renderer.AddTexture("height_texture", heightTexture);
            renderer.AddValue("strenght", 10f * data.heightDifference);
            renderer.AddValue("has_ocean", data.HasOcean);
            renderer.OutputTextureProperties("normal_texture_out", heightTexture.width, heightTexture.height);
            return renderer.GetOutputTexture();
        }
        #endregion
    }
}
