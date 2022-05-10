using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASNode : IHeapItem<ASNode>
{
    public bool walkable;
    public Vector3 worldPosition;
    public int row, col; // position in the grid

    public int gCost;   // distance from starting node (on the current path)
    public int hCost;   // distance from target node
    public ASNode parent; // which node did we come from to this node
    int heapIndex;

    public ASNode(bool _walkwable, Vector3 _worldPos, int _row, int _col)
    {
        walkable = _walkwable;
        worldPosition = _worldPos;
        row = _row;
        col = _col;
    }

    // No one can ever assign the fCost, it is always calculated from g & h costs
    public int fCost {
        get {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get {
            return heapIndex;
        }
        set {
            heapIndex = value;
        }
    }

    // returns 1 if this item has a higher priority than comparator (this item's fcost or hcost is lower than comparator)
    public int CompareTo(ASNode comparator)
    {
        int compare = fCost.CompareTo(comparator.fCost); // -1 if this items is higher prio
        if(compare == 0)
        {
            compare = hCost.CompareTo(comparator.hCost); // -1 if this item is higher prio
        }

        return -compare; // negate to return 1 for indicating this item is higher prio
    }
}
