using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private GameObject stageSelectButton;
    [SerializeField] private GameObject challengeButton;

    private ICustomButton stageSelectButtonInterface;
    private ICustomButton challengeButtonInterface;
    private void Start()
    {
        InitializeButtons();
        SetupButtonListeners();
    }

    private void InitializeButtons()
    {
        stageSelectButtonInterface = GetButtonInterface(stageSelectButton);
        challengeButtonInterface = GetButtonInterface(challengeButton);

        if (stageSelectButtonInterface == null)
            Debug.LogError("StageSelectButton interface initialization failed!");

        if (challengeButtonInterface == null)
            Debug.LogError("ChallengeButton interface initialization failed!");
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

    private void SetupButtonListeners()
    {
        if (stageSelectButtonInterface == null || challengeButtonInterface == null)
        {
            Debug.LogError("Button interfaces not initialized!");
            return;
        }
        stageSelectButtonInterface?.onClick.AddListener(OnStageSelectButtonClicked);
        challengeButtonInterface?.onClick.AddListener(OnChallengeButtonClicked);
    }

    private void OnStageSelectButtonClicked()
    {
        AudioManager.Instance.PlayMainButtonSound();
        Debug.Log("Loading Stage Select Scene...");
        SceneManager.LoadScene("StageSelect");
    }

    private void OnChallengeButtonClicked()
    {
        Debug.Log("Starting Challenge Mode...");
        if (ChallengeManager.Instance == null)
        {
            GameObject challengeManagerObj = new GameObject("ChallengeManager");
            challengeManagerObj.AddComponent<ChallengeManager>();
        }
        AudioManager.Instance.PlayMainButtonSound();
        ChallengeManager.Instance.StartChallenge();
    }

    private void OnDestroy()
    {
        if (stageSelectButtonInterface?.onClick != null)
            stageSelectButtonInterface.onClick.RemoveListener(OnStageSelectButtonClicked);

        if (challengeButtonInterface?.onClick != null)
            challengeButtonInterface.onClick.RemoveListener(OnChallengeButtonClicked);
    }
}