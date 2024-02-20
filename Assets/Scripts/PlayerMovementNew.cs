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
    [SerializeField] private Vision vision;
    [SerializeField] private Vector2 visionOriginAdjustment;
    [SerializeField] private float ledgeGrabDistance;
    [SerializeField] private float ledgeGrabOffset;
    [SerializeField] private float ledgeGrabCooldown;
    [SerializeField][Range(0f, 1f)] private float ledgeGrabTolerance;

    private bool isJumping;
    private bool OnLedge;
    private bool canGrabLedge;
    private int ledgeDir;
    private float ledgeGrabbedForSeconds;
    private float moveSpeedMod = 1;
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    private SpriteRenderer renderer;
    private Vector2 visionDirectionAdjustment;

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
        vision.SetAim(new Vector2(transform.position.x + visionDirectionAdjustment.x, transform.position.y +  visionOriginAdjustment.y + visionDirectionAdjustment.y));

        if (ledgeGrabbedForSeconds < ledgeGrabCooldown) {
            ledgeGrabbedForSeconds += Time.deltaTime;
            canGrabLedge = false;
        }
        else canGrabLedge = true;

        if(Input.GetButton("Sprint")) moveSpeedMod = 2;
        else moveSpeedMod = 1;

        if (Input.GetAxisRaw("Horizontal") > 0 && body.velocity.x <= speedCap  * moveSpeedMod)
        {
            body.velocity = new Vector2(body.velocity.x + acceleration * Time.deltaTime  * moveSpeedMod, body.velocity.y);
            if (ledgeDir == -1) LeaveLedge();
            visionDirectionAdjustment.x = 0.1f;
            renderer.flipX = false;
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && body.velocity.x >= -speedCap  * moveSpeedMod)
        {
            body.velocity = new Vector2(body.velocity.x - acceleration * Time.deltaTime  * moveSpeedMod, body.velocity.y);
            if (ledgeDir == 1) LeaveLedge();
            visionDirectionAdjustment.x = -0.1f;
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

        if (Input.GetAxisRaw("Vertical") < 0)
        {
            if (OnGround())
            {
                if (transform.localScale.y >= 1f) transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
                transform.localScale = new Vector3(1, 0.5f, 1);
            }
            LeaveLedge();
        }
        else if(Input.GetAxisRaw("Vertical") > 0) {
            visionDirectionAdjustment.y = 6f;
        }
        else
        {
            if (transform.localScale.y < 1f) transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            transform.localScale = new Vector3(1, 1, 1);
            visionDirectionAdjustment.y = 0;
        }

        if (Input.GetButtonDown("Jump") && (OnGround() || OnLedge))
        {
            isJumping = true;
            body.velocity = new Vector2(body.velocity.x, jumpVelocity);
            LeaveLedge();
        }


        //Jumping Cutoff so when you let go of the button your jump stops short
        if (Input.GetButtonUp("Jump"))
        {
            if (isJumping && body.velocity.y > jumpCutoffVelocity)
            {
                body.velocity = new Vector2(body.velocity.x, jumpCutoffVelocity);
            }
            isJumping = false;
        }


        bool[] Ledges = LedgeCheck();


        //Left Ledge
        if (Ledges[0] && body.velocity.y < 0 && renderer.flipX && canGrabLedge)
        {
            ledgeDir = -1;
            OnLedge = true;
        }
        //Right Ledge
        else if (Ledges[1] && body.velocity.y < 0 && !renderer.flipX && canGrabLedge)
        {
            ledgeDir = 1;
            OnLedge = true;
        }

        if (OnLedge)
        {
            body.velocity = new Vector2(body.velocity.x, 0f);
            body.gravityScale = 0f;
            OnLedge = true;
        }

        else body.gravityScale = 1f;
    }

    private bool[] LedgeCheck()
    {
        Vector2 upperOrigin = new Vector2(transform.position.x, transform.position.y + ledgeGrabOffset + ledgeGrabTolerance);
        Vector2 lowerOrigin = new Vector2(transform.position.x, transform.position.y + ledgeGrabOffset);

        bool[] output = new bool[2];
        output[0] = false;
        output[1] = false;

        //Check on right
        RaycastHit2D raycastHitTopRight = Physics2D.Raycast(upperOrigin, Vector2.right, ledgeGrabDistance, platformLayer);
        RaycastHit2D raycastHitLowerRight = Physics2D.Raycast(lowerOrigin, Vector2.right, ledgeGrabDistance, platformLayer);
        if (raycastHitLowerRight.collider != null && raycastHitTopRight.collider == null) output[1] = true;

        //Check on Left
        RaycastHit2D raycastHitTopLeft = Physics2D.Raycast(upperOrigin, Vector2.left, ledgeGrabDistance, platformLayer);
        RaycastHit2D raycastHitLowerLeft = Physics2D.Raycast(lowerOrigin, Vector2.left, ledgeGrabDistance, platformLayer);
        if (raycastHitLowerLeft.collider != null && raycastHitTopLeft.collider == null) output[0] = true;


        return output;
    }

    //USE THIS INSTEAD OF OnLedge = false OR IT WILL DO THE BUG
    private void LeaveLedge(){
        if (ledgeGrabbedForSeconds > ledgeGrabCooldown) ledgeGrabbedForSeconds = 0;
        OnLedge = false;
    }

    private bool OnGround()
    {
        Vector2 colliderSize = boxCollider.bounds.size;
        RaycastHit2D boxCast = Physics2D.BoxCast(boxCollider.bounds.center, colliderSize, 0, Vector2.down, 0.1f, platformLayer);
        return boxCast.collider != null;
    }
}
