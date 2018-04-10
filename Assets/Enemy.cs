using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float knockbackStrength;

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
