using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private GameObject HomeButton;
    private ICustomButton HomeButtonInterface;
    private static StageSelectManager instance;

    [Header("Stage Select Buttons")]
    [SerializeField] private GameObject[] entranceButtons = new GameObject[12];

    private ICustomButton[] entranceButtonInterfaces = new ICustomButton[12];

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        AutoDetectEntranceButtons();
        InitializeButtonInterfaces();
    }

    private void AutoDetectEntranceButtons()
    {
        for (int i = 0; i < 12; i++)
        {
            string buttonName = $"Entrance{i + 1}";
            GameObject buttonObj = GameObject.Find(buttonName);

            if (buttonObj != null)
            {
                entranceButtons[i] = buttonObj;
            }
            else
            {
                Debug.LogError($"Could not find button with name: {buttonName}");
            }
        }
    }

    private void InitializeButtonInterfaces()
    {
        HomeButtonInterface = GetButtonInterface(HomeButton);
        for (int i = 0; i < entranceButtons.Length; i++)
        {
            if (entranceButtons[i] != null)
            {
                entranceButtonInterfaces[i] = GetButtonInterface(entranceButtons[i]);
                if (entranceButtonInterfaces[i] == null)
                {
                    Debug.LogError($"Failed to get button interface for Entrance{i + 1}");
                }
            }
        }
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

    private void Start()
    {
        InitializeUI();
        SetupButtons();
    }

    private void InitializeUI()
    {
        SetupButtonIfExists(HomeButtonInterface);
        for (int i = 0; i < entranceButtonInterfaces.Length; i++)
        {
            SetupButtonIfExists(entranceButtonInterfaces[i]);
        }
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
        if (HomeButtonInterface == null)
        {
            Debug.LogError("HomeButton interface initialization failed!");
            return;
        }
        HomeButtonInterface?.onClick.AddListener(OnHomeButtonClicked);

        for (int i = 0; i < entranceButtonInterfaces.Length; i++)
        {
            int stageId = i + 1;
            if (entranceButtonInterfaces[i] != null)
            {
                entranceButtonInterfaces[i].onClick.AddListener(() => OnStageButtonClicked(stageId));
            }
        }
    }

    private void OnHomeButtonClicked()
    {
        AudioManager.Instance.PlaySubButtonSound();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnStageButtonClicked(int stageId)
    {
        AudioManager.Instance.PlayMainButtonSound();
        LoadStage(stageId);
    }

    private void LoadStage(int stageId)
    {
        StageDataManager.Instance.SetCurrentStage(stageId);
        SceneManager.LoadScene("NormalStage");
    }

    private void OnDestroy()
    {
        CleanupButtonListeners(HomeButtonInterface);
        for (int i = 0; i < entranceButtonInterfaces.Length; i++)
        {
            CleanupButtonListeners(entranceButtonInterfaces[i]);
        }
    }

    private void CleanupButtonListeners(ICustomButton button)
    {
        if (button?.onClick != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}