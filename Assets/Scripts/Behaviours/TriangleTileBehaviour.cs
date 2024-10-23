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

    private enum FlipState
    {
        NotFlipping,
        FlippingBeforeHalf,
        FlippingAfterHalf
    }
    
    private static readonly float DefaultScale = 1;
    private float flipProgress;
    private FlipState flipState = FlipState.NotFlipping;
    private Collider2D tileCollider { get { return isUpward ? upwardCollider : downwardCollider; } }
    private BoardManager boardManager;

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
        boardManager = FindObjectOfType<BoardManager>();
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
        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            if (IsClickedOnTile(mousePosition))
            {
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

    private void UpdatePositionAndState()
    {
        Vector3 snappedPosition = TriangleGridUtility.GetSnappedPosOnTriangleGrid(transform.position);
        transform.position = snappedPosition;

        TriangleGridUtility.GridPositionState state = TriangleGridUtility.GetGridPositionState(snappedPosition);

        switch (state)
        {
            case TriangleGridUtility.GridPositionState.OnUpwardCenter:
                isUpward = true;
                break;
            case TriangleGridUtility.GridPositionState.OnDownwardCenter:
                isUpward = false;
                break;
            case TriangleGridUtility.GridPositionState.OnVertex:
                break;
        }
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

    private void UpdateFlip() {
        flipProgress += Time.deltaTime / flipDuration;
        if (flipProgress >= 1) {
            flipProgress = 1;
            flipState = FlipState.NotFlipping;
        }
        else if (flipProgress >= 0.5f && flipState == FlipState.FlippingBeforeHalf) {
            flipState = FlipState.FlippingAfterHalf;
            isFront = !isFront;
        }
        if (flipProgress < 0.5f) {
            transform.localScale = new Vector3(DefaultScale * (1 - flipProgress * 2), DefaultScale, 1);
        } else {
            transform.localScale = new Vector3(DefaultScale * (flipProgress - 0.5f) * 2, DefaultScale, 1);
        }
    }

    private void FlipNeighbors()
    {
        List<GameObject> neighbors = boardManager.GetNeighborsFromTilePosition(transform.position);
        foreach (GameObject neighbor in neighbors)
        {
            TriangleTileBehaviour neighborTile = neighbor.GetComponent<TriangleTileBehaviour>();
            if (neighborTile != null)
            {
                neighborTile.StartFlip();
            }
        }
    }

    private bool CheckAllNeighborsBeforeFlip()
    {
        List<GameObject> neighbors = boardManager.GetNeighborsFromTilePosition(transform.position);
        foreach (GameObject neighbor in neighbors)
        {
            TriangleTileBehaviour neighborTile = neighbor.GetComponent<TriangleTileBehaviour>();
            if (neighborTile != null && neighborTile.flipState != FlipState.NotFlipping)
            {
                return false;
            }
        }
        return true;
    }
}