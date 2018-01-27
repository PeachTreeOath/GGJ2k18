using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyBehavior : MonoBehaviour {


    public Collider2D leftCollider;
    public Collider2D rightCollider;

    public void ActivateLeftSticky()
    {
        ContactFilter2D filter = new ContactFilter2D();
        Collider2D[] results = new Collider2D[10];
        leftCollider.OverlapCollider(filter, results);
    }

    public void ActivateRightSticky()
    {
        
    }
}
