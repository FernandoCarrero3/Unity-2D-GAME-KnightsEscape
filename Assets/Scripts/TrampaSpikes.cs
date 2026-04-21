using UnityEngine;

public class TrampaPinchos : MonoBehaviour
{
    [Header("Configuración")]
    public int dano = 999; // Un número altísimo para garantizar la muerte instantánea

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController jugador = collision.GetComponent<PlayerController>();

            if (jugador != null)
            {
                // Le aplicamos un daño masivo para que muera al instante
                jugador.TakeDamage(dano);
            }
        }
    }
}