using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;

    public Node(bool _walkwable, Vector3 _worldPos)
    {
        walkable = _walkwable;
        worldPosition = _worldPos;
    }
}
