using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PapermateBody behavior is responsible for building out the papermate body.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PapermateBody : MonoBehaviour {

    public int jointCount = 10;
    public int unitLength = 4;
    public float width = 0.2f;
    public float power = 100f;

    private float _radius;
    private LineRenderer _lineRenderer;
    private List<GameObject> _joints;

	// Use this for initialization
	private void Start ()
    {
        _radius = width / 2f;
        _lineRenderer = GetComponent<LineRenderer>();
        InitializeBody();
	}

    // used to build out the physics body and joints of papermate
    private void InitializeBody()
    {
        float segLen = unitLength / (float)jointCount;
        _joints = new List<GameObject>();
        Rigidbody2D prevBody = null;
        for (int i = 0; i < jointCount; i++)
        {
            GameObject joint = new GameObject("joint_" + i);
            joint.transform.localPosition = new Vector3(0f + (segLen * i), 0f, 0f);
            CircleCollider2D circleCollider = joint.AddComponent<CircleCollider2D>();
            circleCollider.radius = _radius;

            Rigidbody2D body = joint.AddComponent<Rigidbody2D>();
            if (prevBody != null)
            {
                DistanceJoint2D distJt = joint.AddComponent<DistanceJoint2D>();
                distJt.connectedBody = prevBody;
            }

            _joints.Add(joint);
            prevBody = body;
        }

        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = jointCount;
        _lineRenderer.SetPositions(_joints.Select(j => j.transform.position).ToArray());
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        _joints.Last().GetComponent<Rigidbody2D>().AddForce(new Vector2(h * power, v * power));

        for (int i = 0; i < jointCount; i++)
        {
            _lineRenderer.SetPosition(i, _joints[i].transform.position);
        }
    }
}
