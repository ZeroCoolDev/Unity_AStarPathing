using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASGrid : MonoBehaviour
{
    public bool bDisplayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize; // corresponds: x ==> x, y ==> z
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    public int obstacleProximityPenalty = 10; // how close we want to allow players to get to unwalkable objects
    LayerMask walkableMask; // contains all the layers in the walkableRegions
    Dictionary<int,int> walkableRegionsMap = new Dictionary<int, int>(); // keyed off index in the layermask array. Value is terrian penalty for that layer

    ASNode[,] grid;
    float nodeDiameter;
    int gridRows, gridCols;

    // Used for visualization of the blurred grid
    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    public int MaxSize {
        get {
            return gridRows * gridCols;
        }
    }

    void Awake()
    {
        foreach(TerrainType region in walkableRegions)
        {
            walkableMask.value |= region.terrainMask.value;

            int layermaskArrayIndex = (int)Mathf.Log(region.terrainMask.value, 2);
            walkableRegionsMap.Add(layermaskArrayIndex, region.terrainPenalty);
        }

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

                int movementPenalty = 0;
                // raycast to find the layer
                Ray ray = new Ray(worldPt + Vector3.up * 50, Vector3.down); // fire a ray straight down from a 'little bit' above the ground
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, 100 /*incase terrain dips*/, walkableMask))
                {
                    walkableRegionsMap.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);   
                }

                if(!walkable)
                {
                    movementPenalty += obstacleProximityPenalty;
                }

                grid[row,col] = new ASNode(walkable, worldPt, row, col, movementPenalty);
            }
        }

        BlurPenaltyMap(3);
    }

    /*
        Uses Box Blur to smooth out values
    */
    void BlurPenaltyMap(int blurSize)
    {
        int kernalSize = blurSize * 2 + 1; // must be odd number
        int kernalExtends = (kernalSize - 1) / 2;//how many squares are there between the central square and the edge of the kernal
    
        // contains kernal vals for each direction.
        int[,] penaltiesHorizontalPass  = new int[gridRows,gridCols];
        int[,] penaltiesVerticalPass  = new int[gridRows,gridCols];

        // traverse each row's columns [col1] -> [col2]
        for(int row = 0; row < gridRows; ++row)
        {
            // the first node in each row needs to fill in the space outside the grid
            // Achieve this by just duplicating the first node
            for(int col = -kernalExtends; col <= kernalExtends; ++col)
            {
                int sampleCol = Mathf.Clamp(col, 0, kernalExtends);
                penaltiesHorizontalPass[row, 0] += grid[row, sampleCol].movementPenalty; // update the first node which is in the row given, and column 0
            }

            for(int col = 1; col < gridCols; ++col)
            {
                int removeCol = Mathf.Clamp(col - kernalExtends - 1, 0, gridCols-1);
                int addCol = Mathf.Clamp(col + kernalExtends, 0, gridCols-1);
                
                // Remove the previous left most, and add the new right
                penaltiesHorizontalPass[row,col] = penaltiesHorizontalPass[row, col-1] - grid[row, removeCol].movementPenalty + grid[row, addCol].movementPenalty;
            }
        }

        // traverse each column's rows [row1]
        //                              v
        //                             [row2]
        for(int col = 0; col < gridCols; ++col)
        {
            // the first node in each row needs to fill in the space outside the grid
            // Achieve this by just duplicating the first node
            for(int row = -kernalExtends; row <= kernalExtends; ++row)
            {
                int sampleRow = Mathf.Clamp(row, 0, kernalExtends);
                penaltiesVerticalPass[0,col] += penaltiesHorizontalPass[sampleRow, col]; // update the first node which is the given column, and row 0
            }
            // Assigns the move penalty for the first row (since below forloop starts at 1)
            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[0,col] / (kernalSize * kernalSize)); // round to nearest int instead of always rounding down
            grid[0,col].movementPenalty = blurredPenalty;

            for(int row = 1; row < gridRows; ++row)
            {
                int removeRow = Mathf.Clamp(row - kernalExtends - 1, 0, gridRows-1);
                int addRow = Mathf.Clamp(row + kernalExtends, 0, gridRows-1);
                
                // Remove the previous upper most, and add the new bottom
                penaltiesVerticalPass[row,col] = penaltiesVerticalPass[row-1, col] - penaltiesHorizontalPass[removeRow, col] + penaltiesHorizontalPass[addRow, col];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[row, col] / (kernalSize * kernalSize)); // round to nearest int instead of always rounding down
                grid[row, col].movementPenalty = blurredPenalty;

                if(blurredPenalty > penaltyMax)
                {
                    penaltyMax = blurredPenalty;
                }
                if(blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;
                }
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
                // create a color between white and black based off the nodes move penalty.
                // 0 = white
                // 1 = black
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
                Gizmos.color = n.bWalkable ? Gizmos.color : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeDiameter);
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
