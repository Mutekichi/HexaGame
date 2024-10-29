// TriangleButtonWrapper.cs
using UnityEngine;
using UnityEngine.Events;

public class TriangleButtonWrapper : MonoBehaviour, ICustomButton
{
    private TriangleButton triangleButton;

    private void Awake()
    {
        triangleButton = GetComponent<TriangleButton>();
    }

    public UnityEvent onClick => triangleButton ? triangleButton.onClick : null;

    public bool interactable
    {
        get => triangleButton ? triangleButton.interactable : false;
        set { if (triangleButton) triangleButton.interactable = value; }
    }
}