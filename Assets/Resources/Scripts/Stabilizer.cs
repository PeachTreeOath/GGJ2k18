using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabilizer : MonoBehaviour {

    private Transform anchorPosition;

    void LateUpdate()
    {
        transform.position = anchorPosition.transform.position;
    }

    public void SetAnchor(Transform parentTransform)
    {
        anchorPosition = parentTransform ;
    }
}
