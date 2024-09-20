using UnityEngine;

public static class TriangleGridUtility
{
    public static readonly float Sqrt3 = Mathf.Sqrt(3);
    public static readonly float CellSize = 2f;

    public static Vector2 ConvertCartesianToOblique(Vector2 cartesian)
    {
        float x_cart = cartesian.x;
        float y_cart = cartesian.y;

        float x_obliq = x_cart - y_cart / Sqrt3;
        float y_obliq = y_cart * 2f / Sqrt3;

        return new Vector2(x_obliq, y_obliq);
    }

    public static Vector2 ConvertObliqueToCartesian(Vector2 oblique)
    {
        float x_obliq = oblique.x;
        float y_obliq = oblique.y;

        float x_cart = x_obliq + y_obliq * 0.5f;
        float y_cart = y_obliq * Sqrt3 / 2f;

        return new Vector2(x_cart, y_cart);
    }

    public static Vector3 SnapToTriangleGrid(Vector3 pos)
    {
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
        } else {
            x_offset = 2/3f;
            y_offset = 2/3f;
        }

        Vector2 snappedOblique = new Vector2((x_floor + x_offset) * CellSize, (y_floor + y_offset) * CellSize);
        Vector2 snappedCartesian = ConvertObliqueToCartesian(snappedOblique);
        return new Vector3(snappedCartesian.x, snappedCartesian.y, pos.z);
    }
}