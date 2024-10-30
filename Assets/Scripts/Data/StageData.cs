using System.Collections.Generic;

[System.Serializable]
public class StageData
{
    public int stageId;
    public int width;
    public int height;
    public string[] initialPattern;
    public string[] targetPattern;
    public bool isTopLeftTriangleDownward;
    public StarCondition starCondition;
}

[System.Serializable]
public class StarCondition
{
    public int toGet3Stars;
    public int toGet2Stars;
}

[System.Serializable]
public class StageDataCollection
{
    public List<StageData> stages;
}
