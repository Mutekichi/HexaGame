using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class StageLogic
{
    void Start()
    {
        Test();
    }
    public struct Stage 
    {
        public Board initialBoard;
        public Board currentBoard;
        public Board answerBoard;
        public int minMoves;
    }

    public struct Tile
    {
        public int index;
        public bool isUpward;
        public bool isFront;
        public int[] neighbors;
    }

    public struct Board 
    {
        public int size;
        public Tile[] tiles;
        public BitArray boardState;
    }

    public enum CellState
    {
        Empty,
        Front,
        Back
    }

    public struct CellExpression
    {
        public int height;
        public int width;
        public bool isTopLeftTriangleDownward;
        public CellState[,] cells;

        public CellExpression(int height, int width, bool isTopLeftTriangleDownward, CellState[,] cells)
        {
            this.height = height;
            this.width = width;
            this.isTopLeftTriangleDownward = isTopLeftTriangleDownward;
            this.cells = cells;
        }
        
        public static Board GenerateBoard(CellExpression cellExpression)
        {
            int height = cellExpression.height;
            int width = cellExpression.width;
            bool isTopLeftTriangleDownward = cellExpression.isTopLeftTriangleDownward;
            CellState[,] cells = cellExpression.cells;

            int[,] tileIndices = new int[height, width];

            int size = CalcBoardSize(cells);
            Tile[] tiles = new Tile[size];
            BitArray boardState = new BitArray(size);

            int cnt = 0;
            for (int ih = 0; ih < height; ++ih)
            {
                for (int iw = 0; iw < width; ++iw)
                {
                    if (cells[height - 1 - ih, iw] == CellState.Empty)
                    {
                        tileIndices[ih, iw] = -1;
                    }
                    else
                    {
                        tiles[cnt] = new Tile
                        {
                            index = cnt,
                            isFront = cells[height - 1 - ih, iw] == CellState.Front,
                            isUpward = isTopLeftTriangleDownward ? (ih + iw) % 2 == 0 : (ih + iw) % 2 == 1,
                            neighbors = new int[3] { -1, -1, -1 }
                        };
                        
                        boardState[cnt] = tiles[cnt].isFront;
                        tileIndices[ih, iw] = cnt++;
                    }
                }
            }

            for (int ih = 0; ih < height; ++ih)
            {
                for (int iw = 0; iw < width; ++iw)
                {
                    if (tileIndices[ih, iw] == -1) continue;

                    int currentIndex = tileIndices[ih, iw];

                    if (iw < width - 1 && tileIndices[ih, iw + 1] != -1)
                        tiles[currentIndex].neighbors[0] = tileIndices[ih, iw + 1];

                    if (iw > 0 && tileIndices[ih, iw - 1] != -1)
                        tiles[currentIndex].neighbors[1] = tileIndices[ih, iw - 1];

                    if (IsDownwardTile(ih, iw, isTopLeftTriangleDownward))
                    {
                        if (ih > 0 && tileIndices[ih - 1, iw] != -1)
                            tiles[currentIndex].neighbors[2] = tileIndices[ih - 1, iw];
                    }
                    else
                    {
                        if (ih < height - 1 && tileIndices[ih + 1, iw] != -1)
                            tiles[currentIndex].neighbors[2] = tileIndices[ih + 1, iw];
                    }
                }
            }

            return new Board
            {
                size = size,
                tiles = tiles,
                boardState = boardState
            };
        }

        private static int CalcBoardSize(CellState[,] board)
        {
            int size = 0;
            for (int i = 0; i < board.GetLength(0); ++i)
            {
                for (int j = 0; j < board.GetLength(1); ++j)
                {
                    if (board[i, j] != CellState.Empty)
                    {
                        size++;
                    }
                }
            }
            return size;
        }
    }

    public static class BoardHelper
    {
        public static string GetBoardStateString(Board board)
        {
            char[] bits = new char[board.size];
            for (int i = 0; i < board.size; i++)
            {
                bits[i] = board.boardState[i] ? '1' : '0';
            }
            return new string(bits);
        }

        public static string GetFormattedBoardState(Board board, int groupSize = 4)
        {
            string binary = GetBoardStateString(board);
            return string.Join("_", 
                binary.Reverse()
                    .Select((c, i) => new { c, i })
                    .GroupBy(x => x.i / groupSize)
                    .Select(g => new string(g.Select(x => x.c).Reverse().ToArray()))
                    .Reverse());
        }

        public static string GetBoardVisualization(Board board)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Board Size: {board.size}");
            sb.AppendLine($"Board State: {GetBoardStateString(board)}");
            sb.AppendLine($"Formatted State: {GetFormattedBoardState(board)}");
            sb.AppendLine("\nTiles:");
            
            for (int i = 0; i < board.size; i++)
            {
                Tile tile = board.tiles[i];
                sb.AppendLine($"\nTile {i}:");
                sb.AppendLine($"  Direction: {(tile.isUpward ? "△" : "▽")}");
                sb.AppendLine($"  Face: {(tile.isFront ? "Front" : "Back")}");
                sb.AppendLine($"  Neighbors:");
                sb.AppendLine($"    Right: {FormatNeighbor(tile.neighbors[0])}");
                sb.AppendLine($"    Left: {FormatNeighbor(tile.neighbors[1])}");
                sb.AppendLine($"    {(tile.isUpward ? "Bottom" : "Top")}: {FormatNeighbor(tile.neighbors[2])}");
            }
            
            return sb.ToString();
        }

        private static string FormatNeighbor(int neighbor)
        {
            return neighbor == -1 ? "none" : neighbor.ToString();
        }
    }

    public static CellState[,] GetCellStateStringsFromStringExpression(string[] stringExpression, int height, int width)
    {
        CellState[,] cellExpression = new CellState[height, width];
        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                switch (stringExpression[i][j])
                {
                    case '0':
                        cellExpression[i, j] = CellState.Empty;
                        break;
                    case '1':
                        cellExpression[i, j] = CellState.Front;
                        break;
                    case '2':
                        cellExpression[i, j] = CellState.Back;
                        break;
                    default:
                        throw new ArgumentException($"Invalid character '{stringExpression[i][j]}' in the string expression at position ({i}, {j})");
                }
            }
        }
        return cellExpression;
    }

    private static bool IsDownwardTile(int ih, int iw, bool isTopLeftTriangleDownward)
    {
        return isTopLeftTriangleDownward ? (ih + iw) % 2 == 0 : (ih + iw) % 2 == 1;
    }

    public static void Test()
    {
        string[] stringExpression = new string[] {
            "0212120",
            "1212121",
            "2121212",
            "0000020"
        };

        int sampleHeight = 4;
        int sampleWidth = 7;

        try
        {
            CellState[,] cellStateStrings = GetCellStateStringsFromStringExpression(stringExpression, sampleHeight, sampleWidth);
            CellExpression sampleCellExpression = new(sampleHeight, sampleWidth, true, cellStateStrings);
            Board sampleBoard = CellExpression.GenerateBoard(sampleCellExpression);

            Debug.Log(BoardHelper.GetBoardVisualization(sampleBoard));
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during board generation: {e.Message}");
            Debug.LogException(e);
        }
    }
}