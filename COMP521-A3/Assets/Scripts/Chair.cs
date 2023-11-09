using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Path node class to offer quick access for key pathfinding info
public class ChairPathNode
{
    public int pos_x;
    public int pos_y;

    public float gValue;
    public float hValue;
    public ChairPathNode parentNode;

    public float fValue
    {
        get { return gValue + hValue; }
    }

    public ChairPathNode(int xPos, int yPos)
    {
        pos_x = xPos;
        pos_y = yPos;
    }
}
public class Chair : MonoBehaviour
{
    SceneHandler sceneHandler;
    Grid gridMap;
    [SerializeField] Rigidbody rb;
    float chairSpeed = 50f;

    // Target agent info
    public GameObject currentTarget;
    Vector2Int targetPosition;

    List<ChairPathNode> pathing;
    ChairPathNode[,] pathNodes;
    ChairPathNode nextNode;
    Vector3 chairVelocity = Vector3.zero;

    // Timer for movement 
    float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        pathing = new List<ChairPathNode>();

        // Initializing gridMap field
        gridMap = GameObject.FindObjectOfType<Grid>();
        sceneHandler = GameObject.FindObjectOfType<SceneHandler>();

        Init();

        // Finding closest agent for the pathing
        FindTarget();

        // Initializing pathing
        pathing = FindChairPath((int)rb.position.x, (int)rb.position.z, targetPosition.x, targetPosition.y);

        // Looking if there are nodes in the path and initializing first node to go to.
        if (pathing.Count != 0)
        {
            nextNode = pathing[0];
            pathing.RemoveAt(0);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // If the timer goes over 2 seconds, update fields and pathing
        if(timer > 2f)
        {
            List<ChairPathNode> newPathing = new List<ChairPathNode>();
            FindTarget();
            newPathing = FindChairPath((int)rb.position.x, (int)rb.position.z, targetPosition.x, targetPosition.y);

            // If the closest agent is blocked by obstacles, pick a random agent instead
            if (newPathing.Count != 0 ) {
                ZeroVelocity();
                pathing = newPathing; 
            }
            else { 
                FindRandomTarget();
                ZeroVelocity();
                pathing = FindChairPath((int)rb.position.x, (int)rb.position.z, targetPosition.x, targetPosition.y);
            }


            // Looking if there are nodes in the path and initializing first node to go to.
            if (pathing.Count != 0)
            {
                nextNode = pathing[0];
                pathing.RemoveAt(0);
            }

            timer = 0f;
        }

        // Looking if the next target node exists and if agent is currently on the target node
        if (nextNode != null && gridMap.grid[nextNode.pos_x, nextNode.pos_y].gridObject == this.gameObject)
        {
            // Checking if this was the last node in the path
            if (pathing.Count > 0)
            {
                // Next target node in pathing
                nextNode = pathing[0];
                // Removes current target node from pathing list
                pathing.RemoveAt(0);

            }
        }

        // Setting velocity only if there is a target node
        if (nextNode != null) { SetVelocity(); }

        rb.velocity = chairVelocity * Time.fixedDeltaTime;
        timer += Time.fixedDeltaTime;
    }

    // Finding the closest agent to the chair
    private void FindTarget()
    {
        // Iterating through the agent list made at initialization
        foreach (GameObject agent in sceneHandler.agentList)
        {
            if(currentTarget == null)
            {
                currentTarget = agent;
            }
            else
            {
                // Check if the absolute value of the distance of agents are smaller than the
                // current target agent. We find eventually the closest one.
                if(Mathf.Abs(Vector3.Distance(agent.transform.position,this.transform.position))
                    < Mathf.Abs(Vector3.Distance(currentTarget.transform.position, this.transform.position)))
                {
                    currentTarget = agent;
                }
            }

            // Storing the agent's position on gridmap.
            targetPosition = gridMap.GetGridPosition(currentTarget.transform.position);
            
        }
    }

    // Variation to help the scene to get unstuck. Chairs would group around a trapped
    // agent and stop moving.
    private void FindRandomTarget()
    {
        // Getting the number of agents in the list
        int agentNum = sceneHandler.agentList.Count;
        int randomAgent = Random.Range(0, agentNum);

        currentTarget = sceneHandler.agentList[randomAgent];
        targetPosition = gridMap.GetGridPosition(currentTarget.transform.position);
    }

    // Initialization of the pathNodes array
    private void Init()
    {
        pathNodes = new ChairPathNode[gridMap.length, gridMap.width];

        for (int x = 0; x < gridMap.length; x++)
        {
            for (int y = 0; y < gridMap.width; y++)
            {
                pathNodes[x, y] = new ChairPathNode(x, y);
            }
        }
    }

    // Returns the List of nodes for the optimal path the agent should take
    public List<ChairPathNode> FindChairPath(int startX, int startY, int endX, int endY)
    {
        ChairPathNode startNode = pathNodes[startX, startY];
        ChairPathNode endNode = pathNodes[endX, endY];

        // List of opened and closed node during the pathfinding process
        List<ChairPathNode> openList = new List<ChairPathNode>();
        List<ChairPathNode> closedList = new List<ChairPathNode>();

        // Adding current starting node as the first node in the open list
        openList.Add(startNode);

        // Loop until there are no more nodes to check, in that case pathfinding failed
        // and returns null

        while (openList.Count > 0)
        {
            ChairPathNode currentNode = openList[0];

            // Choosing the next most promising opened node as current node
            for (int i = 0; i < openList.Count; i++)
            {
                if (currentNode.fValue > openList[i].fValue)
                {
                    currentNode = openList[i];
                }

                if (currentNode.fValue == openList[i].fValue
                    && currentNode.hValue > openList[i].hValue)
                {
                    currentNode = openList[i];
                }
            }

            // Node has now been examined and goes in the closed list
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // If we reached the goal, returns the path by retracing it
            if (currentNode == endNode)
            {
                return RetracePath(startNode, endNode);
            }

            List<ChairPathNode> neighbourNodes = new List<ChairPathNode>();

            // Looking for all neighbour of currentNode that are in bounds,
            // and put them in a list
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0) { continue; }
                    if (gridMap.CheckBoundary(currentNode.pos_x + x, currentNode.pos_y + y) == false)
                    { continue; }

                    neighbourNodes.Add(pathNodes[currentNode.pos_x + x, currentNode.pos_y + y]);

                }
            }

            // Going through the list of neighbours to update path nodes
            for (int i = 0; i < neighbourNodes.Count; i++)
            {
                // Ignores neighbours that have already been closed
                if (closedList.Contains(neighbourNodes[i])) { continue; }

                // Ignores neighbours that are obstacles except the goal
                if (endNode != neighbourNodes[i]
                    && gridMap.CheckWalkable(neighbourNodes[i].pos_x, neighbourNodes[i].pos_y) == false)
                { continue; }

                // Calculates the cost of going to neighbour node following the A* algorithm
                float movementCost = currentNode.gValue + CalculateDistance(currentNode, neighbourNodes[i]);

                // Updating the neighbour node's info it is not part of the open list or
                // if a better path to the neighbour has been found
                if (openList.Contains(neighbourNodes[i]) == false ||
                    movementCost < neighbourNodes[i].gValue)
                {
                    neighbourNodes[i].gValue = movementCost;
                    neighbourNodes[i].hValue = CalculateDistance(neighbourNodes[i], endNode);
                    neighbourNodes[i].parentNode = currentNode;

                    // Adds neighbour to open list if it wasn't already
                    if (openList.Contains(neighbourNodes[i]) == false)
                    {
                        openList.Add(neighbourNodes[i]);
                    }
                }
            }
        }

        return pathing;
    }

    // Calculating the h-value associated with the currentNode following A* algorithm
    private int CalculateDistance(ChairPathNode currentNode, ChairPathNode target)
    {
        int distX = Mathf.Abs(currentNode.pos_x - target.pos_x);
        int distY = Mathf.Abs(currentNode.pos_y - target.pos_y);

        // Associating 10 to a straight line move and 14 to a diagonal one
        if (distX > distY) { return 14 * distY + 10 * (distX - distY); }
        return 14 * distX + 10 * (distY - distX);
    }

    // Using parent nodes, we retrace back from the endNode the optimal path
    // Returns that optimal path
    private List<ChairPathNode> RetracePath(ChairPathNode startNode, ChairPathNode endNode)
    {
        List<ChairPathNode> path = new List<ChairPathNode>();

        // Starting at the end node
        ChairPathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        // Reversing list to get actual path from start to goal
        path.Reverse();

        return path;
    }

    // Function to set the chair's velocity toward next pathing node
    private void SetVelocity()
    {
        // Looking at next node in path to get direction to go
        Vector3 dir = (new Vector3(nextNode.pos_x, 0f, nextNode.pos_y) -
            new Vector3(rb.position.x, 0f,
            rb.position.z)).normalized;

        // Moving the chair's rigidbody towards that node
        chairVelocity = dir * chairSpeed;

    }

    // Function to immobilize chair and remove planned path
    private void ZeroVelocity()
    {
        chairVelocity = Vector3.zero;
        pathing.Clear();
        nextNode = null;
    }
}
