using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TriangleTileBehaviour;

public class StageManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject TriangleTilePrefab;
    [SerializeField] private GameObject frameSpritePrefab;

    [Header("Texts")]
    [SerializeField] private GameObject targetSteps1;
    [SerializeField] private GameObject targetSteps2;
    [SerializeField] private GameObject targetSteps3;
    [SerializeField] private GameObject currentSteps;
    [SerializeField] private GameObject CurrentStarCount;
    [SerializeField] private GameObject StarForChallengeMode;

    [Header("Boards")]
    [SerializeField] private GameObject PlayerBoardInstance;
    [SerializeField] private GameObject TargetBoardInstance;
    private StageData currentStageData;
    private GameUIManager gameUIManager;
    public StageLogic.Board playerBoard;
    public List<GameObject> playerTiles;
    private BitArray targetPatternBitArray;
    private bool IsPuzzleComplete { get; set; } = false;
    private static readonly float distanceBetweenTileCenters = 2 / Mathf.Sqrt(3);
    private static readonly float targetBoardSize = 6;
    private static readonly float playerBoardSize = 12;
    private int maxSteps = 999;
    private int steps = 0;
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
        InitializeStage();
    }
    void InitializeStage()
    {
        currentStageData = StageDataManager.Instance.GetCurrentStageData();
        if (currentStageData == null)
        {
            Debug.LogError("Stage data is null");
            return;
        }
        CheckIsBoardValid();
        MakeTargetPatternBitArray();
        ShowTexts();
        InitializeGameUIManager();
        InitializeBoard();
        TriangleTileBehaviour.OnBoardStateChanged += CheckBoardState;
        UpdateForChallengeMode();
    }

    void OnDestroy()
    {
        TriangleTileBehaviour.OnBoardStateChanged -= CheckBoardState;
    }

    public void OnClickTile(int tileIndex)
    {
        if (tileIndex == -1) return;

        IncrementSteps();

        StageManager stageManager = FindObjectOfType<StageManager>();
        if (stageManager == null || stageManager.playerBoard == null || stageManager.playerTiles == null) return;

        if (CheckAllNeighborsNotFlipping(tileIndex))
        {
            StageLogic.Tile currentTile = stageManager.playerBoard.tiles[tileIndex];
            foreach (int neighborIndex in currentTile.neighbors)
            {
                if (neighborIndex != -1 && neighborIndex < stageManager.playerTiles.Count)
                {
                    GameObject neighborObject = stageManager.playerTiles[neighborIndex];
                    TriangleTileBehaviour neighborTile = neighborObject.GetComponent<TriangleTileBehaviour>();
                    if (neighborTile != null)
                    {
                        neighborTile.StartFlip();
                    }
                }
            }
        }
    }

    private static bool CheckAllNeighborsNotFlipping(int tileIndex)
    {
        StageManager stageManager = FindObjectOfType<StageManager>();
        if (stageManager == null || stageManager.playerBoard == null) return false;

        StageLogic.Tile currentTile = stageManager.playerBoard.tiles[tileIndex];
        foreach (int neighborIndex in currentTile.neighbors)
        {
            if (neighborIndex != -1 && neighborIndex < stageManager.playerTiles.Count)
            {
                GameObject neighborObject = stageManager.playerTiles[neighborIndex];
                TriangleTileBehaviour neighborTile = neighborObject.GetComponent<TriangleTileBehaviour>();
                if (neighborTile != null && neighborTile.flipState != FlipState.NotFlipping)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private void InitializeGameUIManager()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
        if (gameUIManager == null)
        {
            Debug.LogError("GameUIManager not found");
        }
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

        }
    }

    private void CheckIsBoardValid()
    {
        if (currentStageData.initialPattern.Length != currentStageData.height)
        {
            Debug.LogError("Invalid currentStageData.height of initial pattern");
        }
        for (int i = 0; i < currentStageData.height; i++)
        {
            if (currentStageData.initialPattern[i].Length != currentStageData.width)
            {
                Debug.LogError("Invalid currentStageData.width of initial pattern");
            }
        }
        for (int i = 0; i < currentStageData.height; ++i)
        {
            for (int j = 0; j < currentStageData.width; ++j)
            {
                if (currentStageData.initialPattern[i][j] != '0' && currentStageData.initialPattern[i][j] != '1' && currentStageData.initialPattern[i][j] != '2')
                {
                    Debug.LogError("Invalid initial pattern");
                }
                if (currentStageData.targetPattern[i][j] != '0' && currentStageData.targetPattern[i][j] != '1' && currentStageData.targetPattern[i][j] != '2')
                {
                    Debug.LogError("Invalid target pattern");
                }
                if ((currentStageData.initialPattern[i][j] == '0' && currentStageData.targetPattern[i][j] != '0') ||
                    (currentStageData.initialPattern[i][j] != '0' && currentStageData.targetPattern[i][j] == '0'))
                {
                    Debug.LogError("Invalid initial and target pattern");
                }
            }
        }
    }

    private void InitializeBoard()
    {
        StageLogic.CellExpression playerBoardCellExpression = new StageLogic.CellExpression(
            currentStageData.height,
            currentStageData.width,
            currentStageData.isTopLeftTriangleDownward,
            StageLogic.GetCellStateStringsFromStringExpression(currentStageData.initialPattern, currentStageData.height, currentStageData.width)
        );

        StageLogic.CellExpression targetBoardCellExpression = new StageLogic.CellExpression(
            currentStageData.height,
            currentStageData.width,
            currentStageData.isTopLeftTriangleDownward,
            StageLogic.GetCellStateStringsFromStringExpression(currentStageData.targetPattern, currentStageData.height, currentStageData.width)
        );

        PlaceBoardFromCellExpression(
            targetBoardCellExpression,
            Vector3.zero,
            targetBoardSize,
            targetBoardSize,
            TargetBoardInstance,
            false
        );

        playerTiles = PlaceBoardFromCellExpression(
            playerBoardCellExpression,
            Vector3.zero,
            playerBoardSize,
            playerBoardSize,
            PlayerBoardInstance,
            true
        );

        playerBoard = StageLogic.CellExpression.GenerateBoard(playerBoardCellExpression);
        GenerateFrame();
    }

    private List<GameObject> PlaceBoardFromCellExpression(StageLogic.CellExpression cellExpression, Vector3 center, float height, float width, GameObject objectToAttach, bool isTileClickable = true)
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
                if (cellExpression.cells[ih, iw] == StageLogic.CellState.Empty) continue;

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
                    objectToAttach,
                    isTileClickable
                );
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    private GameObject PlaceTriangleTile(Vector3 position, float scale, bool isUpward, bool isFront, int tileIndex, GameObject objectToAttach, bool isTileClickable)
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
            tileBehaviour.IsClickable = isTileClickable;
        }
        return triangleTile;
    }

    private static BoardScaleInfo GetBoardScaleAndOrigin(float boardWidthByTileUnit, float boardHeightByTileUnit, float maxBoardWidth, float maxBoardHeight)
    {
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
        GeneratePlayerBoardFrame();
        GenerateTargetBoardFrame();
    }
    private void GeneratePlayerBoardFrame()
    {
        if (playerBoard == null || playerTiles == null) return;

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
    private void GenerateTargetBoardFrame()
    {
        if (playerBoard == null) return;

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

    private bool IsOuterEdge(StageLogic.Tile tile, EdgeDirectionBetweenTiles direction)
    {
        int neighborIndex = direction switch
        {
            EdgeDirectionBetweenTiles.UpRight or EdgeDirectionBetweenTiles.DownRight => tile.neighbors[0],
            EdgeDirectionBetweenTiles.UpLeft or EdgeDirectionBetweenTiles.DownLeft => tile.neighbors[1],
            EdgeDirectionBetweenTiles.Up or EdgeDirectionBetweenTiles.Down => tile.neighbors[2],
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

    private void ShowTexts()
    {
        targetSteps3.GetComponent<UnityEngine.UI.Text>().text = currentStageData.starCondition.toGet3Stars.ToString();
        targetSteps2.GetComponent<UnityEngine.UI.Text>().text = currentStageData.starCondition.toGet2Stars.ToString();
        targetSteps1.GetComponent<UnityEngine.UI.Text>().text = "∞";
    }

    private void OnPuzzleComplete()
    {
        if (IsPuzzleComplete) return;
        IsPuzzleComplete = true;

        if (StageDataManager.Instance.IsChallengeMode())
        {
            int stars;
            if (steps <= currentStageData.starCondition.toGet3Stars)
            {
                stars = 3;
            }
            else if (steps <= currentStageData.starCondition.toGet2Stars)
            {
                stars = 2;
            }
            else
            {
                stars = 1;
            }

            ChallengeManager.Instance.OnStageComplete(stars);
        }
        else
        {

            if (steps <= currentStageData.starCondition.toGet3Stars)
            {
                gameUIManager.ShowStageClearWindow(3);
            }
            else if (steps <= currentStageData.starCondition.toGet2Stars)
            {
                gameUIManager.ShowStageClearWindow(2);
            }
            else
            {
                gameUIManager.ShowStageClearWindow(1);
            }
        }
    }

    private static void IncrementSteps()
    {
        StageManager loadableStageManager = FindObjectOfType<StageManager>();
        if (loadableStageManager == null) return;

        if (loadableStageManager.steps < loadableStageManager.maxSteps)
        {
            loadableStageManager.steps++;
        }
        loadableStageManager.currentSteps.GetComponent<UnityEngine.UI.Text>().text = "steps: " + loadableStageManager.steps;
    }
    private void UpdateCurrentStarCount()
    {
        CurrentStarCount.GetComponent<UnityEngine.UI.Text>().text = "× " + ChallengeManager.Instance.GetTotalStars();
    }
    private void UpdateForChallengeMode()
    {
        if (ChallengeManager.Instance != null)
        {
            UpdateCurrentStarCount();
        }
        if (CurrentStarCount != null)
        {
            CurrentStarCount.SetActive(StageDataManager.Instance.IsChallengeMode());
        }
        if (StarForChallengeMode != null)
        {
            StarForChallengeMode.SetActive(StageDataManager.Instance.IsChallengeMode());
        }
    }

    public void SetTargetPattern(BitArray pattern)
    {
        targetPatternBitArray = pattern;
    }

    private void MakeTargetPatternBitArray()
    {
        BitArray __targetPatternBitArray = new BitArray(0);
        for (int i = currentStageData.height - 1; i >= 0; i--)
        {
            for (int j = 0; j < currentStageData.width; j++)
            {
                if (currentStageData.targetPattern[i][j] == '0') continue;

                __targetPatternBitArray.Length++;
                __targetPatternBitArray[__targetPatternBitArray.Length - 1] = currentStageData.targetPattern[i][j] == '1';
            }
        }
        targetPatternBitArray = __targetPatternBitArray;
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
            sb.Append(bits[i] ? '1' : '0');
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