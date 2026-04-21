using System.Collections;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;

    [Header("Área de Visión")]
    public float rangoAdelante = 10f;
    public float rangoAltura = 3f;

    [Header("El Cerdito Artillero")]
    public GameObject cerdoArtillero;
    public Animator animCerdo;
    public float retrasoCerdo = 0.5f;

    [Header("Tiempos del Cañón")]
    public float tiempoEntreDisparos = 2f;
    public float retrasoBala = 0.2f;

    private Transform jugador;
    private Animator anim;
    private float proximoDisparo = 0f;
    private bool estaDisparando = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
    }

    void Update()
    {
        // 1. SI EL CERDO MUERE O NO HAY JUGADOR, EL CAÑÓN DEJA DE FUNCIONAR
        if (jugador == null || cerdoArtillero == null) return;

        // Si ya estamos en medio de un disparo, esperamos
        if (estaDisparando) return;

        float distanciaX = Mathf.Abs(jugador.position.x - transform.position.x);
        float distanciaY = Mathf.Abs(jugador.position.y - transform.position.y);

        float direccionAlJugador = jugador.position.x - transform.position.x;
        bool estaDelante = Mathf.Sign(direccionAlJugador) == Mathf.Sign(transform.right.x);

        if (estaDelante && distanciaX <= rangoAdelante && distanciaY <= rangoAltura && Time.time >= proximoDisparo)
        {
            StartCoroutine(RutinaDisparo());
            proximoDisparo = Time.time + tiempoEntreDisparos;
        }
    }

    private IEnumerator RutinaDisparo()
    {
        estaDisparando = true;

        // 1. Le decimos al cerdo que encienda la mecha
        if (animCerdo != null) animCerdo.SetTrigger("LightIt");

        // 2. Esperamos a que el cerdo baje la cerilla hasta la mecha
        yield return new WaitForSeconds(retrasoCerdo);

        if (cerdoArtillero == null)
        {
            estaDisparando = false;
            yield break; // Cancela la rutina por completo
        }

        // 4. El cañón hace su animación de fogonazo
        if (anim != null) anim.SetTrigger("Shoot");

        yield return new WaitForSeconds(retrasoBala);

        // 5. Disparamos con caída gravitacional
        if (balaPrefab != null && puntoDisparo != null)
        {
            Vector2 direccion = (jugador.position - puntoDisparo.position).normalized;
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            puntoDisparo.rotation = Quaternion.Euler(0, 0, angulo);

            Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);
        }

        estaDisparando = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 centroCaja = transform.position + (transform.right * (rangoAdelante / 2f));
        Vector3 tamañoCaja = new Vector3(rangoAdelante, rangoAltura * 2f, 0f);
        Gizmos.DrawWireCube(centroCaja, tamañoCaja);
    }
}