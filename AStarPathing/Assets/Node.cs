using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int row, col; // position in the grid

    public int gCost;   // distance from starting node (on the current path)
    public int hCost;   // distance from target node
    public Node parent; // which node did we come from to this node

    public Node(bool _walkwable, Vector3 _worldPos, int _row, int _col)
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
}
