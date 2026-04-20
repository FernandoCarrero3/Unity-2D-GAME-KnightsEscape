using UnityEngine;

public class BalaCanon : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 7f;
    public int dano = 25;
    public float tiempoDeVida = 3f;

    [Header("Efecto Teledirigido")]
    public float factorCurva = 2.5f;

    [Header("Colisiones")]
    public LayerMask capasObstaculos; // ¡NUEVO! Para elegir qué la destruye

    private Rigidbody2D rb;
    private Transform jugador;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * velocidad;
        Destroy(gameObject, tiempoDeVida);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
    }

    void FixedUpdate()
    {
        if (jugador != null)
        {
            Vector2 direccionIdeal = (jugador.position - transform.position).normalized;
            Vector2 nuevaDireccion = Vector3.Slerp(rb.linearVelocity.normalized, direccionIdeal, factorCurva * Time.fixedDeltaTime);
            rb.linearVelocity = nuevaDireccion * velocidad;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Si golpea al jugador...
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().TakeDamage(dano);
            Destroy(gameObject); // La bala desaparece
        }
        // 2. Si golpea una pared, el suelo, o cualquier capa que marquemos...
        // (Esta línea matemática comprueba si la capa tocada está dentro de nuestra LayerMask)
        else if ((capasObstaculos.value & (1 << collision.gameObject.layer)) > 0)
        {
            Destroy(gameObject); // La bala choca y desaparece
        }
    }
}