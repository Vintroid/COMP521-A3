using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Grid : MonoBehaviour
{
    // 2D Array containing all the grid nodes
    public GridNode[,] grid;

    // Grid dimensions
    [SerializeField] public int width;
    [SerializeField] public int length;
    [SerializeField] float cellSize;

    // Layer masks for game object recognition
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask goalLayer;

    // Goal coordinates for pathfinding
    public Vector2Int goalNode;

    private void Awake()
    {
        // We want the grid initialize first
        GenerateGrid();
    }

    private void Start()
    {
        CheckPassableTerrain();
    }

    private void Update()
    {
        CheckPassableTerrain(); // Updating our grid information because of object movement
        SceneView.RepaintAll(); // Repaints Gizmos because we changed passable nodes
    }

    // Function on startup to initialize the 2d node array
    private void GenerateGrid()
    {
        grid = new GridNode[length, width];

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                grid[x, y] = new GridNode();
            }
        }
    
    }

    // Function to perform a check on array to update node passable field and objects
    private void CheckPassableTerrain()
    {
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                Vector3 worldPosition = GetWorldPosition(x, y);
                RaycastHit hit;

                // Making a box check with the obstacle layer to analyse obstacle placement
                bool hit_detect = Physics.Raycast(worldPosition, Vector3.up,out hit,10f,obstacleLayer);
                bool passable = !Physics.CheckBox(worldPosition,Vector3.one/2,Quaternion.identity,obstacleLayer);
                grid[x, y].passable = passable;
                
                if (hit_detect)
                {
                    grid[x,y].gridObject = hit.collider.gameObject;
                }
            }
        }
    }

    // Function to visualize map on scene view
    private void OnDrawGizmos()
    {
        if (grid == null)
        {
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Vector3 pos = GetWorldPosition(x, y);
                    Gizmos.DrawCube(pos, Vector3.one / 4);

                }
            }
        }
        else
        {
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    // Drawing gizmos cube given a position
                    Vector3 pos = GetWorldPosition(x, y);
                    Gizmos.color = grid[x, y].passable ? Color.white : Color.red;
                    Gizmos.DrawCube(pos, Vector3.one / 4);

                }
            }
        }
    }

    // Function to get world position for the nodes to use
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, 0f , y * cellSize);
    }

    // Function to get grid position for grid objects to use
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector2Int positionOnGrid = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));
        return positionOnGrid;
    }

    // Function to check if an object is out of bounds 
    public bool CheckBoundary(Vector2Int positionOnGrid)
    {
        if (positionOnGrid.x < 0 || positionOnGrid.x >= length) { return false; }
        if (positionOnGrid.y < 0 || positionOnGrid.y >= width) { return false; }
        return true;
    }

    // Overload with different arguments
    internal bool CheckBoundary(int posX, int posY)
    {
        if (posX < 0 || posX >= length) { return false; }
        if (posY < 0 || posY >= width) { return false; }
        return true;
    }

    // Helper function to check if there is an obstacle at input position
    public bool CheckWalkable(int pos_x, int pos_y)
    {
        return grid[pos_x, pos_y].passable;
    }

    // Function to return what object is at input position
    public GameObject GetPlacedObject(Vector2Int gridPosition)
    {
        if (CheckBoundary(gridPosition) == true)
        {
            GameObject gridObject = grid[gridPosition.x, gridPosition.y].gridObject;
            return gridObject;
        }
        else
        {
            return null;
        }
    }
}
