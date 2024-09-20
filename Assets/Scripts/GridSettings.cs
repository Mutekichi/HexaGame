using UnityEngine;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Custom/Grid Settings")]
public class GridSettings : ScriptableObject
{
    public float cellSize = 1f;
    public int gridSize = 20;
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
}