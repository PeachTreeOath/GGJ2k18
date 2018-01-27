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
    public float power = 350;
    public float airPower = 20f;

    public SpriteRenderer leftSprite;
    public SpriteRenderer rightSprite;

    public float labelOffset = 0.25f;
    public Color standardTextColor = Color.white;
    public Color pressedTextColor = new Color(1f, 0.6f, 0f);

    private float _radius;
    private LineRenderer _lineRenderer;
    private List<GameObject> _joints;
    private CircleCollider2D leftCollider;
    private CircleCollider2D rightCollider;
    private Vector3 _offsetVector;
    private int staticPhysicsLayer;
    private int grabbablePhysicsLayer;

    private bool cheatModeEnabled = false;

    private DistanceJoint2D _leftGrabJoint;
    private DistanceJoint2D _rightGrabJoint;
    private Sprite leftSpriteOff;
    private Sprite rightSpriteOff;
    private Sprite leftSpriteOn;
    private Sprite rightSpriteOn;

    // Use this for initialization
    private void Start()
    {
        leftSpriteOff = Resources.Load<Sprite>("Textures/leftButtonOff");
        rightSpriteOff = Resources.Load<Sprite>("Textures/rightButtonOff");
        leftSpriteOn = Resources.Load<Sprite>("Textures/leftButtonOn");
        rightSpriteOn = Resources.Load<Sprite>("Textures/rightButtonOn");

        staticPhysicsLayer = LayerMask.NameToLayer("Default");
        grabbablePhysicsLayer = LayerMask.NameToLayer("Grabbable");

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
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (prevBody != null)
            {
                DistanceJoint2D distJt = joint.AddComponent<DistanceJoint2D>();
                distJt.connectedBody = prevBody;
                HingeJoint2D hingeJoint = joint.AddComponent<HingeJoint2D>();
                hingeJoint.useLimits = true;
                hingeJoint.connectedBody = prevBody;
                JointAngleLimits2D limits = new JointAngleLimits2D();
                limits.max = 0;
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
                capsuleCollider.offset = new Vector2(0, 0.1f);
                capsuleCollider.size = new Vector2(0.21f, 0.21f);

            }
            if (i == jointCount - 1)
            {
                rightCollider = joint.AddComponent<CircleCollider2D>();
                rightCollider.radius = _radius * 1.5f;
                rightCollider.isTrigger = true;
                body.mass = 10f;
                capsuleCollider.offset = new Vector2(0, -0.1f);
                capsuleCollider.size = new Vector2(0.21f, 0.21f);
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
            _leftGrabJoint = LockJoint(_joints.First().GetComponent<Rigidbody2D>(), leftCollider, leftSprite, true);
        else if (Input.GetButtonUp("KeyGrabLeft"))
            UnlockJoint(_joints.First().GetComponent<Rigidbody2D>(), leftSprite, _leftGrabJoint, true);

        if (Input.GetButtonDown("KeyGrabRight"))
            _rightGrabJoint = LockJoint(_joints.Last().GetComponent<Rigidbody2D>(), rightCollider, rightSprite, false);
        else if (Input.GetButtonUp("KeyGrabRight"))
            UnlockJoint(_joints.Last().GetComponent<Rigidbody2D>(), rightSprite, _rightGrabJoint, false);

        if (Input.GetButtonDown("KeyGrabRight"))
        {
            if (_rightGrabJoint == null)
                _rightGrabJoint = LockJoint(_joints.Last().GetComponent<Rigidbody2D>(), rightCollider, leftSprite, true);
        }
        else if (Input.GetButtonUp("KeyGrabRight"))
        {
            UnlockJoint(_joints.Last().GetComponent<Rigidbody2D>(), rightSprite, _rightGrabJoint, false);
            _rightGrabJoint = null;
        }

        if (Input.GetButtonDown("CheatModeButton"))
            toggleCheatMode();
        UpdateLineRendererPositions();

        // render the correct location for each label
        leftSprite.transform.position = _joints.First().transform.position + _offsetVector;
        rightSprite.transform.position = _joints.Last().transform.position + _offsetVector;
        leftSprite.transform.rotation = Quaternion.identity;
        rightSprite.transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        leftSprite.transform.position += new Vector3(0, 0.25f, 0);
        rightSprite.transform.position += new Vector3(0, 0.25f, 0);
    }

    /// <summary>
    /// Check to see if any of the joints in the papermate are colliding with physical bodies
    /// </summary>
    /// <returns></returns>
    private bool IsPaperGrounded()
    {
        if (cheatModeEnabled)
        {
            return true;
        }
        foreach (GameObject jt in _joints)
        {
            CapsuleCollider2D[] cols2D = jt.GetComponents<CapsuleCollider2D>();
            foreach (CapsuleCollider2D col2D in cols2D)
            {
                Collider2D[] results = new Collider2D[10];
                col2D.OverlapCollider(new ContactFilter2D(), results);
                results = results.Where(c => c != null && c.gameObject.layer == staticPhysicsLayer).ToArray();
                if (results.Length > 0 && results[0] != null)
                {
                    return true;
                }
            }
        }

        // if we are grabbing a grounded object we can also return true
        if (_leftGrabJoint != null && _leftGrabJoint.connectedBody.gameObject.layer == staticPhysicsLayer)
            return true;
        if (_rightGrabJoint != null && _rightGrabJoint.connectedBody.gameObject.layer == staticPhysicsLayer)
            return true;

        return false;
    }

    private DistanceJoint2D LockJoint(Rigidbody2D rigidBody, CircleCollider2D col2D, SpriteRenderer sprite, bool isLeft)
    {
        Collider2D[] results = new Collider2D[10];
        col2D.OverlapCollider(new ContactFilter2D(), results);
        results = results.Where(c => c != null && (c.gameObject.layer == staticPhysicsLayer || c.gameObject.layer == grabbablePhysicsLayer)).ToArray();

        if (results.Length > 0 && results[0] != null)
        {
            if (isLeft)
                leftSprite.sprite = leftSpriteOn;
            else
                rightSprite.sprite = rightSpriteOn;

            DistanceJoint2D distJt = rigidBody.gameObject.AddComponent<DistanceJoint2D>();
            distJt.connectedBody = results[0].attachedRigidbody;
            distJt.connectedAnchor = results[0].transform.InverseTransformPoint(rigidBody.transform.position);
            return distJt;
        }
        return null;
    }

    private void UnlockJoint(Rigidbody2D rigidBody, SpriteRenderer sprite, DistanceJoint2D grabJoint, bool isLeft)
    {
        if (isLeft)
            leftSprite.sprite = leftSpriteOff;
        else
            rightSprite.sprite = rightSpriteOff;
        GameObject.Destroy(grabJoint);
    }

    private void toggleCheatMode()
    {
        cheatModeEnabled = !cheatModeEnabled;

        if (cheatModeEnabled)
        {
            Debug.Log("Cheat mode enabled.");
            foreach (GameObject joint in _joints)
            {
                Collider2D[] colliders = joint.GetComponents<Collider2D>();
                foreach (Collider2D collider in colliders)
                {
                    collider.enabled = false;
                }

                Rigidbody2D rb = joint.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0;
            }
        }
        else
        {
            Debug.Log("Cheat mode disabled.");
            foreach (GameObject joint in _joints)
            {
                Collider2D[] colliders = joint.GetComponents<Collider2D>();
                foreach (Collider2D collider in colliders)
                {
                    collider.enabled = true;

                }

                Rigidbody2D rb = joint.GetComponent<Rigidbody2D>();
                rb.gravityScale = 1;
            }
        }
    }

    private void UpdateLineRendererPositions()
    {
        _lineRenderer.SetPositions(_joints.Select(j => j.transform.position).ToArray());

    }
}
