using System.Collections.Generic;
using UnityEngine;

public class GraphSearch
{
    private Graph graph;
    public List<GraphNode> path = new();

    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    public void DFS(GraphNode start_node)   // 깊이 우선 
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var stack = new Stack<GraphNode>();

        stack.Push(start_node);
        visited.Add(start_node);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            path.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent)) continue;

                visited.Add(adjacent);
                stack.Push(adjacent);
            }
        }
    }

    public void DFSRecursion(GraphNode start_node)
    {
        path.Clear();
        DFSRecursion(start_node, new HashSet<GraphNode>());
    }

    private void DFSRecursion(GraphNode current_node, HashSet<GraphNode> visited)
    {
        path.Add(current_node);
        visited.Add(current_node);

        foreach (var adjacent in current_node.adjacents)
        {
            if (!adjacent.CanVisit || visited.Contains(adjacent)) continue;

            DFSRecursion(adjacent, visited);
        }
    }

    public void BFS(GraphNode start_node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();

        queue.Enqueue(start_node);
        visited.Add(start_node);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            path.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent)) continue;

                visited.Add(adjacent);
                queue.Enqueue(adjacent);
            }
        }
    }

    public bool BFSRoute(GraphNode start_node, GraphNode end_node)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();

        queue.Enqueue(start_node);
        visited.Add(start_node);

        var found = false;

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            if (currentNode == end_node)
            {
                found = true;
                break;
            }
            
            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent)) continue;

                visited.Add(adjacent);
                queue.Enqueue(adjacent);
                adjacent.previous = currentNode;
            }
        }

        if (!found) return false;

        GraphNode step = end_node;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }

    public bool Dijkstra(GraphNode start_node, GraphNode end_node)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var pq = new PriorityQueue<GraphNode, int>();
        var distances = new int[graph.nodes.Length];

        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = int.MaxValue;
        }

        distances[start_node.id] = 0;
        pq.Enqueue(start_node, distances[start_node.id]);

        var found = false;

        while (pq.Count > 0)
        {
            var currentNode = pq.Dequeue();

            if (visited.Contains(currentNode)) continue;

            visited.Add(currentNode);

            if (currentNode == end_node)
            {
                found = true;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent)) continue;

                var newDistance = distances[currentNode.id] + adjacent.weight;

                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    adjacent.previous = currentNode;    // 나중에 길 찾기 용도
                    pq.Enqueue(adjacent, distances[adjacent.id]);
                }
            }
        }

        if (!found) return false;

        GraphNode step = end_node;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }

    private int Heuristic(GraphNode a, GraphNode b)
    {
        int ax = a.id % graph.cols;
        int ay = a.id / graph.cols;

        int bx = b.id % graph.cols;
        int by = b.id / graph.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public bool AStar(GraphNode start_node, GraphNode end_node)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var pq = new PriorityQueue<GraphNode, int>();
        var distances = new int[graph.nodes.Length];

        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = int.MaxValue;
        }

        distances[start_node.id] = 0;
        pq.Enqueue(start_node, distances[start_node.id] + Heuristic(start_node, end_node));

        var found = false;

        while (pq.Count > 0)
        {
            var currentNode = pq.Dequeue();

            if (visited.Contains(currentNode)) continue;

            if (currentNode == end_node)
            {
                found = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent)) continue;

                var newDistance = distances[currentNode.id] + adjacent.weight;

                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    adjacent.previous = currentNode;
                    pq.Enqueue(adjacent, distances[adjacent.id] + Heuristic(adjacent, end_node));
                }
            }
        }

        if (!found) return false;

        GraphNode step = end_node;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }
}