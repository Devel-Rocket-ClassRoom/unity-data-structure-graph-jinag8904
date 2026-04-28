using UnityEngine;
using System.Linq;

public enum TileTypes
{
    Empty = -1,
    // 0 ~ 14 : 해안선 타일
    Grass = 15, // 평지
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
    public Tile[] CoastTiles => tiles.Where(t => t.autoTileId >= 0 && t.autoTileId < (int)TileTypes.Grass).ToArray();   // 해안선
    public Tile[] LandTiles => tiles.Where(t => t.autoTileId == (int)TileTypes.Grass).ToArray(); // 육지

    public Tile startTile;
    public Tile castleTile;

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
                var index = row * cols + col;   // 0*5+0(0), 0*5+1(1)...
                var adjacents = tiles[index].adjacents; // 인접 노드 배열

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

    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (tiles[rand], tiles[i]) = (tiles[i], tiles[rand]);  // swap
        }
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);

        int total = Mathf.FloorToInt(tiles.Length * percent);
        for (int i = 0; i < total; i++)
        {
            if (tileType == TileTypes.Empty)
            {
                tiles[i].ClearAdjacents();
            }

            tiles[i].autoTileId = (int)tileType;
        }
    }

    public bool CreateIsland(
        float erodePercent, int erodeIterations, float lakePercent, 
        float treePercent, float hillPercent, float mountainPercent, float townPercent, float monsterPercent)    // Castle 1개
    {
        for (int i = 0; i < erodeIterations; i++)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);
        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Towns);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        var towns = tiles.Where(x => x.autoTileId == (int)TileTypes.Towns).ToArray();
        ShuffleTiles(towns);

        startTile = towns[0];
        castleTile = towns[1];
        castleTile.autoTileId = (int)TileTypes.Castle;

        return true;
    }

    public void UpdateFogs(int boundary, int tileId)
    {
        
    }
}