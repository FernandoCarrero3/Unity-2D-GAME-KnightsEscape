using UnityEngine;

public class DestruirTrasTiempo : MonoBehaviour
{
    public float tiempoDeVida = 0.5f; // Ajusta esto a lo que dure tu animación

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }
}