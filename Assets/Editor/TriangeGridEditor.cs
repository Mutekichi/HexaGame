using UnityEngine;
using UnityEditor;
using System;

[InitializeOnLoad]
public class TriangleGridEditor
{
    static TriangleGridEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseMove && e.shift)
        {
            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            Vector3 snappedPosition = TriangleGridUtility.GetSnappedPosOnTriangleGrid(mousePosition);

            // Move the selected game object to the snapped position
            if (Selection.activeGameObject != null)
            {
                Undo.RecordObject(Selection.activeGameObject.transform, "Move to Grid");
                Selection.activeGameObject.transform.position = snappedPosition;
            }
            // Draw a wire cube at the snapped position
            else
            {
                Handles.color = Color.yellow;
                Handles.DrawWireCube(snappedPosition, Vector3.one * 0.3f);
            }

            HandleUtility.Repaint();
        }

        if (e.type == EventType.Repaint)
        {
            DrawTriangleGrid(sceneView);
        }
    }

    static void DrawTriangleGrid(SceneView sceneView)
    {
        float height = TriangleGridUtility.CellSize * TriangleGridUtility.Sqrt3 * 0.5f;
        int gridSize = 20;

        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        float xOffset = TriangleGridUtility.CellSize * 0.5f;
        float yOffset = height / 3f;

        for (int y = -gridSize; y <= gridSize; y++)
        {
            float rowXOffset = (y % 2 == 0) ? xOffset : TriangleGridUtility.CellSize;
            for (int x = -gridSize; x <= gridSize; x++)
            {
                Vector3 center = new Vector3(x * TriangleGridUtility.CellSize + rowXOffset, y * height + yOffset, 0);
                DrawTriangle(center, TriangleGridUtility.CellSize);
            }
        }
    }

    static void DrawTriangle(Vector3 center, float size)
    {
        float halfSize = size * 0.5f;
        float height = size * TriangleGridUtility.Sqrt3 * 0.5f;

        Vector3 top = center + new Vector3(0, height * (2f/3f), 0);
        Vector3 bottomLeft = center + new Vector3(-halfSize, -height * (1f/3f), 0);
        Vector3 bottomRight = center + new Vector3(halfSize, -height * (1f/3f), 0);

        Handles.DrawLine(top, bottomLeft);
        Handles.DrawLine(bottomLeft, bottomRight);
        Handles.DrawLine(bottomRight, top);
    }
}