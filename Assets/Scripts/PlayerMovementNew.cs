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
    [SerializeField] private float walljumpHeight;
    [SerializeField] private float walljumpDistance;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private Vision vision;
    [SerializeField] private Vector2 visionOriginAdjustment;

    private bool isJumping;
    private bool wallJumping;
    private string direction;
    private string moving;
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    private float initialGravity;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        //Vision Cone Movement
        vision.SetOrigin(new Vector3(transform.position.x + visionOriginAdjustment.x, transform.position.y + visionOriginAdjustment.y, 0));
        vision.SetAim(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        if (Input.GetAxisRaw("Horizontal") > 0 && body.velocity.x <= speedCap)
        {
            body.velocity = new Vector2(body.velocity.x + acceleration * Time.deltaTime, body.velocity.y);
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && body.velocity.x >= -speedCap)
        {
            body.velocity = new Vector2(body.velocity.x - acceleration * Time.deltaTime, body.velocity.y);
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

        if (Input.GetButtonDown("Jump") && OnGround())
        {
            isJumping = true;
            body.velocity = new Vector2(body.velocity.x, jumpVelocity);
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
    }


    private bool OnGround()
    {
        Vector2 colliderSize = boxCollider.bounds.size;
        RaycastHit2D boxCast = Physics2D.BoxCast(boxCollider.bounds.center, colliderSize, 0, Vector2.down, 0.01f, platformLayer);
        return boxCast.collider != null;
    }
}
