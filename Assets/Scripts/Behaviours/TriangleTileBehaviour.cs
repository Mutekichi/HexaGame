using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class TriangleTileBehaviour : MonoBehaviour
{
    [SerializeField] private bool isUpward = true;
    [SerializeField] private SpriteRenderer upwardSprite;
    [SerializeField] private SpriteRenderer downwardSprite;
    // Duration of the flip animation [s]
    [SerializeField] private float flipDuration = 0.5f;


    private enum FlipState
    {
        NotFlipping,
        FlippingBeforeHalf,
        FlippingAfterHalf
    }
    
    // The default scale of the transform.x(y) of the tile
    // TO DO: Set this value to the scale of the tile in the scene
    private static readonly float DefaultScale = 2;
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
        if (upwardSprite == null || downwardSprite == null)
        {
            Debug.LogWarning("UpwardTriangle or DownwardTriangle SpriteRenderer not assigned!");
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
    if (upwardSprite != null)
    {
        upwardSprite.enabled = isUpward;
    }
    if (downwardSprite != null)
    {
        downwardSprite.enabled = !isUpward;
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
        if (flipProgress >= 1)
        {
            flipProgress = 1;
            flipState = FlipState.NotFlipping;
        }
        if (flipProgress >= 0.5f && flipState == FlipState.FlippingBeforeHalf)
        {
            flipState = FlipState.FlippingAfterHalf;
            isUpward = !isUpward;
            UpdateSprite();
        }
        if (flipState == FlipState.FlippingBeforeHalf)
        {
            upwardSprite.transform.localScale = new Vector3(1, 1 - flipProgress * 2, 0) * DefaultScale;
            downwardSprite.transform.localScale = new Vector3(1, 1 - flipProgress * 2, 0) * DefaultScale;
        }
        else
        {
            upwardSprite.transform.localScale = new Vector3(1, (flipProgress - 0.5f) * 2, 0) * DefaultScale;
            downwardSprite.transform.localScale = new Vector3(1, (flipProgress - 0.5f) * 2, 0) * DefaultScale;
        }
    }
}