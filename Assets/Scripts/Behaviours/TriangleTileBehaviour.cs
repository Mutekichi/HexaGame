using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class TriangleTileBehaviour : MonoBehaviour
{
    [SerializeField] private bool isUpward = true;
    [SerializeField] private bool isFront = true;
    [SerializeField] private SpriteRenderer upwardFrontSprite;
    [SerializeField] private SpriteRenderer downwardFrontSprite;
    [SerializeField] private SpriteRenderer upwardBackSprite;
    [SerializeField] private SpriteRenderer downwardBackSprite;
    // Duration of the flip animation [s]
    [SerializeField] private float flipDuration = 0.3f;


    private enum FlipState
    {
        NotFlipping,
        FlippingBeforeHalf,
        FlippingAfterHalf
    }
    
    // The default scale of the transform.x(y) of the tile
    // TO DO: Set this value to the scale of the tile in the scene
    private static readonly float DefaultScale = 1;
    // Progress of the flip animation [0, 1]
    private float flipProgress;
    private FlipState flipState = FlipState.NotFlipping;

    private void OnEnable()
    {
        UpdateSprite();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdatePositionAndState();
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
    }

    private void OnValidate()
    {
        ValidateSprites();
        UpdatePositionAndState();
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
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Check if the mouse is close to the tile
            if (Vector2.Distance(mousePos, transform.position) < 0.5f)
            {
                StartFlip();
            }
        }
    }

    private void UpdatePositionAndState()
    {
        Vector3 snappedPosition = TriangleGridUtility.GetSnappedPosOnTriangleGrid(transform.position);
        transform.position = snappedPosition;

        TriangleGridUtility.GridPositionState state = TriangleGridUtility.GetGridPositionState(snappedPosition);

        switch (state)
        {
            case TriangleGridUtility.GridPositionState.OnUpwardCenter:
                SetUpward(true);
                break;
            case TriangleGridUtility.GridPositionState.OnDownwardCenter:
                SetUpward(false);
                break;
            case TriangleGridUtility.GridPositionState.OnVertex:
                break;
        }
    }

    private void SetUpward(bool upward)
    {
        if (isUpward != upward)
        {
            isUpward = upward;
            UpdateSprite();
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
            UpdateSprite();
        }
        // flip the tile based on the progress of the flip animation
        // X scale is flipped when the tile is flipped
        if (flipProgress < 0.5f) {
            transform.localScale = new Vector3(DefaultScale * (1 - flipProgress * 2), DefaultScale, 1);
        } else {
            transform.localScale = new Vector3(DefaultScale * (flipProgress - 0.5f) * 2, DefaultScale, 1);
        }
    }
}