using UnityEngine;

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
    public int GetTileIndex()
    {
        return tileIndex;
    }
    public float scale;
    public void SetScale(float scale)
    {
        this.scale = scale;
    }
    public float GetScale()
    {
        return scale;
    }
    public bool IsClickable { get; set; } = true;

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

    public enum FlipState
    {
        NotFlipping,
        FlippingBeforeHalf,
        FlippingAfterHalf
    }

    private float flipProgress;
    public FlipState flipState { get; private set; } = FlipState.NotFlipping;
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
        if (GameUIManager.IsMenuVisible() || !IsClickable || stageManager == null)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            if (IsClickedOnTile(mousePosition))
            {
                Debug.Log("tile" + tileIndex + " clicked");
                StageManager.OnClickTile(tileIndex);
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

    public void StartFlip()
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
}