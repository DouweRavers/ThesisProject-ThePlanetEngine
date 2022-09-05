using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{

    internal class BiomeGradientUI : ScriptableObject
    {
        Texture2D _texture;

        PreviewPlanet _planet;
        PlanetCreatorTool _mainUI;

        [SerializeField]
        Texture2D defaultTexture;

        public void Initialize(PreviewPlanet planet, PlanetCreatorTool UI)
        {
            _planet = planet;
            _mainUI = UI;
        }

        public void OnGUI()
        {
            if (_texture == null) GenerateTexture();
            GUILayout.BeginHorizontal();
            Rect textureRect = ShowGradientField();
            Rect pointMenuRect = ShowPointProperties();
            HandleMouseInput(textureRect, pointMenuRect);
            GUILayout.EndHorizontal();
        }

        public void GenerateTexture(bool regenerate = true)
        {
            _texture = _planet.Data.BiomeGradient.GetTexture(256, 256);
            _mainUI.Styles["BiomeTexture"].normal.background = _texture;
            if (regenerate) _mainUI.UpdateUI();
        }

        Rect ShowGradientField()
        {
            // Check for change values
            float smooth = _planet.Data.BiomeGradient.Smooth;

            GUILayout.BeginVertical();
            // Title
            GUILayout.Label("Biome gradient");
            GUILayout.Space(10);

            // Graph
            GUILayout.Label("Humidity");
            GUILayout.Label("", _mainUI.Styles["BiomeTexture"], GUILayout.MinWidth(150), GUILayout.MinHeight(100));
            Rect textureRect = GUILayoutUtility.GetLastRect();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Heat");
            GUILayout.EndHorizontal();

            // Smoothing Menu
            GUILayout.Label("Gradient smoothing");
            _planet.Data.BiomeGradient.Smooth = GUILayout.HorizontalSlider(_planet.Data.BiomeGradient.Smooth, 0.1f, 10f, GUILayout.Height(20));

            if (_planet.Data.BiomeGradient.Smooth != smooth) GenerateTexture();

            // Color points on graph
            for (int i = 0; i < _planet.Data.BiomeGradient.Points.Length; i++)
            {
                GradientPoint point = _planet.Data.BiomeGradient.Points[i];
                int size = i == _planet.Data.BiomeGradient.LastSelectedPoint ? 30 : 20;
                GUI.backgroundColor = Color.Lerp(point.Color, Color.white, 0.5f);
                if (GUI.Button(new Rect(
                    textureRect.x + textureRect.width * point.Position.x - size / 2,
                    textureRect.y + textureRect.height * (1 - point.Position.y) - size / 2,
                    size, size), "", _mainUI.Styles["IndicatorFinished"]))
                {
                    _planet.Data.BiomeGradient.LastSelectedPoint = i;
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            return textureRect;
        }

        Rect ShowPointProperties()
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(100), GUILayout.ExpandHeight(true));

            // Title
            GUILayout.Label("Point menu");
            GUILayout.Space(10);

            // Menu
            if (0 <= _planet.Data.BiomeGradient.LastSelectedPoint)
            {
                // Check for change values
                GradientPoint point = _planet.Data.BiomeGradient.Points[_planet.Data.BiomeGradient.LastSelectedPoint];
                Color prevColor = point.Color;
                Vector2 prevPos = point.Position;
                float prevWeight = point.Weight;

                // Gradient point properties
                GUILayout.Label("Texture:");
                Texture2D newTex = EditorGUILayout.ObjectField(_planet.Data.BiomeGradient.Points[_planet.Data.BiomeGradient.LastSelectedPoint].GetTexture(), typeof(Texture2D), false) as Texture2D;
                if (newTex != null && !newTex.Equals(point.GetTexture())) point.SetTexture(newTex);
                GUILayout.Label("Weight:");
                point.Weight = GUILayout.HorizontalSlider(point.Weight, 0, 5, GUILayout.Height(20));

                // Remove selected point
                if (1 < _planet.Data.BiomeGradient.Points.Length && GUILayout.Button("-", GUILayout.Width(50)))
                {
                    List<GradientPoint> gradientPoints = new List<GradientPoint>(_planet.Data.BiomeGradient.Points);
                    gradientPoints.RemoveAt(_planet.Data.BiomeGradient.LastSelectedPoint);
                    _planet.Data.BiomeGradient.Points = gradientPoints.ToArray();
                    _planet.Data.BiomeGradient.LastSelectedPoint = -1;
                    GenerateTexture();
                }
                else
                {
                    // If not removed check for changes and apply new properties
                    _planet.Data.BiomeGradient.Points[_planet.Data.BiomeGradient.LastSelectedPoint] = point;
                    if (point.Color != prevColor || point.Position != prevPos || point.Weight != prevWeight)
                    {
                        GenerateTexture();
                    }
                }
            }
            else
            {
                GUILayout.Label("Click to add a point.");
            }
            GUILayout.EndVertical();
            Rect pointMenuRect = GUILayoutUtility.GetLastRect();
            return pointMenuRect;
        }

        void HandleMouseInput(Rect textureRect, Rect pointMenuRect)
        {
            // Catch events
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                // Check if mouse press is over the gradient graph area
                if (textureRect.x < e.mousePosition.x && e.mousePosition.x < textureRect.x + textureRect.width &&
                textureRect.y < e.mousePosition.y && e.mousePosition.y < textureRect.y + textureRect.height)
                {
                    Vector2 positionNormalized = new Vector2((e.mousePosition.x - textureRect.x) / textureRect.width, (textureRect.y + textureRect.height - e.mousePosition.y) / textureRect.height);
                    // If point was previously selected apply new position
                    if (0 <= _planet.Data.BiomeGradient.LastSelectedPoint)
                    {
                        GradientPoint point = _planet.Data.BiomeGradient.Points[_planet.Data.BiomeGradient.LastSelectedPoint];
                        point.Position = positionNormalized;
                        _planet.Data.BiomeGradient.Points[_planet.Data.BiomeGradient.LastSelectedPoint] = point;
                    }
                    // No point selected means adding new point
                    else
                    {
                        List<GradientPoint> gradientPoints = new List<GradientPoint>(_planet.Data.BiomeGradient.Points);
                        gradientPoints.Add(new GradientPoint(Resources.Load<Texture2D>("Presets/white"), positionNormalized, 1f));
                        _planet.Data.BiomeGradient.Points = gradientPoints.ToArray();
                        _planet.Data.BiomeGradient.LastSelectedPoint = gradientPoints.Count - 1;
                    }
                    GenerateTexture();
                    GUI.changed = true;
                }
                // Check if mouse is over point menu
                else if (pointMenuRect.x < e.mousePosition.x && e.mousePosition.x < pointMenuRect.x + pointMenuRect.width &&
              pointMenuRect.y < e.mousePosition.y && e.mousePosition.y < pointMenuRect.y + pointMenuRect.height)
                {
                    // Do not unselect points when adjusting point properties
                }
                // When mousepress is not on focus area
                else
                {
                    _planet.Data.BiomeGradient.LastSelectedPoint = -1;
                    GUI.changed = true;
                }
            }
        }
    }
}