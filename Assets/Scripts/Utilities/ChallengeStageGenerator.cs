using System;
using UnityEngine;
using UnityEngine.AI;

public class ChallengeStageGenerator
{

    public static StageData GenerateRandomStage(int level)
    {
        if (level == 1) return GenerateStage(5, 4, false, new string[]
        {
            "00100",
            "11111",
            "11111",
            "00100",
        });
        if (level == 2) return GenerateStage(7, 4, true, new string[]
        {
            "0111110",
            "1111111",
            "1111111",
            "0111110",
        });
        if (level == 3) return GenerateStage(9, 6, true, new string[]
        {
            "000010000",
            "000111000",
            "011111110",
            "011111110",
            "111111111",
            "000010000"
        });
        if (level == 4) return GenerateStage(11, 8, true, new string[]
        {
            "00000100000",
            "00001110000",
            "11111111111",
            "01111111110",
            "01111111110",
            "11111111111",
            "00001110000",
            "00000100000",
        });
        else return GenerateStage(4, 4, true, new string[]
        {
            "0110",
            "1111",
            "1111",
            "0110",
        });
    }


    private static StageData GenerateStage(int width, int height, bool isTopLeftTriangleDownward, string[] shape)
    {
        if (shape == null || shape.Length != height)
        {
            throw new ArgumentException($"Shape array must have length {height}");
        }
        foreach (string row in shape)
        {
            if (row.Length != width)
            {
                throw new ArgumentException($"Each row in shape must have length {width}");
            }
        }

        StageData stageData = new StageData();

        StageLogic.CellExpression cellExpression = new StageLogic.CellExpression(
            height,
            width,
            isTopLeftTriangleDownward,
            StageLogic.GetCellStateStringsFromStringExpression(shape, height, width)
        );
        System.Random random = new System.Random();

        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                if (cellExpression.cells[i, j] != StageLogic.CellState.Empty)
                {
                    if (random.Next(0, 2) == 0)
                    {
                        cellExpression.FlipCell(i, j);
                    }
                }
            }
        }

        StageLogic.CellExpression targetCellExpression = cellExpression.Clone();
        int flip_count = 0;

        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                if (cellExpression.cells[i, j] != StageLogic.CellState.Empty)
                {
                    if (random.Next(0, 2) == 0)
                    {
                        targetCellExpression.FlipNeighbors(i, j);
                        flip_count++;
                        Debug.Log("Flipping neighbors: " + i + " " + j);
                    }
                }
            }
        }

        stageData.stageId = 1;
        stageData.width = width;
        stageData.height = height;
        stageData.initialPattern = StageLogic.GetStringExpressionFromCellExpression(cellExpression);
        stageData.targetPattern = StageLogic.GetStringExpressionFromCellExpression(targetCellExpression);
        stageData.isTopLeftTriangleDownward = isTopLeftTriangleDownward;
        stageData.starCondition = new StarCondition
        {
            toGet3Stars = flip_count,
            toGet2Stars = flip_count * 3 / 10 * 10
        };

        return stageData;
    }
}

