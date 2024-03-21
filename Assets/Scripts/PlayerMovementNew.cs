using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovementNew : MonoBehaviour
{

    enum Upgrades
    {
        flip
    }

    [Header("Speed")]
    [Space(10)]
    public float acceleration;
    public float decceleration;
    [SerializeField] private bool allowSprint;
    [SerializeField] private float speedCap;

    [Header("Jumping")]
    [Space(10)]
    [SerializeField] private float jumpVelocity;
    [SerializeField] private float jumpCutoffVelocity;
    [SerializeField] private float fallSpeedCap;
    [SerializeField] private int extraJumps;
    [SerializeField] private int extraJumpsMax;



    [Header("Ledge Grabbing")]
    [Space(10)]
    [SerializeField] private LayerMask ledgeGrabbable;
    [SerializeField] private float ledgeGrabDistance;
    [SerializeField] private float ledgeGrabOffset;
    [SerializeField] private float ledgeGrabCooldown;
    [SerializeField][Range(0f, 1f)] private float ledgeGrabTolerance;

    [Header("Flipping")]
    [Space(10)]
    [SerializeField] private float flipDistance;
    [SerializeField] private LayerMask flipLayer;

    [Header("Vision")]
    [Space(10)]
    [SerializeField] private Vision vision;    
    [SerializeField] private Vector2 visionOriginAdjustment;
    [SerializeField] private GameObject visionCircle;

    [Header("Platform Layers")]
    [Space(10)]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask fallthruPlatformLayer;

    private Animator animator;
    private bool isJumping;

    private bool onLedge;
    private bool canGrabLedge;
    private bool isCrouching;
    private bool flipped;
    private int ledgeDir;
    private int flipMod = 1;
    private float ledgeGrabbedForSeconds;
    private float moveSpeedMod = 1;
    private Inventory inventory;
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    private SpriteRenderer renderer; //Ignore this error unity is really dumb sometimes

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
        inventory = GetComponent<Inventory>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //Vision Cone Movement
        vision.SetOrigin(transform.position, visionOriginAdjustment * flipMod);
        vision.SetAim(Camera.main.ScreenToWorldPoint(Input.mousePosition));


        //Basic Movement
        if (ledgeGrabbedForSeconds < ledgeGrabCooldown)
        {
            ledgeGrabbedForSeconds += Time.deltaTime;
            canGrabLedge = false;
        }
        else canGrabLedge = true;

        animator.SetFloat("Speed",Mathf.Abs(body.velocity.x)); 

        if (Input.GetButton("Sprint") && allowSprint) moveSpeedMod = 2;
        else moveSpeedMod = 1;

        if (OnLayer(groundLayer | fallthruPlatformLayer)) extraJumps = extraJumpsMax;

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

        if (Input.GetButtonDown("Jump") && (extraJumps >= 1 || onLedge || OnLayer(groundLayer | fallthruPlatformLayer)) && !isCrouching)
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
            transform.position = new Vector3(transform.position.x, transform.position.y + (0.5f * flipMod), transform.position.z);
            transform.localScale = new Vector3(1, 1*flipMod, 1);
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
        if (Ledges[0] && (body.velocity.y < 0 && flipMod == 1 || body.velocity.y > 0 && flipMod == 0) && renderer.flipX && canGrabLedge)
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
        
        else if (flipped) body.gravityScale = -2.5f;
        else body.gravityScale = 2.5f;    

        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x - body.position.x > 0) renderer.flipX = false;
        else renderer.flipX = true;  
        

        //Upgrades
        if (Input.GetKeyDown(KeyCode.R) && OnLayer(flipLayer) && inventory.upgrades[(int) Upgrades.flip].upgradeEnabled)
        {
            Flip();
        }

        body.velocity = new Vector2 (body.velocity.x, Mathf.Clamp(body.velocity.y, -fallSpeedCap, 100));

        if (Input.GetKeyDown(KeyCode.U) && Input.GetKeyDown(KeyCode.I)) SceneManager.LoadScene("menu");
    }

    private bool[] LedgeCheck()
    {
        Vector2 upperOrigin = new Vector2(transform.position.x, transform.position.y + (ledgeGrabOffset + ledgeGrabTolerance) * flipMod);
        Vector2 lowerOrigin = new Vector2(transform.position.x, transform.position.y + (ledgeGrabOffset) * flipMod);

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

    //Check if were standing on a layer using boxcasts
    private bool OnLayer(LayerMask layer)
    {
        Vector2 colliderSize = boxCollider.bounds.size;
        Vector2 colliderBottom = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.center.y - (colliderSize.y / 2));
        Vector2 colliderTop = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.center.y + (colliderSize.y / 2));

        RaycastHit2D boxCast;
        if (flipped) boxCast = Physics2D.BoxCast(colliderTop, colliderSize * 0.1f, 0, Vector2.up, 0.1f, layer);
        else boxCast = Physics2D.BoxCast(colliderBottom, colliderSize * 0.1f, 0, Vector2.down, 0.1f, layer);

        return boxCast.collider != null;
    }

    private void Crouch()
    {
        if (OnLayer(groundLayer | fallthruPlatformLayer) && !isCrouching)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
            transform.localScale = new Vector3(1, 0.5f*flipMod, 1);
            isCrouching = true;
        }
    }

    public void Flip()
    {
        if (flipped)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + flipDistance, transform.position.z);
            transform.localScale = new Vector3(1, 1, 1);
            flipped = false;
            flipMod = 1;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - flipDistance, transform.position.z);
            transform.localScale = new Vector3(1, -1, 1);
            flipped = true;
            flipMod = -1;
        }

        //Flip vision circle as well
        visionCircle.transform.localScale = new Vector3(visionCircle.transform.localScale.x, visionCircle.transform.localScale.y * -1);
    }

    private bool CanUncrouch()
    {
        Vector2 colliderSize = boxCollider.bounds.size;
        RaycastHit2D boxCast = Physics2D.BoxCast(boxCollider.bounds.center, colliderSize, 0, Vector2.up * flipMod, 0.5f, groundLayer);
        return boxCast.collider == null;
    }
}
