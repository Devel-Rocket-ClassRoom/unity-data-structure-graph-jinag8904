using UnityEngine;

public enum Sides 
{ 
    None = -1,
    Top, 
    Left,   
    Right,  
    Bottom     
}

public class Tile
{
    public int id;
    public int autoTileId;  // 에셋의 인덱스로 사용
    public int fowTileId;

    public Tile[] adjacents = new Tile[4];  // 인접 노드 배열 (없으면 null)

    public bool isVisited = false;

    public bool CanMove => autoTileId != (int)TileTypes.Empty;

    public void UpdateAutoTileId()  // 여기
    {
        autoTileId = 0;
        fowTileId = 0;

        for (int i = 0; i < adjacents.Length; i++) // i: 0, 1, 2, 3
        {
            if (adjacents[i] != null)
            {
                autoTileId |= 1 << i;  // 인덱스별로 시프트 연산
                fowTileId |= 1 << i;
            }
        }
    }

    public void RemoveAdjacents(Tile tile)
    {
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null) continue;

            if (adjacents[i].id == tile.id)
            {
                adjacents[i] = null;
                UpdateAutoTileId();
                break;
            }
        }
    }

    public void ClearAdjacents()
    {
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null) continue;

            adjacents[i].RemoveAdjacents(this);
            adjacents[i] = null;
        }

        UpdateAutoTileId();
    }
}