using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Stage stage;
    private Animator animator;

    private int currentTileId;

    public int sight = 1;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        //animator.speed = 0;

        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();
    }

    private void Update()
    {
        var direction = Sides.None;

        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = Sides.Top;
        }

        else if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Sides.Left;
        }

        else if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Sides.Bottom;
        }

        else if (Input.GetKeyDown(KeyCode.D))
        {
            direction = Sides.Right;
        }

        if (direction != Sides.None)
        {
            var targetTile = stage.Map.tiles[currentTileId].adjacents[(int)direction];

            if (targetTile != null && targetTile.CanMove)
            {
                MoveTo(targetTile.id);
            }
        }
    }

    public void MoveTo(int tileId)
    {
        currentTileId = tileId;
        transform.position = stage.GetTilePos(currentTileId);

        for (int i = -sight; i <= sight; i++)  // ex. 시야 3 -> 실제로는 현위치+상하좌우 3칸씩 -> 7x7
        {
            for (int j = -sight; j <= sight; j++)
            {
                int x = currentTileId % stage.mapWidth + i;
                int y = currentTileId / stage.mapHeight + j;

                if (x < 0 || y < 0 || x >= stage.mapWidth || y >= stage.mapHeight) continue;    // 맵 범위를 넘어가면 그대로 둠

                int viewTileId = y * stage.mapWidth + x;    // 밝힐 타일의 Id 계산
                stage.Map.tiles[viewTileId].Visit();        // visit 플래그 설정
            }
        }

        stage.DecorateAllTiles();   // 타일 업데이트
    }
}