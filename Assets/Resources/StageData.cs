using System.Collections.Generic;
using UnityEngine;

namespace StageManagement
{
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
        public StageData()
        {
            stageId = 1;
            width = 1;
            height = 1;
            initialPattern = new string[0];
            targetPattern = new string[0];
            isTopLeftTriangleDownward = true;
            starCondition = new StarCondition();
        }

        public bool ValidatePatterns()
        {
            if (initialPattern == null || targetPattern == null)
                return false;

            if (initialPattern.Length != height || targetPattern.Length != height)
                return false;

            foreach (string row in initialPattern)
            {
                if (row.Length != width)
                    return false;
                if (!IsValidPatternString(row))
                    return false;
            }

            foreach (string row in targetPattern)
            {
                if (row.Length != width)
                    return false;
                if (!IsValidPatternString(row))
                    return false;
            }

            return true;
        }
        private bool IsValidPatternString(string pattern)
        {
            foreach (char c in pattern)
            {
                if (c != '0' && c != '1' && c != '2')
                    return false;
            }
            return true;
        }
        public bool ValidatePatternConsistency()
        {
            if (!ValidatePatterns())
                return false;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bool hasInitialTile = initialPattern[i][j] != '0';
                    bool hasTargetTile = targetPattern[i][j] != '0';
                    if (hasInitialTile != hasTargetTile)
                        return false;
                }
            }
            return true;
        }
    }

    [System.Serializable]
    public class StarCondition
    {
        public int toGet3Stars;
        public int toGet2Stars;
        public StarCondition()
        {
            toGet3Stars = 10;
            toGet2Stars = 15;
        }

        public bool IsValid()
        {
            return toGet3Stars > 0 && toGet2Stars > toGet3Stars;
        }
    }

    [System.Serializable]
    public class StageDataCollection
    {
        public List<StageData> stages = new List<StageData>();
        public StageDataCollection()
        {
            stages = new List<StageData>();
        }

        public bool ValidateStageIds()
        {
            HashSet<int> usedIds = new HashSet<int>();
            foreach (var stage in stages)
            {
                if (stage.stageId <= 0 || !usedIds.Add(stage.stageId))
                    return false;
            }
            return true;
        }

        public bool ValidateAllStages()
        {
            if (!ValidateStageIds())
                return false;

            foreach (var stage in stages)
            {
                if (!stage.ValidatePatterns() ||
                    !stage.ValidatePatternConsistency() ||
                    !stage.starCondition.IsValid())
                    return false;
            }
            return true;
        }

        public StageData GetStageById(int stageId)
        {
            return stages.Find(stage => stage.stageId == stageId);
        }
        private bool isChallengeMode = false;
        private StageData timeAttackStageData;
        private StageData normalStageData;

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
}