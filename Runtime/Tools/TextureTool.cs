using UnityEngine;

namespace PlanetEngine
{
    internal enum CubeSides { FRONT, BACK, LEFT, RIGHT, TOP, BOTTOM }

    internal static class TextureTool
    {
        #region Private properties;
        static ComputeShader activeShader;
        static int activeKernel;

        static ComputeShader baseTextureShader
        {
            get
            {
                ComputeShader baseTextureShader = Resources.Load<ComputeShader>("TextureShaders/BaseTexture");
                if (baseTextureShader == null) Debug.LogWarning("No shader loaded");
                return baseTextureShader;
            }
        }

        static ComputeShader heightTextureShader
        {
            get
            {
                ComputeShader heightTextureShader = Resources.Load<ComputeShader>("TextureShaders/HeightTexture");
                if (heightTextureShader == null) Debug.LogWarning("No shader loaded");
                return heightTextureShader;
            }
        }
        #endregion

        #region Public methods
        public static Texture2D GenerateBaseTexture(int width, int height)
        {
            activeShader = baseTextureShader;
            activeKernel = activeShader.FindKernel("GenerateBaseTexture");
            RenderTexture baseTexture = CreateBaseTexture(width, height);
            activeShader.Dispatch(activeKernel, width, height, 1);
            Texture2D outputTexture = ReadRenderTexture(baseTexture);
            return outputTexture;
        }

        public static Texture2D GenerateBaseTexture(int width, int height, CubeSides side)
        {
            activeShader = baseTextureShader;
            activeKernel = activeShader.FindKernel("GenerateSideBaseTexture");
            RenderTexture baseTexture = CreateBaseTexture(width, height);
            activeShader.SetInt("side", (int)side);
            activeShader.Dispatch(activeKernel, width, height, 1);
            Texture2D outputTexture = ReadRenderTexture(baseTexture);
            return outputTexture;
        }

        public static Texture2D GenerateBaseTexture(Texture2D parentBaseTexture, Rect zone)
        {
            activeShader = baseTextureShader;
            activeKernel = activeShader.FindKernel("GenerateBaseTexture");
            RenderTexture baseTexture = CreateBaseTexture(parentBaseTexture.width, parentBaseTexture.height);
            activeShader.SetTexture(activeKernel, "base_texture", parentBaseTexture);
            SetZone(zone, parentBaseTexture.width, parentBaseTexture.height);
            activeShader.Dispatch(activeKernel, parentBaseTexture.width, parentBaseTexture.height, 1);
            Texture2D outputTexture = ReadRenderTexture(baseTexture);
            return outputTexture;
        }
        
		public static Texture2D GenerateHeightTexture(Texture2D baseTexture, int seed)
		{
            activeShader = heightTextureShader;
            activeKernel = activeShader.FindKernel("GenerateHeightmapTexture");
            RenderTexture heightTexture = CreateHeightTexture(baseTexture.width, baseTexture.height);
            activeShader.SetTexture(activeKernel, "base_texture", baseTexture);
            activeShader.SetInt("seed", seed);
            activeShader.Dispatch(activeKernel, baseTexture.width, baseTexture.height, 1);
            Texture2D outputTexture = ReadRenderTexture(heightTexture);
            return outputTexture;
		}

        








        public static Texture2D GenerateColorTexture(Texture2D heightTexture, Color A, Color B)
        {
            ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
            if (textureShader == null) Debug.LogWarning("No shader loaded");
            int kernelIndex = textureShader.FindKernel("GenerateColorTexture");

            RenderTexture colorTexture = new RenderTexture(heightTexture.width, heightTexture.height, 0, RenderTextureFormat.ARGBFloat);
            colorTexture.enableRandomWrite = true;
            colorTexture.Create();
            textureShader.SetTexture(kernelIndex, "color_texture_out", colorTexture);
            textureShader.SetTexture(kernelIndex, "height_texture", heightTexture);
            textureShader.SetInt("width", heightTexture.width);
            textureShader.SetInt("height", heightTexture.height);
            textureShader.SetFloats("ColorA", new float[] { A.r, A.g, A.b, A.a });
            textureShader.SetFloats("ColorB", new float[] { B.r, B.g, B.b, B.a });
            textureShader.Dispatch(kernelIndex, heightTexture.width, heightTexture.height, 1);
            Texture2D outputTexture = new Texture2D(colorTexture.width, colorTexture.width, TextureFormat.RGBAFloat, false);
            outputTexture.filterMode = FilterMode.Point;
            RenderTexture.active = colorTexture;
            outputTexture.ReadPixels(new Rect(0, 0, colorTexture.width, colorTexture.height), 0, 0);
            outputTexture.Apply();
            return outputTexture;
        }
        #endregion

        #region Private methods
        static RenderTexture CreateBaseTexture(int width, int height)
        {
            RenderTexture baseTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            baseTexture.enableRandomWrite = true;
            baseTexture.Create();
            activeShader.SetTexture(activeKernel, "base_texture_out", baseTexture);
            activeShader.SetInt("width", width);
            activeShader.SetInt("height", height);
            return baseTexture;
        }

        static RenderTexture CreateHeightTexture(int width, int height)
        {
            RenderTexture heightmapTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
            heightmapTexture.enableRandomWrite = true;
            heightmapTexture.Create();
            activeShader.SetTexture(activeKernel, "height_texture_out", heightmapTexture);
            activeShader.SetInt("width", width);
            activeShader.SetInt("height", height);
            return heightmapTexture;
        }

        static void SetZone(Rect zone, int width, int height) 
        {
            activeShader.SetInts("left_bottom_corner", new int[] {
                Mathf.RoundToInt((width - 1) * zone.x),
                Mathf.RoundToInt((height - 1) * zone.y)
            });

            activeShader.SetInts("right_bottom_corner", new int[] {
                Mathf.RoundToInt((width - 1) * (zone.x + zone.width)),
                Mathf.RoundToInt((height - 1) * zone.y)
            });

            activeShader.SetInts("left_top_corner", new int[] {
                Mathf.RoundToInt((width - 1) * zone.x),
                Mathf.RoundToInt((height - 1) * (zone.y + zone.height))
            });

            activeShader.SetInts("right_top_corner", new int[] {
                Mathf.RoundToInt((width - 1) * (zone.x  + zone.width)),
                Mathf.RoundToInt((height - 1) * (zone.y + zone.height))
            });
        }
        static Texture2D ReadRenderTexture(RenderTexture texture, TextureFormat format = TextureFormat.RGBAFloat) 
        {
            Texture2D outputTexture = new Texture2D(texture.width, texture.height, format, false);
            outputTexture.filterMode = FilterMode.Point;
            RenderTexture.active = texture;
            outputTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            outputTexture.Apply();
            return outputTexture;
        }
        #endregion


    }
}
