using UnityEngine;

public class Coleccionable : MonoBehaviour
{
    [Header("Configuración")]
    public int puntos = 100;
    public GameObject efectoVisualPrefab; // Aquí arrastraremos tu EfectoDiamante_Prefab
    
    // Opcional: Sonido
    public AudioClip sonidoRecoger;
    private bool yaRecogido = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !yaRecogido)
        {
            yaRecogido = true;

            // 1. Sumar los puntos
            HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
            if (hud != null)
            {
                hud.SumarPuntos(puntos);
            }

            // 2. Crear el efecto visual de estrellas
            if (efectoVisualPrefab != null)
            {
                Instantiate(efectoVisualPrefab, transform.position, Quaternion.identity);
            }

            // 3. Reproducir sonido flotante (para que no se corte al destruir el diamante)
            if (sonidoRecoger != null)
            {
                // Crea un reproductor temporal de audio en la posición del diamante
                AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position);
            }

            // 4. ¡Desaparece el diamante!
            Destroy(gameObject);
        }
    }
}