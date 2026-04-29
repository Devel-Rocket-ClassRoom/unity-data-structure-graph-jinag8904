using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using static GraphTest;

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
    private Graph graph;

    private Camera cam;

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

    private void Start()
    {
        cam = Camera.main;
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

            if (Input.GetMouseButtonDown(0))    // 길찾기
            {
                int startId = player.isMoving ? player.targetTileId : player.currentTileId;

                if (startId == -1) startId = player.currentTileId;

                if (startId >= 0 && startId < graph.nodes.Length && currentTileId >= 0 && currentTileId < graph.nodes.Length)
                {
                    Search(startId, currentTileId);
                }
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

        graph = new();
        int[,] graphMap = new int[mapHeight, mapWidth];

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                var weight = 1;
                var tileId = map.tiles[i * mapWidth + j].autoTileId;

                switch (tileId)
                {
                    case (int)TileTypes.Empty:
                    case (int)TileTypes.Mountains:
                        weight = -1;
                        break;

                    case (int)TileTypes.Hills:
                        weight = 4;
                        break;

                    case (int)TileTypes.Tree:
                        weight = 2;
                        break;
                }

                graphMap[i, j] = weight;
            }
        }

        graph.Init(graphMap);
    }

    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }

        player = Instantiate(playerPrefab);
        player.WarpTo(map.startTile.id);
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

        if (tile == null) return;

        if (tile.isVisited) // 방문한 타일 -> 해당 타일 스프라이트 적용
        {
            if (tile.autoTileId != (int)TileTypes.Empty)
            {
                renderer.sprite = islandSprites[tile.autoTileId];
            }
            else
            {
                renderer.sprite = null;
            }
        }
        else // 방문하지 않은(공개되지 않은) 타일 -> 안개 적용
        {
            if (tile.autoTileId != (int)TileTypes.Empty && tile.fogTileId != (int)TileTypes.Empty && tile.fogTileId != -1)
            {
                renderer.sprite = fogSprites[tile.fogTileId];
            }
            else
            {
                renderer.sprite = null;
            }
        }
    }

    public void DecorateAllTiles()
    {
        for (int i = 0; i < tileObjs.Length; i++)
        {
            DecorateTile(i);
        }
    }

    public void OnTileVisited(Tile tile)
    {
        int centerX = tile.id % mapWidth;
        int centerY = tile.id / mapWidth;

        for (int i = -player.sight; i <= player.sight; i++)  // ex. 시야 3 -> 실제로는 현위치+상하좌우 3칸씩 -> 7x7
        {
            for (int j = -player.sight; j <= player.sight; j++)
            {
                int x = centerX + i;
                int y = centerY + j;

                if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) continue;    // 맵 범위를 넘어가면 그대로 둠

                int viewTileId = y * mapWidth + x;    // 밝힐 타일의 Id 계산
                Map.tiles[viewTileId].Visit();        // visit 플래그 설정                
            }
        }

        var radius = player.sight + 1;  // 시야 바로 바깥쪽 타일들(안개) 업데이트 용도
        for (int i = -radius; i <= radius; i++) 
        {
            for (int j = -radius; j <= radius; j++)
            {
                int x = centerX + i;
                int y = centerY + j;

                if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) continue;

                int viewTileId = y * mapWidth + x;
                DecorateTile(viewTileId);      
            }
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

    public void Search(int startId, int endId)
    {
        if (startId < 0 || startId >= graph.nodes.Length || endId < 0 || endId >= graph.nodes.Length)
        {
            Debug.LogError($"인덱스 범위 초과 Start: {startId}, End: {endId}, Max: {graph.nodes.Length}");
            return;
        }

        var search = new GraphSearch();
        search.Init(graph);

        search.AStar(graph.nodes[startId], graph.nodes[endId]);
        Debug.Log("search.AStar()");

        if (search.path.Count <= 1)
        {
            Debug.Log("search.path.Count <= 1");

            if (search.path.Count == 1)
            {
                // 시작점, 끝점이 같은 경우
            }

            return;
        }

         player.MovePath(search.path);
    }
}