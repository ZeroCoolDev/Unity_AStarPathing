using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Mathematically the index for any node's parent or children can be found using the following (integer division)
    For item A
    A.parent     =   (A.index - 1) / 2
    A.leftChild  =   (A.index * 2) + 1 
    A.rightChild =   (A.index * 2) + 2
*/
public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    int currentItemCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        ++currentItemCount;
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        --currentItemCount;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    public void UpdateItem(T item)
    {
        // When pathfinding we will only ever increase the priority, never decrease
        SortUp(item);
    }

    public int Count{
        get {
            return currentItemCount;
        }
    }

    bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    void SortDown(T item)
    {
        while(true)
        {
            int leftChildIndex = (item.HeapIndex * 2) + 1;
            int rightChildIndex = (item.HeapIndex * 2) + 2;

            int swapIndex = 0;
            if(leftChildIndex < currentItemCount)
            {
                swapIndex = leftChildIndex; // default to left child if there is one
                if(rightChildIndex < currentItemCount)
                {
                    if(items[leftChildIndex].CompareTo(items[rightChildIndex]) < 0) // the right child is bigger
                    {
                        swapIndex = rightChildIndex;
                    }
                }

                if(item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                { // parent is less than it's highest child so it's in the correct spot
                    break;
                }
            }
            else
            { // parent has no children so already in the correct spot
                break;
            }
        }
    }

    // Move the item up then heap
    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex-1)/2;

        while(true)
        {
            T parentItem = items[parentIndex];
            /*
                A.CompareTo(B) returns
                     1 if B > A
                    -1 if B < A
                     0 if B == A
            */
            if(item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }
        }
    }

    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemA.HeapIndex;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex {
        get;
        set;
    }
}
