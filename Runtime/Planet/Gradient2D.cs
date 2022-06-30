using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// A gradient point contains data a color, position and weight.
    /// The total gradient texture is a interpolation between these points.
    /// </summary>
    [Serializable]
    public struct GradientPoint
    {
        public Color Color { get; set; }
        public Vector2 Position { get; set; }
        public float Weight { get; set; }

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
        /// <summary>
        /// This value determines how much colors are blend with 0 being totally seperate and 1 one combined color.
        /// </summary>
        public float Smooth { get; set; } = 1f;

        /// <summary>
        /// This list holds all data points of the gradient.
        /// </summary>
        public List<GradientPoint> Points { get; set; }

        private void Awake()
        {
            Points = new List<GradientPoint>();
            Points.Add(new GradientPoint(Color.black, Vector2.one * 0.5f, 1f));
        }

        /// <summary>
        /// Generate a texture of given size of this gradient.
        /// </summary>
        /// <returns>A texture of the gradient</returns>
        public Texture2D GetTexture(int width, int height)
        {
            Color[] colors; Vector2[] positions; float[] weights;
            GetPointData(out colors, out positions, out weights);
            if (colors.Length == 0) return null;
            TextureCompute renderer = CreateInstance<TextureCompute>();
            renderer.SetKernel("GenerateGradient2DTexture", ShaderType.GRADIENT);
            renderer.AddArray("point_color", colors);
            renderer.AddArray("point_position", positions);
            renderer.AddArray("point_weight", weights);
            renderer.AddValue("point_count", colors.Length);
            renderer.AddValue("smooth", Smooth);
            renderer.OutputTextureProperties("gradient_texture_out", width, height);
            return renderer.GetOutputTexture();
        }
        /// <summary>
        /// Splits the point data into seperate arrays.
        /// </summary>
        private void GetPointData(out Color[] colors, out Vector2[] positions, out float[] weights)
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
    }
}