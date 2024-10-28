using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    public int stageNumber;
    public GameObject TriangleTilePrefab;
    public GameObject PlayerBoardInstance;
    public GameObject TargetBoardInstance;

    [HideInInspector]
    public StageLogic.Board playerBoard;

    [HideInInspector]
    public List<GameObject> playerTiles;
    [SerializeField] private int height;
    [SerializeField] private int width;
    [SerializeField] private bool isTopLeftTriangleDownward;
    [SerializeField] private string[] initialPattern;
    [SerializeField] private string[] targetPattern;
    private BitArray targetPatternBitArray;

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
        MakeTargetPatternBitArray();
        Test();
        TriangleTileBehaviour.OnBoardStateChanged += CheckBoardState;
    }

    void OnDestroy()
    {
        TriangleTileBehaviour.OnBoardStateChanged -= CheckBoardState;
    }

    private void CheckBoardState(StageLogic.Board board)
    {
        if (board.MatchesPattern(targetPatternBitArray))
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
        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                if (initialPattern[i][j] != '0' && initialPattern[i][j] != '1' && initialPattern[i][j] != '2')
                {
                    Debug.LogError("Invalid initial pattern");
                }
                if (targetPattern[i][j] != '0' && targetPattern[i][j] != '1' && targetPattern[i][j] != '2')
                {
                    Debug.LogError("Invalid target pattern");
                }
                if ((initialPattern[i][j] == '0' && targetPattern[i][j] != '0') ||
                    (initialPattern[i][j] != '0' && targetPattern[i][j] == '0'))
                {
                    Debug.LogError("Invalid initial and target pattern");
                }
            }
        }
    }

    private void OnPuzzleComplete()
    {
        // Debug.Log("Puzzle Complete!");
    }

    public void SetTargetPattern(BitArray pattern)
    {
        targetPatternBitArray = pattern;
    }

    private void MakeTargetPatternBitArray()
    {
        BitArray __targetPatternBitArray = new BitArray(0);
        for (int i = 0; i < __targetPatternBitArray.Length; ++i)
        {
            if (targetPattern[i] == "0")
            {
                continue;
            }
            else if (targetPattern[i] == "1")
            {
                __targetPatternBitArray.Set(i, true);
            }
            else if (targetPattern[i] == "2")
            {
                __targetPatternBitArray.Set(i, false);
            }
            else
            {
                Debug.LogError("Invalid target pattern");
            }
        }
        targetPatternBitArray = __targetPatternBitArray;
    }
    private void Test()
    {
        StageLogic.CellExpression playerBoardCellExpression = new StageLogic.CellExpression(
            height,
            width,
            isTopLeftTriangleDownward,
            StageLogic.GetCellStateStringsFromStringExpression(
                initialPattern,
                height,
                width
            )
        );
        StageLogic.CellExpression targetBoardCellExpression = new StageLogic.CellExpression(
            height,
            width,
            isTopLeftTriangleDownward,
            StageLogic.GetCellStateStringsFromStringExpression(
                targetPattern,
                height,
                width
            )
        );
        PlaceBoardFromCellExpression(targetBoardCellExpression, new Vector3(0, 0, 0), 6f, 6f, TargetBoardInstance);
        playerTiles = PlaceBoardFromCellExpression(playerBoardCellExpression, new Vector3(0, 0, 0), 12f, 12f, PlayerBoardInstance);
        playerBoard = StageLogic.CellExpression.GenerateBoard(playerBoardCellExpression);
    }

    private List<GameObject> PlaceBoardFromCellExpression(StageLogic.CellExpression cellExpression, Vector3 center, float height, float width, GameObject objectToAttach = null)
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
                    tileIndex++,
                    objectToAttach
                );
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    private GameObject PlaceTriangleTile(Vector3 position, float scale, bool isUpward, bool isFront, int tileIndex, GameObject objectToAttach = null)
    {
        GameObject triangleTile = Instantiate(TriangleTilePrefab, objectToAttach != null ? objectToAttach.transform : PlayerBoardInstance.transform);
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