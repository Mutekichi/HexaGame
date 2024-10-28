// GameUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    private static GameUIManager instance;

    [Header("UI Elements")]
    [SerializeField] private GameObject hamburgerMenuButton;
    [SerializeField] private GameObject menuWindow;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject backToStageSelectButton;
    [SerializeField] private GameObject closeWindowButton;

    [Header("Settings")]
    [SerializeField] private bool pauseOnMenuShow = true;

    private ICustomButton hamburgerMenuButtonInterface;
    private ICustomButton retryButtonInterface;
    private ICustomButton backToStageSelectButtonInterface;
    private ICustomButton closeWindowButtonInterface;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeButtonInterfaces();
    }

    private void InitializeButtonInterfaces()
    {
        hamburgerMenuButtonInterface = GetButtonInterface(hamburgerMenuButton);
        retryButtonInterface = GetButtonInterface(retryButton);
        backToStageSelectButtonInterface = GetButtonInterface(backToStageSelectButton);
        closeWindowButtonInterface = GetButtonInterface(closeWindowButton);
    }

    private ICustomButton GetButtonInterface(GameObject buttonObject)
    {
        if (buttonObject == null) return null;

        var standardWrapper = buttonObject.GetComponent<StandardButtonWrapper>();
        if (standardWrapper == null)
        {
            standardWrapper = buttonObject.GetComponent<Button>() != null
                ? buttonObject.AddComponent<StandardButtonWrapper>()
                : null;
        }

        var triangleWrapper = buttonObject.GetComponent<TriangleButtonWrapper>();
        if (triangleWrapper == null)
        {
            triangleWrapper = buttonObject.GetComponent<TriangleButton>() != null
                ? buttonObject.AddComponent<TriangleButtonWrapper>()
                : null;
        }

        return (ICustomButton)standardWrapper ?? triangleWrapper;
    }

    public static bool IsMenuVisible() => instance != null && instance.IsMenuWindowVisible();

    private void Start()
    {
        InitializeUI();
        SetupButtons();
    }

    private void InitializeUI()
    {
        if (menuWindow != null)
        {
            menuWindow.SetActive(false);
        }

        SetupButtonIfExists(hamburgerMenuButtonInterface);
        SetupButtonIfExists(retryButtonInterface);
        SetupButtonIfExists(backToStageSelectButtonInterface);
        SetupButtonIfExists(closeWindowButtonInterface);
    }

    private void SetupButtonIfExists(ICustomButton button)
    {
        if (button != null)
        {
            button.interactable = true;
        }
    }

    private void SetupButtons()
    {
        if (hamburgerMenuButtonInterface != null)
        {
            hamburgerMenuButtonInterface.onClick.AddListener(ShowMenuWindow);
        }

        if (retryButtonInterface != null)
        {
            retryButtonInterface.onClick.AddListener(() =>
            {
                HideMenuWindow();
                OnRetryButtonClicked();
            });
        }

        if (backToStageSelectButtonInterface != null)
        {
            backToStageSelectButtonInterface.onClick.AddListener(() =>
            {
                HideMenuWindow();
                OnBackToStageSelectButtonClicked();
            });
        }

        if (closeWindowButtonInterface != null)
        {
            closeWindowButtonInterface.onClick.AddListener(HideMenuWindow);
        }
    }

    private void ShowMenuWindow()
    {
        if (menuWindow != null)
        {
            menuWindow.SetActive(true);
            if (pauseOnMenuShow)
            {
                PauseGame();
            }
        }
    }

    private void HideMenuWindow()
    {
        if (menuWindow != null)
        {
            menuWindow.SetActive(false);
            if (pauseOnMenuShow)
            {
                ResumeGame();
            }
        }
    }

    private void OnRetryButtonClicked()
    {
        ResumeGame();
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadSceneWithTransition(currentSceneName);
    }

    private void OnBackToStageSelectButtonClicked()
    {
        ResumeGame();
        LoadSceneWithTransition("StageSelect");
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
        CleanupButtonListeners(hamburgerMenuButtonInterface);
        CleanupButtonListeners(retryButtonInterface);
        CleanupButtonListeners(backToStageSelectButtonInterface);
        CleanupButtonListeners(closeWindowButtonInterface);
    }

    private void CleanupButtonListeners(ICustomButton button)
    {
        if (button?.onClick != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsMenuWindowVisible())
            {
                HideMenuWindow();
            }
            else
            {
                ShowMenuWindow();
            }
        }
    }
}