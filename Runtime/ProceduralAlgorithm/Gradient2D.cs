using System.Collections.Generic;
using UnityEngine;
using System;

namespace PlanetEngine
{
    [Serializable]
    public struct GradientPoint
    {
        public Color Color;
        public Vector2 Position;
        public float Weight;

        public GradientPoint(Color color, Vector2 position, float weight)
        {
            Color = color;
            Position = position;
            Weight = weight;
        }
    }


    [Serializable]
    public class Gradient2D : ScriptableObject
    {
        public float Smooth = 1f;
        public List<GradientPoint> Points;

        public Texture2D GetTexture(int width, int height)
        {
            Color[] colors; Vector2[] positions; float[] weights;
            GetPointData(out colors, out positions, out weights);
            if (colors.Length == 0) return null;
            TextureRenderer renderer = CreateInstance<TextureRenderer>();
            renderer.SetKernel("GenerateGradient2DTexture", ShaderType.GRADIENT);
            renderer.AddArray("point_color", colors);
            renderer.AddArray("point_position", positions);
            renderer.AddArray("point_weight", weights);
            renderer.AddValue("point_count", colors.Length);
            renderer.AddValue("smooth", Smooth);
            renderer.OutputTextureProperties("gradient_texture_out", width, height);
            return renderer.GetOutputTexture();
        }

        public void GetPointData(out Color[] colors, out Vector2[] positions, out float[] weights)
        {
            colors = new Color[Points.Count];
            positions = new Vector2[Points.Count];
            weights = new float[Points.Count];

            for (int i = 0; i < Points.Count; i++)
            {
                colors[i] = Points[i].Color;
                positions[i] = Points[i].Position;
                weights[i] = Points[i].Weight;
            }
        }

        void Awake()
        {
            Points = new List<GradientPoint>();
            Points.Add(new GradientPoint(Color.black, Vector2.one * 0.5f, 1f));
        }

    }
}