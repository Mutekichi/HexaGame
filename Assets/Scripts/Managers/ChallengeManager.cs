using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ChallengeManager : MonoBehaviour
{
    private const int TOTAL_STAGES = 5;
    private static ChallengeManager instance;
    public static ChallengeManager Instance => instance;

    private int currentStageIndex = 0;
    private int totalStars = 0;
    private List<int> stageStars = new List<int>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartChallenge()
    {
        currentStageIndex = 0;
        totalStars = 0;
        stageStars.Clear();
        LoadNextChallengeStage();
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
        Debug.Log("Challenge Complete!");
        Debug.Log("Total Stars: " + totalStars);
    }

    public int GetTotalStars() => totalStars;
    public List<int> GetStageStars() => stageStars;
    public int GetCurrentStageNumber() => currentStageIndex + 1;
}
