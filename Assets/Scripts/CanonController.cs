using System.Collections;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;

    [Header("Área de Visión")]
    public float rangoAdelante = 10f; // Distancia máxima hacia adelante
    public float rangoAltura = 3f;    // Cuánto te ve hacia arriba/abajo

    [Header("Tiempos")]
    public float tiempoEntreDisparos = 2f;
    public float retrasoBala = 0.2f;

    private Transform jugador;
    private Animator anim;
    private float proximoDisparo = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
    }

    void Update()
    {
        if (jugador == null) return;

        // 1. Averiguamos la distancia en X y en Y por separado
        float distanciaX = Mathf.Abs(jugador.position.x - transform.position.x);
        float distanciaY = Mathf.Abs(jugador.position.y - transform.position.y);

        // 2. Comprobamos si el jugador está DELANTE del cañón
        // transform.right.x nos dice hacia dónde apunta el cañón (1 derecha, -1 izquierda)
        float direccionAlJugador = jugador.position.x - transform.position.x;
        bool estaDelante = Mathf.Sign(direccionAlJugador) == Mathf.Sign(transform.right.x);

        // 3. Si está delante, dentro de la distancia X, dentro de la altura Y, y el cañón ha recargado...
        if (estaDelante && distanciaX <= rangoAdelante && distanciaY <= rangoAltura && Time.time >= proximoDisparo)
        {
            StartCoroutine(RutinaDisparo());
            proximoDisparo = Time.time + tiempoEntreDisparos;
        }
    }

    private IEnumerator RutinaDisparo()
    {
        if (anim != null) anim.SetTrigger("Shoot");
        yield return new WaitForSeconds(retrasoBala);

        if (balaPrefab != null && puntoDisparo != null)
        {
            Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);
        }
    }

    // Dibujamos la caja de visión en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Calculamos el centro de la caja (la mitad de la distancia hacia adelante)
        Vector3 centroCaja = transform.position + (transform.right * (rangoAdelante / 2f));
        // Tamaño total de la caja
        Vector3 tamañoCaja = new Vector3(rangoAdelante, rangoAltura * 2f, 0f);

        Gizmos.DrawWireCube(centroCaja, tamañoCaja);
    }
}