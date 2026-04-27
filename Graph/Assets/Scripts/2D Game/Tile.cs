using UnityEngine;

// 1010

public enum Sides 
{ 
    Bottom, // 3
    Right,  // 2
    Left,   // 1
    Top     // 0
}

public class Tile
{
    public int id;
    public int autoTileId;  // 에셋의 인덱스로 사용

    public Tile[] adjacents = new Tile[4];  // 인접 노드 배열 (없으면 null)

    public bool isVisited = false;

    public void UpdateAutoTileId()
    {
        autoTileId = 0;
        
        for (int i = 0; i < adjacents.Length; i++ )
        {
            if (adjacents[i] != null)
            {
                autoTileId |= (1 << adjacents.Length - 1 - i);  // 인덱스별로 시프트 연산 -> 1000 / 0100 / 0010 / 0001
            }
        }
    }
}