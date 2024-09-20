using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class TriangleGridSnapping
{
    static readonly float Sqrt3 = Mathf.Sqrt(3);
    static readonly float CellSize = 2f;
    static TriangleGridSnapping()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseMove && e.shift)
        {
            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            Vector3 snappedPosition = SnapToHexGrid(mousePosition);

            if (Selection.activeGameObject != null)
            {
                Undo.RecordObject(Selection.activeGameObject.transform, "Move to Grid");
                Selection.activeGameObject.transform.position = snappedPosition;
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
        float height = CellSize * Sqrt3 * 0.5f;
        int gridSize = 20;

        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        for (int y = -gridSize; y <= gridSize; y++)
        {
            float xOffset = (y % 2 == 0) ? 0 : CellSize * 0.5f;
            for (int x = -gridSize; x <= gridSize; x++)
            {
                Vector3 center = new Vector3(x * CellSize + xOffset, y * height, 0);
                DrawTriangle(center, CellSize);
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

    static Vector3 SnapToHexGrid(Vector3 pos) {
        Vector2 obliqPos = ConvertCartesianToOblique(new Vector2(pos.x, pos.y));
        Vector2 normalizesObliqPos = new Vector2(obliqPos.x / CellSize, obliqPos.y / CellSize);

        float x_floor = Mathf.Floor(normalizesObliqPos.x);
        float y_floor = Mathf.Floor(normalizesObliqPos.y);

        float x_diff = normalizesObliqPos.x - x_floor;
        float y_diff = normalizesObliqPos.y - y_floor;

        float x_offset;
        float y_offset;

        if (x_diff + y_diff < 1f/3f) {
            x_offset = 0f;
            y_offset = 0f;
        } else if (x_diff + y_diff > 5/3f) {
            x_offset = 1f;
            y_offset = 1f;
        } else if (x_diff < 1f/3f && y_diff > 2f/3f) {
            x_offset = 0f;
            y_offset = 1f;
        } else if (x_diff > 2f/3f && y_diff < 1f/3f) {
            x_offset = 1f;
            y_offset = 0f;
        } else if (x_diff + y_diff < 1f) {
            x_offset = 1/3f;
            y_offset = 1/3f;
        } else{
            x_offset = 2/3f;
            y_offset = 2/3f;
        }

        Vector2 snappedOblique = new Vector2((x_floor + x_offset) * CellSize, (y_floor + y_offset) * CellSize);
        Vector2 snappedCartesian = ConvertObliqueToCartesian(snappedOblique);
        return new Vector3(snappedCartesian.x, snappedCartesian.y, pos.z);
    }

    private static Vector2 ConvertCartesianToOblique(Vector2 cartesian) {
        float x_cart = cartesian.x;
        float y_cart = cartesian.y;

        float x_obliq = x_cart - y_cart / Sqrt3;
        float y_obliq = y_cart * 2f / Sqrt3;

        return new Vector2(x_obliq, y_obliq);
    }

    private static Vector2 ConvertObliqueToCartesian(Vector2 oblique) {
        float x_obliq = oblique.x;
        float y_obliq = oblique.y;

        float x_cart = x_obliq + y_obliq * 0.5f;
        float y_cart = y_obliq * Sqrt3 / 2f;

        return new Vector2(x_cart, y_cart);
    }
}