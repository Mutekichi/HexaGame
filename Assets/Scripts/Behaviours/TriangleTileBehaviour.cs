using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class TriangleTileBehaviour : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private bool _isUpward = true;
    public bool isUpward
    {
        get { return _isUpward; }
        set
        {
            if (_isUpward != value)
            {
                _isUpward = value;
                UpdateSprite();
                UpdateCollider();
            }
        }
    }
    public int tileIndex;
    public void SetTileIndex(int index)
    {
        tileIndex = index;
    }
    public int TileIndex => tileIndex;
    public float scale;
    public void SetScale(float scale)
    {
        this.scale = scale;
    }


    [SerializeField] private bool _isFront = true;
    public bool isFront
    {
        get { return _isFront; }
        set
        {
            if (_isFront != value)
            {
                _isFront = value;
                UpdateSprite();
            }
        }
    }

    [SerializeField] private SpriteRenderer upwardFrontSprite;
    [SerializeField] private SpriteRenderer downwardFrontSprite;
    [SerializeField] private SpriteRenderer upwardBackSprite;
    [SerializeField] private SpriteRenderer downwardBackSprite;
    [SerializeField] private Collider2D upwardCollider;
    [SerializeField] private Collider2D downwardCollider;

    [SerializeField] private float flipDuration = 0.3f;
    public delegate void BoardStateChangedHandler(StageLogic.Board board);
    public static event BoardStateChangedHandler OnBoardStateChanged;
    private enum FlipState
    {
        NotFlipping,
        FlippingBeforeHalf,
        FlippingAfterHalf
    }

    private float flipProgress;
    private FlipState flipState = FlipState.NotFlipping;
    private Collider2D tileCollider { get { return isUpward ? upwardCollider : downwardCollider; } }
    private StageManager stageManager;

    private void OnEnable()
    {
        UpdateSprite();
        UpdateCollider();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            // UpdatePositionAndState();
        }
        else
        {
            if (flipState != FlipState.NotFlipping)
            {
                UpdateFlip();
            }
            else
            {
                CheckForClick();
            }
        }
    }

    private void Start()
    {
        UpdateSprite();
        UpdateCollider();
        stageManager = FindObjectOfType<StageManager>();
        mainCamera = Camera.main;
    }

    private void OnValidate()
    {
        ValidateSprites();
        // UpdatePositionAndState();
    }

    private void ValidateSprites()
    {
        if (upwardFrontSprite == null || downwardFrontSprite == null || upwardBackSprite == null || downwardBackSprite == null)
        {
            Debug.LogWarning("Sprites are not set for the tile");
        }
    }

    private void CheckForClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            if (IsClickedOnTile(mousePosition))
            {
                Debug.Log("tile" + tileIndex + " clicked");
                if (CheckAllNeighborsBeforeFlip())
                {
                    FlipNeighbors();
                }
            }
        }
    }

    private bool IsClickedOnTile(Vector2 mousePosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        return hit.collider != null && hit.collider.gameObject == tileCollider.gameObject;
    }

    private void UpdateSprite()
    {
        if (upwardFrontSprite != null)
        {
            upwardFrontSprite.enabled = isUpward && isFront;
        }
        if (downwardFrontSprite != null)
        {
            downwardFrontSprite.enabled = !isUpward && isFront;
        }
        if (upwardBackSprite != null)
        {
            upwardBackSprite.enabled = isUpward && !isFront;
        }
        if (downwardBackSprite != null)
        {
            downwardBackSprite.enabled = !isUpward && !isFront;
        }
    }

    private void UpdateCollider()
    {
        if (upwardCollider != null)
        {
            upwardCollider.enabled = isUpward;
        }
        if (downwardCollider != null)
        {
            downwardCollider.enabled = !isUpward;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isUpward ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }

    private void StartFlip()
    {
        if (flipState == FlipState.NotFlipping)
        {
            flipState = FlipState.FlippingBeforeHalf;
            flipProgress = 0;
        }
    }

    private void UpdateFlip()
    {
        flipProgress += Time.deltaTime / flipDuration;
        if (flipProgress >= 1)
        {
            flipProgress = 1;
            flipState = FlipState.NotFlipping;

            if (tileIndex != -1 && stageManager != null)
            {
                stageManager.playerBoard.SetTileState(tileIndex, isFront);
                // フリップ完了時に盤面の状態変化を通知
                OnBoardStateChanged?.Invoke(stageManager.playerBoard);
            }
        }
        else if (flipProgress >= 0.5f && flipState == FlipState.FlippingBeforeHalf)
        {
            flipState = FlipState.FlippingAfterHalf;
            isFront = !isFront;

            if (tileIndex != -1 && stageManager != null)
            {
                stageManager.playerBoard.FlipTile(tileIndex);
            }
        }

        if (flipProgress < 0.5f)
        {
            transform.localScale = new Vector3(scale * (1 - flipProgress * 2), scale, 1);
        }
        else
        {
            transform.localScale = new Vector3(scale * (flipProgress - 0.5f) * 2, scale, 1);
        }
    }

    private void FlipNeighbors()
    {
        if (tileIndex == -1 || stageManager == null) return;

        // 現在のタイルの隣接情報を取得
        StageLogic.Tile currentTile = stageManager.playerBoard.tiles[tileIndex];

        // 隣接タイルをフリップ
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

    private bool CheckAllNeighborsBeforeFlip()
    {
        if (tileIndex == -1 || stageManager == null) return true;

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
}