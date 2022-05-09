using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASPathfinder : MonoBehaviour
{
    public Transform seeker, target;

    ASGrid grid;

    void Awake()
    {
        // this and Grid.cs must be on the same game object
        grid = GetComponent<ASGrid>();
    }

    void Update()
    {
        FindPath(seeker.position, target.position);
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        ASNode startNode = grid.NodeFromWorldPoint(startPos);
        ASNode targetNode = grid.NodeFromWorldPoint(targetPos);
    
        List<ASNode> openSet = new List<ASNode>();
        HashSet<ASNode> closedSet = new HashSet<ASNode>();

        openSet.Add(startNode);

        int failsafe = Mathf.RoundToInt(grid.gridWorldSize.x * grid.gridWorldSize.y * 2);
        while(openSet.Count > 0)
        {
            --failsafe;
            if(failsafe <= 0 )
            {
                Debug.Log("ERROR: Failsafe hit, infinite loop detected");
                return;
            }

            // Find the node with the lowest FCost that we haven't gone to yet
            ASNode currentNode = openSet[0];
            for(int i = 1; i < openSet.Count; ++i)
            {
                if(openSet[i].fCost < currentNode.fCost ||                                      // node with the lowest fCost
                openSet[i].fCost == currentNode.fCost && openSet[i].hCost == currentNode.hCost)   // or closest to target
                {
                    currentNode = openSet[i];
                }
            }

            // Once we find it that becomes the new current we start pathing from
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if(currentNode == targetNode)// path has been found 
            { 
                RetracePath(startNode, targetNode);
                return; 
            }

            // Check all neighbors to see if any are not walkable or already closed
            foreach(ASNode neighbor in grid.GetNeighbors(currentNode))
            {
                //Debug.Log("Testing neighbors for grid[" + currentNode.row + "]["+ currentNode.col +"]");
                // not walkaable or already closed, skip
                if(!neighbor.walkable || closedSet.Contains(neighbor)) { continue; }

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
                }
            }
        }
    }

    void RetracePath(ASNode startNode, ASNode endNode)
    {
        List<ASNode> path = new List<ASNode>();
        ASNode currentNode = endNode;
        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
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
