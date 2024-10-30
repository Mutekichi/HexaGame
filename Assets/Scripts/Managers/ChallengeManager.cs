using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ChallengeManager : MonoBehaviour
{
    private const int TOTAL_STAGES = 4;
    private static ChallengeManager instance;
    public static ChallengeManager Instance => instance;

    private int currentStageIndex = 0;
    private int totalStars = 0;
    private List<int> stageStars = new List<int>();
    private GameUIManager gameUIManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void StartChallenge()
    {
        currentStageIndex = 0;
        totalStars = 0;
        stageStars.Clear();
        LoadNextChallengeStage();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
    }
    public void OnStageComplete(int stars)
    {
        stageStars.Add(stars);
        totalStars += stars;

        currentStageIndex++;
        if (currentStageIndex < TOTAL_STAGES)
        {
            LoadNextChallengeStage();
        }
        else
        {
            OnChallengeComplete();
        }
    }

    private void LoadNextChallengeStage()
    {
        StageData randomStage = ChallengeStageGenerator.GenerateRandomStage(7, 4);
        StageDataManager.Instance.SetChallengeStage(randomStage);
        SceneManager.LoadScene("NormalStage");
    }

    private void OnChallengeComplete()
    {
        gameUIManager.ShowChallengeClearWindow(totalStars);
    }

    public int GetTotalStars() => totalStars;
    public List<int> GetStageStars() => stageStars;
    public int GetCurrentStageNumber() => currentStageIndex + 1;
}
