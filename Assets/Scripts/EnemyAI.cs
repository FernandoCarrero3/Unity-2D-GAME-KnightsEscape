using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrullar, Perseguir, Atacar }

    [Header("Comportamiento")]
    public bool esEstatico = false;

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
    public float fuerzaGolpeX = 3f;
    public float fuerzaGolpeY = 4f;
    public float tiempoAturdido = 0.3f;

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

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        currentHealth = maxHealth;
        audioSourceEnemigo = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player == null) return;

        if (esEstatico)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetFloat("Speed", 0); 
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRadius) currentState = EnemyState.Atacar;
        else if (distanceToPlayer <= visionRadius) currentState = EnemyState.Perseguir;
        else currentState = EnemyState.Patrullar;

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

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2((movingRight ? moveSpeed : -moveSpeed), rb.linearVelocity.y);

        if (groundCheck != null && wallCheck != null)
        {
            bool isGroundAhead = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
            bool isWallAhead = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);

            if (!isGroundAhead || isWallAhead)
            {
                Flip();
            }
        }
    }

    void Chase()
    {
        float directionX = player.position.x - transform.position.x;
        rb.linearVelocity = new Vector2(Mathf.Sign(directionX) * chaseSpeed, rb.linearVelocity.y);

        if (directionX > 0 && !movingRight) Flip();
        else if (directionX < 0 && movingRight) Flip();

        if (groundCheck != null)
        {
            bool isGroundAhead = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
            if (!isGroundAhead) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
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
        anim.SetTrigger("Dead");
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
        FindObjectOfType<HUDManager>().SumarPuntos(100);
        this.enabled = false;
    }

    private IEnumerator RutinaRetroceso()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (rb != null && player != null)
        {
            float direccion = transform.position.x < player.transform.position.x ? -1f : 1f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(fuerzaGolpeX * direccion, fuerzaGolpeY), ForceMode2D.Impulse);
            this.enabled = false;
            yield return new WaitForSeconds(tiempoAturdido);
            this.enabled = true;
        }
    }
}