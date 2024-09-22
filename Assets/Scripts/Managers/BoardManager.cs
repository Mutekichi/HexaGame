using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
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

    [SerializeField] private List<TileNode> tileList = new List<TileNode>();
    [SerializeField] private float neighborDistanceThreshold = 2f / Mathf.Sqrt(3) + 0.01f;

    private void Awake()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        Debug.Log($"Tiles found: {tiles.Length}");

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