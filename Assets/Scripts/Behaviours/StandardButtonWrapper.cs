// StandardButtonWrapper.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class StandardButtonWrapper : MonoBehaviour, ICustomButton
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public UnityEvent onClick => button ? button.onClick : null;

    public bool interactable
    {
        get => button ? button.interactable : false;
        set { if (button) button.interactable = value; }
    }
}
