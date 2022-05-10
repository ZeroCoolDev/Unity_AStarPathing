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
public class ASHeap<T> where T : IHeapItem<T>
{
    T[] items;
    int currentItemCount;

    public ASHeap(int maxHeapSize)
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

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    void SortDown(T item)
    {
        int failsafe = 900;
        while(true)
        {
            if(--failsafe < 0)
            {
                Debug.Log("SortDown Failsafe Triggered - Infinite loop detected");
                return;
            }
            int leftChildIndex = (item.HeapIndex * 2) + 1;
            int rightChildIndex = (item.HeapIndex * 2) + 2;

            int swapIndex = 0;
            // Get the smallest child of the two
            if(leftChildIndex < currentItemCount)
            {
                swapIndex = leftChildIndex; // default to left child if there is one
                if(rightChildIndex < currentItemCount)
                {
                    if(items[leftChildIndex].CompareTo(items[rightChildIndex]) < 0) // the left childs fcost is greater than the right child
                    {
                        swapIndex = rightChildIndex;
                    }
                }

                if(item.CompareTo(items[swapIndex]) < 0) // this item has a larger fcost than the lowest child so swap
                {
                    Swap(item, items[swapIndex]);
                }
                else
                { // parent is less than it's highest child so it's in the correct spot
                    return;
                }
            }
            else
            { // parent has no children so already in the correct spot
                return;
            }
        }
    }

    // Move the item up then heap
    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex-1)/2;

        int failsafe = 900;
        while(true)
        {
            if(--failsafe < 0)
            {
                Debug.Log("SortUp Failsafe Triggered - Infinite loop detected");
                return;
            }
            T parentItem = items[parentIndex];
            if(item.CompareTo(parentItem) > 0) // this means item's fcost is smaller than parent's fcost
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
        itemB.HeapIndex = itemAIndex;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex {
        get;
        set;
    }
}
