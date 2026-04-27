using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    public enum Algorithm { DFS, BFS, DFSRecursive, BFSRoute, Dijkstra, AStar }

    public Transform uiNodeRoot;

    public UiGraphNode nodePrefab;

    private List<UiGraphNode> uiNodes = new();

    private Graph graph;

    public Algorithm algorithm = Algorithm.DFS;
    public int startId;
    public int endId;

    void Start()
    {
        int[,] map = new int[5, 5]
        { 
            { 1, 2, 3, 1, 1 },
            { 9, 5, 1, 2, 10 },
            { 12, 1, 11, 1, 1 },
            { 1, 1, 5, 2, 1 },
            { 4, 3, 1, 5, 1 }
        };

        graph = new();
        graph.Init(map);
        InitUiNodes(graph);
    }

    private void InitUiNodes(Graph graph)
    {
        foreach (var node in graph.nodes)
        {
            var uiNode = Instantiate(nodePrefab, uiNodeRoot);
            uiNode.SetNode(node);
            uiNode.Reset();
            uiNodes.Add(uiNode);
        }
    }

    private void ResetUiNodes()
    {
        foreach (var uiNode in uiNodes)
        {
            uiNode.Reset();
        }
    }

    [ContextMenu("Search")]

    public void Search()
    {
        var search = new GraphSearch();
        search.Init(graph);

        switch (algorithm)
        {
            case Algorithm.DFS:
                search.DFS(graph.nodes[startId]);
                break;
            case Algorithm.BFS:
                search.BFS(graph.nodes[startId]);
                break;
            case Algorithm.DFSRecursive:
                search.DFSRecursion(graph.nodes[startId]);
                break;
            case Algorithm.BFSRoute:
                search.BFSRoute(graph.nodes[startId], graph.nodes[endId]);
                break;
            case Algorithm.Dijkstra:
                Debug.Log(search.Dijkstra(graph.nodes[startId], graph.nodes[endId]));
                break;
            case Algorithm.AStar:
                search.AStar(graph.nodes[startId], graph.nodes[endId]);
                break;
        }

        ResetUiNodes();

        if (search.path.Count <= 1)
        {
            if (search.path.Count == 1)
            {
                var only = search.path[0];
                uiNodes[only.id].SetColor(Color.red);
            }

            return;
        }

        for (int i = 0; i < search.path.Count; i++)
        {
            var node = search.path[i];
            var color = Color.Lerp(Color.red, Color.blue, (float) i/(search.path.Count - 1));
            uiNodes[node.id].SetColor(color);
            uiNodes[node.id].SetText($"ID: {node.id}\nWeight: {node.weight}\nPath: {i}");
        }
    }
}