using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // physics parameters
    public float GravityStrength;
    public float JumpStrength;
    [Tooltip("Player will fall faster when not holding up and when descending")]
    [Range(0.6f, 1.4f)]
        public float FastFallMultiple;
    [Tooltip("Max vertical falling speed")]
        public float TerminalSpeed;

    public float RunTopSpeed;
    public float GlideTopSpeed;
    public float RunAcceleration;
    [Tooltip("Player slow down when there is no input")]
        public float RunDeceleration;
    [Tooltip("Relative acceleration when in the air compared to on the ground")]
    [Range(0.25f, 1f)]
        public float GlideAccelMultiple;
    [Tooltip("How much the player is slowed if they are running over max speed")]
        public float RunOverspeedResist;
    [Tooltip("How much the player is slowed if they are gliding over max speed")]
        public float GlideOverspeedResist;

    // old physics state
    bool wasGrounded;
    Vector2 wasVelocity;

    // current physics state
    bool physicsFrozen;
    Vector2 preFreezeVelocity;

    // grabbed references
    private Rigidbody2D rigid;

    // utility
    private Vector3 colliderExtents;


    /* Action Methods */

    void Start()
    {
        rigid = gameObject.GetComponent<Rigidbody2D>();
        rigid.sharedMaterial = new PhysicsMaterial2D();

        colliderExtents = gameObject.GetComponent<BoxCollider2D>().bounds.extents;

        wasGrounded = false;
        wasVelocity = Vector2.zero;
    }

    void Update()
    {
        float t = Time.deltaTime;

        if (physicsFrozen)
        {
            rigid.velocity = Vector2.zero;
        }
        else
        {
            // check ground
            RaycastHit2D groundHit = checkGroundCollisions();
            bool isGrounded = groundHit.collider != null;

            // correct for landing sticking
            if (!wasGrounded && isGrounded && Mathf.Abs(wasVelocity.x) > 0.5f)
            {
                float curDir = Mathf.Sign(rigid.velocity.x);
                Vector2 dir = curDir * new Vector2(groundHit.normal.y, -groundHit.normal.x);

                float mag = Mathf.Sqrt(
                    Mathf.Abs(dir.x * wasVelocity.x) +
                    Mathf.Abs(dir.y * wasVelocity.y)
                    );
                rigid.velocity = mag * dir;
            }

            // check inputs
            bool pressingUp = Input.GetKey(KeyCode.UpArrow);
            int pressingHoriz =
                (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) +
                (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);

            // appply gravity
            if (!isGrounded) applyGravity(t);

            // strafe
            if (pressingHoriz != 0) playerStrafeAccel(t, pressingHoriz, isGrounded, groundHit.normal);
            else playerStrafeDecel(t, isGrounded, groundHit.normal);

            // jump
            if (isGrounded && pressingUp) playerJump();

            // update physics material friction
            if (rigid.velocity.magnitude == 0)
            {
                rigid.sharedMaterial.friction = 100f;
            }
            else
            {
                rigid.sharedMaterial.friction = 0f;
            }

            // set for next frame
            wasGrounded = isGrounded;
            wasVelocity = rigid.velocity;
        }

        
    }


    /* Event Methods */

    public void SetPhysicsFrozen (bool frozen)
    {
        physicsFrozen = frozen;

        if (frozen)
        {
            preFreezeVelocity = rigid.velocity;
            rigid.velocity = Vector2.zero;
        }
        else
        {
            rigid.velocity = preFreezeVelocity;
        }
    }


    /* Physics Methods */

    private void applyGravity(float t)
    {
        // choose gravity strength
        float strength = GravityStrength;
        if (!Input.GetKey(KeyCode.UpArrow) || rigid.velocity.y < 0)
        {
            strength *= FastFallMultiple;
        }

        // apply air resistance
        if (rigid.velocity.y < -TerminalSpeed)
        {
            strength = 0;
        }
        else if (rigid.velocity.y < 0)
        {
            strength *= fallAirResistFunc(-rigid.velocity.y / TerminalSpeed);
        }

        // accelerate
        float yspd = rigid.velocity.y - (strength * t);
        rigid.velocity = new Vector2(
            rigid.velocity.x,
            Mathf.Clamp(yspd, -TerminalSpeed, Mathf.Infinity)
            );
    }

    private void playerJump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, JumpStrength);
    }

    private void playerStrafeAccel(float t, int pressing, bool grounded,
        Vector2 groundNormal)
    {
        // find the acceleration vector's direction, then its magnitude
        Vector2 accel = new Vector2(pressing, 0);
        if (grounded)
        {
            accel = pressing * new Vector2(groundNormal.y, -groundNormal.x);
        }
        accel *= RunAcceleration * (grounded ? 1 : GlideAccelMultiple);

        // overspeed resisting
        if (grounded && rigid.velocity.magnitude > RunTopSpeed)
        {
            float curDir = Mathf.Sign(rigid.velocity.x);
            accel = -curDir * new Vector2(groundNormal.y, -groundNormal.x);
            accel *= RunOverspeedResist;
        }
        else if (!grounded && Mathf.Abs(rigid.velocity.x) > GlideTopSpeed)
        {
            float curDir = Mathf.Sign(rigid.velocity.x);
            accel = new Vector2(-curDir, 0);
            accel *= GlideOverspeedResist;
        }

        // accelerate
        rigid.velocity = new Vector2(
            rigid.velocity.x + accel.x * t,
            rigid.velocity.y + accel.y * t
            );
    }

    private void playerStrafeDecel(float t, bool grounded, Vector2 groundNormal)
    {
        if (rigid.velocity.magnitude > 0)
        {
            float curDir = Mathf.Sign(rigid.velocity.x);

            // find the acceleration vector's direction, then its magnitude
            Vector2 accel = new Vector2(-curDir, 0);
            if (grounded)
            {
                accel = -curDir * new Vector2(groundNormal.y, -groundNormal.x);
            }
            accel *= RunAcceleration * (grounded ? 1 : GlideAccelMultiple);

            // decelerate
            rigid.velocity = new Vector2(
                rigid.velocity.x + accel.x * t,
                rigid.velocity.y + accel.y * t
                );

            // snap to zero
            if (Mathf.Sign(rigid.velocity.x) != curDir)
            {
                if (grounded) rigid.velocity = Vector2.zero;
                else rigid.velocity = new Vector2(0, rigid.velocity.y);
            }
        }
    }


    /* Utility Methods */

    private RaycastHit2D checkGroundCollisions()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(1.99f * colliderExtents.x, 2 * colliderExtents.y),
            0,
            Vector2.down,
            0.05f
            );

        // find the closest hit
        RaycastHit2D closestHit = new RaycastHit2D();
        closestHit.distance = 1000f;
        for (int j = 0; j < hits.Length; j++)
        {
            if (hits[j].collider.tag == "Ground"
                && hits[j].distance < closestHit.distance)
            {
                closestHit = hits[j];
            }
        }

        return closestHit;
    }

    private float fallAirResistFunc(float x)
    {
        return 2f * Mathf.Pow(2 / (Mathf.Exp(2f * x) + Mathf.Exp(-2f * x)), 2);
    }

}
