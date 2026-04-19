using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public int maxHealth = 150;
    public int currentHealth;
    private bool isDead = false;

    [Header("UI")]
    public HUDManager hudManager;

    [Header("Ataque y Combate")]
    public Transform attackPoint; // El punto de ataque que creamos
    public float attackRange = 0.5f; // El radio de la esfera invisible
    public LayerMask enemyLayers; // Para saber qué cosas son enemigos
    public int attackDamage = 50; // El daño de la espada

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

            // --- DETECTAR Y DAÑAR ENEMIGOS ---
            // 1. Crea la esfera invisible y guarda a todos los enemigos que toque
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            // 2. A cada enemigo tocado, le quitamos vida
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyAI>().TakeDamage(attackDamage);
            }
            // ----------------------------------------

            nextAttackTime = Time.time + attackCooldown;
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
        if (isDead) return;
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

        if (hudManager != null)
        {
            hudManager.ActualizarVidas(currentHealth, maxHealth);
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

        // Congelamos al personaje
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;

        Debug.Log("¡El caballero ha muerto! Saltando a Game Over...");

        // --- NUEVO: Guardar puntos y cambiar de escena ---
        if (hudManager != null)
        {
            hudManager.GuardarPuntuacionFinal();
        }

        // --- QUITAMOS EL INVOKE Y USAMOS LA CORRUTINA ---
        StartCoroutine(RutinaMuerte());
    }

    // --- NUEVA RUTINA QUE HACE LA FOTO ---
    IEnumerator RutinaMuerte()
    {
        // 1. Esperamos 2 segundos dramáticos
        yield return new WaitForSeconds(2f);

        // 2. Esperamos a que la cámara termine de renderizar este fotograma
        yield return new WaitForEndOfFrame();

        // 3. ¡Click! Hacemos la captura y la guardamos en la variable del otro script
        GameOverManager.capturaDePantalla = ScreenCapture.CaptureScreenshotAsTexture();

        // 4. Cambiamos de escena
        SceneManager.LoadScene("GameOver");
    }
}