using UnityEngine;
using UnityEngine.UI; // Para poder usar Image
using TMPro; // Para poder usar TextMeshPro

public class HUDManager : MonoBehaviour
{
    [Header("Vidas")]
    public Image[] corazones; // Un array (lista) para guardar nuestras 3 imágenes

    [Header("Tiempo")]
    public TextMeshProUGUI textoTiempo;
    private float tiempoJugado = 0f;

    void Update()
    {
        // 1. Cronómetro
        tiempoJugado += Time.deltaTime; // Va sumando los segundos que pasan
        
        // Convertimos los segundos totales a minutos y segundos
        int minutos = Mathf.FloorToInt(tiempoJugado / 60);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60);
        
        // Lo mostramos con formato 00:00
        textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    // 2. Método para apagar los corazones cuando nos pegan
    public void ActualizarVidas(int vidasActuales)
    {
        for (int i = 0; i < corazones.Length; i++)
        {
            // Si el índice del corazón es menor que nuestras vidas, se muestra. Si no, se oculta.
            if (i < vidasActuales)
            {
                corazones[i].enabled = true;
            }
            else
            {
                corazones[i].enabled = false;
            }
        }
    }
}