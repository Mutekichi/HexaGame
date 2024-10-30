using UnityEngine;

public class StageDataManager : MonoBehaviour
{
    private static StageDataManager instance;
    public static StageDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("StageDataManager");
                instance = go.AddComponent<StageDataManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private StageDataCollection stageDataCollection;
    private StageData normalStageData;
    private bool isChallengeMode = false;
    private StageData timeAttackStageData;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        LoadStageData();
    }

    private void LoadStageData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("stages");
        if (jsonFile != null)
        {
            stageDataCollection = JsonUtility.FromJson<StageDataCollection>(jsonFile.text);
        }
        else
        {
            Debug.LogError("Stage data file not found!");
        }
    }

    public void SetCurrentStage(int stageId)
    {
        normalStageData = stageDataCollection.stages.Find(s => s.stageId == stageId);
        isChallengeMode = false;
        if (normalStageData == null)
        {
            Debug.LogError($"Stage {stageId} not found!");
            return;
        }
    }

    public void SetChallengeStage(StageData stageData)
    {
        timeAttackStageData = stageData;
        isChallengeMode = true;
    }

    public StageData GetCurrentStageData()
    {
        return isChallengeMode ? timeAttackStageData : normalStageData;
    }

    public bool IsChallengeMode() => isChallengeMode;
}