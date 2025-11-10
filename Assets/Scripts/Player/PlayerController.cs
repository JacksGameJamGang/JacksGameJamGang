using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float jumpForce = 20f;

    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float fallSpeedMultiplyer = 2.5f;

	//fall speed limits
	private float fallSpeedBase;
	private float gravityScaleBase;
    private float gravityScaleMax;

	[Header("Ground Check Settings")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float groundCheckRadius = 0.2f;

    public static event Action OnPlayerJump;
    public static event Action<Collider2D> OnPlayerTouchGround;

	[SerializeField] private Animator animator;

    // Stuff to track movement and jumping
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    public bool isGrounded { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        fallSpeedBase = fallSpeed;
		gravityScaleBase = rb.gravityScale;
        gravityScaleMax = rb.gravityScale * 3;
    }

    void Update()
    {
        if (!GameStateManager.IsInPlayableState()) return;

		Jump();
    }

    private void FixedUpdate()
	{
		if (!GameStateManager.IsInPlayableState()) return;

		Movement();
    }

    //movement
    private void Movement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        if (gameObject.CompareTag("Player"))
            FlipPlayer(horizontalInput);
        else if (gameObject.CompareTag("Dog"))
            FlipPlayer(-horizontalInput);

        if (horizontalInput != 0)
            animator.SetBool("IsRunning", true);
        else
            animator.SetBool("IsRunning", false);
	}
    private void Jump()
    {
        isGrounded = TouchingGroundCheck();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            OnPlayerJump?.Invoke();
        }
    }
    public void DogAiForceJump()
    {
		rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
	}
    public void UpdateFallSpeed()
    {
        if (isGrounded && rb.gravityScale != gravityScaleBase)
        {
			rb.gravityScale = gravityScaleBase;
            fallSpeed = fallSpeedBase;
		}
        else if (!isGrounded && rb.gravityScale < gravityScaleMax)
        {
            fallSpeed += fallSpeedMultiplyer * Time.deltaTime;
			rb.gravityScale += fallSpeed * Time.deltaTime;
		}
    }

    void FlipPlayer(float horizontalInput)
    {
        if (horizontalInput > 0)
            spriteRenderer.flipX = false;
        else if (horizontalInput < 0)
            spriteRenderer.flipX = true;
    }

    //ground
    private bool TouchingGroundCheck()
    {
        Collider2D touchingGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isGrounded && touchingGround)
			OnPlayerTouchGround?.Invoke(touchingGround);

        return touchingGround;
	}
    public void DogTouchingGroundCheck()
    {
		isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
	}
	public LayerMask GetGroundLayerMask()
    {
        return groundLayer;
    }

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
	}
}
