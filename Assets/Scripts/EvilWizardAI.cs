using UnityEngine;
using System.Collections;

public class EvilWizardAI : MonoBehaviour
{
    public enum EstadoMago { Patrullar, Perseguir, Atacar, Aturdido }

    [Header("Estado Actual")]
    public EstadoMago estadoActual = EstadoMago.Patrullar;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 4f;
    public float radioVision = 8f;

    [Header("Mini-Árbol de Decisión (Ataques)")]
    public float rangoAtaqueCorto = 1.5f;
    public float rangoAtaqueLargo = 4f;
    public float tiempoEntreAtaques = 2f;
    private float proximoAtaque = 0f;

    [Header("Combate")]
    public Transform puntoAtaque;
    public LayerMask capaJugador;
    public int dañoAtaque1 = 20;
    public int dañoAtaque2 = 40;

    [Header("Salud y Retroceso")]
    public int maxHealth = 200;
    private int currentHealth;
    public float fuerzaGolpeX = 3f;
    public float fuerzaGolpeY = 2f;
    public float tiempoAturdido = 0.5f;

    [Header("Sensores del Suelo")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.1f;
    public LayerMask capaSuelo;

    [Header("Eventos de Jefe")]
    public GameObject barreraMagica;

    [Header("Sonidos")]
    public AudioClip sonidoRecibirDaño;
    private AudioSource audioSource;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private bool mirandoDerecha = false;
    private bool yaEstaMuerto = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (yaEstaMuerto || player == null || estadoActual == EstadoMago.Aturdido) return;

        float distanciaAlJugador = Vector2.Distance(transform.position, player.position);

        if (distanciaAlJugador <= rangoAtaqueLargo && Time.time >= proximoAtaque && estadoActual != EstadoMago.Atacar)
        {
            estadoActual = EstadoMago.Atacar;

            if (distanciaAlJugador <= rangoAtaqueCorto)
            {
                StartCoroutine(RutinaAtaque(1, rangoAtaqueCorto, dañoAtaque1, "Attack1"));
            }
            else
            {
                StartCoroutine(RutinaAtaque(2, rangoAtaqueLargo, dañoAtaque2, "Attack2"));
            }
        }
        else if (distanciaAlJugador <= radioVision && estadoActual != EstadoMago.Atacar)
        {
            estadoActual = EstadoMago.Perseguir;
        }
        else if (distanciaAlJugador > radioVision && estadoActual != EstadoMago.Atacar)
        {
            estadoActual = EstadoMago.Patrullar;
        }

        // --- LA MÁQUINA DE ESTADOS ---
        switch (estadoActual)
        {
            case EstadoMago.Patrullar:
                ComportamientoPatrullar();
                break;
            case EstadoMago.Perseguir:
                ComportamientoPerseguir();
                break;
            case EstadoMago.Atacar:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }

    private void ComportamientoPatrullar()
    {
        anim.Play("Run");
        rb.linearVelocity = new Vector2((mirandoDerecha ? velocidadPatrulla : -velocidadPatrulla), rb.linearVelocity.y);

        bool tocandoSueloAdelante = Physics2D.OverlapCircle(groundCheck.position, checkRadius, capaSuelo);
        bool tocandoPared = wallCheck != null && Physics2D.OverlapCircle(wallCheck.position, checkRadius, capaSuelo);

        // --- LOS CHIVATOS ---
        if (!tocandoSueloAdelante)
        {
            Debug.Log("¡ME DOY LA VUELTA PORQUE NO TOCO EL SUELO!");
            Flip();
        }
        else if (tocandoPared)
        {
            Debug.Log("¡ME DOY LA VUELTA PORQUE CHOCO CON LA PARED!");
            Flip();
        }
    }

    private void ComportamientoPerseguir()
    {
        anim.Play("Run"); // Arreglo animación

        float direccionX = player.position.x - transform.position.x;
        rb.linearVelocity = new Vector2(Mathf.Sign(direccionX) * velocidadPersecucion, rb.linearVelocity.y);

        if (direccionX > 0 && !mirandoDerecha) Flip();
        else if (direccionX < 0 && mirandoDerecha) Flip();
    }

    private IEnumerator RutinaAtaque(int tipo, float radioImpacto, int daño, string nombreAnimacion)
    {
        rb.linearVelocity = Vector2.zero;
        anim.Play(nombreAnimacion, -1, 0f);

        yield return new WaitForSeconds(0.4f);

        Collider2D[] jugadoresGolpeados = Physics2D.OverlapCircleAll(puntoAtaque.position, radioImpacto, capaJugador);

        foreach (Collider2D jugador in jugadoresGolpeados)
        {
            jugador.GetComponent<PlayerController>().TakeDamage(daño);
        }

        yield return new WaitForSeconds(0.6f);

        proximoAtaque = Time.time + tiempoEntreAtaques;
        estadoActual = EstadoMago.Perseguir;
    }

    private void Flip()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    public void TakeDamage(int damage)
    {
        if (yaEstaMuerto) return;

        currentHealth -= damage;
        StopAllCoroutines();

        if (currentHealth > 0)
        {
            anim.Play("Hit", -1, 0f);

            if (audioSource != null && sonidoRecibirDaño != null)
            {
                audioSource.PlayOneShot(sonidoRecibirDaño);
            }

            StartCoroutine(RutinaAturdido());
        }
        else
        {
            yaEstaMuerto = true;
            Die();
        }
    }

    private IEnumerator RutinaAturdido()
    {
        estadoActual = EstadoMago.Aturdido;

        float direccion = transform.position.x < player.transform.position.x ? -1f : 1f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(fuerzaGolpeX * direccion, fuerzaGolpeY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(tiempoAturdido);

        estadoActual = EstadoMago.Perseguir;
    }

    void Die()
    {
        anim.Play("Death", -1, 0f);
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;

        HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
        if (hud != null) hud.SumarPuntos(500);

        if (barreraMagica != null)
        {
            Destroy(barreraMagica); // Hace que el objeto desaparezca por completo
        }

        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioVision);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaqueCorto);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, rangoAtaqueLargo);

        if (puntoAtaque != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(puntoAtaque.position, checkRadius);
        }
    }
}