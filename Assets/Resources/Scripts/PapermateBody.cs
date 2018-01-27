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

    // Use this for initialization
    private void Start()
    {
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

            }
            if (i == jointCount - 1)
            {
                rightCollider = joint.AddComponent<CircleCollider2D>();
                rightCollider.radius = _radius * 1.5f;
                rightCollider.isTrigger = true;
                body.mass = 10f;
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
            LockJoint(_joints.First().GetComponent<Rigidbody2D>(), leftCollider, leftTextMesh);
        else if (Input.GetButtonUp("J_LeftStickPress"))
            UnlockJoint(_joints.First().GetComponent<Rigidbody2D>(), leftTextMesh);

        if (Input.GetButtonDown("J_RightStickPress"))
            LockJoint(_joints.Last().GetComponent<Rigidbody2D>(), rightCollider, rightTextMesh);
        else if (Input.GetButtonUp("J_RightStickPress"))
            UnlockJoint(_joints.Last().GetComponent<Rigidbody2D>(), rightTextMesh);

        for (int i = 0; i < jointCount; i++)
        {
            _lineRenderer.SetPosition(i, _joints[i].transform.position);
        }

        // render the correct location for each label
        leftTextMesh.transform.position = _joints.First().transform.position + _offsetVector;
        rightTextMesh.transform.position = _joints.Last().transform.position + _offsetVector;
        leftTextMesh.transform.rotation = Quaternion.identity;
        rightTextMesh.transform.rotation = Quaternion.identity;
    }

    private void LockJoint(Rigidbody2D rigidBody, CircleCollider2D col2D, TextMesh textMesh)
    {
        ContactFilter2D filter = new ContactFilter2D();
        LayerMask mask = new LayerMask();
        mask.value = LayerMask.NameToLayer("Default");
        filter.layerMask = mask;
        Collider2D[] results = new Collider2D[10];
        col2D.OverlapCollider(filter, results);
        results = results.Where(c => c != null && c.gameObject.layer == mask.value).ToArray();

        if (results.Length > 0 && results[0] != null)
        {
            textMesh.color = pressedTextColor;
            rigidBody.constraints = RigidbodyConstraints2D.FreezePosition;
        }
    }

    private void UnlockJoint(Rigidbody2D rigidBody, TextMesh textMesh)
    {
        rigidBody.constraints = RigidbodyConstraints2D.None;
        textMesh.color = standardTextColor;
    }
}
