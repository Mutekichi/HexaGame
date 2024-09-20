using UnityEngine;
using UnityEditor;
using System;

[InitializeOnLoad]
public class HexGridEditor
{
    static HexGridEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseMove && e.shift)
        {
            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            Vector3 snappedPosition = HexGridUtility.SnapToHexGrid(mousePosition);

            if (Selection.activeGameObject != null)
            {
                Undo.RecordObject(Selection.activeGameObject.transform, "Move to Grid");
                Selection.activeGameObject.transform.position = snappedPosition;
            }
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
        float height = HexGridUtility.CellSize * HexGridUtility.Sqrt3 * 0.5f;
        int gridSize = 20;

        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        for (int y = -gridSize; y <= gridSize; y++)
        {
            float xOffset = (y % 2 == 0) ? 0 : HexGridUtility.CellSize * 0.5f;
            for (int x = -gridSize; x <= gridSize; x++)
            {
                Vector3 center = new Vector3(x * HexGridUtility.CellSize + xOffset, y * height, 0);
                DrawTriangle(center, HexGridUtility.CellSize);
            }
        }
    }

    static void DrawTriangle(Vector3 center, float size)
    {
        Vector3 top = center + new Vector3(0, size * 0.577f, 0);
        Vector3 bottomLeft = center + new Vector3(-size * 0.5f, -size * 0.289f, 0);
        Vector3 bottomRight = center + new Vector3(size * 0.5f, -size * 0.289f, 0);

        Handles.DrawLine(top, bottomLeft);
        Handles.DrawLine(bottomLeft, bottomRight);
        Handles.DrawLine(bottomRight, top);
    }
}