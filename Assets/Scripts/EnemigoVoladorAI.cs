using UnityEngine;
using System.Collections;

public class EnemigoVoladorAI : MonoBehaviour
{
    public enum EstadoVolador { Patrullar, Posicionarse, Cayendo, EnSuelo, Aturdido }

    [Header("Estado Actual")]
    public EstadoVolador estadoActual = EstadoVolador.Patrullar;

    [Header("Patrulla y Visión")]
    public float velocidadVuelo = 3f;
    public float radioVision = 7f;
    public Transform puntoPatrullaA;
    public Transform puntoPatrullaB;
    private Transform destinoActual;

    [Header("Ataque")]
    public float alturaSobreJugador = 3f;
    public float velocidadPersecucion = 5f;
    public float velocidadCaida = 10f;
    public int attackDamage = 40;
    public float tiempoRecuperacionSuelo = 2f;

    [Header("Salud y Retroceso")]
    public int maxHealth = 100;
    private int currentHealth;
    public float fuerzaGolpeX = 4f;
    public float fuerzaGolpeY = 2f;
    public float tiempoAturdido = 0.4f;

    [Header("Sensores Suelo")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Sonidos")]
    public AudioClip sonidoGolpe;
    private AudioSource audioSource;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private bool yaEstaMuerto = false;
    private bool mirandoDerecha = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;

        // Aseguramos que empiece flotando perfectamente
        rb.gravityScale = 0f;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        destinoActual = puntoPatrullaB;
        anim.Play("EnemigoVolador_Fly");
    }

    void Update()
    {
        if (yaEstaMuerto || player == null || estadoActual == EstadoVolador.Aturdido) return;

        switch (estadoActual)
        {
            case EstadoVolador.Patrullar:
                ComportamientoPatrullar();
                break;
            case EstadoVolador.Posicionarse:
                ComportamientoPosicionarse();
                break;
            case EstadoVolador.Cayendo:
                ComportamientoCayendo();
                break;
            case EstadoVolador.EnSuelo:
                // Se gestiona por corrutina
                rb.linearVelocity = Vector2.zero;
                break;
        }

        MirarAlObjetivo();
    }

    private void ComportamientoPatrullar()
    {
        if (destinoActual == null) return;

        transform.position = Vector2.MoveTowards(transform.position, destinoActual.position, velocidadVuelo * Time.deltaTime);

        if (Vector2.Distance(transform.position, destinoActual.position) < 0.2f)
        {
            destinoActual = destinoActual == puntoPatrullaA ? puntoPatrullaB : puntoPatrullaA;
        }

        if (Vector2.Distance(transform.position, player.position) <= radioVision)
        {
            estadoActual = EstadoVolador.Posicionarse;
        }
    }

    private void ComportamientoPosicionarse()
    {
        Vector2 posicionObjetivo = new Vector2(player.position.x, player.position.y + alturaSobreJugador);

        transform.position = Vector2.MoveTowards(transform.position, posicionObjetivo, velocidadPersecucion * Time.deltaTime);

        if (Vector2.Distance(transform.position, posicionObjetivo) < 0.2f)
        {
            StartCoroutine(RutinaAtaqueCaida());
        }
    }

    private IEnumerator RutinaAtaqueCaida()
    {
        estadoActual = EstadoVolador.Cayendo;
        anim.Play("EnemigoVolador_SmashStart");

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        anim.Play("EnemigoVolador_SmashLoop");
        rb.gravityScale = velocidadCaida;
    }

    private void ComportamientoCayendo()
    {
        bool tocandoSuelo = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (tocandoSuelo && estadoActual == EstadoVolador.Cayendo)
        {
            estadoActual = EstadoVolador.EnSuelo; // Cambiamos el estado INMEDIATAMENTE
            StartCoroutine(RutinaRecuperacionSuelo());
        }
    }

    private IEnumerator RutinaRecuperacionSuelo()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        anim.Play("EnemigoVolador_SmashEnd");

        yield return new WaitForSeconds(tiempoRecuperacionSuelo);

        anim.Play("EnemigoVolador_Fly");
        estadoActual = EstadoVolador.Posicionarse;
    }

    private void MirarAlObjetivo()
    {
        float direccionX = 0;

        if (estadoActual == EstadoVolador.Patrullar && destinoActual != null)
            direccionX = destinoActual.position.x - transform.position.x;
        else if (estadoActual == EstadoVolador.Posicionarse)
            direccionX = player.position.x - transform.position.x;

        if (direccionX > 0 && !mirandoDerecha) Flip();
        else if (direccionX < 0 && mirandoDerecha) Flip();
    }

    void Flip()
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
            anim.Play("EnemigoVolador_Hit", -1, 0f);

            if (audioSource != null && sonidoGolpe != null)
            {
                audioSource.PlayOneShot(sonidoGolpe);
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
        estadoActual = EstadoVolador.Aturdido;
        rb.gravityScale = 1f;

        float direccion = transform.position.x < player.transform.position.x ? -1f : 1f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(fuerzaGolpeX * direccion, fuerzaGolpeY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(tiempoAturdido);

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        anim.Play("EnemigoVolador_Fly");
        estadoActual = EstadoVolador.Posicionarse;
    }

    void Die()
    {
        anim.Play("EnemigoVolador_Die");

        if (audioSource != null && sonidoGolpe != null)
        {
            audioSource.PlayOneShot(sonidoGolpe);
        }
        
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;

        HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
        if (hud != null) hud.SumarPuntos(200);

        this.enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Comprobamos si hemos chocado contra el jugador y si el enemigo sigue vivo
        if (collision.gameObject.CompareTag("Player") && !yaEstaMuerto)
        {
            // 2. ¡EL CAMBIO CLAVE! ¿Estaba en estado "Cayendo" en el momento del impacto?
            if (estadoActual == EstadoVolador.Cayendo)
            {
                // Solo aquí hace daño
                collision.gameObject.GetComponent<PlayerController>().TakeDamage(attackDamage);

                // Además, forzamos el estado EnSuelo para que no te siga haciendo daño
                // si te quedas debajo de él
                estadoActual = EstadoVolador.EnSuelo;
                StartCoroutine(RutinaRecuperacionSuelo());
            }
        }
    }
}