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

        // コンストラクタ
        public StageData()
        {
            // デフォルト値の設定
            stageId = 1;
            width = 1;
            height = 1;
            initialPattern = new string[0];
            targetPattern = new string[0];
            isTopLeftTriangleDownward = true;
            starCondition = new StarCondition();
        }

        // パターンの検証メソッド
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

        // パターン文字列の検証
        private bool IsValidPatternString(string pattern)
        {
            foreach (char c in pattern)
            {
                if (c != '0' && c != '1' && c != '2')
                    return false;
            }
            return true;
        }

        // 初期パターンとターゲットパターンの整合性チェック
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

        // コンストラクタ
        public StarCondition()
        {
            toGet3Stars = 10;
            toGet2Stars = 15;
        }

        // 条件の妥当性チェック
        public bool IsValid()
        {
            // 3つ星の条件は2つ星の条件より少ない手数であるべき
            return toGet3Stars > 0 && toGet2Stars > toGet3Stars;
        }
    }

    [System.Serializable]
    public class StageDataCollection
    {
        public List<StageData> stages = new List<StageData>();

        // コンストラクタ
        public StageDataCollection()
        {
            stages = new List<StageData>();
        }

        // ステージIDの検証
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

        // 全ステージの検証
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

        // ステージIDからステージデータを取得
        public StageData GetStageById(int stageId)
        {
            return stages.Find(stage => stage.stageId == stageId);
        }
    }
}