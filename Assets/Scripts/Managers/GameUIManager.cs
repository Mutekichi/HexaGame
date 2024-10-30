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
    [SerializeField] private GameObject stageClearWindow;
    [SerializeField] private GameObject Stars3;
    [SerializeField] private GameObject Stars2;
    [SerializeField] private GameObject Stars1;
    [SerializeField] private GameObject retryButtonOnStageClearWindow;
    [SerializeField] private GameObject backToStageSelectButtonOnStageClearWindow;

    [Header("Settings")]
    [SerializeField] private bool pauseOnMenuShow = true;

    private ICustomButton hamburgerMenuButtonInterface;
    private ICustomButton retryButtonInterface;
    private ICustomButton backToStageSelectButtonInterface;
    private ICustomButton closeWindowButtonInterface;
    private ICustomButton retryButtonOnStageClearInterface;
    private ICustomButton backToStageSelectButtonOnStageClearInterface;

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
        retryButtonOnStageClearInterface = GetButtonInterface(retryButtonOnStageClearWindow);
        backToStageSelectButtonOnStageClearInterface = GetButtonInterface(backToStageSelectButtonOnStageClearWindow);
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

    public static bool IsMenuVisible() => instance != null && (instance.IsMenuWindowVisible() || instance.IsStageClearWindowVisible());

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

        if (stageClearWindow != null)
        {
            stageClearWindow.SetActive(false);
        }

        SetupButtonIfExists(hamburgerMenuButtonInterface);
        SetupButtonIfExists(retryButtonInterface);
        SetupButtonIfExists(backToStageSelectButtonInterface);
        SetupButtonIfExists(closeWindowButtonInterface);
        SetupButtonIfExists(retryButtonOnStageClearInterface);
        SetupButtonIfExists(backToStageSelectButtonOnStageClearInterface);
    }

    private void SetupButtonIfExists(ICustomButton button)
    {
        if (button != null)
        {
            button.interactable = true;
        }
        else if (button == null)
        {
            Debug.LogWarning("Button is not set up correctly.");
        }
    }

    private void SetupButtons()
    {
        hamburgerMenuButtonInterface?.onClick.AddListener(ShowMenuWindow);

        retryButtonInterface?.onClick.AddListener(() =>
        {
            HideMenuWindow();
            OnRetryButtonClicked();
        });

        backToStageSelectButtonInterface?.onClick.AddListener(() =>
        {
            HideMenuWindow();
            OnBackToStageSelectButtonClicked();
        });

        closeWindowButtonInterface?.onClick.AddListener(HideMenuWindow);

        retryButtonOnStageClearInterface?.onClick.AddListener(() =>
        {
            Debug.Log("Retry button on Stage Clear clicked");
            HideStageClearWindow();
            OnRetryButtonClicked();
        });

        backToStageSelectButtonOnStageClearInterface?.onClick.AddListener(() =>
        {
            Debug.Log("Back to stage select button clicked");
            HideStageClearWindow();
            OnBackToStageSelectButtonClicked();
        });
    }

    public void ShowStageClearWindow(int stars)
    {
        if (stageClearWindow != null)
        {
            stageClearWindow.SetActive(true);

            stars = Mathf.Clamp(stars, 1, 3);

            if (Stars1 != null) Stars1.SetActive(stars == 1);
            if (Stars2 != null) Stars2.SetActive(stars == 2);
            if (Stars3 != null) Stars3.SetActive(stars == 3);

            if (retryButtonOnStageClearInterface != null)
            {
                retryButtonOnStageClearInterface.interactable = true;
            }

            if (backToStageSelectButtonOnStageClearInterface != null)
            {
                backToStageSelectButtonOnStageClearInterface.interactable = true;
            }

            if (pauseOnMenuShow)
            {
                PauseGame();
            }
        }
    }

    private void HideStageClearWindow()
    {
        if (stageClearWindow != null)
        {
            stageClearWindow.SetActive(false);
            if (pauseOnMenuShow)
            {
                ResumeGame();
            }
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

    public bool IsStageClearWindowVisible()
    {
        return stageClearWindow != null && stageClearWindow.activeSelf;
    }

    private void OnDestroy()
    {
        CleanupButtonListeners(hamburgerMenuButtonInterface);
        CleanupButtonListeners(retryButtonInterface);
        CleanupButtonListeners(backToStageSelectButtonInterface);
        CleanupButtonListeners(closeWindowButtonInterface);
        CleanupButtonListeners(retryButtonOnStageClearInterface);
        CleanupButtonListeners(backToStageSelectButtonOnStageClearInterface);
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
            else if (!IsStageClearWindowVisible()) // ステージクリアウィンドウが表示されていない場合のみメニューを表示
            {
                ShowMenuWindow();
            }
        }
    }
}