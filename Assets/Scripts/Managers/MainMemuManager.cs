using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private GameObject stageSelectButton;
    [SerializeField] private GameObject timeAttackButton;

    private ICustomButton stageSelectButtonInterface;
    private ICustomButton timeAttackButtonInterface;

    private void Start()
    {
        InitializeButtons();
        SetupButtonListeners();
    }

    private void InitializeButtons()
    {
        stageSelectButtonInterface = GetButtonInterface(stageSelectButton);
        timeAttackButtonInterface = GetButtonInterface(timeAttackButton);

        if (stageSelectButtonInterface == null)
            Debug.LogError("StageSelectButton interface initialization failed!");

        if (timeAttackButtonInterface == null)
            Debug.LogError("TimeAttackButton interface initialization failed!");
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
        if (stageSelectButtonInterface == null || timeAttackButtonInterface == null)
        {
            Debug.LogError("Button interfaces not initialized!");
            return;
        }
        stageSelectButtonInterface?.onClick.AddListener(OnStageSelectButtonClicked);
        timeAttackButtonInterface?.onClick.AddListener(OnTimeAttackButtonClicked);
    }

    private void OnStageSelectButtonClicked()
    {
        Debug.Log("Loading Stage Select Scene...");
        SceneManager.LoadScene("StageSelect");
    }

    private void OnTimeAttackButtonClicked()
    {
        Debug.Log("Loading Time Attack Scene...");
        SceneManager.LoadScene("TimeAttack");
    }

    private void OnDestroy()
    {
        if (stageSelectButtonInterface?.onClick != null)
            stageSelectButtonInterface.onClick.RemoveListener(OnStageSelectButtonClicked);

        if (timeAttackButtonInterface?.onClick != null)
            timeAttackButtonInterface.onClick.RemoveListener(OnTimeAttackButtonClicked);
    }
}