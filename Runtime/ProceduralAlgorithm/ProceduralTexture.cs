using UnityEngine;

namespace PlanetEngine
{
    internal enum CubeSides { FRONT, BACK, LEFT, RIGHT, TOP, BOTTOM }

    internal class ProceduralTexture : ScriptableObject
    {
        #region Data textures
        internal static Texture2D GetBaseTexture(int width, int height)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateBaseTexture", ShaderType.BASE);
            generator.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat);
            return generator.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        internal static Texture2D GetBaseTexture(int width, int height, CubeSides side)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateSideBaseTexture", ShaderType.BASE);
            generator.AddValue("side", (int)side);
            generator.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat);
            return generator.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        internal static Texture2D GetBaseTexture(int width, int height, Vector3[] corners)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateCutOutBaseTexture", ShaderType.BASE);
            generator.AddValue("left_top_vec", corners[0]);
            generator.AddValue("right_top_vec", corners[1]);
            generator.AddValue("right_bottom_vec", corners[2]);
            generator.AddValue("left_bottom_vec", corners[3]);
            generator.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat);
            return generator.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        internal static Texture2D GetBaseTexture(Texture2D parentBaseTexture, Rect zone)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("RegenerateBaseTexture", ShaderType.BASE);
            generator.AddTexture("base_texture", parentBaseTexture);
            SetZone(zone, parentBaseTexture.width, parentBaseTexture.height);
            generator.OutputTextureProperties("base_texture_out", parentBaseTexture.width, parentBaseTexture.height, RenderTextureFormat.ARGBFloat);
            return generator.GetOutputTexture(TextureFormat.RGBAFloat);

            void SetZone(Rect zone, int width, int height)
            {
                generator.AddValue("left_bottom_corner", new int[] {
                    Mathf.RoundToInt((width - 1) * zone.x),
                    Mathf.RoundToInt((height - 1) * zone.y)
                });

                generator.AddValue("right_bottom_corner", new int[] {
                    Mathf.RoundToInt((width - 1) * (zone.x + zone.width)),
                    Mathf.RoundToInt((height - 1) * zone.y)
                });

                generator.AddValue("left_top_corner", new int[] {
                    Mathf.RoundToInt((width - 1) * zone.x),
                    Mathf.RoundToInt((height - 1) * (zone.y + zone.height))
                });

                generator.AddValue("right_top_corner", new int[] {
                    Mathf.RoundToInt((width - 1) * (zone.x  + zone.width)),
                    Mathf.RoundToInt((height - 1) * (zone.y + zone.height))
                });
            }
        }

        internal static Texture2D GetHeightTexture(Texture2D baseTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateHeightmapTexture", ShaderType.DATA);
            generator.AddTexture("base_texture", baseTexture);
            generator.AddValue("seed", data.Seed);
            generator.AddValue("continent_scale", data.ContinentScale);
            generator.OutputTextureProperties("height_texture_out", baseTexture.width, baseTexture.height, RenderTextureFormat.RFloat);
            return generator.GetOutputTexture(TextureFormat.RFloat);
        }

        internal static Texture2D GetHeatTexture(Texture2D baseTexture, Texture2D heightTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateHeatTexture", ShaderType.DATA);
            generator.AddTexture("base_texture", baseTexture);
            generator.AddTexture("height_texture", heightTexture);
            generator.AddValue("solar_heat", data.SolarHeat);
            generator.AddValue("height_cooling", data.HeightCooling);
            generator.AddValue("has_ocean", data.HasOcean);
            generator.OutputTextureProperties("heat_texture_out", baseTexture.width, baseTexture.height, RenderTextureFormat.RFloat);
            return generator.GetOutputTexture(TextureFormat.RFloat);
        }

        internal static Texture2D GetHumidityTexture(Texture2D heightTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateHumidityTexture", ShaderType.DATA);
            generator.AddTexture("height_texture", heightTexture);
            generator.AddValue("humidity_factor", data.HumidityTransfer);
            generator.AddValue("has_ocean", data.HasOcean);
            generator.OutputTextureProperties("humidity_texture_out", heightTexture.width, heightTexture.height, RenderTextureFormat.RFloat);
            return generator.GetOutputTexture(TextureFormat.RFloat);
        }
        #endregion

        #region Color textures
        internal static Texture2D GetHeightTextureColored(Texture2D baseTexture, Texture2D heightTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateHeightmapColorTexture", ShaderType.COLOR);
            generator.AddTexture("height_texture", heightTexture);
            generator.AddTexture("base_texture", baseTexture);
            generator.AddTexture("gradient_ocean_texture", data.OceanGradient.GetTexture(baseTexture.width, baseTexture.height));
            generator.AddValue("has_ocean", data.HasOcean);
            generator.AddValue("solar_heat", data.SolarHeat);
            generator.AddValue("height_cooling", data.HeightCooling);
            generator.OutputTextureProperties("color_texture_out", baseTexture.width, baseTexture.height);
            return generator.GetOutputTexture();
        }

        internal static Texture2D GetHeatTextureColored(Texture2D dataTexture)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateHeatmapColorTexture", ShaderType.COLOR);
            generator.AddTexture("heat_texture", dataTexture);
            generator.OutputTextureProperties("color_texture_out", dataTexture.width, dataTexture.height);
            return generator.GetOutputTexture();
        }

        internal static Texture2D GetHumidityTextureColored(Texture2D dataTexture)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateHumidityColorTexture", ShaderType.COLOR);
            generator.AddTexture("humidity_texture", dataTexture);
            generator.OutputTextureProperties("color_texture_out", dataTexture.width, dataTexture.height);
            return generator.GetOutputTexture();
        }

        internal static Texture2D GetBiomeTextureColored(Texture2D heightTexture, Texture2D heatTexture, Texture2D humidityTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateBiomeColorTexture", ShaderType.COLOR);
            generator.AddTexture("height_texture", heightTexture);
            generator.AddTexture("heat_texture", heatTexture);
            generator.AddTexture("humidity_texture", humidityTexture);
            generator.AddTexture("gradient_ocean_texture", data.OceanGradient.GetTexture(heightTexture.width, heightTexture.height));
            generator.AddTexture("gradient_biome_texture", data.BiomeGradient.GetTexture(heightTexture.width, heightTexture.height));
            generator.AddValue("has_ocean", data.HasOcean);
            generator.OutputTextureProperties("color_texture_out", heightTexture.width, heightTexture.height);
            return generator.GetOutputTexture();
        }

        internal static Texture2D GetOceanTextureColored(Texture2D heightTexture, Texture2D heatTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateOceanColorTexture", ShaderType.COLOR);
            generator.AddTexture("height_texture", heightTexture);
            generator.AddTexture("heat_texture", heatTexture);
            generator.AddTexture("gradient_ocean_texture", data.OceanGradient.GetTexture(heightTexture.width, heightTexture.height));
            generator.OutputTextureProperties("color_texture_out", heightTexture.width, heightTexture.height);
            return generator.GetOutputTexture();
        }

        internal static Texture2D GetGroundTextureColored(Texture2D heatTexture, Texture2D humidityTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateGroundColorTexture", ShaderType.COLOR);
            generator.AddTexture("heat_texture", heatTexture);
            generator.AddTexture("humidity_texture", humidityTexture);
            generator.AddTexture("gradient_biome_texture", data.BiomeGradient.GetTexture(heatTexture.width, heatTexture.height));
            generator.OutputTextureProperties("color_texture_out", heatTexture.width, heatTexture.height);
            return generator.GetOutputTexture();
        }
        #endregion

        #region Effect textures
        internal static Texture2D GetOceanReflectiveTexture(Texture2D heightTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateHeightmapReflectiveTexture", ShaderType.EFFECT);
            generator.AddTexture("height_texture", heightTexture);
            generator.AddValue("reflectiveness", data.OceanReflectiveness);
            generator.OutputTextureProperties("reflective_texture_out", heightTexture.width, heightTexture.height);
            return generator.GetOutputTexture();
        }

        internal static Texture2D GetNormalTexture(Texture2D heightTexture, ProceduralData data)
        {
            TextureCompute generator = CreateInstance<TextureCompute>();
            generator.SetKernel("GenerateNormalTexture", ShaderType.EFFECT);
            generator.AddTexture("height_texture", heightTexture);
            generator.AddValue("strenght", 10f * data.heightDifference);
            generator.AddValue("has_ocean", data.HasOcean);
            generator.OutputTextureProperties("normal_texture_out", heightTexture.width, heightTexture.height);
            return generator.GetOutputTexture();
        }
        #endregion
    }
}
