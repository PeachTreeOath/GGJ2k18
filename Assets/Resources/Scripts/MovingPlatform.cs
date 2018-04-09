using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public float xMagnitude;
    public float yMagnitude;
    public float speed = 1;
    public float delay;

    private Vector3 origPos;
    private float elapsedTime;

    void Start()
    {
        origPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        transform.position = origPos + new Vector3(xMagnitude * Mathf.Sin(elapsedTime * speed + delay), yMagnitude * Mathf.Sin(elapsedTime * speed + delay), 0);
    }
}
