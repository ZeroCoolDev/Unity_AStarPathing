using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASNode : IHeapItem<ASNode>
{
    public bool bWalkable;
    public Vector3 worldPosition;
    public int row, col; // position in the grid
    public int movementPenalty;

    public int gCost;   // distance from starting node (on the current path)
    public int hCost;   // distance from target node
    public ASNode parent; // which node did we come from to this node
    int heapIndex;

    public ASNode(bool _walkwable, Vector3 _worldPos, int _row, int _col, int _movePenalty)
    {
        bWalkable = _walkwable;
        worldPosition = _worldPos;
        row = _row;
        col = _col;
        movementPenalty = _movePenalty;
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
        /*
            A.CompareTo(B) returns
                 1 if A > B
                -1 if A < B
                 0 if A == B
        */
        // The lower f(h)cost is better 
        int compare = fCost.CompareTo(comparator.fCost); // compare = -1 if this.fcost < comparator.fcost
        if(compare == 0)
        {
            compare = hCost.CompareTo(comparator.hCost); // compare = -1 if this.hcost < comparator.hcost
        }

        // -1 if this node has a better path than comparator so negate to return 1 for better path
        return -compare;
    }
}
