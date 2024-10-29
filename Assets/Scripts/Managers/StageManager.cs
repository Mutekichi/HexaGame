using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    public int stageNumber;
    [SerializeField] GameObject TriangleTilePrefab;
    [SerializeField] private GameObject frameSpritePrefab;
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

    private enum EdgeDirectionBetweenTiles
    {
        Up,
        UpRight,
        DownRight,
        Down,
        DownLeft,
        UpLeft
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
    private static readonly float distanceBetweenTileCenters = 2 / Mathf.Sqrt(3);

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
        else
        {
            DebugBitArray(board.boardState, "Current Board State");
            DebugBitArray(targetPatternBitArray, "Target Pattern");
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
        Debug.Log("Puzzle Complete!");
    }

    public void SetTargetPattern(BitArray pattern)
    {
        targetPatternBitArray = pattern;
    }

    private void MakeTargetPatternBitArray()
    {
        BitArray __targetPatternBitArray = new BitArray(0);
        for (int i = height - 1; i >= 0; i--)
        {
            for (int j = 0; j < width; j++)
            {
                if (targetPattern[i][j] == '0')
                {
                    continue;
                }
                else if (targetPattern[i][j] == '1')
                {
                    __targetPatternBitArray.Length++;
                    __targetPatternBitArray[__targetPatternBitArray.Length - 1] = true;
                }
                else
                {
                    __targetPatternBitArray.Length++;
                    __targetPatternBitArray[__targetPatternBitArray.Length - 1] = false;
                }
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
        GenerateFrame();
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

    private void GenerateFrame()
    {
        if (PlayerBoardInstance == null || TargetBoardInstance == null) return;

        ClearBorders();

        // Player Board用のフレーム生成
        if (playerBoard != null && playerTiles != null)
        {
            Dictionary<int, GameObject> tileIndexToGameObject = new Dictionary<int, GameObject>();
            for (int i = 0; i < playerTiles.Count; i++)
            {
                var tileBehaviour = playerTiles[i].GetComponent<TriangleTileBehaviour>();
                tileIndexToGameObject[tileBehaviour.GetTileIndex()] = playerTiles[i];
            }

            foreach (StageLogic.Tile tile in playerBoard.tiles)
            {
                if (!tileIndexToGameObject.TryGetValue(tile.index, out GameObject tileObject)) continue;

                Vector3 center = tileObject.transform.position;
                float scale = tileObject.GetComponent<TriangleTileBehaviour>().GetScale();

                foreach (EdgeDirectionBetweenTiles direction in tileFacingUpToDirections[tile.isUpward])
                {
                    bool isOuterEdge = IsOuterEdge(tile, direction);

                    PlaceBorderSprite(
                        center + unitEdgeDirectionToVector[direction] * distanceBetweenTileCenters * scale / 2,
                        edgeDirectionToAngle[direction],
                        scale,
                        isOuterEdge ? 1f : 0.4f,
                        PlayerBoardInstance
                    );
                }
            }
        }

        // Target Board用のフレーム生成
        if (playerBoard != null)  // playerBoardの構造を使用
        {
            var targetTiles = new List<GameObject>();
            for (int i = 0; i < TargetBoardInstance.transform.childCount; i++)
            {
                targetTiles.Add(TargetBoardInstance.transform.GetChild(i).gameObject);
            }

            Dictionary<int, GameObject> tileIndexToGameObject = new Dictionary<int, GameObject>();
            for (int i = 0; i < targetTiles.Count; i++)
            {
                var tileBehaviour = targetTiles[i].GetComponent<TriangleTileBehaviour>();
                tileIndexToGameObject[tileBehaviour.GetTileIndex()] = targetTiles[i];
            }

            foreach (StageLogic.Tile tile in playerBoard.tiles)
            {
                if (!tileIndexToGameObject.TryGetValue(tile.index, out GameObject tileObject)) continue;

                Vector3 center = tileObject.transform.position;
                float scale = tileObject.GetComponent<TriangleTileBehaviour>().GetScale();

                foreach (EdgeDirectionBetweenTiles direction in tileFacingUpToDirections[tile.isUpward])
                {
                    bool isOuterEdge = IsOuterEdge(tile, direction);

                    PlaceBorderSprite(
                        center + unitEdgeDirectionToVector[direction] * distanceBetweenTileCenters * scale / 2,
                        edgeDirectionToAngle[direction],
                        scale,
                        isOuterEdge ? 1f : 0.4f,
                        TargetBoardInstance
                    );
                }
            }
        }
    }
    private bool IsOuterEdge(StageLogic.Tile tile, EdgeDirectionBetweenTiles direction)
    {
        // neighborのインデックスを取得
        int neighborIndex = direction switch
        {
            EdgeDirectionBetweenTiles.UpRight or EdgeDirectionBetweenTiles.DownRight => tile.neighbors[0],  // 右
            EdgeDirectionBetweenTiles.UpLeft or EdgeDirectionBetweenTiles.DownLeft => tile.neighbors[1],    // 左
            EdgeDirectionBetweenTiles.Up or EdgeDirectionBetweenTiles.Down => tile.neighbors[2],            // 上/下
            _ => -1
        };

        return neighborIndex == -1;
    }

    private void PlaceBorderSprite(Vector3 center, float angle, float scale, float alpha, GameObject objectToAttach)
    {
        string framesName = objectToAttach == PlayerBoardInstance ? "PlayerFrames" : "TargetFrames";
        Transform framesParent = objectToAttach.transform.Find(framesName);
        if (framesParent == null)
        {
            var framesObject = new GameObject(framesName);
            framesParent = framesObject.transform;
            framesParent.SetParent(objectToAttach.transform);
            framesParent.localPosition = Vector3.zero;
        }

        GameObject sprite = Instantiate(frameSpritePrefab, center, Quaternion.identity, framesParent);
        sprite.transform.Rotate(Vector3.forward, angle);
        sprite.transform.localScale = sprite.transform.localScale * scale;

        SpriteRenderer spriteRenderer = sprite.GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private void ClearBorders()
    {
        // PlayerFramesのクリア
        Transform playerFrames = PlayerBoardInstance?.transform.Find("PlayerFrames");
        if (playerFrames != null)
        {
            if (Application.isPlaying)
            {
                Destroy(playerFrames.gameObject);
            }
            else
            {
                DestroyImmediate(playerFrames.gameObject);
            }
        }

        // TargetFramesのクリア
        Transform targetFrames = TargetBoardInstance?.transform.Find("TargetFrames");
        if (targetFrames != null)
        {
            if (Application.isPlaying)
            {
                Destroy(targetFrames.gameObject);
            }
            else
            {
                DestroyImmediate(targetFrames.gameObject);
            }
        }
    }
    public static void DebugBitArray(BitArray bits, string label = null, int groupSize = 8)
    {
        if (bits == null)
        {
            Debug.Log("BitArray is null");
            return;
        }

        var sb = new StringBuilder(bits.Length + (bits.Length / groupSize));

        for (int i = 0; i < bits.Length; i++)
        {
            // 1か0を追加
            sb.Append(bits[i] ? '1' : '0');

            // グループサイズごとにスペースを追加（最後以外）
            if (i < bits.Length - 1 && (i + 1) % groupSize == 0)
            {
                sb.Append(' ');
            }
        }

        if (string.IsNullOrEmpty(label))
        {
            Debug.Log($"BitArray: {sb}");
        }
        else
        {
            Debug.Log($"{label}: {sb}");
        }
    }
}