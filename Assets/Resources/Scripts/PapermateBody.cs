using System.Collections;
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
    public float upwardLift = 10f;
    public float glideVelocityMax = 4f;

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

    private Vector3 movingSpawn = new Vector3();

    float time = 0f;

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
        _offsetVector = new Vector3(0f, labelOffset, -2f);
        InitializeBody();
    }

    // used to build out the physics body and joints of papermate
    private void InitializeBody(bool unCrinkle = false)
    {
        float segLen = unitLength / (float)jointCount;
        _joints = new List<GameObject>();
        Rigidbody2D prevBody = null;
        for (int i = 0; i < jointCount; i++)
        {

            GameObject joint = new GameObject("joint_" + i);
            joint.transform.localPosition = new Vector3(transform.position.x, transform.position.y + (segLen * i), transform.position.z);
            if (unCrinkle)
                joint.transform.localPosition = new Vector3(movingSpawn.x, movingSpawn.y + (segLen * i), movingSpawn.z);

            joint.layer = LayerMask.NameToLayer("Nonattachable");
            joint.transform.SetParent(transform);
            joint.AddComponent<WinTrigger>();

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
                leftCollider.radius = _radius * 2f;
                leftCollider.isTrigger = true;
                body.mass = 10f;
                capsuleCollider.offset = new Vector2(0, 0.1f);
                capsuleCollider.size = new Vector2(0.21f, 0.21f);

            }
            if (i == jointCount - 1)
            {
                rightCollider = joint.AddComponent<CircleCollider2D>();
                rightCollider.radius = _radius * 2f;
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
        bool amIGrounded = IsPaperGrounded();
        float framePower = amIGrounded ? power : airPower;
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
        /*
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
        */
        if (Input.GetButtonDown("CheatModeButton"))
            toggleCheatMode();
        if (Input.GetButtonDown("UnwrinkleButton"))
            Uncrinkle();
        if (Input.GetButtonDown("ResetGame"))
            toggleCheatMode();
        UpdateLineRendererPositions();



        // apply special upward force when you are parallel
        Vector2 vel = _joints.First().GetComponent<Rigidbody2D>().velocity;
        if (!amIGrounded && vel.y < 0)
        {
            Transform left, right;
            if (_joints.Last().transform.position.x >= _joints.First().transform.position.x)
            {
                right = _joints.Last().transform;
                left = _joints.First().transform;
            }
            else
            {
                left = _joints.Last().transform;
                right = _joints.First().transform;
            }

            // find the ratio of horizontal to vertical spread
            float distance = Vector3.Distance(left.position, right.position);
            float horiz = Mathf.Abs(right.position.x - left.position.x);
            float hRatio = horiz / distance;
            float vert = left.position.y - right.position.y;
            float vRatio = vert / distance;
            float vertMove = Mathf.Abs(vel.x) > glideVelocityMax && Mathf.Sign(vel.x) == Mathf.Sign(vRatio) ? 0f : Mathf.Sign(vRatio) * 100 * hRatio;
            Vector3 currentUpLift = new Vector3(vertMove, Mathf.Abs(vel.y * upwardLift * hRatio), 0f);

            // apply the upward lift to each point
            _joints.First().GetComponent<Rigidbody2D>().AddForce(currentUpLift);
            _joints.Last().GetComponent<Rigidbody2D>().AddForce(currentUpLift);
        }

        // render the correct location for each label
        leftSprite.transform.position = _joints.First().transform.position + _offsetVector;
        rightSprite.transform.position = _joints.Last().transform.position + _offsetVector;
        leftSprite.transform.rotation = Quaternion.identity;
        rightSprite.transform.rotation = Quaternion.identity;
        movingSpawn = _joints[0].transform.position;
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
            distJt.autoConfigureDistance = false;
            distJt.distance = 0.01f;
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

    private void Uncrinkle()
    {
        foreach (GameObject obj in _joints)
            DestroyImmediate(obj);
        _joints.Clear();
        InitializeBody(true);
        CameraPlayerController camera = FindObjectOfType<CameraPlayerController>();
        camera.mainPlayer = _joints[jointCount / 2].gameObject;

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

    private Vector3 zLineOffset = new Vector3(0, 0, -1);
    private void UpdateLineRendererPositions()
    {
        _lineRenderer.SetPositions(_joints.Select(j => (j.transform.position + zLineOffset))
                                   .ToArray());
    }

    public void FreezeBody()
    {
        foreach(GameObject joint in _joints)
        {
            Destroy(leftSprite.gameObject);
            Destroy(rightSprite.gameObject);
            Destroy(joint);
            Destroy(gameObject);
        }
    }
}
