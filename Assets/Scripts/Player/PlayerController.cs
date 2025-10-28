using NaughtyAttributes;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Inspector stuff
    [SerializeField] private float speed = 6f;
    [SerializeField] private float jumpForce = 20f;

    //fall speed
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float fallSpeedMultiplyer = 2.5f;

	//fall speed limits
	private float fallSpeedBase;
	private float gravityScaleBase;
    private float gravityScaleMax;

    [SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float groundCheckRadius = 0.2f;

	[SerializeField] private Animator animator;

    // Stuff to track movement and jumping
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;

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
        Jump();
    }

    private void FixedUpdate()
    {
        Movement();
    }

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

        UpdateFallSpeed();

	}

    private void Jump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //Debug.Log(isGrounded);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
    private void UpdateFallSpeed()
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

}
