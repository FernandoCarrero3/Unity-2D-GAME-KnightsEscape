using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MuroSecreto : MonoBehaviour
{
    private Tilemap tilemap;
    private bool descubierto = false;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el jugador entra en la pared y aún no ha sido descubierta
        if (collision.CompareTag("Player") && !descubierto)
        {
            descubierto = true;
            StartCoroutine(DesvanecerMuro());
        }
    }

    IEnumerator DesvanecerMuro()
    {
        float duracion = 0.5f; // Medio segundo en desaparecer
        float tiempo = 0;
        Color colorOriginal = tilemap.color;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            // Interpolar el canal Alpha (transparencia) de 1 a 0
            colorOriginal.a = Mathf.Lerp(1f, 0f, tiempo / duracion);
            tilemap.color = colorOriginal;
            yield return null;
        }
    }
}