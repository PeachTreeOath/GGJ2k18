using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PapermateBody behavior is responsible for building out the papermate body.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PapermateBody : MonoBehaviour
{

    public int jointCount = 10;
    public int unitLength = 4;
    public float width = 0.2f;
    public float power = 100f;

    private float _radius;
    private LineRenderer _lineRenderer;
    private List<GameObject> _joints;
    private CircleCollider2D leftCollider;
    private CircleCollider2D rightCollider;

    // Use this for initialization
    private void Start()
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
            joint.transform.localPosition = new Vector3(transform.position.x, transform.position.y + (segLen * i), transform.position.z);
            joint.layer = LayerMask.NameToLayer("Nonattachable");
            joint.transform.SetParent(transform);

            CircleCollider2D circleCollider = joint.AddComponent<CircleCollider2D>();
            circleCollider.radius = _radius;

            Rigidbody2D body = joint.AddComponent<Rigidbody2D>();
            if (prevBody != null)
            {
                DistanceJoint2D distJt = joint.AddComponent<DistanceJoint2D>();
                distJt.connectedBody = prevBody;
                HingeJoint2D hingeJoint = joint.AddComponent<HingeJoint2D>();
                hingeJoint.useLimits = true;
                hingeJoint.connectedBody = prevBody;
                JointAngleLimits2D limits = new JointAngleLimits2D();
                limits.max = 30f;
                limits.min = 0;
                hingeJoint.limits = limits;
            }

            _joints.Add(joint);
            prevBody = body;

            // Add colliders on the ends for latching onto objects
            if (i == 0)
            {
                leftCollider = joint.AddComponent<CircleCollider2D>();
                leftCollider.radius = _radius * 1.5f;
                leftCollider.isTrigger = true;
            }
            if (i == jointCount - 1)
            {
                rightCollider = joint.AddComponent<CircleCollider2D>();
                rightCollider.radius = _radius * 1.5f;
                rightCollider.isTrigger = true;
            }
        }

        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = jointCount;
        _lineRenderer.SetPositions(_joints.Select(j => j.transform.position).ToArray());
    }

    private void Update()
    {
        float h1 = Input.GetAxis("J_LeftStickX");
        float v1 = Input.GetAxis("J_LeftStickY");
        _joints.First().GetComponent<Rigidbody2D>().AddForce(new Vector2(h1 * power, v1 * power));

        float h2 = Input.GetAxis("J_RightStickX");
        float v2 = Input.GetAxis("J_RightStickY");
        _joints.Last().GetComponent<Rigidbody2D>().AddForce(new Vector2(h2 * power, v2 * power));

        if (Input.GetButtonDown("J_LeftStickPress"))
            LockLeftJoint();
        else if (Input.GetButtonUp("J_LeftStickPress"))
            UnlockLeftJoint();

        if (Input.GetButtonDown("J_RightStickPress"))
            LockRightJoint();
        else if (Input.GetButtonUp("J_RightStickPress"))
            UnlockRightJoint();

        for (int i = 0; i < jointCount; i++)
        {
            _lineRenderer.SetPosition(i, _joints[i].transform.position);
        }
    }

    private void LockLeftJoint()
    {
        ContactFilter2D filter = new ContactFilter2D();
        LayerMask mask = new LayerMask();
        mask.value = LayerMask.NameToLayer("Default");
        filter.layerMask = mask;
        Collider2D[] results = new Collider2D[10];
        leftCollider.OverlapCollider(filter, results);

        if (results[0] != null)
        {
            _joints.First().GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
        }
    }

    private void LockRightJoint()
    {
        ContactFilter2D filter = new ContactFilter2D();
        LayerMask mask = new LayerMask();
        mask.value = LayerMask.NameToLayer("Default");
        filter.layerMask = mask;
        Collider2D[] results = new Collider2D[10];
        rightCollider.OverlapCollider(filter, results);

        if (results[0] != null)
        {
            _joints.Last().GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
        }
    }

    private void UnlockLeftJoint()
    {
        _joints.First().GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
    }

    private void UnlockRightJoint()
    {
        _joints.Last().GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
    }
}
