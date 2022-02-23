using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System;

namespace PlanetEngine {
	internal enum CubeSides { FRONT, BACK, LEFT, RIGHT, TOP, BOTTOM }

	internal static class TextureTool {
		
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

		public static Texture2D GenerateBaseTexture(int width, int height, CubeSides side)
		{
			ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
			if (textureShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = textureShader.FindKernel("GenerateSideBaseTexture");

			RenderTexture baseTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
			baseTexture.enableRandomWrite = true;
			baseTexture.Create();
			textureShader.SetTexture(kernelIndex, "base_texture_out", baseTexture);
			textureShader.SetInt("width", width);
			textureShader.SetInt("height", height);
			textureShader.SetInt("side", (int)side);
			textureShader.Dispatch(kernelIndex, width, height, 1);
			Texture2D outputTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
			outputTexture.filterMode = FilterMode.Point;
			RenderTexture.active = baseTexture;
			outputTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			outputTexture.Apply();
			return outputTexture;
		}

		public static Texture2D GenerateBaseTexture(Texture2D baseTexture, Rect zone)
		{
			ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
			if (textureShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = textureShader.FindKernel("RegenerateBaseTexture");

			// Output texture
			RenderTexture renderTexture = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.ARGBFloat);
			renderTexture.enableRandomWrite = true;
			renderTexture.Create();
			textureShader.SetTexture(kernelIndex, "base_texture_out", renderTexture);
			textureShader.SetInt("width", baseTexture.width);
			textureShader.SetInt("height", baseTexture.height);

			textureShader.SetTexture(kernelIndex, "base_texture", baseTexture);
			textureShader.SetInts("left_bottom_corner", new int[] {
				Mathf.RoundToInt((baseTexture.width - 1) * zone.x), 
				Mathf.RoundToInt((baseTexture.height - 1) * zone.y)
			});
			
			textureShader.SetInts("right_bottom_corner", new int[] {
				Mathf.RoundToInt((baseTexture.width - 1) * (zone.x + zone.width)),
				Mathf.RoundToInt((baseTexture.height - 1) * zone.y)
			});
			
			textureShader.SetInts("left_top_corner", new int[] {
				Mathf.RoundToInt((baseTexture.width - 1) * zone.x),
				Mathf.RoundToInt((baseTexture.height - 1) * (zone.y + zone.height))
			});
			
			textureShader.SetInts("right_top_corner", new int[] {
				Mathf.RoundToInt((baseTexture.width - 1) * (zone.x  + zone.width)),
				Mathf.RoundToInt((baseTexture.height - 1) * (zone.y + zone.height))
			});

			textureShader.Dispatch(kernelIndex, baseTexture.width, baseTexture.height, 1);
			Texture2D outputTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBAFloat, false);
			outputTexture.filterMode = FilterMode.Point;
			RenderTexture.active = renderTexture;
			outputTexture.ReadPixels(new Rect(0, 0, baseTexture.width, baseTexture.height), 0, 0);
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
