using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASGrid : MonoBehaviour
{
    public bool bDisplayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize; // corresponds: x ==> x, y ==> z
    public float nodeRadius;

    ASNode[,] grid;
    float nodeDiameter;
    int gridRows, gridCols;

    public int MaxSize {
        get {
            return gridRows * gridCols;
        }
    }

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        // How many nodes can we fit into out grid
        nodeDiameter = nodeRadius * 2;
        gridRows = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridCols = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        grid = new ASNode[gridRows,gridCols];

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
                grid[row,col] = new ASNode(walkable, worldPt, row, col);
            }
        }
    }

    public List<ASNode> GetNeighbors(ASNode resident)
    {
        List<ASNode> neighbors = new List<ASNode>();
        for(int row = -1; row <= 1; ++row)
        {
            for(int col = -1; col <= 1; ++col)
            {
                if(row == 0 && col == 0) { continue; } // don't count the resident as a neighbor to itself

                int checkRow = resident.row + row;
                int checkCol = resident.col + col;

                // check that this node is in the grid
                if( checkRow >= 0 && checkRow < gridRows &&
                    checkCol >= 0 && checkCol < gridCols)
                {
                    neighbors.Add(grid[checkRow, checkCol]);
                }
            }
        }

        return neighbors;
    }

    public ASNode NodeFromWorldPoint(Vector3 worldPos)
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

        if(grid == null)
        {
            CreateGrid();
        }
        if(grid != null && bDisplayGridGizmos)
        {
            foreach(ASNode n in grid)
            {
                Gizmos.color = n.bWalkable ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
