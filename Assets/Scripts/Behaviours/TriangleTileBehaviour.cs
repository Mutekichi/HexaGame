using UnityEngine;

[ExecuteAlways]
public class TriangleTileBehaviour : MonoBehaviour
{
    [SerializeField] private bool isUpward = true;
    [SerializeField] private SpriteRenderer upwardSprite;
    [SerializeField] private SpriteRenderer downwardSprite;

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
                // 頂点上にある場合の処理は変更しません
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
            upwardSprite.gameObject.SetActive(isUpward);
        }
        if (downwardSprite != null)
        {
            downwardSprite.enabled = !isUpward;
            downwardSprite.gameObject.SetActive(!isUpward);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isUpward ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}