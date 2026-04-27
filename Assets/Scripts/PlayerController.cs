using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;

    [Header("Wall Slide")]
    public Transform wallCheck;
    public LayerMask wallLayer;
    public float wallSlidingSpeed = 2f; // Velocidad lenta a la que cae rozando la pared
    private bool isWallSliding;

    [Header("Parkour - Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(7f, 12f);
    public float wallJumpDuration = 0.3f; // Tiempo que pierdes el control al rebotar
    private bool isWallJumping;

    [Header("Rodar (Roll)")]
    public float rollSpeed = 8f; // Velocidad del impulso
    public float rollTime = 0.4f; // Cuánto dura la voltereta
    public float rollCooldown = 1f; // Tiempo de espera para volver a rodar
    private bool isRolling;
    private bool canRoll = true;

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

    [Header("Efectos de Sonido")]
    public AudioClip sonidoEspada;
    public AudioClip sonidoDaño;
    public AudioClip sonidoMuerte;
    private AudioSource audioSourceSFX;

    void Start()
    {
        PlayerPrefs.SetInt("PuntuacionActual", 0);
        int saludGuardada = PlayerPrefs.GetInt("MaxHealth", 150);

        maxHealth = saludGuardada;
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        currentHealth = maxHealth;

        audioSourceSFX = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDead) return;

        // Si el personaje está rodando, no le dejamos caminar, atacar ni saltar
        if (isRolling)
        {
            return;
        }

        if (isWallJumping)
        {
            return;
        }

        // Detectar si pulsamos el botón de rodar
        if (Input.GetKeyDown(KeyCode.LeftShift) && canRoll)
        {
            StartCoroutine(RutinaRodar());
        }

        // Movimiento horizontal
        moveInput = Input.GetAxisRaw("Horizontal");

        // Salto
        if ((Input.GetButtonDown("Jump") && isGrounded) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (isWallSliding)
            {
                EjecutarWallJump();
            }

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

            audioSourceSFX.PlayOneShot(sonidoEspada);

            // --- DETECTAR Y DAÑAR ENEMIGOS ---
            // 1. Crea la esfera invisible y guarda a todos los enemigos que toque
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            // 2. A cada enemigo tocado, le quitamos vida
            foreach (Collider2D enemy in hitEnemies)
            {
                // Comprobamos si es un cerdo
                EnemyAI cerdo = enemy.GetComponent<EnemyAI>();
                if (cerdo != null)
                {
                    cerdo.TakeDamage(attackDamage);
                }

                // Comprobamos si es el nuevo enemigo volador
                EnemigoVoladorAI volador = enemy.GetComponent<EnemigoVoladorAI>();
                if (volador != null)
                {
                    volador.TakeDamage(attackDamage);
                }
            }

            nextAttackTime = Time.time + attackCooldown;
            comboTimer = attackCooldown + 0.5f;
        }

        // Girar sprite y las colisiones según dirección
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // <-- 3. Actualizar parámetros del Animator -->
        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);

        ComprobarWallSlide();
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

        if (!isWallJumping && !isRolling)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
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
            audioSourceSFX.PlayOneShot(sonidoDaño);
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

        audioSourceSFX.PlayOneShot(sonidoMuerte);

        Debug.Log("¡El caballero ha muerto! Saltando a Game Over...");

        // --- Guardar puntos y cambiar de escena ---
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

    // --- LÓGICA DEL WALL SLIDE ---
    private bool TocandoPared()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.3f, wallLayer);
    }

    private void ComprobarWallSlide()
    {
        // Si tocamos pared, NO estamos tocando el suelo, y estamos pulsando hacia los lados
        if (TocandoPared() && !isGrounded && Input.GetAxisRaw("Horizontal") != 0)
        {
            if (isWallJumping) return;
            isWallSliding = true;
            // Frenamos su caída para que resbale despacito
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));

            // Activamos animación de Wall Slide
            anim.SetBool("WallSlide", true);
        }
        else
        {
            isWallSliding = false;
            anim.SetBool("WallSlide", false);
        }
    }

    // --- LÓGICA DE RODAR (ROLL) ---
    private IEnumerator RutinaRodar()
    {
        canRoll = false;
        isRolling = true;

        // Disparamos la animación
        anim.SetTrigger("Roll");

        // Guardamos la gravedad original y la ponemos a 0 para que no caiga mientras rueda
        float gravedadOriginal = rb.gravityScale;
        rb.gravityScale = 0f;

        // Averiguamos hacia dónde mira (1 = derecha, -1 = izquierda)
        float direccionX = Mathf.Sign(transform.localScale.x);

        // Le damos el empujón
        rb.linearVelocity = new Vector2(direccionX * rollSpeed, 0f);

        // Esperamos lo que dure la voltereta
        yield return new WaitForSeconds(rollTime);

        // Restauramos todo
        rb.gravityScale = gravedadOriginal;
        isRolling = false;

        // Esperamos el cooldown para poder volver a rodar
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    // Función vacía para evitar el error de la animación de polvo del Asset
    public void AE_SlideDust()
    {

    }

    private void EjecutarWallJump()
    {
        StartCoroutine(RutinaWallJump());
    }

    private IEnumerator RutinaWallJump()
    {
        // 1. Activamos el modo salto y soltamos la pared
        isWallJumping = true;
        isWallSliding = false;
        anim.SetBool("WallSlide", false);

        // 2. Averiguamos hacia dónde mira ahora mismo
        float direccionX = Mathf.Sign(transform.localScale.x);

        // 3. Saltamos hacia el lado contrario
        float direccionSalto = -direccionX;

        // 4. Giramos al personaje a la fuerza para que mire a donde salta
        transform.localScale = new Vector3(direccionSalto, 1, 1);

        // 5. Vaciamos la velocidad actual antes de empujar
        rb.linearVelocity = Vector2.zero;

        // 6. Aplicamos el impulso
        rb.AddForce(new Vector2(direccionSalto * wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);

        // 7. Esperamos bloqueados (wallJumpDuration es 0.3f)
        yield return new WaitForSeconds(wallJumpDuration);

        // 8. Le devolvemos el control
        isWallJumping = false;
    }
}