using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public int stageNumber;
    public GameObject TriangleTilePrefab;
    public GameObject PlayerBoardInstance;
    public StageLogic.Board playerBoard;
    public List<GameObject> playerTiles;

    private class BoardScaleInfo
    {
        public Vector3 origin;
        public float tileUnit;

        public BoardScaleInfo(Vector3 origin, float tileUnit)
        {
            this.origin = origin;
            this.tileUnit = tileUnit;
        }
    }

    void Start()
    {
        FindPlayerBoard();
        Test();
    }

    void Update()
    {

    }

    void Test()
    {
        StageLogic.CellExpression cellExpression = new StageLogic.CellExpression(
            4,
            7,
            true,
            StageLogic.GetCellStateStringsFromStringExpression(new string[] {
                "0212120",
                "2121212",
                "1212121",
                "0000010"
            }, 4, 7)
        );
        playerTiles = PlaceBoardFromPlaceExpression(cellExpression, new Vector3(0, 0, 0), 12f, 12f);
        playerBoard = StageLogic.CellExpression.GenerateBoard(cellExpression);

        // タイルの接続関係を表示
        foreach (var tile in playerBoard.tiles)
        {
            Debug.Log($"Tile {tile.index} has neighbors: {string.Join(", ", tile.neighbors)}");
        }
    }

    private void FindPlayerBoard()
    {
        GameObject playerBoard = GameObject.Find("PlayerBoardInstance");
        Debug.Log(playerBoard ? "PlayerBoardInstance found" : "PlayerBoardInstance not found");
    }

    private List<GameObject> PlaceBoardFromPlaceExpression(StageLogic.CellExpression cellExpression, Vector3 center, float height, float width)
    {
        List<GameObject> tiles = new List<GameObject>();
        float boardWidthByTileUnit = cellExpression.width * 0.5f + 0.5f;
        float boardHeightByTileUnit = cellExpression.height * Mathf.Sqrt(3) / 2f;

        BoardScaleInfo scaleInfo = GetBoardScaleAndOrigin(boardWidthByTileUnit, boardHeightByTileUnit, width, height);

        int tileIndex = 0;

        for (int ih = cellExpression.height - 1; ih >= 0; --ih)
        {
            for (int iw = 0; iw < cellExpression.width; ++iw)
            {
                if (cellExpression.cells[ih, iw] == StageLogic.CellState.Empty)
                {
                    continue;
                }
                Vector3 offset = new Vector3(-width, -height, 0) / 2f + scaleInfo.origin;
                bool isDownwardTile = StageLogic.IsDownwardTileFromIndex(ih, iw, cellExpression.isTopLeftTriangleDownward);
                GameObject tile = PlaceTriangleTile(
                    offset + new Vector3(
                        scaleInfo.tileUnit * (iw + 1) / 2f,
                        scaleInfo.tileUnit * Mathf.Sqrt(3) / 2f * (cellExpression.height - ih - 1f + (isDownwardTile ? 2f / 3f : 1f / 3f)),
                        0
                    ),
                    scaleInfo.tileUnit / 2f,
                    !isDownwardTile,
                    cellExpression.cells[ih, iw] == StageLogic.CellState.Front,
                    tileIndex++
                );
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    private GameObject PlaceTriangleTile(Vector3 position, float scale = 1f, bool isUpward = true, bool isFront = true, int tileIndex = -1)
    {
        GameObject triangleTile = Instantiate(TriangleTilePrefab, PlayerBoardInstance.transform);
        triangleTile.transform.localPosition = position;
        triangleTile.transform.localScale = new Vector3(scale, scale, 1);

        TriangleTileBehaviour tileBehaviour = triangleTile.GetComponent<TriangleTileBehaviour>();
        if (tileBehaviour != null)
        {
            tileBehaviour.isUpward = isUpward;
            tileBehaviour.isFront = isFront;
            tileBehaviour.SetTileIndex(tileIndex);
            tileBehaviour.SetScale(scale);
        }
        return triangleTile;
    }

    private static BoardScaleInfo GetBoardScaleAndOrigin
    (
        float boardWidthByTileUnit,
        float boardHeightByTileUnit,
        float maxBoardWidth,
        float maxBoardHeight
    )
    {
        Debug.Log($"Board width by tile unit: {boardWidthByTileUnit}, board height by tile unit: {boardHeightByTileUnit}");

        if (boardWidthByTileUnit / boardHeightByTileUnit > maxBoardWidth / maxBoardHeight)
        {
            float tileUnit = maxBoardWidth / boardWidthByTileUnit;
            return new BoardScaleInfo(
                new Vector3(0, (maxBoardHeight - boardHeightByTileUnit * tileUnit) / 2),
                tileUnit
            );
        }
        else
        {
            float tileUnit = maxBoardHeight / boardHeightByTileUnit;
            return new BoardScaleInfo(
                new Vector3((maxBoardWidth - boardWidthByTileUnit * tileUnit) / 2, 0),
                tileUnit
            );
        }
    }
}