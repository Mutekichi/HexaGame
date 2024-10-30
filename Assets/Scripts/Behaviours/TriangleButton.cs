using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasRenderer))]
public class TriangleButton : Graphic, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Vector2[] trianglePoints = new Vector2[3];
    private Canvas parentCanvas;

    // UnityEventを追加
    [SerializeField]
    private Button.ButtonClickedEvent m_OnClick = new Button.ButtonClickedEvent();
    public Button.ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }

    private bool m_Interactable = true;
    public bool interactable
    {
        get { return m_Interactable; }
        set
        {
            m_Interactable = value;
            if (!m_Interactable)
            {
                // 無効時は薄いグレー
                color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                color = Color.white;
            }
            SetVerticesDirty();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("TriangleButton must be placed under a Canvas in the hierarchy!");
        }
    }

    public override bool Raycast(Vector2 screenPoint, Camera eventCamera)
    {
        if (!parentCanvas) return false;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            screenPoint,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventCamera,
            out localPoint))
        {
            return IsPointInTriangle(localPoint);
        }
        return false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        trianglePoints[0] = new Vector2(width * 0.5f, 0);
        trianglePoints[1] = new Vector2(0, height);
        trianglePoints[2] = new Vector2(width, height);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        for (int i = 0; i < 3; i++)
        {
            vertex.position = new Vector3(trianglePoints[i].x, trianglePoints[i].y);
            vh.AddVert(vertex);
        }

        vh.AddTriangle(0, 1, 2);
    }

    private bool IsPointInTriangle(Vector2 point)
    {
        float d1 = Sign(point, trianglePoints[0], trianglePoints[1]);
        float d2 = Sign(point, trianglePoints[1], trianglePoints[2]);
        float d3 = Sign(point, trianglePoints[2], trianglePoints[0]);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!m_Interactable) return;

        if (m_OnClick != null)
        {
            m_OnClick.Invoke();
        }
        Debug.Log($"{gameObject.name} Clicked!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!m_Interactable) return;

        color = Color.gray;
        SetVerticesDirty();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!m_Interactable) return;

        color = Color.white;
        SetVerticesDirty();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}