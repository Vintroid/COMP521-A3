using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Path node class to offer quick access for key pathfinding info
public class AgentPathNode
{
    public int pos_x;
    public int pos_y;

    public float gValue;
    public float hValue;
    public AgentPathNode parentNode;

    public float fValue
    {
        get { return gValue + hValue; }
    }

    public AgentPathNode(int xPos, int yPos)
    {
        pos_x = xPos;
        pos_y = yPos;
    }
}

public class Agent : MonoBehaviour
{
    Grid gridMap;
    float agentSpeed = 110f;
    [SerializeField] Rigidbody rb;

    // Array for all checkable nodes for pathing
    AgentPathNode[,] pathNodes;

    // Current path that is being followed
    List<AgentPathNode> pathing;

    // Agent new target node
    AgentPathNode nextNode = null;

    // Agent current velocity
    Vector3 agentVelocity = Vector3.zero;

    // Agent node to node timer for adaptability
    private float nodeToNodeTimer = 0f;

    private void Start()
    {
        pathing = new List<AgentPathNode>();

        // Initializing gridMap field
        gridMap = GameObject.FindObjectOfType<Grid>();
        Init();
        // Initializing pathing
        pathing = FindPath((int)rb.position.x,(int)rb.position.z,gridMap.goalNode.x,gridMap.goalNode.y);

        // Looking if there are nodes in the path and initializing first node to go to.
        if (pathing.Count != 0)
        {
            nextNode = pathing[0];
            pathing.RemoveAt(0);
        }
    }

    private void FixedUpdate()
    {
        // If there is no goal at a current point, the agent stops and wait 1 second before
        // doing pathfinding again towards new goal.
        // The Vector2Int(-1,-1) is an error code setup by the sceneHandler when there
        // are no goals in the scene

        if (gridMap.goalNode == new Vector2Int(-1,-1))
        {
            ZeroVelocity();
            nodeToNodeTimer = 0f;
        }
        else
        {
            // Looking if the next target node exists and if agent is currently on the target node
            if (nextNode != null && gridMap.grid[nextNode.pos_x,nextNode.pos_y].gridObject == this.gameObject)
            {
                // Checking if this was the last node in the path
                if (pathing.Count > 0)
                {
                    // Next target node in pathing
                    nextNode = pathing[0];
                    // Removes current target node from pathing list
                    pathing.RemoveAt(0);
                 
                    nodeToNodeTimer = 0f;

                }
                else
                {
                    // Arrived at the goal
                    ZeroVelocity();
                    nodeToNodeTimer = 0f;
                }

            }
            else
            {
                // If agent has not reached the target node after 1 second
                // a new path is recalculated
                if (nodeToNodeTimer > 1f)
                {
                    ZeroVelocity();

                    // If there is a goal on the map check
                    if (gridMap.goalNode != new Vector2Int(-1, -1))
                    {
                        pathing = FindPath((int)rb.position.x, (int)rb.position.z, gridMap.goalNode.x, gridMap.goalNode.y);
                    }

                    // Looking if there are nodes in the path and initializing first node to go to.
                    if (pathing.Count != 0)
                    {
                        nextNode = pathing[0];
                        pathing.RemoveAt(0);
                    }

                    nodeToNodeTimer = 0f;
                }
                
            }
        }

        nodeToNodeTimer += Time.fixedDeltaTime;

        // Setting velocity only if there is a target node
        if (nextNode != null) { SetVelocity(); }
        rb.velocity = agentVelocity * Time.fixedDeltaTime;
        
    }

    // Initialization of the pathNodes array
    private void Init()
    {
        pathNodes = new AgentPathNode[gridMap.length, gridMap.width];

        for (int x = 0; x < gridMap.length; x++)
        {
            for (int y = 0; y < gridMap.width; y++)
            {
                pathNodes[x, y] = new AgentPathNode(x, y);
            }
        }
    }

    // Returns the List of nodes for the optimal path the agent should take
    public List<AgentPathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        AgentPathNode startNode = pathNodes[startX, startY];
        AgentPathNode endNode = pathNodes[endX, endY];

        // List of opened and closed node during the pathfinding process
        List<AgentPathNode> openList = new List<AgentPathNode>();
        List<AgentPathNode> closedList = new List<AgentPathNode>();

        // Adding current starting node as the first node in the open list
        openList.Add(startNode);

        // Loop until there are no more nodes to check, in that case pathfinding failed
        // and returns null

        while (openList.Count > 0)
        {
            AgentPathNode currentNode = openList[0];

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

            List<AgentPathNode> neighbourNodes = new List<AgentPathNode>();

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

                // Ignores neighbours that are obstacles
                if (gridMap.CheckWalkable(neighbourNodes[i].pos_x, neighbourNodes[i].pos_y) == false)
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
    private int CalculateDistance(AgentPathNode currentNode, AgentPathNode target)
    {
        int distX = Mathf.Abs(currentNode.pos_x - target.pos_x);
        int distY = Mathf.Abs(currentNode.pos_y - target.pos_y);

        // Associating 10 to a straight line move and 14 to a diagonal one
        if (distX > distY) { return 14 * distY + 10 * (distX - distY); }
        return 14 * distX + 10 * (distY - distX);
    }

    // Using parent nodes, we retrace back from the endNode the optimal path
    // Returns that optimal path
    private List<AgentPathNode> RetracePath(AgentPathNode startNode, AgentPathNode endNode)
    {
        List<AgentPathNode> path = new List<AgentPathNode>();

        // Starting at the end node
        AgentPathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        // Reversing list to get actual path from start to goal
        path.Reverse();

        return path;
    }

    // Function to set the agent's velocity toward next pathing node
    private void SetVelocity()
    {
        // Looking at next node in path to get direction to go
        Vector3 dir = (new Vector3(nextNode.pos_x, 0f, nextNode.pos_y) -
            new Vector3(rb.position.x, 0f,
            rb.position.z)).normalized;
        
        // Moving the agent's rigidbody towards that node
        agentVelocity = dir * agentSpeed;
        
    }

    // Function to immobilize agent and remove planned path
    private void ZeroVelocity()
    {
        agentVelocity = Vector3.zero;
        pathing.Clear();
        nextNode = null;
    }
}
