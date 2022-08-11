using System;
using UnityEditor;
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
        public Color Color;
        Texture2D _texture;
        [SerializeField]
        string _texturePath;
        public Vector2 Position;
        public float Weight;

        public GradientPoint(Color color, Vector2 position, float weight)
        {
            _texture = null;
            _texturePath = null;
            Color = color;
            Position = position;
            Weight = weight;
        }

        public GradientPoint(Texture2D texture, Vector2 position, float weight)
        {
            _texture = texture;
            string path = AssetDatabase.GetAssetPath(texture);
            if (path == null || path.Length == 0)
            {
                path = $"Assets/PlanetEngineData/Unsafed-PointTexture-{Time.fixedUnscaledTime}.png";
                AssetDatabase.CreateAsset(texture, path);
            }
            _texturePath = path;
            Color = CalcAverageColorOfTexture(texture);
            Position = position;
            Weight = weight;
        }

        public GradientPoint(string path, Vector2 position, float weight)
        {
            _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            _texturePath = path;
            Color = CalcAverageColorOfTexture(_texture);
            Position = position;
            Weight = weight;
        }

        public void SetTexture(Texture2D texture)
        {
            _texture = texture;
            string path = AssetDatabase.GetAssetPath(texture);
            if (path == null || path.Length == 0)
            {
                path = $"Assets/PlanetEngineData/Unsafed-PointTexture-{Time.fixedUnscaledTime}.png";
                AssetDatabase.CreateAsset(texture, path);
            }
            _texturePath = path;
        }

        public void SetTexture(string path)
        {
            _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            _texturePath = path;
        }

        public Texture2D GetTexture() { return _texture; }

        public void UpdateTexture()
        {
            _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(_texturePath);
            UpdateColor();
        }

        public void UpdateColor()
        {
            if (_texture == null) Color = CalcAverageColorOfTexture(_texture);
        }

        public bool Equals(GradientPoint point)
        {
            return
                this._texture == point._texture &&
                this.Color == point.Color &&
                this.Position == point.Position &&
                this.Weight == point.Weight;
        }


        static Color CalcAverageColorOfTexture(Texture2D texture)
        {
            Color[] texColors = texture.GetPixels();

            int total = texColors.Length;

            float r = 0;
            float g = 0;
            float b = 0;

            for (int i = 0; i < total; i++)
            {

                r += texColors[i].r;

                g += texColors[i].g;

                b += texColors[i].b;

            }

            return new Color((r / total), (g / total), (b / total), 1);
        }
    }

    [Serializable]
    public struct Gradient2D
    {
        /// <summary>
        /// This value determines how much colors are blend with 10 being totally seperate and 0.1 one combined color.
        /// </summary>
        public float Smooth;
        public int LastSelectedPoint;
        /// <summary>
        /// This list holds all data points of the gradient.
        /// </summary>
        public GradientPoint[] Points;

        public Gradient2D(Color color)
        {
            LastSelectedPoint = 0;
            Smooth = 5f;
            Points = new GradientPoint[0];
            Points = new GradientPoint[] { new GradientPoint(color, Vector2.one * 0.5f, 1f) };
        }

        public Gradient2D(Texture2D texture)
        {
            LastSelectedPoint = 0;
            Smooth = 5f;
            Points = new GradientPoint[0];
            Points = new GradientPoint[] { new GradientPoint(texture, Vector2.one * 0.5f, 1f) };
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
            TextureCompute renderer = ScriptableObject.CreateInstance<TextureCompute>();
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
        /// Calculates the weight used in the shader for a given point at a given location.
        /// </summary>
        public float GetPointValueAt(float x, float y, int pointNumber)
        {
            float distance = Vector2.Distance(Points[pointNumber].Position, new Vector2(x, y));
            if (distance == 0) return 1f;
            distance = Mathf.Pow(distance, Smooth);
            return 1 / (distance * Points[pointNumber].Weight);
        }

        /// <summary>
        /// Splits the point data into seperate arrays.
        /// </summary>
        private void GetPointData(out Color[] colors, out Vector2[] positions, out float[] weights)
        {
            colors = new Color[Points.Length];
            positions = new Vector2[Points.Length];
            weights = new float[Points.Length];

            for (int i = 0; i < Points.Length; i++)
            {
                colors[i] = Points[i].Color;
                positions[i] = Points[i].Position;
                weights[i] = Points[i].Weight;
            }
        }
    }
}