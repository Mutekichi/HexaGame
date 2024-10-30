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
    private StageData currentStageData;

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
        currentStageData = stageDataCollection.stages.Find(s => s.stageId == stageId);
        if (currentStageData == null)
        {
            Debug.LogError($"Stage {stageId} not found!");
            return;
        }
    }

    public StageData GetCurrentStageData() => currentStageData;
}