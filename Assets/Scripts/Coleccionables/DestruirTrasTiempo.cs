using UnityEngine;

public class DestruirTrasTiempo : MonoBehaviour
{
    public float tiempoDeVida = 0.5f;

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }
}