using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{
    internal class OceanGradientUI : ScriptableObject
    {
        Texture2D _texture;
        PreviewPlanet _planet;
        PlanetCreatorTool _mainUI;

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

        public void GenerateTexture()
        {
            _texture = _planet.Data.OceanGradient.GetTexture(256, 256);
            _mainUI.Styles["OceanTexture"].normal.background = _texture;
            _mainUI.UpdateUI();
        }

        Rect ShowGradientField()
        {
            // Check for change values
            float smooth = _planet.Data.OceanGradient.Smooth;
            float refleciveness = _planet.Data.OceanReflectiveness;

            GUILayout.BeginVertical();
            // Title
            GUILayout.Label("Ocean gradient");
            GUILayout.Space(10);

            // Graph
            GUILayout.Label("Depth");
            GUILayout.Label("", _mainUI.Styles["OceanTexture"], GUILayout.MinWidth(150), GUILayout.MinHeight(100));
            Rect textureRect = GUILayoutUtility.GetLastRect();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Temperature");
            GUILayout.EndHorizontal();

            // Smoothing Menu
            GUILayout.Label("Gradient smoothing");
            _planet.Data.OceanGradient.Smooth = GUILayout.HorizontalSlider(_planet.Data.OceanGradient.Smooth, 0.1f, 10f, GUILayout.Height(20));
            GUILayout.Label("Reflection");
            _planet.Data.OceanReflectiveness = GUILayout.HorizontalSlider(_planet.Data.OceanReflectiveness, 0.5f, 1f, GUILayout.Height(20));

            if (_planet.Data.OceanGradient.Smooth != smooth || _planet.Data.OceanReflectiveness != refleciveness) GenerateTexture();

            // Color points on graph
            for (int i = 0; i < _planet.Data.OceanGradient.Points.Length; i++)
            {
                GradientPoint point = _planet.Data.OceanGradient.Points[i];
                int size = i == _planet.Data.OceanGradient.LastSelectedPoint ? 30 : 20;
                GUI.backgroundColor = Color.Lerp(point.Color, Color.white, 0.2f);
                if (GUI.Button(new Rect(
                    textureRect.x + textureRect.width * point.Position.x - size / 2,
                    textureRect.y + textureRect.height * (1 - point.Position.y) - size / 2,
                    size, size), "", _mainUI.Styles["IndicatorFinished"]))
                {
                    _planet.Data.OceanGradient.LastSelectedPoint = i;
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
            if (0 <= _planet.Data.OceanGradient.LastSelectedPoint)
            {
                // Check for change values
                GradientPoint point = _planet.Data.OceanGradient.Points[_planet.Data.OceanGradient.LastSelectedPoint];
                GradientPoint prevPoint = new GradientPoint(point.Color, point.Position, point.Weight);

                // Gradient point properties
                GUILayout.Label("Color:");
                point.Color = EditorGUILayout.ColorField(point.Color);
                GUILayout.Label("Weight:");
                point.Weight = GUILayout.HorizontalSlider(point.Weight, 0, 5, GUILayout.Height(20));
                // Remove selected point
                if (1 < _planet.Data.OceanGradient.Points.Length && GUILayout.Button("-", GUILayout.Width(50)))
                {
                    List<GradientPoint> gradientPoints = new List<GradientPoint>(_planet.Data.OceanGradient.Points);
                    gradientPoints.RemoveAt(_planet.Data.OceanGradient.LastSelectedPoint);
                    _planet.Data.OceanGradient.Points = gradientPoints.ToArray();
                    _planet.Data.OceanGradient.LastSelectedPoint = -1;
                    GenerateTexture();
                }
                else
                {
                    // If not removed check for changes and apply new properties
                    _planet.Data.OceanGradient.Points[_planet.Data.OceanGradient.LastSelectedPoint] = point;
                    if (!point.Equals(prevPoint))
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
                    if (0 <= _planet.Data.OceanGradient.LastSelectedPoint)
                    {
                        List<GradientPoint> gradientPoints = new List<GradientPoint>(_planet.Data.OceanGradient.Points);

                        GradientPoint point = _planet.Data.OceanGradient.Points[_planet.Data.OceanGradient.LastSelectedPoint];
                        point.Position = positionNormalized;
                        _planet.Data.OceanGradient.Points[_planet.Data.OceanGradient.LastSelectedPoint] = point;
                    }
                    // No point selected means adding new point
                    else
                    {
                        List<GradientPoint> gradientPoints = new List<GradientPoint>(_planet.Data.OceanGradient.Points);
                        gradientPoints.Add(new GradientPoint(Color.white, positionNormalized, 1f));
                        _planet.Data.OceanGradient.Points = gradientPoints.ToArray();
                        _planet.Data.OceanGradient.LastSelectedPoint = gradientPoints.Count - 1;
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
                    _planet.Data.OceanGradient.LastSelectedPoint = -1;
                    GUI.changed = true;
                }
            }
        }
    }
}