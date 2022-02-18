using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System;

namespace PlanetEngine {

	public static class TextureTool {
		
		public static Texture2D GenerateBaseTexture(int width, int height) {
			ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
			if (textureShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = textureShader.FindKernel("GenerateBaseTexture");

			RenderTexture baseTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
			baseTexture.enableRandomWrite = true;
			baseTexture.Create();
			textureShader.SetTexture(kernelIndex, "base_texture_out", baseTexture);
			textureShader.SetInt("width", width);
			textureShader.SetInt("height", height);
			textureShader.Dispatch(kernelIndex, width, height, 1);
			Texture2D outputTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
			outputTexture.filterMode = FilterMode.Point;
			RenderTexture.active = baseTexture;
			outputTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			outputTexture.Apply();
			return outputTexture;
		}

		public static Texture2D RegenerateBaseTextureForSubSurface(Texture2D baseTexture, Rect zone, RectInt size)
		{
			ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
			if (textureShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = textureShader.FindKernel("RegenerateBaseTexture");

			// Output texture
			RenderTexture renderTexture = new RenderTexture(size.width, size.height, 0, RenderTextureFormat.ARGBFloat);
			renderTexture.enableRandomWrite = true;
			renderTexture.Create();
			textureShader.SetTexture(kernelIndex, "base_texture_out", renderTexture);
			textureShader.SetInt("width", size.width - 1);
			textureShader.SetInt("height", size.height - 1);

			// Input vertices
			Color[] vertexColors = new Color[]{
				baseTexture.GetPixel(
					Mathf.RoundToInt(baseTexture.width * zone.x),
					Mathf.RoundToInt(baseTexture.height * zone.y)),
				baseTexture.GetPixel(
					Mathf.RoundToInt(baseTexture.width * (zone.x + zone.width))- 2,
					Mathf.RoundToInt(baseTexture.height * zone.y)+ 1),
				baseTexture.GetPixel(
					Mathf.RoundToInt(baseTexture.width * zone.x)+ 1,
					Mathf.RoundToInt(baseTexture.height * (zone.y + zone.height))- 2),
				baseTexture.GetPixel(
					Mathf.RoundToInt(baseTexture.width * (zone.x  + zone.width))- 2,
					Mathf.RoundToInt(baseTexture.height * (zone.y + zone.height))-2)
			};
			textureShader.SetFloats("left_bottom_corner", new float[] { vertexColors[0].r, vertexColors[0].g, vertexColors[0].b, vertexColors[0].a });
			textureShader.SetFloats("right_bottom_corner", new float[] { vertexColors[1].r, vertexColors[1].g, vertexColors[1].b, vertexColors[1].a });
			textureShader.SetFloats("left_top_corner", new float[] { vertexColors[2].r, vertexColors[2].g, vertexColors[2].b, vertexColors[2].a });
			textureShader.SetFloats("right_top_corner", new float[] { vertexColors[3].r, vertexColors[3].g, vertexColors[3].b, vertexColors[3].a });

			textureShader.Dispatch(kernelIndex, size.width, size.height, 1);
			Texture2D outputTexture = new Texture2D(size.width, size.height, TextureFormat.RGBAFloat, false);
			outputTexture.filterMode = FilterMode.Point;
			RenderTexture.active = renderTexture;
			outputTexture.ReadPixels(new Rect(0, 0, size.width, size.height), 0, 0);
			outputTexture.Apply();
			return outputTexture;
		}

		public static Texture2D GenerateHeightTexture(Texture2D baseTexture)
		{
			ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
			if (textureShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = textureShader.FindKernel("GenerateHeightmapTexture");

			RenderTexture heightmapTexture = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.RFloat);
			heightmapTexture.enableRandomWrite = true;
			heightmapTexture.Create();
			textureShader.SetTexture(kernelIndex, "height_texture_out", heightmapTexture);
			textureShader.SetTexture(kernelIndex, "base_texture", baseTexture);
			textureShader.SetInt("width", baseTexture.width);
			textureShader.SetInt("height", baseTexture.height);
			textureShader.Dispatch(kernelIndex, baseTexture.width, baseTexture.height, 1);
			Texture2D outputTexture = new Texture2D(heightmapTexture.width, heightmapTexture.width, TextureFormat.RFloat, false);
			outputTexture.filterMode = FilterMode.Point;
			RenderTexture.active = heightmapTexture;
			outputTexture.ReadPixels(new Rect(0, 0, heightmapTexture.width, heightmapTexture.height), 0, 0);
			outputTexture.Apply();
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
	}
}
