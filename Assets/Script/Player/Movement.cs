using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Tooltip("The animator controller used to animate the player.")]
    public Animator animator = null;

    public PhysicsMaterial2D movemat = null;
    public PhysicsMaterial2D stopmat = null;

    public float speed = 200;
    private Rigidbody2D rb = null;
    public GameObject checkGround = null;
    public bool OnGround = false;

    private Vector3 v_Velocity = Vector3.zero;

    [SerializeField]
    private LayerMask lm_Ground;

    private bool facingRight = true;
    [SerializeField]
    private int jumpCount = 1;
    public int maxJumpCount = 1;
    public float jumpTime = 0.4f;
    public float jumpDelay = 0f;
    public float jumpForce = 400f;


    //var for animator
    private bool isMoving = false;
    [SerializeField]
    private float airTime = 0f;
    [SerializeField]
    private float lastPositionHeight;
    private bool isJump = false;
    private bool isAirJump = false;


    //The InputManager to read input from
    private InputManager inputManager;




    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null )
        {
            Debug.LogError("Can't found player's Rigidbody!!!");
        }
        if (checkGround == null)
        {
            Debug.LogWarning("Can't found check ground object!!!");
        }
        if (movemat != null && stopmat != null) rb.sharedMaterial = stopmat;
        SetupInput();
        lastPositionHeight = gameObject.transform.position.y;
    }

    private void SetupInput()
    {
        if (inputManager == null)
        {
            inputManager = InputManager.instance;
        }
        if (inputManager == null)
        {
            Debug.LogWarning("There is no player input manager in the scene, there needs to be one for the Controller to work");
        }
    }

    // Update is called once per frame
    void Update()
    {
        isMoving = false;
        isJump = false;
        if (jumpDelay > 0) jumpDelay -= Time.deltaTime;
        CheckGround();
        CheckAirPosition();
        HandleInput();
        HandleAnimator();
    }

    private void HandleInput()
    {
        float move = 0f;
        move = inputManager.horizontalMoveAxis;
        if (move != 0f)
        {
            rb.sharedMaterial = movemat;
            Move(move);
        }
        else rb.sharedMaterial = stopmat;
        if (inputManager.jumpPress) Jump();
    }

    private void HandleAnimator()
    {
        animator.SetBool("OnGround", OnGround);
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsJump", isJump);
        animator.SetBool("IsAirJump", isAirJump);
        animator.SetFloat("AirTime", airTime);
    }

    private void CheckGround()
    {
        OnGround = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkGround.transform.position, 0.2f, lm_Ground);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                if (jumpDelay <= 0) OnGround = true;
                airTime = 0f;
                if (jumpDelay <= 0) jumpCount = 1;
            }
        }
    }

    public void CheckAirPosition()
    {
        if (!OnGround)
        {
            float currentHeight = gameObject.transform.position.y;
            if (currentHeight > lastPositionHeight)
            {
                if (airTime < 0f) airTime = 0f;
                airTime += Time.deltaTime;
            }
            else if (currentHeight < lastPositionHeight)
            {
                if (airTime > 0f) airTime = 0f;
                airTime -= Time.deltaTime;
            }
            lastPositionHeight = currentHeight;
        }
    }

    private void Move(float move)
    {
        
        if (OnGround)
        {
            Vector3 targetVelocity = new Vector2(move * speed, rb.velocity.y);
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref v_Velocity, 0.1f);
            isMoving = true;
        }
        if (move > 0 && !facingRight)
        {
            Flip();
        }
        else if (move < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Jump()
    {
        if (jumpCount > 0 && jumpDelay <= 0)
        {
            rb.AddForce(new Vector2(0f, jumpForce));
            jumpCount--;
            jumpDelay = 0.3f;
            isJump = true;
            if (!OnGround) isAirJump = true;
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

}
