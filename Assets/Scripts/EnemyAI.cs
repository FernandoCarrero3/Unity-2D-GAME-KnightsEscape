using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrullar, Perseguir, Atacar }

    [Header("Estado Actual")]
    public EnemyState currentState = EnemyState.Patrullar;

    [Header("Ajustes de Patrulla")]
    public float moveSpeed = 2f;
    private bool movingRight = false;

    [Header("Visión y Persecución")]
    public float visionRadius = 5f;
    public float chaseSpeed = 3.5f;
    private Transform player;

    [Header("Ataque")]
    public float attackRadius = 1.5f;
    public float attackCooldown = 1.5f;
    private float nextAttackTime = 0f;
    public int attackDamage = 35;

    [Header("Sensores")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Retroceso (Knockback)")]
    public float fuerzaGolpeX = 3f; // Fuerza hacia atrás
    public float fuerzaGolpeY = 4f; // Fuerza del saltito hacia arriba
    public float tiempoAturdido = 0.3f; // Tiempo que el cerdo no puede moverse

    [Header("Salud del Enemigo")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Efectos de Sonido")]
    public AudioClip sonidoDañoCerdo;
    private AudioSource audioSourceEnemigo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentHealth = maxHealth;

        audioSourceEnemigo = GetComponent<AudioSource>();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // El cerebro de la Máquina de Estados
        if (distanceToPlayer <= attackRadius) currentState = EnemyState.Atacar;
        else if (distanceToPlayer <= visionRadius) currentState = EnemyState.Perseguir;
        else currentState = EnemyState.Patrullar;

        // Ejecutar el comportamiento
        switch (currentState)
        {
            case EnemyState.Patrullar:
                Patrol();
                break;
            case EnemyState.Perseguir:
                Chase();
                break;
            case EnemyState.Atacar:
                Attack();
                break;
        }

        // <-- 3. ACTUALIZAR EL PARÁMETRO SPEED (PARA IDLE/RUN) -->
        // Usamos Mathf.Abs para saber si se mueve a cualquier lado
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2((movingRight ? moveSpeed : -moveSpeed), rb.linearVelocity.y);

        bool isGroundAhead = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        bool isWallAhead = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);

        if (!isGroundAhead || isWallAhead)
        {
            Flip();
        }
    }

    void Chase()
    {
        float directionX = player.position.x - transform.position.x;
        rb.linearVelocity = new Vector2(Mathf.Sign(directionX) * chaseSpeed, rb.linearVelocity.y);

        if (directionX > 0 && !movingRight) Flip();
        else if (directionX < 0 && movingRight) Flip();

        bool isGroundAhead = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (!isGroundAhead) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Attack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        float directionX = player.position.x - transform.position.x;
        if (directionX > 0 && !movingRight) Flip();
        else if (directionX < 0 && movingRight) Flip();

        if (Time.time >= nextAttackTime)
        {
            anim.SetTrigger("Attack");

            player.GetComponent<PlayerController>().TakeDamage(attackDamage);

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private bool yaEstaMuerto = false;

    public void TakeDamage(int damage)
    {

        if (yaEstaMuerto) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hit");

            StartCoroutine(RutinaRetroceso());

            if (audioSourceEnemigo != null && sonidoDañoCerdo != null)
            {
                audioSourceEnemigo.PlayOneShot(sonidoDañoCerdo);
            }
        }
        else
        {
            yaEstaMuerto = true;
            Die();
        }
    }

    void Die()
    {
        Debug.Log("¡El cerdo ha muerto!");

        // Reproduce la animación de morir (trigger "Dead")
        anim.SetTrigger("Dead");

        rb.bodyType = RigidbodyType2D.Static;
        // Desactivamos el collider para que el cadáver no estorbe ni nos empuje
        GetComponent<Collider2D>().enabled = false;

        FindObjectOfType<HUDManager>().SumarPuntos(100);

        // Desactivamos este script para que deje de perseguirnos y atacar estando muerto
        this.enabled = false;
    }

    private IEnumerator RutinaRetroceso()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (rb != null && player != null)
        {
            // 1. Calculamos desde dónde nos ha pegado el jugador
            // Si el jugador está más a la derecha, el cerdo sale volando a la izquierda (-1), y viceversa.
            float direccion = transform.position.x < player.transform.position.x ? -1f : 1f;

            // 2. Frenamos al cerdo en seco por si venía corriendo
            rb.linearVelocity = Vector2.zero;

            // 3. Le damos el empujón físico hacia atrás y hacia arriba (Impulso)
            rb.AddForce(new Vector2(fuerzaGolpeX * direccion, fuerzaGolpeY), ForceMode2D.Impulse);

            // 4. Pausamos su inteligencia artificial (este mismo script) para que no camine en el aire
            this.enabled = false;

            // 5. Esperamos a que caiga al suelo (aturdido)
            yield return new WaitForSeconds(tiempoAturdido);

            // 6. Le devolvemos la consciencia para que vuelva a atacar
            this.enabled = true;
        }
    }
}