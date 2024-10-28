using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    public int stageNumber;
    public GameObject TriangleTilePrefab;
    public GameObject PlayerBoardInstance;

    [HideInInspector]
    public StageLogic.Board playerBoard;

    [HideInInspector]
    public List<GameObject> playerTiles;
    public int height;
    public int width;
    public bool isTopLeftTriangleDownward;
    public string[] initialPattern;
    public BitArray targetPattern;

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
        CheckIsBoardValid();
        Test();
        TriangleTileBehaviour.OnBoardStateChanged += CheckBoardState;
    }

    void OnDestroy()
    {
        TriangleTileBehaviour.OnBoardStateChanged -= CheckBoardState;
    }

    private void CheckBoardState(StageLogic.Board board)
    {
        if (board.MatchesPattern(targetPattern))
        {
            OnPuzzleComplete();
        }
    }
    private void CheckIsBoardValid()
    {
        if (initialPattern.Length != height)
        {
            Debug.LogError("Invalid height of initial pattern");
        }
        for (int i = 0; i < height; i++)
        {
            if (initialPattern[i].Length != width)
            {
                Debug.LogError("Invalid width of initial pattern");
            }
        }
    }

    private void OnPuzzleComplete()
    {
        // Debug.Log("Puzzle Complete!");
    }

    public void SetTargetPattern(BitArray pattern)
    {
        targetPattern = pattern;
    }

    private void Test()
    {
        StageLogic.CellExpression cellExpression = new StageLogic.CellExpression(
            height,
            width,
            isTopLeftTriangleDownward,
            StageLogic.GetCellStateStringsFromStringExpression(
                initialPattern,
                height,
                width
            )
        );
        playerTiles = PlaceBoardFromPlaceExpression(cellExpression, new Vector3(0, 0, 0), 12f, 12f);
        playerBoard = StageLogic.CellExpression.GenerateBoard(cellExpression);
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

    private GameObject PlaceTriangleTile(Vector3 position, float scale, bool isUpward, bool isFront, int tileIndex)
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

    private static BoardScaleInfo GetBoardScaleAndOrigin(float boardWidthByTileUnit, float boardHeightByTileUnit, float maxBoardWidth, float maxBoardHeight)
    {
        // Debug.Log($"Board width by tile unit: {boardWidthByTileUnit}, board height by tile unit: {boardHeightByTileUnit}");

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