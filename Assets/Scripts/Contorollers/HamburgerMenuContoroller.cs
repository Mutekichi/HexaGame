using UnityEngine;
using UnityEngine.UI;

public class HamburgerMenuController : MonoBehaviour
{
    public GameObject menuPanel;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleMenu);
    }

    private void ToggleMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }
}