using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static readonly float CellSize = 2f;
    public static readonly float distanceBetweenTileCenters = CellSize / Mathf.Sqrt(3);
    [SerializeField] private GameObject frameSpritePrefab;
    [System.Serializable]
    public struct TileNode
    {
        public int index;
        public List<int> neighbors;
        public Vector3 position;
        public GameObject tileObject;

        public TileNode(int index, Vector3 position, GameObject tileObject)
        {
            this.index = index;
            this.position = position;
            this.tileObject = tileObject;
            this.neighbors = new List<int>();
        }
    }

    private enum EdgeDirectionBetweenTiles
    {
        Up,
        UpRight,
        DownRight,
        Down,
        DownLeft,
        UpLeft

    }
    private EdgeDirectionBetweenTiles getOppositeDirection(EdgeDirectionBetweenTiles direction)
    {
        switch (direction)
        {
            case EdgeDirectionBetweenTiles.Up:
                return EdgeDirectionBetweenTiles.Down;
            case EdgeDirectionBetweenTiles.UpRight:
                return EdgeDirectionBetweenTiles.DownLeft;
            case EdgeDirectionBetweenTiles.DownRight:
                return EdgeDirectionBetweenTiles.UpLeft;
            case EdgeDirectionBetweenTiles.Down:
                return EdgeDirectionBetweenTiles.Up;
            case EdgeDirectionBetweenTiles.DownLeft:
                return EdgeDirectionBetweenTiles.UpRight;
            case EdgeDirectionBetweenTiles.UpLeft:
                return EdgeDirectionBetweenTiles.DownRight;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
    private Dictionary<bool, List<EdgeDirectionBetweenTiles>> tileFacingUpToDirections = new Dictionary<bool, List<EdgeDirectionBetweenTiles>>
    {
        {true, new List<EdgeDirectionBetweenTiles> { EdgeDirectionBetweenTiles.Down, EdgeDirectionBetweenTiles.UpRight, EdgeDirectionBetweenTiles.UpLeft }},
        {false, new List<EdgeDirectionBetweenTiles> { EdgeDirectionBetweenTiles.Up, EdgeDirectionBetweenTiles.DownRight, EdgeDirectionBetweenTiles.DownLeft }}
    };
    private Dictionary<EdgeDirectionBetweenTiles, Vector3> unitEdgeDirectionToVector = new Dictionary<EdgeDirectionBetweenTiles, Vector3>
    {
        {EdgeDirectionBetweenTiles.Up, new Vector3(0, 1, 0)},
        {EdgeDirectionBetweenTiles.UpRight, new Vector3(Mathf.Sqrt(3) / 2, 0.5f, 0)},
        {EdgeDirectionBetweenTiles.DownRight, new Vector3(Mathf.Sqrt(3) / 2, -0.5f, 0)},
        {EdgeDirectionBetweenTiles.Down, new Vector3(0, -1, 0)},
        {EdgeDirectionBetweenTiles.DownLeft, new Vector3(-Mathf.Sqrt(3) / 2, -0.5f, 0)},
        {EdgeDirectionBetweenTiles.UpLeft, new Vector3(-Mathf.Sqrt(3) / 2, 0.5f, 0)}
    };
    private Dictionary<EdgeDirectionBetweenTiles, float> edgeDirectionToAngle = new Dictionary<EdgeDirectionBetweenTiles, float>
    {
        {EdgeDirectionBetweenTiles.Up, 90},
        {EdgeDirectionBetweenTiles.UpRight, 30},
        {EdgeDirectionBetweenTiles.DownRight, -30},
        {EdgeDirectionBetweenTiles.Down, -90},
        {EdgeDirectionBetweenTiles.DownLeft, -150},
        {EdgeDirectionBetweenTiles.UpLeft, 150}
    };
    private struct QueueElementForPlacingBorder
    {
        public int tileIndex;
        public bool isUpward;
        public List<EdgeDirectionBetweenTiles> directionsToVisit;
    }

    [SerializeField] private List<TileNode> tileList = new List<TileNode>();
    [SerializeField] private float neighborDistanceThreshold = 2f / Mathf.Sqrt(3) + 0.01f;

    private void Awake()
    {
        GenerateBoard();
    }
    [ContextMenu("Generate Frame")]
    private void GenerateFrame()
    {
        GenerateBoard();
        ClearBorders();

        for (int i = 0; i < tileList.Count; ++i)
        {
            Vector3 center = GetTilePosition(i);
            bool isUpward = IsTileFacingUp(i);
            foreach (EdgeDirectionBetweenTiles direction in tileFacingUpToDirections[isUpward])
            {
                Vector3 edgeCenter = center + unitEdgeDirectionToVector[direction] * distanceBetweenTileCenters;
                bool isOuterEdge = GetTileIndex(edgeCenter) == -1;
                PlaceBorderSprite(
                    center + unitEdgeDirectionToVector[direction] * distanceBetweenTileCenters / 2, 
                    edgeDirectionToAngle[direction],
                    isOuterEdge ? 1f : 0.4f
                );
            }
        }
    }

    private void PlaceBorderSprite(Vector3 center, float angle, float alpha = 1f)
    {
        Transform framesParent = GameObject.Find("Frames")?.transform ?? new GameObject("Frames").transform;
        GameObject sprite = Instantiate(frameSpritePrefab, center, Quaternion.identity, framesParent);
        sprite.transform.Rotate(Vector3.forward, angle);

        SpriteRenderer spriteRenderer = sprite.GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
    private void ClearBorders()
    {
        GameObject[] sprites = GameObject.FindGameObjectsWithTag("Border");
        foreach (GameObject sprite in sprites)
        {
            // editor only
            DestroyImmediate(sprite);
        }
    }
    private void GenerateBoard()
    {
        tileList.Clear();
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        for (int i = 0; i < tiles.Length; i++)
        {
            Vector3 pos = tiles[i].transform.position;
            tileList.Add(new TileNode(i, pos, tiles[i]));
        }

        for (int i = 0; i < tileList.Count - 1; i++)
        {
            for (int j = i + 1; j < tileList.Count; j++)
            {
                if (AreNeighbors(tileList[i], tileList[j]))
                {
                    tileList[i].neighbors.Add(j);
                    tileList[j].neighbors.Add(i);
                }
            }
        }
    }

    private bool AreNeighbors(TileNode a, TileNode b)
    {
        return Vector3.Distance(a.position, b.position) < neighborDistanceThreshold;
    }

    public int GetTileIndex(Vector3 pos)
    {
        for (int i = 0; i < tileList.Count; i++)
        {
            if (Vector3.Distance(tileList[i].position, pos) < 0.01f)
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsTileFacingUp(int tileIndex)
    {
        return tileList[tileIndex].tileObject.GetComponent<TriangleTileBehaviour>().isUpward;
    }

    public List<GameObject> GetNeighborsFromTilePosition(Vector3 pos)
    {
        int tileIndex = GetTileIndex(pos);
        if (tileIndex == -1)
        {
            return null;
        }

        List<GameObject> neighbors = new List<GameObject>();
        foreach (int neighborIndex in tileList[tileIndex].neighbors)
        {
            neighbors.Add(tileList[neighborIndex].tileObject);
        }
        return neighbors;
    }
    public int GetTileCount()
    {
        return tileList.Count;
    }

    public Vector3 GetTilePosition(int tileIndex)
    {
        if (tileIndex >= 0 && tileIndex < tileList.Count)
        {
            return tileList[tileIndex].position;
        }
        return Vector3.zero;
    }
}