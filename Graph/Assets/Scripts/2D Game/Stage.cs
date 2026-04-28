using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject tilePrefab;
    private GameObject[] tileObjs;

    public int mapWidth = 20;
    public int mapHeight = 20;

    [Range(0f, 0.9f)]
    public float erodePercent = 0.5f;
    public int erodeIteration = 2;

    [Range(0f, 0.9f)]
    public float lakePercent;
    [Range(0f, 0.9f)]
    public float treePercent;
    [Range(0f, 0.9f)]
    public float hillPercent;
    [Range(0f, 0.9f)]
    public float mountainPercent;
    [Range(0f, 0.9f)]
    public float townPercent;
    [Range(0f, 0.9f)]
    public float monsterPercent;

    public Vector2 tileSize = new(16, 16);

    public Sprite[] islandSprites;
    public Sprite[] fogSprites;

    public Map Map => map;
    private Map map;

    public Camera cam;

    private int prevTileId = -1;

    public PlayerMovement playerPrefab;
    private PlayerMovement player;

    public Vector3 FirstTilePos
    {
        get
        {
            var pos = transform.position;
            pos.x -= (mapWidth * tileSize.x * 0.5f);
            pos.y += (mapHeight * tileSize.y * 0.5f);
            pos.x -= tileSize.x * 0.5f;
            pos.y += tileSize.y * 0.5f;
            return pos;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetStage();
        }

        if (tileObjs != null)
        {
            int currentTileId = ScreenPosToTileId(Input.mousePosition);
            if (prevTileId != currentTileId)
            {
                tileObjs[currentTileId].GetComponent<SpriteRenderer>().color = Color.green;
                if (prevTileId >= 0 && prevTileId < tileObjs.Length)
                {
                    tileObjs[prevTileId].GetComponent<SpriteRenderer>().color = Color.white;
                }

                prevTileId = currentTileId;
            }
        }
    }

    private void ResetStage()
    {
        map = new();
        map.Init(mapHeight, mapWidth);
        map.CreateIsland(erodePercent, erodeIteration, lakePercent, treePercent, hillPercent, mountainPercent, townPercent, monsterPercent);
        CreateGrid();
        CreatePlayer();

        // 안개 업데이트
        map.UpdateFogs(player.boundary, WorldPosToTileId(player.transform.position));
    }

    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }

        player = Instantiate(playerPrefab);
        player.MoveTo(map.startTile.id);
    }

    private void CreateGrid()
    {
        if (tileObjs != null)
        {
            foreach (var tile in tileObjs)
            {
                Destroy(tile.gameObject);
            }
        }

        tileObjs = new GameObject[mapWidth * mapHeight];

        var pos = FirstTilePos;

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                var tileId = i * mapWidth + j;

                var newGo = Instantiate(tilePrefab, transform);
                newGo.transform.position = pos;
                pos.x += tileSize.x;

                tileObjs[tileId] = newGo;
                DecorateTile(tileId);
            }

            pos.x = FirstTilePos.x;
            pos.y -= tileSize.y;
        }
    }

    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];
        var tileGo = tileObjs[tileId];

        var renderer = tileGo.GetComponent<SpriteRenderer>();
        if (tile.autoTileId != (int)TileTypes.Empty)
        {
            //foreach (var t in tile.adjacents)
            //{
                //if (t.isVisited)
                //{
                    renderer.sprite = islandSprites[tile.autoTileId];
                //    break;
                //}
                //else
                //{
                    //renderer.sprite = fogSprites[tile.autoTileId];
                //}
            //}
        }
        else
        {
            renderer.sprite = null;
        }
    }

    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(transform.position.z - cam.transform.position.z);
        return WorldPosToTileId(cam.ScreenToWorldPoint(screenPos));
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        int xIndex = (int)((worldPos.x - FirstTilePos.x) / tileSize.x + 0.5f);
        int yIndex = (int)((FirstTilePos.y - worldPos.y) / tileSize.y + 0.5f);
        xIndex = Mathf.Clamp(xIndex, 0, mapWidth - 1);
        yIndex = Mathf.Clamp(yIndex, 0, mapHeight - 1);
        return yIndex * mapWidth + xIndex;
    }

    public Vector3 GetTilePos(int tileId)
    {
        var pos = Vector3.zero;

        var y = tileId / mapWidth;
        var x = tileId % mapWidth;

        return GetTilePos(y, x);
    }

    public Vector3 GetTilePos(int y, int x)
    {
        var pos = FirstTilePos;

        pos.x += x * tileSize.x;
        pos.y -= y * tileSize.y;

        return pos;
    }
}