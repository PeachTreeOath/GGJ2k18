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
    public float airPower = 20f;

    public TextMesh leftTextMesh;
    public TextMesh rightTextMesh;
    public float labelOffset = 0.25f;
    public Color standardTextColor = Color.white;
    public Color pressedTextColor = new Color(1f, 0.6f, 0f);

    private float _radius;
    private LineRenderer _lineRenderer;
    private List<GameObject> _joints;
    private CircleCollider2D leftCollider;
    private CircleCollider2D rightCollider;
    private Vector3 _offsetVector;
    private ContactFilter2D _filter;
    private LayerMask _mask;

    private DistanceJoint2D _leftGrabJoint;
    private DistanceJoint2D _rightGrabJoint;

    // Use this for initialization
    private void Start()
    {
        _filter = new ContactFilter2D();
        _mask = new LayerMask();
        _mask.value = LayerMask.NameToLayer("Default");
        _filter.layerMask = _mask;

        _radius = width / 2f;
        _lineRenderer = GetComponent<LineRenderer>();
        _offsetVector = new Vector3(0f, labelOffset, 0f);
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

            CapsuleCollider2D capsuleCollider = joint.AddComponent<CapsuleCollider2D>();
            capsuleCollider.size = new Vector2(0.21f, 0.6f);

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
                body.mass = 0.5f;

            }

            _joints.Add(joint);
            prevBody = body;

            // Add colliders on the ends for latching onto objects
            if (i == 0)
            {
                leftCollider = joint.AddComponent<CircleCollider2D>();
                leftCollider.radius = _radius * 1.5f;
                leftCollider.isTrigger = true;
                body.mass = 10f;
                capsuleCollider.offset = new Vector2(0, 0.25f);

            }
            if (i == jointCount - 1)
            {
                rightCollider = joint.AddComponent<CircleCollider2D>();
                rightCollider.radius = _radius * 1.5f;
                rightCollider.isTrigger = true;
                body.mass = 10f;
                capsuleCollider.offset = new Vector2(0, -0.25f);
            }
        }

        // use world points and set up for three points per joint minus the two dangling off the ends
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = jointCount;
        UpdateLineRendererPositions();

        // set the middle point as the camera follower
        CameraPlayerController camera = FindObjectOfType<CameraPlayerController>();
        camera.mainPlayer = _joints[jointCount / 2].gameObject;
    }

    private void Update()
    {
        // can only apply forces if we are touching a physics body
        float framePower = IsPaperGrounded() ? power : airPower;
        float h1 = Input.GetAxis("J_LeftStickX");
        float v1 = Input.GetAxis("J_LeftStickY");
        _joints.First().GetComponent<Rigidbody2D>().AddForce(new Vector2(h1 * framePower, v1 * framePower));

        float h2 = Input.GetAxis("J_RightStickX");
        float v2 = Input.GetAxis("J_RightStickY");
        _joints.Last().GetComponent<Rigidbody2D>().AddForce(new Vector2(h2 * framePower, v2 * framePower));

        if (Input.GetButtonDown("KeyGrabLeft"))
            _leftGrabJoint = LockJoint(_joints.First().GetComponent<Rigidbody2D>(), leftCollider, leftTextMesh);
        else if (Input.GetButtonUp("KeyGrabLeft"))
            UnlockJoint(_joints.First().GetComponent<Rigidbody2D>(), leftTextMesh, _leftGrabJoint);

        if (Input.GetButtonDown("KeyGrabRight"))
            _rightGrabJoint = LockJoint(_joints.Last().GetComponent<Rigidbody2D>(), rightCollider, rightTextMesh);
        else if (Input.GetButtonUp("KeyGrabRight"))
            UnlockJoint(_joints.Last().GetComponent<Rigidbody2D>(), rightTextMesh, _rightGrabJoint);

        UpdateLineRendererPositions();

        // render the correct location for each label
        leftTextMesh.transform.position = _joints.First().transform.position + _offsetVector;
        rightTextMesh.transform.position = _joints.Last().transform.position + _offsetVector;
        leftTextMesh.transform.rotation = Quaternion.identity;
        rightTextMesh.transform.rotation = Quaternion.identity;
    }

    private void UpdateLineRendererPositions()
    {
        //temporary for setting up the final check in
        _lineRenderer.SetPositions(_joints.Select(j => j.transform.position).ToArray());

        // // gets the vector locations of each joint
        // List<Vector3> vecs = _joints.Select(j => j.transform.position)

        // // Assumes 3 times as many points as joints, minus the two end points
        // // Uses cubic interpolation
        // for(int i = 0; i < jointCount; i++)
        // {
        //     _joints[i].transform.position
        // }

    }

    /// <summary>
    /// Check to see if any of the joints in the papermate are colliding with physical bodies
    /// </summary>
    /// <returns></returns>
    private bool IsPaperGrounded()
    {
        foreach (GameObject jt in _joints)
        {
            CapsuleCollider2D[] cols2D = jt.GetComponents<CapsuleCollider2D>();
            foreach (CapsuleCollider2D col2D in cols2D)
            {
                Collider2D[] results = new Collider2D[10];
                col2D.OverlapCollider(_filter, results);
                results = results.Where(c => c != null && c.gameObject.layer == _mask.value).ToArray();
                if (results.Length > 0 && results[0] != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private DistanceJoint2D LockJoint(Rigidbody2D rigidBody, CircleCollider2D col2D, TextMesh textMesh)
    {
        Collider2D[] results = new Collider2D[10];
        col2D.OverlapCollider(_filter, results);
        results = results.Where(c => c != null && c.gameObject.layer == _mask.value).ToArray();

        if (results.Length > 0 && results[0] != null)
        {
            textMesh.color = pressedTextColor;
            DistanceJoint2D distJt = rigidBody.gameObject.AddComponent<DistanceJoint2D>();
            distJt.connectedBody = results[0].attachedRigidbody;
            distJt.connectedAnchor = results[0].transform.InverseTransformPoint(rigidBody.transform.position);
            return distJt;
        }
        return null;
    }

    private void UnlockJoint(Rigidbody2D rigidBody, TextMesh textMesh, DistanceJoint2D grabJoint)
    {
        rigidBody.constraints = RigidbodyConstraints2D.None;
        textMesh.color = standardTextColor;
        GameObject.Destroy(grabJoint);
    }
}
