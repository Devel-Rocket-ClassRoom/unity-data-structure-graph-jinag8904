using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    private Stage stage;
    private Animator animator;

    public int currentTileId = -1;
    public int targetTileId = -1;

    public int sight = 1;

    public float moveSpeed = 10f;
    public bool isMoving = false;
    private Coroutine coMove = null;
    private Coroutine coMovePath = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0;

        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();
    }

    //private void Update()
    //{
    //    var direction = Sides.None;

    //    if (Input.GetKeyDown(KeyCode.W))
    //    {
    //        direction = Sides.Top;
    //    }

    //    else if (Input.GetKeyDown(KeyCode.A))
    //    {
    //        direction = Sides.Left;
    //    }

    //    else if (Input.GetKeyDown(KeyCode.S))
    //    {
    //        direction = Sides.Bottom;
    //    }

    //    else if (Input.GetKeyDown(KeyCode.D))
    //    {
    //        direction = Sides.Right;
    //    }

    //    if (direction != Sides.None)
    //    {
    //        var targetTile = stage.Map.tiles[currentTileId].adjacents[(int)direction];

    //        if (targetTile != null && targetTile.CanMove)
    //        {
    //            MoveTo(targetTile.id);
    //        }
    //    }
    //}

    public void WarpTo(int tileId)
    {
        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }
        isMoving = false;
        targetTileId = -1;

        currentTileId = tileId;
        transform.position = stage.GetTilePos(tileId);

        Debug.Log($"WarpTo()");

        stage.OnTileVisited(stage.Map.tiles[currentTileId]);
        stage.DecorateAllTiles();   // 타일 업데이트
    }

    public void MoveTo(int tileId)
    {
        if (isMoving) return;

        targetTileId = tileId;

        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }
        coMove = StartCoroutine(CoMove());
    }   

    private IEnumerator CoMove()
    {
        isMoving = true;
        animator.speed = 1;

        var startPos = transform.position;
        var endPos = stage.GetTilePos(targetTileId);
        var duration = Vector3.Distance(startPos, endPos) / moveSpeed;

        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        currentTileId = targetTileId;
        targetTileId = -1;

        transform.position = endPos;
        animator.speed = 0;

        Debug.Log($"player.currentTileId = {currentTileId}");
        stage.OnTileVisited(stage.Map.tiles[currentTileId]);
        stage.DecorateAllTiles();   // 타일 업데이트

        isMoving = false;
        coMove = null;
    }

    public void MovePath(List<GraphNode> path)
    {
        if (coMovePath != null)
        {
            StopCoroutine(coMovePath);
        }

        coMovePath = StartCoroutine(CoMovePath(path));
    }

    public IEnumerator CoMovePath(List<GraphNode> path)
    {
        while (coMove != null) yield return null;

        for (int i = 1; i < path.Count; i++)
        {
            if (path[i] == null) break;

            targetTileId = path[i].id;
            MoveTo(targetTileId);

            while (coMove != null) yield return null;
        }

        coMovePath = null;
    }
}