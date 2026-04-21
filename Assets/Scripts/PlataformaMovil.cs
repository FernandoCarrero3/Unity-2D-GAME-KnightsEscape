using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    [Header("Ruta de la plataforma")]
    public Transform puntoA; // Punto inferior
    public Transform puntoB; // Punto superior
    public float velocidad = 2f;

    private Transform destinoActual;

    void Start()
    {
        // Empezamos yendo hacia el Punto B
        destinoActual = puntoB;
    }

    void Update()
    {
        // 1. Movemos la plataforma poco a poco hacia el destino
        transform.position = Vector2.MoveTowards(transform.position, destinoActual.position, velocidad * Time.deltaTime);

        // 2. Comprobamos si hemos llegado al destino (con un pequeño margen de 0.1f para evitar errores)
        if (Vector2.Distance(transform.position, destinoActual.position) < 0.1f)
        {
            // Cambiamos el destino: si estábamos yendo a B, ahora vamos a A, y viceversa
            if (destinoActual == puntoB)
            {
                destinoActual = puntoA;
            }
            else
            {
                destinoActual = puntoB;
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Cuando el jugador pisa la plataforma, lo hacemos hijo de ella
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Cuando el jugador salta y se va, le quitamos el parentesco
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}