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
    private Animator anim;
    private bool isGrounded;
    private float moveInput;

    private int comboStep = 0;
    private float comboTimer = 0f;

    // --- VARIABLES DE COOLDOWN ---
    public float attackCooldown = 0.4f; // El tiempo que dura la animación del espadazo
    private float nextAttackTime = 0f;  // Cuándo podremos volver a atacar

    [Header("Salud")]
    public int maxHealth = 3;
    public int currentHealth;
    private bool isDead = false;

    [Header("UI")]
    public HUDManager hudManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        // Movimiento horizontal
        moveInput = Input.GetAxisRaw("Horizontal");

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // --- SISTEMA DE COMBOS ---
        // El temporizador va bajando. Si llega a 0, el combo se reinicia.
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                comboStep = 0;
            }
        }

        // Al hacer clic izquierdo Y si ya ha pasado el tiempo de recarga
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            comboStep++;
            if (comboStep > 3)
            {
                comboStep = 1;
            }

            anim.SetTrigger("Attack" + comboStep);

            // Bloqueamos el siguiente ataque durante 0.4 segundos (ajusta este valor si es necesario)
            nextAttackTime = Time.time + attackCooldown;

            // Damos un margen de medio segundo extra tras el cooldown para continuar el combo
            comboTimer = attackCooldown + 0.5f;
        }
        // -------------------------

        // Girar sprite según dirección
        if (moveInput > 0) spriteRenderer.flipX = false;
        else if (moveInput < 0) spriteRenderer.flipX = true;

        // <-- 3. Actualizar parámetros del Animator -->
        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);
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

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log("¡Ay! Vidas restantes: " + currentHealth);

        if (hudManager != null)
        {
            hudManager.ActualizarVidas(currentHealth);
        }

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt"); // Reproduce la animación de daño
        }
        else
        {
            Die(); // Si la vida llega a 0 o menos, morimos
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die"); // Reproduce la animación de muerte

        // Opcional: Quitamos el colisionador para que los enemigos pasen de largo y no nos sigan empujando
        GetComponent<Collider2D>().enabled = false;

        // Frenamos al personaje para que caiga quieto
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        Debug.Log("¡El caballero ha muerto! Game Over.");
    }
}