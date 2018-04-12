using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float knockbackStrength;
    public float gravityValue;
    public Vector2 gravityDirection;

    //For configuring movement
    public float maxVelocity;
    public float acceleration;
    public bool isPatroling;

    //For moving along a wall, positive is perpendicular moving right from the gravity vector, while negative is left.
    private bool isMovingPositive = true;

    private void Update()
    {
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.AddForce(gravityDirection * gravityValue);

        //Now figure out movement 
        if (isPatroling)
        {
            float movingPositiveSign = isMovingPositive ? 1 : -1;

            print("QuaternionValue: " + Quaternion.Euler(0, movingPositiveSign * 90, 0));
            Vector2 movementDirection = Quaternion.Euler(0, movingPositiveSign * 90, 0) * gravityDirection;
            print("MovementDirection: " + movementDirection);
            if (Mathf.Abs(body.velocity.magnitude) < maxVelocity)
            {
                body.AddForce(acceleration * movementDirection);
                print("MovementForce: " + acceleration * movementDirection);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<WinTrigger>() != null)
        {
            PapermateBody body = other.GetComponentInParent(typeof(PapermateBody)) as PapermateBody;

            if (body != null)
            {
                body.ApplyKnockback(transform.position, knockbackStrength);
            }
        }
    }
}
