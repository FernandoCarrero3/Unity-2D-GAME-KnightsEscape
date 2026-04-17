using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Movimiento horizontal
        moveInput = Input.GetAxisRaw("Horizontal");

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Girar sprite según dirección
        if (moveInput > 0) spriteRenderer.flipX = false;
        else if (moveInput < 0) spriteRenderer.flipX = true;
    }

    void FixedUpdate()
    {
        // Comprobar si está en el suelo
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, 
            groundCheckRadius, 
            groundLayer
        );

        // Aplicar movimiento
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}