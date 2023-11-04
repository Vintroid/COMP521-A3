using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    // Unity objects needed for scene initialization
    [SerializeField] Grid gridMap;
    [SerializeField] GameObject goal;
    [SerializeField] GameObject agent;
    [SerializeField] GameObject chair;

    // Parameters for scene initialization
    public int agentNumber = 0;
    public int chairNumber = 0;
    public float agentSpeed = 1f;
    public float chairSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // Generate agents, chairs, and goal at random
        // There shouldn't be more than 551 objects in grid
        GenerateAgents();
        GenerateChairs();
        GenerateGoal();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Placing agents randomly on the grid
    private void GenerateAgents()
    {
        int placedAgents = 0;
        int maxWidth = gridMap.width;
        int maxLength = gridMap.length;

        while(placedAgents < agentNumber)
        {
            // Getting a random node
            int randomWidth = Random.Range(0,maxWidth);
            int randomLength = Random.Range(0,maxLength);

            // Checking if node already contains an object
            if (gridMap.grid[randomLength,randomWidth].gridObject  == null)
            {
                // If empty instantiates a new agent in the map
                Vector3 worldPosition = gridMap.GetWorldPosition(randomLength,randomWidth);
                Vector3 agentOffset = new Vector3(0f, 1f, 0f);
                GameObject newAgent = GameObject.Instantiate(agent, worldPosition + agentOffset , Quaternion.identity);
                gridMap.grid[randomLength,randomWidth].gridObject = newAgent;
                placedAgents++;
            }

        }
    }

    // Placing chairs randomly on the grid
    private void GenerateChairs()
    {
        int placedChairs = 0;
        int maxWidth = gridMap.width;
        int maxLength = gridMap.length;

        while (placedChairs < chairNumber)
        {
            // Getting a random node
            int randomWidth = Random.Range(0, maxWidth);
            int randomLength = Random.Range(0, maxLength);

            // Checking if node already contains an object
            if (gridMap.grid[randomLength, randomWidth].gridObject == null)
            {
                // If empty instantiates a new agent in the map
                Vector3 worldPosition = gridMap.GetWorldPosition(randomLength, randomWidth);
                Vector3 chairOffset = new Vector3(0f, 1f, 0f);
                GameObject newChair = GameObject.Instantiate(chair, worldPosition + chairOffset, Quaternion.Euler(0f,180f,0f));
                gridMap.grid[randomLength, randomWidth].gridObject = newChair;
                placedChairs++;
            }

        }
    }

    private void GenerateGoal()
    {
        bool goalPlaced = false;
        int maxWidth = gridMap.width;
        int maxLength = gridMap.length;

        while (!goalPlaced)
        {
            // Getting a random node
            int randomWidth = Random.Range(0, maxWidth);
            int randomLength = Random.Range(0, maxLength);

            // Checking if node already contains an object
            if (gridMap.grid[randomLength, randomWidth].gridObject == null)
            {
                // If empty instantiates a new agent in the map
                Vector3 worldPosition = gridMap.GetWorldPosition(randomLength, randomWidth);
                Vector3 goalOffset = new Vector3(0f, 0.05f, 0f);
                GameObject newGoal = GameObject.Instantiate(goal, worldPosition + goalOffset, Quaternion.identity);
                gridMap.grid[randomLength, randomWidth].gridObject = newGoal;
                goalPlaced = true;
            }

        }
    }
}
