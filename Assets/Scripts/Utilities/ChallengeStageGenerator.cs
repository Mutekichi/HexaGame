public class ChallengeStageGenerator
{
    public static StageData GenerateRandomStage(int width, int height)
    {
        return new StageData
        {
            stageId = -1,
            width = width,
            height = height,
            initialPattern = GenerateInitialPattern(),
            targetPattern = GenerateTargetPattern(),
            isTopLeftTriangleDownward = true,
            starCondition = new StarCondition
            {
                toGet3Stars = 5,
                toGet2Stars = 10
            }
        };
    }

    private static string[] GenerateInitialPattern()
    {
        return new string[]
        {
            "0212120",
            "2121212",
            "1212121",
            "0000010"
        };
    }

    private static string[] GenerateTargetPattern()
    {
        return new string[]
        {
            "0211110",
            "1111111",
            "1111111",
            "0000010"
        };
    }
}