using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindZoneCustom : MonoBehaviour {

    /// <summary>
    /// The direction of the wind
    /// </summary>
    public Vector2 direction = new Vector2();

    /// <summary>
    /// The Speed of the wind;
    /// </summary>
    public int windSpeed = 50;

    

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Paper has entered the DAAAAAANGER Zone");
    }

    void OnTriggerStay2D(Collider2D other)
    {
        //Debug.Log("DANGER ZONE");
        Rigidbody2D rigidBody = other.GetComponent<Rigidbody2D>();
        rigidBody.AddForce(direction * windSpeed);
    }

    void OnTriggerExit2D(Collider2D other)
    {
     //   Debug.Log("Paper has left the DAAAAANNNNGER Zone");
    }


}
