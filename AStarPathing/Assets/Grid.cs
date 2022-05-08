using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize; // corresponds: x ==> x, y ==> z
    public float nodeRadius;

    Node[,] grid;
    float nodeDiameter;
    int gridRows, gridCols;

    void Start()
    {
        // How many nodes can we fit into out grid
        nodeDiameter = nodeRadius * 2;
        gridRows = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridCols = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridRows,gridCols];

        // Go from middle of grid left and down half the size of the grid
        Vector3 worldBottomLeft = transform.position - (Vector3.right * (gridWorldSize.x / 2)) - (Vector3.forward * (gridWorldSize.y / 2));

        for(int row = 0; row < gridRows; ++row)
        {
            for(int col = 0; col < gridCols; ++col)
            {
                // Move right and up 
                Vector3 worldPt =   worldBottomLeft + 
                                    Vector3.right * (row * nodeDiameter + nodeRadius) + 
                                    Vector3.forward * (col * nodeDiameter + nodeRadius);

                bool walkable = !Physics.CheckSphere(worldPt, nodeRadius, unwalkableMask);
                grid[row,col] = new Node(walkable, worldPt);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        // Convert from point to percentage of how far along the grid it is (Ex: leftmost = 0, halfway = 0.5, rightmost = 1)
        float percentRow = (worldPos.x + gridWorldSize.x/2) / gridWorldSize.x;
        float percentCol = (worldPos.z + gridWorldSize.y/2) / gridWorldSize.y;
        
        // Clamp to [0, 1]
        percentRow = Mathf.Clamp01(percentRow);
        percentCol = Mathf.Clamp01(percentCol);
        
        int row = Mathf.RoundToInt((gridRows-1) * percentRow);
        int col = Mathf.RoundToInt((gridCols-1) * percentCol);
        return grid[row,col];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1f, gridWorldSize.y)); // Z is vertical coordinate

        if(grid != null)
        {
            foreach(Node n in grid)
            {
                Gizmos.color = n.walkable ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }

}
