using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class StageLogic : MonoBehaviour
{
    public struct Stage 
    {
        Board initialBoard;
        Board currentBoard;
        Board answerBoard;
        int minMoves;
    }
    public struct Tile
    {
        public int index;
        public bool isUpward; // true if the triangle is △ / ▲, false if the triangle is ▽ / ▼
        public bool isFront; // true if the triangle is △ / ▽, true if the triangle is ▲ / ▼, false 
        public int[] neighbors; // neighbors[0]: to the right, [1]: to the left, [2]: to the top or bottom    
    }
    // collection of tiles
    public struct Board 
    {
        public int size;
        public Tile[] tiles;
        public int boardState; // bitstring of length size, boardState & (1 << i) == 1 if the i-th tile is front.
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
        public bool isTopLeftTriangleDownward; // true if the top left triangle is ▽ / ▼, false if the top left triangle is △ / ▲
        public CellState[,] cells;

        // initializer
        public CellExpression(int height, int width, bool isTopLeftTriangleDownward, CellState[,] cells)
        {
            this.height = height;
            this.width = width;
            this.isTopLeftTriangleDownward = isTopLeftTriangleDownward;
            this.cells = cells;
        }
        
        private static Board GenerateBoard(CellExpression cellExpression)
        {
            int height = cellExpression.height;
            int width = cellExpression.width;
            bool isTopLeftTriangleDownward = cellExpression.isTopLeftTriangleDownward;
            CellState[,] cells = cellExpression.cells;

            int[,] tileIndices = new int[height, width];

            int size = CalcBoardSize(cells);
            Tile[] tiles = new Tile[size];

            int boardState = 0;

            int cnt = 0;
            for (int ih = 0; ih < height; ++ih) {
                for (int iw = 0; iw < width; ++iw) {
                    if (cells[ih, iw] == CellState.Empty) {
                        tileIndices[ih, iw] = -1;
                    } else {
                        tiles[cnt] = new Tile();
                        tiles[cnt].index = cnt;
                        tiles[cnt].isFront = cells[ih, iw] == CellState.Front;
                        tiles[cnt].isUpward = isTopLeftTriangleDownward ? (ih + iw) % 2 == 0 : (ih + iw) % 2 == 1;
                        if (tiles[cnt].isFront) {
                            boardState |= 1 << cnt;
                        }
                        tileIndices[ih, iw] = cnt++;
                    }
                }
            }

            for (int ih = 0; ih < height; ++ih) {
                for (int iw = 0; iw < width; ++iw) {
                    if (tileIndices[ih, iw] == -1) {
                        continue;
                    }
                    if (iw != width - 1) {
                        if (tileIndices[ih, iw + 1] != -1) {
                            tiles[tileIndices[ih, iw]].neighbors[0] = tileIndices[ih, iw + 1];
                        }
                    }
                    if (iw != 0) {
                        if (tileIndices[ih, iw - 1] != -1) {
                            tiles[tileIndices[ih, iw]].neighbors[1] = tileIndices[ih, iw - 1];
                        }
                    }
                    if (ih != 0 && IsDownwardTile(ih, iw, isTopLeftTriangleDownward)) {
                        if (tileIndices[ih - 1, iw] != -1) {
                            tiles[tileIndices[ih, iw]].neighbors[2] = tileIndices[ih - 1, iw];
                        }
                    }
                    if (ih != height - 1 && !IsDownwardTile(ih, iw, isTopLeftTriangleDownward)) {
                        if (tileIndices[ih + 1, iw] != -1) {
                            tiles[tileIndices[ih, iw]].neighbors[2] = tileIndices[ih + 1, iw];
                        }
                    }
                }
            }

            return new Board() {
                size = size,
                tiles = tiles,
                boardState = 0
            };
        }
        

        
        private static int CalcBoardSize(CellState[,] board)
        {
            int size = 0;
            for (int i = 0; i < board.GetLength(0); ++i) {
                for (int j = 0; j < board.GetLength(1); ++j) {
                    if (board[i, j] != CellState.Empty) {
                        size++;
                    }
                }
            }
            return size;
        }
    }

    private static bool IsDownwardTile(int ih, int iw, bool isTopLeftTriangleDownward)
    {
        return isTopLeftTriangleDownward ? (ih + iw) % 2 == 0 : (ih + iw) % 2 == 1;
    }
    private int GetBoardBitstring(int size, Tile[] tiles)
    {
        int bitstring = 0;
        for (int i = 0; i < size; ++i) {
            bitstring |= (tiles[i].isUpward ? 1 : 0) << (i);
        }

        return bitstring;
    }

    private static int Popcount(int n)
    {
        int count = 0;
        while (n != 0)
        {
            n &= (n - 1);
            count++;
        }
        return count;
    }

    private static int Msb(int n) {
        if (n == 0) {
            return -1;
        }
        int msb = 0;
        while (n != 0)
        {
            n >>= 1;
            msb++;
        }
        return msb;
    }
}