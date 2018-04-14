using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PapermateBody behavior is responsible for building out the papermate body.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PapermateBodyNew : MonoBehaviour
{
    public int jointCount = 5;
    public int paperLength = 4;
    public float width = 0.2f;
    public float power = 27500;
    public float airPower = 2000;
    public float upwardLift = 10f;
    public float glideVelocityMax = 4f;
    public float stunDuration;

    public SpriteRenderer leftSprite;
    public SpriteRenderer rightSprite;

    public float labelOffset = 0.5f;
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
    private Sprite leftSpriteReady;
    private Sprite rightSpriteReady;
    private CameraPlayerController cameraPlayerController;

    private Vector3 movingSpawn = new Vector3();

    private bool isXbox_One_Controller = false;
    private bool isPS4_Controller = false;

    private bool leftGrabbed = false;
    private bool rightGrabbed = false;

    // These 4 are used to differentiate between bumper and trigger pulls so that they don't mix. I.e. holding bumper but then letting go of trigger shouldn't release your grab.
    private bool isKeyGrabbingLeft;
    private bool isKeyGrabbingRight;
    private bool isJoyGrabbingLeft;
    private bool isJoyGrabbingRight;

    private GameObject leftStabilizePointGo;
    private GameObject rightStabilizePointGo;
    private HingeJoint2D leftStabilizePoint;
    private HingeJoint2D rightStabilizePoint;
    private GameObject centerPoint;

    private bool isStabilized = false;
    private bool isStunned = false;

    // Use this for initialization
    private void Start()
    {
        cameraPlayerController = Camera.main.gameObject.GetComponent<CameraPlayerController>();
        leftSpriteOff = Resources.Load<Sprite>("Textures/leftButtonOff");
        rightSpriteOff = Resources.Load<Sprite>("Textures/rightButtonOff");
        leftSpriteOn = Resources.Load<Sprite>("Textures/leftButtonOn");
        rightSpriteOn = Resources.Load<Sprite>("Textures/rightButtonOn");
        leftSpriteReady = Resources.Load<Sprite>("Textures/leftButtonReady");
        rightSpriteReady = Resources.Load<Sprite>("Textures/rightButtonReady");

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
        Rigidbody2D prevBody = null;
        _joints = new List<GameObject>();

        float segLen = paperLength / (float)jointCount;
        Vector3 spawnLoc = unCrinkle ? movingSpawn : transform.position;

        for (int i = 0; i < jointCount; i++)
        {
            GameObject joint = new GameObject("joint_" + i);
            Transform jointTransform = joint.transform;
            jointTransform.localPosition = spawnLoc + new Vector3(0, segLen * i, 0);

            joint.layer = LayerMask.NameToLayer("Nonattachable");
            jointTransform.SetParent(transform);
            joint.AddComponent<WinTrigger>();

            Rigidbody2D body = joint.AddComponent<Rigidbody2D>();
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Add colliders on the ends for latching onto objects
            CapsuleCollider2D collider = joint.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.21f, segLen);

            if (i == 0)
            {

                leftCollider = joint.AddComponent<CircleCollider2D>();
                leftCollider.radius = segLen*0.75f;
                leftCollider.isTrigger = true;
                body.mass = 10f;
            }
            else if (i == jointCount - 1)
            {

                rightCollider = joint.AddComponent<CircleCollider2D>();
                rightCollider.radius = segLen/2;
                rightCollider.isTrigger = true;
                body.mass = 10f;

                HingeJoint2D hinge = joint.AddComponent<HingeJoint2D>();
                hinge.connectedBody = prevBody;
            }
            else
            {
                HingeJoint2D hinge = joint.AddComponent<HingeJoint2D>();
                hinge.connectedBody = prevBody;
            }

            _joints.Add(joint);
            prevBody = body;
        }

        // use world points and set up for three points per joint minus the two dangling off the ends
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = jointCount;
        UpdateLineRendererPositions();

        // set the middle point as the camera follower
        CameraPlayerController camera = FindObjectOfType<CameraPlayerController>();
        camera.mainPlayer = _joints[jointCount / 2].gameObject;

        transform.SetParent(_joints[jointCount / 2].gameObject.transform);

        //InitializeStabilizers(_joints.First().GetComponent<Rigidbody2D>(), _joints.Last().GetComponent<Rigidbody2D>());
    }

    private void Update()
    {
        string[] names = Input.GetJoystickNames();
        for (int x = 0; x < names.Length; x++)
        {
            //print(names[x].Length);
            if (names[x].Length == 19)
            {
                //print("PS4 CONTROLLER IS CONNECTED");
                isPS4_Controller = true;
            }
            else if (names[x].Length == 33)
            {
                //print("XBOX ONE CONTROLLER IS CONNECTED");
                isPS4_Controller = false;

            }
            else
            {
                //print("SOME OTHER CONTROLLER IS CONNECTED");
                isPS4_Controller = false;
            }
        }

        // can only apply forces if we are touching a physics body
        bool amIGrounded = IsPaperGrounded();
        float framePower = amIGrounded ? power : airPower;
        Rigidbody2D leftBody = _joints.First().GetComponent<Rigidbody2D>();
        Rigidbody2D rightBody = _joints.Last().GetComponent<Rigidbody2D>();

        // left end handling
        float h1 = Input.GetAxis("J_LeftStickX");
        float v1 = Input.GetAxis("J_LeftStickY");
        leftBody.AddForce(new Vector2(h1 * framePower * Time.deltaTime, v1 * framePower * Time.deltaTime));

        if (!leftGrabbed)
            IsJointContacting(leftCollider, leftSprite, true);

        if (Input.GetButton("KeyGrabLeft") && !leftGrabbed && !isStunned)
        {
            _leftGrabJoint = LockJoint(leftBody, leftCollider, leftSprite, true);
            isKeyGrabbingLeft = true;
        }
        else if (Input.GetButtonUp("KeyGrabLeft") && isKeyGrabbingLeft || isStunned)
        {
            UnlockJoint(leftBody, leftSprite, _leftGrabJoint, true);
            leftGrabbed = false;
            isKeyGrabbingLeft = false;
        }

        if (Input.GetAxisRaw("JoyGrabLeft") > 0 && !leftGrabbed && !isStunned)
        {
            _leftGrabJoint = LockJoint(leftBody, leftCollider, leftSprite, true);
            isJoyGrabbingLeft = true;
        }
        else if (Input.GetAxisRaw("JoyGrabLeft") == 0 && isJoyGrabbingLeft || isStunned)
        {
            UnlockJoint(leftBody, leftSprite, _leftGrabJoint, true);
            leftGrabbed = false;
            isJoyGrabbingLeft = false;
        }

        // right end handling
        float h2;
        float v2;
        if (isPS4_Controller)
        {
            h2 = Input.GetAxis("PS4_RightStickX");
            v2 = Input.GetAxis("PS4_RightStickY");
        }
        else
        {
            h2 = Input.GetAxis("J_RightStickX");
            v2 = Input.GetAxis("J_RightStickY");
        }

        rightBody.AddForce(new Vector2(h2 * framePower * Time.deltaTime, v2 * framePower * Time.deltaTime));

        if (!rightGrabbed)
            IsJointContacting(rightCollider, rightSprite, false);

        if (Input.GetButton("KeyGrabRight") && !rightGrabbed && !isStunned)
        {
            _rightGrabJoint = LockJoint(rightBody, rightCollider, rightSprite, false);
            isKeyGrabbingRight = true;
        }
        else if ((Input.GetButtonUp("KeyGrabRight") && isKeyGrabbingRight) || isStunned)
        {
            UnlockJoint(rightBody, rightSprite, _rightGrabJoint, false);
            rightGrabbed = false;
            isKeyGrabbingRight = false;
        }

        if (Input.GetAxisRaw("JoyGrabRight") > 0 && !rightGrabbed && !isStunned)
        {
            _rightGrabJoint = LockJoint(rightBody, rightCollider, rightSprite, false);
            isJoyGrabbingRight = true;
        }
        else if ((Input.GetAxisRaw("JoyGrabRight") == 0 && isJoyGrabbingRight) || isStunned)
        {
            UnlockJoint(rightBody, rightSprite, _rightGrabJoint, false);
            rightGrabbed = false;
            isJoyGrabbingRight = false;
        }

        /*if (Input.GetButton("Stabilize") && !isStabilized)
        {
            Stabilize(leftBody, rightBody);
        }
        else if (Input.GetButtonUp("Stabilize") && isStabilized)
        {
            Unstabilize();
        }*/

        // special keys
        if (isPS4_Controller)
        {
            if (Input.GetButtonDown("PS4_CheatModeButton"))
                toggleCheatMode();
            if (Input.GetButtonDown("PS4_UnwrinkleButton"))
                Uncrinkle();
        }
        else
        {
            if (Input.GetButtonDown("CheatModeButton"))
                toggleCheatMode();
            if (Input.GetButtonDown("UnwrinkleButton"))
                Uncrinkle();
        }
        if (Input.GetButtonDown("ResetGame"))
            Application.LoadLevel(0);

        // update graphic positions
        UpdateLineRendererPositions();

        // apply special upward force when falling
        //Vector2 vel = leftBody.velocity;
        //if (!amIGrounded && vel.y < 0)
        //{
        //    Vector2 upLift = GetLiftFromEndpoints(leftBody.position,
        //                                          rightBody.position, vel);
        //
        //    leftBody.AddForce(upLift);
        //   rightBody.AddForce(upLift);
        //}
    }

    private void LateUpdate()
    {
        Transform leftTransform = leftSprite.transform;
        Transform rightTransform = rightSprite.transform;
        Vector3 firstPosition = _joints.First().transform.position;

        // render the correct location for each label
        leftTransform.position = firstPosition + _offsetVector;
        rightTransform.position = _joints.Last().transform.position + _offsetVector;
        leftTransform.rotation = Quaternion.identity;
        rightTransform.rotation = Quaternion.identity;
        movingSpawn = firstPosition;
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
            {
                leftSprite.sprite = leftSpriteOn;
                leftGrabbed = true;
            }
            else
            {
                rightSprite.sprite = rightSpriteOn;
                rightGrabbed = true;
            }

            DistanceJoint2D distJt = rigidBody.gameObject.AddComponent<DistanceJoint2D>();
            distJt.connectedBody = results[0].attachedRigidbody;
            distJt.connectedAnchor = results[0].transform.InverseTransformPoint(rigidBody.transform.position);
            distJt.autoConfigureDistance = false;
            distJt.distance = 0.01f;
            AudioManager.instance.PlaySound("stick");
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

    private void InitializeStabilizers(Rigidbody2D leftRigidBody, Rigidbody2D rightRigidBody)
    {
        centerPoint = _joints[_joints.Count / 2];
        GameObject leftStabilizePointGo = GameObject.Find("leftStabilizePoint");
        GameObject rightStabilizePointGo = GameObject.Find("rightStabilizePoint");
        leftStabilizePoint = leftStabilizePointGo.GetComponent<HingeJoint2D>();
        rightStabilizePoint = rightStabilizePointGo.GetComponent<HingeJoint2D>();
        GetComponentInChildren<Stabilizer>().SetAnchor(centerPoint.transform);
        leftStabilizePoint.connectedBody = leftRigidBody;
        rightStabilizePoint.connectedBody = rightRigidBody;
        leftStabilizePoint.enabled = false;
        rightStabilizePoint.enabled = false;
    }

    private void Stabilize(Rigidbody2D leftRigidBody, Rigidbody2D rightRigidBody)
    {
        Debug.Log("Stabilizing");
        //Collider2D[] results = new Collider2D[10];
        //col2D.OverlapCollider(new ContactFilter2D(), results);
        //results = results.Where(c => c != null && (c.gameObject.layer == staticPhysicsLayer || c.gameObject.layer == grabbablePhysicsLayer)).ToArray();

        //distJt.connectedAnchor = results[0].transform.InverseTransformPoint(rigidBody.transform.position);

        isStabilized = true;
        leftStabilizePoint.enabled = true;
        rightStabilizePoint.enabled = true;
    }

    private void Unstabilize()
    {
        Debug.Log("Unstabilizing");
        leftStabilizePoint.enabled = false;
        rightStabilizePoint.enabled = false;
        isStabilized = false;
    }

    private bool IsJointContacting(CircleCollider2D col2D, SpriteRenderer sprite, bool isLeft)
    {
        Collider2D[] results = new Collider2D[10];
        col2D.OverlapCollider(new ContactFilter2D(), results);
        results = results.Where(c => c != null && (c.gameObject.layer == staticPhysicsLayer || c.gameObject.layer == grabbablePhysicsLayer)).ToArray();

        if (results.Length > 0 && results[0] != null)
        {
            if (isLeft)
            {
                leftSprite.sprite = leftSpriteReady;
            }
            else
            {
                rightSprite.sprite = rightSpriteReady;
            }

            return true;
        }

        if (isLeft)
        {
            leftSprite.sprite = leftSpriteOff;
        }
        else
        {
            rightSprite.sprite = rightSpriteOff;
        }

        return false;
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

    private Vector3 zLineOffset = new Vector3(0, 0, -1);// make closer to user

    private void UpdateLineRendererPositions()
    {
        _lineRenderer.SetPositions(_joints.Select(j => (j.transform.position + zLineOffset))
                                   .ToArray());
    }

    private Vector2 GetLiftFromEndpoints(Vector2 pos1, Vector2 pos2, Vector2 vel)
    {
        bool nonFlipped = pos1.x < pos2.x;
        Vector2 left = nonFlipped ? pos1 : pos2;
        Vector2 right = nonFlipped ? pos2 : pos1;

        // find the ratio of horizontal to vertical spread
        float distance = Vector2.Distance(left, right);
        float horiz = Mathf.Abs(right.x - left.x);
        float hRatio = horiz / distance;

        float vert = left.y - right.y;
        float vRatio = vert / distance;
        float horizForce = Mathf.Abs(vel.x) > glideVelocityMax && // exceeding max glide speed
                                vel.x * vRatio >= 0 ? // tilting in direction of motion
                                0f : Mathf.Sign(vRatio) * 100 * hRatio;

        float vertForce = Mathf.Abs(vel.y * upwardLift * hRatio);

        // apply the upward lift to each point
        return new Vector2(horizForce, vertForce);
    }

    public void FreezeBody()
    {
        foreach (GameObject joint in _joints)
        {

            cameraPlayerController.updateTime = false;

            Destroy(leftSprite.gameObject);
            Destroy(rightSprite.gameObject);
            Destroy(joint);
            Destroy(gameObject);

        }
    }

    public void ApplyKnockback(Vector3 enemyPos, float knockbackStrength)
    {
        //Calculate vector between enemy and centerpoint of paper
        Vector3 knockbackDirection = _joints[5].transform.position - enemyPos;
        knockbackDirection = knockbackDirection.normalized;
        Vector3 knockBackForce = knockbackDirection * knockbackStrength;

        foreach (GameObject joint in _joints)
        {
            Rigidbody2D rb = joint.GetComponent<Rigidbody2D>();
            rb.AddForce(knockBackForce, ForceMode2D.Impulse);
        }

        StartCoroutine(StunnedState());

    }

    IEnumerator StunnedState()
    {
        float time = 0f;
        isStunned = true;
        while (time < stunDuration)
        {
            time += Time.deltaTime;
            yield return null;
        }

        isStunned = false;


    }
}
