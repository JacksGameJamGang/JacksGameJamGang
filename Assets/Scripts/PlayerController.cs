using NUnit.Framework.Constraints;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Inspector values
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    // Values for tracking movement and jumping
    private Rigidbody2D rb;
    private bool isGrounded;

    // private int jumpCounter = 2; // In-case there's double jump

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //Debug.Log("is on ground?" + isGrounded);
        // Check for grounded status
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        // Jumping logic
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        /* This is the jumping conditions in-case there's double jump 
        if (Input.GetButtonDown("Jump") && jumpCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCounter--;
        }
        if (isGrounded) jumpCounter = 2;
        */
    }
    private void FixedUpdate()
    {
        // Horizontal movement logic
        float horizontalInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

}
