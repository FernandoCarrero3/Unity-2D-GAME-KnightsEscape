using UnityEngine;

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

    [Header("Salud del Enemigo")]
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentHealth = maxHealth;
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
}