using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PriorityQueue<TElement, TPriority>
{
    private List<(TElement Element, TPriority Priority)> heap = new List<(TElement, TPriority)>();
    private Comparer<TPriority> comparer = Comparer<TPriority>.Default;
    public int Count => heap.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        heap.Add((element, priority));
        HeapifyUp(heap.Count - 1);
    }
    public TElement Dequeue()
    {
        var root = heap[0].Element;
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);
        return root;
    }
    public TElement Peek()
    {
        return heap[0].Element;
    }

    public void HeapifyUp(int index)
    {
        while (true)
        {
            int parentIndex = (index - 1) / 2;
            if (comparer.Compare(heap[index].Priority, heap[parentIndex].Priority) < 0)
            {
                var temp = heap[index];
                heap[index] = heap[parentIndex];
                heap[parentIndex] = temp;
                index = parentIndex;
            }
            else
            {
                break;
            }
        }
    }
    public void HeapifyDown(int index)
    {
        while (true)
        {
            int leftChild = index * 2 + 1;
            int rightChile = index * 2 + 2;
            int min = index;
            if (leftChild < heap.Count && comparer.Compare(heap[leftChild].Priority, heap[min].Priority) < 0)
            {
                min = leftChild;
            }
            if (rightChile < heap.Count && comparer.Compare(heap[rightChile].Priority, heap[min].Priority) < 0)
            {
                min = rightChile;
            }
            if (min == index)
            {
                break;
            }
            var temp = heap[index];
            heap[index] = heap[min];
            heap[min] = temp;
            index = min;
        }

    }
    public void Clear()
    {
        heap.Clear();
    }
}