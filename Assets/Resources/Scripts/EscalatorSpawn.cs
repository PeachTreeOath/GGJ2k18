using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscalatorSpawn : MonoBehaviour
{
    public int numStairs = 8; // how many prefabs to generate
    public float stairSpeed = 1; // how quickly the prefabs should move

    public GameObject stepPrefab; // game object to create/mutate
    public GameObject first; // prefab source
    public GameObject last; // prefab sink

    private float _distance; // distance between start/finish
    private float _spawnDistance; // distance to travel before spawning prefab

    private Vector3 _firstLoc; // position of source
    private Vector3 _direction; // normalized direction to sink
	private List<GameObject> steps = new List<GameObject>(); // used as a queue

    // Use this for initialization
    void Start()
    {
        // store position of the generator
        _firstLoc = first.transform.position;
        Vector3 difference = last.transform.position - _firstLoc;

        // get direction and distance to object sink
        _direction = difference.normalized;
        _distance = difference.magnitude;
        _spawnDistance = _distance / numStairs;
        Vector3 spawnDirection = _spawnDistance * _direction;

        // generate steps in reverse to generate furthest out stair first
        for (int i = numStairs - 1; i >= 0; i--)
        {     
            GenerateStep(_firstLoc + (i * spawnDirection));
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Move the steps
        Vector2 motion = _direction * stairSpeed * Time.deltaTime;
        foreach (GameObject step in steps)
        {
            step.transform.Translate(motion);
        }

        //Destroy old steps
        GameObject oldestStep = steps[0];
        if ((oldestStep.transform.position - _firstLoc).magnitude > _distance)
        {
            steps.Remove(oldestStep);
            Destroy(oldestStep);
        }

        // Create new steps if the closest step is outside the spawn distance
        if ((steps[steps.Count - 1].transform.position - _firstLoc).magnitude > _spawnDistance)
        {
            GenerateStep(_firstLoc);
        }
    }

    /// <summary>
    /// Generates a new step and adds it to the queue using the given location 
    /// as the starting location
    /// </summary>
    /// <param name="position">The position to locate the step at</param>
    private void GenerateStep(Vector3 position)
    {
        steps.Add(Instantiate(stepPrefab, position, Quaternion.identity));
    }
}
