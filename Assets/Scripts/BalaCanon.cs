using UnityEngine;

public class BalaCanon : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 12f; // Le subimos un poco la velocidad para compensar la gravedad
    public int dano = 25;
    public float tiempoDeVida = 3f;

    [Header("Colisiones")]
    public LayerMask capasObstaculos;

    void Start()
    {
        // 1. Sale disparada en la dirección en la que nace (el cañón decidirá esta dirección)
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * velocidad;

        // 2. Programamos su autodestrucción
        Destroy(gameObject, tiempoDeVida);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().TakeDamage(dano);
            Destroy(gameObject);
        }
        else if ((capasObstaculos.value & (1 << collision.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
        }
    }
}