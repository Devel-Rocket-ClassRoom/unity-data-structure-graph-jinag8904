using UnityEngine;

public enum TileTypes
{
    Empty = -1,
    // 0 ~ 14 : 해안선 타일
    // 15 : 평지
    Grass = 15,
    Tree,
    Hills,
    Mountains,
    Towns,
    Castle,
    Monster
}

public class Map    // 타일 전체를 관리
{
    public int rows = 0;    // 행
    public int cols = 0;    // 열

    public Tile[] tiles;

    public void Init(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;

        tiles = new Tile[rows * cols];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var index = row * cols + col;   // 0, 1, 2...
                var adjacents = tiles[index].adjacents; // 인접 노드

                if ((row - 1) >= 0)
                {
                    adjacents[(int)Sides.Top] = tiles[index - cols];    // 위쪽 이웃
                }

                if ((col + 1) < cols)
                {
                    adjacents[(int)Sides.Right] = tiles[index + 1];     // 오
                }

                if ((col - 1) >= 0)
                {
                    adjacents[(int)Sides.Left] = tiles[index - 1];      // 왼
                }

                if ((row + 1) < rows)
                {
                    adjacents[(int)Sides.Bottom] = tiles[index + cols]; // 아
                }
            }
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdateAutoTileId();
        }
    }
}