// ICustomButton.cs
using UnityEngine.Events;

public interface ICustomButton
{
    UnityEvent onClick { get; }
    bool interactable { get; set; }
}