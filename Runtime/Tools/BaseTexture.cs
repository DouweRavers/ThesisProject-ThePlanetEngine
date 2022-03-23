using UnityEngine;

namespace PlanetEngine
{
    internal enum CubeSides { FRONT, BACK, LEFT, RIGHT, TOP, BOTTOM }
    internal class BaseTexture : ScriptableObject
    {
        public static Texture2D GetTexture(int width, int height)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateBaseTexture", ShaderType.BASETEXTURE);
            renderer.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat);
            return renderer.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        public static Texture2D GetTexture(int width, int height, CubeSides side)
        {
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateSideBaseTexture", ShaderType.BASETEXTURE);
            renderer.AddValue("side", (int)side);
            renderer.OutputTextureProperties("base_texture_out", width, height, RenderTextureFormat.ARGBFloat);
            return renderer.GetOutputTexture(TextureFormat.RGBAFloat);
        }

        public static Texture2D GetTexture(Texture2D parentBaseTexture, Rect zone)
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


    }
}