using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementNew : MonoBehaviour
{
    public float acceleration;
    public float decceleration;

    [SerializeField] private float speedCap;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private float jumpCutoffVelocity;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask fallthruPlatformLayer;
    [SerializeField] private LayerMask ledgeGrabbable;
    [SerializeField] private Vision vision;
    [SerializeField] private Vector2 visionOriginAdjustment;
    [SerializeField] private float ledgeGrabDistance;
    [SerializeField] private float ledgeGrabOffset;
    [SerializeField] private float ledgeGrabCooldown;
    [SerializeField] private int extraJumps;
    [SerializeField] private int extraJumpsMax;
    [SerializeField][Range(0f, 1f)] private float ledgeGrabTolerance;

    private bool isJumping;
    private bool onLedge;
    private bool canGrabLedge;
    private bool isCrouching;
    private bool flipped;
    private int ledgeDir;
    private int flipMod = 1;
    private float ledgeGrabbedForSeconds;
    private float moveSpeedMod = 1;
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    private SpriteRenderer renderer; //Ignore this error unity is really dumb sometimes

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //Vision Cone Movement
        vision.SetOrigin(transform.position, visionOriginAdjustment);
        vision.SetAim(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        if (ledgeGrabbedForSeconds < ledgeGrabCooldown)
        {
            ledgeGrabbedForSeconds += Time.deltaTime;
            canGrabLedge = false;
        }
        else canGrabLedge = true;

        if (Input.GetButton("Sprint")) moveSpeedMod = 2;
        else moveSpeedMod = 1;

        if(OnGround()) extraJumps = extraJumpsMax;

        if(Input.GetKeyDown(KeyCode.R))
        {
            Flip();
        }

        if (Input.GetAxisRaw("Horizontal") > 0 && body.velocity.x <= speedCap * moveSpeedMod)
        {
            body.velocity = new Vector2(body.velocity.x + acceleration * Time.deltaTime * moveSpeedMod, body.velocity.y);
            if (ledgeDir == -1) LeaveLedge();
            renderer.flipX = false;
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && body.velocity.x >= -speedCap * moveSpeedMod)
        {
            body.velocity = new Vector2(body.velocity.x - acceleration * Time.deltaTime * moveSpeedMod, body.velocity.y);
            if (ledgeDir == 1) LeaveLedge();
            renderer.flipX = true;
        }
        else
        {
            //Left decceleration
            if (body.velocity.x > 0)
            {
                body.velocity = new Vector2(body.velocity.x - decceleration * Time.deltaTime, body.velocity.y);
                if (body.velocity.x < 0)
                {
                    body.velocity = new Vector2(0, body.velocity.y);
                }

            }

            //Right decceleration
            if (body.velocity.x < 0)
            {
                body.velocity = new Vector2(body.velocity.x + decceleration * Time.deltaTime, body.velocity.y);
                if (body.velocity.x > 0)
                {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
            }
        }

        if (Input.GetButtonDown("Jump") && (extraJumps >= 1 || onLedge || OnGround()) && !isCrouching)
        {
            isJumping = true;
            body.velocity = new Vector2(body.velocity.x, jumpVelocity * flipMod);
            LeaveLedge();
            extraJumps -= 1;
        }

        if (Input.GetAxisRaw("Vertical") < 0)
        {
            Crouch();
        }
        else if (CanUncrouch() && isCrouching)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            transform.localScale = new Vector3(1, 1, 1);
            isCrouching = false;
        }


        //Jumping Cutoff so when you let go of the button your jump stops short
        if (Input.GetButtonUp("Jump"))
        {
            if (isJumping && body.velocity.y * flipMod > jumpCutoffVelocity * flipMod)
            {
                body.velocity = new Vector2(body.velocity.x, jumpCutoffVelocity * flipMod);
            }
            isJumping = false;
        }


        bool[] Ledges = LedgeCheck();


        //Left Ledge
        if (Ledges[0] && body.velocity.y < 0 && renderer.flipX && canGrabLedge)
        {
            ledgeDir = -1;
            onLedge = true;
        }
        //Right Ledge
        else if (Ledges[1] && body.velocity.y < 0 && !renderer.flipX && canGrabLedge)
        {
            ledgeDir = 1;
            onLedge = true;
        }

        if (onLedge)
        {
            body.velocity = new Vector2(body.velocity.x, 0f);
            body.gravityScale = 0f;
            onLedge = true;
        }

        else if (flipped) body.gravityScale = -1f;
        else body.gravityScale = 1;
    }

    private bool[] LedgeCheck()
    {
        Vector2 upperOrigin = new Vector2(transform.position.x, transform.position.y + ledgeGrabOffset + ledgeGrabTolerance);
        Vector2 lowerOrigin = new Vector2(transform.position.x, transform.position.y + ledgeGrabOffset);

        bool[] output = new bool[2];
        output[0] = false;
        output[1] = false;

        //Check on right
        RaycastHit2D raycastHitTopRight = Physics2D.Raycast(upperOrigin, Vector2.right, ledgeGrabDistance, ledgeGrabbable);
        RaycastHit2D raycastHitLowerRight = Physics2D.Raycast(lowerOrigin, Vector2.right, ledgeGrabDistance, ledgeGrabbable);
        if (raycastHitLowerRight.collider != null && raycastHitTopRight.collider == null) output[1] = true;

        //Check on Left
        RaycastHit2D raycastHitTopLeft = Physics2D.Raycast(upperOrigin, Vector2.left, ledgeGrabDistance, ledgeGrabbable);
        RaycastHit2D raycastHitLowerLeft = Physics2D.Raycast(lowerOrigin, Vector2.left, ledgeGrabDistance, ledgeGrabbable);
        if (raycastHitLowerLeft.collider != null && raycastHitTopLeft.collider == null) output[0] = true;


        return output;
    }

    //USE THIS INSTEAD OF OnLedge = false OR IT WILL DO THE BUG
    private void LeaveLedge()
    {
        if (!onLedge) return;
        if (ledgeGrabbedForSeconds > ledgeGrabCooldown) ledgeGrabbedForSeconds = 0;
        onLedge = false;
    }

    private bool OnGround()
    {
        Vector2 colliderSize = boxCollider.bounds.size;
        Vector2 colliderBottom = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.center.y - (colliderSize.y/2));
        Vector2 colliderTop = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.center.y + (colliderSize.y / 2));

        RaycastHit2D boxCast;
        if (flipped) boxCast = Physics2D.BoxCast(colliderTop, colliderSize * 0.1f, 0, Vector2.up, 0.1f, platformLayer | fallthruPlatformLayer);
        else boxCast = Physics2D.BoxCast(colliderBottom, colliderSize * 0.1f, 0, Vector2.down, 0.1f, platformLayer | fallthruPlatformLayer);

        Debug.DrawRay(colliderTop, Vector2.up, Color.green);
        return boxCast.collider != null;
    }

    private void Crouch()
    {
        if (OnGround() && !isCrouching)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
            transform.localScale = new Vector3(1, 0.5f, 1);
            isCrouching = true;
        }
    }

    private void Flip()
    {
        if (flipped)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z);
            transform.localScale = new Vector3(1, 1, 1);
            flipped = false;
            flipMod = 1;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 2.5f, transform.position.z);
            transform.localScale = new Vector3(1, -1, 1);
            flipped = true;
            flipMod = -1;
        }
    }

    private bool CanUncrouch()
    {
        Vector2 colliderSize = boxCollider.bounds.size;
        RaycastHit2D boxCast = Physics2D.BoxCast(boxCollider.bounds.center, colliderSize, 0, Vector2.up, 0.5f, platformLayer);
        return boxCast.collider == null;
    }
}
