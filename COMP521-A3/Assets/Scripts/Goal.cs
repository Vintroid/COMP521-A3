using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] SceneHandler sceneHandler;
    private float timer;

    private void Start()
    {
        sceneHandler = GameObject.FindObjectOfType<SceneHandler>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // If the goal has not been reached yet in 10 seconds
        // it will disappear automatically and reappear randomly.
        if(timer > 10)
        {
            Debug.Log("Goal not reached");
            sceneHandler.WaitSignal();
            Destroy(this.gameObject);
        }
    }

    // Signals the grid if an agent has reached the goal, and then
    // the goal disappears
    private void OnCollisionEnter(Collision collisionInfo)
    {
        if(collisionInfo.collider.gameObject.tag == "Agent")
        {
            Debug.Log("Goal reached in " + timer + " seconds");
            sceneHandler.WaitSignal();
            Destroy(this.gameObject);
        }
    }
}
