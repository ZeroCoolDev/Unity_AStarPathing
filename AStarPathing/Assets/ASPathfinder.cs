using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class ASPathfinder : MonoBehaviour
{
// deprecated but keeping for now
//    public Transform seeker, target;

    ASRequestPathManager requestManager;
    ASGrid grid;

    void Awake()
    {
        // this and Grid.cs must be on the same game object
        grid = GetComponent<ASGrid>();
        requestManager = GetComponent<ASRequestPathManager>();
    }

// deprecated but keeping for now
    // void Update()
    // {
    //     FindPath(seeker.position, target.position);
    // }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] waypoints = new Vector3[0];
        bool bPathSuccess = false;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        ASNode startNode = grid.NodeFromWorldPoint(startPos);
        ASNode targetNode = grid.NodeFromWorldPoint(targetPos);
    
        ASHeap<ASNode> openSet = new ASHeap<ASNode>(grid.MaxSize);
        openSet.Add(startNode);
        HashSet<ASNode> closedSet = new HashSet<ASNode>();

        // There is no possible path if either are unwalkwable
        if(startNode.bWalkable && targetNode.bWalkable)
        {
            int failsafe = Mathf.RoundToInt(grid.gridWorldSize.x * grid.gridWorldSize.y * 2);
            while(openSet.Count > 0)
            {
                --failsafe;
                if(failsafe <= 0 )
                {
                    UnityEngine.Debug.Log("ERROR: Failsafe hit, infinite loop detected");
                    bPathSuccess = false;
                    break;
                }

                // Find the node with the lowest FCost that we haven't gone to yet
                ASNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if(currentNode == targetNode)// path has been found 
                { 
                    sw.Stop();
                    //UnityEngine.Debug.Log("Path found: " + sw.ElapsedMilliseconds + "ms");
                    bPathSuccess = true;
                    break; 
                }

                // Check all neighbors to see if any are not walkable or already closed
                foreach(ASNode neighbor in grid.GetNeighbors(currentNode))
                {
                    //UnityEngine.Debug.Log("Testing neighbors for grid[" + currentNode.row + "]["+ currentNode.col +"]");
                    // not walkaable or already closed, skip
                    if(!neighbor.bWalkable || closedSet.Contains(neighbor)) { continue; }

                    // Check if our 
                    //      current path -> neighbor is shorter than 
                    //      path from start -> neighbor (neighbor.gCost)
                    // IF so, that means the this neighbor has a better path
                    int costFromCurrentToNeighbor = currentNode.gCost + DistanceBetweenNodes(currentNode, neighbor);
                    if(costFromCurrentToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        // Our current path is the best, update the neighbor's gCost and hCost
                        neighbor.gCost = costFromCurrentToNeighbor;
                        neighbor.hCost = DistanceBetweenNodes(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if(!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }
        }
        yield return null; // wait for 1 frame before returning
        if(bPathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, bPathSuccess);
    }

    Vector3[] RetracePath(ASNode startNode, ASNode endNode)
    {
        List<ASNode> path = new List<ASNode>();
        ASNode currentNode = endNode;
        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    // Only create waypoints along the path that actually change in direction
    Vector3[] SimplifyPath(List<ASNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        for(int i = 1; i < path.Count; ++i)
        {
            int dx = path[i-1].row - path[i].row;
            int dy = path[i-1].col - path[i].col;
            Vector2 directionNew = new Vector2(dx, dy);
            if(directionNew != directionOld)// we've changed direction!
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    /*
    caridnal (n, s, e, w)
    diagonal (ne, nw, se, sw)

        Distance between two caridnal nodes is 1
        Distance between two diagonal nodes is sqrt(2) = ~1.4
           
            multiply by 10 for common practice
       
        Distance between two caridnal nodes is 10
        Distance between two diagonal nodes is 14

    Therefor when we move horizontally we move by increments of 10
    When we move vertically we move by increments of 14

    Eq: 
        let x = horizontal distance between n1 & n2: abs(n2.x-n1.x)
        let y = vertical distance between n1 & n2: abs(n2.y-n1.y)

        let a = max(x, y)
        let b = min(x, y)
        
        Distance = 14a + 10(b-a)
    */
    int DistanceBetweenNodes(ASNode n1, ASNode n2)
    {
        int distX = Mathf.Abs(n2.row - n1.row);
        int distY = Mathf.Abs(n2.col - n1.col);

        int a = Mathf.Min(distX, distY);
        int b = Mathf.Max(distX, distY);

        return (14 * a) + (10 * (b - a)); 
    }
}
