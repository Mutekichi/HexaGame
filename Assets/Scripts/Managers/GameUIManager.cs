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
    [SerializeField] private GameObject challengeClearWindow;
    [SerializeField] private GameObject Stars3;
    [SerializeField] private GameObject Stars2;
    [SerializeField] private GameObject Stars1;
    [SerializeField] private GameObject retryButtonOnStageClearWindow;
    [SerializeField] private GameObject backToMainMenuButtonOnStageClearWindow;
    [SerializeField] private GameObject retryButtonOnChallengeClearWindow;
    [SerializeField] private GameObject backToMainMenuButtonOnChallengeClearWindow;

    [Header("Settings")]
    [SerializeField] private bool pauseOnMenuShow = true;

    private ICustomButton hamburgerMenuButtonInterface;
    private ICustomButton retryButtonInterface;
    private ICustomButton backToStageSelectButtonInterface;
    private ICustomButton closeWindowButtonInterface;
    private ICustomButton retryButtonOnStageClearInterface;
    private ICustomButton backToMainMenuButtonOnStageClearInterface;
    private ICustomButton retryButtonOnChallengeClearInterface;
    private ICustomButton backToMainMenuButtonOnChallengeClearInterface;

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
        backToMainMenuButtonOnStageClearInterface = GetButtonInterface(backToMainMenuButtonOnStageClearWindow);
        retryButtonOnChallengeClearInterface = GetButtonInterface(retryButtonOnChallengeClearWindow);
        backToMainMenuButtonOnChallengeClearInterface = GetButtonInterface(backToMainMenuButtonOnChallengeClearWindow);
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
        if (menuWindow != null) menuWindow.SetActive(false);
        if (stageClearWindow != null) stageClearWindow.SetActive(false);
        if (challengeClearWindow != null) challengeClearWindow.SetActive(false);

        SetupButtonIfExists(hamburgerMenuButtonInterface);
        SetupButtonIfExists(retryButtonInterface);
        SetupButtonIfExists(backToStageSelectButtonInterface);
        SetupButtonIfExists(closeWindowButtonInterface);
        SetupButtonIfExists(retryButtonOnStageClearInterface);
        SetupButtonIfExists(backToMainMenuButtonOnStageClearInterface);
        SetupButtonIfExists(retryButtonOnChallengeClearInterface);
        SetupButtonIfExists(backToMainMenuButtonOnChallengeClearInterface);
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

    private void ToggleWindow(GameObject window, bool show)
    {
        if (window != null)
        {
            window.SetActive(show);
            if (pauseOnMenuShow)
            {
                if (show) PauseGame();
                else ResumeGame();
            }
        }
    }

    private void SetupClearWindowButton(ICustomButton button, string logMessage, bool isRetry)
    {
        button?.onClick.AddListener(() =>
        {
            Debug.Log(logMessage);
            HideStageClearWindow();
            HideChallengeClearWindow();
            if (isRetry) OnRetryButtonClicked();
            else OnBackToStageSelectButtonClicked();
        });
    }

    private void SetupButtons()
    {
        hamburgerMenuButtonInterface?.onClick.AddListener(() =>
        {
            ShowMenuWindow();
            AudioManager.Instance.PlaySubButtonSound();
        });
        closeWindowButtonInterface?.onClick.AddListener(() =>
        {

            HideMenuWindow();
            AudioManager.Instance.PlaySubButtonSound();
        });

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

        SetupClearWindowButton(retryButtonOnStageClearInterface, "Retry button on Stage Clear clicked", true);
        SetupClearWindowButton(backToMainMenuButtonOnStageClearInterface, "Back to stage select button clicked", false);
        SetupClearWindowButton(retryButtonOnChallengeClearInterface, "Retry button on Challenge Clear clicked", true);
        SetupClearWindowButton(backToMainMenuButtonOnChallengeClearInterface, "Back to main menu button clicked", false);
    }

    private void SetActiveNumberObject(Transform parent, int number, int maxNumber)
    {
        Transform starCountsTransform = parent.Find("StarCounts");
        if (starCountsTransform != null)
        {
            for (int i = 1; i <= maxNumber; i++)
            {
                Transform numberObj = starCountsTransform.Find(i.ToString());
                if (numberObj != null)
                {
                    numberObj.gameObject.SetActive(i == number);
                }
            }
        }
        else
        {
            Debug.LogWarning("StarCounts transform not found in challenge clear window");
        }
    }

    public void PlayAudioClip(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
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

            if (backToMainMenuButtonOnStageClearInterface != null)
            {
                backToMainMenuButtonOnStageClearInterface.interactable = true;
            }

            if (pauseOnMenuShow)
            {
                PauseGame();
            }
        }
    }

    public void ShowChallengeClearWindow(int stars)
    {
        if (challengeClearWindow != null)
        {
            challengeClearWindow.SetActive(true);
            SetActiveNumberObject(challengeClearWindow.transform, stars, 12);

            if (retryButtonOnChallengeClearInterface != null)
            {
                retryButtonOnChallengeClearInterface.interactable = true;
            }

            if (backToMainMenuButtonOnChallengeClearInterface != null)
            {
                backToMainMenuButtonOnChallengeClearInterface.interactable = true;
            }

            if (pauseOnMenuShow)
            {
                PauseGame();
            }
        }
    }

    private void ShowMenuWindow() => ToggleWindow(menuWindow, true);
    private void HideMenuWindow() => ToggleWindow(menuWindow, false);
    private void HideStageClearWindow() => ToggleWindow(stageClearWindow, false);
    private void HideChallengeClearWindow() => ToggleWindow(challengeClearWindow, false);

    private void OnRetryButtonClicked()
    {
        AudioManager.Instance.PlayMainButtonSound();
        ResumeGame();
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadSceneWithTransition(currentSceneName);
    }

    private void OnBackToStageSelectButtonClicked()
    {
        AudioManager.Instance.PlaySubButtonSound();
        ResumeGame();
        if (StageDataManager.Instance.IsChallengeMode())
        {
            LoadSceneWithTransition("MainMenu");
        }
        else
        {
            LoadSceneWithTransition("StageSelect");
        }
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

    public bool IsChallengeClearWindowVisible()
    {
        return challengeClearWindow != null && challengeClearWindow.activeSelf;
    }

    private void OnDestroy()
    {
        CleanupButtonListeners(hamburgerMenuButtonInterface);
        CleanupButtonListeners(retryButtonInterface);
        CleanupButtonListeners(backToStageSelectButtonInterface);
        CleanupButtonListeners(closeWindowButtonInterface);
        CleanupButtonListeners(retryButtonOnStageClearInterface);
        CleanupButtonListeners(backToMainMenuButtonOnStageClearInterface);
        CleanupButtonListeners(retryButtonOnChallengeClearInterface);
        CleanupButtonListeners(backToMainMenuButtonOnChallengeClearInterface);
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
            else if (!IsStageClearWindowVisible() && !IsChallengeClearWindowVisible())
            {
                ShowMenuWindow();
            }
        }
    }
}