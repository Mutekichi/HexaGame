using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    private static GameUIManager instance;

    [SerializeField] private GameObject hamburgerMenuButton;
    [SerializeField] private GameObject menuWindow;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject backToStageSelectButton;
    [SerializeField] private GameObject closeWindowButton;

    private void Awake()
    {
        instance = this;
    }

    public static bool IsMenuVisible()
    {
        return instance != null && instance.IsMenuWindowVisible();
    }

    private void Start()
    {
        if (menuWindow != null)
        {
            menuWindow.SetActive(false);
        }

        SetupButtons();
    }

    private void SetupButtons()
    {
        if (hamburgerMenuButton != null)
        {
            hamburgerMenuButton.GetComponent<Button>()?.onClick.AddListener(ShowMenuWindow);
        }

        if (retryButton != null)
        {
            retryButton.GetComponent<Button>()?.onClick.AddListener(OnRetryButtonClicked);
        }

        if (backToStageSelectButton != null)
        {
            backToStageSelectButton.GetComponent<Button>()?.onClick.AddListener(OnBackToStageSelectButtonClicked);
        }

        if (closeWindowButton != null)
        {
            closeWindowButton.GetComponent<Button>()?.onClick.AddListener(HideMenuWindow);
        }
    }

    private void ShowMenuWindow()
    {
        if (menuWindow != null)
        {
            menuWindow.SetActive(true);
        }
    }

    private void HideMenuWindow()
    {
        if (menuWindow != null)
        {
            menuWindow.SetActive(false);
        }
    }

    private void OnRetryButtonClicked()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void OnBackToStageSelectButtonClicked()
    {
        SceneManager.LoadScene("StageSelect");
    }

    private void LoadSceneWithTransition(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public bool IsMenuWindowVisible()
    {
        return menuWindow != null && menuWindow.activeSelf;
    }

    private void OnDestroy()
    {
        if (hamburgerMenuButton != null)
        {
            var button = hamburgerMenuButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        if (retryButton != null)
        {
            var button = retryButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        if (backToStageSelectButton != null)
        {
            var button = backToStageSelectButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        if (closeWindowButton != null)
        {
            var button = closeWindowButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}