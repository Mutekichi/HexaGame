using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button stageSelectButton;
    [SerializeField] private Button timeAttackButton;

    private void Start()
    {
        // ボタンが割り当てられているか確認
        if (stageSelectButton == null)
        {
            Debug.LogError("StageSelectButton is not assigned!");
            return;
        }

        if (timeAttackButton == null)
        {
            Debug.LogError("TimeAttackButton is not assigned!");
            return;
        }

        // ボタンにリスナーを追加
        stageSelectButton.onClick.AddListener(OnStageSelectButtonClicked);
        timeAttackButton.onClick.AddListener(OnTimeAttackButtonClicked);
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
        // リスナーの解除
        if (stageSelectButton != null)
            stageSelectButton.onClick.RemoveListener(OnStageSelectButtonClicked);

        if (timeAttackButton != null)
            timeAttackButton.onClick.RemoveListener(OnTimeAttackButtonClicked);
    }
}